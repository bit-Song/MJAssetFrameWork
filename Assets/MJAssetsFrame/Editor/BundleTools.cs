using MJ.AssetFrameWork.ABFrame;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class BundleTools : MonoBehaviour
    {
        //枚举生成位置
        private static string mBundleModuleEnumFilePath = Application.dataPath + "/MJAssetsFrame/Config/BundleModuleEnum.cs";
        //路径生成位置
        private static string mBundleModuleAssetsFilePath = Application.dataPath + "/MJAssetsFrame/Config/AssetPath.cs";
        //资源存放路径
        private static string mBundleModuleAssetsPath = "Assets/BundleAssetsDemo";
        [MenuItem("MJFrame/GeneratorModuleEnum")]
        public static void GenerateBundleModuleEnum()
        {
            string namespaceName = "MJ.AssetFrameWork.ABFrame";
            string classname = "BundleModuleEnum";

            if (File.Exists(mBundleModuleEnumFilePath))
            {
                File.Delete(mBundleModuleEnumFilePath);
                AssetDatabase.Refresh();
            }

            var writer = File.CreateText(mBundleModuleEnumFilePath);
            writer.WriteLine("/* ----------------------------------------------");
            writer.WriteLine("/* Title:AssetBundle模块类");
            writer.WriteLine("/* Data:" + System.DateTime.Now);
            writer.WriteLine("/* Description:  Represents each module which is used to download an load.");
            writer.WriteLine("/* Modify:");
            writer.WriteLine("----------------------------------------------*/");

            writer.WriteLine($"namespace {namespaceName}");
            writer.WriteLine("{");
            List<BundleModuleData> moduleList = BuildBundleConfigura.Instance.AssetBundleConfig;

            if (moduleList == null || moduleList.Count <= 0)
            {
                return;
            }
            writer.WriteLine("\t" + $"public enum {classname}");
            writer.WriteLine("\t" + "{");
            writer.WriteLine("\t\tNone,");
            for (int i = 0; i < moduleList.Count; i++)
            {
                writer.WriteLine("\t\t" + moduleList[i].moduleName + ",");
            }

            writer.WriteLine("\t" + "}");

            writer.WriteLine("}");

            writer.Close();

            AssetDatabase.Refresh();

        }





        [MenuItem("MJFrame/GeneratorModuleAssetsPath")]
        public static void GeneratorModuleAssetsPath()
        {
            if (File.Exists(mBundleModuleAssetsFilePath))
            {
                File.Delete(mBundleModuleAssetsFilePath);
                AssetDatabase.Refresh();
            }
            string namespaceName = "MJ.AssetFrameWork.ABFrame";
            string classname = "AssetPath";
            var writer = File.CreateText(mBundleModuleAssetsFilePath);
            writer.WriteLine("/* ----------------------------------------------");
            writer.WriteLine("/* Title:AssetBundle路径类");
            writer.WriteLine("/* Data:" + System.DateTime.Now);
            writer.WriteLine("/* Description:  Represents the path for each resource object to be loaded.");
            writer.WriteLine("/* Modify:");
            writer.WriteLine("----------------------------------------------*/");
            writer.WriteLine($"namespace {namespaceName}");
            writer.WriteLine("{");
            writer.WriteLine("\t" + $"public static class {classname}");
            writer.WriteLine("\t" + "{");
            string[] guids = AssetDatabase.FindAssets("", new[] { mBundleModuleAssetsPath });
            string[] baseFolderNames = mBundleModuleAssetsPath.Split("/");

            //for (int i = 0; i < baseFolderNames.Length; i++)
            //{
            //    writer.WriteLine("\t" + $"public static class {baseFolderNames[i]}");
            //    writer.WriteLine("\t" + "{");
            //}


            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;

                string[] folderNames = assetPath.Split("/");

                for (int i = 2; i < folderNames.Length; i++)
                {
                    writer.WriteLine("\t" + $"public static class {folderNames[i]}");
                    writer.WriteLine("\t" + "{");
                    //如果是倒数第二个文件夹，则生成常量字符串
                    if (i == folderNames.Length - 2)
                    {
                        writer.WriteLine("\t" + $"public const string {Path.GetFileNameWithoutExtension(assetPath)} =  \"{assetPath}\";");
                        break;
                    }
                }
                for (int i = 0; i < folderNames.Length - (baseFolderNames.Length + 1); i++)
                {
                    writer.WriteLine("\t" + "}");
                }

                Debug.Log(assetPath);
            }
            writer.WriteLine("\t" + "}");
            writer.WriteLine("\t" + "}");
            writer.Close();
            AssetDatabase.Refresh();
        }
    }

}


