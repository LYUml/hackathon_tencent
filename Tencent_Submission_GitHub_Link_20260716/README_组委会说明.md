# 《画皮》腾讯 Hackathon 源代码提交说明

本目录是面向组委会整理的项目源码入口。由于完整 Unity 工程包含大量美术资源，最终提交使用 GitHub 仓库链接；组委会查看源码时请以本目录为准。

## 项目信息

- 项目名称：画皮
- 类型：Unity 2D 剧情推理 / 探索解谜 Demo
- 推荐 Unity 版本：6000.3.10f1
- 主工程目录：`huapi`
- 主场景：`huapi/Assets/Scenes/SampleScene.unity`

## 打开方式

1. 使用 Unity Hub。
2. 选择打开本目录下的 `huapi` 文件夹。
3. Unity 会根据 `huapi/Packages/manifest.json` 和 `huapi/Packages/packages-lock.json` 自动恢复依赖。
4. 打开场景 `huapi/Assets/Scenes/SampleScene.unity`。
5. 进入 Play Mode 体验 Demo。

## 目录说明

```text
Tencent_Submission_GitHub_Link_20260716/
├─ README_组委会说明.md
├─ FILE_STRUCTURE.md
├─ huapi/
│  ├─ Assets/              Unity 运行所需源码、场景、Prefab、数据和美术资源
│  ├─ Packages/            Unity Package 依赖声明
│  └─ ProjectSettings/     Unity 工程设置
└─ docs/
   └─ design/              项目剧情、玩法、谜题和进度说明
```

## 主要源码位置

- UI 和主 Demo 流程：`huapi/Assets/Scripts/UI/HuapiFullUIInstaller.cs`
- 玩家控制：`huapi/Assets/Scripts/Player/PlayerController.cs`
- 对话系统：`huapi/Assets/Scripts/Dialogue`
- 调查与线索：`huapi/Assets/Scripts/Investigation`
- 数据定义：`huapi/Assets/Scripts/Data`
- 编辑器生成工具：`huapi/Assets/Scripts/Editor`

## 未放入本目录的内容

原仓库中还保留了一些开发过程材料，例如 UI 草稿、提示词、导出 PPT/PDF、Excel 清单、重复原始素材和 Unity 本地缓存。它们不是打开源码所必需的内容，因此没有放入本提交目录。

Unity 本地生成目录也不应作为源码提交：

- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`
- `*.csproj`
- `*.slnx`