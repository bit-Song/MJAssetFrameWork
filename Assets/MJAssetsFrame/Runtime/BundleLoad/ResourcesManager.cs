using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using MJ.AssetFrameWork.ABFrame.Pool;

namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 缓存对象
    /// </summary>
    public class CacheObject
    {
        public uint crc;
        public string path;
        public int insid;
        public GameObject obj;
        public void Release()
        {
            crc = 0;
            insid = 0;
            path = "";
            obj = null;
        }
    }


    public class ResourcesManager : IResourcesInterface
    {
        //已经加载过的资源字典 key 为路径  value 为资源对象
        private Dictionary<uint, BundleItem> mAlreayLoadAssetsDic = new Dictionary<uint, BundleItem>();

        //所有对象字典
        private Dictionary<int, CacheObject> mAllObjectDic = new Dictionary<int, CacheObject>();
        //缓存对象类对象池（统一由PoolManager管理）
        private IObjectPool<CacheObject> mCacheObjectPool;
        private IObjectPool<CacheObject> CacheObjPool => mCacheObjectPool ??= PoolManager.Instance.GetOrCreateClassPool(() => new CacheObject(), onGet: obj => obj.Release());

        //每个资源路径的活跃实例计数（在场景中 + 在对象池中），为0时才卸载AssetBundle
        private Dictionary<uint, int> mActiveInstanceCountDic = new Dictionary<uint, int>();

        //任务加载异步列表
        private List<long> mAsyncLoadingTaskList = new List<long>();
        //异步加载任务唯一id
        private long asyncGuid;
        /// <summary>
        /// 异步加载任务唯一id
        /// </summary>
        private long mAsyncTaskGuid
        {
            get
            {
                if (asyncGuid > long.MaxValue)
                    asyncGuid = 0;
                return asyncGuid++;
            }
        }


        private Dictionary<uint, UniTaskCompletionSource> mWaitDownloadTcs = new Dictionary<uint, UniTaskCompletionSource>();

        //等待加载的资源列表
        private List<HotFileInfo> mWaitLoadAndCloneAssetsList = new List<HotFileInfo>();

        public void Initlizate()
        {
            HotAssetsManager.DownLoadBundleFinish += AssetsDownLoadFinish;
        }


        #region 对象加载
        /// <summary>
        /// AB包资源下载完成回调
        /// </summary>
        /// <param name="info"></param>
        private void AssetsDownLoadFinish(HotFileInfo info)
        {
            //处理比配置文件先下载的ab包
            if (info.abName.Contains("bundleconfig"))
            {
                Debug.Log("Handler waitLoadList Count:" + mWaitLoadAndCloneAssetsList.Count);
                HotFileInfo[] hotFileArray = mWaitLoadAndCloneAssetsList.ToArray();
                mWaitLoadAndCloneAssetsList.Clear();
                foreach (var item in hotFileArray)
                {
                    AssetsDownLoadFinish(item);
                }
                return;
            }

            if (mWaitDownloadTcs.Count > 0)
            {
                //从这个ab包中得到这个包中所有的文件信息
                List<BundleItem> assetsItemLsit = AssetBundleManager.Instance.GetBundleItemByAbName(info.abName);
                //如果长度为0 说明配置文件没有加载
                //由于资源下载是多线程下载，会出现assetbundle下载速度大于配置文件下载速度
                if (assetsItemLsit.Count == 0)
                {
                    for (int i = 0; i < mWaitLoadAndCloneAssetsList.Count; i++)
                    {
                        if (mWaitLoadAndCloneAssetsList[i].abName == info.abName)
                            return;
                    }
                    mWaitLoadAndCloneAssetsList.Add(info);
                    return;
                }
                else
                {
                    uint crc = 0;

                    for (int i = 0; i < assetsItemLsit.Count; i++)
                    {
                        crc = Crc32.GetCrc32(assetsItemLsit[i].path);
                        if (mWaitDownloadTcs.ContainsKey(crc))
                        {
                            mWaitDownloadTcs[crc].TrySetResult();
                            Debug.Log("ResourcesManager AssetsDownLoadFinish Load obj path:" + assetsItemLsit[i].path);
                        }
                    }
                }
            }
        }

        #region 同步克隆物体重载
        /// <summary>
        /// 同步克隆物体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <param name="localPosition"></param>
        /// <param name="localScale"></param>
        /// <param name="quateraion"></param>
        /// <returns></returns>
        public GameObject Instantiate(string path)
        {
            return Instantiate(path, null);
        }
        public GameObject Instantiate(string path, Transform parent)
        {
            return Instantiate(path, parent, Vector3.zero);
        }
        public GameObject Instantiate(string path, Transform parent, Vector3 localPosition)
        {
            return Instantiate(path, parent, localPosition, Vector3.one);
        }
        public GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            return Instantiate(path, parent, localPosition, localScale, Quaternion.identity);
        }
        public GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion quateraion)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            //从对象池获取对象（池空时自动同步加载AB并克隆，完整记账）
            GameObject chacheObj = GetOrCreateGoPool(Crc32.GetCrc32(path), path).Get();
            if (chacheObj == null)
            {
                Debug.LogError("GameObject Load failed ,path is null ...");
                return null;
            }
            chacheObj.transform.SetParent(parent);
            chacheObj.transform.localPosition = localPosition;
            chacheObj.transform.localScale = localScale;
            chacheObj.transform.rotation = quateraion;
            return chacheObj;
        }
        #endregion
        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private GameObject InstantiateClone(string path, GameObject obj, Transform parent)
        {
            obj = GameObject.Instantiate(obj, parent, false);
            CacheObject cacheObject = CacheObjPool.Get();
            cacheObject.obj = obj;
            cacheObject.path = path;
            cacheObject.crc = Crc32.GetCrc32(path);
            cacheObject.insid = obj.GetInstanceID();
            mAllObjectDic.Add(cacheObject.insid, cacheObject);

            //增加克隆物体在场景中的计数 用来卸载ab包
            uint crc = cacheObject.crc;
            if (mActiveInstanceCountDic.ContainsKey(crc))
                mActiveInstanceCountDic[crc]++;
            else
                mActiveInstanceCountDic.Add(crc, 1);
            return obj;
        }

        /// <summary>
        /// 异步克隆物体
        /// </summary> 
        /// <param name="path">路径</param>
        /// <param name="loadAsync">加载回调</param>
        /// <param name="param1">加载参数</param>
        /// <param name="param2">加载参数2</param>
        public async UniTask<GameObject> InstantiateAsync(string path)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            IObjectPool<GameObject> goPool = GetOrCreateGoPool(Crc32.GetCrc32(path), path);
            //先从对象池查找对象（不扩展，避免空池时触发同步加载）
            if (goPool.TryGet(out GameObject cacheObj))
            {
                return cacheObj;
            }
            //获取异步加载任务唯一id
            long guid = mAsyncTaskGuid;
            mAsyncLoadingTaskList.Add(guid);
            //开始异步加载资源（此时不实例化）
            GameObject prefab = await LoadResourceAsync<GameObject>(path);
            if (prefab != null)
            {
                if (mAsyncLoadingTaskList.Contains(guid))
                {
                    mAsyncLoadingTaskList.Remove(guid);
                    //资源已缓存，Get内部createFunc为同步取缓存并克隆，无额外IO开销
                    GameObject nObj = goPool.Get();
                    return nObj;
                }
                Debug.LogError("Async Load GameObject Command be remover:" + path);
                return null;
            }
            mAsyncLoadingTaskList.Remove(guid);
            return null;
        }

        /// <summary>
        /// 克隆并且等待资源下载完成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<GameObject> InstantiateAndLoadAsync(string path)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            uint crc = Crc32.GetCrc32(path);
            //先从对象池查找对象（不扩展，避免空池时触发同步加载）
            if (GetOrCreateGoPool(crc, path).TryGet(out GameObject cacheObj))
            {
                return cacheObj;
            }
            GameObject obj = await InstantiateAsync(path);
            if (obj != null)
            {
                return obj;
            }
            else
            {
                try
                {
                    //如果已经有TCS在等待了，直接复用即可
                    if (mWaitDownloadTcs.TryGetValue(crc, out var exixtingTcs))
                    {
                        await exixtingTcs.Task;
                    }
                    else
                    {
                        var tcs = new UniTaskCompletionSource();
                        mWaitDownloadTcs.Add(crc, tcs);
                        await tcs.Task;
                        mWaitDownloadTcs.Remove(crc);
                    }
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
            obj = await InstantiateAsync(path);
            if (obj != null)
                return obj;

            Debug.LogError("Asset Load Failed is null，path:" + path);
            return null;
        }
        /// <summary>
        /// 获取或创建按资源crc划分的GameObject克隆池（由PoolManager统一管理）
        /// 池的创建/取出/归还/销毁回调绑定了ResourcesManager的完整记账逻辑
        /// </summary>
        /// <param name="crc">资源路径crc</param>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        private IObjectPool<GameObject> GetOrCreateGoPool(uint crc, string path)
        {
            return PoolManager.Instance.GetOrCreateGameObjectPool(crc, () => new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject prefab = LoadResource<GameObject>(path);
                    //资源加载失败返回null，由ObjectPool.Get判空直接返回null不入池
                    if (prefab == null)
                        return null;
                    GameObject go = InstantiateClone(path, prefab, null);
                    go.SetActive(false);
                    return go;
                },
                onGet: go => go.SetActive(true),
                //返回对象池后重置参数
                onReturn: go =>
                {
                    go.SetActive(false);
                    go.transform.SetParent(MJAssetsABFrame.RecyclObjRoot);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                },
                onDestroy: OnPooledGameObjectDestroyed,
                config: PoolManager.Instance.NewGameObjectPoolConfig()));
        }

        /// <summary>
        /// 池内对象被销毁时的统一记账出口
        /// 自动回收/池溢出/Kill/Clear等所有销毁路径都经此方法：
        /// 移除对象字典记录、递减活跃实例计数（归零时卸载AB包）、回收CacheObject包装类
        /// </summary>
        /// <param name="go"></param>
        private void OnPooledGameObjectDestroyed(GameObject go)
        {
            if (go == null) return;

            int insid = go.GetInstanceID();
            if (mAllObjectDic.TryGetValue(insid, out CacheObject cacheObject))
            {
                mAllObjectDic.Remove(insid);
                DecreaseInstanceCount(cacheObject.crc);
                cacheObject.Release();
                CacheObjPool.Return(cacheObject);
            }
            GameObject.Destroy(go);
        }

        /// <summary>
        /// 递减资源路径的活跃实例计数，归零时卸载对应AssetBundle
        /// </summary>
        /// <param name="crc">资源路径crc</param>
        private void DecreaseInstanceCount(uint crc)
        {
            if (!mActiveInstanceCountDic.TryGetValue(crc, out int count))
                return;

            count--;
            if (count <= 0)
            {
                mActiveInstanceCountDic.Remove(crc);
                BundleItem item;
                //卸载ab包并移除资源缓存（缓存已随AB释放失效）
                if (mAlreayLoadAssetsDic.TryGetValue(crc, out item))
                {
                    mAlreayLoadAssetsDic.Remove(crc);
                    AssetBundleManager.Instance.ReleaseAssets(item, true);
                }
                else
                {
                    Debug.LogError("mAlreayLoadAssetsDic not find BundleItem crc:" + crc);
                }
            }
            else
            {
                mActiveInstanceCountDic[crc] = count;
            }
        }

        /// <summary>
        /// 预加载对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="count"></param>
        public void PreLoadObj(string path, int count = 1)
        {
            List<GameObject> preLaodObjList = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                preLaodObjList.Add(Instantiate(path));
            }
            //回收到对象池
            foreach (var obj in preLaodObjList)
            {
                Release(obj);
            }
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        public void PreLoadResource<T>(string path) where T : UnityEngine.Object
        {
            LoadResource<T>(path);
        }

        #endregion
        #region 资源加载

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
            if (!mAlreayLoadAssetsDic.ContainsKey(crc))
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
#endif

        #endregion

        /// <summary>
        /// 移除对象加载TCS
        /// </summary>
        /// <param name="crc">资源路径path 转crc</param>
        public void RemoveObjectLoadTCS(uint crc = 0)
        {
            if (crc == 0)
                return;

            if (mWaitDownloadTcs.ContainsKey(crc))
            {
                mWaitDownloadTcs[crc].TrySetCanceled();
                mWaitDownloadTcs.Remove(crc);
            }

        }

        /// <summary>
        /// 释放对象占用内存
        /// 回收到对象池
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="destroy"></param>
        public void Release(GameObject obj, bool destroy = false)
        {
            int insid = obj.GetInstanceID();

            //通过其他方式创建的 不支持回收
            if (!mAllObjectDic.TryGetValue(insid, out CacheObject cacheObject))
            {
                Debug.LogError("Recycl Obj failed,obj is GameObject.Instantiate Create");
                return;
            }

            IObjectPool<GameObject> goPool = GetOrCreateGoPool(cacheObject.crc, cacheObject.path);
            if (destroy)
            {
                //彻底销毁并联动记账（内部走OnPooledGameObjectDestroyed：销毁、递减计数、归零卸载AB）
                goPool.Kill(obj);
            }
            else
            {
                //回收到对象池（onReturn内挂RecyclObjRoot并隐藏）
                if (cacheObject.obj == null)
                {
                    Debug.LogError("CacheObject.obj is null Releas Failed!");
                    return;
                }
                goPool.Return(obj);
            }
        }

        /// <summary>
        /// 释放图片资源所占用的内存
        /// </summary>
        /// <param name="texture"></param>
        public void Release(Texture texture)
        {
            Resources.UnloadAsset(texture);
        }

        /// <summary>
        /// 加载图片资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Sprite LoadSprite(string path)
        {
            if (path.EndsWith(".png") == false) path += ".png";
            return LoadResource<Sprite>(path);
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Texture LoadTexture(string path)
        {
            if (path.EndsWith(".jpg") == false) path += ".jpg";
            return LoadResource<Texture>(path);
        }

        /// <summary>
        /// 加载音频文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public AudioClip LoadAudio(string path)
        {
            return LoadResource<AudioClip>(path);
        }

        /// <summary>
        /// 加载Text资源文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public TextAsset LoadTextAsset(string path)
        {
            return LoadResource<TextAsset>(path);
        }

        /// <summary>
        /// 加载图集中的图片
        /// </summary>
        /// <param name="atlasPath"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite LoadAtlasSprite(string atlasPath, string spriteName)
        {
            if (atlasPath.EndsWith(".spriteatlas") == false) atlasPath += "spriteatlas";

            return LoadSpriteFormAltas(LoadResource<SpriteAtlas>(atlasPath), spriteName);
        }

        /// <summary>
        /// 从图集中加载指定名称的图片
        /// </summary>
        /// <param name="spriteAtlas"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        private Sprite LoadSpriteFormAltas(SpriteAtlas spriteAtlas, string spriteName)
        {
            if (spriteAtlas == null)
            {
                Debug.LogError("Not find spriteAtlas Name:" + spriteAtlas);
                return null;
            }
            //从图集中获取指定名称的图片
            Sprite sprite = spriteAtlas.GetSprite(spriteName);
            if (sprite != null)
                return sprite;
            else
            {
                Debug.LogError("Not find sprite name:" + spriteName);
                return null;
            }
        }

        /// <summary>
        /// 异步加载图片资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<Texture> LoadTextureAsync(string path)
        {
            if (path.EndsWith(".jpg") == false) path += ".jpg";

            Texture texture = await LoadResourceAsync<Texture>(path);
            if (texture != null)
                return texture;
            Debug.LogError("Async load texture Error!Path:" + path);
            return null;
        }

        /// <summary>
        /// 异步加载sprite
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="image"></param>
        /// <param name="setNativeSize"></param>
        /// <returns></returns>
        public async UniTask<Sprite> LoadSpriteAsync(string path, Image image = null, bool setNativeSize = false)
        {
            if (path.EndsWith(".jpg") == false) path += ".jpg";

            Sprite sprite = await LoadResourceAsync<Sprite>(path);
            if (sprite != null)
            {
                if (image != null)
                {
                    image.sprite = sprite;
                    if (setNativeSize)
                        image.SetNativeSize();
                }
                return sprite;
            }
            Debug.LogError("Async load sprite Error!Path:" + path);
            return null;
        }

        /// <summary>
        /// 清理所有异步加载任务
        /// </summary>
        public void ClearAllAsyncLoadTask()
        {
            mAsyncLoadingTaskList.Clear();

        }
        /// <summary>
        /// 清理加载的资源释放内存
        /// </summary>
        /// <param name="absoluteCleaning">为true时 销毁所有由assetBundle加载和生成的对象，彻底释放内存占用。
        /// 为false时 销毁对象池中的对象，但不销毁由AssetBundle克隆并在使用的对象</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ClearResoucesAssets(bool absoluteCleaning)
        {
            ////absoluteCleaning为true 销毁所有由assetbundle加载和生成的对象，彻底释放内存占用
            ////为false 只销毁对象池中的闲置对象，不销毁由AssetBundle克隆并在使用的对象
            ////所有销毁路径统一经池的onDestroy回调（OnPooledGameObjectDestroyed）完成记账联动
            //PoolManager.Instance.ClearAllGameObjectPools(absoluteCleaning);

            ////true：所有克隆（含在用）已销毁，防御式兜底清理残余记账
            ////false：在用对象的记账必须保留，否则后续无法Release、AB无法卸载
            //if (absoluteCleaning)
            //{
            //    foreach (var item in mAllObjectDic.Values)
            //    {
            //        item.Release();
            //    }
            //    //清理列表
            //    mAllObjectDic.Clear();
            //    mActiveInstanceCountDic.Clear();
            //    ClearAllAsyncLoadTask();
            //}

            ////释放AssetBundle 及里面资源所占用的内存
            //foreach (var item in mAlreayLoadAssetsDic)
            //    AssetBundleManager.Instance.ReleaseAssets(item.Value, absoluteCleaning);
            //// 取消所有等待下载完成的任务
            //CancleAllWaiteDownLoadTcs();
            //mAlreayLoadAssetsDic.Clear();
            ////释放未被引用的资源
            //Resources.UnloadUnusedAssets();
            ////触发垃圾回收
            //System.GC.Collect();
            PoolManager.Instance.ClearAllGameObjectPools(absoluteCleaning);

            if (absoluteCleaning)
            {
                // 所有 GameObject 都销毁
                foreach (var item in mAllObjectDic.Values)
                {
                    item.Release();
                }

                mAllObjectDic.Clear();
                mActiveInstanceCountDic.Clear();

                foreach (var item in mAlreayLoadAssetsDic)
                {
                    AssetBundleManager.Instance.ReleaseAssets(item.Value, true);
                }

                mAlreayLoadAssetsDic.Clear();

                ClearAllAsyncLoadTask();
            }

            CancleAllWaiteDownLoadTcs();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        /// <summary>
        /// 取消所有等待下载完成的任务
        /// </summary>
        private void CancleAllWaiteDownLoadTcs()
        {
            if (mWaitDownloadTcs.Count <= 0)
                return;
            foreach (var tcs in mWaitDownloadTcs.Values)
            {
                tcs.TrySetCanceled();
            }
            mWaitDownloadTcs.Clear();
        }
    }

}