using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace MJ.AssetFrameWork.ABFrame
{
    public partial class MJAssetsABFrame : MJABFrameBase
    {
        private IHotAssets mHotAssets = null;

        private IDecompressAssets mDecompressAssets = null;
        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFrameWork()
        {
            mHotAssets = new HotAssetsManager();
            mDecompressAssets = new AssetsDecompressManager();
        }

        public void Update()
        {
            //执行主线程回调
            mHotAssets?.OnMainThreadUpdate();
        }
    }
}