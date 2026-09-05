using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using static UnityEditor.Progress;

namespace MJ.AssetFrameWork.ABFrame.Pool
{
    /// <summary>
    /// 对象池管理器
    /// 统一创建和管理所有对象池：类对象池（AssetBundleCache/CacheObject等包装类）与按资源crc划分的GameObject克隆池
    /// 每帧驱动各池的闲置对象自动回收
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager instance;
        public static PoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject pool = new GameObject();
                    pool.name = "PoolManager";
                    instance = pool.AddComponent<PoolManager>();
                    DontDestroyOnLoad(pool);
                    return instance;
                }
                return instance;
            }
        }

        //按资源crc划分的GameObject克隆池
        private readonly Dictionary<uint, IObjectPool<GameObject>> _poolGameObjects = new Dictionary<uint, IObjectPool<GameObject>>();

        //类对象池注册表（AssetBundleCache/CacheObject等纯C#包装类池）
        private readonly Dictionary<Type, IPoolStats> _classPools = new Dictionary<Type, IPoolStats>();

        /// <summary>
        /// 预加载200个 不自动回收 
        /// </summary>
        /// <returns></returns>
        public PoolConfig NewClassPoolConfig()
        {
            return new PoolConfig { InitialSize = 200, MaxSize = -1, PreloadOnStart = true, PreloadCount = 200, AutoRecycleTime = 0f };
        }

        /// <summary>
        /// GameObject克隆池默认配置：容量10 懒创建 闲置120秒自动回收
        /// </summary>
        /// <returns></returns>
        public PoolConfig NewGameObjectPoolConfig()
        {
            return new PoolConfig { MaxSize = -1, PreloadOnStart = false, AutoRecycleTime = 120f };
        }

        public void InitInfo()
        {
            //池配置以 NewClassPoolConfig/NewGameObjectPoolConfig 提供的默认值为准，在各池创建时分配，此处无需额外初始化
        }

        private void Update()
        {
            // 更新所有池的自动回收
            foreach (var pool in _poolGameObjects.Values)
            {
                pool?.UpdateAutoRecycle();
            }

            foreach (var pool in _classPools.Values)
            {
                pool?.UpdateAutoRecycle();
            }
        }

        /// <summary>
        /// 获取或创建一个类对象池（AssetBundleCache/CacheObject等纯C#包装类）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="createFunc">对象创建方法</param>
        /// <param name="onGet">取出对象时回调</param>
        /// <param name="onReturn">归还对象时回调</param>
        /// <param name="onDestroy">池销毁对象时回调</param>
        /// <param name="config">池配置，为空时使用类对象池默认配置</param>
        /// <returns></returns>
        public IObjectPool<T> GetOrCreateClassPool<T>(Func<T> createFunc, Action<T> onGet = null,
            Action<T> onReturn = null, Action<T> onDestroy = null, PoolConfig config = null) where T : class
        {
            if (_classPools.TryGetValue(typeof(T), out var pooled) && pooled is IObjectPool<T> pool)
            {
                return pool;
            }

            pool = new ObjectPool<T>(createFunc, onGet, onReturn, onDestroy, config: config ?? NewClassPoolConfig());
            _classPools[typeof(T)] = pool;
            return pool;
        }

        /// <summary>
        /// 创建一个AB包缓存类对象池管理（GetOrCreateClassPool的便捷别名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public IObjectPool<T> CreateAbPool<T>() where T : class, new()
        {
            return GetOrCreateClassPool(() => new T(), config: NewClassPoolConfig());
        }

        /// <summary>
        /// 查询按资源crc划分的GameObject克隆池（未创建返回null）
        /// </summary>
        /// <param name="crc">资源路径crc</param>
        /// <returns></returns>
        public IObjectPool<GameObject> GetGameObjectPool(uint crc)
        {
            return _poolGameObjects.TryGetValue(crc, out var pool) ? pool : null;
        }

        /// <summary>
        /// 获取或创建按资源crc划分的GameObject克隆池
        /// 工厂由调用方提供，用于绑定各自的创建与记账回调
        /// </summary>
        /// <param name="crc">资源路径crc</param>
        /// <param name="factory">池工厂方法</param>
        /// <returns></returns>
        public IObjectPool<GameObject> GetOrCreateGameObjectPool(uint crc, Func<IObjectPool<GameObject>> factory)
        {
            if (_poolGameObjects.TryGetValue(crc, out var pool))
                return pool;

            pool = factory();
            _poolGameObjects.Add(crc, pool);
            return pool;
        }

        /// <summary>
        /// 移除并清理指定crc的GameObject克隆池
        /// </summary>
        /// <param name="crc">资源路径crc</param>
        public void RemoveGameObjectPool(uint crc)
        {
            if (_poolGameObjects.TryGetValue(crc, out var pool))
            {
                pool.Clear();
                _poolGameObjects.Remove(crc);
            }
        }

        /// <summary>
        /// 清理所有GameObject克隆池
        /// </summary>
        /// <param name="includeActive">为true时 销毁所有对象（含在用对象）；为false时 只销毁池内闲置对象</param>
        public void ClearAllGameObjectPools(bool includeActive)
        {
            foreach (var pool in _poolGameObjects.Values)
            {
                if (includeActive)
                    pool.Clear();
                else
                    pool.ClearInactive();
            }
        }

        /// <summary>
        /// 获取所有池的统计信息（调试用）
        /// </summary>
        /// <returns></returns>
        public List<PoolStats> GetAllStats()
        {
            //List<PoolStats> statsList = new List<PoolStats>();
            //foreach (var pool in _poolGameObjects.Values)
            //    statsList.Add(pool.GetStats());
            //foreach (var pool in _classPools.Values)
            //    statsList.Add(pool.GetStats());

            List<PoolStats> statsList = new List<PoolStats>();
            foreach (var pool in _poolGameObjects.Values)
            {
                statsList.Add(pool.GetStats());
                Debug.Log("poolGameObject:" + pool.GetStats().ToString());
            }
            foreach (var pool in _classPools.Values)
            {
                statsList.Add(pool.GetStats());
                Debug.Log("classPools:" + pool.GetStats().ToString());
            }

            return statsList;
        }
    }
}
