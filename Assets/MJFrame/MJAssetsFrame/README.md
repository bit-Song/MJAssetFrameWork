# MJAssetFrameWork 项目文档

## 1. 项目概述

MJAssetFrameWork 是一套基于 Unity 的 **AssetBundle 热更新框架**，提供完整的资源打包、内嵌解压、版本检测、热更下载、资源加载、对象池管理等功能。框架使用 **UniTask** 实现异步流程，使用 **Newtonsoft.Json** 进行配置序列化。

### 技术栈

| 组件 | 说明 |
|------|------|
| Unity / Tuanjie 1.8.2 | 游戏引擎 |
| UniTask | 异步任务库（零分配 async/await） |
| Newtonsoft.Json | JSON 序列化/反序列化 |
| UnityWebRequest | 网络下载 |
| AES / MD5 / CRC32 | 加密与校验 |

---

## 2. 目录结构

```
Assets/
├── MJAssetsFrame/
│   ├── MJAssetsABFrame.cs          # 框架单例入口（MonoBehaviour）
│   ├── MJAssetsABFrames.cs         # 框架静态 API 门面（partial class）
│   ├── Config/                     # 配置数据类
│   │   ├── BundleSettings.cs       #   全局设置（ScriptableObject）
│   │   ├── BundleConfig.cs        #   AB 配置文件数据结构
│   │   ├── BundleModuleData.cs     #   模块配置数据结构
│   │   └── BundleModuleEnum.cs    #   模块枚举（自动生成）
│   ├── Runtime/
│   │   ├── MJABFrameBase.cs        #   单例基类
│   │   ├── BundleHot/              #   热更子系统
│   │   │   ├── IHotAssets.cs       #     热更接口
│   │   │   ├── IDecompressAssets.cs#     解压抽象类
│   │   │   ├── HotAssetsManager.cs #     热更管理器（多模块调度）
│   │   │   ├── HotAssetsModule.cs  #     单模块热更逻辑
│   │   │   ├── AssetDownLoader.cs  #     下载队列管理器
│   │   │   ├── DownLoadThread.cs   #     单文件下载器
│   │   │   ├── AssetsDecompressManager.cs # 内嵌资源解压
│   │   │   ├── HotUpdateManager.cs #     热更流程编排
│   │   │   └── HotAssetsManifest.cs#     热更清单数据结构
│   │   ├── BundleLoad/             #   资源加载子系统
│   │   │   ├── IResourcesInterface.cs #  资源接口
│   │   │   ├── AssetBundleManager.cs #   AB 加载/缓存/引用计数
│   │   │   └── ResourcesManager.cs  #     资源加载/对象池
│   │   ├── Editor/
│   │   │   └── BuildBundleCompiler.cs #  AB 打包编译器
│   │   └── Help/                   #   工具类
│   │       ├── ClassObjectPool.cs  #     泛型对象池
│   │       ├── CRC32.cs            #     CRC32 校验
│   │       ├── MD5.cs              #     MD5 校验
│   │       ├── AES.cs              #     AES 加密
│   │       └── FileHelper.cs       #     文件操作工具
│   ├── Editor/                     # 编辑器扩展
│   │   ├── BuildWindow.cs          #   主打包窗口
│   │   ├── BundleBehaviour.cs      #   打包窗口基类
│   │   ├── BuildBundleWindow.cs    #   AB 打包页
│   │   ├── BuildHotPatchWindow.cs  #   热更打包页
│   │   ├── BundleModuleConfig.cs   #   模块配置窗口
│   │   ├── BuildBundleConfigura.cs #   模块配置 ScriptableObject
│   │   ├── LeftMenuWinow.cs        #   左侧菜单树
│   │   ├── BundleTools.cs          #   枚举生成工具
│   │   └── DownContentWindow.cs   #   （空，预留）
│   ├── Example/                    # 示例代码
│   │   ├── HotAssetsWindow.cs      #   热更进度 UI
│   │   ├── UpdateTipsWindow.cs    #   更新提示弹窗
│   │   └── LoginWindow.cs          #   登录窗口示例
│   ├── Other/Editor/               # 编辑器辅助
│   │   ├── ArrayWindow.cs          #   路径数组编辑器
│   │   └── SignArrayWindow.cs      #   签名路径数组编辑器
│   └── Plugins/UniTask/            # 第三方库（不修改）
├── BundleDemo/                     # Demo 场景
│   ├── Hall/Example/
│   │   └── GameModeItem.cs         #   游戏模式入口按钮
│   └── ButtonClick.cs               #   通用按钮点击
└── Test/
    └── Test.cs                     # 测试入口
```

---

## 3. 架构分层

```
┌─────────────────────────────────────────────────────────┐
│                    业务层（Demo）                         │
│   Test.cs / LoginWindow / GameModeItem / ButtonClick     │
├─────────────────────────────────────────────────────────┤
│              框架入口层（Static API）                     │
│   MJAssetsABFrame + MJAssetsABFrames (partial class)     │
├──────────────┬──────────────┬───────────────────────────┤
│  热更子系统   │  加载子系统   │       解压子系统           │
│ HotAssets    │ Resources    │ AssetsDecompress          │
│ Manager      │ Manager      │ Manager                   │
│      ↓       │      ↓       │      ↓                    │
│ HotAssets    │ AssetBundle  │ IDecompressAssets         │
│ Module       │ Manager      │ (abstract)                │
│      ↓       │      ↓       │                           │
│ AssetDown    │ BundleItem   │                           │
│ Loader       │ AssetBundle  │                           │
│      ↓       │ Cache        │                           │
│ DownLoad     │ ClassObject  │                           │
│ Thread       │ Pool         │                           │
├──────────────┴──────────────┴───────────────────────────┤
│                    配置层                                 │
│   BundleSettings (ScriptableObject) / BundleConfig       │
│   BundleModuleData / BundleModuleEnum                    │
├─────────────────────────────────────────────────────────┤
│                    工具层                                 │
│   CRC32 / MD5 / AES / FileHelper / ClassObjectPool       │
├─────────────────────────────────────────────────────────┤
│                    编辑器层                               │
│   BuildWindow / BundleBehaviour / BuildBundleCompiler     │
│   BundleModuleConfig / BuildBundleConfigura              │
└─────────────────────────────────────────────────────────┘
```

---

## 4. 核心类说明

### 4.1 框架入口

| 类 | 说明 |
|------|------|
| `MJABFrameBase` | 单例基类（MonoBehaviour），通过 `Instance` 获取 `MJAssetsABFrame` 实例 |
| `MJAssetsABFrame` | 框架核心（partial class），持有三个子系统的引用，提供 `InitFrameWork()` 初始化 |
| `MJAssetsABFrames` | 框架静态 API 门面（partial class），所有对外暴露的方法均为 static |

**初始化方式：**
```csharp
// 在场景中的 MonoBehaviour 中调用
MJAssetsABFrame.Instance.InitFrameWork();
```

### 4.2 热更子系统

#### IHotAssets（接口）
热更子系统的抽象接口，定义了热更、版本检测、获取模块三个核心方法。

#### HotAssetsManager
- 实现 `IHotAssets` 接口
- 管理多个 `HotAssetsModule` 的并发下载调度
- 通过 `MAX_THREAD_COUNT` 限制同时下载的模块数
- `MultipleThreadBalancing()` 在模块间均衡分配下载并发数
- 使用 `WaitDownLoadModule` 队列管理等待下载的模块

#### HotAssetsModule
- 单个模块的热更逻辑单元
- 负责下载 Manifest → 对比版本 → 计算差异 → 启动下载
- 持有 `mNeedDownLoadAssetList`（需下载文件列表）和 `mAllHotAssetList`（全部热更文件）
- 管理下载进度（`mAssetDownLoadSizeM` / `mAssetsMaxSizeM`）
- 优先下载 `bundleconfig` 配置文件

#### AssetDownLoader
- 单模块内的文件下载队列管理器
- 维护 `Queue<HotFileInfo>` 下载队列和 `List<DownLoadThread>` 活跃下载列表
- 支持 `MAX_THREAD_COUNT` 个文件并行下载
- 每个文件下载完成后自动启动下一个

#### DownLoadThread
- 单个文件下载器
- 使用 `UnityWebRequest` + `DownloadHandlerFile` 异步下载
- 支持失败重试（最多 3 次）
- 下载前清理残留半成品文件

#### AssetsDecompressManager
- 继承 `IDecompressAssets` 抽象类
- 将 StreamingAssets 中的内嵌 AB 解压到 persistentDataPath
- 通过 MD5 校验避免重复解压
- 支持解压进度追踪

#### HotUpdateManager
- 热更流程编排器（业务层）
- 编排：解压 → 网络检测 → 版本检测 → 用户确认 → 热更下载 → 加载配置 → 初始化游戏环境
- 处理流量网络下的用户确认弹窗

### 4.3 资源加载子系统

#### IResourcesInterface（接口）
资源加载子系统的抽象接口，定义了实例化、加载、释放等方法。

#### AssetBundleManager
- 单例，管理 AB 的加载、缓存、引用计数和释放
- `LoadAssetBundelConfig(module)` 加载模块的配置文件（JSON 序列化为 `Dictionary<uint, BundleItem>`）
- `LoadAssetBundle(crc)` 通过 CRC 查找并加载 AB（含依赖加载）
- 支持热更路径和内嵌解压路径自动选择
- 支持 AES 加密 AB 解密加载
- 使用 `AssetBundleCache` 引用计数管理，引用归零时卸载
- 使用 `ClassObjectPool<AssetBundleCache>` 池化缓存对象

#### ResourcesManager
- 实现 `IResourcesInterface`
- 同步/异步资源加载（`LoadResource<T>` / `LoadResourceAsync<T>`）
- 同步/异步实例化（`Instantiate` / `InstantiateAsync` / `InstantiateAndLoadAsync`）
- 对象池管理（`mObjectPoolDic` + `mCacheObjectPool`）
- 支持 Editor 模式直接通过 `AssetDatabase` 加载
- 监听 `HotAssetsManager.DownLoadBundleFinish` 事件，处理资源下载完成后的回调
- 资源释放（池回收或销毁）+ AB 引用计数管理
- 深度清理 `ClearResoucesAssets(bool absoluteCleaning)`

### 4.4 配置层

| 类 | 说明 |
|------|------|
| `BundleSettings` | 全局配置（ScriptableObject），存储下载地址、热更模式、加载模式、加密设置等 |
| `BundleConfig` | AB 配置文件数据结构，包含 `List<BundleInfo>` |
| `BundleInfo` | 单个 AB 的配置信息（路径、CRC、名称、依赖） |
| `BundleModuleData` | 模块打包配置（预制体路径、文件夹路径、签名路径） |
| `BundleModuleEnum` | 模块枚举（由 `BundleTools` 自动生成） |
| `HotAssetsManifest` | 热更清单（公告、下载地址、补丁列表） |
| `HotAssetsPatch` | 热更补丁（版本号、文件列表） |
| `HotFileInfo` | 热更文件信息（包名、MD5、大小） |
| `BuiltinBundleInfo` | 内嵌 AB 信息（文件名、MD5、大小） |

### 4.5 编辑器层

| 类 | 说明 |
|------|------|
| `BuildWindow` | 主打包窗口，左侧菜单 + 右侧内容区 |
| `BundleBehaviour` | 打包窗口基类，绘制模块按钮网格 |
| `BuildBundleWindow` | AB 打包页（打包资源 + 内嵌资源） |
| `BuildHotPatchWindow` | 热更打包页（打包热更 + 上传资源 + 公告/版本） |
| `BundleModuleConfig` | 模块配置窗口（预制体包/文件夹子包/单个补丁包三种配置） |
| `BuildBundleConfigura` | 模块配置容器（ScriptableObject） |
| `BuildBundleCompiler` | AB 打包编译器（核心打包逻辑） |
| `BundleTools` | 枚举生成工具 |
| `ArrayWindow` / `SignArrayWindow` | 路径数组编辑器（ReorderableList） |

---

## 5. 核心流程

### 5.1 启动流程

```
Test.cs
  │
  ├─ Awake: MJAssetsABFrame.Instance.InitFrameWork()
  │    ├─ 创建 RecyclObjRoot（DontDestroyOnLoad）
  │    ├─ 创建 HotAssetsManager
  │    ├─ 创建 AssetsDecompressManager
  │    └─ 创建 ResourcesManager + Initlizate()
  │
  └─ Start: HotUpdateManager.Instance.HotAndPackAssets(BundleModuleEnum.Hall)
       │
       ├─ 1. 解压内嵌文件
       │    └─ AssetsDecompressManager.StartDeCompressBuiltinFile()
       │    └─ 等待 WaitDecompress()
       │
       ├─ 2. 网络检测
       │    └─ NotReachable → 弹窗提示
       │    └─ Reachable → 继续
       │
       ├─ 3. 版本检测
       │    └─ MJAssetsABFrame.CheckAssetsVersion(module)
       │    └─ 下载 Manifest → 对比版本 → 计算差异
       │
       ├─ 4. 用户确认（流量网络时）
       │    └─ UpdateTipsWindow 弹窗确认
       │
       ├─ 5. 热更下载
       │    └─ MJAssetsABFrame.HotAssets(module)
       │    └─ HotAssetsManager → HotAssetsModule → AssetDownLoader → DownLoadThread
       │
       ├─ 6. 加载配置
       │    └─ AssetBundleManager.Instance.LoadAssetBundelConfig(module)
       │
       └─ 7. 初始化游戏环境
            └─ InitGameEnv() → 加载本地资源/配置/场景
```

### 5.2 热更下载流程

```
HotAssetsManager.HotAssets(module)
  │
  ├─ 并发数 < MAX_THREAD_COUNT?
  │    ├─ 是 → 加入下载列表 → MultipleThreadBalancing()
  │    │       → HotAssetsModule.StartHotAssets()
  │    │
  │    └─ 否 → 进入等待队列 → await tcs.Task
  │              → 被唤醒后重新检查并发数
  │
  └─ HotAssetsModule.StartHotAssets()
       ├─ CheckAssetsVersion()
       │    ├─ DownLoadHotAssetsManifest() — UnityWebRequest 下载 JSON 清单
       │    ├─ CheckModuleAssetsIsHot() — 对比本地/服务端补丁版本
       │    └─ ComputeNeedHotAssetsList() — MD5 校验，计算需下载文件
       │
       └─ StartDownLoadHotAssets()
            ├─ 优先排列 bundleconfig 文件
            ├─ 创建 AssetDownLoader
            └─ AssetDownLoader.StartThreadDownLoadQueue()
                 │
                 ├─ 启动 MAX_THREAD_COUNT 个 DownLoadThread
                 │
                 └─ DownLoadThread.StartDownLoad()
                      ├─ UnityWebRequest.Get(url)
                      ├─ DownloadHandlerFile 写盘
                      ├─ 失败重试（最多 3 次）
                      │
                      ├─ 成功 → DownLoadAssetBundleSuccess()
                      │           → 启动下一个下载
                      │
                      └─ 全部完成 → DownLoadAssetBundleFinish()
                                     → 拷贝 Manifest → 通知 ResourcesManager
```

### 5.3 资源加载流程

```
MJAssetsABFrame.Instantiate(path)
  │
  ├─ ResourcesManager.Instantiate(path)
  │    ├─ CRC 计算
  │    ├─ 对象池查找 → 命中则直接返回
  │    ├─ 未命中 → LoadResource<GameObject>(path)
  │    │    ├─ Editor 模式 → AssetDatabase.LoadAssetAtPath
  │    │    └─ AB 模式 → AssetBundleManager.LoadAssetBundle(crc)
  │    │         ├─ 查 mAllBundleAssetDic 获取 BundleItem
  │    │         ├─ 加载主 AB（LoadFromFile / LoadFromMemory[加密]）
  │    │         ├─ 加载依赖 AB
  │    │         └─ 引用计数 +1
  │    │
  │    └─ 实例化 → CacheObject 记录 → 存入 mAllObjectDic
  │
  └─ 释放 → ResourcesManager.Release(obj)
       ├─ destroy=false → 回收到对象池 → 挂到 RecyclObjRoot
       └─ destroy=true  → Destroy → 释放 AB 引用 → 引用归零则卸载 AB
```

### 5.4 打包流程

```
BuildWindow (菜单: MJFrame/AssetBundle)
  │
  ├─ 选择模块（双击进入 BundleModuleConfig 配置）
  │    ├─ 预制体包路径（每个 Prefab 独立打成一个 AB）
  │    ├─ 文件夹子包路径（每个子文件夹打成一个 AB）
  │    └─ 单个补丁包路径（指定文件夹打成一个 AB）
  │
  ├─ AssetBundle 打包页 → "打包资源"
  │    └─ BuildBundleCompiler.BuildAssetBundle(data)
  │         ├─ BuileAllForlder() — 处理签名文件夹
  │         ├─ BuildRootSubForlder() — 处理子文件夹
  │         ├─ BuildAllPrefabs() — 处理预制体（含依赖分析）
  │         ├─ ModifyAllFileBundleName() — 设置 AB 名称
  │         ├─ WriteAssetBundleConfig() — 生成 JSON 配置
  │         ├─ BuildPipeline.BuildAssetBundles() — Unity 打包
  │         ├─ DeleteAllBundleManifestFile() — 清理 .manifest
  │         └─ EncryptAllBundle() — AES 加密（可选）
  │
  ├─ "内嵌资源" → CopyBundleToStramingAssets()
  │    └─ 复制到 StreamingAssets + 生成 <module>info.json
  │
  └─ HotPatch 打包页 → "打包热更"
       └─ BuildAssetBundle(data, E_BuildType.HotPatch, version, notice)
            └─ GeneratorHotAssets()
                 └─ GeneralHotAssetsManifest() — 生成热更清单 JSON
```

---

## 6. 关键设计

### 6.1 模块化设计

框架以 `BundleModuleEnum` 为模块划分，每个模块独立管理：
- 独立的热更清单（`<ModuleEnum>AssetsHotManifest.json`）
- 独立的 AB 配置文件（`<module>bundleconfig`）
- 独立的下载队列和进度
- 独立的对象池和缓存

### 6.2 路径体系

| 路径 | 用途 |
|------|------|
| `StreamingAssets/AssetBundle/<Module>/` | 内嵌 AB（随包发布） |
| `persistentDataPath/DeCompressAssets/<Module>/` | 解压后的内嵌 AB |
| `persistentDataPath/HotAssets/<Module>/` | 热更下载的 AB |
| `persistentDataPath/Server<Module>AssetsHotManifest.json` | 服务端清单缓存 |
| `persistentDataPath/Local<Module>AssetsHotManifest.json` | 本地清单记录 |

AB 加载时优先使用热更路径，其次使用解压路径。

### 6.3 下载并发控制

两级并发限制：
1. **模块级**：`HotAssetsManager.MAX_THREAD_COUNT = 3`，限制同时下载的模块数
2. **文件级**：`AssetDownLoader.MAX_THREAD_COUNT = 3`，限制单模块内同时下载的文件数

`MultipleThreadBalancing()` 在模块间均衡分配并发数（如 3 模块各分 1 个，2 模块分 2+1）。

### 6.4 引用计数

`AssetBundleCache` 使用引用计数管理 AB 生命周期：
- 加载 AB 时 `refereaceCount++`
- 释放 AB 时 `refereaceCount--`
- 引用归零时 `Unload(unLoad)` 并回收到 `ClassObjectPool`

### 6.5 对象池

两层对象池：
- `ClassObjectPool<CacheObject>` — 池化 `CacheObject` 实例（减少 GC）
- `mObjectPoolDic<uint, List<CacheObject>>` — 按资源 CRC 分组的 GameObject 池

### 6.6 加密

通过 `BundleSettings.bundleEnctypt.isEncrypt` 控制是否加密：
- 打包时 `BuildBundleCompiler.EncryptAllBundle()` 使用 AES 加密
- 加载时 `AssetBundleManager.LoadAssetBundle()` 使用 AES 解密后 `LoadFromMemory`

---

## 7. 对外 API 速查

### 热更相关

```csharp
// 检测版本
CheckVersionResult result = await MJAssetsABFrame.CheckAssetsVersion(BundleModuleEnum.Hall);

// 开始热更
await MJAssetsABFrame.HotAssets(BundleModuleEnum.Hall);

// 获取热更模块（读取进度）
HotAssetsModule module = MJAssetsABFrame.GetHotAssetsModule(BundleModuleEnum.Hall);

// 解压内嵌文件
IDecompressAssets decompress = MJAssetsABFrame.StartDeCompressBuiltinFile(BundleModuleEnum.Hall);
await MJAssetsABFrame.WaitDeCompress();
float progress = MJAssetsABFrame.GetDeCompressProgress();
```

### 资源加载

```csharp
// 同步实例化
GameObject obj = MJAssetsABFrame.Instantiate("Assets/Path/Prefab.prefab");

// 异步实例化
GameObject obj = await MJAssetsABFrame.InstantiateAsync("Assets/Path/Prefab.prefab");

// 等待下载完成后实例化
GameObject obj = await MJAssetsABFrame.InstantiateAndLoadAsync("Assets/Path/Prefab.prefab");

// 加载资源
Sprite sprite = MJAssetsABFrame.LoadSprite("Assets/Path/Image");
Texture tex = MJAssetsABFrame.LoadTexture("Assets/Path/Texture");
AudioClip audio = MJAssetsABFrame.LoadAudio("Assets/Path/Audio");
TextAsset text = MJAssetsABFrame.LoadTextAsset("Assets/Path/Text");

// 图集
Sprite s = MJAssetsABFrame.LoadAtlasSprite("Assets/Path/Atlas", "SpriteName");

// 异步加载
Texture tex = await MJAssetsABFrame.LoadTextureAsync("Assets/Path/Texture");
Sprite s = await MJAssetsABFrame.LoadSpriteAsync("Assets/Path/Image", imageComponent);
```

### 资源释放

```csharp
// 回收到对象池
MJAssetsABFrame.Release(gameObject);

// 彻底销毁
MJAssetsABFrame.Release(gameObject, destroy: true);
MJAssetsABFrame.Release(texture); // Texture 直接卸载

// 清理所有资源
MJAssetsABFrame.ClearResoucesAssets(true);  // 深度清理
MJAssetsABFrame.ClearResoucesAssets(false); // 仅清理对象池

// 预加载
MJAssetsABFrame.PreLoadObj("Assets/Path/Prefab.prefab", count: 5);
MJAssetsABFrame.PreLoadResource<Sprite>("Assets/Path/Image");
```

### 编辑器菜单

| 菜单路径 | 功能 |
|---------|------|
| `MJFrame/AssetBundle` | 打开 AB 打包窗口 |
| `MJFrame/GeneratorModuleEnum` | 重新生成模块枚举 |

---

## 8. 类图

类图文件位于 `Assets/MJAssetsFrame/ClassDiagram.puml`，使用 PlantUML 格式。

可用 [PlantUML Web Server](http://www.plantuml.com/plantuml) 或 VS Code PlantUML 插件渲染查看。

---

## 9. 扩展指南

### 新增资源模块

1. 在打包窗口中点击 `+` 添加模块
2. 配置模块名称和资源路径
3. 保存配置
4. 执行菜单 `MJFrame/GeneratorModuleEnum` 重新生成枚举
5. 打包 AB / 内嵌资源

### 新增打包平台

修改 `BundleSettings.cs` 中 `buildTarget` 枚举（已包含全平台）。

### 自定义热更流程

继承 `HotUpdateManager` 或修改其 `HotAndPackAssets` 方法，自定义流程编排。

### 自定义资源加载

实现 `IResourcesInterface` 接口，替换 `ResourcesManager`。

### 自定义解压

继承 `IDecompressAssets` 抽象类，替换 `AssetsDecompressManager`。
