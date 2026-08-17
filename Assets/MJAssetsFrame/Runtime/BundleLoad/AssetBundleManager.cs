using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{

    public class BundleItem
    {
        //文件加载路径
        public string path;

        //文件加载路径crc
        public uint crc;

        //assetbundel名字
        public string bundleName;

        //资源名
        public string assetName;

        //assetbundle所处模块
        public BundleModuleEnum bundleModuleType;

        //assetbundel依赖
        public List<string> bundleDependce;

        //assetbundle
        public AssetBundle assetBundle;

        //通过assetbundle加载出来的对象
        public UnityEngine.Object obj;
    }

    public class AssetBundleManager
    {
        private static AssetBundleManager isntance = new AssetBundleManager();
        public static AssetBundleManager Instance => isntance;

        //ab包配置文件加载路径
        private string mBundleConfigPath;
        //ab包配置文件名称
        private string mBundleConfigName;
        //ab包配置文件名称无后缀
        private string mAssetBundleConfigName;
        //所有模块的assetbundle的资源对象字典
        private Dictionary<uint, BundleItem> mAllBundleAssetDic = new Dictionary<uint, BundleItem>();

        /// <summary>
        /// 生成AssetBundleConfig配置文件路径
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <returns></returns>
        public bool GeneratorBundleConfigPath(BundleModuleEnum bundleModule)
        {
            mBundleConfigName = bundleModule.ToString().ToLower() + "bundleconfig" + BundleSettings.Instance.BundlePostfix;
            mAssetBundleConfigName = bundleModule.ToString().ToLower() + "bundleconfig";
            mBundleConfigPath = BundleSettings.Instance.GetHotAssetsPath(bundleModule) + mBundleConfigName;

            //如果配置文件不存在 可能为内嵌文件解压至内嵌解压文件路径中
            if (!File.Exists(mBundleConfigPath))
            {
                mBundleConfigPath = BundleSettings.Instance.GetAssetsDeCompressPath(bundleModule);
                //如果两个地方都不存在 说明用户没有下载成功
                if (!File.Exists(mBundleConfigPath))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 加载assetsBundle配置文件
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public void LoadAssetBundelConfig(BundleModuleEnum bundleModule)
        {
            try
            {
                //获取当前模块配置文件所在路径
                if (GeneratorBundleConfigPath(bundleModule))
                {
                    AssetBundle bundleConfig = AssetBundle.LoadFromFile(mBundleConfigPath);

                    string bundleConfigJson = bundleConfig.LoadAsset<TextAsset>(mAssetBundleConfigName).text;
                    BundleConfig bundleManifeset = JsonConvert.DeserializeObject<BundleConfig>(bundleConfigJson);

                    //将所有assetbundle信息存放在字典中进行管理
                    foreach (var info in bundleManifeset.bundleInfoList)
                    {
                        if (!mAllBundleAssetDic.ContainsKey(info.crc))
                        {
                            BundleItem item = new BundleItem();
                            item.path = info.path;
                            item.crc = info.crc;
                            item.bundleModuleType = bundleModule;
                            item.assetName = info.assetName;
                            item.bundleDependce = info.bundleDependce;
                            item.bundleName = info.bundleName;
                            mAllBundleAssetDic.Add(item.crc, item);
                        }
                        else
                        {
                            Debug.LogError("AssetBundle Already Exists! BundleName:" + info.bundleName);
                        }
                    }
                    //释放这个assetBundle配置
                    bundleConfig.Unload(false);
                }
                else
                {
                    Debug.LogError("AssetBundleConfig Not Find. Load AssetBundel Failed! BundleModule:" + bundleModule);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Load AssetBundleConfig Failed,Exception:" + e.Message);
            }
        }
    }
}
