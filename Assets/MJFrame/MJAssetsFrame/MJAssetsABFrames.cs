using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MJ.AssetFrameWork.ABFrame
{
    public partial class MJAssetsABFrame
    {

        #region HotAsset
        /// <summary>
        /// 开始热更
        /// </summary>
        /// <param name="bundleModule">热更模块</param>
        /// <param name="startHotCallback">开始热更回调</param>
        /// <param name="finishHotCallback">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载回调</param>
        /// <param name="isCheckVersion">是否检测版本</param>
        public static async UniTask HotAssets(BundleModuleEnum bundleModule, bool isCheckVersion = true)
        {
            await instance.mHotAssets.HotAssets(bundleModule, isCheckVersion);
        }

        /// <summary>
        /// 检测资源版本是否需要热更，获取资源版本的大小
        /// </summary>
        /// <param name="bundleModule">热更模块类型</param>
        /// <param name="callBack">检测完成回调</param>
        public static async UniTask<CheckVersionResult> CheckAssetsVersion(BundleModuleEnum bundleModule)
        {
            return await instance.mHotAssets.CheckAssetsVersion(bundleModule);
        }

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name = "bundleModule" > 热更模块类型 </ param >
        /// < returns ></ returns >
        public static HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModule)
        {
            return instance.mHotAssets.GetHotAssetsModule(bundleModule);
        }

        #endregion


        #region DeCompress
        /// <summary>
        /// 开始解压内嵌文件
        /// </summary>
        /// <returns></returns>
        public static IDecompressAssets StartDeCompressBuiltinFile(BundleModuleEnum bundleModuleEnum)
        {
            return instance.mDecompressAssets.StartDeCompressBuiltinFile(bundleModuleEnum);
        }
        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns></returns>
        public static float GetDeCompressProgress()
        {
            return instance.mDecompressAssets.GetDeCompressProgress();
        }

        /// <summary>
        /// 等待资源解压完成
        /// </summary>
        /// <returns></returns>

        public static UniTask WaitDeCompress()
        {
            return instance.mDecompressAssets.WaitDecompress();
        }

        #endregion

        public static void PreLoadObj(string path, int count = 1)
        {
            instance.mResources.PreLoadObj(path, count);
        }

        public static void PreLoadResource<T>(string path) where T : UnityEngine.Object
        {
            instance.mResources.PreLoadResource<T>(path);
        }

        public static GameObject Instantiate(string path)
        {
            return instance.mResources.Instantiate(path);
        }
        public static GameObject Instantiate(string path, Transform parent)
        {
            return instance.mResources.Instantiate(path, parent);
        }
        public static GameObject Instantiate(string path, Transform parent, Vector3 localPosition)
        {
            return instance.mResources.Instantiate(path, parent, localPosition);
        }
        public static GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            return instance.mResources.Instantiate(path, parent, localPosition, localScale);
        }
        public static GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion quateraion)
        {
            return instance.mResources.Instantiate(path, parent, localPosition, localScale, quateraion);
        }
        public static async UniTask<GameObject> InstantiateAsync(string path)
        {
            return await instance.mResources.InstantiateAsync(path);
        }
        public static async UniTask<GameObject> InstantiateAndLoadAsync(string path)
        {
            return await instance.mResources.InstantiateAndLoadAsync(path);
        }

        public static void RemoveObjectLoadTCS(uint crc)
        {
            instance.mResources.RemoveObjectLoadTCS(crc);
        }

        public static void Release(GameObject obj, bool destroy = false)
        {
            instance.mResources.Release(obj, destroy);
        }

        public static void Release(Texture texture)
        {
            instance.mResources.Release(texture);
        }
        public static Sprite LoadSprite(string path)
        {
            return instance.mResources.LoadSprite(path);
        }

        public static Texture LoadTexture(string path)
        {
            return instance.mResources.LoadTexture(path);
        }

        public static AudioClip LoadAudio(string path)
        {
            return instance.mResources.LoadAudio(path);
        }

        public static TextAsset LoadTextAsset(string path)
        {
            return instance.mResources.LoadTextAsset(path);
        }

        public static Sprite LoadAtlasSprite(string atlasPath, string spriteName)
        {
            return instance.mResources.LoadAtlasSprite(atlasPath, spriteName);
        }

        public static async UniTask<Texture> LoadTextureAsync(string path)
        {
            return await instance.mResources.LoadTextureAsync(path);
        }

        public static async UniTask<Sprite> LoadSpriteAsync(string path, Image image, bool setNativeSize = false)
        {
            return await instance.mResources.LoadSpriteAsync(path, image, setNativeSize);
        }
        public static void ClearAllAsyncLoadTask()
        {
            instance.mResources.ClearAllAsyncLoadTask();
        }

        /// <summary>
        /// 是否深度清理
        /// </summary>
        /// <param name="absoluteCleaning"></param>
        public static void ClearResoucesAssets(bool absoluteCleaning)
        {
            instance.mResources.ClearResoucesAssets(absoluteCleaning);
        }


    }
}

