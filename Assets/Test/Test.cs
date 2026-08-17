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
    private async Task Start()
    {
        HotUpdateManager.Instance.HotAndPackAssets(BundleModuleEnum.GameItem).Forget();
    }
}
