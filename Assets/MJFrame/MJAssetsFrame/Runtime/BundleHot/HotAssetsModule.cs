using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 下载状态
    /// </summary>
    public enum E_DownloadState
    {
        None,        // 未开始
        Waiting,     // 等待下载（在队列中排队）
        Downloading, // 正在下载
        Finished     // 下载完成
    }

    public struct CheckVersionResult
    {
        //是否需要热更
        public bool isHot;
        //资源大小
        public float sizeM;
    }
    public class HotAssetsModule
    {
        //manifest尝试重新下载次数
        public int MAX_MANIFEST_DOWN_LOAD_COUNT = 3;

        /// <summary>
        /// 当前下载的资源模块类型
        /// </summary>
        public BundleModuleEnum CurBundleModuleEnum { get; set; }

        private E_DownloadState mCurDownloadStateEnum = E_DownloadState.None;
        /// <summary>
        /// 当前下载状态
        /// </summary>
        public E_DownloadState CurDownloadStateEnum
        {
            get
            {
                return mCurDownloadStateEnum;
            }

            set
            {
                if (mCurDownloadStateEnum == value) return;
                mCurDownloadStateEnum = value;
                OnDownLoadStateChanged?.Invoke(mCurDownloadStateEnum);
            }
        }
        /// <summary>
        /// 当下载状态改变时触发的回调
        /// </summary>
        private event Action<E_DownloadState> OnDownLoadStateChanged;

        //需要下载的资源列表
        public List<HotFileInfo> mNeedDownLoadAssetList = new List<HotFileInfo>();
        //所有热更的资源列表
        public List<HotFileInfo> mAllHotAssetList = new List<HotFileInfo>();

        //服务端资源清单
        private HotAssetsManifest mServerHotAssetsManifest;
        //本地资源清单
        private HotAssetsManifest mLocalHotAssetsManifest;

        /// <summary>
        /// 所有热更资源的长度
        /// </summary>
        public int HotAssetCount => mAllHotAssetList.Count;

        /// <summary>
        /// 热更公告
        /// </summary>
        public string UpdateNoticeContent => mServerHotAssetsManifest.updateNotice;


        //服务端资源热更清单存储路径
        private string mServerHotAssetsManifestPath
        {
            get
            {
                return Application.persistentDataPath + "/Server" + CurBundleModuleEnum + "AssetsHotManifest.json";
            }
        }
        //本地资源热更清单文件存储存路径
        private string mLocalHotAssetManisetPath
        {
            get
            {
                return Application.persistentDataPath + "/Local" + CurBundleModuleEnum + "AssetsHotManifest.json";
            }
        }

        //最大下载的资源大小 M
        public float mAssetsMaxSizeM { get; set; }

        //资源已经下载的大小 M
        public float mAssetDownLoadSizeM;
        //资源下载器
        private AssetDownLoader mAssetDownLoader;

        //TODO文件路径重置 热更资源存储路径
        public string HotAssetsSavePath
        {
            get
            {
                return Application.persistentDataPath + "/HotAssets/" + CurBundleModuleEnum + "/";
            }
        }

        //下载所有资源完成的回调
        public Action<BundleModuleEnum> OnDownLoadAllAssetsFinish;

        public HotAssetsModule(BundleModuleEnum bundleModule)
        {
            CurBundleModuleEnum = bundleModule;
        }

        public async UniTask StartHotAssets(bool isCheckAssetsVersion = true)
        {
            //设置状态为正在下载
            CurDownloadStateEnum = E_DownloadState.Downloading;

            //创建一个TCS 捕获所有文件下载完成的命令
            var tcs = new UniTaskCompletionSource();
            OnDownLoadAllAssetsFinish += (module) => tcs.TrySetResult();
            if (isCheckAssetsVersion)
            {
                mNeedDownLoadAssetList.Clear();
                mAllHotAssetList.Clear();
                CheckVersionResult result = await CheckAssetsVersion();
                if (result.isHot)
                {
                    StartDownLoadHotAssets();
                }
                else
                {
                    //AssetBundleManager.Instance.LoadAssetBundelConfig(CurBundleModuleEnum);
                    OnDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
                }
            }
            else
            {
                //OnDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
                //如果不需要检测 就直接开始下载
                StartDownLoadHotAssets();
            }
            await tcs.Task;
        }

        /// <summary>
        /// 开始下载热更资源
        /// </summary>
        /// <param name="startDownLoadCallBack"></param>
        public void StartDownLoadHotAssets()
        {
            //优先下载AssetBundel配置文件，下载完成后调用回调
            //热更资源下载完成后同样需要回调，以便动态加载刚下载的资源
            List<HotFileInfo> downLoadList = new List<HotFileInfo>();
            for (int i = 0; i < mNeedDownLoadAssetList.Count; i++)
            {
                HotFileInfo hotFile = mNeedDownLoadAssetList[i];
                //说明是配置文件需要优先下载
                if (hotFile.abName.Contains("bundleconfig"))
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
            mAssetDownLoader = new AssetDownLoader(this, downLoadQueue, mServerHotAssetsManifest.downLoadUrl, HotAssetsSavePath);


            //开始下载队列中的资源
            mAssetDownLoader.StartThreadDownLoadQueue();
        }


        /// <summary>
        /// 检测资源版本
        /// </summary>
        /// <returns></returns>
        public async UniTask<CheckVersionResult> CheckAssetsVersion()
        {
            //下载资源配置文件
            await DownLoadHotAssetsManifest();

            //1.资源清单下载
            if (CheckModuleAssetsIsHot())
            {
                HotAssetsPatch serverHotPath = mServerHotAssetsManifest.hotAssetsPatcheList[mServerHotAssetsManifest.hotAssetsPatcheList.Count - 1];
                bool isNeedHot = ComputeNeedHotAssetsList(serverHotPath);
                //是否需要热更
                if (isNeedHot)
                    return new CheckVersionResult() { isHot = true, sizeM = mAssetsMaxSizeM };
                else
                    return new CheckVersionResult() { isHot = false, sizeM = 0 };
            }
            else
                return new CheckVersionResult() { isHot = false, sizeM = 0 };

        }

        /// <summary>
        /// 下载Manifest文件
        /// </summary>
        /// <returns></returns>
        public async UniTask DownLoadHotAssetsManifest()
        {
            int downLaodCount = 0;

            for (int i = 0; i < MAX_MANIFEST_DOWN_LOAD_COUNT; i++)
            {
                downLaodCount++;

                string url = BundleSettings.Instance.AssetDownLoadUrl + CurBundleModuleEnum + "AssetsHotManifest.json";
                using UnityWebRequest webRequest = UnityWebRequest.Get(url);
                webRequest.timeout = 30;

                Debug.Log("*** Request HotAssetsMainfest Url:" + url);
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log("***  Request AssetsBundle HotAssetsManifest Url Finish Module:" + CurBundleModuleEnum + "txt:" + webRequest.downloadHandler.text);
                        //写入服务端资源热更清单到本地
                        FileHelper.WriteFile(mServerHotAssetsManifestPath, webRequest.downloadHandler.data);
                        mServerHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(webRequest.downloadHandler.text);
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("服务端资源清单下载异常，文件不存在或者配置有问题,更新出错,正尝试第" + downLaodCount + "次下载" + "\r\n" + e.ToString());
                    }
                }
                else
                {
                    //TODO 处理Manifest下载失败的情况
                    Debug.LogError("Mainfest DownLoad Error,Currently attempting the " + downLaodCount + " re-download" + "\r\n" + webRequest.error + " URL:" + url);
                }
                if (downLaodCount >= MAX_MANIFEST_DOWN_LOAD_COUNT)
                {
                    Debug.LogError("Mainfest DownLoad Error:" + webRequest.error + "\r\nURL:" + url);
                    //TODO 三次下载失败后 提示用户尝试重新进入游戏 

                    break;
                }
                await UniTask.Delay(1000);
            }
        }

        /// <summary>
        /// 计算需要进行热更的文件列表
        /// </summary>
        public bool ComputeNeedHotAssetsList(HotAssetsPatch serverAssetsPath)
        {
            if (!Directory.Exists(HotAssetsSavePath))
                Directory.CreateDirectory(HotAssetsSavePath);
            mAssetsMaxSizeM = 0;
            if (File.Exists(mLocalHotAssetManisetPath))
                mLocalHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(File.ReadAllText(mLocalHotAssetManisetPath));
            foreach (var item in serverAssetsPath.hotAssetsList)
            {
                //获取本地AssetBundle文件路径
                string localFilePath = HotAssetsSavePath + item.abName;
                mAllHotAssetList.Add(item);
                //TODO MD5
                //如果本地文件不存在或者本地文件与服务端不一致 需要热更
                if (!File.Exists(localFilePath) || item.md5 !=/* MD5.GetMd5FromFile(localFilePath)*/GetLocalFileMd5ByBundleName(item.abName))
                {
                    mNeedDownLoadAssetList.Add(item);
                    mAssetsMaxSizeM += item.size / 1024f;
                }

            }
            return mNeedDownLoadAssetList.Count > 0;
        }
        public string GetLocalFileMd5ByBundleName(string bundleName)
        {
            if (mLocalHotAssetsManifest != null && mLocalHotAssetsManifest.hotAssetsPatcheList.Count > 0)
            {
                HotAssetsPatch localPath = mLocalHotAssetsManifest.hotAssetsPatcheList[mLocalHotAssetsManifest.hotAssetsPatcheList.Count - 1];

                foreach (var item in localPath.hotAssetsList)
                {
                    if (string.Equals(bundleName, item.abName))
                    {
                        return item.md5;
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 检测模块资源是否需要热更
        /// </summary>
        /// <returns></returns>
        public bool CheckModuleAssetsIsHot()
        {
            //如果服务器资源清单不存在，不需要热更
            if (mServerHotAssetsManifest == null)
                return false;

            //如果本地资源清单不存在 说明我们需要进行热更
            if (!File.Exists(mLocalHotAssetManisetPath))
                return true;
            //判断本地资源清单与服务器清单的资源数量是否相同 如果不同说明需要热更
            mLocalHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(File.ReadAllText(mLocalHotAssetManisetPath));
            if ((mLocalHotAssetsManifest.hotAssetsPatcheList.Count == 0 && mServerHotAssetsManifest.hotAssetsPatcheList.Count != 0)
                || mLocalHotAssetsManifest.hotAssetsPatcheList.Count != mServerHotAssetsManifest.hotAssetsPatcheList.Count)
                return true;

            //获取本地热更补丁的最后一个补丁
            HotAssetsPatch localHotPatch = mLocalHotAssetsManifest.hotAssetsPatcheList[mLocalHotAssetsManifest.hotAssetsPatcheList.Count - 1];
            //获取服务端热更补丁的最后一个补丁
            HotAssetsPatch serverHotPatch = mServerHotAssetsManifest.hotAssetsPatcheList[mServerHotAssetsManifest.hotAssetsPatcheList.Count - 1];

            //判断本地资源清单补丁号是否于服务端资源清单补丁版本号是否一致，不一致需要进行热更
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
        /// 设置下载线程个数
        /// </summary>
        /// <param name="threadCount"></param>
        public void SetDownLoadThreadCount(int threadCount)
        {
            Debug.Log("多线程负载均衡:" + threadCount + " ModuleType:" + CurBundleModuleEnum);
            if (mAssetDownLoader != null)
                mAssetDownLoader.MAX_THREAD_COUNT = threadCount;
        }
        #region 资源下载回调
        public void DownLoadAssetBundleSuccess(HotFileInfo hotFileInfo)
        {
            string abName = hotFileInfo.abName.Replace(BundleSettings.Instance.BundlePostfix, "");
            //判断是不是资源配置文件
            if (hotFileInfo.abName.Contains("bundleconfig"))
            {
                //TODO 下载成功需要及时加载配置文件
                AssetBundleManager.Instance.LoadAssetBundelConfig(CurBundleModuleEnum);


            }
            else
            {

            }
            Debug.Log("更新完成abName：" + hotFileInfo.abName);
            HotAssetsManager.DownLoadBundleFinish?.Invoke(hotFileInfo);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hotFileInfo"></param>
        public void DownLoadAssetBundleFailed(HotFileInfo hotFileInfo)
        {

        }

        /// <summary>
        /// 所有资源下载完成
        /// </summary>
        public void DownLoadAssetBundleFinish()
        {
            if (File.Exists(mLocalHotAssetManisetPath))
                File.Delete(mLocalHotAssetManisetPath);
            //将服务端热更清单文件拷贝到本地
            File.Copy(mServerHotAssetsManifestPath, mLocalHotAssetManisetPath);
            //设置为下载完成
            CurDownloadStateEnum = E_DownloadState.Finished;
            //需要告诉外部全部下载完成
            OnDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
            //然后置空
            OnDownLoadAllAssetsFinish = null;
            ClearDowanLoadStateChangeCallBack();
            AssetBundleManager.Instance.LoadAssetBundelConfig(CurBundleModuleEnum);
        }
        #endregion

        /// <summary>
        /// 判断热更文件是否存在
        /// </summary>
        /// <param name="bundleName"></param>
        public bool HotAssetsIsExists(string bundleName)
        {
            foreach (var item in mAllHotAssetList)
            {
                if (string.Equals(bundleName, item.abName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 增加状态改变后触发的回调函数
        /// </summary>
        public void AddDowanLoadStateChangeCallBack(Action<E_DownloadState> downLoadStateChangeAction)
        {
            OnDownLoadStateChanged += downLoadStateChangeAction;
        }

        public void RemoveDowanLoadStateChangeCallBack(Action<E_DownloadState> downLoadStateChangeAction)
        {
            OnDownLoadStateChanged -= downLoadStateChangeAction;
        }

        public void ClearDowanLoadStateChangeCallBack()
        {
            OnDownLoadStateChanged = null;
        }

    }
}