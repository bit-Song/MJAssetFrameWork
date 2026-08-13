using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async UniTask HotAssets(BundleModuleEnum bundleModule, bool isCheckVersion = true)
        {
           await mHotAssets.HotAssets(bundleModule, isCheckVersion);
        }

        /// <summary>
        /// 检测资源版本是否需要热更，获取资源版本的大小
        /// </summary>
        /// <param name="bundleModule">热更模块类型</param>
        /// <param name="callBack">检测完成回调</param>
        public async UniTask<CheckVersionResult> CheckAssetsVersion(BundleModuleEnum bundleModule)
        {
            return await mHotAssets.CheckAssetsVersion(bundleModule);
        }

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name = "bundleModule" > 热更模块类型 </ param >
        /// < returns ></ returns >
        public HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModule)
        {
            return mHotAssets.GetHotAssetsModule(bundleModule);
        }
    }
}

