import fs from "node:fs/promises";
import path from "node:path";
import { Presentation, PresentationFile } from "@oai/artifact-tool";

const ROOT = "C:/Users/lyuml/Desktop/hackathon_tencent";
const OUT = "C:/Users/lyuml/Desktop/hackathon_tencent/outputs/huapi_project_deck";
const PPTX = path.join(OUT, "画皮_Project_Deck.pptx");
const PNG_DIR = path.join(OUT, "rendered_slides");
const MONTAGE = path.join(OUT, "deck_montage.webp");

const W = 1280;
const H = 720;
const C = {
  black: "#090607",
  ink: "#130B0B",
  wine: "#4C0E13",
  red: "#A52824",
  gold: "#D6B36A",
  paleGold: "#F1DDA5",
  paper: "#E7D5AA",
  paperDark: "#B89559",
  white: "#F7EFE0",
  muted: "#BDAE95",
};

const asset = (...parts) => path.join(ROOT, ...parts);
const assets = {
  cover: asset("game", "huapi", "Assets", "Art", "Sprites", "Generated", "generated_cover_redesign.png"),
  logo: asset("ui_update", "01home", "01home_LOGO.png"),
  props: asset("game", "huapi", "Assets", "Art", "Sprites", "Backgrounds", "戏曲道具与盔头陈列室.png"),
  stage: asset("game", "huapi", "Assets", "Art", "Sprites", "Backgrounds", "戏曲舞台.png"),
  rehearsal: asset("game", "huapi", "Assets", "Art", "Sprites", "Backgrounds", "排练室.png"),
  backstage: asset("game", "huapi", "Assets", "Art", "Sprites", "Backgrounds", "舞台与幕后.png"),
  theater: asset("game", "huapi", "Assets", "Art", "Sprites", "Generated", "generated_theater_exterior.png"),
  sceneMap: asset("game", "huapi", "Assets", "Art", "Sprites", "Generated", "generated_overall_scene_map.png"),
  uiSheet: asset("ui", "mockups", "all", "00_all_ui_mockups_contact_sheet.png"),
  dan: asset("game", "huapi", "Assets", "Art", "Sprites", "Characters", "旦.png"),
  jing: asset("game", "huapi", "Assets", "Art", "Sprites", "Characters", "净.png"),
  chou: asset("game", "huapi", "Assets", "Art", "Sprites", "Characters", "丑.png"),
  sheng: asset("game", "huapi", "Assets", "Art", "Sprites", "Characters", "生.png"),
  boss: asset("game", "huapi", "Assets", "Art", "Sprites", "Characters", "老板.png"),
  muou: asset("game", "huapi", "Assets", "Art", "Sprites", "Characters", "木偶.png"),
};

function mime(file) {
  const ext = path.extname(file).toLowerCase();
  if (ext === ".jpg" || ext === ".jpeg") return "image/jpeg";
  if (ext === ".webp") return "image/webp";
  return "image/png";
}

async function imgBlob(file) {
  const bytes = await fs.readFile(file);
  return bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength);
}

async function writeBlob(file, blob) {
  await fs.writeFile(file, new Uint8Array(await blob.arrayBuffer()));
}

function slideBase(slide, eyebrow = "腾讯云黑客松作品介绍") {
  slide.background.fill = C.black;
  slide.shapes.add({
    geometry: "rect",
    position: { left: 0, top: 0, width: W, height: H },
    fill: C.black,
    line: { style: "solid", fill: "none", width: 0 },
  });
  slide.shapes.add({
    geometry: "rect",
    position: { left: 0, top: 0, width: W, height: 12 },
    fill: C.wine,
    line: { style: "solid", fill: "none", width: 0 },
  });
  const e = slide.shapes.add({
    geometry: "textbox",
    position: { left: 72, top: 44, width: 360, height: 24 },
    fill: "none",
    line: { style: "solid", fill: "none", width: 0 },
  });
  e.text = eyebrow;
  e.text.style = { fontSize: 16, bold: true, color: C.gold, fontFace: "Microsoft YaHei" };
}

function addTitle(slide, text, top = 78, width = 760, size = 42) {
  const t = slide.shapes.add({
    geometry: "textbox",
    position: { left: 72, top, width, height: 104 },
    fill: "none",
    line: { style: "solid", fill: "none", width: 0 },
  });
  t.text = text;
  t.text.style = { fontSize: size, bold: true, color: C.white, fontFace: "Microsoft YaHei" };
  return t;
}

function addBody(slide, text, left, top, width, height, size = 22, color = C.white) {
  const b = slide.shapes.add({
    geometry: "textbox",
    position: { left, top, width, height },
    fill: "none",
    line: { style: "solid", fill: "none", width: 0 },
  });
  b.text = text;
  b.text.style = { fontSize: size, color, fontFace: "Microsoft YaHei" };
  return b;
}

function addRule(slide, left, top, width = 116) {
  slide.shapes.add({
    geometry: "rect",
    position: { left, top, width, height: 4 },
    fill: C.gold,
    line: { style: "solid", fill: "none", width: 0 },
  });
}

function addPaper(slide, left, top, width, height, title, body, accent = C.red) {
  const card = slide.shapes.add({
    geometry: "roundRect",
    position: { left, top, width, height },
    fill: C.paper,
    line: { style: "solid", fill: C.paperDark, width: 2 },
    borderRadius: "rounded-lg",
  });
  const stripe = slide.shapes.add({
    geometry: "rect",
    position: { left, top, width: 8, height },
    fill: accent,
    line: { style: "solid", fill: "none", width: 0 },
  });
  const h = addBody(slide, title, left + 24, top + 18, width - 48, 36, 23, "#251914");
  h.text.style = { fontSize: 23, bold: true, color: "#251914", fontFace: "Microsoft YaHei" };
  addBody(slide, body, left + 24, top + 64, width - 48, height - 82, 17, "#32251D");
  return [card, stripe];
}

async function addImage(slide, file, left, top, width, height, fit = "cover", opacity = 1) {
  const image = slide.images.add({
    blob: await imgBlob(file),
    contentType: mime(file),
    alt: path.basename(file),
    fit,
    position: { left, top, width, height },
  });
  if (opacity !== 1) image.opacity = opacity;
  return image;
}

function overlay(slide, alpha = 0.55) {
  slide.shapes.add({
    geometry: "rect",
    position: { left: 0, top: 0, width: W, height: H },
    fill: `rgba(0,0,0,${alpha})`,
    line: { style: "solid", fill: "none", width: 0 },
  });
}

async function build() {
  await fs.mkdir(PNG_DIR, { recursive: true });
  const p = Presentation.create({ slideSize: { width: W, height: H } });

  {
    const s = p.slides.add();
    await addImage(s, assets.cover, 0, 0, W, H, "cover");
    overlay(s, 0.2);
    const bar = s.shapes.add({
      geometry: "rect",
      position: { left: 0, top: 0, width: 515, height: H },
      fill: "rgba(0,0,0,0.62)",
      line: { style: "solid", fill: "none", width: 0 },
    });
    void bar;
    const e = addBody(s, "腾讯云黑客松 Project Deck", 72, 66, 360, 34, 18, C.gold);
    e.text.style = { fontSize: 18, bold: true, color: C.gold, fontFace: "Microsoft YaHei" };
    addTitle(s, "画皮", 168, 360, 76);
    addBody(s, "现实是历史的重演", 78, 270, 390, 42, 28, C.paleGold);
    addRule(s, 78, 330, 160);
    addBody(s, "中国戏曲 × 民俗悬疑 × 叙事解谜\n白天收集线索，夜晚揭开画皮", 78, 368, 360, 110, 24, C.white);
  }

  {
    const s = p.slides.add();
    slideBase(s);
    await addImage(s, assets.props, 684, 0, 596, H, "cover");
    overlay(s, 0.35);
    addTitle(s, "一句话概念", 84, 560, 44);
    addRule(s, 76, 178);
    addBody(
      s,
      "在一个所有人都戴着脸谱生活的戏班中，玩家扮演年轻侦探，白天调查失踪旧案，夜晚通过“观皮”揭开角色面具下的真实身份。",
      76,
      224,
      566,
      154,
      27,
      C.white,
    );
    addPaper(s, 82, 424, 228, 140, "核心类型", "叙事解谜\n调查推理\n视觉小说", C.red);
    addPaper(s, 334, 424, 250, 140, "目标玩家", "民俗、悬疑、破案\n与轻恐怖题材受众", C.gold);
    addPaper(s, 608, 424, 250, 140, "体验承诺", "不靠战斗推进\n靠观察、对话与证据判断", C.red);
  }

  {
    const s = p.slides.add();
    slideBase(s, "故事背景");
    await addImage(s, assets.theater, 0, 0, W, H, "cover");
    overlay(s, 0.64);
    addTitle(s, "旧戏重演，把民国旧案搬回舞台", 74, 820, 42);
    addRule(s, 76, 168, 190);
    addPaper(s, 76, 230, 338, 240, "民国火灾旧案", "古镇戏班曾发生离奇大火，多人身亡，两人下落不明。案件被埋进戏班传闻，只留下残缺证据。", C.red);
    addPaper(s, 470, 230, 338, 240, "匿名委托", "玩家作为私人侦探收到邮件与高额赏金，伪装成戏班学徒进入内部调查。", C.gold);
    addPaper(s, 864, 230, 338, 240, "现实复刻历史", "多年后同一剧院、同一出戏再次排练。案发现场像旧案重演，但细节指向人为安排。", C.red);
    addBody(s, "Deck 的叙事重点：评委看到的不只是题材包装，而是一套能支撑玩法的悬疑结构。", 92, 586, 1040, 38, 22, C.paleGold);
  }

  {
    const s = p.slides.add();
    slideBase(s, "核心玩法");
    await addImage(s, assets.backstage, 0, 0, W, H, "cover");
    overlay(s, 0.67);
    addTitle(s, "昼夜双循环让线索与真相互相推进", 76, 780, 41);
    addRule(s, 78, 168, 170);
    const y = 248;
    addPaper(s, 88, y, 280, 250, "白天：调查", "探索场景\n点击人物与器物\n对话盘问\n收集证据", C.gold);
    addPaper(s, 500, y, 280, 250, "夜晚：观皮", "整理线索\n擦开黑雾\n补全角色档案\n逼近真实身份", C.red);
    addPaper(s, 912, y, 280, 250, "判定：推理", "组合证据\n排除伪线索\n证明旧案不是诅咒\n而是人为复刻", C.gold);
    addBody(s, "探索 → 证据 → 观皮 → 新方向 → 再探索", 298, 580, 720, 44, 30, C.white);
  }

  {
    const s = p.slides.add();
    slideBase(s, "独特卖点");
    await addImage(s, assets.stage, 0, 0, W, H, "cover");
    overlay(s, 0.66);
    addTitle(s, "卖点不是“恐怖”，而是看穿身份的过程", 74, 810, 40);
    addRule(s, 76, 166, 180);
    addPaper(s, 88, 222, 310, 230, "画皮系统", "每个角色有被黑雾遮挡的隐藏档案。相关证据越完整，真实面貌越清晰。", C.red);
    addPaper(s, 486, 222, 310, 230, "戏曲文化作为机制", "脸谱、行当、后台规矩和戏台空间都进入谜题，不只是视觉装饰。", C.gold);
    addPaper(s, 884, 222, 310, 230, "非战斗推理体验", "玩家胜利来自观察、对话、拖拽机关与证据判断，适合短 Demo 展示。", C.red);
    addBody(s, "一句展示语：识人知面不知心，揭面之前先揭证据。", 120, 570, 920, 42, 28, C.paleGold);
  }

  {
    const s = p.slides.add();
    slideBase(s, "角色与美术");
    addTitle(s, "角色以戏曲行当建立第一印象，再被“画皮”推翻", 76, 1000, 38);
    addRule(s, 78, 158, 190);
    const chars = [
      ["旦", assets.dan],
      ["净", assets.jing],
      ["丑", assets.chou],
      ["生", assets.sheng],
      ["班主", assets.boss],
      ["木偶", assets.muou],
    ];
    let x = 62;
    for (const [label, file] of chars) {
      s.shapes.add({
        geometry: "roundRect",
        position: { left: x, top: 205, width: 180, height: 366 },
        fill: "#120909",
        line: { style: "solid", fill: C.wine, width: 2 },
        borderRadius: "rounded-lg",
      });
      await addImage(s, file, x + 12, 222, 156, 300, "contain");
      const cap = addBody(s, label, x, 532, 180, 34, 22, C.paleGold);
      cap.text.style = { fontSize: 22, bold: true, color: C.paleGold, alignment: "center", fontFace: "Microsoft YaHei" };
      x += 203;
    }
    addBody(s, "暗红、黑、金与纸质证据构成统一视觉；脸谱提供身份标签，也制造叙事误导。", 88, 624, 960, 38, 23, C.white);
  }

  {
    const s = p.slides.add();
    slideBase(s, "场景与谜题");
    addTitle(s, "可探索空间围绕“案发现场 - 舞台 - 后台”展开", 76, 960, 38);
    addRule(s, 78, 158, 200);
    const items = [
      ["道具与盔头陈列室", assets.props, "案发现场调查、脸谱盒、药瓶与证据发现"],
      ["戏曲舞台", assets.stage, "锣鼓节奏、帷幕视觉与旧戏重演线索"],
      ["排练室", assets.rehearsal, "人物盘问、座位表、茶杯与时间线"],
    ];
    let x = 70;
    for (const [title, file, desc] of items) {
      await addImage(s, file, x, 215, 350, 198, "cover");
      s.shapes.add({
        geometry: "rect",
        position: { left: x, top: 215, width: 350, height: 198 },
        fill: "rgba(0,0,0,0.16)",
        line: { style: "solid", fill: C.gold, width: 2 },
      });
      const h = addBody(s, title, x, 440, 350, 32, 22, C.paleGold);
      h.text.style = { fontSize: 22, bold: true, color: C.paleGold, alignment: "center", fontFace: "Microsoft YaHei" };
      addBody(s, desc, x + 18, 486, 314, 78, 18, C.white);
      x += 430;
    }
    addBody(s, "谜题原型：门闩反扣、锣点复原、滑轨牵线、木偶箱封印。错误拖放扣心，正确拖放锁定部件。", 100, 620, 1060, 36, 21, C.paper);
  }

  {
    const s = p.slides.add();
    slideBase(s, "Demo 完成度");
    await addImage(s, assets.uiSheet, 620, 104, 584, 438, "contain");
    addTitle(s, "当前 Demo 已具备完整展示链路", 76, 560, 40);
    addRule(s, 78, 166, 170);
    const lines = [
      "两天流程：intro、白天探索、夜晚推进、观皮、第二天调查、推理判定、未完待续。",
      "UI 已接入：主菜单、教程、暂停、地图、线索背包、人物档案、阶段切换。",
      "交互已接入：点击音效、提示音、BGM 循环、方向键/WASD 切换场景。",
      "谜题已从文字发现升级为可拖拽机关，具备失败反馈和正确锁定。",
    ];
    let top = 224;
    for (const line of lines) {
      s.shapes.add({
        geometry: "ellipse",
        position: { left: 84, top: top + 8, width: 12, height: 12 },
        fill: C.gold,
        line: { style: "solid", fill: "none", width: 0 },
      });
      addBody(s, line, 112, top, 456, 62, 19, C.white);
      top += 80;
    }
    addBody(s, "展示状态：可讲清、可试玩、可继续扩展。", 78, 592, 560, 42, 25, C.paleGold);
  }

  {
    const s = p.slides.add();
    await addImage(s, assets.cover, 0, 0, W, H, "cover");
    overlay(s, 0.55);
    addBody(s, "腾讯云黑客松作品介绍", 78, 56, 360, 32, 18, C.gold);
    addTitle(s, "我们想交付的不是一段怪谈，\n而是一套能被玩家亲手拆开的旧案。", 78, 760, 44);
    addRule(s, 82, 258, 180);
    addPaper(s, 84, 318, 330, 172, "作品价值", "用中国戏曲视觉承载悬疑推理，把脸谱身份转化为调查与误导机制。", C.red);
    addPaper(s, 476, 318, 330, 172, "当前完成度", "Unity Demo 已形成核心链路，适合黑客松现场路演与试玩演示。", C.gold);
    addPaper(s, 868, 318, 330, 172, "下一步", "补齐美术一致性、扩展角色支线、完善观皮揭示与最终推理。", C.red);
    addBody(s, "白天收集线索，夜晚揭开画皮。", 82, 590, 820, 44, 33, C.paleGold);
  }

  for (const [index, slide] of p.slides.items.entries()) {
    const stem = `slide-${String(index + 1).padStart(2, "0")}`;
    await writeBlob(path.join(PNG_DIR, `${stem}.png`), await p.export({ slide, format: "png", scale: 1.5 }));
    await fs.writeFile(path.join(PNG_DIR, `${stem}.layout.json`), await (await slide.export({ format: "layout" })).text(), "utf8");
  }
  await writeBlob(MONTAGE, await p.export({ format: "webp", montage: true, scale: 0.7 }));
  const pptx = await PresentationFile.exportPptx(p);
  await pptx.save(PPTX);
  console.log(PPTX);
}

build().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
