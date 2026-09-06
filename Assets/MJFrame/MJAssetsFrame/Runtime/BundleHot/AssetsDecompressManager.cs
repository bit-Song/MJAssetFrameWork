using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MJ.AssetFrameWork.ABFrame
{

    public class AssetsDecompressManager : IDecompressAssets
    {
        //资源内容路径
        private string mStreamingAssetsBudlePath;

        //资源解压路径
        private string mDecompressPath;

        //需要解压的资源列表
        private List<string> mNeedDecompressAssetsFileList = new List<string>();

        //存储解压事件
        private UniTask mDecompressTask;

        /// <summary>
        /// 开始解压内嵌文件（同步返回，解压在后台进行）
        /// </summary>
        /// <param name="bundleModuleEnum">解压内源模块</param>
        /// <returns></returns>
        public override IDecompressAssets StartDeCompressBuiltinFile(BundleModuleEnum bundleModuleEnum)
        {
            if (ComputeDeCompressFile(bundleModuleEnum))
            {
                IsStartDecompress = true;
                mDecompressTask = UnPackToPresistentDataPath(bundleModuleEnum);
            }
            else
            {
                Debug.Log("不需要解压文件：" + bundleModuleEnum.ToString());
                mDecompressTask = UniTask.CompletedTask;
            }
            return this;
        }

        /// <summary>
        /// 等待解压完成
        /// </summary>
        public override UniTask WaitDecompress()
        {
            return mDecompressTask;
        }


        /// <summary>
        /// 计算需要解压的文件
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <returns></returns>
        private bool ComputeDeCompressFile(BundleModuleEnum bundleModuleEnum)
        {
            mStreamingAssetsBudlePath = BundleSettings.Instance.GetAssetsBuiltinBundlePath(bundleModuleEnum);
            mDecompressPath = BundleSettings.Instance.GetAssetsDeCompressPath(bundleModuleEnum);
            mNeedDecompressAssetsFileList.Clear();

#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN
            //如果文件夹不存在就创建
            if (!Directory.Exists(mDecompressPath))
                Directory.CreateDirectory(mDecompressPath);

            //计算需要解压的文件及大小
            TextAsset textAssets = Resources.Load<TextAsset>(bundleModuleEnum + "info");
            if (textAssets != null)
            {
                List<BuiltinBundleInfo> builtinBundleInfoList = JsonConvert.DeserializeObject<List<BuiltinBundleInfo>>(textAssets.text);
                foreach (var info in builtinBundleInfoList)
                {
                    //本地文件存储路径
                    string localFilePath = mDecompressPath + info.fileName;
                    if (localFilePath.EndsWith(".meta"))
                        continue;

                    //计算出需要解压的文件
                    if (!File.Exists(localFilePath) || MD5.GetMd5FromFile(localFilePath) != info.md5)
                    {
                        //重新进行解压
                        mNeedDecompressAssetsFileList.Add(info.fileName);
                        TotalSizem += info.size / 1024f;
                    }

                }


            }
            else
            {
                Debug.LogError(bundleModuleEnum + "info" + "不存在，请检测内嵌资源是否内嵌完成");
            }
            //如果大于0说明需要解压
            return mNeedDecompressAssetsFileList.Count > 0;
#else

            return false;
#endif
        }

        /// <summary>
        /// 得到当前资源解压进度
        /// </summary>
        /// <returns></returns>
        public override float GetDeCompressProgress()
        {
            return AlreadyDecompressSizem / TotalSizem;
        }


        /// <summary>
        /// 解压文件至持久化目录
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <returns></returns>
        private async UniTask UnPackToPresistentDataPath(BundleModuleEnum bundleModuleEnum)
        {
            foreach (var fileName in mNeedDecompressAssetsFileList)
            {
                string filePath = "";

#if UNITY_EDITOR_OSX || UNITY_IOS
                filePath = "file://" + mStreamingAssetsBudlePath + fileName;
#else
                filePath = mStreamingAssetsBudlePath + fileName;
#endif
                Debug.Log("Start UnPack AssetBundle filePath:" + filePath + "\r\n UnPackPath:" + mDecompressPath);

                //通过UnityWebRequest 访问本地文件 
                using UnityWebRequest unityWebRequest = UnityWebRequest.Get(filePath);
                unityWebRequest.timeout = 30;
                await unityWebRequest.SendWebRequest();
                if (unityWebRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("UnPack Error:" + unityWebRequest.error);
                }
                else
                {
                    byte[] bytes = unityWebRequest.downloadHandler.data;
                    FileHelper.WriteFile(mDecompressPath + fileName, bytes);
                    AlreadyDecompressSizem += (bytes.Length / 1024f) / 1024f;
                    Debug.Log("AlreadyDecompressSizem:" + AlreadyDecompressSizem + "TotalSizem:" + TotalSizem);
                    Debug.Log("UnPack Finish " + mDecompressPath + fileName);
                }
                unityWebRequest.Dispose();

                //IsStartDecompress = false;
            }
            IsStartDecompress = false;
        }

    }
}
