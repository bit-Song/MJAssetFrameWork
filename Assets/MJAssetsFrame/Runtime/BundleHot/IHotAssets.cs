using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public interface IHotAssets
    {
        /// <summary>
        /// 开始热更
        /// </summary>
        /// <param name="bundleModule">热更模块</param>
        /// <param name="startHotCallback">开始热更回调</param>
        /// <param name="finishHotCallback">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载回调</param>
        /// <param name="isCheckVersion">是否检测版本</param>
        void HotAssets(BundleModuleEnum bundleModule, Action<BundleModuleEnum> startHotCallback, Action<BundleModuleEnum> finishHotCallback, Action<BundleModuleEnum> waiteDownLoad, bool isCheckVersion = true);

        /// <summary>
        /// 检测资源版本是否需要热更，获取资源版本的大小
        /// </summary>
        /// <param name="bundleModule">热更模块类型</param>
        /// <param name="callBack">检测完成回调</param>
        void CheckAssetsVersion(BundleModuleEnum bundleModule, Action<bool, float> callBack);

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModule">热更模块类型</param>
        /// <returns></returns>
        HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModule);

        /// <summary>
        /// 主线程更新
        /// </summary>
        void OnMainThreadUpdate();
    }
}

