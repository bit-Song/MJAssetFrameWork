using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace MJ.AssetFrameWork.ABFrame
{
    public class BuildBundleWindow : BundleBehaviour
    {
        protected string[] buildButtonsNameArr = new string[] { "打包资源", "内嵌资源" };
        public override void Initzation()
        {
            base.Initzation();
        }

        /// <summary>
        /// 绘制添加资源模块的按钮
        /// </summary>
        public override void DrawAddModuleButton()
        {
            base.DrawAddModuleButton();
        }

        /// <summary>
        /// 绘制下方按钮
        /// </summary>
        public override void DrawBuildButtons()
        {
            base.DrawBuildButtons();

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
                //绘制打包图标和内嵌资源图标
                btnRect = GUILayoutUtility.GetLastRect();
                iconRect = new Rect(btnRect.x + 10f, btnRect.y + 12f, 30f, 30f);
                string iconName = (i == 0) ? CurPlatfam : "SceneSet Icon";
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
                    BuildBundleCompiler.BuildAssetBundle(item);
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
                    BuildBundleCompiler.CopyBundleToStramingAssets(item);
                }
            }
        }
    }
}

