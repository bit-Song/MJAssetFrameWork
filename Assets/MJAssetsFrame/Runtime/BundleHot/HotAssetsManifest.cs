using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 热更清单
    /// </summary>
    public class HotAssetsManifest
    {
        //热更公告
        public string updateNotice;
        //热更下载地址
        public string downLoadUrl;
        //热更资源补丁列表
        public List<HotAssetsPatch> hotAssetsPatcheList = new List<HotAssetsPatch>();


    }

    /// <summary>
    /// 热更资源补丁
    /// </summary>
    public class HotAssetsPatch
    {
        //热更补丁版本
        public int patchVersion;
        //热更资源信息列表
        public List<HotFileInfo> hotAssetsList = new List<HotFileInfo>();
    }

    public class HotFileInfo
    {
        //ab包名
        public string abName;
        //校验码
        public string md5;
        //文件大小
        public float size;
    }

}
