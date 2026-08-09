using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class BuildHotPatchWindow : BundleBehaviour
    {
        protected string[] buildButtonsNameArr = new string[] { "打包热更", "上传资源" };
        //热更描述 热更公告
        [HideInInspector] public string patchDes = "输入本次热更描述...";
        //热更补丁版本
        [HideInInspector] public string hotVersion = "1";
        public override void Initzation()
        {
            base.Initzation();
        }

        public override void DrawAddModuleButton()
        {
            base.DrawAddModuleButton();

        }

        public override void OGUI()
        {
            base.OGUI();
        }

        /// <summary>
        /// 绘制按钮和内容
        /// </summary>
        public override void DrawBuildButtons()
        {
            base.DrawBuildButtons();

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("请输入本次热更公告");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            patchDes = GUILayout.TextField(patchDes, GUILayout.Width(800), GUILayout.Height(80));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            hotVersion = EditorGUILayout.TextField("热更资源版本:", hotVersion, GUILayout.Width(800), GUILayout.Height(24));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            Rect btnRect;
            Rect iconRect;
            for (int i = 0; i < buildButtonsNameArr.Length; i++)
            {
                GUIStyle style = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("PreButtonBlue");
                style.fixedHeight = 55;

                if (GUILayout.Button(buildButtonsNameArr[i], style, GUILayout.Height(55)))
                {
                    if (i == 0)
                    {
                        //打包AssetBundle按钮事件
                        BuildBundle();
                    }
                    else
                    {
                        CopyBundleToStreamingAssetsPath();
                    }
                }

                //绘制打包图标和上传图标
                btnRect = GUILayoutUtility.GetLastRect();
                iconRect = new Rect(btnRect.x + 10f, btnRect.y + 12f, 30f, 30f);
                string iconName = (i == 0) ? CurPlatfam : "CollabPush";
                GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent(iconName).image);

            }
            GUILayout.EndHorizontal();

        }

        /// <summary>
        /// 打包资源
        /// </summary>
        public override void BuildBundle()
        {
            base.BuildBundle();
            foreach (var item in moduleDataList)
            {
                if (item.isBuild)
                {
                    //TODO 
                    BuildBundleCompiler.BuildAssetBundle(item, E_BuildType.HotPatch, int.Parse(hotVersion), patchDes);
                }
            }
        }

        /// <summary>
        /// 内嵌资源
        /// </summary>
        public void CopyBundleToStreamingAssetsPath()
        {
            foreach (var item in moduleDataList)
            {
                if (item.isBuild)
                {
                    //TODO 
                }
            }
        }
    }
}

