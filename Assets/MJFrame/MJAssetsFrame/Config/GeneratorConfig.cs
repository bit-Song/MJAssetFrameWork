using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    [CreateAssetMenu(menuName = "ScriptableObject/AssetFrame/GeneratorConfig", fileName = "GeneratorConfig", order = 4)]
    public class GeneratorConfig : ScriptableObject
    {
        private static GeneratorConfig _instance;
        public static GeneratorConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    //_instance = AssetDatabase.LoadAssetAtPath<BuildBundleConfigura>("Assets/MJAssetsFrame/Config/BuildBundleConfigura.asset");
                    _instance = Resources.Load<GeneratorConfig>("GeneratorConfig");
                }
                return _instance;
            }
        }
        //枚举生成位置
        [Header("枚举文件存放路径")]
        [SerializeField]
        private string bundleModuleEnumFilePath = "MJAssetsFrame/Config";
        public string BundleModuleEnumFilePath
        {
            get
            {
                return Application.dataPath + "/" + bundleModuleEnumFilePath + "/BundleModuleEnum.cs";
            }
        }
        //路径生成位置
        [Header("资源加载文件存放路径")]
        [SerializeField]
        private string bundleModuleAssetsFilePath = "MJAssetsFrame/Config";
        public string BundleModuleAssetsFilePath
        {
            get
            {
                return Application.dataPath + "/" + bundleModuleAssetsFilePath + "/AssetPath.cs";
            }
        }

        //资源存放路径
        [Header("资源存放路径")]
        [SerializeField]
        private string bundleModuleAssetsPath = "BundleAssetsDemo";

        public string BundleModuleAssetsPath
        {
            get
            {
                return "Assets/" + bundleModuleAssetsPath;
            }
        }
    }

}