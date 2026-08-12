using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using UnityEngine;
namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 资源下载线程
    /// </summary>
    public class DownLoadThread
    {
        //当前资源模块
        private HotAssetsModule curHotAssetsMoudle;

        //当前热更文件信息
        private HotFileInfo hotFileInfo;

        //文件下载地址
        private string downLoadUrl;

        //下载文件的存储地址
        private string fileSavePath;

        //下载成功回调
        private Action<DownLoadThread, HotFileInfo> downLoadSuccess;
        //下载失败回调
        private Action<DownLoadThread, HotFileInfo> downLoadFailed;
        //下载大小
        private float downLoadSizeKb;

        //当前下载次数
        private int curDownLoadCount;
        private const int Max_TRY_DOWNLOAD_COUNT = 3;

        /// <summary>
        /// 资源下载线程
        /// </summary>
        /// <param name="assetsModule">资源所属模块</param>
        /// <param name="hotFileInfo">需要下载的资源呢</param>
        /// <param name="downLoadUrl">资源下载路径</param>
        /// <param name="fileSavePath">文件存储地址</param>
        public DownLoadThread(HotAssetsModule assetsModule, HotFileInfo hotFileInfo, string downLoadUrl, string fileSavePath)
        {
            this.curHotAssetsMoudle = assetsModule;
            this.hotFileInfo = hotFileInfo;
            this.fileSavePath = fileSavePath + "/" + hotFileInfo.abName;
            this.downLoadUrl = downLoadUrl + "/" + hotFileInfo.abName;
        }

        /// <summary>
        /// 开始通过子线程下载资源
        /// </summary>
        /// <param name="downLoadSuccess">下载成功回调</param>
        /// <param name="downLoadFailed">下载失败回调</param>
        public void StartDownLoad(Action<DownLoadThread, HotFileInfo> downLoadSuccess, Action<DownLoadThread, HotFileInfo> downLoadFailed)
        {
            curDownLoadCount++;
            this.downLoadSuccess = downLoadSuccess;
            this.downLoadFailed = downLoadFailed;

            //开启线程下载资源
            Task.Run(() =>
            {
                try
                {
                    Debug.Log("StartDownLoad ModuleEnum:" + curHotAssetsMoudle.CurBundleModuleEnum + "AssetBudnle Url:" + downLoadUrl);
                    HttpWebRequest request = WebRequest.Create(downLoadUrl) as HttpWebRequest;
                    request.Method = "Get";
                    //发起请求
                    HttpWebResponse reponse = request.GetResponse() as HttpWebResponse;

                    //创建本地文件流
                    FileStream fileStream = File.Create(fileSavePath);

                    using (var stream = reponse.GetResponseStream())
                    {
                        //if (stream.Length == 0)
                        //    Debug.LogError("Fiel DownLoad Exception plase check file fileName:" + hotFileInfo.abName + " fileUrl:" + downLoadUrl);
                        byte[] buffer = new byte[512];
                        //从字节流中读取字节到buffer中
                        int size = stream.Read(buffer, 0, buffer.Length);

                        while (size > 0)
                        {
                            //将字节写入文件流中
                            fileStream.Write(buffer, 0, size);

                            //重复读取
                            size = stream.Read(buffer, 0, buffer.Length);
                            //记录下载文件的大小
                            //1mb = 1024kb    1kb = 1024字节
                            downLoadSizeKb += size;
                            //计算以M为单位的大小
                            curHotAssetsMoudle.AssetDownLoadSizeM += ((size / 1024) / 1024);
                        }
                        fileStream.Dispose();
                        fileStream.Close();
                        Debug.Log("DownLoadSuccess MoudleEnmu:" + curHotAssetsMoudle.CurBundleModuleEnum + "AssetBundleUrl:" + downLoadUrl + " FileSavePath:" + fileSavePath);
                        downLoadSuccess?.Invoke(this, hotFileInfo);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("DownlLoadAssetBundel Error Url:" + downLoadUrl + "\r\nexception:" + e.ToString());
                    //尝试下载次数
                    if (curDownLoadCount > Max_TRY_DOWNLOAD_COUNT)
                    {
                        downLoadFailed?.Invoke(this, hotFileInfo);
                    }
                    else
                    {
                        Debug.LogError("文件下载失败，正在重新下载，当前尝试下载次数：" + curDownLoadCount);
                        StartDownLoad(downLoadSuccess, downLoadFailed);
                    }
                }
            });
        }
    }
}

