using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class ClassObjectPool<T> where T : class, new()
    {
        /// <summary>
        /// 存放类的一个对象池  偏底层的东西尽量不使用List
        /// </summary>
        protected Stack<T> mPool = new Stack<T>();

        //最大的缓存对象个数 小于等于0表示不限个数
        protected int mMaxCount = 0;

        public ClassObjectPool(int maxCount)
        {
            mMaxCount = maxCount;
            for (int i = 0; i < maxCount; i++)
            {
                mPool.Push(new T());
            }
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public T Spawn()
        {
            if (mPool.Count > 0)
                return mPool.Pop();
            else
                return new T();
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj"></param>
        public void Recycl(T obj)
        {
            if (obj == null)
            {
                Debug.LogError("Recycl obj failed,obj is null");
                return;
            }
            mPool.Push(obj);
        }
    }
}

