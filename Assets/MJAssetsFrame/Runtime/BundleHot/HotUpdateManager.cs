using System.Collections;
using System.Collections.Generic;
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


        /// <summary>
        /// 检测资源版本
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public void CheackAssetsVersion(BundleModuleEnum bundleModuleEnum)
        {
            MJAssetsABFrame.Instance.CheckAssetsVersion(bundleModuleEnum, (isHot, sizeM) =>
            {
                if (isHot)
                {
                    //当用户使用流量时，需要询问用户是否需要更新资源
                    if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                    {
                        UpdateTipsWindow updateTipsWindow = InstantiateResourcesObj<UpdateTipsWindow>("UpdateTipsWindow");
                        updateTipsWindow.InitView("当前有" + sizeM.ToString("F2") + "M资源需要更新，是否更新",
                            () =>
                            {
                                //确认更新
                                StartHotAssets(bundleModuleEnum);
                            },
                            () =>
                            {
                                //退出游戏
                                Application.Quit();
                            });
                    }
                    else
                    {
                        StartHotAssets(bundleModuleEnum);
                    }
                    OnHotFinishCallBack(bundleModuleEnum);
                }
                else
                {
                    //如果不需要热更说明用户已经更新过了,资源是最新的，可以直接进入游戏
                    OnHotFinishCallBack(bundleModuleEnum);

                }
            });
        }


        /// <summary>
        /// 开始热更资源
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public void StartHotAssets(BundleModuleEnum bundleModuleEnum)
        {
            MJAssetsABFrame.Instance.HotAssets(bundleModuleEnum, OnStartHotAssetsCallBack, OnHotFinishCallBack, null, false);
        }

        /// <summary>
        /// 热更完成回调
        /// </summary>
        public void OnHotFinishCallBack(BundleModuleEnum bundleModuleEnum)
        {
            Debug.Log("资源热更完成 OnHotFinishCallBack 。。。。");

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
