using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MJ.AssetFrameWork.ABFrame.Example
{
    public class HallWindow : MonoBehaviour
    {
        public Button exShopButton;
        void Start()
        {
            exShopButton.onClick.AddListener(OnExShopButtonClick);
        }

        public void OnExShopButtonClick()
        {
            MJAssetsABFrame.Instantiate(AssetPathConfig.HALL_PREFAB_PAHT+"ExShopWindow");
        }

    }

}