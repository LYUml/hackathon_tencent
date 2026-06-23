# 《画皮》UI 项目导入指南

## 项目结构

```
ui/
├── Scripts/
│   ├── Core/              # 核心管理
│   │   ├── UIManager.cs
│   │   ├── PanelBase.cs
│   │   └── UIStack.cs
│   ├── Data/              # ScriptableObject 数据类
│   │   ├── ClueData.cs
│   │   ├── CharacterData.cs
│   │   ├── DialogueData.cs
│   │   ├── ObserveSkinRevealData.cs
│   │   ├── ExplorationSceneData.cs   # 场景探索数据
│   │   ├── SceneHotspotData.cs       # 热点数据
│   │   └── SceneCharacterEntryData.cs # 人物入口数据
│   ├── Panels/            # 面板逻辑
│   │   ├── MainMenuPanel.cs
│   │   ├── DialoguePanel.cs
│   │   ├── CluePanel.cs
│   │   ├── CharacterArchivePanel.cs
│   │   ├── ObserveSkinPanel.cs
│   │   ├── PausePanel.cs
│   │   └── Exploration/              # 场景探索系统
│   │       ├── ExplorationPanel.cs   # 探索主面板
│   │       ├── SceneBackgroundView.cs # 背景显示
│   │       ├── SceneHotspotItem.cs   # 调查热点
│   │       ├── SceneCharacterEntry.cs # 人物入口
│   │       ├── ObjectiveNoteView.cs  # 目标提示
│   │       └── ClueToastView.cs      # 线索获得提示
│   └── Editor/            # 编辑器工具
│       └── HuapiUIGenerator.cs
├── Prefabs/
│   ├── MainMenuPanel.prefab
│   ├── ExplorationPanel.prefab       # 探索面板
│   ├── SceneHotspotItem.prefab       # 热点模块
│   └── SceneCharacterEntry.prefab    # 人物模块
├── index.html             # 设计文档概览页
└── UI_Architecture_HuaPi_V{1-4}.md  # 设计文档
```

---

## 前置要求

1. **Unity Editor**（推荐 2022.3 LTS 或更新版本）
2. **TextMeshPro**（导入 TMP Essentials）

---

## 第一步：导入 TextMeshPro

1. 打开 Unity Editor
2. 菜单栏：`Window > TextMeshPro > Import TMP Essential Resources`
3. 点击 `Import` 确认导入

> **注意**：如果不导入 TMP Essentials，所有 TMP_Text 相关代码会报错。

---

## 第二步：创建 Unity 项目

1. 打开 Unity Hub
2. 点击 `New Project`
3. 选择 `2D (URP)` 或 `2D` 模板
4. 设置项目名称为 `HuaPi`
5. 选择保存路径，点击 `Create`

---

## 第三步：导入 UI 脚本

### 方式 A：手动复制（推荐）

1. 将 `ui/Scripts/` 文件夹复制到 Unity 项目 `Assets/` 目录下
2. 在 Unity 中等待脚本编译完成

### 方式 B：创建符号链接（高级）

```bash
# macOS/Linux
ln -s /path/to/tencent_hackathon/ui/Scripts /path/to/UnityProject/Assets/HuaPiScripts
```

---

## 第四步：生成 UI 场景

1. 在 Unity 菜单栏点击：`Tools / HuaPi / Generate UI`
2. 点击 `生成 UI 场景` 按钮
3. 检查 Hierarchy 面板，确认已生成以下结构：
   - `UI_Root`
     - `UIManager`
     - `BackgroundCanvas` (Sort Order 0)
     - `WorldHUDCanvas` (Sort Order 10)
     - `DialogueCanvas` (Sort Order 20)
     - `NormalPanelCanvas` (Sort Order 30)
     - `PopupCanvas` (Sort Order 40)
     - `RevealCanvas` (Sort Order 50)
     - `SystemCanvas` (Sort Order 60)
     - `EventSystem`

---

## 第五步：创建数据资源

1. 在 Project 窗口中，右键点击 `Assets/HuaPi/Data` 文件夹
2. 选择 `Create / HuaPi / ClueData` 创建线索数据
3. 选择 `Create / HuaPi / CharacterData` 创建角色数据
4. 选择 `Create / HuaPi / DialogueData` 创建对话数据
5. 选择 `Create / HuaPi / ObserveSkinRevealData` 创建观皮数据
6. 选择 `Create / HuaPi / ExplorationSceneData` 创建场景探索数据

---

## 第六步：创建面板 Prefab

> **快捷方式**：使用 `Tools / HuaPi / Generate UI` 可一键生成所有 Canvas 层级和面板 Prefab。

### 1. 主菜单面板

1. 在 Hierarchy 中创建 Empty GameObject，命名为 `MainMenuPanel`
2. 添加 `MainMenuPanel.cs` 脚本
3. 添加 `CanvasGroup` 组件
4. 添加 `RectTransform`，设置为全屏（Anchors: Stretch-Stretch）
5. 创建子对象：
   - `Title` (TMP_Text)：文字 "画皮"
   - `Subtitle` (TMP_Text)：文字 "MASKS BEHIND MASKS"
   - `Menu/StartGame` (TMP_Text + Button)：文字 "开始游戏"
   - `Menu/ContinueGame` (TMP_Text + Button)
   - `Menu/Settings` (TMP_Text + Button)
   - `Menu/Quit` (TMP_Text + Button)
   - `BottomInfo` (TMP_Text)：文字 "薛家戏班 · 民国二十年"
6. 将 MainMenuPanel 拖入 `Assets/HuaPi/Prefabs` 保存为 Prefab

### 2. 探索面板（ExplorationPanel）

**结构**：

```
ExplorationPanel (CanvasGroup + ExplorationPanel.cs)
├── BackgroundView (SceneBackgroundView)
│   ├── BackgroundImage (Image) — 场景背景图
│   ├── DarkOverlay (Image) — 压暗遮罩
│   └── VignetteOverlay (Image) — 暗角/黑雾
├── HotspotContainer (Transform) — 热点挂载点
├── CharacterContainer (Transform) — 人物挂载点
├── ObjectiveNoteView (ObjectiveNoteView)
│   ├── LocationText (TMP_Text) — 当前地点
│   └── ObjectiveText (TMP_Text) — 当前目标
└── ClueToastView (ClueToastView)
    ├── TitleLabel (TMP_Text) — "获得线索"
    ├── ClueNameLabel (TMP_Text) — 线索名
    └── ClueDescLabel (TMP_Text) — 线索描述
```

**数据驱动**：

- 创建 `ExplorationSceneData` ScriptableObject
- 配置背景 Sprite、目标文本、热点列表、人物列表
- 在 `ExplorationPanel` 的 `Init` 中传入场景数据

**Demo 数据**：

探索面板默认启用 `useDemoData`，会加载"后台化妆间"Demo：
- 背景：`pic/背景图/剧场后台化妆间:休息室.png`
- 人物：`pic/人物立绘/旦.png`
- 热点：镜台（点击获得线索"镜台下的旧戏票"）

### 3. 其他面板

重复类似步骤创建：
- `DialoguePanel`（对话面板）
- `CluePanel`（线索面板）
- `CharacterArchivePanel`（角色档案面板）
- `ObserveSkinPanel`（观皮面板）
- `PausePanel`（暂停面板）

---

## 第七步：配置 UIManager

1. 选中 `UI_Root/UIManager`
2. 在 Inspector 中，将各面板 Prefab 拖入对应字段：
   - `Main Menu Panel Prefab` → `MainMenuPanel.prefab`
   - `Exploration Panel Prefab` → `ExplorationPanel.prefab`（新增）
   - `Dialogue Panel Prefab` → `DialoguePanel.prefab`
   - 以此类推
3. 在 `MainMenuPanel` 的"开始游戏"按钮中添加调用：
   ```csharp
   UIManager.Instance.ClosePanel(UIPanelType.MainMenu);
   UIManager.Instance.OpenPanel(UIPanelType.Exploration);
   ```
