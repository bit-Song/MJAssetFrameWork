using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class ResourcesManager
    {
        //已经加载过的资源字典 key 为路径  value 为资源对象
        private Dictionary<uint, BundleItem> mAlreayLoadAssetsDic = new Dictionary<uint, BundleItem>();

        /// <summary>
        /// 同步资源加载 外部直接调用，用来加载不需要实例化的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("LoadResource Failed path is null");
                return null;
            }
            uint crc = Crc32.GetCrc32(path);
            BundleItem item = GetCacheItemFormAssetDic(crc);

            //如果加载过就直接返回
            if (item.obj != null)
                return item.obj as T;

            T obj = null;
#if UNITY_EDITOR

            if (BundleSettings.Instance.loadAssetType == E_LoadAssetEnum.Editor)
            {
                obj = LoadAssetsFromeEdiot<T>(path);
            }
#endif
            if (obj == null)
            {
                //加载对应的AssetBundle
                item = AssetBundleManager.Instance.LoadAssetBundle(crc);
                if (item != null)
                {
                    if (item.assetName != null)
                    {
                        obj = item.obj != null ? item.obj as T : item.assetBundle.LoadAsset<T>(item.assetName);
                    }
                    else
                    {
                        Debug.LogError("item.assetBundle is null");
                    }
                }
                else
                {
                    Debug.LogError("item is null ,path:" + path);
                }
            }
            item.obj = obj;
            item.path = path;
            //缓存已经加载过的资源
            if (mAlreayLoadAssetsDic.ContainsKey(crc))
                mAlreayLoadAssetsDic.Add(crc, item);
            return obj;
        }


        /// <summary>
        /// 异步资源加载 外部直接调用，用来加载不需要实例化的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<T> LoadResourceAsync<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("LoadResource Failed path is null");
                return null;
            }
            uint crc = Crc32.GetCrc32(path);
            BundleItem item = GetCacheItemFormAssetDic(crc);

            //如果加载过就直接返回
            if (item.obj != null)
                return item.obj as T;

            T obj = null;
#if UNITY_EDITOR

            if (BundleSettings.Instance.loadAssetType == E_LoadAssetEnum.Editor)
            {
                obj = LoadAssetsFromeEdiot<T>(path);
            }
#endif
            if (obj == null)
            {
                //加载对应的AssetBundle
                item = AssetBundleManager.Instance.LoadAssetBundle(crc);
                if (item != null)
                {
                    if (item.obj != null)
                    {
                        item.path = path;
                        item.crc = crc;
                        if (!mAlreayLoadAssetsDic.ContainsKey(crc))
                            mAlreayLoadAssetsDic.Add(crc, item);
                        return item.obj as T;
                    }
                    else
                    {
                        AssetBundleRequest request = item.assetBundle.LoadAssetAsync<T>(item.assetName);
                        await request;
                        item.obj = request.asset as T;
                        item.path = path;
                        item.crc = crc;
                        if (!mAlreayLoadAssetsDic.ContainsKey(crc))
                            mAlreayLoadAssetsDic.Add(crc, item);
                        //等待加载完成后返回
                        return item.obj as T;
                    }

                }
                else
                {
                    Debug.LogError("item is null ,path:" + path);
                }
            }
            else
            {
                item.obj = obj;
                item.path = path;
                //缓存已经加载过的资源
                if (!mAlreayLoadAssetsDic.ContainsKey(crc))
                    mAlreayLoadAssetsDic.Add(crc, item);
            }
            return obj;
        }
        /// <summary>
        /// 从缓存中获取我们的BundleItem
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        public BundleItem GetCacheItemFormAssetDic(uint crc)
        {
            BundleItem item = null;
            mAlreayLoadAssetsDic.TryGetValue(crc, out item);

            return item != null ? item : new BundleItem { crc = crc };

        }

#if UNITY_EDITOR
        public T LoadAssetsFromeEdiot<T>(string path) where T : UnityEngine.Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
#endif
}