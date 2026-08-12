using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public partial class MJAssetsABFrame
    {
        /// <summary>
        /// 开始热更
        /// </summary>
        /// <param name="bundleModule">热更模块</param>
        /// <param name="startHotCallback">开始热更回调</param>
        /// <param name="finishHotCallback">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载回调</param>
        /// <param name="isCheckVersion">是否检测版本</param>
        public void HotAssets(BundleModuleEnum bundleModule, Action<BundleModuleEnum> startHotCallback, Action<BundleModuleEnum> finishHotCallback, Action<BundleModuleEnum> waiteDownLoad, bool isCheckVersion = true)
        {
            mHotAssets.HotAssets(bundleModule, startHotCallback, finishHotCallback, waiteDownLoad, isCheckVersion);
        }

        /// <summary>
        /// 检测资源版本是否需要热更，获取资源版本的大小
        /// </summary>
        /// <param name="bundleModule">热更模块类型</param>
        /// <param name="callBack">检测完成回调</param>
        public void CheckAssetsVersion(BundleModuleEnum bundleModule, Action<bool, float> callBack)
        {
            mHotAssets.CheckAssetsVersion(bundleModule, callBack);
        }

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModule">热更模块类型</param>
        /// <returns></returns>
        public HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModule)
        {
           return mHotAssets.GetHotAssetsModule(bundleModule);
        }
    }
}

