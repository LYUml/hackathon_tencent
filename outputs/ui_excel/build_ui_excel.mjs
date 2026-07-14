import fs from "node:fs/promises";
import { SpreadsheetFile, Workbook } from "@oai/artifact-tool";

const outputDir = "D:/Unity/tx/hackathon_tencent/outputs/ui_excel";
await fs.mkdir(outputDir, { recursive: true });

const workbook = Workbook.create();
const sheet = workbook.worksheets.add("UI缺口清单");
const summary = workbook.worksheets.add("交付优先级");
const guide = workbook.worksheets.add("操作指南");

sheet.showGridLines = false;
summary.showGridLines = false;
guide.showGridLines = false;

const rows = [
  ["优先级", "UI模块", "当前状态", "还缺什么", "美术需要产出", "关键状态/变体", "用途", "建议文件名", "备注"],
  ["P0", "探索界面实际截图式 UI", "缺高保真图", "场景里热点、人物可对话入口、目标、线索/档案/暂停入口的实际摆放", "1920x1080 探索界面完整稿；热点组件状态图", "可调查、已调查、关键线索、可对话、已对话、不可用", "Demo 白天调查主界面", "ui_explore_mockup.png", "优先做，和对话界面一起决定游戏第一印象"],
  ["P0", "证据判定界面", "只有概念模板", "真正游戏内的横向证据条、选择 1-3 条证据、提交判定、错误扣心", "1920x1080 判定界面；证据条状态切图", "默认、Hover、选中、可用于当前判定、错误、正确使用", "小推理节点和证据出示", "ui_judgement_mockup.png", "参考言弹，但视觉是旧戏票/旧纸条"],
  ["P0", "观皮界面", "缺高保真图", "中央画像、黑雾、三证据槽、观皮按钮、错误吞回、正确揭示", "1920x1080 观皮完整稿；正确/错误/完全揭示状态", "未选择、选中、正确匹配、错误匹配、局部揭示、完全揭示", "核心特色玩法", "ui_observe_skin_mockup_v2.png", "Demo 正确组合：旧戏票 + 脸谱盒 + 演出流程表"],
  ["P0", "对话调查界面", "已有 V2 示意图", "需要美术按 V2 深化成正式稿，补角色状态和按钮细节", "1920x1080 对话界面正式稿；话题菜单状态切图", "普通、已问过、新解锁、不可用、证据出示、警戒、破绽、封口", "弹丸式白天调查对话", "ui_dialogue_mockup_final.png", "已有 ui_dialogue_mockup_v2.png 可作参考"],
  ["P1", "扣心 / 失败界面", "缺图", "心 -1 动效、剩 1 心危险、0 心失败画面", "五心图标状态；失败画面完整稿", "5/5、4/5、3/5、2/5、1/5、0/5、裂开、熄灭", "推理错误反馈", "ui_heart_states.png", "心不要做卡通红心，可做脸谱碎片/血墨点"],
  ["P1", "人物档案详情页", "只有概念", "四人档案布局、嫌疑变化、隐藏身份遮罩、关系纸条", "人物档案完整稿；档案卡状态图", "未解锁、基础信息、可疑点、嫌疑上升、嫌疑下降、身份揭示", "记录沈青衣、阿喜、周岱、薛万山", "ui_archive_mockup.png", "像侦探笔记，不像数据面板"],
  ["P1", "线索详情页", "缺图", "点击证据条后展开的详情页，包括来源、关键词、推理用途", "线索详情完整稿；8 条证据示例", "新获得、已读、关键、未解锁、可观皮、已使用", "查看证据内容", "ui_clue_detail_mockup.png", "需要和证据条背包风格统一"],
  ["P1", "Toast / 目标更新", "缺图", "获得线索、新话题解锁、人物档案更新、当前目标更新", "Toast 组件状态图；目标更新样式", "出现、停留、消失、普通、重要、错误", "探索和对话反馈", "ui_toast_components.png", "旧纸条滑入，一行墨字浮现"],
  ["P2", "主菜单", "概念已有", "标题字效、菜单状态、完整背景插画方向", "主菜单完整稿；标题字效；菜单 4 态", "默认、Hover、选中、禁用", "游戏入口", "ui_main_menu_mockup.png", "可后置 polish"],
  ["P2", "暂停 / 设置", "概念已有", "暂停暗场、音量/文字速度/亮度 slider、开关 toggle、确认弹窗", "暂停设置完整稿；控件状态图", "默认、Hover、选中、禁用、确认/取消", "系统菜单", "ui_pause_settings_mockup.png", "两天 Demo 可简化"],
  ["P2", "章节标题卡", "缺图", "第一章、地点切换、Demo 结束字幕的视觉样式", "章节卡和地点卡组件", "出现、停留、消失", "转场和叙事节奏", "ui_chapter_title_cards.png", "旧纸/暗场字幕方向"],
  ["P3", "存档 / 读档", "未规划", "存档槽、时间、章节名、缩略图", "存档读档界面稿", "空槽、已有存档、选中、覆盖确认", "后续版本", "ui_save_load_mockup.png", "Demo 不必须"],
];

sheet.getRangeByIndexes(0, 0, rows.length, rows[0].length).values = rows;
sheet.freezePanes.freezeRows(1);

const used = sheet.getRangeByIndexes(0, 0, rows.length, rows[0].length);
used.format.font = { name: "Microsoft YaHei", size: 10, color: "#2A1712" };
used.format.wrapText = true;
sheet.getRange("A1:I1").format = {
  fill: "#1A1210",
  font: { bold: true, color: "#F1E5D0" },
  borders: { preset: "all", style: "thin", color: "#C9A96E" },
};
sheet.getRange(`A2:I${rows.length}`).format = {
  fill: "#F2E7D2",
  borders: { insideHorizontal: { style: "thin", color: "#D8C7A7" }, bottom: { style: "thin", color: "#D8C7A7" } },
};
sheet.getRange(`A2:A${rows.length}`).format.font = { bold: true, color: "#FFFFFF" };

const priorityColors = { P0: "#8B2020", P1: "#B56B2A", P2: "#7A6A45", P3: "#5A5A5A" };
for (let r = 2; r <= rows.length; r++) {
  const p = rows[r-1][0];
  sheet.getRange(`A${r}`).format.fill = priorityColors[p] || "#5A5A5A";
  if (p === "P0") sheet.getRange(`A${r}:I${r}`).format.fill = "#F4D7D2";
  if (p === "P1") sheet.getRange(`A${r}:I${r}`).format.fill = "#F4E4CE";
  if (p === "P2") sheet.getRange(`A${r}:I${r}`).format.fill = "#EFE8D8";
  if (p === "P3") sheet.getRange(`A${r}:I${r}`).format.fill = "#E8E4DC";
  sheet.getRange(`A${r}`).format.fill = priorityColors[p] || "#5A5A5A";
}

sheet.getRange("A:A").format.columnWidth = 8;
sheet.getRange("B:B").format.columnWidth = 24;
sheet.getRange("C:C").format.columnWidth = 18;
sheet.getRange("D:D").format.columnWidth = 42;
sheet.getRange("E:E").format.columnWidth = 36;
sheet.getRange("F:F").format.columnWidth = 34;
sheet.getRange("G:G").format.columnWidth = 24;
sheet.getRange("H:H").format.columnWidth = 30;
sheet.getRange("I:I").format.columnWidth = 36;
sheet.getRange(`A1:I${rows.length}`).format.rowHeight = 62;
sheet.getRange("A1:I1").format.rowHeight = 34;
sheet.tables.add(`A1:I${rows.length}`, true, "UIDemandTable");
sheet.getRange(`A2:A${rows.length}`).dataValidation = { rule: { type: "list", values: ["P0", "P1", "P2", "P3"] } };
sheet.getRange(`C2:C${rows.length}`).dataValidation = { rule: { type: "list", values: ["缺图", "缺高保真图", "只有概念", "概念已有", "已有 V2 示意图", "未规划"] } };

const summaryRows = [
  ["分类", "数量", "建议处理"],
  ["P0 核心必须", "=COUNTIF('UI缺口清单'!A2:A13,\"P0\")", "先做探索、证据判定、观皮，并深化对话 V2"],
  ["P1 Demo增强", "=COUNTIF('UI缺口清单'!A2:A13,\"P1\")", "补扣心失败、人物档案、线索详情、Toast"],
  ["P2 可后置", "=COUNTIF('UI缺口清单'!A2:A13,\"P2\")", "主菜单、暂停设置、章节卡可最后 polish"],
  ["P3 后续版本", "=COUNTIF('UI缺口清单'!A2:A13,\"P3\")", "存档读档不影响两天 Demo"],
];
summary.getRangeByIndexes(0, 0, summaryRows.length, 3).values = summaryRows;
summary.getRange("A1:C1").format = { fill: "#1A1210", font: { bold: true, color: "#F1E5D0" } };
summary.getRange("A2:C5").format = { fill: "#F2E7D2", borders: { preset: "all", style: "thin", color: "#D8C7A7" }, font: { color: "#2A1712" }, wrapText: true };
summary.getRange("A:A").format.columnWidth = 18;
summary.getRange("B:B").format.columnWidth = 10;
summary.getRange("C:C").format.columnWidth = 60;
summary.getRange("A1:C5").format.rowHeight = 42;
summary.freezePanes.freezeRows(1);

const note = [
  ["两天 Demo 推荐交付顺序"],
  ["1. 探索界面实际截图式 UI"],
  ["2. 证据判定界面"],
  ["3. 观皮界面"],
  ["4. 对话调查界面最终稿"],
  ["5. 扣心/失败、Toast、人物档案简版"],
];
summary.getRange("E1:E6").values = note;
summary.getRange("E1:E6").format = { fill: "#1A1210", font: { color: "#F1E5D0", bold: true }, wrapText: true, borders: { preset: "all", style: "thin", color: "#C9A96E" } };
summary.getRange("E:E").format.columnWidth = 42;

const guideRows = [
  ["模块", "操作说明", "交付要求", "验收标准"],
  ["先看哪几张图", "先打开 ui/mockups/all/00_all_ui_mockups_contact_sheet.png 看总览，再按 01 到 12 的顺序查看单张图。", "美术和程序都按编号沟通，不要只说“那个界面”。", "团队能明确每张图对应哪个 UI 模块。"],
  ["美术工作流", "先做 P0：探索、对话、证据判定、观皮。P0 完成后再做 P1：扣心失败、档案、线索详情、Toast。", "每个界面先交 1920x1080 完整稿，再交组件拆分图。", "P0 四张图达到可进游戏的视觉质量。"],
  ["组件拆分", "从完整稿中拆出可复用组件：话题按钮、证据条、心、热点框、Toast、普通按钮、暗场遮罩。", "每个组件至少默认 / Hover / 选中或禁用 3 态；关键组件需要错误 / 正确状态。", "程序能直接按状态切换图片或样式。"],
  ["切图命名", "命名用 ui_模块_组件_状态，例如 ui_dialogue_topic_new.png、ui_clue_strip_selected.png。", "不要使用“最终版1”“新新新”“未命名”等文件名。", "文件名能直接看出用途和状态。"],
  ["透明图规则", "按钮、纸条、墨迹、遮罩、心、热点框建议导出透明 PNG。全屏背景和完整稿导出普通 PNG。", "透明 PNG 保留边缘阴影；不要带多余黑底。", "放进 Unity 后不出现黑边、白边或底色块。"],
  ["Unity 实装", "程序按 UI 模块做 Panel：ExploreHUD、DialoguePanel、EvidencePanel、ObserveSkinPanel、ArchivePanel、ToastPanel。", "每个 Panel 对应 Excel 表里的 UI 模块，字段对应证据名、角色名、目标文本、心数。", "能用数据替换文字和状态，不靠截图写死。"],
  ["交互验证", "验证探索点击、对话话题、证据出示、判定扣心、观皮成功/失败、Toast 更新。", "每个交互至少有默认、反馈、结束三步状态。", "玩家不会不知道当前能点哪里、点错发生了什么。"],
  ["文字规范", "对话正文必须优先可读。标题、目标和按钮可以更风格化，但不要牺牲辨识度。", "正式稿里确认字号：标题、目标、对白、按钮、辅助说明。", "1920x1080 下截图缩小预览仍能读清主要文字。"],
  ["动效说明", "静态图完成后补动效：Toast 滑入、证据条选中、扣心裂开、观皮黑雾散开、章节标题淡入淡出。", "动效可以用文字说明或 3-5 帧序列图。", "程序能按说明还原节奏，不需要猜。"],
  ["最终验收", "每个 UI 模块必须有：完整稿、组件状态图、命名清晰的切图、必要动效说明。", "P0 必须先验收；P1 可以简化；P2/P3 可后置。", "两天 Demo 至少能跑通调查、对话、判定、观皮闭环。"],
];

guide.getRangeByIndexes(0, 0, guideRows.length, guideRows[0].length).values = guideRows;
guide.freezePanes.freezeRows(1);
guide.getRange(`A1:D${guideRows.length}`).format = {
  font: { name: "Microsoft YaHei", size: 11, color: "#2A1712" },
  wrapText: true,
  borders: { insideHorizontal: { style: "thin", color: "#D8C7A7" } },
};
guide.getRange("A1:D1").format = {
  fill: "#1A1210",
  font: { bold: true, color: "#F1E5D0" },
  borders: { preset: "all", style: "thin", color: "#C9A96E" },
};
guide.getRange(`A2:D${guideRows.length}`).format.fill = "#F2E7D2";
guide.getRange("A:A").format.columnWidth = 18;
guide.getRange("B:B").format.columnWidth = 48;
guide.getRange("C:C").format.columnWidth = 40;
guide.getRange("D:D").format.columnWidth = 36;
guide.getRange(`A1:D${guideRows.length}`).format.rowHeight = 58;
guide.getRange("A1:D1").format.rowHeight = 34;
guide.tables.add(`A1:D${guideRows.length}`, true, "UIGuideTable");

const errors = await workbook.inspect({ kind: "match", searchTerm: "#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A", options: { useRegex: true, maxResults: 50 }, summary: "formula error scan" });
console.log(errors.ndjson);
const preview = await workbook.render({ sheetName: "UI缺口清单", range: "A1:I13", scale: 1, format: "png" });
await fs.writeFile(`${outputDir}/ui_gap_list_preview.png`, new Uint8Array(await preview.arrayBuffer()));
const guidePreview = await workbook.render({ sheetName: "操作指南", range: "A1:D11", scale: 1, format: "png" });
await fs.writeFile(`${outputDir}/ui_operation_guide_preview.png`, new Uint8Array(await guidePreview.arrayBuffer()));

const xlsx = await SpreadsheetFile.exportXlsx(workbook);
await xlsx.save(`${outputDir}/画皮_UI缺口与美术交付清单.xlsx`);
