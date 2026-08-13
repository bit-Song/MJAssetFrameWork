using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class DownLoadEventHandler
    {
        public DownLoadEvent downLoadEvent;//回调
        public HotFileInfo hotFileInfo;
    }

    //下载事件
    public delegate void DownLoadEvent(HotFileInfo hotInfo);

    public class AssetDownLoader
    {
        //最大下载线程个数
        public int MAX_THREAD_COUNT = 3;

        //资源文件下载地址
        public string assetsDownLoadUrl;
        //热更文件的存储路径
        public string hotAssetsSvaePath;

        //当前热更的资源模块
        public HotAssetsModule curHotAssetsModule;

        //文件下载队列
        private Queue<HotFileInfo> downLoadQueue;

        //当前所有正在下载的线程列表
        private List<DownLoadThread> mAllDownLoadThreadList = new List<DownLoadThread>();

        //防止完成回调触发多次
        private bool mIsFinished = false;

        /// <summary>
        /// 资源下载器
        /// </summary>
        public AssetDownLoader(HotAssetsModule hotAssetsModule, Queue<HotFileInfo> downLoadQueue, string downLoadUrl, string hotAssetsSavePath)
        {
            this.curHotAssetsModule = hotAssetsModule;
            this.downLoadQueue = downLoadQueue;
            this.assetsDownLoadUrl = downLoadUrl;
            this.hotAssetsSvaePath = hotAssetsSavePath;
        }

        /// <summary>
        /// 开始下载队列中的资源
        /// </summary>
        public void StartThreadDownLoadQueue()
        {
            lock (mAllDownLoadThreadList)
            {
                for (int i = 0; i < MAX_THREAD_COUNT; i++)
                {
                    if (downLoadQueue.Count > 0)
                    {
                        Debug.Log("Start DownLoad AssetBundle MAX_THREAD_COUNT:" + MAX_THREAD_COUNT);
                        //并行下载处理
                        StartDownLoadNextBundle().Forget();
                    }
                }
            }
        }
        /// <summary>
        /// 下载完成后启动下一个
        /// </summary>
        public void DownLoadNextBundle()
        {
            lock (mAllDownLoadThreadList)
            {
                if (mIsFinished)
                    return;

                //队列还有文件，启动下一个下载
                if (downLoadQueue.Count > 0)
                {
                    StartDownLoadNextBundle().Forget();
                }
                //队列空了且没有正在下载的线程，说明全部完成
                else if (mAllDownLoadThreadList.Count == 0)
                {
                    mIsFinished = true;
                    curHotAssetsModule.DownLoadAssetBundleFinish();
                }
            }
        }

        /// <summary>
        /// 移除下载线程
        /// </summary>
        public void RemoveDownLoadThread(DownLoadThread downLoadThread)
        {
            lock (mAllDownLoadThreadList)
            {
                if (mAllDownLoadThreadList.Contains(downLoadThread))
                    mAllDownLoadThreadList.Remove(downLoadThread);
            }
        }
        /// <summary>
        /// 开始下载下一个AssetBundle
        /// </summary>
        private async UniTask StartDownLoadNextBundle()
        {
            HotFileInfo hotFileInfo = downLoadQueue.Dequeue();
            DownLoadThread downLoadItem = new DownLoadThread(curHotAssetsModule, hotFileInfo, assetsDownLoadUrl, hotAssetsSvaePath);
            mAllDownLoadThreadList.Add(downLoadItem);
            //await DownLoadAsync(downLoadItem, hotFileInfo);
            //下载成功后进行处理 下载成功后的回调
            bool success = await downLoadItem.StartDownLoad();
            RemoveDownLoadThread(downLoadItem);

            //处理下载成功后的内容
            if (success)
            {
                curHotAssetsModule.DownLoadAssetBundleSuccess(hotFileInfo);
            }
            else
            {
                curHotAssetsModule.DownLoadAssetBundleFailed(hotFileInfo);
            }
            DownLoadNextBundle();
        }

    }
}