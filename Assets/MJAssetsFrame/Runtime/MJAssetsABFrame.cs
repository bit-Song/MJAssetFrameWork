using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using MJ.AssetFrameWork.ABFrame.Pool;

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
            RecyclObjRoot.gameObject.SetActive(false);
            DontDestroyOnLoad(recyclObjRoot);

            //初始化对象池管理器（懒创建PoolManager节点，先于任何取池操作）
            PoolManager.Instance.InitInfo();

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