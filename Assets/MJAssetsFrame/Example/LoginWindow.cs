using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class LoginWindow : MonoBehaviour
    {



        public void OnLOginButtonClick()
        {
            MJAssetsABFrame.Release(gameObject);
            MJAssetsABFrame.ClearResoucesAssets(true);
            //µ¯³ö´°¿Ú´óÌü
            MJAssetsABFrame.Instantiate(@"Assets/BundleDemo/Hall/Prefab/HallWindow");

        }
    }
}

