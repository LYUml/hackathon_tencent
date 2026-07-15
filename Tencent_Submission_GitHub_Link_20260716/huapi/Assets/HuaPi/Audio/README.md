# HuaPi Audio Drop-In Guide

Place generated audio files in these folders using the exact filenames below.

## BGM

Folder: `Assets/HuaPi/Audio/BGM`

- `bgm_main_theme_first_skin.ogg`
- `bgm_day_investigation_backstage.ogg`
- `bgm_dialogue_tension.ogg`
- `bgm_judgement_evidence.ogg`
- `bgm_observe_skin_ritual.ogg`
- `bgm_reveal_cliffhanger.ogg`

Recommended export: OGG, 44.1kHz or 48kHz, clean loop points.

## SFX

Folder: `Assets/HuaPi/Audio/SFX`

- `sfx_clue_acquire.wav`
- `sfx_evidence_select.wav`
- `sfx_heart_loss.wav`
- `sfx_observe_success.wav`
- `sfx_scene_transition.wav`
- `sfx_boss_entrance.wav`

Recommended export: WAV, 44.1kHz or 48kHz. Keep UI SFX short and quiet.

The runtime audio manager also accepts `.wav`, `.mp3`, `.aiff`, and `.aif` for BGM if the base filename matches.
