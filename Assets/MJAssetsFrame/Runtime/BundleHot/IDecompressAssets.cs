using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public abstract class IDecompressAssets
    {

        /// <summary>
        /// 需要解压资源的总大小
        /// </summary>
        public float TotalSizem { get; protected set; }

        /// <summary>
        /// 已经解压的大小
        /// </summary>
        public float AlreadyDecompressSizem { get; protected set; }

        /// <summary>
        /// 是否开始解压
        /// </summary>
        public bool IsStartDecompress { get; protected set; }

        ///// <summary>
        ///// 开始解压内嵌文件
        ///// </summary>
        ///// <returns></returns>
        //public abstract UniTask<IDecompressAssets> StartDeCompressBuiltinFile(BundleModuleEnum bundleModuleEnum);


        /// <summary>
        /// 开始解压内嵌文件（同步返回，解压在后台进行）
        /// </summary>
        /// <returns></returns>
        public abstract IDecompressAssets StartDeCompressBuiltinFile(BundleModuleEnum bundleModuleEnum);

        /// <summary>
        /// 等待解压完成
        /// </summary>

        public abstract UniTask WaitDecompress();

        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns></returns>
        public abstract float GetDeCompressProgress();
    }
}