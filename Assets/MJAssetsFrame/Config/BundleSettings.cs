using System;
using UnityEngine;
namespace MJ.AssetFrameWork.ABFrame
{

    /// <summary>
    /// AssetBundle热更模式
    /// </summary>
    public enum E_BundleHotEnum
    {
        //不热更
        NoHot,
        //热更
        Hot,
    }



    [CreateAssetMenu(menuName = "ScriptableObject/AssetFrame/AssetsBundleSettings", fileName = "AssetsBundleSettings")]
    public class BundleSettings : ScriptableObject
    {
        private static BundleSettings instance;
        public static BundleSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<BundleSettings>("AssetsBundleSettings");
                }
                return instance;
            }
        }

        //AssetBundle下载地址
        [Header("AssetBundle下载地址")]
        public string AssetDownLoadUrl;

        [Header("资源热更模式")]
        public E_BundleHotEnum bundleHotType;

        [Header("最大下载线程数量")]
        public int MAX_THREAD_COUNT;

        //打包设置
        [Header("是否加密AssetBundle")]
        public BundleEncryptToggle bundleEnctypt = new BundleEncryptToggle();

        [Header("资源打包平台")]
        public BuildTarget buildTarget;

        [Header("AssetBundle打包格式")]
        public BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;

        [Header("AssetBundle文件名后缀")]
        public string BundlePostfix = ".unity";




        /// <summary>
        /// AssetBundle解压路径
        /// </summary>
        private string BuiltinDeComprssPath { get { return Application.persistentDataPath + "/DeCompressAssets/"; } }

        /// <summary>
        /// AssetBundle内嵌文件路径
        /// </summary>
        private string BuiltinAssetsPath { get { return Application.streamingAssetsPath + "/AssetBundle/"; } }

        /// <summary>
        /// 热更文件储存路径
        /// </summary>
        private string HotAssetsPath { get { return Application.persistentDataPath + "/HotAssets/"; } }

        /// <summary>
        /// 获取解压文件路径
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <returns></returns>
        public string GetAssetsDeCompressPath(BundleModuleEnum bundleModuleEnum)
        {
            return BuiltinDeComprssPath + bundleModuleEnum + "/";
        }

        /// <summary>
        /// 获取资源内嵌的路径
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <returns></returns>
        public string GetAssetsBuiltinBundlePath(BundleModuleEnum bundleModuleEnum)
        {
            return BuiltinAssetsPath + bundleModuleEnum + "/";
        }

        /// <summary>
        /// 获取热更文件存储路径
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <returns></returns>
        public string GetHotAssetsPath(BundleModuleEnum bundleModuleEnum)
        {
            return HotAssetsPath + bundleModuleEnum + "/";
        }
    }

    /// <summary>
    /// 加密标签
    /// </summary>
    [System.Serializable]
    public class BundleEncryptToggle
    {
        //是否加密
        public bool isEncrypt;
        //密钥
        public string encryptKey;

    }

    /// <summary>
    /// 打包的目标平台
    /// </summary>
    public enum BuildTarget
    {
        //
        // 摘要:
        //     Build a macOS standalone.
        StandaloneOSX = 2,
        [Obsolete("Use StandaloneOSX instead (UnityUpgradable) -> StandaloneOSX", true)]
        StandaloneOSXUniversal = 3,
        //
        // 摘要:
        //     Build a macOS Intel 32-bit standalone. (This build target is deprecated)
        [Obsolete("StandaloneOSXIntel has been removed in 2017.3")]
        StandaloneOSXIntel = 4,
        //
        // 摘要:
        //     Build a Windows standalone.
        StandaloneWindows = 5,
        //
        // 摘要:
        //     Build a web player. (This build target is deprecated. Building for web player
        //     will no longer be supported in future versions of Unity.)
        [Obsolete("WebPlayer has been removed in 5.4", true)]
        WebPlayer = 6,
        //
        // 摘要:
        //     Build a streamed web player.
        [Obsolete("WebPlayerStreamed has been removed in 5.4", true)]
        WebPlayerStreamed = 7,
        //
        // 摘要:
        //     Build an iOS player.
        iOS = 9,
        [Obsolete("PS3 has been removed in >=5.5")]
        PS3 = 10,
        [Obsolete("XBOX360 has been removed in 5.5")]
        XBOX360 = 11,
        //
        // 摘要:
        //     Build an Android .apk standalone app.
        Android = 13,
        //
        // 摘要:
        //     Build a Linux standalone.
        [Obsolete("StandaloneLinux has been removed in 2019.2")]
        StandaloneLinux = 17,
        //
        // 摘要:
        //     Build a Windows 64-bit standalone.
        StandaloneWindows64 = 19,
        //
        // 摘要:
        //     Build to WebGL platform.
        WebGL = 20,
        //
        // 摘要:
        //     Build an Windows Store Apps player.
        WSAPlayer = 21,
        //
        // 摘要:
        //     Build a Linux 64-bit standalone.
        StandaloneLinux64 = 24,
        //
        // 摘要:
        //     Build a Linux universal standalone.
        [Obsolete("StandaloneLinuxUniversal has been removed in 2019.2")]
        StandaloneLinuxUniversal = 25,
        [Obsolete("Use WSAPlayer with Windows Phone 8.1 selected")]
        WP8Player = 26,
        //
        // 摘要:
        //     Build a macOS Intel 64-bit standalone. (This build target is deprecated)
        [Obsolete("StandaloneOSXIntel64 has been removed in 2017.3")]
        StandaloneOSXIntel64 = 27,
        [Obsolete("BlackBerry has been removed in 5.4")]
        BlackBerry = 28,
        [Obsolete("Tizen has been removed in 2017.3")]
        Tizen = 29,
        [Obsolete("PSP2 is no longer supported as of Unity 2018.3")]
        PSP2 = 30,
        //
        // 摘要:
        //     Build a PS4 Standalone.
        PS4 = 31,
        [Obsolete("PSM has been removed in >= 5.3")]
        PSM = 32,
        //
        // 摘要:
        //     Build a Xbox One Standalone.
        XboxOne = 33,
        [Obsolete("SamsungTV has been removed in 2017.3")]
        SamsungTV = 34,
        //
        // 摘要:
        //     Build to Nintendo 3DS platform.
        [Obsolete("Nintendo 3DS support is unavailable since 2018.1")]
        N3DS = 35,
        [Obsolete("Wii U support was removed in 2018.1")]
        WiiU = 36,
        //
        // 摘要:
        //     Build to Apple's tvOS platform.
        tvOS = 37,
        //
        // 摘要:
        //     Build a Nintendo Switch player.
        Switch = 38,
        [Obsolete("Lumin has been removed in 2022.2")]
        Lumin = 39,
        //
        // 摘要:
        //     Build a Stadia standalone.
        Stadia = 40,
        //
        // 摘要:
        //     Build a CloudRendering standalone.
        [Obsolete("CloudRendering is deprecated, please use LinuxHeadlessSimulation (UnityUpgradable) -> LinuxHeadlessSimulation", false)]
        CloudRendering = 41,
        //
        // 摘要:
        //     Build a LinuxHeadlessSimulation standalone.
        LinuxHeadlessSimulation = 41,
        [Obsolete("GameCoreScarlett is deprecated, please use GameCoreXboxSeries (UnityUpgradable) -> GameCoreXboxSeries", false)]
        GameCoreScarlett = 42,
        GameCoreXboxSeries = 42,
        GameCoreXboxOne = 43,
        //
        // 摘要:
        //     Build to PlayStation 5 platform.
        PS5 = 44,
        //
        // 摘要:
        //     Build to Embedded Linux platform.
        EmbeddedLinux = 45,
        //
        // 摘要:
        //     Build to QNX platform.
        QNX = 46,
        //
        // 摘要:
        //     Build to MiniGame platform. Identical to "WeixinMiniGame".
        MiniGame = 47,
        //
        // 摘要:
        //     WeixinMiniGame is deprecated.
        WeixinMiniGame = 47,
        //
        // 摘要:
        //     Build an OpenHarmony .hap standalone app.
        OpenHarmony = 48,
        //
        // 摘要:
        //     Build an HMI Android .apk standalone app.
        HMIAndroid = 49,
        ArmLinux = 50,
        ArmLinuxServer = 51,
        //
        // 摘要:
        //     Build a visionOS player.
        VisionOS = 52,
        //
        // 摘要:
        //     Build to PlayableAds platform.
        PlayableAds = 53,
        //
        // 摘要:
        //     OBSOLETE: Use iOS. Build an iOS player.
        [Obsolete("Use iOS instead (UnityUpgradable) -> iOS", true)]
        iPhone = -1,
        [Obsolete("BlackBerry has been removed in 5.4")]
        BB10 = -1,
        [Obsolete("Use WSAPlayer instead (UnityUpgradable) -> WSAPlayer", true)]
        MetroPlayer = -1,
        NoTarget = -2
    }

    /// <summary>
    /// AssetBundle压缩方式
    /// </summary>
    public enum BuildAssetBundleOptions
    {
        //
        // 摘要:
        //     Build assetBundle without any special option.
        None = 0,
        //
        // 摘要:
        //     Don't compress the data when creating the AssetBundle.
        UncompressedAssetBundle = 1,
        //
        // 摘要:
        //     Includes all dependencies.
        [Obsolete("This has been made obsolete. It is always enabled in the new AssetBundle build system introduced in 5.0.")]
        CollectDependencies = 2,
        //
        // 摘要:
        //     Forces inclusion of the entire asset.
        [Obsolete("This has been made obsolete. It is always disabled in the new AssetBundle build system introduced in 5.0.")]
        CompleteAssets = 4,
        //
        // 摘要:
        //     Do not include type information within the AssetBundle.
        DisableWriteTypeTree = 8,
        //
        // 摘要:
        //     Builds an asset bundle using a hash for the id of the object stored in the asset
        //     bundle.
        [Obsolete("This has been made obsolete. It is always enabled in the new AssetBundle build system introduced in 5.0.")]
        DeterministicAssetBundle = 0x10,
        //
        // 摘要:
        //     Force rebuild the assetBundles.
        ForceRebuildAssetBundle = 0x20,
        //
        // 摘要:
        //     Ignore the type tree changes when doing the incremental build check.
        IgnoreTypeTreeChanges = 0x40,
        //
        // 摘要:
        //     Append the hash to the assetBundle name.
        AppendHashToAssetBundleName = 0x80,
        //
        // 摘要:
        //     Use chunk-based LZ4 compression when creating the AssetBundle.
        ChunkBasedCompression = 0x100,
        //
        // 摘要:
        //     Do not allow the build to succeed if any errors are reporting during it.
        StrictMode = 0x200,
        //
        // 摘要:
        //     Do a dry run build.
        DryRunBuild = 0x400,
        //
        // 摘要:
        //     Disables Asset Bundle LoadAsset by file name.
        DisableLoadAssetByFileName = 0x1000,
        //
        // 摘要:
        //     Disables Asset Bundle LoadAsset by file name with extension.
        DisableLoadAssetByFileNameWithExtension = 0x2000,
        //
        // 摘要:
        //     Removes the Unity Version number in the Archive File & Serialized File headers
        //     during the build.
        AssetBundleStripUnityVersion = 0x8000,
        //
        // 摘要:
        //     Use the content of the asset bundle to calculate the hash. Enabling this flag
        //     is recommended to improve incremental build results, but it will force a rebuild
        //     of all existing AssetBundles that have been built without the flag.
        UseContentHash = 0x10000,
        //
        // 摘要:
        //     Use when AssetBundle dependencies need to be calculated recursively, such as
        //     when you have a dependency chain of matching typed Scriptable Objects.
        RecurseDependencies = 0x20000,
        //
        // 摘要:
        //     Use to prevent duplicating a texture when it is referenced in multiple bundles.
        //     This would primarily happen with particle systems. The new behavior does not
        //     duplicate the texture if the sprite does not belong to an atlas. Using this flag
        //     is the desired behavior, but is not set by default for backwards compatability
        //     reasons.
        StripUnatlasedSpriteCopies = 0x40000,
        //
        // 摘要:
        //     Enable Protection to AssetBundle with Encryption.
        EnableProtection = 0x80000,
        //
        // 摘要:
        //     Pack Virtual Geometry cluster when creating the AssetBundle.
        PackClustersIntoAssetBundle = 0x100000,
        //
        // 摘要:
        //     Build AssetBundle archive with Multithead.
        MultithreadBuildArchive = 0x200000,
        //
        // 摘要:
        //     Includes all dependencies based on AssetDatabase.
        CollectFileDependencies = 0x400000,
        //
        // 摘要:
        //     Enable AssetBundle compatibility for Instant Asset.
        EnableInstantAsset = 0x800000,
        //
        // 摘要:
        //     Build AssetBundles using multiple processes to improve build performance.
        MultiProcessAssetBundleBuilding = 0x1000000
    }


}

