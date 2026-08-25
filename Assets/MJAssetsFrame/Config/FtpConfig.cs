using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    [CreateAssetMenu(menuName = "ScriptableObject/AssetFrame/UpLoaderFtpConfig", fileName = "UpLoaderFtpConfig")]
    public class FtpConfig : ScriptableObject
    {
        private static FtpConfig instance;
        public static FtpConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<FtpConfig>("UpLoaderFtpConfig");
                }
                return instance;
            }
        }
        [Header("FTP服务器地址")]
        public string host = "ftp://127.0.0.1/";
        [Header("FTP端口")]
        public int port = 21;
        [Header("FTP用户名")]
        public string userName = "";
        [Header("FTP密码")]
        public string password = "";

        /// <summary>
        /// 得到远端的URL路径
        /// </summary>
        /// <returns></returns>
        public string GetRemoteUrl(string remoteRelativePath)
        {
            string relPath = remoteRelativePath.Replace('\\', '/').Trim('/');

            return host+ remoteRelativePath;
        }
    }
}
