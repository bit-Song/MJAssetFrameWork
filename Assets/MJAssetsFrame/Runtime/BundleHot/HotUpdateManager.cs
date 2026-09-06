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

        /// <summary>
        /// 热更并且解压模块
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public async UniTask HotAndPackAssets(BundleModuleEnum bundleModuleEnum)
        {
            IDecompressAssets decompress = MJAssetsABFrame.StartDeCompressBuiltinFile(bundleModuleEnum);
            //等待解压
            await MJAssetsABFrame.WaitDeCompress();

            //检测当前释是否有网络
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }
            else
            {
                Debug.Log("版本检测");
                await CheackAssetsVersion(bundleModuleEnum);
            }

        }

        public async UniTask NotNetButtonClick(BundleModuleEnum bundleModuleEnum)
        {
            //如果当前用户没有网络就弹出弹窗提示
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                await CheackAssetsVersion(bundleModuleEnum);
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
            InitGameEnv().Forget();
        }
        /// <summary>
        /// 热更开始回调
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        public void OnStartHotAssetsCallBack(BundleModuleEnum bundleModuleEnum)
        {

        }

        /// <summary>
        /// 初始化游戏环境
        /// </summary>
        /// <returns></returns>
        private async UniTask InitGameEnv()
        {
            for (int i = 0; i < 100; i++)
            {
                if (i == 1)
                {
                    Debug.Log("加载本地资源");
                }
                else if (i == 20)
                {
                    Debug.Log("加载配置文件");
                }
                else if (i == 70)
                {
                    Debug.Log("加载AssetBundle配置文件");
                }
                else if (i == 90)
                {
                    Debug.Log("加载游戏配置文件");
                    LoadGameConfig();
                }
                else if (i == 99)
                {
                    Debug.Log("加载地图场景");
                }
                await UniTask.Yield();
            }
        }
        /// <summary>
        /// 加载游戏配置文件
        /// </summary>
        public void LoadGameConfig()
        {

        }
        public T InstantiateResourcesObj<T>(string prefabName)
        {
            return GameObject.Instantiate(Resources.Load<GameObject>(prefabName)).GetComponent<T>();
        }
    }
}