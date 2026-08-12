using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 等待下载的模块
    /// </summary>
    public class WaitDownLoadModule
    {
        public BundleModuleEnum bundleModule;
        public Action<BundleModuleEnum> startHot;
        public Action<BundleModuleEnum> hotFinish;

        public Action<BundleModuleEnum, float> hotAssetsProgressCallBack;
    }



    /// <summary>
    /// 资源热更管理器
    /// </summary>
    public class HotAssetsManager : IHotAssets
    {
        //最大并发下载线程个数
        private int MAX_THREAD_COUNT = 3;

        //所有热更资源模块的字典
        private Dictionary<BundleModuleEnum, HotAssetsModule> allAssetsModuleDic = new Dictionary<BundleModuleEnum, HotAssetsModule>();

        //正在下载热更资源模块的字典
        private Dictionary<BundleModuleEnum, HotAssetsModule> downLoadingAssetsModuleDic = new Dictionary<BundleModuleEnum, HotAssetsModule>();

        //正在下载热更资源的列表
        public List<HotAssetsModule> downLoadAssetsModuleList = new List<HotAssetsModule>();

        //等待下载的队列
        private Queue<WaitDownLoadModule> waitDownLoadModuleQueue = new Queue<WaitDownLoadModule>();


        public void HotAssets(BundleModuleEnum bundleModule, Action<BundleModuleEnum> startHotCallback, Action<BundleModuleEnum> finishHotCallback, Action<BundleModuleEnum> waiteDownLoad, bool isCheckVersion = true)
        {
            //如果不需要热更直接执行热更完成回调即可
            if (BundleSettings.Instance.bundleHotType == E_BundleHotEnum.NoHot)
            {
                finishHotCallback?.Invoke(bundleModule);
                return;
            }

            HotAssetsModule assetsModule = GetOrNewAssetModule(bundleModule);

            //判断是否有闲置的下载线程
            if (downLoadingAssetsModuleDic.Count < MAX_THREAD_COUNT)
            {
                if (!downLoadingAssetsModuleDic.ContainsKey(bundleModule))
                {
                    downLoadingAssetsModuleDic.Add(bundleModule, assetsModule);
                }
                if (!downLoadAssetsModuleList.Contains(assetsModule))
                {
                    downLoadAssetsModuleList.Add(assetsModule);
                }
                assetsModule.OnDownLoadAllAssetsFinish += HotModuleAssetsFinish;
                //开始热更资源
                assetsModule.StartHotAssets(() => { MultipleThreadBalancing(); startHotCallback?.Invoke(bundleModule); }, finishHotCallback);
            }
            else
            {
                waiteDownLoad?.Invoke(bundleModule);
                //把热更模块添加到等待下载队列
                waitDownLoadModuleQueue.Enqueue(new WaitDownLoadModule() { bundleModule = bundleModule, startHot = startHotCallback, hotFinish = finishHotCallback });
            }
        }


        public HotAssetsModule GetOrNewAssetModule(BundleModuleEnum bundleModule)
        {
            HotAssetsModule assetsModule = null;

            if (allAssetsModuleDic.ContainsKey(bundleModule))
                assetsModule = allAssetsModuleDic[bundleModule];
            else
            {
                assetsModule = new HotAssetsModule(bundleModule, MJAssetsABFrame.Instance);
                allAssetsModuleDic.Add(bundleModule, assetsModule);
            }
            return assetsModule;
        }


        /// <summary>
        /// 检测资源版本是否需要热更
        /// </summary>
        /// <param name="bundleModule">热更模块</param>
        /// <param name="callBack">热更回调</param>
        public void CheckAssetsVersion(BundleModuleEnum bundleModule, Action<bool, float> callBack)
        {
            HotAssetsModule assetsModule = GetOrNewAssetModule(bundleModule);
            assetsModule.CheckAssetsVersion(callBack);
        }

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <returns></returns>
        public HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModule)
        {
            if (allAssetsModuleDic.ContainsKey(bundleModule))
                return allAssetsModuleDic[bundleModule];

            return null;
        }


        /// <summary>
        /// 热更模块资源完成
        /// </summary>
        /// <param name="bundleModule"></param>
        public void HotModuleAssetsFinish(BundleModuleEnum bundleModule)
        {
            //将下载完成的模块从下载的字典中移除
            if (downLoadingAssetsModuleDic.ContainsKey(bundleModule))
            {
                HotAssetsModule assetsModule = downLoadingAssetsModuleDic[bundleModule];
                if (downLoadAssetsModuleList.Contains(assetsModule))
                    downLoadAssetsModuleList.Remove(assetsModule);

                downLoadingAssetsModuleDic.Remove(bundleModule);
            }
            //判断等待下载的队列中是否有等待热更的模块
            //如果存在就开始进行热更
            if (waitDownLoadModuleQueue.Count > 0)
            {
                WaitDownLoadModule waitDownLoadModule = waitDownLoadModuleQueue.Dequeue();
                HotAssets(waitDownLoadModule.bundleModule, waitDownLoadModule.startHot, waitDownLoadModule.hotFinish, null);
            }
            else
            {
                //TODO 负载均衡
                //在没有等待热更的情况下 并且有线程空闲的情况 需要把闲置的线程分配给其他正在热更的模块
                MultipleThreadBalancing();

            }
        }

        /// <summary>
        /// 多线程均衡
        /// </summary>
        public void MultipleThreadBalancing()
        {
            //获取当前正在下载的热更资源模块的个数
            int count = downLoadingAssetsModuleDic.Count;

            //计算多线程均衡后分配个数
            //以最大下载线程个数为3为例
            //1.   3/1 = 3；最大并发下载线程个数为3 (偶数)
            //2.   3/2 = 1.5f； 向上取整 2 1（奇数）
            //3.   3/3 = 1；  每个模块都有一个下载线程
            float threadCount = MAX_THREAD_COUNT * 1.0f / count;
            //主线程下载个数
            int mainThreadCount = 0;
            //通过（int）强转向下取整
            int threadBalancingCount = (int)threadCount;

            //说明为奇数线程
            if ((int)threadCount < threadCount)
            {
                //向上取整
                mainThreadCount = Mathf.CeilToInt(threadCount);
                //向下取整
                threadBalancingCount = Mathf.FloorToInt(threadCount);
            }
            //多线程均衡
            int i = 0;
            foreach (var item in downLoadingAssetsModuleDic.Values)
            {
                if (mainThreadCount != 0 && i == 0)
                {
                    item.SetDownLoadThreadCount(mainThreadCount);//设置主下载线程个数
                }
                else
                {
                    item.SetDownLoadThreadCount(threadBalancingCount);
                }
                i++;
            }


        }


        /// <summary>
        /// 主线程更新
        /// </summary>
        public void OnMainThreadUpdate()
        {
            for (int i = 0; i < downLoadAssetsModuleList.Count; i++)
            {
                downLoadAssetsModuleList[i].OnMainThreadUpdate();
            }
        }



    }
}

