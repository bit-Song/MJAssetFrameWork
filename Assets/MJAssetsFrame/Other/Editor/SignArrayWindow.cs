using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class SignArrayWindow
    {
        public List<BundleFileInfo> fileInfoList = new List<BundleFileInfo>();
        private ReorderableList reorderableList;
        public string title;

        public List<BundleFileInfo> FileInfoList
        {
            get { return fileInfoList; }
            set
            {
                fileInfoList.Clear();
                fileInfoList.AddRange(value);
            }
        }
        public void Init(string title = "")
        {
            this.title = title;
            reorderableList = new ReorderableList(fileInfoList, typeof(BundleFileInfo),
                true, true, true, true);

            reorderableList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, this.title);
            };

            //每行画两个字段
            reorderableList.drawElementCallback = (rect, index, active, focused) =>
            {
                var info = fileInfoList[index];

                // 左半边：AB Name
                Rect nameRect = new Rect(rect.x, rect.y + 2, rect.width * 0.4f, rect.height - 4);
                info.abName = EditorGUI.TextField(nameRect, info.abName);

                // 右半边：路径选择
                Rect pathRect = new Rect(rect.x + rect.width * 0.42f, rect.y + 2, rect.width * 0.58f - 30, rect.height - 4);
                info.bundlePath = EditorGUI.TextField(pathRect, info.bundlePath);

                // 最右边：选择按钮
                Rect btnRect = new Rect(rect.x + rect.width - 28, rect.y + 2, 25, rect.height - 4);
                if (GUI.Button(btnRect, "···"))
                {
                    string path = EditorUtility.OpenFolderPanel("选择文件夹", "", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        // 绝对路径转相对路径
                        if (path.StartsWith(Application.dataPath))
                        {
                            path = "Assets" + path.Substring(Application.dataPath.Length);
                            info.bundlePath = path;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("提示", "选择的文件夹不在当前工程目录下", "确认");
                            Debug.LogWarning("选择的文件夹不在当前工程目录下: " + path);
                            return;
                        }
                    }
                }
            };

            reorderableList.onAddCallback = (list) =>
            {
                fileInfoList.Add(new BundleFileInfo());
            };
        }

        public void GetWindow()
        {
            reorderableList.DoLayoutList();
        }
    }
}

