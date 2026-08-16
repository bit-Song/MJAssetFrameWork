using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MJ.AssetFrameWork.ABFrame
{
    public class HotAssetsWindow : MonoBehaviour
    {
        public Slider progressSlider;
        public Text progressText;
        public Text rateText;
        private HotAssetsModule mHotAssetsModule;

        private IDecompressAssets mDecompressAssets;//更新公告总结点

        public GameObject updateNoticeObj;
        public Text updateNoticeText;//更新公告文本
        /// <summary>
        /// 显示解压文件进度
        /// </summary>
        /// <param name="decompress"></param>
        public void ShowDecompressProgress(IDecompressAssets decompress)
        {
            mDecompressAssets = decompress;

            progressSlider.value = 0;
            progressText.text = "";
        }

        /// <summary>
        /// 显示热更资源进度
        /// </summary>
        /// <param name="assetsModule"></param>
        public void ShowHotAssetsProgress(HotAssetsModule assetsModule)
        {
            mDecompressAssets = null;
            mHotAssetsModule = assetsModule;
            progressText.text = "";
            progressSlider.value = 0;
            mHotAssetsModule = assetsModule;
            updateNoticeObj.SetActive(true);
            updateNoticeText.text = assetsModule.UpdateNoticeContent.Replace("\\n", "\n");


        }


        void Update()
        {
            if (mDecompressAssets != null && progressSlider.value != 1f)
            {
                progressText.text = "资源解压中，过程不消耗流量...";

                progressSlider.value = mDecompressAssets.GetDeCompressProgress();
            }

            if (mHotAssetsModule != null)
            {
                progressText.text = string.Format("资源下载中。。。{0}m/{1}m", mHotAssetsModule.mAssetDownLoadSizeM.ToString("F1"), mHotAssetsModule.mAssetsMaxSizeM.ToString("F1"));
                progressSlider.value = mHotAssetsModule.mAssetDownLoadSizeM / mHotAssetsModule.mAssetsMaxSizeM;
            }

        }
    }

}