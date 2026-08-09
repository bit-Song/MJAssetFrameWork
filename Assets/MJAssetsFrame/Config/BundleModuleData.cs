using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MJ.AssetFrameWork.ABFrame
{
    [System.Serializable]
    public class BundleModuleData
    {
        //AssetBundle模块id
        public long bundleid;
        //模块名称
        public string moduleName;
        //是否打包
        public bool isBuild;

        //上一次点击按钮的时间
        public float lastClickBtnTime;

        public List<string> prefabPathArr;

        public List<string> rootFolderPathArr;

        public List<BundleFileInfo> signFolderPathArr;
    }

    [System.Serializable]
    public class BundleFileInfo
    {
        public string abName = "AB Name";
        public string bundlePath = "BundlePath...";
    }
}