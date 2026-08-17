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


    /// <summary>
    /// AssetBudle 缓存
    /// </summary>
    public class AssetBundleCache
    {
        public AssetBundle assetBundle;
        //引用计数
        public int refereaceCount;


        public void Release(bool unLoad)
        {
            assetBundle.Unload(unLoad);
            assetBundle = null;
            refereaceCount = 0;
        }


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
        //key crc
        //value BundleItem
        private Dictionary<uint, BundleItem> mAllBundleAssetDic = new Dictionary<uint, BundleItem>();
        //已经加载过的资源对象字典
        //key bundleName 
        //value asserbundleCache
        private Dictionary<string, AssetBundleCache> mAllAlreadAssetBundleDic = new Dictionary<string, AssetBundleCache>();
        //创建一个assetbundleCache类的对象池
        public ClassObjectPool<AssetBundleCache> mAssetBundleCachePool = new ClassObjectPool<AssetBundleCache>(200);

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
        /// <summary>
        /// 通过资源路径中的crc加载该资源所在的AssetBundle
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        public BundleItem LoadAssetBundle(uint crc)
        {
            BundleItem item = null;
            //先在资源字典中查询是否存在该资源
            //如果存在说明该资源已经被打包成assetBundel
            //可以直接加载
            if (mAllBundleAssetDic.TryGetValue(crc, out item))
            {
                //如果assetBundle为空，说明该资源所在的assetbundle没有加载进内存
                if (item.assetBundle == null)
                {
                    //加载该assetbundel
                    item.assetBundle = LoadAssetBundle(item.bundleName, item.bundleModuleType);
                    //需要加载该ab包的依赖项
                    foreach (var bundleName in item.bundleDependce)
                    {
                        if (item.bundleName != bundleName)
                            LoadAssetBundle(bundleName, item.bundleModuleType);
                    }
                    return item;
                }
                else
                    return item;
            }
            else
            {
                //不存在说明没该资源没被打包
                Debug.LogError("Asset not exists AssetBundleConfig !Load Failed:" + crc);
                return null;
            }
        }


        /// <summary>
        /// 通过AssetbundleName加载AssetBundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="bundleModuleEnum"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string bundleName, BundleModuleEnum bundleModuleEnum)
        {
            AssetBundleCache bundle = null;
            mAllAlreadAssetBundleDic.TryGetValue(bundleName, out bundle);
            if (bundle == null || (bundle != null && bundle.assetBundle == null))
            {
                //从类对象池中取出一个AssetBundleCache
                bundle = mAssetBundleCachePool.Spawn();
                string hotFilePath = BundleSettings.Instance.GetHotAssetsPath(bundleModuleEnum) + bundleName;
                HotAssetsModule module = MJAssetsABFrame.GetHotAssetsModule(bundleModuleEnum);
                //是否使用的是热更路径
                bool isHotPath = module == null ? (File.Exists(hotFilePath) ? true : false) : (module.HotAssetCount == 0 ? (File.Exists(hotFilePath) ? true : false) : module.HotAssetsIsExists(bundleName));
                //通过是否是热更路径计算出AssetBundle加载路径
                string bundlePath = isHotPath ? hotFilePath : BundleSettings.Instance.GetAssetsDeCompressPath(bundleModuleEnum) + bundleName;

                //判断assetbundle是否加密
                if (BundleSettings.Instance.bundleEnctypt.isEncrypt)
                {
                    byte[] bytes = AES.AESFileByteDecrypt(bundlePath, BundleSettings.Instance.bundleEnctypt.encryptKey);
                    bundle.assetBundle = AssetBundle.LoadFromMemory(bytes);
                }
                else
                {
                    //通过FromFile加载速度最快
                    bundle.assetBundle = AssetBundle.LoadFromFile(bundlePath);
                }
                if (bundle.assetBundle == null)
                {
                    Debug.LogError("Load AssetBundle Failed bundlePath :" + bundlePath);
                    return null;
                }
                bundle.refereaceCount++;
                mAllAlreadAssetBundleDic.Add(bundleName, bundle);
            }
            else
                bundle.refereaceCount++;
            return bundle.assetBundle;
        }

        /// <summary>
        /// 释放AssetBundle 并且释放AssetBundle所占用资源
        /// </summary>
        /// <param name="assetItem"></param>
        /// <param name="unLoad"></param>
        public void ReleaseAssets(BundleItem assetItem, bool unLoad)
        {
            if (assetItem != null)
            {
                assetItem.obj = null;
                ReleaseAssetBundle(assetItem, unLoad);
                if (assetItem.bundleDependce != null)
                {
                    foreach (var bundleName in assetItem.bundleDependce)
                    {
                        ReleaseAssetBundle(null, unLoad, bundleName);

                    }
                }

            }
            else
            {
                Debug.LogError("AssetItem is null, release Assets failed!");
            }
        }

        /// <summary>
        /// 释放AssetBundle及所占用资源
        /// </summary>
        /// <param name="assetItem"></param>
        /// <param name="unLoad"></param>
        /// <param name="bundleName"></param>
        public void ReleaseAssetBundle(BundleItem assetItem, bool unLoad, string bundleName = "")
        {
            string assetBundleName = "";
            if (assetItem == null)
                assetBundleName = bundleName;
            else
                assetBundleName = assetItem.bundleName;

            //AssetBundle释放
            AssetBundleCache bundleCacheItem = null;
            //如果已经加载过，并且名字不为空 就去释放他
            if (!string.IsNullOrEmpty(assetBundleName) && mAllAlreadAssetBundleDic.TryGetValue(assetBundleName, out bundleCacheItem))
            {
                if (bundleCacheItem.assetBundle != null)
                {
                    bundleCacheItem.refereaceCount--;
                    //小于等于0说明资源没有被引用可以直接释放
                    if (bundleCacheItem.refereaceCount <= 0)
                    {
                        //bundleCacheItem.assetBundle.Unload(unLoad);
                        //从已经加载过的资源对象字典中移除
                        mAllAlreadAssetBundleDic.Remove(assetBundleName);
                        bundleCacheItem.Release(unLoad);
                        //回收对象
                        mAssetBundleCachePool.Recycl(bundleCacheItem);
                        ////从所有模块的assetbundle的资源对象字典中移除
                        //mAllBundleAssetDic.Remove(assetItem.crc);
                    }
                }
                else
                {
                    Debug.LogError("ReleaseAssetBundle Failed, bundleCacheItem.assetBundle is Null");
                }
            }
            else
            {
                Debug.LogError("ReleaseAssetBundle Failed,BundleName is Null or bundleCacheItem is Null");
            }
        }
    }
}
