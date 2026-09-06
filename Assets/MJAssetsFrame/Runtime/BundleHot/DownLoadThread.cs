using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MJ.AssetFrameWork.ABFrame
{

    public class DownLoadThread
    {
        //当前资源模块
        private HotAssetsModule mCurHotAssetsMoudle;

        //当前热更文件信息
        private HotFileInfo mHotFileInfo;

        //文件下载地址
        private string mDownLoadUrl;
        //下载文件的存储地址
        private string mFileSavePath;

        //下载成功回调
        private Action<DownLoadThread, HotFileInfo> mDownLoadSuccess;
        //下载失败回调
        private Action<DownLoadThread, HotFileInfo> mDownLoadFailed;
        //下载大小
        private float downLoadSizeKb;

        //当前下载次数
        private int mDurDownLoadCount;
        private const int Max_TRY_DOWNLOAD_COUNT = 3;

        /// <summary>
        /// 资源下载线程
        /// </summary>
        /// <param name="assetsModule">资源所属模块</param>
        /// <param name="hotFileInfo">需要下载的资源</param>
        /// <param name="downLoadUrl">资源下载路径</param>
        /// <param name="fileSavePath">文件存储地址</param>
        public DownLoadThread(HotAssetsModule assetsModule, HotFileInfo hotFileInfo, string downLoadUrl, string fileSavePath)
        {
            this.mCurHotAssetsMoudle = assetsModule;
            this.mHotFileInfo = hotFileInfo;
            this.mFileSavePath = fileSavePath + "/" + hotFileInfo.abName;
            this.mDownLoadUrl = downLoadUrl + "/" + hotFileInfo.abName;
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> StartDownLoad()
        {
            for (int i = 0; i < Max_TRY_DOWNLOAD_COUNT; i++)
            {
                mDurDownLoadCount++;
                // 清理上次失败残留的半成品文件
                if (File.Exists(mFileSavePath))
                    File.Delete(mFileSavePath);

                using (UnityWebRequest request = UnityWebRequest.Get(mDownLoadUrl))
                {
                    request.downloadHandler = new DownloadHandlerFile(mFileSavePath);

                    // 根据文件大小动态计算 timeout
                    float estimatedSeconds = mHotFileInfo.size / 1024f / 20f; // 假设最低 20KB/s
                    request.timeout = Mathf.Max(30, (int)(estimatedSeconds * 2)); // 留 2 倍余量
                    //request.timeout = 30;

                    Debug.Log("StartDownLoad ModuleEnum:" + mCurHotAssetsMoudle.CurBundleModuleEnum + " AssetBundle Url:" + mDownLoadUrl);

                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        downLoadSizeKb = request.downloadedBytes / 1024f;
                        await UniTask.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        float downloadedSizeM = request.downloadedBytes / 1024f / 1024f;
                        mCurHotAssetsMoudle.mAssetDownLoadSizeM += downloadedSizeM;
                        downLoadSizeKb = request.downloadedBytes / 1024f;
                        Debug.Log("DownLoadSuccess ModuleEnum:" + mCurHotAssetsMoudle.CurBundleModuleEnum + " AssetBundleUrl:" + mDownLoadUrl + " FileSavePath:" + mFileSavePath);
                        return true;
                    }

                    Debug.LogError("文件下载失败，正在重新下载，当前尝试下载次数：" + mDurDownLoadCount + "\r\nURL:" + mDownLoadUrl + "\r\nError:" + request.error);
                }
            }
            return false;




            //for (int i = 0; i < Max_TRY_DOWNLOAD_COUNT; i++)
            //{
            //    mDurDownLoadCount++;
            //    bool success = false;
            //    string errorMsg = null;
            //    float downloadedSize = 0;

            //    await UniTask.RunOnThreadPool(() =>
            //    {
            //        try
            //        {
            //            Debug.Log("StartDownLoad ModuleEnum:" + mCurHotAssetsMoudle.CurBundleModuleEnum + "AssetBudnle Url:" + mDownLoadUrl);
            //            HttpWebRequest request = WebRequest.Create(mDownLoadUrl) as HttpWebRequest;
            //            request.Method = "Get";

            //            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            //            using (var stream = response.GetResponseStream())
            //            using (var fileStream = File.Create(mFileSavePath))
            //            {
            //                byte[] buffer = new byte[1024];
            //                int size = stream.Read(buffer, 0, buffer.Length);

            //                while (size > 0)
            //                {
            //                    fileStream.Write(buffer, 0, size);
            //                    size = stream.Read(buffer, 0, buffer.Length);
            //                    downLoadSizeKb += size;
            //                    downloadedSize += size;
            //                    //await UniTask.Delay(10);
            //                }
            //                success = true;
            //                Debug.Log("DownLoadSuccess MoudleEnmu:" + mCurHotAssetsMoudle.CurBundleModuleEnum + "AssetBundleUrl:" + mDownLoadUrl + " FileSavePath:" + mFileSavePath);
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            errorMsg = e.ToString();
            //        }
            //    });

            //    if (success)
            //    {
            //        mCurHotAssetsMoudle.mAssetDownLoadSizeM += downloadedSize / 1024f / 1024f;
            //        return true;   // 成功
            //    }
            //    Debug.LogError("文件下载失败，正在重新下载，当前尝试下载次数：" + mDurDownLoadCount + "\r\nURL:" + mDownLoadUrl);
            //}
            //return false;  // 全部失败
        }
    }

}
