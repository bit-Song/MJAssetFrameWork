using System.Text;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame.Pool
{
    /// <summary>
    /// 基础对象池配置
    /// </summary>
    [System.Serializable]
    public class PoolConfig
    {
        [Header("基础设置")]
        [Tooltip("初始池大小")]
        public int InitialSize = 10;

        [Tooltip("最大池大小 (0表示无限制)")]
        public int MaxSize = 0;

        [Tooltip("是否允许自动扩展")]
        public bool AllowExpand = true;

        [Header("预加载设置")]
        [Tooltip("是否在启动时预加载")]
        public bool PreloadOnStart = true;

        [Tooltip("预加载数量")]
        public int PreloadCount = 5;

        [Header("回收设置")]
        [Tooltip("闲置对象自动回收时间 (0表示不回收)")]
        public float AutoRecycleTime = 0f;


        public override string ToString()
        {
            return $"对象池配置信息: 对象池容量 {(MaxSize == 0 ? "无限制" : MaxSize)}, 是否允许自动扩展 {(AllowExpand ? "是" : "否")}," +
                $" 闲置对象自动回收时间 {(AutoRecycleTime == 0 ? "永不回收" : AutoRecycleTime)},是否开启预加载 {(PreloadOnStart ? "是" : "否")},预加载数量{PreloadCount}";
        }
    }
}