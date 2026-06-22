# 《画皮》第二版 UI 风格与 Unity UGUI 架构方案

> 版本：V2.0 恐怖漫画版
> 基准：16:9 横屏 1920×1080 | 2D 漫画/插画风格 | Unity UGUI
> 日期：2026-06-22

---

## 一、16:9 基准分辨率与安全区

### 设计基准
- **分辨率**：1920 × 1080
- **Canvas Scaler**：`Scale With Screen Size`
- **Reference Resolution**：1920 × 1080
- **Screen Match Mode**：`Match Width Or Height`（初始 `0.5`，观皮界面因中央构图重可调整为 `0.6`）
- **适配策略**：UGUI 锚点系统保证关键元素在缩放时保持相对位置，背景图使用 `Scale With Screen Size` 并设置 `Reference Resolution` 的 `Expand` 或 `Shrink` 确保无黑边。

### 安全边距
- 左右 ≥ 80 px（1920 宽下）
- 上下 ≥ 60 px
- 关键按钮、文字、交互区域不超出安全边距
- 对话框和证据面板不超过屏幕高度 40%–50%，为 2D 角色立绘和场景留出空间

### 各页面安全区布局示意

| 页面 | 核心区域 | 安全区说明 |
|------|----------|-----------|
| 主菜单 | 左侧标题 + 右侧菜单 | 标题在左 1/3，菜单在右 1/3，背景铺满 |
| 对话 | 左 35% 立绘 + 底部 28% 对话框 | 对话框不遮挡角色脸部，选项在对话框上方 |
| 线索背包 | 左 30% 列表 + 右 60% 详情 | 筛选标签在顶部，返回在右下角 |
| 角色档案 | 左 25% 立绘 + 中 40% 信息 + 右 30% 进度 | 隐藏画像区域在右侧面板 |
| 观皮 | 左 20% 档案 + 中 45% 画像 + 右 30% 线索 | 中央画像是视觉焦点，底部保留操作按钮 |
| 设置 | 中央面板 | 半透明黑底覆盖，面板不贴边 |

---

## 二、第二版 UI 风格关键词

16:9 横屏 | 深色恐怖 | 2D 漫画插画 | 旧戏班 | 暗红黑墨 | 脸谱异化 | 纸张腐坏 | 皮影剪影 | 亡魂戏子 | 民国悬疑

---

## 三、色彩方案（更暗、更恐怖）

| 用途 | 色值 | 说明 |
|------|------|------|
| **背景极深** | `#0a0808` | 主背景，近乎纯黑，用于夜晚、观皮、菜单 |
| **背景深** | `#1a1210` | 面板底色，暖调深褐，用于 HUD、档案 |
| **背景中** | `#2a1a14` | 卡片/按钮底色，比深底略亮，用于按钮和列表项 |
| **旧纸** | `#d8cfc0` | 文字承载区，低饱和度旧纸黄，不温暖 |
| **强调暗红** | `#8b2020` | 危险、血色、关键线索、矛盾标记、错误反馈 |
| **脏灰蓝** | `#4a5a6a` | 亡魂、寒冷、旧墙、夜间空气，辅助色 |
| **暗铜金** | `#c9a96e` | 极少高亮：选中态、传统纹样边框、可交互按钮描边 |
| **正白** | `#f0ece4` | 仅用于黑底上的重要标题和关键文字，高对比 |
| **墨灰** | `#5a5a5a` | 次要文字、未激活状态、禁用项 |
| **黑雾** | `#0a0808` | 纯黑雾层，遮罩用，可配合透明度渐变 |
| **水墨纹理** | `#2a2a2a` | 2D 墨雾中的层次，非纯黑 |

### 使用规则
- 画面 70% 以上区域为近黑色或深褐色，营造压抑感。
- 暗红 `#8b2020` 仅用于危险标记、矛盾文字、关键线索，面积不超过 5%。
- 旧纸 `#d8cfc0` 仅用于文字面板和对话框，饱和度低，不显得温暖明亮。
- 暗铜金 `#c9a96e` 仅用于选中态、当前目标和按钮高亮，面积不超过 8%。
- 脏灰蓝 `#4a5a6a` 用于冷色恐怖界面和亡魂角色相关元素。
- 保证所有正文文字与背景对比度 ≥ 4.5:1（WCAG AA 最低标准），恐怖感不等于看不清。

---

## 四、2D 漫画 / 插画风格规则

### 整体原则
- 所有 UI 元素必须是 2D 手绘/漫画风格，禁止使用现代扁平矢量图标、渐变按钮、圆角玻璃拟态。
- UI 边框像漫画分镜框、旧纸框、戏牌框或破损档案框。
- 按钮像撕裂的戏票、旧木牌、纸签或破损剧本页。
- 背景装饰使用手绘黑影、皮影轮廓、脸谱残片、墨线和烧痕。

### 手绘线稿 vs 简洁元素的区分

| 元素 | 风格 | 原因 |
|------|------|------|
| 边框/面板 | 手绘线稿 + 破损边缘 | 营造旧档案和漫画分镜感 |
| 按钮 | 手绘木牌/戏票形状 | 保持戏班和旧纸氛围 |
| 图标（线索、档案） | 手绘线稿 | 与角色立绘统一风格 |
| 对话框 | 手绘烧边纸页 | 叙事沉浸感 |
| 黑雾/遮罩 | 2D 手绘墨雾/水墨扩散 | 非写实粒子，保持漫画风格 |
| 角色头像/立绘 | 2D 竖幅立绘，黑轮廓线 | 与角色设计图一致 |
| 进度条/Slider | 简洁几何 + 暗铜描边 | 保证可读性和操作精度 |
| 文字区域 | 简洁矩形，旧纸底色 | 长文本阅读需要清晰边界 |
| 弹窗/提示 | 简洁旧纸卡片 | 快速信息需要清晰，不过度装饰 |

---

## 五、如何参考 `character` 文件夹人物设计图

### 青衣 / 正旦
- **UI 应用**：主菜单、章节封面、角色档案、关键揭露界面。
- **色彩参考**：暗红 `#8b2020` 与黑色 `#0a0808` 的搭配，来自青衣暗红戏服与白面脸谱的对比。
- **装饰元素**：凤冠头饰的复杂纹样可用于标题边框和档案装饰。
- **气质**：端庄但阴冷，悲情亡魂，档案中表面身份与隐藏真相的冲突。

### 小生
- **UI 应用**：夜晚档案界面、冷色恐怖场景、折扇和文生巾元素。
- **色彩参考**：脏灰蓝 `#4a5a6a` 来自小生浅蓝灰戏衣的褪色版。
- **装饰元素**：折扇可转化为线索卡片的角标或对话选项的图标。
- **气质**：苍白书生亡魂，档案中可用灰蓝色调表现其“冷”的隐藏性格。

### 文丑
- **UI 应用**：错误反馈、可疑提示、反常线索标记。
- **色彩参考**：暗绿/墨绿作为角色识别色，不成为主色。
- **装饰元素**：小花脸的“不协调感”可用于错误线索的视觉反馈（如线索卡片的歪扭、颜色偏移）。
- **气质**：滑稽外表下的不安感，错误匹配时线索卡片可以“丑角化”变形。

### 净行·武净
- **UI 应用**：观皮成功、关键秘密揭露、章节转场。
- **色彩参考**：暗铜金 `#c9a96e` 来自武净重彩脸谱中的暗金纹样。
- **装饰元素**：厚重边框、脸谱裂纹、甲片纹样可用于揭露成功的压迫性构图。
- **气质**：厉鬼武将，关键揭露时画面可以加入脸谱裂纹和暗金纹样闪烁。

### 考据说明
建议游戏中通过“图鉴”或“档案考据”区分：
- **传统原型**：戏曲行当的真实历史和文化含义。
- **游戏亡魂化**：基于传统原型进行恐怖化艺术改编，保留形制但加入破损、血渍、黑雾等异化元素。

---

## 六、字体与中文阅读层级

### 字体选择
- **标题/戏曲感文字**：站酷庆科黄油体或思源宋体 Heavy（内嵌 TTF，TMP 预生成 Atlas）。
- **正文**：思源黑体/Noto Sans SC Regular（清晰、长段中文阅读）。
- **数字/系统提示**：Noto Sans SC Medium（干净、快速扫描）。

### 字号层级（1920×1080 基准）

| 层级 | 字号 | 字重 | 用途 | 颜色 |
|------|------|------|------|------|
| 主标题 | 42–52px | 500 | 游戏标题、章节标题 | 暗红/暗金 |
| 副标题 | 24–30px | 500 | 面板标题、角色名 | 暗金/正白 |
| 线索名 | 18–20px | 500 | 线索卡片、档案标签 | 深褐/暗金 |
| 正文 | 15–16px | 400 | 描述、对话、长文本 | 深褐/旧纸 |
| 按钮文字 | 15–16px | 500 | 按钮、菜单、选项 | 暗金/旧纸 |
| 系统提示 | 12–13px | 400 | 提示、来源、状态 | 墨灰/暗铜 |
| 矛盾标记 | 15–16px | 500 | 关键矛盾句 | 暗红 |

### 对话文本阅读优化
- 行高 1.8–2.0，每行不超过 24–26 个汉字。
- 关键矛盾句用暗红 `#8b2020` 加粗，配合轻微底纹或暗红点染标记。
- 避免荧光高亮、现代下划线，使用墨迹晕染或暗红边缘标记。

### WebGL 字体处理
- 所有中文字体内嵌到 `Assets/Fonts/`，TMP 预生成 Atlas（常用字 3500 字即可）。
- TMP Settings 中指定全局 Fallback 为 Noto Sans SC。
- 构建前运行字体 Subset 裁剪，减小包体。

---

## 七、核心页面设计（16:9 横屏布局）

### 1. 主菜单

**16:9 布局**：
- 左侧 1/3 区域：游戏标题《画皮》+ 副标题。
- 右侧 1/3 区域：垂直菜单按钮（开始/继续/设置/退出）。
- 背景：2D 插画——空戏台、半开暗红帷幕、悬挂脸谱残片、黑雾中的青衣/武净剪影。
- 底部：极暗旧剧院地面、火烧痕迹、散落戏票。

**视觉要求**：
- 按钮为破损戏票/旧木牌，手绘撕裂边缘。
- 选中态：暗红描边 + 墨迹扩散 + 低频闪烁（0.3s 间隔，暗金边缘明暗变化）。
- 不使用明亮高饱和按钮。

### 2. 白天探索 HUD

**16:9 布局**：
- 左上角：当前地点（旧纸标签）+ 时间阶段（白天）。
- 右上角：暂停按钮（小木牌）+ 系统入口。
- 右下角：线索/档案快捷入口（两个小木牌图标）。
- 底部中央：当前目标提示（旧纸条，短暂显示）。

**视觉要求**：
- HUD 使用暗色半透明旧纸或墨色底板，透明度 70%。
- 可调查物件高亮：暗铜金/灰白描边虚线圆环，悬停显示物件名称（旧纸标签弹出）。
- 线索获得提示：从右侧滑入，像被墨迹污染的纸条，2 秒淡出。

### 3. 对话界面

**16:9 布局**：
- 左侧 35%–40%：角色 2D 立绘区域（竖幅）。
- 底部 28%–35%：对话文本框，占全宽或右 60%。
- 对话选项：浮在对话框上方或右侧，不遮挡角色脸部。

**视觉要求**：
- 对话框：破旧剧本页/烧边纸张，手绘烧边边缘，旧纸底色 `#d8cfc0`。
- 角色名牌：小戏牌样式，暗红底 + 暗金文字。
- 可疑词句：暗红点染、墨迹下划线或轻微抖动（0.2s 循环，位移 1px），不用荧光高亮。
- 推进：纸张翻动或墨字显现效果（逐字显示，Typewriter）。

### 4. 线索 / 证据背包

**16:9 布局**：
- 左侧 30%：线索列表（破损纸片卡片）。
- 右侧 60%：线索详情（旧档案页）。
- 顶部：分类筛选标签（全部/人物/地点/火灾/已使用）。
- 右下角：返回按钮。

**视觉要求**：
- 线索卡片：破损纸片/旧照片/戏票，手绘边缘。
- 已使用线索：盖上暗红印章（“已观皮”圆形章）。
- 未获得线索：用黑色墨块或撕裂空位表示，不用亮色问号。
- 详情面板：旧纸底色，字段标签用暗铜色，内容用深褐色。

### 5. 角色档案

**16:9 布局**：
- 左侧 25%：角色 2D 立绘/脸谱剪影（竖向卡片）。
- 中间 40%：表面身份、行当、已知信息、可疑点。
- 右侧 30%：隐藏画像揭露进度、关联线索列表。

**视觉要求**：
- 档案像侦探笔记 + 戏班名册 + 漫画分镜的结合。
- 每个角色可用少量识别色（青衣=暗红，小生=脏灰蓝，丑=暗绿，净=暗金），整体保持暗色统一。
- 隐藏画像未揭露区域：黑雾/墨块/撕裂纸遮挡，不要用纯灰遮罩。
- 角色图：竖向卡片或半身立绘，不用圆形头像。

### 6. 夜晚观皮界面

**16:9 布局**：
- 左侧 20%：角色档案摘要（立绘小图 + 可疑点 + 进度条）。
- 中央 45%：被黑雾覆盖的隐藏画像（旧供桌相框/戏班牌位框）。
- 右侧 30%：可用线索列表 + 观皮/返回按钮。
- 底部：当前推理提示条。

**视觉要求**：
- 背景：极暗戏台、皮影屏风、黑色幕布或烧毁剧院，背景装饰透明度 ≤ 15%。
- 画像框：旧供桌相框/戏班牌位/破损剧照框，手绘厚重边框，暗铜描边。
- 黑雾：2D 手绘墨雾，多个半透明黑色圆形/不规则形状叠加，边缘手绘晕染感，不用写实粒子烟雾。
- 正确匹配：黑雾从线索应用点向外退散，像墨水被水冲开（透明度渐变 + 缩放）。
- 关键揭露：画面短暂压暗 0.5 秒，只留白脸/伤痕/脸谱裂纹被照亮（中央高亮 + 四周压暗）。

### 7. 黑雾揭露反馈

**反馈设计**：
- **线索选中**：线索卡片暗金描边亮起，200ms。
- **正确匹配**：暗红线条从线索卡片连接到画像区域（手绘线），黑雾局部透明度从 0.85 降到 0.2，持续 600–800ms。伴随环境音压低。
- **错误匹配**：线索卡片被黑墨“吞回”（透明度从 1 降到 0.3 再回升），伴随短促低沉反馈声（或文字提示“这条线索……不相关”）。
- **关键揭露**：画面停顿 0.5–1 秒，环境音压到几乎静音，中央画像局部高亮（白脸/伤痕/裂纹），其余区域压暗。新文字以墨迹显现动画（从透明到不透明，300ms/字）。
- **新目标解锁**：不用现代弹窗，用旧纸条从底部滑入或血色印章盖在 HUD 目标区域。

### 8. 暂停 / 设置界面

**16:9 布局**：
- 全屏半透明黑底覆盖（透明度 0.7），保留当前场景轮廓。
- 中央旧剧本夹/木牌面板，垂直排列选项。

**视觉要求**：
- 面板：旧剧本夹，手绘木纹/纸纹边框。
- Slider：旧纸轨道 + 暗铜滑块，不用 Unity 默认风格。
- Toggle：旧纸勾选框 + 手绘对勾，不用默认 Toggle。
- 选项：继续/音量/文字速度/亮度/返回主菜单/退出。

---

## 八、页面流程说明

```
[主菜单]
   │ 开始游戏
   ▼
[白天探索 HUD] ──→ 调查物件 → [调查小弹窗] → 获得线索 → HUD 右侧滑入纸条提示
   │ 点击 NPC
   ▼
[对话界面] ──→ 推进对话 → 选择选项 → 获得线索/矛盾
   │ 按 Tab / 点击线索入口
   ▼
[线索背包] ──→ 查看详情 → 关联角色 → 筛选/查看状态
   │ 按 C / 点击档案入口
   ▼
[角色档案] ──→ 查看揭露进度 → 点击观皮入口
   │ 夜晚切换 / 手动进入
   ▼
[观皮界面] ──→ 选择线索 → 点击观皮 → 正确/错误反馈 → 黑雾消散/墨吞回
   │ 揭露成功 → 解锁新信息/新目标
   ▼
[新目标提示] → [白天 HUD 更新目标] → 循环
```

---

## 九、Canvas 分层与 Sort Order

| 层级 | Canvas 名称 | Sort Order | 用途 | 常驻/按需 | 阻断输入 | 背景可见 |
|------|------------|------------|------|-----------|----------|----------|
| 0 | BackgroundCanvas | 0 | 主菜单背景、观皮背景、转场背景 | 常驻 | 否 | 是 |
| 10 | WorldHUDCanvas | 10 | 白天 HUD、地点、目标、快捷入口 | 常驻 | 否 | 是 |
| 20 | DialogueCanvas | 20 | 对话框、角色名牌、选项 | 按需 | 是 | 是（半透明） |
| 30 | NormalPanelCanvas | 30 | 线索背包、角色档案、设置页 | 按需 | 是 | 是（半透明） |
| 40 | PopupCanvas | 40 | 线索获得提示、确认框、错误 | 按需 | 部分 | 是 |
| 50 | RevealCanvas | 50 | 黑雾消散、关键揭露、压暗、恐怖闪帧 | 按需 | 是 | 否（覆盖） |
| 60 | SystemCanvas | 60 | 暂停菜单、主菜单覆盖层 | 按需 | 是 | 是（半透明黑底） |

### 规则说明
- **SystemCanvas（60）** 始终在最上，Esc 随时触发。
- **RevealCanvas（50）** 在 Popup 之上，确保关键揭露动画不被弹窗遮挡。
- 打开面板时，该层 Canvas 的 `Graphic Raycaster` 激活，下层（如 WorldHUDCanvas）的 `Graphic Raycaster` 可保留，但场景输入（GameplayInput）被禁用。
- 同一层内面板通过 `CanvasGroup.alpha` + `SetSiblingIndex` 控制显示和层级。

---

## 十、UI 管理结构与各模块职责

### UIManager（单例，全局协调）
- 维护 `UIStack`，记录面板打开/关闭顺序。
- 提供 `OpenPanel(UIPanelType, data)`、`ClosePanel()`、`CloseAll()`、`Back()`。
- 管理各层 Canvas 的显示/隐藏和 Sort Order。
- 处理 Esc 键：栈空 → 打开暂停；非空 → 关闭栈顶面板。
- 协调 `GameplayInput` 的启用/禁用。

### PanelBase（抽象基类，所有面板继承）
- `Init(object data)`：接收数据初始化。
- `Show()`：播放打开动画，设置 `CanvasGroup.interactable = true, blocksRaycasts = true`。
- `Hide()`：播放关闭动画，完成后设置 `CanvasGroup.interactable = false, blocksRaycasts = false`。
- `Refresh()`：根据数据刷新界面。
- `OnClose()`：关闭回调，可通知调用者。
- 每个面板持有 `CanvasGroup` 和 `Animator`（可选）。

### UIPanelType（枚举）
```csharp
public enum UIPanelType
{
    MainMenu, ExploreHUD, DialoguePanel, CluePanel,
    CharacterArchivePanel, ObserveSkinPanel, PauseMenu,
    ConfirmPopup, ClueNotification, RevealEffectOverlay
}
```

### 各面板脚本职责

| 脚本 | 职责 | 说明 |
|------|------|------|
| **HUDController** | 管理白天 HUD：地点、目标、快捷入口、线索获得提示条 | 常驻，Gameplay 时激活，其他面板打开时隐藏 |
| **DialoguePanel** | 管理对话文本、角色名、角色立绘、选项列表、推进逻辑 | 对话打开时暂停场景输入，支持逐字显示和选项选择 |
| **CluePanel** | 管理线索列表、线索详情、筛选标签、关联角色显示 | 数据驱动，根据 ClueData 数组刷新 |
| **CharacterArchivePanel** | 管理角色立绘、表面信息、可疑点、关联线索、揭露进度 | 左侧立绘区，右侧信息面板，隐藏画像区域 |
| **ObserveSkinPanel** | 管理观皮界面：画像区域、黑雾遮罩、线索选择、观皮按钮、反馈 | 核心交互，处理线索匹配判定和揭露进度更新 |
| **RevealEffectController** | 管理黑雾消散、关键揭露、画面压暗、恐怖闪帧 | 在 RevealCanvas 上，独立控制动效，不依赖其他面板 |
| **PopupManager** | 管理确认弹窗、线索获得提示、错误提示 | 模态弹窗，自动关闭或需确认 |

---

## 十一、Prefab 拆分与字段说明

| Prefab | 用途 | 主要子节点 | 绑定字段 | 状态支持 |
|--------|------|-----------|----------|----------|
| **CommonButton_DarkOpera** | 通用按钮 | Image(背景), TMP_Text | `buttonText`, `backgroundImage`, `CanvasGroup` | Hover, Selected, Disabled |
| **MenuButton_TornTicket** | 主菜单按钮 | Image(破损戏票底), TMP_Text, 左侧竖条 | `normalColor`, `highlightColor`, `tornEdgeImage` | Hover, Selected |
| **DialogueBox_OldScript** | 对话文本框 | Image(旧纸底), TMP_Text, 烧边装饰, 继续箭头 | `dialogueText`, `continueArrow`, `burnEdgeImage` | Default, Typing, Waiting |
| **DialogueOption_InkLine** | 对话选项 | Image(旧纸底), TMP_Text, 描边 | `optionText`, `optionData` | Hover, Selected |
| **ClueCard_BurntPaper** | 线索卡片 | Image(破损纸底), TMP_Text(名称), TMP_Text(来源), 印章 | `clueName`, `sourceText`, `stampImage`, `isUsed` | Default, Selected, Used |
| **ClueDetailPanel** | 线索详情 | Image(旧纸底), 字段标签(TMP), 字段内容(TMP), 印章 | `title`, `sourceLocation`, `sourceCharacter`, `description`, `relatedCharacter`, `useStatus` | Default |
| **CharacterArchiveCard** | 角色档案卡片 | Image(立绘区), TMP_Text(姓名), TMP_Text(身份), 进度条 | `portraitImage`, `characterName`, `surfaceIdentity`, `progressBar` | Default, Revealed |
| **CharacterPortraitSlot** | 角色立绘槽 | Image(立绘), Image(脸谱框) | `portraitSprite`, `faceMaskFrame` | Default, Revealed |
| **FaceMaskIcon** | 脸谱图标 | Image(脸谱), 线稿描边 | `maskSprite`, `outlineImage` | Default, Cracked |
| **ObserveSkinPortraitFrame** | 观皮画像框 | Image(旧供桌框), Image(画像底), Image(黑雾遮罩), RevealNodes | `frameImage`, `portraitImage`, `fogMask`, `revealNodes` | Default, Revealing, Revealed |
| **InkFogMask** | 黑雾遮罩 | 多个 Image(2D 墨雾圆形/不规则), CanvasGroup | `fogImages`, `fogAlpha` | Default, Fading, Gone |
| **RevealNode** | 揭露节点 | RectTransform(区域), Image(遮罩), TMP_Text(解锁文字) | `revealArea`, `fogImage`, `unlockedText` | Hidden, Revealing, Revealed |
| **ClueSlot_ObserveSkin** | 观皮线索槽 | Image(旧纸底), TMP_Text(名称), TMP_Text(来源) | `clueName`, `sourceLocation`, `clueId` | Default, Selected, Used |
| **ObjectiveNote** | 目标提示条 | Image(旧纸条底), TMP_Text, CanvasGroup | `objectiveText`, `noteImage` | Default, Showing, Hiding |
| **Popup_ClueAcquired** | 线索获得提示 | Image(墨迹纸条), TMP_Text, CanvasGroup | `notificationText`, `autoHideTime` | Showing, Hiding |
| **Popup_Confirm** | 确认弹窗 | Image(旧纸底), TMP_Text(标题), TMP_Text(内容), 确认/取消按钮 | `titleText`, `contentText`, `confirmButton`, `cancelButton` | Default |
| **SettingsSlider_DarkPaper** | 设置 Slider | Image(旧纸轨道), Image(暗铜滑块), TMP_Text(标签) | `slider`, `fillImage`, `handleImage`, `labelText` | Default |

---

## 十二、数据驱动（ScriptableObject）

### UIStyleConfig（UI 主题配置）
```csharp
[CreateAssetMenu(fileName = "UIStyleConfig", menuName = "HuaPi/UIStyleConfig")]
public class UIStyleConfig : ScriptableObject
{
    public Color backgroundNearBlack;
    public Color backgroundDark;
    public Color backgroundPaper;
    public Color accentRed;
    public Color highlightGold;
    public Color borderBrown;
    public Color textDark;
    public Color textLight;
    public Color textMuted;
    public Color fogBlack;
    public Color inkBlue;
    
    public TMP_FontAsset titleFont;
    public TMP_FontAsset bodyFont;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
}
```

### ClueData（线索数据）
```csharp
[CreateAssetMenu(fileName = "ClueData", menuName = "HuaPi/ClueData")]
public class ClueData : ScriptableObject
{
    public string clueId;
    public string clueName;
    public string description;
    public string sourceLocation;
    public string sourceCharacter;
    public string relatedCharacter;
    public bool isAcquired;
    public bool isUsedInGuanPi;
    public Sprite clueIcon; // 2D手绘图标
}
```

### CharacterData（角色档案数据）
```csharp
[CreateAssetMenu(fileName = "CharacterData", menuName = "HuaPi/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public string characterName;
    public string roleType; // 青衣/小生/文丑/净行
    public string surfaceIdentity;
    public Sprite faceMaskIcon; // 脸谱图标
    public Sprite portraitSprite; // 2D立绘
    public Sprite hiddenPortrait; // 隐藏画像
    public string knownInfo;
    public string suspiciousPoints;
    public float revealProgress; // 0-1
    public Color roleColor; // 角色识别色
}
```

### DialogueData（对话数据）
```csharp
[System.Serializable]
public class DialogueLine
{
    public string speakerId;
    public string text;
    public string[] clueIdsToGrant;
    public string[] highlightedKeywords; // 需要暗红标记的关键词
    public int[] nextOptionIds;
}

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public int nextNodeId;
    public string[] requiredClueIds;
    public string[] clueIdsToConsume;
}
```

### ObserveSkinRevealData（观皮揭露节点）
```csharp
[CreateAssetMenu(fileName = "ObserveSkinRevealData", menuName = "HuaPi/ObserveSkinRevealData")]
public class ObserveSkinRevealData : ScriptableObject
{
    public string nodeId;
    public string relatedCharacterId;
    public string requiredClueId;
    public Rect revealArea; // 在隐藏画像上的归一化区域 (0-1)
    public string revealedText;
    public bool triggersNewObjective;
    public string newObjectiveId;
    public float fogFadeDuration = 0.8f;
}
```

### ObjectiveData（目标数据）
```csharp
[CreateAssetMenu(fileName = "ObjectiveData", menuName = "HuaPi/ObjectiveData")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveId;
    public string objectiveText;
    public int stage; // 当前阶段
    public string[] completionConditions;
    public bool isCompleted;
}
```

### 数据驱动流程
- **线索列表刷新**：`CluePanel` 读取 `ClueData[]`，根据 `isAcquired` 过滤，根据 `isUsedInGuanPi` 显示印章。
- **角色档案显示**：`CharacterArchivePanel` 读取 `CharacterData`，根据 `revealProgress` 显示隐藏画像区域。
- **对话选项显示**：`DialoguePanel` 读取 `DialogueData`，根据 `requiredClueIds` 判断选项是否可用。
- **观皮匹配判定**：`ObserveSkinPanel` 比对玩家选中的 `clueId` 与 `ObserveSkinRevealData.requiredClueId`。
- **黑雾揭露进度**：`RevealEffectController` 根据 `revealProgress` 调整 `InkFogMask` 的透明度。
- **新目标提示**：`ObjectiveData` 更新后，`HUDController` 触发 `ObjectiveNote` 旧纸条滑入。

---

## 十三、输入与交互

### 默认交互
- **鼠标点击**：调查物件、选择对话选项、打开面板、点击按钮、推进对话。
- **悬停**：显示物件名称、按钮高亮、线索卡片预览。

### 键盘快捷键
- `Esc`：打开暂停或关闭当前面板/返回上一级。
- `Tab`：打开/关闭线索背包。
- `C`：打开/关闭角色档案。
- `Space` / 左键：推进对话。

### 输入冲突处理
- **UI 与场景分层**：`UIManager` 在打开面板时调用 `GameplayInput.Disable()`，关闭时 `Enable()`。使用 Unity 的 Input System 或自定义输入管理器实现。
- **Graphic Raycaster 阻挡**：UI 层 `Graphic Raycaster` 的 `Blocking Objects` 设为 `All`，`Blocking Mask` 包含 UI 层，阻挡下方场景点击。
- **对话模式**：`DialoguePanel` 打开时，`GameplayInput` 禁用，仅接受对话输入（推进、选项选择）。
- **观皮模式**：`ObserveSkinPanel` 打开时，禁用其他面板入口，仅接受线索选择和观皮按钮输入。
- **拖拽 vs 点击观皮**：比赛 Demo 建议优先**点击选择**（点击线索 → 点击观皮），拖拽可作为后续扩展。点击更稳定，浏览器端更可靠。
- **浏览器端适配**：
  - 监听窗口失焦/聚焦事件，失焦时自动暂停（可选）。
  - 监听窗口缩放，Canvas Scaler 自动适配。
  - 全屏切换时保持 UI 可用，按钮大小不随缩放异常变化。

---

## 十四、动效与恐怖反馈

### MVP 必须实现的动效

1. **面板淡入/淡出**
   - 实现：`CanvasGroup.alpha` 从 0 → 1，150–300ms。
   - 打开：旧纸面板从 `scaleY=0.95` + `alpha=0` 到 `scaleY=1` + `alpha=1`，轻微弹性缓动。

2. **按钮 Hover / Selected**
   - Hover：暗金描边从 `alpha=0.3` 到 `alpha=1`，100ms。
   - Selected：左侧竖条亮起 + 暗金描边，200ms。

3. **线索获得提示**
   - 旧纸条从右侧滑入（`anchoredPosition.x` 从 +100 到 0），`alpha` 从 0 到 1，200ms。
   - 停留 2 秒后，滑出 + 淡出，200ms。

4. **观皮正确匹配反馈**
   - 暗红手绘线从线索卡片连接到画像区域（`Image` 的 `fillAmount` 从 0 到 1，300ms）。
   - 黑雾局部 `CanvasGroup.alpha` 从 0.85 到 0.2，600–800ms。
   - 环境音压低（通过 AudioManager）。
   - 新文字以墨迹显现（`alpha` 从 0 到 1，逐字 300ms）。

5. **观皮错误匹配反馈**
   - 线索卡片 `CanvasGroup.alpha` 从 1 → 0.3 → 1，200ms，伴随轻微抖动（`anchoredPosition` 偏移 ±3px，回弹）。
   - 文字提示：“这条线索……不相关。” 从卡片下方淡入，2 秒淡出。

6. **黑雾局部消散**
   - `InkFogMask` 中对应区域的 `Image` 的 `alpha` 从 0.85 渐变到 0，500–800ms。
   - 可用协程或 DOTween 实现。

### 后续 Polish 动效（非 MVP）

1. **纸张翻页**：面板打开/关闭时像旧纸页翻起，用 `RectTransform` 的 `rotation` 和 `scale` 模拟。
2. **墨迹扩散边框**：面板边框从一角开始，墨迹像水一样扩散到四周，使用 `Image` 的 `fillAmount` 或 Shader。
3. **角色立绘呼吸/阴影抖动**：角色立绘轻微缩放（`scale` 从 1 到 1.02，循环 3s），阴影轻微位移，营造不安感。
4. **脸谱裂纹闪现**：关键揭露时，已揭露区域的脸谱出现裂纹线条（`alpha` 从 0 到 1 到 0，快速闪烁 3 次）。
5. **短暂闪帧**：关键揭露时，画面全白 0.05 秒，然后回到压暗画面，像恐怖漫画的闪回。

### 恐怖反馈原则
- 少用跳吓（jump scare），多用**停顿、压暗、局部显现、声音留白**。
- UI 动效要像手绘漫画和水墨层在变化，而非现代数字特效。
- 避免：科幻扫描线、霓虹光效、粒子爆炸、现代弹窗动画。
- 节奏：普通操作快（150–300ms），关键揭露慢（停顿 + 渐变，1–2 秒）。

---

## 十五、MVP 优先实现顺序

1. **基础 UGUI 框架**
   - Canvas 分层（Background/WorldHUD/Dialogue/NormalPanel/Popup/Reveal/System）。
   - UIManager 单例 + UIStack。
   - PanelBase 基类。
   - UIStyleConfig ScriptableObject。

2. **通用 Prefab**
   - CommonButton_DarkOpera、MenuButton_TornTicket、DialogueBox_OldScript、Popup_Confirm。

3. **主菜单**
   - 2D 插画背景（临时占位图）+ 标题 + 菜单按钮 + 打开/关闭动画。

4. **白天探索 HUD**
   - 地点标签、目标提示、快捷入口、暂停按钮。
   - 可调查物件高亮（虚线圆环 + 名称弹出）。
   - 线索获得提示条（旧纸条滑入）。

5. **对话界面**
   - 角色名牌、角色立绘区域、对话文本框、选项列表。
   - 推进逻辑（Space/点击）、逐字显示（Typewriter）。
   - 可疑词句暗红标记。

6. **线索系统 UI**
   - 线索列表（ClueCard_BurntPaper）、详情面板（ClueDetailPanel）。
   - 筛选标签（全部/人物/地点/火灾/已使用）。
   - 已使用线索暗红印章。

7. **角色档案 UI**
   - 角色立绘区、表面信息、可疑点、关联线索。
   - 隐藏画像区域（黑雾遮罩 + 进度条）。

8. **观皮界面**
   - 画像框（ObserveSkinPortraitFrame）、黑雾遮罩（InkFogMask）。
   - 线索选择（ClueSlot_ObserveSkin）、观皮按钮。
   - 正确/错误匹配反馈（黑雾消散/墨吞回）。

9. **黑雾揭露动效**
   - 至少实现一个可用的局部揭露效果（透明度渐变）。
   - 关键揭露的停顿 + 压暗。

10. **暂停/设置**
    - 半透明黑底覆盖 + 旧纸面板。
    - 旧纸风格 Slider 和 Toggle。
    - 音量、文字速度、亮度、返回、退出。

11. **统一视觉走查**
    - 字体、颜色、描边、动效节奏统一检查。
    - 浏览器端测试缩放适配。

---

## 十六、核心设计原则总结

> **在 16:9 的 2D 漫画恐怖界面中，让玩家清楚地调查、阅读、整理证据，并在黑雾退散时感到自己正在揭开戏曲脸谱之下的禁忌真相。**

1. **清晰可用 > 氛围表达 > 复杂装饰**：玩家必须知道当前在哪、能做什么、下一步去哪。
2. **恐怖来自低明度和局部对比**：70% 以上近黑，暗红和暗金只用于引导注意力。
3. **2D 手绘统一**：所有 UI 元素与角色立绘风格一致，禁止现代扁平。
4. **黑雾是核心记忆点**：2D 手绘墨雾，正确匹配时向外退散，错误时被墨吞回。
5. **动效服务于情绪**：普通操作快，关键揭露慢（停顿 + 压暗 + 局部显现）。
6. **浏览器端优先**：复杂 Shader 降级为透明度渐变，确保可读性和点击响应。

---

*UI 设计师：像素君*
*版本：V2.0 恐怖漫画版*
*日期：2026-06-22*
*状态：可直接指导 Unity UGUI 实现*
