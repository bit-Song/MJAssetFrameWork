using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public enum E_MenuSelected
    {
        Root,
        Build,
        AssetBundle,
        HotPatch,
        BundleSetting
    }

    public class LeftMenuWinow : TreeView
    {
        public BuildWindow buildWindow;
        public E_MenuSelected CurrentSelected { get; private set; } = E_MenuSelected.AssetBundle;

        public LeftMenuWinow(TreeViewState state) : base(state)
        {
            Reload();
            SetDefaultSelection();
        }

        protected override TreeViewItem BuildRoot()
        {
            int id = 0;
            var root = new TreeViewItem(id++, -1, "Root");
            var build = new TreeViewItem(id++, 0, "Build");
            build.icon = EditorGUIUtility.IconContent("aboutwindow.mainheader").image as Texture2D;
            var assetBundle = new TreeViewItem(id++, 1, "AssetBundle");
            assetBundle.icon = EditorGUIUtility.IconContent("BuildSettings.Editor").image as Texture2D;
            var hotPatch = new TreeViewItem(id++, 1, "HotPatch");
            hotPatch.icon = EditorGUIUtility.IconContent("BuildSettings.Editor").image as Texture2D;
            var bundleSetting = new TreeViewItem(id++, 0, "Bundle Setting");
            bundleSetting.icon = EditorGUIUtility.IconContent("_Popup@2x").image as Texture2D;

            root.AddChild(build);
            root.AddChild(bundleSetting);
            build.AddChild(assetBundle);
            build.AddChild(hotPatch);
            //设置默认节点处于打开状态
            SetExpanded((int)E_MenuSelected.Build, true);
            return root;
        }

        /// <summary>
        /// 修改选择项时
        /// </summary>
        /// <param name="selectedIds"></param>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            if (selectedIds.Count > 0)
            {
                //设置当前选中的页面
                CurrentSelected = (E_MenuSelected)(selectedIds[0]);
            }
        }

        /// <summary>
        /// 设置默认打开节点
        /// </summary>
        private void SetDefaultSelection()
        {
            SetSelection(new List<int> { (int)E_MenuSelected.AssetBundle });

        }
    }
}
