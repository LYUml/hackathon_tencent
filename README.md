# hackathon_tencent

这是《画皮》腾讯 Hackathon 项目的总源码工作区。仓库根目录保留了完整开发过程中的源码、素材、文档、导出物和中间文件，便于回溯开发过程。

## 组委会源码提交入口

请以以下目录作为主要提交和评审入口：

```text
Tencent_Submission_GitHub_Link_20260716/
```

该目录已经单独整理为面向组委会的源码结构，包含可打开的 Unity 工程和必要说明：

```text
Tencent_Submission_GitHub_Link_20260716/
├─ README_组委会说明.md
├─ FILE_STRUCTURE.md
├─ huapi/
│  ├─ Assets/
│  ├─ Packages/
│  └─ ProjectSettings/
└─ docs/
   └─ design/
```

打开方式：

1. 使用 Unity Hub 打开 `Tencent_Submission_GitHub_Link_20260716/huapi`。
2. 推荐 Unity 版本：`6000.3.10f1`。
3. 主场景：`Assets/Scenes/SampleScene.unity`。
4. 进入 Play Mode 运行 Demo。

## 根目录结构说明

```text
hackathon_tencent/
├─ Tencent_Submission_GitHub_Link_20260716/  组委会主要提交目录
├─ game/                                     原始 Unity 开发工程
├─ ui_update/                                UI 美术过程素材和大图
├─ pic/                                      原始角色、背景等素材
├─ ui/                                       早期 UI mockup、HTML 原型和 UI 规划
├─ outputs/                                  PPT、PDF、Excel、预览图等导出物
├─ prompt/                                   开发过程中的提示词草稿
├─ idea/                                     早期想法记录
├─ character/                                角色参考图
├─ font/                                     字体原始文件
├─ Library/ Logs/ UserSettings/              Unity 本地缓存和用户设置
└─ *.md                                      剧情、玩法、谜题和进度文档
```

## 重要说明

- `Tencent_Submission_GitHub_Link_20260716/` 是本次提交给腾讯组委会查看的主要源码文件夹。
- 根目录下其它目录大多是过程性文件、原始素材、导出物或本地缓存，不建议作为评审入口。
- Unity 工程运行所需的核心结构是 `huapi/Assets`、`huapi/Packages`、`huapi/ProjectSettings`。
- `Library/`、`Temp/`、`Logs/`、`UserSettings/`、`*.csproj`、`*.slnx` 都是 Unity 或 IDE 生成内容，不属于必须提交的源码。
