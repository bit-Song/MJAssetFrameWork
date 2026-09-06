using Cysharp.Threading.Tasks;
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
        /// <param name="startHotCallBack">开始热更回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载的回调</param>
        /// <param name="isCheckAssetsVersion">是否需要检测资源版本</param>
        UniTask HotAssets(BundleModuleEnum bundleModule, bool isCheckAssetsVersion = true);
        /// <summary>
        /// 检测资源版本是否需要热更，获取需要热更资源的大小
        /// </summary>
        /// <param name="bundleModule">热更模块类型</param>
        /// <param name="callBack">检测完成回调</param>
        UniTask<CheckVersionResult> CheckAssetsVersion(BundleModuleEnum bundleModule);
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