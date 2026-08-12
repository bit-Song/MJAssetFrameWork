using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// <summary>
    /// 多线程资源下载器
    /// </summary>
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

        //下载成功回调
        private DownLoadEvent downLoadSuccess;

        //下载失败回调
        private DownLoadEvent downLoadFailed;

        //下载完成回调
        private DownLoadEvent downLoadFinish;

        //下载回调的列表
        private Queue<DownLoadEventHandler> downLaodEventQueue = new Queue<DownLoadEventHandler>();

        //当前所有正在下载的线程列表
        private List<DownLoadThread> allDownLoadThreadList = new List<DownLoadThread>();
        /// <summary>
        /// 资源下载器
        /// </summary>
        /// <param name="hotAssetsManifest">资源下载模块</param>
        /// <param name="downLoadQueue">资源下载队列</param>
        /// <param name="downLoadUrl">资源下载路径</param>
        /// <param name="hotAssetsSavePath">热更资源存储路径</param>
        /// <param name="downSuccess">下载成功回调</param>
        /// <param name="dpwnloadFailed">下载失败回调</param>
        /// <param name="downLoadFinish">下载结束回调</param>
        public AssetDownLoader(HotAssetsModule hotAssetsModule, Queue<HotFileInfo> downLoadQueue, string downLoadUrl, string hotAssetsSavePath,
            DownLoadEvent downLoadSuccess, DownLoadEvent downLoadFailed, DownLoadEvent downLoadFinish)
        {
            this.curHotAssetsModule = hotAssetsModule;
            this.downLoadQueue = downLoadQueue;
            this.assetsDownLoadUrl = downLoadUrl;
            this.downLoadSuccess = downLoadSuccess;
            this.hotAssetsSvaePath = hotAssetsSavePath;
            this.downLoadFailed = downLoadFailed;
            this.downLoadFinish = downLoadFinish;
        }

        public void StartThreadDownLoadQueue()
        {
            //根据最大的线程下载个数，开启基础下载通道

            for (int i = 0; i < MAX_THREAD_COUNT; i++)
            {
                if (downLoadQueue.Count > 0)
                {
                    Debug.Log("Start DownLoad AssetBundlel MAX_THREAD_COUTN:" + MAX_THREAD_COUNT);
                    StartDownLoadNextBundle();
                }
            }

        }

        /// <summary>
        /// 开始下载下一个AssetBundle
        /// </summary>
        private void StartDownLoadNextBundle()
        {
            HotFileInfo hotFileInfo = downLoadQueue.Dequeue();
            DownLoadThread downLoadItem = new DownLoadThread(curHotAssetsModule, hotFileInfo, assetsDownLoadUrl, hotAssetsSvaePath);

            downLoadItem.StartDownLoad(DownLoadSuccess, DownLoadFailed);
            allDownLoadThreadList.Add(downLoadItem);
        }

        /// <summary>
        /// 开始下载下一个AssetBundle
        /// </summary>
        public void DownLoadNextBundle()
        {
            //如果当前下载的线程个数已经达到上限就关闭当前下载通道
            if (allDownLoadThreadList.Count > MAX_THREAD_COUNT)
            {
                Debug.Log("DownLoadNextBundle Out MaxTheadCount,Close this DownLoad Channel...");
                return;
            }

            if (downLoadQueue.Count > 0)
            {
                StartDownLoadNextBundle();
                if (allDownLoadThreadList.Count < MAX_THREAD_COUNT)
                {
                    //计算正在等待下载线程的个数
                    int idleThreadCount = MAX_THREAD_COUNT - allDownLoadThreadList.Count;

                    for (int i = 0; i < idleThreadCount; i++)
                    {
                        if (downLoadQueue.Count > 0)
                        {
                            StartDownLoadNextBundle();
                        }
                    }
                }
            }
            else
            {
                //等待下载中的线程全部结束说明文件下载完成
                if (allDownLoadThreadList.Count == 0)
                {
                    TriggerCallBackInMainThread(new DownLoadEventHandler { downLoadEvent = downLoadFinish });
                }
            }

        }

        /// <summary>
        /// 下载成功
        /// </summary>
        /// <param name="downLoadThread"></param>
        /// <param name="hotFileInfo"></param>
        public void DownLoadSuccess(DownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            RemoveDownLoadThread(downLoadThread);
            //因为是在子线程中下载，所以回调也是在子线程中触发的
            //我们需要将回调放到主线程中调用
            //加入到下载回调的队列中  
            TriggerCallBackInMainThread(new DownLoadEventHandler { downLoadEvent = downLoadSuccess, hotFileInfo = hotFileInfo });
            DownLoadNextBundle();
        }

        /// <summary>
        /// 下载失败
        /// </summary>
        /// <param name="downLoadThread"></param>
        /// <param name="hotFileInfo"></param>
        public void DownLoadFailed(DownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            RemoveDownLoadThread(downLoadThread);
            TriggerCallBackInMainThread(new DownLoadEventHandler { downLoadEvent = downLoadFailed, hotFileInfo = hotFileInfo });
            DownLoadNextBundle();
        }
        /// <summary>
        /// 在主线程中触发回调
        /// </summary>
        /// <param name="downLoadEventHandler"></param>
        public void TriggerCallBackInMainThread(DownLoadEventHandler downLoadEventHandler)
        {
            lock (downLaodEventQueue)
            {
                downLaodEventQueue.Enqueue(downLoadEventHandler);
            }
        }


        /// <summary>
        /// 主线程更新接口
        /// </summary>
        public void OnMainThreadUpdate()
        {
            //在这里处理下载完成的回调信息 
            //if (downLoadQueue.Count > 0)
            //{
            //    DownLoadEventHandler downLoadEvent = downLaodEventQueue.Dequeue();
            //    downLoadEvent.downLoadEvent?.Invoke(downLoadEvent.hotFileInfo);
            //}
            if (downLaodEventQueue.Count > 0)
            {
                DownLoadEventHandler downLoadEvent = downLaodEventQueue.Dequeue();
                downLoadEvent.downLoadEvent?.Invoke(downLoadEvent.hotFileInfo);
            }
        }
        public void RemoveDownLoadThread(DownLoadThread downLoadThread)
        {
            if (allDownLoadThreadList.Contains(downLoadThread))
                allDownLoadThreadList.Remove(downLoadThread);
        }
    }
}
