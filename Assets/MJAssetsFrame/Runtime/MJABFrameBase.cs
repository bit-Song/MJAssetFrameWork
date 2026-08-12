using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{

    public class MJABFrameBase : MonoBehaviour
    {
        protected static MJAssetsABFrame instance = null;

        public static MJAssetsABFrame Instance
        {
            get
            {
                if (instance == null)
                    instance = Object.FindObjectOfType<MJAssetsABFrame>();
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<MJAssetsABFrame>();
                    DontDestroyOnLoad(instance.gameObject);
                    instance.OnInitlizate();
                }
                return instance;
            }
        }




        protected virtual void OnInitlizate()
        {

        }
    }

}