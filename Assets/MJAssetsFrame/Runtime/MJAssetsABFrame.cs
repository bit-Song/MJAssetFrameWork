using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace MJ.AssetFrameWork.ABFrame
{
    public partial class MJAssetsABFrame : MJABFrameBase
    {
        protected IHotAssets mHotAssets = null;
        protected override void OnInitlizate()
        {
            base.OnInitlizate();
        }

        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFrameWork()
        {
            mHotAssets = new HotAssetsManager();
        }

        public void Update()
        {
            //执行主线程回调
            mHotAssets?.OnMainThreadUpdate();
        }
    }
}