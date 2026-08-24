using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame.Example
{
    public class ExShopItem : MonoBehaviour
    {
        public Transform gameItemParent;
        public GameObject loadingObj;

        private GameObject mItemObj;


        public async UniTask SetData(int itemId)
        {
            loadingObj.SetActive(true);
            GameObject itemObj = await MJAssetsABFrame.InstantiateAndLoadAsync("Assets/BundleDemo/GameItem/" + itemId + "/" + itemId);
            loadingObj.SetActive(false);
            if (itemObj != null)
            {
                itemObj.SetActive(true);
                itemObj.transform.SetParent(gameItemParent);
                itemObj.transform.localPosition = Vector3.zero;
                itemObj.transform.localScale = Vector3.one;
                itemObj.transform.rotation = Quaternion.identity;
                mItemObj = itemObj;
            }

        }


        public void Release()
        {
            if (mItemObj != null)
            {
                MJAssetsABFrame.Release(mItemObj, true);
            }
            MJAssetsABFrame.Release(this.gameObject, true);
        }
    }

}