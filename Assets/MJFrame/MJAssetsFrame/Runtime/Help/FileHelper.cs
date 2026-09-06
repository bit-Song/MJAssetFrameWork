using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MJ.AssetFrameWork.ABFrame
{
    public class FileHelper
    {
        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="folderPath"></param>
        public static void DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                string[] pathsArr = Directory.GetFiles(folderPath, "*");
                foreach (var path in pathsArr)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                Directory.Delete(folderPath);
            }
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        public static void WriteFile(string filePath, byte[] data)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileStream stream = File.Create(filePath);
            stream.Write(data, 0, data.Length);
            stream.Dispose();
            stream.Close();
        }
    }

}
