using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame.Pool
{
    /// <summary>
    /// 对象池拓展方法
    /// </summary>
    public static class PoolExtensions
    {
        /// <summary>
        /// 从 Stack 中移除指定元素（扩展方法）
        /// </summary>
        public static bool RemoveFromStack<T>(this Stack<T> stack, T item)
        {
            if (stack.Contains(item))
            {
                var tempList = new List<T>(stack);
                tempList.Remove(item);
                stack.Clear();
                foreach (var tempItem in tempList)
                {
                    stack.Push(tempItem);
                }
                return true;
            }
            return false;
        }

        ///// <summary>
        ///// 便捷的获取组件并返回池的方法
        ///// </summary>
        //public static T GetComponentFromPool<T>(this PoolManager poolManager, string poolKey) where T : Component
        //{
        //    var go = poolManager.Get(poolKey);
        //    return go?.GetComponent<T>();
        //}

        ///// <summary>
        ///// 便捷的返回组件到池的方法
        ///// </summary>
        //public static void ReturnToPool(this Component component)
        //{
        //    if (component != null)
        //    {
        //        PoolManager.Instance.Return(component.gameObject);
        //    }
        //}
    }
}