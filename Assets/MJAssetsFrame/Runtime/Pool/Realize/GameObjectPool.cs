using System;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame.Pool
{
    /// <summary>
    /// GameObject 对象池
    /// </summary>
    public class GameObjectPool : IObjectPool<GameObject>
    {
        //内部对象池
        //真正负责逻辑处理
        private readonly ObjectPool<GameObject> _internalPool;
        private readonly Transform _parent;
        private readonly GameObject _prefab;
        private readonly PoolConfig _config;
        /// <summary>
        /// 初始化创建 gameobject对象池
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="config"></param>
        public GameObjectPool(GameObject prefab, Transform parent = null, PoolConfig config = null)
        {
            _prefab = prefab;
            _parent = parent;
            _config = config;

            _internalPool = new ObjectPool<GameObject>(
                createFunc: CreateGameObject,
                onGet: OnGetGameObject,
                onReturn: OnReturnGameObject,
                onDestroy: OnDestroyGameObject,
                createPool: CreatePool,
                config: config
            );
        }

        public GameObject Get()
        {
            return _internalPool.Get();
        }
        public bool TryGet(out GameObject obj)
        {
            return _internalPool.TryGet(out obj);
        }
        public void Return(GameObject obj)
        {
            _internalPool.Return(obj);
        }
        public void Kill(GameObject obj)
        {
            _internalPool.Kill(obj);
        }
        public void Preload(int count)
        {
            _internalPool.Preload(count);
        }
        public void Clear()
        {
            _internalPool.Clear();

        }
        public void ClearInactive()
        {
            _internalPool.ClearInactive();
        }
        public void UpdateAutoRecycle()
        {
            _internalPool.UpdateAutoRecycle();
        }
        public int InactiveCount => _internalPool.InactiveCount;
        public PoolStats GetStats()
        {
            return _internalPool.GetStats();
        }

        #region 私有方法

        private GameObject CreateGameObject()
        {
            var go = GameObject.Instantiate(_prefab, _parent);
            go.name = _prefab.name + " (Pooled)";
            go.SetActive(false);
            return go;
        }

        private void OnGetGameObject(GameObject go)
        {
            go.SetActive(true);
            // 通知所有池化组件
            var poolables = go.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnSpawn();
            }
        }

        private void OnReturnGameObject(GameObject go)
        {
            go.SetActive(false);

            // 重置位置和父级
            if (_parent != null)
            {
                go.transform.SetParent(_parent);
            }

            // 重置缩放和位置
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            // 通知所有池化组件
            var poolables = go.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnDespawn();
            }
        }
        private void OnDestroyGameObject(GameObject go)
        {
            GameObject.Destroy(go);
        }

        public IObjectPool<GameObject> CreatePool()
        {
            return new GameObjectPool(_prefab, _parent, _config);
        }

        #endregion
    }
}

