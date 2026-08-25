using Cysharp.Threading.Tasks;
using MJ.AssetFrameWork.ABFrame;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MJ.AssetFrameWork.ABFrame
{
    public class Test : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log(Application.persistentDataPath);
            MJAssetsABFrame.Instance.InitFrameWork();
        }
        private async void Start()
        {
            //await HotUpdateManager.Instance.HotAndPackAssets(BundleModuleEnum.Hall);
            ////等待资源热更完成后加载物体
            //Debug.Log("开始加载");
            //StartGame();


            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://127.0.0.1/HotAssets/1");
                request.Credentials = new NetworkCredential(FtpConfig.Instance.userName, FtpConfig.Instance.password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // 目录创建成功
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("失败" + e.Message);
                throw;
            }
        }

        public void StartGame()
        {

            //MJAssetsABFrame.Instantiate(@"Assets/BundleDemo/Hall/Prefab/LoginWindow");
            //MJAssetsABFrame.HotAssets(BundleModuleEnum.GameItem, true).Forget();
        }

    }
}
