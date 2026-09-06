using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// AssetPath代码生成器
    /// </summary>
    public static class AssetPathGenerator
    {
        #region Node

        /// <summary>
        /// AssetPath节点
        /// </summary>
        private class AssetPathNode
        {
            /// <summary>
            /// 原始名称
            /// </summary>
            public string OriginalName;

            /// <summary>
            /// 合法的C#名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 是否为文件
            /// </summary>
            public bool IsFile;

            /// <summary>
            /// 资源完整路径
            /// </summary>
            public string AssetPath;

            /// <summary>
            /// 子节点
            /// </summary>
            public readonly Dictionary<string, AssetPathNode> Children = new Dictionary<string, AssetPathNode>();

            public AssetPathNode(
                string originalName,
                string name,
                bool isFile = false)
            {
                OriginalName = originalName;
                Name = name;
                IsFile = isFile;
            }
        }

        #endregion


        #region Generator

        /// <summary>
        /// 生成AssetPath代码
        /// </summary>
        [MenuItem("MJFrame/GeneratorModuleAssetsPath")]
        public static void GeneratorModuleAssetsPath()
        {
            string generatePath =  GeneratorConfig.Instance.BundleModuleAssetsFilePath;

            string rootPath = GeneratorConfig.Instance.BundleModuleAssetsPath;

            if (string.IsNullOrEmpty(rootPath))
            {
                Debug.LogError("AssetPath生成失败：BundleModuleAssetsPath为空。");
                return;
            }

            if (string.IsNullOrEmpty(generatePath))
            {
                Debug.LogError("AssetPath生成失败：BundleModuleAssetsFilePath为空。");
                return;
            }

            try
            {
                //  获取资源
                string[] guids = AssetDatabase.FindAssets("",new[] { rootPath } );

                // 根节点
                AssetPathNode root =new AssetPathNode("Root", "Root");

                // 计算扫描根目录深度
                //
                // 例如：
                // Assets/HotAssets
                //
                // Split之后：
                // Assets
                // HotAssets
                //
                // depth = 2
                int rootDepth =rootPath.Split('/').Length;

                //  构建资源树

                foreach (string guid in guids)
                {
                    string assetPath =AssetDatabase.GUIDToAssetPath(guid);

                    // 跳过文件夹
                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        continue;
                    }

                    AddAssetToTree(
                        root,
                        assetPath,
                        rootDepth
                    );
                }

                // 生成代码

                StringBuilder builder =new StringBuilder();

                WriteHeader(builder);

                // Namespace
                builder.AppendLine("namespace MJ.AssetFrameWork.ABFrame");

                builder.AppendLine("{");

                // AssetPath
                WriteIndent(builder, 1);

                builder.AppendLine( "public static class AssetPath");

                WriteIndent(builder, 1);

                builder.AppendLine("{");

                // 生成所有节点
                foreach (AssetPathNode child in root.Children.Values)
                {
                    GenerateNodeCode(
                        builder,
                        child,
                        2
                    );
                }

                // AssetPath结束
                WriteIndent(builder, 1);
                builder.AppendLine("}");

                // Namespace结束
                builder.AppendLine("}");

                //  写入文件

                string directory = Path.GetDirectoryName(generatePath);

                if (!string.IsNullOrEmpty(directory) &&
                    !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(
                    generatePath,
                    builder.ToString(),
                    Encoding.UTF8
                );

                // 刷新Unity
                AssetDatabase.Refresh();

                Debug.Log(
                    $"AssetPath生成成功：{generatePath}"
                );
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"AssetPath生成失败：\n{e.Message}"
                );
            }
        }

        #endregion


        #region Build Tree

        /// <summary>
        /// 将资源添加到树中
        /// </summary>
        private static void AddAssetToTree(
            AssetPathNode root,
            string assetPath,
            int rootDepth)
        {
            string[] pathParts =
                assetPath.Split('/');

            AssetPathNode current = root;

            // 文件夹
            // 例如：
            //
            // Assets/HotAssets/Game/Player/Prefabs/Cube.prefab
            //
            // rootDepth = 2
            //
            // 从Game开始生成：
            //
            // Game
            // └── Player
            //     └── Prefabs

            for (int i = rootDepth;
                 i < pathParts.Length - 1;
                 i++)
            {
                string originalName =
                    pathParts[i];

                string className =
                    MakeValidIdentifier(originalName);

                // 查找当前目录下是否已经存在
                if (current.Children.TryGetValue(
                        className,
                        out AssetPathNode existingNode))
                {
                    // 如果原始名称不一样
                    // 说明经过非法字符转换后产生了冲突
                    if (existingNode.OriginalName != originalName)
                    {
                        throw new Exception(
                            $"资源目录命名冲突：\n" +
                            $"'{existingNode.OriginalName}'\n" +
                            $"'{originalName}'\n" +
                            $"都会生成C#名称：'{className}'"
                        );
                    }

                    // 已经存在，继续使用
                    current = existingNode;
                }
                else
                {
                    AssetPathNode newNode =
                        new AssetPathNode(
                            originalName,
                            className
                        );

                    current.Children.Add(
                        className,
                        newNode
                    );

                    current = newNode;
                }
            }

            // 文件

            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            string extension =  Path.GetExtension(assetPath);

            // 去掉 .
            extension =extension.TrimStart('.');

            string variableName;

            if (!string.IsNullOrEmpty(extension))
            {
                // Cube.prefab
                // ↓
                // Cube_prefab

                variableName =
                    $"{fileName}_{extension}";
            }
            else
            {
                variableName =
                    fileName;
            }

            // 转换成合法C#名称
            variableName =MakeValidIdentifier(variableName);

            // 文件名称冲突检查

            if (current.Children.TryGetValue(
                    variableName,
                    out AssetPathNode existingFile))
            {
                // 如果不是同一个资源路径
                if (existingFile.AssetPath != assetPath)
                {
                    throw new Exception(
                        $"资源名称冲突：\n" +
                        $"'{existingFile.AssetPath}'\n" +
                        $"'{assetPath}'\n" +
                        $"都会生成C#名称：'{variableName}'"
                    );
                }

                // 已经存在
                return;
            }

            //==================================================
            // 创建文件节点
            //==================================================

            AssetPathNode fileNode =
                new AssetPathNode(
                    pathParts[pathParts.Length - 1],
                    variableName,
                    true
                );

            fileNode.AssetPath =
                assetPath;

            current.Children.Add(
                variableName,
                fileNode
            );
        }

        #endregion


        #region Generate Code

        /// <summary>
        /// 递归生成代码
        /// </summary>
        private static void GenerateNodeCode(
            StringBuilder builder,
            AssetPathNode node,
            int indent)
        {
            //==================================================
            // 文件
            //==================================================

            if (node.IsFile)
            {
                WriteIndent(
                    builder,
                    indent
                );

                builder.AppendLine(
                    $"public const string {node.Name} = \"{node.AssetPath}\";"
                );

                return;
            }

            //==================================================
            // 文件夹
            //==================================================

            WriteIndent(
                builder,
                indent
            );

            builder.AppendLine(
                $"public static class {node.Name}"
            );

            WriteIndent(
                builder,
                indent
            );

            builder.AppendLine("{");

            // 递归生成子节点
            foreach (AssetPathNode child in node.Children.Values)
            {
                GenerateNodeCode(
                    builder,
                    child,
                    indent + 1
                );
            }

            WriteIndent(
                builder,
                indent
            );

            builder.AppendLine("}");
        }

        #endregion


        #region Identifier

        /// <summary>
        /// 将名称转换成合法的C#标识符
        /// </summary>
        private static string MakeValidIdentifier( string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "_";
            }

            StringBuilder builder =
                new StringBuilder();

            // 第一个字符

            char first = name[0];

            if (IsValidIdentifierStart(first))
            {
                builder.Append(first);
            }
            else
            {
                // 数字、特殊字符等
                // 统一使用 _
                builder.Append('_');

                if (IsValidIdentifierPart(first))
                {
                    builder.Append(first);
                }
            }

            // 后续字符

            for (int i = 1;
                 i < name.Length;
                 i++)
            {
                char c = name[i];

                if (IsValidIdentifierPart(c))
                {
                    builder.Append(c);
                }
                else
                {
                    // 非法字符
                    // - → _
                    // 空格 → _
                    // . → _
                    // # → _
                    builder.Append('_');
                }
            }

            string result =
                builder.ToString();

            // C#关键字

            if (IsCSharpKeyword(result))
            {
                result = "_" + result;
            }

            return result;
        }


        /// <summary>
        /// 判断是否允许作为C#标识符的第一个字符
        /// </summary>
        private static bool IsValidIdentifierStart(
            char c)
        {
            return
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_' ||
                char.IsLetter(c);
        }


        /// <summary>
        /// 判断是否允许作为C#标识符的后续字符
        /// </summary>
        private static bool IsValidIdentifierPart(
            char c)
        {
            return
                IsValidIdentifierStart(c) ||
                (c >= '0' && c <= '9');
        }


        /// <summary>
        /// 判断是否为C#关键字
        /// </summary>
        private static bool IsCSharpKeyword(
            string value)
        {
            switch (value)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                case "while":
                    return true;

                default:
                    return false;
            }
        }

        #endregion


        #region Utility

        /// <summary>
        /// 写入缩进
        /// </summary>
        private static void WriteIndent(
            StringBuilder builder,
            int indent)
        {
            builder.Append(
                new string('\t', indent)
            );
        }


        /// <summary>
        /// 写入文件头
        /// </summary>
        private static void WriteHeader(StringBuilder builder)
        {
            builder.AppendLine( "/* ----------------------------------------------");

            builder.AppendLine(" * Title: AssetBundle路径类");

            builder.AppendLine( $" * Date: {DateTime.Now}");

            builder.AppendLine( " * Description: Represents the path for each resource object to be loaded.");

            builder.AppendLine( " * ----------------------------------------------");
            builder.AppendLine( " */");
            builder.AppendLine();
        }

        #endregion
    }
}