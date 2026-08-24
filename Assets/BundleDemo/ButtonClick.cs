using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MJ.AssetFrameWork.ABFrame
{
    public class ButtonClick : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        void OnButtonClick()
        {
            //MJAssetsABFrame.Release(transform.parent.gameObject, true);
            MJAssetsABFrame.ClearResoucesAssets(true);
            MJAssetsABFrame.Instantiate(@"Assets/BundleDemo/Hall/Prefab/HallWindow");
        }
    }
}

