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

        //对象池字典
        private Dictionary<uint, List<CacheObject>> mObjectPoolDic = new Dictionary<uint, List<CacheObject>>();

        //所有对象字典
        private Dictionary<int, CacheObject> mAllObjectDic = new Dictionary<int, CacheObject>();
        //缓存对象类对象池
        private ClassObjectPool<CacheObject> mCacheObjectPool = new ClassObjectPool<CacheObject>(200);


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
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            //先从对象池查找对象
            GameObject chacheObj = GetCacheObjectFromPools(Crc32.GetCrc32(path));
            if (chacheObj != null)
            {
                chacheObj.transform.SetParent(null);
                chacheObj.transform.localPosition = Vector3.zero;
                chacheObj.transform.localScale = Vector3.one;
                chacheObj.transform.rotation = Quaternion.identity;
                return chacheObj;
            }
            else
            {
                //加载该对象
                GameObject obj = LoadResource<GameObject>(path);
                if (obj != null)
                {
                    GameObject nObj = Instantiate(path, obj, null);
                    nObj.transform.localPosition = Vector3.zero;
                    nObj.transform.localScale = Vector3.one;
                    nObj.transform.rotation = Quaternion.identity;
                    return nObj;
                }

            }
            Debug.LogError("GameObject Load failed ,path is null ...");
            return null;
        }
        public GameObject Instantiate(string path, Transform parent)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            //先从对象池查找对象
            GameObject chacheObj = GetCacheObjectFromPools(Crc32.GetCrc32(path));
            if (chacheObj != null)
            {
                chacheObj.transform.SetParent(parent);
                chacheObj.transform.localPosition = Vector3.zero;
                chacheObj.transform.localScale = Vector3.one;
                chacheObj.transform.rotation = Quaternion.identity;
                return chacheObj;
            }
            else
            {
                //加载该对象
                GameObject obj = LoadResource<GameObject>(path);
                if (obj != null)
                {
                    GameObject nObj = Instantiate(path, obj, parent);
                    nObj.transform.localPosition = Vector3.zero;
                    nObj.transform.localScale = Vector3.one;
                    nObj.transform.rotation = Quaternion.identity;
                    return nObj;
                }

            }
            Debug.LogError("GameObject Load failed ,path is null ...");
            return null;
        }
        public GameObject Instantiate(string path, Transform parent, Vector3 localPosition)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            //先从对象池查找对象
            GameObject chacheObj = GetCacheObjectFromPools(Crc32.GetCrc32(path));
            if (chacheObj != null)
            {
                chacheObj.transform.SetParent(parent);
                chacheObj.transform.localPosition = localPosition;
                chacheObj.transform.localScale = Vector3.one;
                chacheObj.transform.rotation = Quaternion.identity;
                return chacheObj;
            }
            else
            {
                //加载该对象
                GameObject obj = LoadResource<GameObject>(path);
                if (obj != null)
                {
                    GameObject nObj = Instantiate(path, obj, parent);
                    nObj.transform.localPosition = localPosition;
                    nObj.transform.localScale = Vector3.one;
                    nObj.transform.rotation = Quaternion.identity;
                    return nObj;
                }

            }
            Debug.LogError("GameObject Load failed ,path is null ...");
            return null;
        }
        public GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            //先从对象池查找对象
            GameObject chacheObj = GetCacheObjectFromPools(Crc32.GetCrc32(path));
            if (chacheObj != null)
            {
                chacheObj.transform.SetParent(parent);
                chacheObj.transform.localPosition = localPosition;
                chacheObj.transform.localScale = localScale;
                chacheObj.transform.rotation = Quaternion.identity;
                return chacheObj;
            }
            else
            {
                //加载该对象
                GameObject obj = LoadResource<GameObject>(path);
                if (obj != null)
                {
                    GameObject nObj = Instantiate(path, obj, parent);
                    nObj.transform.localPosition = localPosition;
                    nObj.transform.localScale = localScale;
                    nObj.transform.rotation = Quaternion.identity;
                    return nObj;
                }

            }
            Debug.LogError("GameObject Load failed ,path is null ...");
            return null;
        }
        public GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion quateraion)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            //先从对象池查找对象
            GameObject chacheObj = GetCacheObjectFromPools(Crc32.GetCrc32(path));
            if (chacheObj != null)
            {
                chacheObj.transform.SetParent(parent);
                chacheObj.transform.localPosition = localPosition;
                chacheObj.transform.localScale = localScale;
                chacheObj.transform.rotation = quateraion;
                return chacheObj;
            }
            else
            {
                //加载该对象
                GameObject obj = LoadResource<GameObject>(path);
                if (obj != null)
                {
                    GameObject nObj = Instantiate(path, obj, parent);
                    nObj.transform.localPosition = localPosition;
                    nObj.transform.localScale = localScale;
                    nObj.transform.rotation = quateraion;
                    return nObj;
                }

            }
            Debug.LogError("GameObject Load failed ,path is null ...");
            return null;
        }
        #endregion
        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private GameObject Instantiate(string path, GameObject obj, Transform parent)
        {
            obj = GameObject.Instantiate(obj, parent, false);
            CacheObject cacheObject = mCacheObjectPool.Spawn();
            cacheObject.obj = obj;
            cacheObject.path = path;
            cacheObject.crc = Crc32.GetCrc32(path);
            cacheObject.insid = obj.GetInstanceID();
            mAllObjectDic.Add(cacheObject.insid, cacheObject);
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
            //先从对象池查找对象
            GameObject cacheObj = GetCacheObjectFromPools(Crc32.GetCrc32(path));
            if (cacheObj != null)
            {
                return cacheObj;
            }
            //获取异步加载任务唯一id
            long guid = mAsyncTaskGuid;
            mAsyncLoadingTaskList.Add(guid);
            //开始异步加载
            GameObject obj = await LoadResourceAsync<GameObject>(path);
            if (obj != null)
            {
                if (mAsyncLoadingTaskList.Contains(guid))
                {
                    mAsyncLoadingTaskList.Remove(guid);
                    GameObject nObj = Instantiate(path, obj, null);
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
            //先从对象池查找对象
            GameObject cacheObj = GetCacheObjectFromPools(crc);
            if (cacheObj != null)
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
        /// 从对象池中获取对象
        /// </summary>
        /// <returns></returns>
        private GameObject GetCacheObjectFromPools(uint crc)
        {
            List<CacheObject> objList = null;
            mObjectPoolDic.TryGetValue(crc, out objList);

            if (objList != null && objList.Count > 0)
            {
                //直接取出对象池中第0个
                CacheObject obj = objList[0];
                objList.RemoveAt(0);
                return obj.obj;
            }
            return null;
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
            CacheObject cacheObject = null;
            int insid = obj.GetInstanceID();

            mAllObjectDic.TryGetValue(insid, out cacheObject);
            //通过其他方式创建的 不支持回收
            if (cacheObject == null)
            {
                Debug.LogError("Recycl Obj failed,obj is GameObject.Instantiate Create");
                return;
            }
            if (destroy)
            {
                GameObject.Destroy(obj);
                if (mAllObjectDic.ContainsKey(insid))
                    mAllObjectDic.Remove(insid);
                //获取该物体所在对象池
                List<CacheObject> objectPoolList = null;
                //得到该对象所在的对象池
                mObjectPoolDic.TryGetValue(cacheObject.crc, out objectPoolList);
                if (objectPoolList != null)
                {
                    //从对象池中移除缓存对象
                    if (objectPoolList.Contains(cacheObject))
                    {
                        objectPoolList.Remove(cacheObject);
                    }
                    cacheObject.Release();
                    //回收
                    mCacheObjectPool.Recycl(cacheObject);
                }
                //如果该对象在对象池中不存在，或者已经全部释放了，就卸载该对象AssetBundle的资源占用
                if (objectPoolList == null || objectPoolList.Count == 0)
                {
                    BundleItem item;
                    if (mAlreayLoadAssetsDic.TryGetValue(cacheObject.crc, out item))
                    {
                        AssetBundleManager.Instance.ReleaseAssets(item, true);
                    }
                    else
                    {
                        Debug.LogError("mAlreayLoadAssetsDic not find BundleItem Path:" + cacheObject.path);
                    }
                }
            }
            else
            {
                //回收到对象池
                List<CacheObject> objList = null;
                mObjectPoolDic.TryGetValue(cacheObject.crc, out objList);
                //字典中没有该对象池
                if (objList == null)
                {
                    //创建对象池
                    objList = new List<CacheObject>();
                    objList.Add(cacheObject);
                    mObjectPoolDic.Add(cacheObject.crc, objList);
                }
                else
                {
                    //回收到对象池
                    objList.Add(cacheObject);
                }

                //回收到对象回收节点下
                if (cacheObject.obj != null)
                {
                    cacheObject.obj.transform.SetParent(MJAssetsABFrame.RecyclObjRoot);
                }
                else
                {
                    Debug.LogError("CacheObject.obj is null Releas Failed!");
                }
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
            if (absoluteCleaning)
            {
                foreach (var item in mAllObjectDic)
                {
                    if (item.Value.obj != null)
                    {
                        //销毁Object对象 回收缓存类对象，等待下次复用
                        GameObject.Destroy(item.Value.obj);
                        item.Value.Release();
                        mCacheObjectPool.Recycl(item.Value);
                    }
                }
                //清理列表
                mAllObjectDic.Clear();
                mObjectPoolDic.Clear();
                ClearAllAsyncLoadTask();
            }
            else
            {
                foreach (var objList in mObjectPoolDic.Values)
                {
                    if (objList != null)
                    {
                        foreach (var cacheObj in objList)
                        {
                            if (cacheObj != null)
                            {

                                GameObject.Destroy(cacheObj.obj);
                                cacheObj.Release();
                                mCacheObjectPool.Recycl(cacheObj);
                            }
                        }
                    }
                }
                mObjectPoolDic.Clear();
            }

            //释放AssetBundle 及里面资源所占用的内存
            foreach (var item in mAlreayLoadAssetsDic)
                AssetBundleManager.Instance.ReleaseAssets(item.Value, absoluteCleaning);
            // 取消所有等待下载完成的任务
            CancleAllWaiteDownLoadTcs();
            mAlreayLoadAssetsDic.Clear();
            //释放未被引用的资源
            Resources.UnloadUnusedAssets();
            //触发垃圾回收
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