---
name: unity-game-ui-kit-generation
description: Generate game UI asset kits in Unity via a three-step workflow — Step 1 generates a game UI screenshot from text, Step 2 converts it into a UI cutout sheet on magenta background, Step 3 uses CV connected-component detection to slice the sheet into individual sprite PNGs. Use this skill whenever the user wants to create game UI assets such as HUD layouts, inventory screens, buttons, panels, health bars, skill icons — e.g. "帮我生成游戏UI", "生成背包界面", "make a game HUD", "create UI kit for my game". Trigger proactively for any game UI design or UI element extraction request in Unity. Do NOT use for standalone 2D sprites/icons (use generate_sprite) or general images (use generate_image).
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="game-ui-kit-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**（本 skill **无 placeholder**）
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4b Sprite 模板用 `execute_csharp_script` 建 `SpriteRenderer` 或 Canvas 子节点 + `Image`（**不是** `execute_custom_tool`）。
> - **子代理**：Step 3 `slice_image` 返回 `sliced_asset_paths` 后，**可选**调 `place_assets_in_scene` 把需要的 Sprite 放到场景。Step 1/2 **不调**（中间产物，不需要放置）。
> - **主 agent**：报告里的 `sliced_asset_paths` 是产出路径，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"放到场景"时才调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Game UI Kit in Unity 🎮

通过三步工作流生成游戏 UI 资产套件。
Output: Step 1 产出 UI 截图 PNG（landscape_16_9）；Step 2 产出 UI 元素抠图 sheet PNG（square_hd，品红背景）；Step 3 产出多张独立 Sprite PNG，自动保存到 `Assets/TJGenerators/History/`。

## 三步工作流概览

| 步骤 | 输入 | 输出 | 用途 |
|---|---|---|---|
| Step 1 | 文本描述（无 `screenshot_path`） | UI 截图 PNG（landscape_16_9） | 预览 UI 布局设计 |
| Step 2 | Step 1 截图本地路径（`screenshot_path`） | UI 抠图 sheet PNG（square_hd，品红背景） | CV 切割的素材 |
| Step 3 | Step 2 的 cutout sheet 路径 | 多张独立 Sprite PNG | 直接可用于游戏 UI |

> ⚠️ **三步串行依赖**——Step 2 必须等 Step 1 完成后才能提交（需要截图路径）；Step 3 必须等 Step 2 完成后才能执行（需要 cutout sheet 路径）。Step 3 是同步操作，不需要等待通知。

## 🚦 执行流程（不要跳读外链）

### Step 1：生成 UI 截图

1. 调 `generate_game_ui_kit`（**不传** `screenshot_path`）→ 拿 `task_id` + `placeholder_path`（1×1 灰色 PNG）
2. **跳过** `place_assets_in_scene`（中间产物，不放置）
3. **END RESPONSE TURN** — 不要 poll、不要 `query_game_ui_kit_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `image_path`（截图本地路径）→ **立即提交 Step 2**

### Step 2：生成 UI 抠图 sheet

5. 调 `generate_game_ui_kit`（`screenshot_path` = Step 1 的 `image_path`）→ 拿 `task_id` + `placeholder_path`
6. **END RESPONSE TURN** — 不要 poll
7. 下一轮收到 `<bg_task_done>` → 读 `image_path`（cutout sheet 本地路径）→ **立即执行 Step 3**

### Step 3：CV 切割 cutout sheet → 独立 Sprite

8. 调 `slice_image`（`image_path` = Step 2 的 `image_path`，`background_mode` = `"solid_color"`）→ 拿 `sliced_asset_paths` + `sliced_count`
9. **可选**：调 `place_assets_in_scene` 把切割后的 Sprite 放到场景（资产类型 `Sprite`）
10. 报告完成

**档位**：Step 1/2 每步 30–90 秒；120 秒内无通知才允许 `query_game_ui_kit_status` 一次。Step 3 同步返回，无需等待。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **三步串行**——Step 2 依赖 Step 1 的 `image_path`，Step 3 依赖 Step 2 的 `image_path`，不能并发。
2. **有 placeholder**——Step 1/2 都返回 `placeholder_path`（1×1 灰色 PNG），但中间产物不需要放置。
3. **`prompt` 在 Step 2 中被忽略**——Step 2 使用后端固定的 cutout prompt，`prompt` 参数必须传但内容不影响结果。
4. **Step 2 产出品红背景**——cutout sheet 使用纯品红 (#FF00FF) 背景，Step 3 的 `slice_image` 用 `solid_color` 模式自动去除。
5. **`screenshot_path` 是本地路径**——C# host 自动上传到 CDN 并提交给后端，与 `generate_image` 的 `image_path` 模式一致。
6. **Step 3 是同步操作**——`slice_image` 立即返回切割结果，不需要 `task_id` 或轮询。
7. **并发上限 5**——同时运行的 game_ui_kit 任务最多 5 个（但步骤串行，实际每个套件占 1 个并发槽 × 2 次）。

## When to Use / NOT to Use

适用：游戏 HUD 设计、背包界面、技能栏、主菜单 UI、设置面板、对话框样式、游戏 UI 元素提取。

不适用：
- 独立 2D 精灵（图标、立绘、道具） → `generate_sprite`
- 通用图片 / 概念图 / 纹理 → `generate_image`
- 3D 模型 / 材质 / 天空盒 → 各自专属 skill
- 已有 UI 截图、只需抠图 → 仍可用本 skill 的 Step 2（传入 `screenshot_path`）

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_game_ui_kit` — Step 1（生成 UI 截图）

```python
execute_custom_tool(
  tool_name="generate_game_ui_kit",
  parameters={
    "prompt": "fantasy RPG inventory screen with health bars, item slots, skill buttons",  # Required
    # screenshot_path: 不传（Step 1）
    "quality": "medium",       # 可选："low" / "medium" / "high"，默认 "medium"
    "output_format": "png",    # 可选："png" / "jpeg" / "webp"，默认 "png"
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

**后端 prompt 增强**：后端会自动在用户 prompt 后追加 UI 设计关键词（`complete game UI screen design, full HUD layout, health bars, mana bars, buttons, panels, inventory grid, skill icons, mini-map, dialogue box, score display, clean professional game interface` 等），无需自己写这些。

### `generate_game_ui_kit` — Step 2（生成 UI 抠图 sheet）

```python
execute_custom_tool(
  tool_name="generate_game_ui_kit",
  parameters={
    "prompt": "fantasy RPG inventory screen",  # 必传但被忽略，用 Step 1 的原 prompt 即可
    "screenshot_path": "Assets/TJGenerators/History/ui_screenshot_xxx.png",  # Required：Step 1 的 image_path
    "quality": "medium",
    "output_format": "png",
  }
)
```

**后端 prompt**：Step 2 使用固定 cutout prompt（提取所有 UI 元素为独立 cutout、网格排列、品红背景），用户 `prompt` 不影响结果。

### 返回字段

**Step 1 / Step 2 通用返回**：
- `task_id`
- `placeholder_path`：1×1 灰色占位 PNG，**立即可用**（但中间产物不需要放置）
- `step`：`1` 或 `2`
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

**Step 1 完成时**：

| 字段 | 说明 |
|---|---|
| `image_path` | UI 截图本地路径 — **传给 Step 2 的 `screenshot_path`** |
| `preview_url` | 预览 URL |

**Step 2 完成时**：

| 字段 | 说明 |
|---|---|
| `image_path` | cutout sheet 本地路径（品红背景 PNG） |
| `preview_url` | 预览 URL |

### `query_game_ui_kit_status` / `list_game_ui_kit_tasks`

`query_game_ui_kit_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `placeholder_path`（仅 `generating` 时）。

`list_game_ui_kit_tasks` 返回当前 session 的所有 game_ui_kit 任务。

## 参数速查

### generate_game_ui_kit

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `prompt` | string | **required** | Step 1: UI 描述；Step 2: 被忽略但必传 |
| `screenshot_path` | string | — | Step 2 only：Step 1 的 `image_path`。省略 = Step 1 |
| `quality` | string | `"medium"` | `"low"` / `"medium"` / `"high"` |
| `output_format` | string | `"png"` | `"png"` / `"jpeg"` / `"webp"` |
| `output_path` | string | — | 不建议指定，默认 `Assets/TJGenerators/History/` |

### slice_image

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `image_path` | string | **required** | cutout sheet 的本地路径（Step 2 的 `image_path`） |
| `background_mode` | string | `"auto"` | `"auto"` / `"transparent"` / `"solid_color"`（品红背景用 `solid_color`） |
| `color_tolerance` | float | `15` | 0-100，越大越多像素被判为背景 |
| `alpha_threshold` | float | `0.1` | 0-1，透明背景模式下使用 |
| `min_region_pixels` | int | `100` | 小于此像素的区域被忽略 |
| `padding` | int | `2` | 每个切割元素周围的额外像素 |
| `set_as_sprite` | bool | `true` | 自动设为 Sprite 导入模式 |

## 使用示例

### 完整三步流程

```python
# === Step 1: 生成 UI 截图 ===
result = execute_custom_tool(
    tool_name="generate_game_ui_kit",
    parameters={
        "prompt": "fantasy RPG inventory screen with health bars, item slots, skill buttons",
        "quality": "medium"
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
# ✅ END RESPONSE TURN — 等 bg_task_done
# 通知到达后读 image_path，提交 Step 2
```

```python
# === Step 2: 生成 UI 抠图 sheet ===
# （在 Step 1 的 <bg_task_done> 到达后执行）
result = execute_custom_tool(
    tool_name="generate_game_ui_kit",
    parameters={
        "prompt": "fantasy RPG inventory screen",  # 原始 prompt，Step 2 忽略
        "screenshot_path": screenshot_path,           # Step 1 的 image_path
        "quality": "medium"
    }
)
task_id = result["task_id"]
# ✅ END RESPONSE TURN — 等 bg_task_done
# 通知到达后读 image_path，执行 Step 3
```

```python
# === Step 3: CV 切割 cutout sheet ===
# （在 Step 2 的 <bg_task_done> 到达后执行，同步返回）
result = execute_custom_tool(
    tool_name="slice_image",
    parameters={
        "image_path": cutout_sheet_path,       # Step 2 的 image_path
        "background_mode": "solid_color",     # 品红背景
        "color_tolerance": 15,
        "set_as_sprite": True
    }
)
if result.get("success"):
    sliced_paths = result["sliced_asset_paths"]
    sliced_count = result["sliced_count"]
    # ✅ 可选：place_assets_in_scene 放置切割后的 Sprite
    # ✅ 报告完成
```

### 跳过 Step 1（用户已有截图）

```python
# 用户提供了已有截图的本地路径
result = execute_custom_tool(
    tool_name="generate_game_ui_kit",
    parameters={
        "prompt": "existing UI",              # 必传但被忽略
        "screenshot_path": "Assets/UI/existing_screenshot.png",
    }
)
# 直接进入 Step 2
```

## CV 切割（Step 3：slice_image）

Step 2 产出的 cutout sheet 是一张品红背景的大图，Step 3 使用 `slice_image` 工具自动完成 CV 切割：

- **连通域检测**（8-connected BFS）自动找到每个独立 UI 元素
- **颜色去背景**：自动估计背景色并从边缘像素中扣除（color decontamination），消除品红残留边
- **羽化边缘**：2px box blur 产生软边缘，避免硬锯齿
- **逐个裁剪**：每个连通域裁剪为独立 PNG，自动设为 Sprite (2D and UI) Single mode

### `slice_image` — CV 切割 cutout sheet

```python
execute_custom_tool(
  tool_name="slice_image",
  parameters={
    "image_path": "Assets/TJGenerators/History/GameUIKit_xxx.png",  # Required: Step 2 的 image_path
    "background_mode": "solid_color",  # 品红背景用 solid_color；可选 'auto'/'transparent'/'solid_color'，默认 'auto'
    "color_tolerance": 15,     # 可选 0-100，越大越多像素被判为背景，默认 15
    "min_region_pixels": 100,  # 可选，小于此像素的区域被忽略，默认 100
    "padding": 2,               # 可选，每个切割元素周围的额外像素，默认 2
    "set_as_sprite": True,     # 可选，自动设为 Sprite 导入模式，默认 True
  }
)
```

**返回字段**：

| 字段 | 说明 |
|---|---|
| `success` | 是否成功 |
| `sliced_count` | 切割出的 Sprite 数量 |
| `output_directory` | 输出目录（`Assets/TJGenerators/History/{sourceName}_sliced_{timestamp}/`） |
| `sliced_asset_paths` | 切割后的 Sprite 路径数组 |

### slice_image 参数调优

| 问题 | 调整 |
|---|---|
| 品红残留边缘 | 提高 `color_tolerance` 到 20-25 |
| 元素被误合并 | 提高 `min_region_pixels` 过滤噪声；或降低 `color_tolerance` |
| 元素被切断 | 降低 `color_tolerance`（前景像素被误判为背景） |
| 切割太少 | 降低 `min_region_pixels`；确认 `background_mode` 正确 |
| 边缘有白边 | `slice_image` 已内置颜色去背景，如仍有残留可手动后处理 |

### 放入场景

Step 3 切割后的 Sprite 可作为 `Sprite` 类型放入场景。

资产类型 **`Sprite`**，路径用 `sliced_asset_paths` 中的路径。

> 切割后的独立 Sprite 可直接用于 UI Image 组件或 SpriteRenderer。如需打图集，可使用 `generate_sprite_atlas` 工具或 Unity Sprite Atlas。

## Prompt 写作指南

| 用途 | Prompt 示例 |
|---|---|
| RPG 背包 | `"fantasy RPG inventory screen with health bars, item slots, skill buttons"` |
| FPS HUD | `"first-person shooter HUD with ammo counter, minimap, crosshair, health bar"` |
| 主菜单 | `"medieval game main menu with ornate buttons, settings panel, character portrait"` |
| 对话框 | `"visual novel dialogue box with text area, character name plate, choice buttons"` |
| 技能树 | `"skill tree UI with branching nodes, connection lines, unlock buttons"` |

技巧：
- 描述 **UI 类型和包含的元素**（按钮、面板、血条、物品格）
- 提及 **游戏类型/风格**（fantasy RPG / sci-fi / medieval）
- 后端会自动增强 prompt，无需写 "HUD layout" 等关键词
- 英文 prompt 效果更佳

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| Step 1 截图不像游戏 UI | prompt 太笼统 | 描述具体 UI 元素（血条、物品格、技能按钮）；后端会增强但基础描述仍重要 |
| Step 2 cutout sheet 为空/全品红 | `screenshot_path` 路径错误 | 确认使用 Step 1 `<bg_task_done>` 中的 `image_path` |
| Step 2 提交报错 | 缺少 `screenshot_path` | Step 2 必须传 `screenshot_path`，值为 Step 1 的 `image_path` |
| Step 1 和 Step 2 用了不同 prompt | Step 2 忽略 prompt | 这是正常的——Step 2 使用固定 cutout prompt |
| Step 3 切割出 0 个元素 | `background_mode` 不对或 `color_tolerance` 太高 | 品红背景用 `solid_color`；降低 `color_tolerance` |
| Step 3 品红残留边缘 | `color_tolerance` 太低 | 提高 `color_tolerance` 到 20-25 |
| Step 3 元素被误合并 | `color_tolerance` 太高或 `min_region_pixels` 太低 | 降低 `color_tolerance`；提高 `min_region_pixels` |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- PNG < 5 KB → 仍是 placeholder 或任务丢失
- PNG ≥ 50 KB → 真实图片已就绪

可用 `glob("Assets/TJGenerators/History/*.png")` + 文件大小恢复。注意区分 Step 1 截图和 Step 2 cutout sheet（按时间和尺寸判断）。

---

**Task ID Format**：`game_ui_kit_{counter}_{timestamp}`

**Notes**：
- Step 1/2 使用 Frontier Game Design 模型
- Step 1 使用 `landscape_16_9` 尺寸；Step 2 使用 `square_hd` 尺寸
- Step 2 的 cutout prompt 是后端固定的，用户 prompt 不影响
- Step 3 `slice_image` 使用 CV 连通域检测（8-connected BFS），同步返回
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**
- 需 Unity Editor 在线运行；消耗 AI 服务额度
