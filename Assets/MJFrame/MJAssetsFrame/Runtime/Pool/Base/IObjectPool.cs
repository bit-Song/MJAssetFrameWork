namespace MJ.AssetFrameWork.ABFrame.Pool
{
    /// <summary>
    /// 非泛型池信息接口，供 PoolManager 不依赖泛型参数统一遍历管理（自动回收、统计、清理）
    /// </summary>
    public interface IPoolStats
    {
        /// <summary>
        /// 更新闲置对象自动回收（由 PoolManager 每帧驱动）
        /// </summary>
        void UpdateAutoRecycle();

        /// <summary>
        /// 获取池统计信息
        /// </summary>
        PoolStats GetStats();

        /// <summary>
        /// 清理池（销毁闲置与活跃对象）
        /// </summary>
        void Clear();

        /// <summary>
        /// 只销毁池内闲置对象（不动活跃对象）
        /// </summary>
        void ClearInactive();

        /// <summary>
        /// 池内闲置对象数量
        /// </summary>
        int InactiveCount { get; }
    }

    /// <summary>
    /// 对象池接口
    /// </summary>
    public interface IObjectPool<T> : IPoolStats where T : class
    {
        /// <summary>
        /// 从池中获取对象（池空时自动创建）
        /// </summary>
        T Get();

        /// <summary>
        /// 尝试从池中获取对象（池空时不创建新对象，返回false）
        /// </summary>
        bool TryGet(out T obj);

        /// <summary>
        /// 将对象返回池中
        /// </summary>
        void Return(T obj);

        /// <summary>
        /// 从池中强制移除指定对象并销毁（不参与回收，直接走销毁回调）
        /// </summary>
        void Kill(T obj);

        /// <summary>
        /// 预加载对象
        /// </summary>
        void Preload(int count);

        IObjectPool<T> CreatePool();
    }

    /// <summary>
    /// 对象池统计信息
    /// </summary>
    public struct PoolStats
    {
        public int TotalCount;
        public int ActiveCount;
        public int InactiveCount;

        public PoolConfig PoolConfig;

        public override string ToString()
        {
            return $"对象池统计: 总计{TotalCount}, 活跃{ActiveCount}, 闲置{InactiveCount}\r\n" + PoolConfig.ToString();
        }
    }

    /// <summary>
    /// 池化对象接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 当对象从池中取出时调用
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 当对象返回池中时调用
        /// </summary>
        void OnDespawn();
    }
}