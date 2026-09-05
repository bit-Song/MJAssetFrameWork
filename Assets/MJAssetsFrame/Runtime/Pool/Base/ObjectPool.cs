using System.Collections.Generic;
using System;

using UnityEngine;

/// <summary>
/// 基础对象池
/// </summary>
///
namespace MJ.AssetFrameWork.ABFrame.Pool
{
    public class ObjectPool<T> : IObjectPool<T> where T : class
    {
        //非激活的对象
        private readonly Stack<T> _inactiveObjects = new Stack<T>();
        //激活对象
        private readonly HashSet<T> _activeObjects = new HashSet<T>();
        //最后的使用时间
        private readonly Dictionary<T, float> _lastUseTime = new Dictionary<T, float>();
        //当创建时调用
        private readonly Func<T> _createFunc;
        //取出
        private readonly Action<T> _onGet;
        //进入
        private readonly Action<T> _onReturn;
        //移除
        private readonly Action<T> _onDestroy;
        //创建对象池的方法
        private readonly Func<IObjectPool<T>> _createPool;
        //池配置文件
        private readonly PoolConfig _config;
        private bool _isInitialized = false;
        private int TotalCount => _activeObjects.Count + _inactiveObjects.Count;
        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onReturn = null,
            Action<T> onDestroy = null,
            Func<IObjectPool<T>> createPool = null,
            PoolConfig config = null)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onReturn = onReturn;
            _onDestroy = onDestroy;
            _config = config ?? new PoolConfig();
            _createPool = createPool;
            //如果启动了预加载 就提前生成多个对象
            if (_config.PreloadOnStart)
            {
                Preload(_config.PreloadCount);
            }

            _isInitialized = true;
        }


        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get()
        {
            T obj;
            //先检测 是否有可用对象
            if (_inactiveObjects.Count > 0)
            {
                obj = _inactiveObjects.Pop();
            }//检测是否可以自动扩展    或者   当前数量小于设置的最大容器值
            else if (_config.AllowExpand || (_config.MaxSize == 0 || TotalCount < _config.MaxSize))
            {
                obj = CreateNewObject();
                //创建失败（如资源加载失败）不入池 直接返回null
                if (obj == null)
                {
                    Debug.LogWarning("对象池创建新对象失败，createFunc返回null");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning($"对象池已满，无法创建新对象。最大大小: {_config.MaxSize}");
                return null;
            }

            _activeObjects.Add(obj);
            _lastUseTime[obj] = Time.time;

            _onGet?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// 将对象返回池中
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            if (!_activeObjects.Contains(obj))
            {
                Debug.LogWarning("尝试回收不属于该池的对象");
                return;
            }

            _activeObjects.Remove(obj);

            // 检查池大小限制
            if (_config.MaxSize > 0 && _inactiveObjects.Count >= _config.MaxSize)
            {
                _lastUseTime.Remove(obj);
                _onDestroy?.Invoke(obj);
                return;
            }

            _inactiveObjects.Push(obj);
            _lastUseTime[obj] = Time.time;

            _onReturn?.Invoke(obj);
        }

        /// <summary>
        /// 尝试从池中获取对象（池空时不创建新对象）
        /// 有闲置对象时按完整Get语义取出并返回true，否则返回false
        /// </summary>
        public bool TryGet(out T obj)
        {
            if (_inactiveObjects.Count > 0)
            {
                obj = Get();
                return true;
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// 从池中强制移除指定对象并销毁（不参与回收，直接走销毁回调）
        /// 供外部显式销毁池所管理的对象时使用
        /// </summary>
        public void Kill(T obj)
        {
            if (obj == null) return;

            //活跃对象 直接销毁
            if (_activeObjects.Remove(obj))
            {
                _lastUseTime.Remove(obj);
                _onDestroy?.Invoke(obj);
                return;
            }
            //闲置对象（理论上外部不应持有，防御式处理）
            if (_inactiveObjects.RemoveFromStack(obj))
            {
                _lastUseTime.Remove(obj);
                _onDestroy?.Invoke(obj);
                return;
            }
            Debug.LogWarning("Kill失败，该对象不属于此池");
        }

        /// <summary>
        /// 预加载对象
        /// </summary>
        public void Preload(int count)
        {
            count = Math.Min(count, _config.MaxSize > 0 ? _config.MaxSize : int.MaxValue);

            for (int i = 0; i < count; i++)
            {
                if (_config.MaxSize > 0 && TotalCount >= _config.MaxSize) break;

                var obj = CreateNewObject();
                //创建失败不入池 防止null污染池
                if (obj == null) break;
                _inactiveObjects.Push(obj);
                _lastUseTime[obj] = Time.time;
            }
        }

        /// <summary>
        /// 清理池
        /// </summary>
        public void Clear()
        {
            // 清理闲置对象
            while (_inactiveObjects.Count > 0)
            {
                var obj = _inactiveObjects.Pop();
                _onDestroy?.Invoke(obj);
            }

            // 清理活跃对象（警告）
            if (_activeObjects.Count > 0)
            {
                foreach (var item in _activeObjects)
                {
                    _onDestroy?.Invoke(item);
                }
                Debug.LogWarning($"清理对象池时仍有 {_activeObjects.Count} 个活跃对象");
                _activeObjects.Clear();
            }
            _lastUseTime.Clear();
        }

        /// <summary>
        /// 只销毁池内全部闲置对象（不动活跃对象）
        /// </summary>
        public void ClearInactive()
        {
            while (_inactiveObjects.Count > 0)
            {
                var obj = _inactiveObjects.Pop();
                _lastUseTime.Remove(obj);
                _onDestroy?.Invoke(obj);
            }
        }

        /// <summary>
        /// 获取池统计信息
        /// </summary>
        public PoolStats GetStats()
        {
            return new PoolStats
            {
                TotalCount = TotalCount,
                ActiveCount = _activeObjects.Count,
                InactiveCount = _inactiveObjects.Count,
                PoolConfig = _config
            };
        }

        /// <summary>
        /// 更新自动回收
        /// </summary>
        public void UpdateAutoRecycle()
        {
            if (_config.AutoRecycleTime <= 0) return;

            var currentTime = Time.time;
            var objectsToRecycle = new List<T>();

            // 检查闲置对象是否需要回收
            foreach (var obj in _inactiveObjects)
            {
                if (currentTime - _lastUseTime[obj] > _config.AutoRecycleTime)
                {
                    objectsToRecycle.Add(obj);
                }
            }

            // 回收超时的对象
            foreach (var obj in objectsToRecycle)
            {
                _inactiveObjects.RemoveFromStack(obj); // 扩展方法
                _lastUseTime.Remove(obj);
                _onDestroy?.Invoke(obj);
            }
        }
        public IObjectPool<T> CreatePool()
        {
            return _createPool?.Invoke();
        }

        /// <summary>
        /// 池内闲置对象数量
        /// </summary>
        public int InactiveCount => _inactiveObjects.Count;
        #region 私有方法

        private T CreateNewObject()
        {
            var obj = _createFunc();
            _lastUseTime[obj] = Time.time;
            return obj;
        }


        #endregion
    }
}