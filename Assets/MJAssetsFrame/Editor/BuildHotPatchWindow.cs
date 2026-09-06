using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using static Codice.Client.Common.WebApi.WebApiEndpoints;

namespace MJ.AssetFrameWork.ABFrame
{
    public class ModuleVersionInfo
    {
        public int latestVersion;      // 当前最新版本
        public int targetVersion;      // 打包时使用的目标版本
        public List<int> allVersions;  // 所有历史版本
        public string versionInput;    // 手动模式下的版本输入
    }
    public class BuildHotPatchWindow : BundleBehaviour
    {
        protected string[] buildButtonsNameArr = new string[] { "打包热更", "上传资源" };
        //热更描述 热更公告
        [HideInInspector] public string patchDes = "输入本次热更描述...";
        // 是否自动递增版本号
        private bool autoIncrement = true;
        // 每个模块的版本信息缓存：moduleName -> (最新版本, 选定的目标版本)
        private Dictionary<string, ModuleVersionInfo> moduleVersionCache = new Dictionary<string, ModuleVersionInfo>();
        // 版本列表滚动位置
        private Vector2 versionListScroll = Vector2.zero;

        public string UpLoadResourcesPath
        {
            get
            {
                return Application.dataPath + "/../HotAssets/";
            }
        }
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

            // 公告输入（保持不变）
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("请输入本次热更公告");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            patchDes = GUILayout.TextField(patchDes, GUILayout.Width(800), GUILayout.Height(80));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            DrawInfoVersionArea();

            GUILayout.Space(10);

            DrawDonwActionButton();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制下方按钮
        /// </summary>
        private void DrawDonwActionButton()
        {
            //绘制打包按钮
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
                        //上传资源
                        UpLoadResources();
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
        /// 绘制版本信息区域
        /// </summary>
        private void DrawInfoVersionArea()
        {
            // === 版本信息区域：固定高度带框，内部可滚动 ===
            GUILayout.BeginVertical("HelpBox");

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选中模块版本信息", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            autoIncrement = EditorGUILayout.ToggleLeft("自动递增版本号", autoIncrement, GUILayout.Width(140));

            GUILayout.EndHorizontal();
            // 绘制区域
            versionListScroll = GUILayout.BeginScrollView(versionListScroll, GUILayout.Height(120));
            foreach (var item in moduleDataList)
            {
                if (!item.isBuild) continue;

                if (!moduleVersionCache.ContainsKey(item.moduleName))
                    OnModuleClicked(item);

                var info = moduleVersionCache[item.moduleName];
                GUILayout.BeginHorizontal();

                if (autoIncrement)
                {
                    //OnModuleClicked(item);
                    EditorGUILayout.LabelField(
                        $"模块 {item.moduleName}:   当前版本 {info.latestVersion}  →  新版本 {info.targetVersion}",
                        GUILayout.Width(500));
                }
                else
                {
                    EditorGUILayout.LabelField($"模块 {item.moduleName}:", GUILayout.Width(120));

                    if (info.allVersions.Count > 0)
                    {
                        string[] options = info.allVersions.Select(v => v.ToString()).ToArray();
                        int idx = Mathf.Max(0, info.allVersions.IndexOf(info.latestVersion));
                        int newIdx = EditorGUILayout.Popup(idx, options, GUILayout.Width(80));
                        info.latestVersion = info.allVersions[newIdx];
                        info.targetVersion = info.latestVersion + 1;
                        moduleVersionCache[item.moduleName] = info;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("暂无历史版本", GUILayout.Width(100));
                    }
                    info.versionInput = EditorGUILayout.TextField(info.versionInput, GUILayout.Width(80));

                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }


        /// <summary>
        /// 打包资源
        /// </summary>
        public override void BuildBundle()
        {
            base.BuildBundle();
            ModuleVersionInfo moduleVersionInfo;
            foreach (var item in moduleDataList)
            {
                if (item.isBuild)
                {
                    if (moduleVersionCache.ContainsKey(item.moduleName))
                    {
                        moduleVersionInfo = moduleVersionCache[item.moduleName];
                        //如果使用的自动递增版本号就使用 moduleVersionInfo.targetVersion 否则使用 moduleVersionInfo.versionInput
                        if (autoIncrement)
                            BuildBundleCompiler.BuildAssetBundle(item, E_BuildType.HotPatch, moduleVersionInfo.targetVersion, patchDes);
                        else
                            BuildBundleCompiler.BuildAssetBundle(item, E_BuildType.HotPatch, int.Parse(moduleVersionInfo.versionInput), patchDes);
                    }
                    else
                        Debug.LogError("Build Error Not Find moduleName!!!");
                }
            }
            foreach (var item in moduleDataList)
            {
                OnModuleClicked(item);
            }
        }


        public override void OnModuleClicked(BundleModuleData moduleData)
        {
            base.OnModuleClicked(moduleData);
            //点击模块时需要刷新面板信息 
            //更新面板中当前版本信息，和最后版本信息
            string moduleName = moduleData.moduleName;
            int latest = BuildBundleCompiler.GetLatestHotPatchVersion(moduleName);
            var allVersions = BuildBundleCompiler.GetAllHotPatchVersions(moduleName);

            ModuleVersionInfo moduleVersionInfo;

            if (!moduleVersionCache.ContainsKey(moduleName))
            {
                moduleVersionInfo = new ModuleVersionInfo
                {
                    latestVersion = latest,
                    targetVersion = autoIncrement ? ++latest : latest,
                    allVersions = allVersions
                };
                moduleVersionCache.Add(moduleName, moduleVersionInfo);
            }
            else
            {
                moduleVersionInfo = moduleVersionCache[moduleName];
                moduleVersionInfo.latestVersion = latest;
                moduleVersionInfo.targetVersion = autoIncrement ? ++latest : latest;
                moduleVersionInfo.allVersions = allVersions;
            }
        }


        /// <summary>
        /// 内嵌资源
        /// </summary>
        public void UpLoadResources()
        {
            //将最新资源全部上传
            FtpUploader.Instance.UploadDirectory(UpLoadResourcesPath, FtpConfig.Instance.host);
        }
    }
}

