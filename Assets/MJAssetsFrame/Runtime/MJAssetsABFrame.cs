using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace MJ.AssetFrameWork.ABFrame
{
    public partial class MJAssetsABFrame : MJABFrameBase
    {
        public static Transform RecyclObjRoot { get; private set; }
        private IHotAssets mHotAssets = null;
        private IResourcesInterface mResources = null;
        private IDecompressAssets mDecompressAssets = null;
        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFrameWork()
        {
            GameObject recyclObjRoot = new GameObject("RecyclObjRoot");
            RecyclObjRoot = recyclObjRoot.transform;
            DontDestroyOnLoad(recyclObjRoot);

            mHotAssets = new HotAssetsManager();
            mDecompressAssets = new AssetsDecompressManager();
            mResources = new ResourcesManager();
            mResources.Initlizate();
        }

        public void Update()
        {
            //执行主线程回调
            mHotAssets?.OnMainThreadUpdate();
        }
    }
}