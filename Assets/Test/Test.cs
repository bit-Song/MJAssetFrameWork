using Cysharp.Threading.Tasks;
using MJ.AssetFrameWork.ABFrame;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Test : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(Application.persistentDataPath);
        MJAssetsABFrame.Instance.InitFrameWork();
    }
    private async void Start()
    {
        await HotUpdateManager.Instance.HotAndPackAssets(BundleModuleEnum.Hall);
        //等待资源热更完成后加载物体
        Debug.Log("开始加载");
        StartGame();
    }

    public void StartGame()
    {
        MJAssetsABFrame.Instantiate(@"Assets/BundleDemo/Hall/Prefab/LoginWindow");
    }

}
