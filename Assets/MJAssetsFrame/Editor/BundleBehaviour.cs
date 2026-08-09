using Codice.CM.Client.Gui;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace MJ.AssetFrameWork.ABFrame
{
    public class BundleBehaviour
    {
        /// <summary>
        /// 模块配置列表
        /// </summary>
        public List<BundleModuleData> moduleDataList;
        /// <summary>
        /// 模块配置行列表
        /// </summary>
        public List<List<BundleModuleData>> rowModuleDataList;
        //当前平台
        protected string CurPlatfam;

        public virtual void Initzation()
        {
            //获取多模块资源配置列表
            moduleDataList = BuildBundleConfigura.Instance.AssetBundleConfig;
            rowModuleDataList = new List<List<BundleModuleData>>();

            for (int i = 0; i < moduleDataList.Count; i++)
            {
                int rowIndex = Mathf.FloorToInt(i / 6);
                if (rowModuleDataList.Count < rowIndex + 1)
                {
                    rowModuleDataList.Add(new List<BundleModuleData>());
                }
                rowModuleDataList[rowIndex].Add(moduleDataList[i]);
            }
#if UNITY_IOS
            CurPlatfam = "BuildSettings.iPhone";
#else
            CurPlatfam = "BuildSettings.Android";
#endif
        }


        public virtual void OGUI()
        {
            if (rowModuleDataList == null)
                return;

            GUIContent content = EditorGUIUtility.IconContent("SceneAsset Icon".Trim());
            content.tooltip = "单击可选中和取消\n快速双击可打开配置窗口";

            for (int i = 0; i < rowModuleDataList.Count; i++)
            {
                GUILayout.BeginHorizontal();

                for (int j = 0; j < rowModuleDataList[i].Count; j++)
                {
                    BundleModuleData moduleData = rowModuleDataList[i][j];

                    Rect btnRect;
                    if (GUILayout.Button(content, GUILayout.Width(130), GUILayout.Height(170)))
                    {
                        moduleData.isBuild = !moduleData.isBuild;
                        if (Time.realtimeSinceStartup - moduleData.lastClickBtnTime <= 0.18f)
                        {
                            BundleModuleConfig.ShowWindow(moduleData.moduleName);
                        }
                        moduleData.lastClickBtnTime = Time.realtimeSinceStartup;
                    }

                    btnRect = GUILayoutUtility.GetLastRect();

                    Rect labelRect = new Rect(btnRect.x, btnRect.yMax - 22f, btnRect.width, 20f);
                    GUI.Label(labelRect, moduleData.moduleName, new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    });

                    if (moduleData.isBuild)
                    {
                        GUIStyle style = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("LightmapEditorSelectedHighlight");
                        GUI.Toggle(new Rect(btnRect.x, btnRect.y, btnRect.width, btnRect.height), true, GUIContent.none, style);
                    }
                }

                if (i == rowModuleDataList.Count - 1)
                {
                    DrawAddModuleButton();
                }
                GUILayout.EndHorizontal();
            }

            if (rowModuleDataList.Count == 0)
                DrawAddModuleButton();
        }

        public virtual void DrawBuildButtons()
        {
        }

        public virtual void BuildBundle()
        {
            //TODO 待处理事项 可以在打包的时候自动生成一个枚举类
        }

        /// <summary>
        /// 绘制添加资源模块按钮
        /// </summary>
        public virtual void DrawAddModuleButton()
        {
            GUIContent addContent = EditorGUIUtility.IconContent("CollabCreate Icon".Trim(), "");
            if (GUILayout.Button(addContent, GUILayout.Width(130), GUILayout.Height(170)))
            {
                BundleModuleConfig.ShowWindow("");
            }
        }
    }
}
