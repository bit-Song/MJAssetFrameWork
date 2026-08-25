using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class BuildWindow : EditorWindow
    {
        public BundleBehaviour bundleBehaviour;
        public BuildBundleWindow buildBundleWindow = new BuildBundleWindow();
        public BuildHotPatchWindow hotBundleWindow = new BuildHotPatchWindow();

        //左侧菜单显示
        private TreeViewState _treeState;
        private LeftMenuWinow _leftMenuWindow;

        [MenuItem("MJFrame/AssetBundle")]
        public static void ShowAssetBundleWindow()
        {
            BuildWindow buildWindow = GetWindow<BuildWindow>();
            buildWindow.minSize = new Vector2(985, 612);
            buildWindow.maxSize = new Vector2(985, 612);

            //Debug.Log("Hall Version:" + BuildBundleCompiler.GetLatestHotPatchVersion(BundleModuleEnum.Game));


        }



        private void OnEnable()
        {
            _treeState = new TreeViewState();
            _leftMenuWindow = new LeftMenuWinow(_treeState);
            _leftMenuWindow.buildWindow = this;

            buildBundleWindow.Initzation();
            hotBundleWindow.Initzation();
        }

        private Vector2 _rightScroll;

        private void OnGUI()
        {
            ShowBehaviourWindow(_leftMenuWindow.CurrentSelected);
            using (new EditorGUILayout.HorizontalScope())
            {
                // 左侧菜单：固定180宽度，撑满高度
                Rect leftRect = GUILayoutUtility.GetRect(180f, position.height, GUILayout.Width(180f), GUILayout.ExpandHeight(true));
                _leftMenuWindow.OnGUI(leftRect);

                // 右侧内容区域
                using (new EditorGUILayout.VerticalScope())
                {
                    //_rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
                    switch (_leftMenuWindow.CurrentSelected)
                    {
                        case E_MenuSelected.AssetBundle:
                        case E_MenuSelected.HotPatch:
                            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
                            ////重绘时刷新数据 防止外部修改不能实时更新
                            bundleBehaviour?.Initzation();
                            bundleBehaviour.OGUI();
                            EditorGUILayout.EndScrollView();
                            bundleBehaviour.DrawBuildButtons();
                            break;
                        case E_MenuSelected.BundleSetting:
                            //显示BundleSettings面板内容
                            Editor.CreateEditor(BundleSettings.Instance).OnInspectorGUI();
                            break;
                        case E_MenuSelected.UpLoadSetting:
                            Editor.CreateEditor(FtpConfig.Instance).OnInspectorGUI();
                            break;
                    }
                }
            }
        }


        public void ShowBehaviourWindow(E_MenuSelected e_MenuSelected)
        {
            switch (e_MenuSelected)
            {
                case E_MenuSelected.AssetBundle:
                    bundleBehaviour = buildBundleWindow;
                    break;
                case E_MenuSelected.HotPatch:
                    bundleBehaviour = hotBundleWindow;
                    break;
                case E_MenuSelected.Setting:
                    break;
            }
        }

    }
}
