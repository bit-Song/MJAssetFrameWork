using MJ.AssetFrameWork.ABFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class BundleModuleConfig: EditorWindow
    {
        //当前选中的页签
        private E_MenuSelected _CurrentPage = E_MenuSelected.AssetBundle;
        //包名
        public string moduleName;

        //用于资源文件夹选择
        DefaultAsset _folderAsset;
        //预制体包页签文本
        private string prefabTabel = "该文件夹下的所有预制体都会单独打成一个AssetBundle";
        //预制体包路径配置Tatle
        private string prefabTatle = "预制体资源路径配置";
        //存储配置的路径
        public ArrayWindow preArrayWindow = new ArrayWindow();

        //文件夹子包页签文本
        public string rootFolderSubBundle = "该文件夹下的所有子文件夹都会单独达成一个AssetBundle";
        private string rootFolderSubBundleTatle = "文件夹子包径配置";
        public ArrayWindow rootFolderWindo = new ArrayWindow();


        //单个补丁包页签文本
        public string signBundle = "指定的文件夹会单独打成一个AssetBundle";
        private string signBundleTatle = "单个补丁包路径配置";
        //public ArrayWindow signBundleWindow = new ArrayWindow();
        public SignArrayWindow signBundleWindow = new SignArrayWindow();

        private string[] _toolbarNames = { "预制体包", "文件夹子包", "单个补丁包" };

        int toolbarIndex = 0;
        int selectIndex;
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("请输入资源模块名称", MessageType.Warning);
            moduleName = EditorGUILayout.TextField("资源模块名称", moduleName, GUILayout.Width(800), GUILayout.Height(24));
            selectIndex = GUILayout.Toolbar(toolbarIndex, _toolbarNames);
            SelectAction();
            //将按钮绘制在最底部
            GUILayout.FlexibleSpace();
            //绘制按钮事件
            if (GUILayout.Button("DeleteConfiguration", GUILayout.Height(47)))
            {
                //移除配置
                DeleteConfiguration();
            }
            if (GUILayout.Button("SaveConfiguration", GUILayout.Height(47)))
            {
                //保存配置
                SaveConfiguration();
            }

            EditorGUILayout.EndVertical();
        }




        /// <summary>
        /// 选择事件
        /// </summary>
        private void SelectAction()
        {
            if (selectIndex != toolbarIndex)
                toolbarIndex = selectIndex;

            EditorGUILayout.BeginVertical();
            switch (toolbarIndex)
            {
                case 0:
                    EditorGUILayout.HelpBox(prefabTabel, MessageType.None);
                    preArrayWindow.GetWindow();
                    break;
                case 1:
                    EditorGUILayout.HelpBox(rootFolderSubBundle, MessageType.None);
                    rootFolderWindo.GetWindow();
                    break;
                case 2:
                    EditorGUILayout.HelpBox(signBundle, MessageType.None);
                    signBundleWindow.GetWindow();
                    break;
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示当前窗口
        /// </summary>
        /// <param name="moduleName"></param>
        public static void ShowWindow(string moduleName)
        {
            BundleModuleData bundleModuleData = BuildBundleConfigura.Instance.GetBundleDataByName(moduleName);
            BundleModuleConfig window = GetWindowWithRect<BundleModuleConfig>(new Rect(0, 0, 600, 600));
            if (bundleModuleData != null)
            {
                window.moduleName = bundleModuleData.moduleName;
                window.preArrayWindow.pathList = bundleModuleData.prefabPathArr;
                window.rootFolderWindo.pathList = bundleModuleData.rootFolderPathArr;
                window.signBundleWindow.fileInfoList = bundleModuleData.signFolderPathArr;
            }
            else
                Debug.LogError("资源包查找失败");

            window.preArrayWindow.Init(window.prefabTatle);
            window.rootFolderWindo.Init(window.rootFolderSubBundleTatle);
            window.signBundleWindow.Init(window.signBundleTatle);

        }

        /// <summary>
        /// 移除配置
        /// </summary>
        private void SaveConfiguration()
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                UnityEditor.EditorUtility.DisplayDialog("保存失败！", "模块名称不能为空", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认保存", $"确定要保存模块「{moduleName}」吗？", "确认保存", "取消"))
                return;
            BundleModuleData moduleData = BuildBundleConfigura.Instance.GetBundleDataByName(moduleName);

            if (moduleData == null)
            {
                //添加新的模块资源
                moduleData = new BundleModuleData();
                moduleData.moduleName = this.moduleName;
                moduleData.prefabPathArr = preArrayWindow.pathList;
                moduleData.rootFolderPathArr = rootFolderWindo.pathList;
                moduleData.signFolderPathArr = signBundleWindow.fileInfoList;
                BuildBundleConfigura.Instance.SaveModuleData(moduleData);
            }
            else
            {
                moduleData.prefabPathArr = preArrayWindow.pathList;
                moduleData.rootFolderPathArr = rootFolderWindo.pathList;
                moduleData.signFolderPathArr = signBundleWindow.fileInfoList;
                BuildBundleConfigura.Instance.SaveModuleData(moduleData);
            }

            UnityEditor.EditorUtility.DisplayDialog("保存成功！", "配置已储存", "确定");
            Close();
            BuildWindow.ShowAssetBundleWindow();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void DeleteConfiguration()
        {
            //提示窗口
            if (!EditorUtility.DisplayDialog("确认删除", $"确定要删除模块「{moduleName}」吗？", "确认删除", "取消"))
                return;

            BuildBundleConfigura.Instance.RemoveModuleByName(moduleName);
            UnityEditor.EditorUtility.DisplayDialog("删除成功！", "配置已删除", "确定");
            Close();
            BuildWindow.ShowAssetBundleWindow();

        }
    }
}

