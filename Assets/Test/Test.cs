using Cysharp.Threading.Tasks;
using MJ.AssetFrameWork.ABFrame;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(Application.persistentDataPath);
        MJAssetsABFrame.Instance.InitFrameWork();
    }
    private void Start()
    {
        HotUpdateManager.Instance.HotAndPackAssets(BundleModuleEnum.GameItem).Forget();



    }
}
