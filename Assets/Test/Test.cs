using Cysharp.Threading.Tasks;
using MJ.AssetFrameWork.ABFrame;
using MJ.AssetFrameWork.ABFrame.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class Test : MonoBehaviour
    {
        public Stack<GameObject> pool = new Stack<GameObject>();
        public Stack<GameObject> pool2 = new Stack<GameObject>();
        private void Awake()
        {
            Debug.Log(Application.persistentDataPath);
            MJAssetsABFrame.Instance.InitFrameWork();
        }
        private async void Start()
        {
            //await HotUpdateManager.Instance.HotAndPackAssets(BundleModuleEnum.Game);
            //等待资源热更完成后加载物体
            Debug.Log("开始加载");

            await MJAssetsABFrame.HotAssets(BundleModuleEnum.Game, true);
            await MJAssetsABFrame.HotAssets(BundleModuleEnum.Login, true);
            StartGame();
        }

        public void StartGame()
        {

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                pool.Push(MJAssetsABFrame.Instantiate(AssetPath.Game.Prefab.Cube));
            }
           

            if (Input.GetKeyDown(KeyCode.S))
            {
                MJAssetsABFrame.Release(pool.Pop(), false);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                MJAssetsABFrame.Release(pool.Pop(), true);
            }
            


            if (Input.GetKeyDown(KeyCode.Q))
            {
                pool2.Push(MJAssetsABFrame.Instantiate(AssetPath.Login.Prefab.Cube));
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                MJAssetsABFrame.Release(pool2.Pop(), true);
            }



            if (Input.GetKeyDown(KeyCode.F))
            {
                MJAssetsABFrame.ClearResoucesAssets(false);
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                MJAssetsABFrame.ClearResoucesAssets(true);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PoolManager.Instance.GetAllStats();
            }
        }
    }
}
