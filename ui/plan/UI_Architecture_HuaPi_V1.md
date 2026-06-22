# 《画皮》UI 风格与 Unity UGUI 架构方案

## 一、UI 风格关键词

暗红、黑墨、旧纸、戏台、脸谱、皮影、压抑、悬疑、克制。

---

## 二、色彩方案

| 用途 | 色值 | 说明 |
|------|------|------|
| **背景深** | `#1a1210` | 主背景色，关联水墨与暗场 |
| **背景中** | `#2a1a14` | 面板底色，带暖调深褐 |
| **背景浅** | `#f5f0e8` | 旧纸色，信息面板和对话框底色 |
| **强调色** | `#8b2020` | 暗红，用于戏台帷幕、血色记忆、关键线索 |
| **高亮** | `#c9a96e` | 暗金，用于选中态、重要按钮、标题 |
| **描边** | `#6b4423` | 暗铜棕，用于面板边框和按钮描边 |
| **正文深** | `#3a2a1a` | 旧纸上的正文 |
| **正文浅** | `#f5f0e8` | 深色面板上的正文 |
| **禁用/次要** | `#9c8a6e` | 次要文字、未选中项 |
| **黑雾** | `#0a0808` | 纯黑雾层，几乎不透 |
| **水墨灰** | `#4a3a3a` | 黑雾中的纹理层次 |

### 使用规则
- 深色面板（背景中 `#2a1a14`）配金色/旧纸文字，用于菜单、HUD、档案。
- 浅色面板（旧纸色 `#f5f0e8`）配深褐文字，用于对话框、线索详情。
- 暗红 `#8b2020` 仅用于关键按钮、矛盾标记、进度条、错误反馈，面积不超过 5%。
- 暗金 `#c9a96e` 用于选中态、当前目标、高亮线索，面积不超过 10%。

---

## 三、字体与文字层级

### 字体选择
- **标题/戏曲感文字**：使用支持中文的书法/牌匾字体（如站酷庆科黄油体、思源宋体 Heavy）。Unity 中可内嵌 TTF，WebGL 构建时将字体 Subset 裁剪后打包。
- **正文**：思源黑体/Noto Sans SC Regular，确保长段中文清晰可读。
- **数字/系统提示**：使用等宽或干净的无衬线字体（如 Roboto Mono、Noto Sans SC Medium）。

### 字号层级（基于 1920×1080 参考分辨率）

| 层级 | 字号 | 字重 | 用途 |
|------|------|------|------|
| 主标题 | 36–42px | 500 | 游戏标题、章节标题 |
| 副标题 | 24–30px | 500 | 面板标题、角色名 |
| 线索名 | 18–20px | 500 | 线索卡片标题、档案标签 |
| 正文 | 15–16px | 400 | 描述文本、对话内容 |
| 按钮文字 | 15–16px | 500 | 按钮标签、菜单项 |
| 系统提示 | 12–13px | 400 | 提示、来源、状态、快捷键 |

### 对话文本阅读优化
- 行高 1.8–2.0，确保长句呼吸感。
- 每行不超过 24–26 个汉字。
- 关键矛盾句用暗红 `#8b2020` 或暗金 `#c9a96e` 加粗，配合轻微下划线或底纹标记。

### 字体缺失处理（WebGL）
- 所有字体内嵌到 `Assets/Fonts/`，使用 TextMeshPro 的 Atlas 预生成。
- 若构建后发现字体缺失，TMP 会回退到系统默认字体。建议在 `TMP Settings` 中指定全局 Fallback 字体为 Noto Sans SC。

---

## 四、图标与装饰元素方向

| 元素 | 使用方向 | 注意 |
|------|----------|------|
| **京剧脸谱** | 主菜单标题两侧、角色档案头像边框、章节标记 | 仅作为边框或角落装饰，不铺满 |
| **皮影剪影** | 转场动画、空状态提示、档案背景、观皮界面背景 | 用半透明深色剪影，避免抢眼 |
| **黑雾/水墨** | 观皮遮罩、线索揭露动效、面板打开时的墨迹显现 | 用粒子或动画 Shader，避免静态贴图 |
| **纸张破损/烧痕** | 线索面板边缘、旧照片边框、档案页面 | 仅用于边缘，不覆盖内容区 |
| **戏台帷幕** | 主菜单背景、过场背景 | 静态或微动，不抢按钮焦点 |
| **烛光/灯笼** | 氛围光、悬停反馈、选中态 | 用暗金/暖色，避免过亮 |

---

## 五、核心页面设计

### 1. 主菜单
- **布局**：标题居中，菜单按钮垂直排列于中下方，背景为暗红帷幕 + 黑雾中的角色剪影。
- **按钮风格**：旧木牌/戏票形状，圆角 3px，底色 `#2a1a14`，描边 `#6b4423`，选中态加暗金 `#c9a96e` 描边 + 左侧竖条。
- **背景**：左右帷幕（SVG/纹理），底部淡入黑雾，中央偏上悬挂模糊脸谱剪影。

### 2. 白天探索 HUD
- **布局**：左上地点+目标，右上快捷入口（线索/档案），右下时间阶段标识，底部中央可调查物件高亮。
- **风格**：轻量旧纸标签 + 深色小面板。地点和目标用旧纸底色，入口用深色面板。
- **高亮反馈**：可调查物件用暗金虚线圆环 + 物件名称浮现。线索获得提示用顶部滑入暗金文字条，2 秒淡出。

### 3. 对话界面
- **布局**：左侧角色立绘（占画面 1/4），右侧旧纸卷对话框（占画面 1/2），下方选项列表。
- **对话框**：旧纸色底色 `#f5f0e8`，上下边缘加浅灰横纹模拟纸卷，圆角 3px。
- **选项**：旧纸卡片，选中态暗金描边，普通态浅灰描边。关键追问选项可标暗红。
- **矛盾标记**：对话文本中的关键句用暗红加粗 + 轻微底纹。

### 4. 线索 / 证据背包
- **布局**：左 1/3 线索列表+筛选标签，右 2/3 线索详情。
- **列表项**：深色卡片，选中态旧纸色 + 暗金描边，已使用线索标暗红点。
- **详情面板**：旧纸底色，分字段展示：名称、来源地点、来源人物、描述、关联角色、使用状态。
- **筛选标签**：暗红胶囊（选中）/ 深色边框（未选中）。

### 5. 角色档案
- **布局**：左侧角色头像+脸谱标识，右侧信息面板（表面身份、已知信息、可疑点、关联线索、揭露进度）。
- **风格**：侦探笔记/戏班名册，旧纸底色，字段用暗铜标签 + 深褐文字。
- **隐藏画像区域**：保留矩形区域，底部显示揭露进度条，未揭露时覆盖黑雾纹理。

### 6. 夜晚观皮界面
- **布局**：中央隐藏画像区（黑雾覆盖），左侧线索选择列表，右侧操作按钮（观皮/查看档案）。
- **画像区**：深色边框，内部被 `#0a0808` 黑雾覆盖，已揭露区域局部显现。底部进度条。
- **线索选择**：点击选中，支持拖拽到画像区。
- **氛围**：背景用暗场戏台/烛光，黑雾用粒子或动画 Shader 实现水墨扩散感。

### 7. 黑雾揭露反馈
- **普通线索获得**：HUD 顶部轻量提示条，暗金文字，2 秒淡出。
- **观皮成功**：黑雾从线索应用点向外退散，露出隐藏画像局部。伴随短促停顿、环境音降低。新文字以墨迹显现动画展示。
- **观皮失败**：线索卡片轻微暗下，墨迹回流动画，温和提示“这条线索似乎不相关”。
- **关键秘密揭露**：短暂黑屏过渡，画像放大，新揭露区域高亮，文字逐字显现。

### 8. 暂停 / 设置界面
- **布局**：半透明黑底覆盖，中央旧纸面板，垂直排列选项。
- **选项**：继续、音量滑块、文字速度、画面亮度、返回主菜单、退出。
- **风格**：与主菜单一致，旧木牌按钮，暗金选中态。

---

## 六、页面流程说明

```
[主菜单]
   │ 开始游戏
   ▼
[白天探索 HUD] ──→ 点击物件 → [调查小弹窗] → 获得线索 → HUD 提示
   │ 点击 NPC
   ▼
[对话界面] ──→ 选择选项 → 推进对话 → 获得线索/矛盾
   │ 按 Tab / 点击线索入口
   ▼
[线索背包] ──→ 查看详情 → 关联角色
   │ 按 C / 点击档案入口
   ▼
[角色档案] ──→ 查看揭露进度
   │ 夜晚切换 / 手动进入观皮
   ▼
[观皮界面] ──→ 选择线索 → 观皮 → 黑雾消散 / 失败反馈
   │ 揭露成功 → 解锁新信息
   ▼
[新目标 / 新白天] → 循环
```

---

## 七、Canvas 分层方案

| 层级（Sort Order） | 用途 | 说明 |
|--------------------|------|------|
| **0 - 背景层** | 菜单背景、观皮背景、转场底图 | 静态图片或全屏 RawImage，不响应交互 |
| **10 - 场景 HUD 层** | 白天探索 HUD、当前目标、地点信息 | 常驻，Gameplay 时显示，Pause 时隐藏 |
| **20 - 常规面板层** | 线索背包、角色档案、对话界面 | 按需打开，Gameplay 时可能暂停输入 |
| **30 - 弹窗层** | 确认框、线索获得提示、错误提示 | 短暂显示，自动关闭或需确认 |
| **40 - 揭露/过场层** | 黑雾消散、关键秘密揭露、章节转场 | 最高视觉优先，可能覆盖所有内容 |
| **50 - 系统菜单层** | 暂停、设置、主菜单覆盖层 | 始终可访问，Esc 触发，暂停游戏时间 |

### 层级顺序规则
- 每一层使用独立的 Canvas，Sort Order 递增，避免遮挡错误。
- 系统菜单层（50）始终在最上，确保任何时候可暂停。
- 揭露/过场层（40）在弹窗层之上，确保关键动画不被遮挡。
- 同一层内的面板通过 `SetSiblingIndex` 或 `CanvasGroup.alpha` 控制显示顺序。

---

## 八、UIManager 与 PanelBase 职责

### UIManager（单例）
- 维护 `PanelStack`，记录当前打开的面板顺序。
- 提供 `OpenPanel(UIPanelType type, object data = null)`，`ClosePanel()`，`CloseAllPanels()`。
- 管理 `Canvas` 各层的显示/隐藏。
- 处理 Esc 键：优先关闭栈顶面板，若栈空则打开暂停菜单。
- 协调 `GameplayInput` 的禁用/启用（当面板打开时暂停场景输入）。

### PanelBase（抽象基类）
- `Init()`：初始化，接收数据。
- `Show()`：播放打开动画，设置 `CanvasGroup.interactable = true`。
- `Hide()`：播放关闭动画，完成后 `CanvasGroup.interactable = false`。
- `Refresh()`：根据数据刷新界面。
- `OnClose()`：关闭回调，用于通知调用者。
- 每个面板持有自己的 `CanvasGroup` 和 `Animator`（可选）。

### UIPanelType（枚举）
```csharp
public enum UIPanelType
{
    MainMenu, ExploreHUD, Dialogue, ClueInventory, 
    CharacterProfile, GuanPi, PauseMenu, ConfirmPopup, ClueNotification
}
```

### 面板栈行为
- 打开面板时压栈，关闭时弹栈。
- 从栈顶面板返回时，恢复下一层面板的交互。
- 弹窗（如 ConfirmPopup）不压栈，而是作为模态覆盖层显示。

---

## 九、主要 Prefab 列表与职责

| Prefab | 职责 | 主要子节点 | 暴露字段 |
|--------|------|-----------|----------|
| **UI_Button_Generic** | 通用按钮 | Image(背景), TMP_Text | `buttonText`, `onClickEvent` |
| **UI_Button_MainMenu** | 主菜单按钮 | Image(背景), TMP_Text, 左侧竖条 | `highlightColor`, `normalColor` |
| **UI_DialogueBox** | 对话文本框 | 旧纸底图, TMP_Text, 继续箭头 | `dialogueText`, `continueArrow` |
| **UI_DialogueOption** | 对话选项卡片 | 旧纸底图, TMP_Text | `optionText`, `optionData` |
| **UI_ClueCard** | 线索列表项 | 底图, 名称文本, 来源文本, 状态点 | `clueName`, `sourceText`, `isUsedDot` |
| **UI_ClueDetail** | 线索详情面板 | 字段标签, 字段内容 | `title`, `sourceLocation`, `sourceCharacter`, `description`, `relatedCharacter`, `useStatus` |
| **UI_CharacterProfileCard** | 角色档案卡 | 头像, 脸谱框, 信息字段, 进度条 | `characterName`, `faceMask`, `infoFields`, `progressBar` |
| **UI_HiddenPortrait** | 隐藏画像区域 | 画像底图, 黑雾遮罩, 已揭露区域 | `portraitImage`, `fogMask`, `revealedAreas` |
| **UI_GuanPiSlot** | 观皮线索槽 | 底图, 线索名称 | `slotClueId`, `slotClueName` |
| **UI_ClueNotification** | 线索获得提示 | 底图, 文字, 图标 | `notificationText`, `autoHideTime` |
| **UI_ConfirmPopup** | 确认弹窗 | 标题, 内容, 确认/取消按钮 | `titleText`, `contentText`, `onConfirm`, `onCancel` |
| **UI_HighlightMarker** | 可调查物件高亮 | 虚线圆环, 名称标签 | `markerName`, `targetObject` |
| **UI_ProgressDot** | 揭露进度点 | 暗金/暗红圆点 | `isRevealed`, `revealAnimation` |

---

## 十、关键数据结构（ScriptableObject）

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
}
```

### CharacterProfileData（角色档案数据）
```csharp
[CreateAssetMenu(fileName = "CharacterProfileData", menuName = "HuaPi/CharacterProfileData")]
public class CharacterProfileData : ScriptableObject
{
    public string characterId;
    public string characterName;
    public string surfaceIdentity;
    public string faceMaskType;
    public string knownInfo;
    public string suspiciousPoints;
    public Sprite hiddenPortrait;
    public float revealProgress; // 0–1
}
```

### GuanPiNodeData（观皮揭露节点）
```csharp
[CreateAssetMenu(fileName = "GuanPiNodeData", menuName = "HuaPi/GuanPiNodeData")]
public class GuanPiNodeData : ScriptableObject
{
    public string nodeId;
    public string relatedCharacterId;
    public string requiredClueId;
    public Rect revealArea; // 在隐藏画像上的区域
    public string revealedText;
    public bool triggersNewObjective;
    public string newObjectiveId;
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
    public string[] highlightedKeywords;
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

### UIThemeData（UI 主题配置）
```csharp
[CreateAssetMenu(fileName = "UIThemeData", menuName = "HuaPi/UIThemeData")]
public class UIThemeData : ScriptableObject
{
    public Color backgroundDark;
    public Color backgroundMedium;
    public Color backgroundPaper;
    public Color accentRed;
    public Color highlightGold;
    public Color borderBrown;
    public Color textDark;
    public Color textLight;
    public Color textMuted;
    public Color fogBlack;
    // ... 其他主题色
}
```

---

## 十一、交互方式与输入冲突处理

### 鼠标交互（默认）
- 左键点击：调查物件、选择对话选项、打开面板、点击按钮、推进对话。
- 悬停：显示物件名称、按钮高亮、线索卡片预览。
- 拖拽（可选）：将线索拖拽到观皮区域。

### 键盘快捷键
- `Esc`：打开暂停菜单，或关闭当前面板/返回上一级。
- `Tab`：打开/关闭线索背包。
- `C`：打开/关闭角色档案。
- `Space` / 左键：推进对话。

### 输入冲突处理
- **Gameplay 与 UI 分层**：`UIManager` 在打开面板时调用 `GameplayInput.Disable()`，关闭时 `Enable()`。
- **Graphic Raycaster 优先级**：确保 UI 层的 `Graphic Raycaster` 的 `Blocking Objects` 设置为 `All`，`Blocking Mask` 包含 UI 层，阻挡下方场景输入。
- **对话模式**：对话打开时，场景调查和移动输入被禁用，仅接受对话相关输入（推进、选项选择）。
- **观皮模式**：观皮界面打开时，仅接受线索选择和观皮按钮输入，禁用其他面板入口。

---

## 十二、动效与反馈设计

### 必须实现（MVP）
1. **面板淡入/淡出**：使用 `CanvasGroup.alpha` + 协程，150–300ms。
2. **线索获得提示**：从顶部滑入，暗金文字，2 秒后淡出。
3. **按钮悬停**：暗金描边亮起，100ms 过渡。
4. **观皮成功**：黑雾遮罩局部透明度变化（从 0.85 到 0），露出下方画像。持续 500–800ms。
5. **观皮失败**：线索卡片轻微抖动（`RectTransform.anchoredPosition` 偏移 3px，回弹），200ms。

### 后续 Polish（非 MVP）
1. **面板打开**：纸张展开动画（ScaleY 从 0 到 1，带弹性缓动）。
2. **面板关闭**：墨迹收回动画（遮罩从右向左收缩）。
3. **黑雾消散**：水墨扩散 Shader 效果，粒子向外飘散。
4. **关键揭露**：画面短暂缩放、环境音停顿、文字逐字显现（Typewriter 效果）。
5. **转场**：皮影剪影过场，黑雾从中央向外扩散。

### 性能注意
- 所有动画优先使用 `DOTween` 或协程 + `CanvasGroup`，避免 Animator 复杂状态机。
- 黑雾效果在 WebGL 上可用简单透明度渐变代替 Shader，确保兼容性。

---

## 十三、MVP 优先实现顺序

1. **基础 UGUI 框架**：Canvas 分层（0/10/20/30/40/50），UIManager 单例，PanelBase 基类，UI 主题数据。
2. **通用 Prefab**：旧木牌按钮、旧纸对话框、确认弹窗、提示条。
3. **主菜单**：背景图 + 菜单按钮 + 打开/关闭动画。
4. **白天探索 HUD**：地点标签、目标面板、快捷入口、物件高亮、线索获得提示。
5. **对话界面**：角色名、立绘区域、对话文本、选项、推进逻辑。
6. **线索系统 UI**：线索列表、详情面板、筛选标签、线索获得提示。
7. **角色档案 UI**：角色基础信息、可疑点、关联线索、揭露进度条。
8. **观皮界面**：隐藏画像区域、黑雾遮罩、线索选择、观皮按钮、结果反馈。
9. **暂停/设置**：覆盖层、音量、文字速度、亮度、返回/退出。
10. **统一视觉**：字体、颜色、描边、动效节奏统一走查。

---

## 十四、设计原则总结

> **清晰可用 > 氛围表达 > 复杂装饰。**

- 每个界面必须让玩家知道：当前在哪、能做什么、下一步去哪。
- 暗红和暗金只用于引导注意力，不铺满画面。
- 黑雾和脸谱是记忆点，但真相揭露时的反馈才是情绪高潮。
- 所有动效必须服务于信息传达，不是炫技。
- 浏览器端构建时，优先保证可读性和点击响应，Shader 和复杂动画作为降级方案。

---

*UI 设计师：像素君*
*方案日期：2026-06-22*
*版本：MVP 比赛版*
