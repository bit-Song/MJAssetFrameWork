using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine.Networking;

namespace MJ.AssetFrameWork.ABFrame
{
    /// <summary>
    /// 限速下载处理器
    /// </summary>
    public class ThrottleDownloadHandler : DownloadHandlerScript
    {
        private FileStream m_FileStream;
        private readonly float m_MaxBytesPerSecond;
        private readonly Stopwatch m_Stopwatch;
        private long m_TotalBytes;

        /// <summary>
        /// 是否限速（false时全速下载）
        /// </summary>
        public static bool EnableThrottle = false;

        /// <summary>
        /// 限速 KB/s
        /// </summary>
        public static float ThrottleKBPerSecond = 100f;

        public ThrottleDownloadHandler(string savePath) : base()
        {
            m_FileStream = File.Create(savePath);
            m_Stopwatch = Stopwatch.StartNew();
            m_MaxBytesPerSecond = ThrottleKBPerSecond * 1024f;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0)
                return false;

            m_FileStream.Write(data, 0, dataLength);
            m_TotalBytes += dataLength;

            // 不限速时直接返回
            if (!EnableThrottle)
                return true;

            // 计算当前已用时间下应该下载的最大字节数
            double elapsedSeconds = m_Stopwatch.Elapsed.TotalSeconds;
            double allowedBytes = m_MaxBytesPerSecond * elapsedSeconds;

            // 如果实际下载量超过允许量，sleep 等待
            if (m_TotalBytes > allowedBytes)
            {
                double waitTime = (m_TotalBytes - allowedBytes) / m_MaxBytesPerSecond;
                Thread.Sleep((int)(waitTime * 1000));
            }

            return true;
        }

        protected override void CompleteContent()
        {
            m_FileStream?.Flush();
            m_FileStream?.Close();
            m_FileStream = null;
        }

        public override void Dispose()
        {
            m_FileStream?.Close();
            m_FileStream = null;
            base.Dispose();
        }
    }
}