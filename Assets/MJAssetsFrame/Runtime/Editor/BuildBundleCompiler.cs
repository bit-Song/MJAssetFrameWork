using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public enum E_BuildType
    {
        //AB包
        AssetBundle,
        //热更补丁
        HotPatch
    }

    public class BuildBundleCompiler
    {
        // 更新公告
        private static string updateNotice;
        // 热更补丁版本
        private static int hotPatchVersion;
        // 打包类型
        private static E_BuildType buildType;
        //打包模块数据
        private static BundleModuleData buildModuleData;
        // 打包模块类型
        private static BundleModuleEnum bundleModuleEnum;

        //所有AssetBundle文件路径列表
        private static List<string> allBundlePatchList = new List<string>();
        //所有文件夹的Bundle字典
        private static Dictionary<string, List<string>> allFolderBundleDic = new Dictionary<string, List<string>>();
        //所有预制体的Bundle字典
        private static Dictionary<string, List<string>> allPrefabsBundleDic = new Dictionary<string, List<string>>();

        //public const string BundlePostfix = ".unity";


        /// <summary>
        /// 框架内部Resources路径
        /// </summary>
        private static string MyResourcesPath
        {
            get
            {
                return Application.dataPath + "/MJAssetsFrame/Resources/";
            }
        }

        /// <summary>
        /// AssetBundle文件输出路径
        /// </summary>
        private static string BundleOutPutPath
        {
            get
            {
                return Application.dataPath + "/../AssetBundle/" + bundleModuleEnum + "/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            }
        }

        /// <summary>
        /// 热更资源文件输出路径
        /// </summary>
        private static string HotAssetsOutPutPath
        {
            get
            {
                return Application.dataPath + "/../HotAssets/" + bundleModuleEnum + "/" + hotPatchVersion + "/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            }
        }

        private static string HotAssetManifestPath
        {
            get
            {
                return Application.dataPath + "/../HotAssets/" + bundleModuleEnum + "AssetsHotManifest.json";
            }
        }

        /// <summary>
        /// 打包AssetBundel
        /// </summary>
        /// <param name="bundleModuleData">资源模块配置数据</param>
        /// <param name="buildType">打包类型</param>
        /// <param name="hotPatchVersion">热更补丁版本</param>
        /// <param name="updateNotice">更新公告</param>
        public static void BuildAssetBundle(BundleModuleData bundleModuleData, E_BuildType buildType = E_BuildType.AssetBundle, int hotPatchVersion = 0, string updateNotice = "")
        {
            //初始化打包数据
            Initlization(bundleModuleData, buildType, hotPatchVersion, updateNotice);

            //打包所有文件夹
            BuileAllForlder();
            // 打包父文件夹下的所有子文件夹
            BuildRootSubForlder();
            //打包指定文件夹下所有的预制体
            BuildAllPrefabs();

            //开始打包AssetBundle
            BuildAllAssetBundle();
        }

        /// <summary>
        /// 初始化信息
        /// </summary>
        /// <param name="bundleModuleData"></param>
        /// <param name="buildType"></param>
        /// <param name="hotPatchVersion"></param>
        /// <param name="updateNotice"></param>
        public static void Initlization(BundleModuleData bundleModuleData, E_BuildType buildType = E_BuildType.AssetBundle, int hotPatchVersion = 0, string updateNotice = "")
        {
            //清理数据 防止数据残留
            allBundlePatchList.Clear();
            allFolderBundleDic.Clear();
            allPrefabsBundleDic.Clear();

            buildModuleData = bundleModuleData;
            BuildBundleCompiler.buildType = buildType;
            BuildBundleCompiler.hotPatchVersion = hotPatchVersion;
            BuildBundleCompiler.updateNotice = updateNotice;
            bundleModuleEnum = (BundleModuleEnum)Enum.Parse(typeof(BundleModuleEnum), bundleModuleData.moduleName);
            //先清空打包路径的文件夹
            FileHelper.DeleteFolder(BundleOutPutPath);
            //重新创建
            Directory.CreateDirectory(BundleOutPutPath);
        }
        /// <summary>
        /// 打包所有文件夹
        /// </summary>
        public static void BuileAllForlder()
        {
            if (buildModuleData.signFolderPathArr == null || buildModuleData.signFolderPathArr.Count == 0)
                return;

            for (int i = 0; i < buildModuleData.signFolderPathArr.Count; i++)
            {
                //获取文件夹路径
                string path = buildModuleData.signFolderPathArr[i].bundlePath.Replace(@"\", "/");
                //路径查重
                if (!IsRepeatBundleFile(path))
                {
                    //将路径添加进所有路径中
                    allBundlePatchList.Add(path);
                    //获取以模块名+_+AbName的格式的AssetBundle包名
                    string bundleName = GenerateBundleName(buildModuleData.signFolderPathArr[i].abName);
                    //添加到文件夹包中
                    if (!allFolderBundleDic.ContainsKey(bundleName))
                    {
                        allFolderBundleDic.Add(bundleName, new List<string> { path });
                    }
                    else
                    {
                        allFolderBundleDic[bundleName].Add(path);
                    }
                }
                else
                    Debug.LogError(" RepeatBundleFile ：" + path);
            }
        }

        /// <summary>
        /// 打包父文件夹下的所有子文件夹
        /// </summary>
        public static void BuildRootSubForlder()
        {

            //检测父文件夹是否有配置，如果没配置就直接跳过
            if (buildModuleData.rootFolderPathArr == null || buildModuleData.rootFolderPathArr.Count == 0)
            {
                return;
            }

            for (int i = 0; i < buildModuleData.rootFolderPathArr.Count; i++)
            {
                string path = buildModuleData.rootFolderPathArr[i] + "/";
                //获取符文夹的所有的子文件夹
                string[] folderArr = Directory.GetDirectories(path);
                foreach (var item in folderArr)
                {
                    path = item.Replace(@"\", "/");
                    int nameIndex = path.LastIndexOf("/") + 1;
                    //获取文件夹同名的AssetBundle名称
                    string bundleName = GenerateBundleName(path.Substring(nameIndex, path.Length - nameIndex));
                    if (!IsRepeatBundleFile(path))
                    {
                        allBundlePatchList.Add(path);
                        if (!allFolderBundleDic.ContainsKey(bundleName))
                        {
                            allFolderBundleDic.Add(bundleName, new List<string> { path });
                        }
                        else
                        {
                            allFolderBundleDic[bundleName].Add(path);
                        }
                    }
                    else
                    {
                        Debug.LogError("RepeatBundle file FolderPath:" + path);
                    }
                    //处理子文件夹资源的代码
                    string[] filePathArr = Directory.GetFiles(path, "*");
                    foreach (var filePath in filePathArr)
                    {
                        //过滤.meta文件
                        if (!filePath.EndsWith(".meta"))
                        {
                            string abFilePath = filePath.Replace(@"\", "/");
                            if (!IsRepeatBundleFile(abFilePath))
                            {
                                allBundlePatchList.Add(abFilePath);
                                if (!allFolderBundleDic.ContainsKey(bundleName))
                                {
                                    allFolderBundleDic.Add(bundleName, new List<string> { abFilePath });
                                }
                                else
                                {
                                    allFolderBundleDic[bundleName].Add(abFilePath);
                                }
                            }
                        }
                    }
                }
            }


        }

        /// <summary>
        /// 打包指定文件夹下所有的预制体
        /// </summary>
        public static void BuildAllPrefabs()
        {
            if (buildModuleData.prefabPathArr == null || buildModuleData.prefabPathArr.Count == 0)
            {
                return;
            }
            //获取所有预制体的GUID
            string[] guidArr = AssetDatabase.FindAssets("t:Prefab", buildModuleData.prefabPathArr.ToArray());

            for (int i = 0; i < guidArr.Length; i++)
            {
                //将GUID转换成文件路径
                string filePath = AssetDatabase.GUIDToAssetPath(guidArr[i]);
                //计算AssetBundle名称
                string bundleName = GenerateBundleName(Path.GetFileNameWithoutExtension(filePath));
                if (!allBundlePatchList.Contains(filePath))
                {
                    //获取预制体所有的依赖项
                    string[] dependsArr = AssetDatabase.GetDependencies(filePath);
                    List<string> dependsList = new List<string>();
                    for (int k = 0; k < dependsArr.Length; k++)
                    {
                        string path = dependsArr[k];
                        //如果不是冗余文件，就归纳进打包
                        if (!IsRepeatBundleFile(path))
                        {
                            allBundlePatchList.Add(path);
                            dependsList.Add(path);
                        }
                    }
                    if (!allPrefabsBundleDic.ContainsKey(bundleName))
                    {
                        allPrefabsBundleDic.Add(bundleName, dependsList);
                    }
                    else
                    {
                        Debug.LogError("重复预制体名字，当前模块下有预制体文件重复 Name:" + bundleName);
                    }
                }
            }
        }

        /// <summary>
        /// 打包所有的ab包
        /// </summary>
        public static void BuildAllAssetBundle()
        {
            //修改所有要打包的文件的AssetBundleName
            ModifyAllFileBundleName();
            //生成一份AssetBundle配置
            WriteAssetBundleConfig();

            AssetDatabase.Refresh();

            //调用UnityAPI打包AssetBundle
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(BundleOutPutPath,
                (UnityEditor.BuildAssetBundleOptions)Enum.Parse(typeof(UnityEditor.BuildAssetBundleOptions), BundleSettings.Instance.buildAssetBundleOptions.ToString()),
                (UnityEditor.BuildTarget)Enum.Parse(typeof(UnityEditor.BuildTarget), BundleSettings.Instance.buildTarget.ToString()));

            if (manifest == null)
            {
                EditorUtility.DisplayProgressBar("BuildAssetBundle!", "BuildAssetBundle failed!", 1);
                Debug.LogError("AssetBundle Build failed!");
            }
            else
            {
                Debug.Log("AssetBundle Build Successs!:" + manifest + "\r\nPath:" + BundleOutPutPath);
                //删除所有AB包自动生成的manifest文件
                DeleteAllBundleManifestFile();
                //加密AB包
                EncryptAllBundle();
                if (buildType == E_BuildType.HotPatch)
                {
                    GeneratorHotAssets();
                }
            }
            ModifyAllFileBundleName(true);

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 修改或清空AssetBundle
        /// </summary>
        /// <param name="clear"></param>
        public static void ModifyAllFileBundleName(bool clear = false)
        {
            int i = 0;
            //修改所有文件夹下AssetBundle name
            foreach (var item in allFolderBundleDic)
            {
                i++;
                //修改所有文件夹下所有AssetBundle name进度
                EditorUtility.DisplayProgressBar("Modify AssetBundle Name", "Name:" + item.Key, i * 1.0f / allFolderBundleDic.Count);
                foreach (var path in item.Value)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(path);
                    if (importer != null)
                    {
                        importer.assetBundleName = (clear ? "" : item.Key + BundleSettings.Instance.BundlePostfix);
                    }
                }
            }
            i = 0;
            foreach (var item in allPrefabsBundleDic)
            {
                i++;
                List<string> bundleList = item.Value;
                foreach (var path in bundleList)
                {
                    //修改所有预制体的AssetBundleName
                    EditorUtility.DisplayProgressBar("Modify AssetBundle Name", "Name:" + item.Key, i * 1.0f / allPrefabsBundleDic.Count);
                    AssetImporter importer = AssetImporter.GetAtPath(path);
                    if (importer != null)
                    {
                        importer.assetBundleName = (clear ? "" : item.Key + BundleSettings.Instance.BundlePostfix);
                    }
                }

            }
            if (clear)
            {
                string bundleConfigPath = Application.dataPath + "/" + bundleModuleEnum.ToString().ToLower() + "assetbundleconfig.json";
                AssetImporter importer = AssetImporter.GetAtPath(bundleConfigPath.Replace(Application.dataPath, "Assets"));
                if (importer != null)
                {
                    importer.assetBundleName = "";
                }
                //移除未使用的AssetBundleName
                AssetDatabase.RemoveUnusedAssetBundleNames();
            }
        }

        /// <summary>
        /// 生成AssetBundle配置文件
        /// </summary>
        public static void WriteAssetBundleConfig()
        {
            BundleConfig config = new BundleConfig();
            //初始化依赖性容器
            config.bundleInfoList = new List<BundleInfo>();
            //所有AssetBundle文件字典 key =路径 value =AssetBundleName
            Dictionary<string, string> allBundleFilePathDic = new Dictionary<string, string>();
            //获取到工程内所有的AssetBundleName
            string[] allBundleArr = AssetDatabase.GetAllAssetBundleNames();

            foreach (var bundleName in allBundleArr)
            {
                //获取指定AssetBundleName 下的所有的文件路径
                string[] bundleFileArr = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

                foreach (var filePath in bundleFileArr)
                {
                    if (!filePath.EndsWith(".cs"))
                    {
                        allBundleFilePathDic.Add(filePath, bundleName);
                    }
                }
            }
            //计算AssetBundle数据，生成AsestBundle配置文件。
            foreach (var item in allBundleFilePathDic)
            {
                //获取文件路径
                string filePath = item.Key;
                if (!filePath.EndsWith(".cs"))
                {

                    BundleInfo info = new BundleInfo();
                    info.path = filePath;
                    info.bundleName = item.Value;
                    info.assetName = Path.GetFileName(filePath);
                    info.crc = Crc32.GetCrc32(filePath);
                    info.bundleDependce = new List<string>();

                    //得到这个文件的依赖项
                    string[] depence = AssetDatabase.GetDependencies(filePath);
                    foreach (var dePath in depence)
                    {
                        //如果依赖项不是当前的这个文件，以及依赖项不是cs脚本 就进行处理
                        //防止重复添加自己
                        if (!dePath.Equals(filePath) && dePath.EndsWith(".cs") == false)
                        {
                            string assetBundleName = "";
                            if (allBundleFilePathDic.TryGetValue(dePath, out assetBundleName))
                            {
                                //如果依赖项已经包含这个AssetBundle就不进行处理，否则添加进依赖项
                                if (!info.bundleDependce.Contains(assetBundleName))
                                {
                                    info.bundleDependce.Add(assetBundleName);
                                }
                            }
                        }
                    }

                    config.bundleInfoList.Add(info);
                }
            }
            //生成AsestBundle配置文件。序列化
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            //生成位置
            //string bundleConfigPath = Application.dataPath + "/" + bundleModuleEnum.ToString().ToLower() + "assetbundleconfig.json";
            string bundleConfigPath = Application.dataPath + "/" + bundleModuleEnum.ToString().ToLower() + "bundleconfig.json";
            StreamWriter writer = File.CreateText(bundleConfigPath);
            writer.Write(json);
            writer.Dispose();
            writer.Close();

            AssetDatabase.Refresh();
            //修改AssetBundle配置文件的AssetBundleName
            AssetImporter importer = AssetImporter.GetAtPath(bundleConfigPath.Replace(Application.dataPath, "Assets"));
            if (importer != null)
            {
                importer.assetBundleName = bundleModuleEnum.ToString().ToLower() + "bundleconfig" + BundleSettings.Instance.BundlePostfix;
            }
        }



        /// <summary>
        /// 是否是重复的Bundle文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsRepeatBundleFile(string path)
        {
            if (path.EndsWith(".cs"))
                return true;

            foreach (var item in allBundlePatchList)
            {
                if (string.Equals(item, path) || item.Contains(path) || path.EndsWith(".cs"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ab包名拼接
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        public static string GenerateBundleName(string abName)
        {
            return bundleModuleEnum.ToString() + "_" + abName;
        }


        /// <summary>
        /// 删除AB包自动生成的ManifestFile文件
        /// </summary>
        private static void DeleteAllBundleManifestFile()
        {
            string[] pathArr = Directory.GetFiles(BundleOutPutPath);

            foreach (var path in pathArr)
            {
                if (path.EndsWith(".manifest"))
                    File.Delete(path);
            }
        }


        /// <summary>
        /// 加密所有AssetBundle
        /// </summary>
        public static void EncryptAllBundle()
        {
            //如果不需要加密就直接返回
            if (!BundleSettings.Instance.bundleEnctypt.isEncrypt)
                return;

            DirectoryInfo directoryInfo = new DirectoryInfo(BundleOutPutPath);
            FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < fileInfoArr.Length; i++)
            {
                //加密进度
                EditorUtility.DisplayProgressBar("加密文件", "Name:" + fileInfoArr[i].Name, i * 1.0f / fileInfoArr.Length);
                //TODO文件加密  后续修改成C#原生加密方式
                AES.AESFileEncrypt(fileInfoArr[i].FullName, "MJ");
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("AssetBundle Encrypt Finish");
        }


        /// <summary>
        /// 内嵌资源到StreamingAssets文件中
        /// </summary>
        /// <param name="modeData"></param>
        /// <param name="showTips"></param>
        public static void CopyBundleToStramingAssets(BundleModuleData modeData, bool showTips = true)
        {
            bundleModuleEnum = (BundleModuleEnum)Enum.Parse(typeof(BundleModuleEnum), modeData.moduleName);
            //获取目标文件夹下所有Ab文件
            DirectoryInfo directoryInfo = new DirectoryInfo(BundleOutPutPath);
            FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            //Bundle内嵌的目标文件夹
            string streamingAssetsPath = Application.streamingAssetsPath + "/AssetBundle/" + bundleModuleEnum + "/";
            //防止文件残留
            FileHelper.DeleteFolder(streamingAssetsPath);
            Directory.CreateDirectory(streamingAssetsPath);

            List<BuiltinBundleInfo> bundleInfoList = new List<BuiltinBundleInfo>();


            for (int i = 0; i < fileInfoArr.Length; i++)
            {
                //拷贝单个包进度
                EditorUtility.DisplayProgressBar("内嵌资源中", "Name:" + fileInfoArr[i].Name, i * 1.0f / fileInfoArr.Length);
                File.Copy(fileInfoArr[i].FullName, streamingAssetsPath + fileInfoArr[i].Name);
                //生成内嵌资源文件信息
                BuiltinBundleInfo info = new BuiltinBundleInfo();
                info.fileName = fileInfoArr[i].Name;
                //TODOMD5  后面改成原生的MD5解析方式
                info.md5 = MJ.AssetFrameWork.ABFrame.MD5.GetMd5FromFile(fileInfoArr[i].FullName);
                info.size = fileInfoArr[i].Length / 1024;
                bundleInfoList.Add(info);
            }
            //将配置文件序列化
            string json = JsonConvert.SerializeObject(bundleInfoList, Formatting.Indented);

            if (!Directory.Exists(MyResourcesPath))
                Directory.CreateDirectory(MyResourcesPath);
            //写入配置文件到Resources文件夹
            FileHelper.WriteFile(MyResourcesPath + bundleModuleEnum + "info.json", Encoding.UTF8.GetBytes(json));
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

            if (showTips)
            {
                EditorUtility.DisplayDialog("内嵌资源", "内嵌资源完成 Path:" + streamingAssetsPath, "确认");
            }
            Debug.Log("Assets Copy toStreamingAssets Finish");
        }

        /// <summary>
        /// 生成热更资源
        /// </summary>
        public static void GeneratorHotAssets()
        {
            //防止文件残留
            FileHelper.DeleteFolder(HotAssetsOutPutPath);
            Directory.CreateDirectory(HotAssetsOutPutPath);
            //得到所有的ab包文件路径
            string[] bundlePatchArr = Directory.GetFiles(BundleOutPutPath, "*" + BundleSettings.Instance.BundlePostfix);
            for (int i = 0; i < bundlePatchArr.Length; i++)
            {
                EditorUtility.DisplayProgressBar("生成热更文件", "Name:" + Path.GetFileName(bundlePatchArr[i]), i * 0.1f / bundlePatchArr.Length);
                string path = bundlePatchArr[i];
                //热更资源目标路径
                string disPath = HotAssetsOutPutPath + Path.GetFileName(path);
                File.Copy(path, disPath);
            }
            Debug.Log("热更文件生成成功");
            GeneralHotAssetsManifest();
        }


        /// <summary>
        /// 生成热更资源配置清单
        /// </summary>
        public static void GeneralHotAssetsManifest()
        {
            HotAssetsManifest assetsManifest = new HotAssetsManifest();
            assetsManifest.updateNotice = updateNotice;
            assetsManifest.downLoadUrl = BundleSettings.Instance.AssetDownLoadUrl + bundleModuleEnum + "/" +
                hotPatchVersion + "/" + BundleSettings.Instance.buildTarget;
            //设置补丁信息
            HotAssetsPatch hotAssetsPatch = new HotAssetsPatch();
            hotAssetsPatch.patchVersion = hotPatchVersion;

            //计算热更补丁文件信息
            DirectoryInfo directoryInfo = new DirectoryInfo(HotAssetsOutPutPath);
            //得到文件信息 
            FileInfo[] bundleInfos = directoryInfo.GetFiles("*" + BundleSettings.Instance.BundlePostfix);

            foreach (FileInfo bundleInfo in bundleInfos)
            {
                HotFileInfo info = new HotFileInfo();
                info.abName = bundleInfo.Name;
                //TODO MD5 修改
                info.md5 = MD5.GetMd5FromFile(bundleInfo.FullName);
                info.size = bundleInfo.Length / 1024.0f;
                hotAssetsPatch.hotAssetsList.Add(info);
            }
            assetsManifest.hotAssetsPatcheList.Add(hotAssetsPatch);
            //把对象转换成json
            string json = JsonConvert.SerializeObject(assetsManifest, Formatting.Indented);
            //写入到本地文件
            FileHelper.WriteFile(HotAssetManifestPath, Encoding.UTF8.GetBytes(json));

        }
    }
}