# 文件结构说明

## 提交入口

组委会请从以下目录进入项目：

```text
Tencent_Submission_GitHub_Link_20260716/huapi
```

这是一个标准 Unity 工程目录，包含 `Assets`、`Packages`、`ProjectSettings` 三个必要目录。

## Unity 工程结构

```text
huapi/
├─ Assets/
│  ├─ Art/                 当前 Demo 使用的主要美术、背景、角色、UI 图片
│  ├─ Audio/               BGM 和音频资源
│  ├─ Data/                ScriptableObject 数据资产
│  ├─ Fonts/               项目字体资源
│  ├─ Prefabs/             角色、UI、NPC、调查点等 Prefab
│  ├─ Resources/           运行时加载的数据、字体、线索、对话
│  ├─ Scenes/              Unity 场景文件
│  ├─ Scripts/             主要 C# 源码
│  ├─ Settings/            渲染和项目资源设置
│  └─ TextMesh Pro/        TextMesh Pro 必要资源
├─ Packages/
│  ├─ manifest.json
│  └─ packages-lock.json
└─ ProjectSettings/
   └─ Unity 工程配置文件
```

## C# 源码分区

```text
Assets/Scripts/
├─ Character/              角色数据
├─ Data/                   线索、对话、角色、揭露数据定义
├─ DayNight/               昼夜流程
├─ Dialogue/               NPC 对话和对话 UI 管理
├─ Editor/                 数据生成、场景生成、构建辅助工具
├─ Huapi/                  旧版 Demo 系统和人物档案逻辑
├─ Interaction/            可交互接口
├─ Investigation/          调查点、线索收集、调查 UI
├─ Manager/                游戏管理、存档、镜头、氛围控制
├─ Player/                 玩家控制
└─ UI/                     当前 Demo 的主 UI 和面板
```

## 文档目录

```text
docs/design/
├─ PROGRESS_LOG.md
├─ game_design_day1_complete.md
├─ game_design_MVP_story.md
├─ game_design_puzzles_paper_bride_reference.md
├─ game_story_case01_day1_script.md
└─ game_story_script_MVP.md
```

这些文档用于说明项目背景、剧情、玩法、谜题设计和当前完成进度。

## 不属于提交入口的原仓库目录

原仓库根目录中的以下目录主要是开发过程材料或缓存，不建议作为组委会源码入口：

- `outputs/`：PPT、PDF、Excel、渲染预览等导出物
- `ui_update/`：UI 原始大图和过程素材
- `pic/`：原始角色和背景素材
- `prompt/`：提示词草稿
- `ui/`：早期 UI mockup 和 HTML 原型
- `Library/`、`Logs/`、`UserSettings/`：Unity 本地生成缓存

源码评审和运行请以 `Tencent_Submission_GitHub_Link_20260716/huapi` 为准。
