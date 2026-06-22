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
│   │   └── ObserveSkinRevealData.cs
│   ├── Panels/            # 面板逻辑
│   │   ├── MainMenuPanel.cs
│   │   ├── DialoguePanel.cs
│   │   ├── CluePanel.cs
│   │   ├── CharacterArchivePanel.cs
│   │   ├── ObserveSkinPanel.cs
│   │   └── PausePanel.cs
│   └── Editor/            # 编辑器工具
│       └── HuapiUIGenerator.cs
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

---

## 第六步：创建面板 Prefab

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

### 2. 其他面板

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
   - `Dialogue Panel Prefab` → `DialoguePanel.prefab`
   - 以此类推

---

## 第八步：运行测试

1. 按 `Ctrl+S` 保存场景
2. 按 `Ctrl+P` 或点击 Play 按钮运行
3. 检查 Console 窗口是否有错误

---

## 常见问题

### Q: 编译报错 "The type or namespace name 'TMPro' could not be found"
**A**: 没有导入 TextMeshPro Essentials。请执行第一步。

### Q: 编译报错 "The type or namespace name 'HuaPi' could not be found"
**A**: 脚本没有正确导入。请检查 `Scripts` 文件夹是否在 `Assets` 目录下。

### Q: 运行后没有 UI 显示
**A**: 检查 UIManager 的面板 Prefab 是否已赋值。如果 Prefab 为空，UIManager 不会创建面板。

### Q: 按钮点击没有反应
**A**: 检查 EventSystem 是否存在。如果没有，请重新生成 UI 场景。

---

## 设计文档

| 版本 | 说明 | 文件 |
|------|------|------|
| V1 | 基础版（视觉方案 + UGUI 架构） | `UI_Architecture_HuaPi_V1.md` |
| V2 | 恐怖漫画版（16:9 + 2D漫画 + 角色参考） | `UI_Architecture_HuaPi_V2.md` |
| V3 | 无边界沉浸式版（场景即界面） | `UI_Architecture_HuaPi_V3.md` |
| V4 | 纯净沉浸版（hakidame 字体 + 开始界面像插画） | `UI_Architecture_HuaPi_V4.md` |

---

## 字体配置（hakidame）

### 1. 导入字体文件

1. 将 `hakidame.ttf` 拖入 `Assets/Fonts/` 文件夹

### 2. 生成 TMP Font Asset

1. 菜单栏：`Window > TextMeshPro > Font Asset Creator`
2. Font Source: 选择 `hakidame`
3. Atlas Resolution: `2048 x 2048`
4. Character Set: `Unicode Range (Hex)`
5. 输入范围: `4E00-9FFF`
6. 点击 `Generate Font Atlas`
7. 保存到 `Assets/Fonts/hakidame SDF.asset`

### 3. 设置 Fallback

1. 选中 `hakidame SDF` 字体 Asset
2. 在 Inspector 中展开 `Fallback Font Assets`
3. 添加 `Noto Sans SC` 或 `思源黑体` 作为 Fallback

### 4. 使用字体

1. 选中 TMP_Text 对象
2. 在 Inspector 中将 `Font Asset` 设置为 `hakidame SDF`

---

## 技术支持

- Unity 版本：2022.3 LTS 或更新
- 目标平台：PC / WebGL（浏览器端）
- 分辨率：1920 x 1080（16:9 横屏）

---

*《画皮》UI 项目 · 2026*
