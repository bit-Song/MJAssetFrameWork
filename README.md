# MJAssetFrameWork

一套基于 Unity / Tuanjie 引擎的 **AssetBundle 热更新框架**，提供资源打包、内嵌解压、版本检测、热更下载、资源加载、对象池管理等完整功能链。

## 功能特性

- **模块化热更** — 按业务模块独立打包、独立热更、独立下载队列
- **多线程下载** — 模块级 + 文件级两级并发控制，线程负载均衡
- **资源加密** — AES 加密 AssetBundle，运行时解密加载
- **对象池** — GameObject 级 + 类实例级两层对象池，减少 GC
- **引用计数** — AssetBundle 引用计数管理，自动卸载闲置资源
- **编辑器工具** — 可视化打包窗口，模块配置、AB 打包、热更打包一站式完成
- **UniTask 异步** — 全链路 async/await，零分配异步流程
- **断点续传** — 文件级 MD5 校验，已下载文件跳过重下

## 技术栈

| 组件 | 说明 |
|------|------|
| Unity / Tuanjie 1.8.2 | 游戏引擎 |
| UniTask | 异步任务库 |
| Newtonsoft.Json | JSON 序列化 |
| AES / MD5 / CRC32 | 加密与校验 |

## 快速开始

### 1. 初始化框架

```csharp
void Awake()
{
    MJAssetsABFrame.Instance.InitFrameWork();
}
```

### 2. 热更检测与下载

```csharp
// 检测版本
CheckVersionResult result = await MJAssetsABFrame.CheckAssetsVersion(BundleModuleEnum.Hall);

if (result.isHot)
{
    // 开始热更
    await MJAssetsABFrame.HotAssets(BundleModuleEnum.Hall);
}

// 加载资源
GameObject obj = MJAssetsABFrame.Instantiate("Assets/BundleDemo/Hall/Prefab/HallWindow");
```

### 3. 异步加载资源

```csharp
// 等待下载完成后实例化（资源可能还在热更中）
GameObject obj = await MJAssetsABFrame.InstantiateAndLoadAsync("Assets/Path/Prefab");

// 加载图片 / 音频 / 图集
Sprite sprite = MJAssetsABFrame.LoadSprite("Assets/Path/Image");
Texture tex = MJAssetsABFrame.LoadTexture("Assets/Path/Texture");
AudioClip audio = MJAssetsABFrame.LoadAudio("Assets/Path/Audio");
Sprite s = MJAssetsABFrame.LoadAtlasSprite("Assets/Path/Atlas", "SpriteName");
```

### 4. 释放资源

```csharp
// 回收到对象池（内存不释放，下次直接复用）
MJAssetsABFrame.Release(gameObject);

// 彻底销毁（释放 AssetBundle）
MJAssetsABFrame.Release(gameObject, destroy: true);

// 清理所有资源
MJAssetsABFrame.ClearResoucesAssets(true);  // 深度清理
MJAssetsABFrame.ClearResoucesAssets(false);  // 仅清理对象池
```

### 5. 打包 AssetBundle

菜单 `MJFrame → AssetBundle` 打开打包窗口，配置模块后一键打包。

## 目录结构

```
Assets/
├── MJAssetsFrame/              # 框架核心
│   ├── MJAssetsABFrame.cs      #   框架单例入口
│   ├── MJAssetsABFrames.cs     #   静态 API 门面
│   ├── Config/                 #   配置数据类
│   ├── Runtime/
│   │   ├── BundleHot/         #     热更子系统
│   │   ├── BundleLoad/         #     资源加载子系统
│   │   └── Help/               #     工具类（CRC32/MD5/AES）
│   ├── Editor/                 #   打包窗口
│   └── Example/                #   示例代码
├── BundleDemo/                 # Demo 场景
└── Test/                       # 测试入口
```

## 架构概览

```
业务层 (Demo)
    ↓
框架入口 (MJAssetsABFrame 静态 API)
    ↓
┌──────────────┬──────────────┬──────────────┐
│  热更子系统   │  加载子系统   │  解压子系统   │
│ HotAssets    │ Resources    │ Decompress   │
│ Manager      │ Manager      │ Manager      │
└──────────────┴──────────────┴──────────────┘
    ↓
配置层 (BundleSettings / BundleConfig)
    ↓
工具层 (CRC32 / MD5 / AES / ClassObjectPool)
```

## 核心流程

```
启动 → 解压内嵌资源 → 网络检测 → 版本检测 → 热更下载 → 加载配置 → 进入游戏
```

## API 速查

| API | 说明 |
|-----|------|
| `CheckAssetsVersion(module)` | 检测模块是否需要热更 |
| `HotAssets(module)` | 执行热更下载 |
| `GetHotAssetsModule(module)` | 获取热更模块（读取进度） |
| `Instantiate(path)` | 同步实例化 |
| `InstantiateAsync(path)` | 异步实例化 |
| `InstantiateAndLoadAsync(path)` | 等待下载完成后实例化 |
| `LoadSprite / LoadTexture / LoadAudio` | 加载资源 |
| `LoadAtlasSprite(atlas, name)` | 从图集加载 Sprite |
| `Release(obj, destroy)` | 释放对象（池回收 / 销毁） |
| `ClearResoucesAssets(deep)` | 清理资源 |
| `PreLoadObj(path, count)` | 预加载对象 |

## 详细文档

完整的框架设计文档见 [Assets/MJAssetsFrame/README.md](Assets/MJAssetsFrame/README.md)。

## 许可

MIT License
