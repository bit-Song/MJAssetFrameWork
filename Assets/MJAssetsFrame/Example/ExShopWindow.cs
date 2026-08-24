using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame.Example
{
    public class ExShopWindow : MonoBehaviour
    {
        public Transform itemParent;

        public List<int> itemIDlist = new List<int>();

        public List<ExShopItem> exShopItems = new List<ExShopItem>();


        private void Start()
        {

            //AssetBundleManager.Instance.LoadAssetBundelConfig(BundleModuleEnum.GameItem);

        }

        private void OnEnable()
        {
            for (int i = 0; i < 15; i++)
            {
                itemIDlist.Add(i + 6000 + 1);
            }

            //生成兑换道具列表
            foreach (var id in itemIDlist)
            {
                GameObject itemObj = MJAssetsABFrame.Instantiate(AssetPathConfig.HALL_PREFAB_PAHT + "ExShopItem", itemParent);
                itemObj.SetActive(true);
                ExShopItem item = itemObj.GetComponent<ExShopItem>();
                item.SetData(id).Forget();
                exShopItems.Add(item);
            }
        }


        public void OnDisable()
        {
            foreach (var item in exShopItems)
            {
                item.Release();
            }

        }

        public void OnClickButtonClick()
        {
            MJAssetsABFrame.Release(this.gameObject, true);
        }
    }
}