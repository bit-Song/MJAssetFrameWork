using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 热更流程代码
    /// </summary>
    public class HotUpdateManager
    {
        private static HotUpdateManager instance = new HotUpdateManager();
        public static HotUpdateManager Instance => instance;

        private HotAssetsWindow mHotAssetsWindow;

        /// <summary>
        /// 热更并且解压模块
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public async UniTask HotAndPackAssets(BundleModuleEnum bundleModuleEnum)
        {

            mHotAssetsWindow = InstantiateResourcesObj<HotAssetsWindow>("HotAssetsWindow");

            //开始解压游戏内嵌资源
            //IDecompressAssets decompress = await MJAssetsABFrame.StartDeCompressBuiltinFile(bundleModuleEnum);
            IDecompressAssets decompress = MJAssetsABFrame.StartDeCompressBuiltinFile(bundleModuleEnum);
            mHotAssetsWindow.ShowDecompressProgress(decompress);
            //等待解压
            await MJAssetsABFrame.WaitDeCompress();
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                InstantiateResourcesObj<UpdateTipsWindow>("UpdatetipsWindow").InitView("当前无网络，请检测后重试", () => { NotNetButtonClick(bundleModuleEnum).Forget(); }, () => { NotNetButtonClick(bundleModuleEnum).Forget(); });
                return;
            }
            else
            {
                CheackAssetsVersion(bundleModuleEnum).Forget();
            }

        }

        public async UniTask NotNetButtonClick(BundleModuleEnum bundleModuleEnum)
        {
            //如果当前用户没有网络就弹出弹窗提示
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                CheackAssetsVersion(bundleModuleEnum).Forget();
            }
            else
            {

            }
        }


        /// <summary>
        /// 检测资源版本
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public async UniTask CheackAssetsVersion(BundleModuleEnum bundleModuleEnum, bool isCheckVersion = true)
        {
            CheckVersionResult checkVersionResult = await MJAssetsABFrame.CheckAssetsVersion(bundleModuleEnum);

            if (checkVersionResult.isHot)
            {
                //当用户使用流量时，需要询问用户是否需要更新资源
                if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    UpdateTipsWindow updateTipsWindow = InstantiateResourcesObj<UpdateTipsWindow>("UpdateTipsWindow");
                    updateTipsWindow.InitView("当前有" + checkVersionResult.sizeM.ToString("F2") + "M资源需要更新，是否更新",
                        async () =>
                        {
                            OnStartHotAssetsCallBack(bundleModuleEnum);
                            //确认更新
                            await StartHotAssets(bundleModuleEnum, isCheckVersion);
                            //完成回调
                            OnHotFinishCallBack(bundleModuleEnum);
                        },
                        () =>
                        {
                            //退出游戏
                            Application.Quit();
                        });
                }
                else
                {
                    await StartHotAssets(bundleModuleEnum, isCheckVersion);
                    //完成回调
                    OnHotFinishCallBack(bundleModuleEnum);
                }
            }
            else
            {
                //如果不需要热更说明用户已经更新过了,资源是最新的，可以直接进入游戏
                OnHotFinishCallBack(bundleModuleEnum);

            }
        }

        /// <summary>
        /// 开始热更资源
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public async UniTask StartHotAssets(BundleModuleEnum bundleModuleEnum, bool isCheckVersion = true)
        {
            //更新热更进度
            mHotAssetsWindow.ShowHotAssetsProgress(MJAssetsABFrame.GetHotAssetsModule(bundleModuleEnum));
            await MJAssetsABFrame.HotAssets(bundleModuleEnum, isCheckVersion);
        }

        /// <summary>
        /// 热更完成回调
        /// </summary>
        public void OnHotFinishCallBack(BundleModuleEnum bundleModuleEnum)
        {
            Debug.Log("资源热更完成 OnHotFinishCallBack 。。。。");
            //加载资源配置文件
            AssetBundleManager.Instance.LoadAssetBundelConfig(bundleModuleEnum);

        }
        /// <summary>
        /// 热更开始回调
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public void OnStartHotAssetsCallBack(BundleModuleEnum bundleModuleEnum)
        {

        }

        public T InstantiateResourcesObj<T>(string prefabName)
        {
            return GameObject.Instantiate(Resources.Load<GameObject>(prefabName)).GetComponent<T>();
        }
    }
}