using MJ.AssetFrameWork.ABFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class ArrayWindow
    {
        public List<string> pathList = new List<string>();
        private ReorderableList _reorderableList;
        public string title;

        public List<string> FileInfoList
        {
            get { return pathList; }
            set
            {
                pathList.Clear();
                pathList.AddRange(value);
            }
        }
        public void Init(string title = "")
        {
            this.title = title;
            _reorderableList = new ReorderableList(pathList, typeof(BundleFileInfo),
                true, true, true, true);

            _reorderableList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, this.title);
            };

            //每行画两个字段
            _reorderableList.drawElementCallback = (rect, index, active, focused) =>
            {
                // 右半边：路径选择
                Rect pathRect = new Rect(rect.x + rect.width * 0.42f, rect.y + 2, rect.width * 0.58f - 30, rect.height - 4);
                pathList[index]= EditorGUI.TextField(pathRect, pathList[index]);

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
                            pathList[index] = path;
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

            _reorderableList.onAddCallback = (list) =>
            {
                pathList.Add("");
            };
        }

        public void GetWindow()
        {
            _reorderableList.DoLayoutList();
        }
    }
}

