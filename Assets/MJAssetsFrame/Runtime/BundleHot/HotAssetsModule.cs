using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 热更资源模块
    /// </summary>
    public class HotAssetsModule
    {
        //热更资源存储路径
        public string HotAssetsSavePath
        {
            get
            {
                return Application.persistentDataPath + "/HotAssets/" + CurBundleModuleEnum + "/";
            }
        }

        //需要下载的资源列表
        public List<HotFileInfo> needDownLoadAssetList = new List<HotFileInfo>();
        //所有热更的资源列表
        public List<HotFileInfo> allDownLoadAssetList = new List<HotFileInfo>();

        //服务端资源清单
        private HotAssetsManifest ServerHotAssetsManifest;
        //本地资源清单
        private HotAssetsManifest LocalHotAssetsManifest;

        //服务端资源热更清单存储路径
        private string ServerHotAssetsManifestPath;
        //本地资源热更清单文件存储存路径
        private string LocalHotAssetManisetPath;

        // 当前下载的资源模块
        public BundleModuleEnum CurBundleModuleEnum { get; set; }

        //最大下载的资源大小 M
        public float AssetsMaxSizeM { get; set; }

        //资源已经下载的大小 M
        public float AssetDownLoadSizeM;

        //资源下载器
        private AssetDownLoader AssetDownLoader;

        //AssetBundle配置文件下载完成监听
        public Action<string> OnDownLoadABConfigListener;

        //AssetBundle下载完成监听
        public Action<string> OnDownLoadAssetBundleListener;

        //下载所有资源完成的回调
        public Action<BundleModuleEnum> OnDownLoadAllAssetsFinish;

        //用于开启协程
        public MonoBehaviour mono;

        public HotAssetsModule(BundleModuleEnum bundleModule, MonoBehaviour mono)
        {
            CurBundleModuleEnum = bundleModule;
            this.mono = mono;

        }



        /// <summary>
        /// 开始热更资源
        /// </summary>
        /// <param name="startDownLoadCallback">开始下载回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="isCheckAssetsVersion">是否检测资源版本</param>
        public void StartHotAssets(Action startDownLoadCallback, Action<BundleModuleEnum> hotFinish = null, bool isCheckAssetsVersion = true)
        {
            this.OnDownLoadAllAssetsFinish += hotFinish;
            if (isCheckAssetsVersion)
            {
                //检测资源版本是否需要热更
                CheckAssetsVersion((isHot, size) =>
                {
                    if (isHot)
                    {
                        StartDownLoadHotAssets(startDownLoadCallback);
                    }
                    else
                    {
                        OnDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
                    }
                });
            }
        }

        /// <summary>
        /// 开始下载热更资源
        /// </summary>
        /// <param name="startDownLoadCallBack"></param>
        public void StartDownLoadHotAssets(Action startDownLoadCallBack)
        {
            //优先下载AssetBundel配置文件，下载完成后调用回调
            //热更资源下载完成后同样需要回调，以便动态加载刚下载的资源
            List<HotFileInfo> downLoadList = new List<HotFileInfo>();
            for (int i = 0; i < needDownLoadAssetList.Count; i++)
            {
                HotFileInfo hotFile = needDownLoadAssetList[i];
                //说明是配置文件需要优先下载
                if (hotFile.abName.Contains("config"))
                {
                    downLoadList.Insert(0, hotFile);
                }
                else
                {
                    downLoadList.Add(hotFile);
                }
            }
            //资源下载队列
            Queue<HotFileInfo> downLoadQueue = new Queue<HotFileInfo>();
            //加入队列 
            foreach (var item in downLoadList)
            {
                downLoadQueue.Enqueue(item);
            }

            //通过资源下载器开始下载
            AssetDownLoader = new AssetDownLoader(this, downLoadQueue, ServerHotAssetsManifest.downLoadUrl, HotAssetsSavePath, DownLoadAssetBundleSuccess, DownLoadAssetBundleFailed, DownLoadAssetBundleFinish);

            startDownLoadCallBack?.Invoke();
            //开始下载队列中的资源
            AssetDownLoader.StartThreadDownLoadQueue();
        }

        /// <summary>
        ///  检测资源版本
        /// </summary>
        /// <param name="checkCallBack"></param>
        public void CheckAssetsVersion(Action<bool, float> checkCallBack)
        {
            //检测资源版本
            GeneratorHotAssetsMainfest();
            needDownLoadAssetList.Clear();
            mono.StartCoroutine(DownLoadHotAssetsManifest(() =>
            {
                //1.资源清单下载
                if (CheckModuleAssetsIsHot())
                {
                    HotAssetsPatch serverHotPath = ServerHotAssetsManifest.hotAssetsPatcheList[ServerHotAssetsManifest.hotAssetsPatcheList.Count - 1];
                    bool isNeedHot = ComputeNeedHotAssetsList(serverHotPath);
                    //是否需要热更
                    if (isNeedHot)
                        checkCallBack?.Invoke(true, AssetsMaxSizeM);
                    else
                        checkCallBack?.Invoke(false, 0);
                }
                else
                    checkCallBack?.Invoke(false, 0);

                //2.如果需要热更，开始计算需要下载的文件 开始下载文件
                //3.如果不需要热更说明文件是最新的 直接下载完成

            }));
        }


        /// <summary>
        /// 计算需要进行热更的文件列表
        /// </summary>
        public bool ComputeNeedHotAssetsList(HotAssetsPatch serverAssetsPath)
        {
            if (!Directory.Exists(HotAssetsSavePath))
                Directory.CreateDirectory(HotAssetsSavePath);

            foreach (var item in serverAssetsPath.hotAssetsList)
            {
                //获取本地AssetBundle文件路径
                string localFilePath = HotAssetsSavePath + item.abName;
                allDownLoadAssetList.Add(item);
                //TODO MD5
                //如果本地文件不存在或者本地文件与服务端不一致 需要热更
                if (!File.Exists(localFilePath) || item.md5 != MD5.GetMd5FromFile(localFilePath))
                {
                    needDownLoadAssetList.Add(item);
                    AssetsMaxSizeM += item.size / 1024f;
                }

            }
            return needDownLoadAssetList.Count > 0;
        }



        /// <summary>
        /// 检测模块资源是否需要热更
        /// </summary>
        /// <returns></returns>
        public bool CheckModuleAssetsIsHot()
        {
            //如果服务器资源清单不存在，不需要热更
            if (ServerHotAssetsManifest == null)
                return false;

            //如果本地资源清单不存在 说明我们需要进行热更
            if (!File.Exists(LocalHotAssetManisetPath))
                return true;


            //判断本地资源清单补丁号是否于服务端资源清单补丁版本号是否一致，不一致需要进行热更
            HotAssetsManifest localHotAssetsManigefst = JsonConvert.DeserializeObject<HotAssetsManifest>(File.ReadAllText(LocalHotAssetManisetPath));
            if (localHotAssetsManigefst.hotAssetsPatcheList.Count == 0 && ServerHotAssetsManifest.hotAssetsPatcheList.Count != 0)
                return true;
            //获取本地热更补丁的最后一个补丁
            HotAssetsPatch localHotPatch = localHotAssetsManigefst.hotAssetsPatcheList[localHotAssetsManigefst.hotAssetsPatcheList.Count - 1];
            //获取服务端热更补丁的最后一个补丁
            HotAssetsPatch serverHotPatch = ServerHotAssetsManifest.hotAssetsPatcheList[ServerHotAssetsManifest.hotAssetsPatcheList.Count - 1];

            if (localHotPatch != null && serverHotPatch != null)
            {
                if (localHotPatch.patchVersion != serverHotPatch.patchVersion)
                    return true;
                else
                    return false;
            }
            if (serverHotPatch != null)
                return true;
            else
                return false;


        }
        /// <summary>
        /// 下载资源热更清单
        /// </summary>
        /// <returns></returns>
        public IEnumerator DownLoadHotAssetsManifest(Action downlogFinish)
        {
            string url = BundleSettings.Instance.AssetDownLoadUrl + "/HotAssets/" + CurBundleModuleEnum + "AssetsHotManifest.json";
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.timeout = 30;

            Debug.Log("*** Request HotAssetsMainfest Url:" + url);
            //等待下载完成
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("***  Request AssetsBundle HotAssetsManifest Url Finish Module:" + CurBundleModuleEnum + "txt:" + webRequest.downloadHandler.text);
                    //写入服务端资源热更清单到本地
                    FileHelper.WriteFile(ServerHotAssetsManifestPath, webRequest.downloadHandler.data);
                    ServerHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(webRequest.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Debug.LogError("服务端资源清单下载异常，文件不存在或者配置有问题,更新出错，请检查：" + e.ToString());
                }
            }
            else
            {
                Debug.LogError("DownLoad Error:" + webRequest.error + "\r\nURL:" + url);
            }

            downlogFinish?.Invoke();
        }

        /// <summary>
        /// 生成资源热更清单路径
        /// </summary>
        private void GeneratorHotAssetsMainfest()
        {
            ServerHotAssetsManifestPath = Application.persistentDataPath + "/Server" + CurBundleModuleEnum + "AssetsHotManifest.json";

            LocalHotAssetManisetPath = Application.persistentDataPath + "/Local" + CurBundleModuleEnum + "AssetsHotManifest.json";
        }

        #region 资源下载回调
        public void DownLoadAssetBundleSuccess(HotFileInfo hotFileInfo)
        {
            string abName = hotFileInfo.abName.Replace(BundleSettings.Instance.BundlePostfix, "");
            //判断是不是资源配置文件
            if (hotFileInfo.abName.Contains("bundleconfig"))
            {
                OnDownLoadABConfigListener?.Invoke(abName);
                //TODO 下载成功需要及时加载配置文件
                //
            }
            else
            {
                OnDownLoadAssetBundleListener?.Invoke(abName);

            }
        }
        public void DownLoadAssetBundleFailed(HotFileInfo hotFileInfo)
        {

        }
        public void DownLoadAssetBundleFinish(HotFileInfo hotFileInfo)
        {
            if (File.Exists(LocalHotAssetManisetPath))
                File.Delete(LocalHotAssetManisetPath);

            //将服务端热更清单文件拷贝到本地
            File.Copy(ServerHotAssetsManifestPath, LocalHotAssetManisetPath);
            OnDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
        }
        #endregion

        public void OnMainThreadUpdate()
        {
            AssetDownLoader?.OnMainThreadUpdate();
        }

        /// <summary>
        /// 设置下载线程个数
        /// </summary>
        /// <param name="threadCount"></param>
        public void SetDownLoadThreadCount(int threadCount)
        {
            Debug.Log("多线程负载均衡:" + threadCount + " ModuleType:" + CurBundleModuleEnum);
            if (AssetDownLoader != null)
                AssetDownLoader.MAX_THREAD_COUNT = threadCount;
        }
    }
}
