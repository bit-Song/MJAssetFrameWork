using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class FtpUploader
    {
        private int totalFileCount;
        private int uploadedFileCount;

        private static FtpUploader instance = new FtpUploader();
        public static FtpUploader Instance => instance;

        public FtpUploader()
        {
        }

        /// <summary>
        /// 上传单个文件到FTP
        /// </summary>
        /// <param name="localPath">本地文件绝对路径</param>
        /// <param name="remoteRelativePath">服务器相对路径（相对于 remoteBasePath），如 Hall/6/StandaloneWindows/hall_hall.ab</param>
        public void UploadFile(string localPath, string remoteRelativePath)
        {
            // 确保远程目录存在
            EnsureRemoteDirectoryExists(remoteRelativePath);
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteRelativePath);
                request.Credentials = new NetworkCredential(FtpConfig.Instance.userName, FtpConfig.Instance.password);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.KeepAlive = false;

                byte[] fileData = File.ReadAllBytes(localPath);
                request.ContentLength = fileData.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileData, 0, fileData.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // 上传成功
                }

                uploadedFileCount++;
                float progress = (float)uploadedFileCount / totalFileCount;
                EditorUtility.DisplayProgressBar(
                    "FTP上传资源",
                    $"[{uploadedFileCount}/{totalFileCount}] {Path.GetFileName(localPath)}",
                    progress);
                Debug.Log("资源文件上传成功:"+ localPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"FTP上传失败: {localPath}\n{e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 上传文件夹下的所有文件（递归子目录）
        /// </summary>
        /// <param name="localDirPath">本地文件夹路径</param>
        /// <param name="remoteRelativePath">远程相对路径</param>
        public void UploadDirectory(string localDirPath, string remoteRelativePath)
        {
            // 统计文件总数
            string[] allFiles = Directory.GetFiles(localDirPath, "*", SearchOption.AllDirectories);
            totalFileCount += allFiles.Length;

            foreach (string filePath in allFiles)
            {
                // 计算相对于本地目录的相对路径
                string relativePath = filePath.Substring(localDirPath.Length).Replace('\\', '/').TrimStart('/');
                string remotePath = remoteRelativePath + "/" + relativePath;
                UploadFile(filePath, remotePath);
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 递归创建远程目录
        /// </summary>
        private void EnsureRemoteDirectoryExists(string remoteFileUrl)
        {
            // 从文件URL中提取目录URL
            string dirUrl = remoteFileUrl.Substring(0, remoteFileUrl.LastIndexOf('/'));
            // 拆分路径，逐级创建
            string basePath = $"ftp://{FtpConfig.Instance.host.Replace("ftp://", "")}";

            string relativePath = dirUrl.Substring(basePath.Length).TrimStart('/');

            string[] segments = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string currentPath = basePath;
            foreach (string segment in segments)
            {
                currentPath = currentPath + "/" + segment;
                TryCreateDirectory(currentPath);
            }
        }

        /// <summary>
        /// 尝试创建目录（已存在则跳过）
        /// </summary>
        private void TryCreateDirectory(string dirUrl)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(dirUrl);
                request.Credentials = new NetworkCredential(FtpConfig.Instance.userName, FtpConfig.Instance.password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // 目录创建成功
                }
            }
            catch (WebException)
            {
                // 目录已存在，忽略异常
            }
        }

        /// <summary>
        /// 重置计数器
        /// </summary>
        public void ResetCounter()
        {
            totalFileCount = 0;
            uploadedFileCount = 0;
        }
    }
}

