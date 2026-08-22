using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MJ.AssetFrameWork.ABFrame
{

    public class GameModeItem : MonoBehaviour
    {
        public Button button;
        public Image downSliderImage;
        public Text downLoadSpeedText;//下载速度  3m/s
        public Text downLoadRationText;//下载百分比进度 60%
        public Text downLoadProgressText;//下载进度 30m/110m
        public Text downLoadTips;//开始下载提示
        public GameObject updateRoot; //热更总结点

        public BundleModuleEnum gameType;

        private HotAssetsModule mHotModule;

        private float lastTime;
        private float lasetDownLoadSize;

        private void Start()
        {
            button.onClick.AddListener(OnClick);
        }
        private void Update()
        {
            if (mHotModule != null)
            {
                //如果是等待状态
                if (mHotModule.CurDownloadStateEnum == E_DownloadState.Waiting)
                {
                    updateRoot.SetActive(true);
                    downLoadTips.text = "等待更新中";
                    return;
                }

                downLoadProgressText.text = string.Format("{0}m/{1}m",
                    mHotModule.mAssetDownLoadSizeM.ToString("F0"), mHotModule.mAssetsMaxSizeM.ToString("F0"));

                downLoadRationText.text = (mHotModule.mAssetDownLoadSizeM / mHotModule.mAssetsMaxSizeM * 100).ToString("F0") + "%";
                downSliderImage.fillAmount = mHotModule.mAssetDownLoadSizeM / mHotModule.mAssetsMaxSizeM;

                if (Time.realtimeSinceStartup - lastTime > 1)
                {
                    downLoadSpeedText.text = (mHotModule.mAssetDownLoadSizeM - lasetDownLoadSize).ToString("F1") + "M";
                    lasetDownLoadSize = mHotModule.mAssetDownLoadSizeM;
                    lastTime = Time.realtimeSinceStartup;
                }


            }
        }

        public void OnClick()
        {
            OnGameButtonClick().Forget();
        }
        public async UniTask OnGameButtonClick()
        {
            CheckVersionResult checkVersionResult = await MJAssetsABFrame.CheckAssetsVersion(gameType);

            if (checkVersionResult.isHot)
            {
                updateRoot.SetActive(true);
                downLoadTips.text = "正在更新";
                mHotModule = MJAssetsABFrame.GetHotAssetsModule(gameType);
                await MJAssetsABFrame.HotAssets(gameType, false);

                mHotModule = null;
                updateRoot.SetActive(false);
                downLoadTips.text = "更新完成";
                Debug.Log("资源热更完成" + gameType);

            }
            else
            {
                MJAssetsABFrame.ClearResoucesAssets(true);
                AssetBundleManager.Instance.LoadAssetBundelConfig(gameType);
                MJAssetsABFrame.Instantiate("Assets/BundleDemo/" + gameType + "/Prefab/" + gameType + "Window");
            }

        }

    }
}
