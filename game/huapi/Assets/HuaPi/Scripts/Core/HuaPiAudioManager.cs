using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HuaPi.Core
{
    /// <summary>
    /// Lightweight runtime audio controller for the vertical-slice demo.
    /// It tolerates missing clips so AI-generated audio can be dropped in later.
    /// </summary>
    public sealed class HuaPiAudioManager : MonoBehaviour
    {
        public const string BgmMainTheme = "bgm_main_theme_first_skin";
        public const string BgmDayInvestigation = "bgm_day_investigation_backstage";
        public const string BgmDialogueTension = "bgm_dialogue_tension";
        public const string BgmJudgement = "bgm_judgement_evidence";
        public const string BgmObserveSkin = "bgm_observe_skin_ritual";
        public const string BgmReveal = "bgm_reveal_cliffhanger";

        public const string SfxClueAcquire = "sfx_clue_acquire";
        public const string SfxEvidenceSelect = "sfx_evidence_select";
        public const string SfxHeartLoss = "sfx_heart_loss";
        public const string SfxObserveSuccess = "sfx_observe_success";
        public const string SfxSceneTransition = "sfx_scene_transition";
        public const string SfxBossEntrance = "sfx_boss_entrance";

        private const float DefaultBgmVolume = 0.58f;
        private const float DefaultSfxVolume = 0.82f;
        private const float DefaultFadeSeconds = 1.0f;

        private static readonly Dictionary<string, string> BgmPaths = new Dictionary<string, string>
        {
            { BgmMainTheme, "Assets/HuaPi/Audio/BGM/bgm_main_theme_first_skin" },
            { BgmDayInvestigation, "Assets/HuaPi/Audio/BGM/bgm_day_investigation_backstage" },
            { BgmDialogueTension, "Assets/HuaPi/Audio/BGM/bgm_dialogue_tension" },
            { BgmJudgement, "Assets/HuaPi/Audio/BGM/bgm_judgement_evidence" },
            { BgmObserveSkin, "Assets/HuaPi/Audio/BGM/bgm_observe_skin_ritual" },
            { BgmReveal, "Assets/HuaPi/Audio/BGM/bgm_reveal_cliffhanger" }
        };

        private static readonly Dictionary<string, string> SfxPaths = new Dictionary<string, string>
        {
            { SfxClueAcquire, "Assets/HuaPi/Audio/SFX/sfx_clue_acquire" },
            { SfxEvidenceSelect, "Assets/HuaPi/Audio/SFX/sfx_evidence_select" },
            { SfxHeartLoss, "Assets/HuaPi/Audio/SFX/sfx_heart_loss" },
            { SfxObserveSuccess, "Assets/HuaPi/Audio/SFX/sfx_observe_success" },
            { SfxSceneTransition, "Assets/HuaPi/Audio/SFX/sfx_scene_transition" },
            { SfxBossEntrance, "Assets/HuaPi/Audio/SFX/sfx_boss_entrance" }
        };

        private static readonly string[] SupportedExtensions = { ".ogg", ".wav", ".mp3", ".aiff", ".aif" };

        private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();
        private readonly HashSet<string> _reportedMissingClips = new HashSet<string>();
        private AudioSource _bgmA;
        private AudioSource _bgmB;
        private AudioSource _activeBgm;
        private AudioSource _inactiveBgm;
        private AudioSource _sfxSource;
        private Coroutine _fadeRoutine;
        private string _currentBgmId;

        public static HuaPiAudioManager Instance { get; private set; }

        public static HuaPiAudioManager EnsureInstance()
        {
            if (Instance != null) return Instance;

            var go = new GameObject("HuaPi Audio Manager");
            DontDestroyOnLoad(go);
            return go.AddComponent<HuaPiAudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _bgmA = CreateSource(true);
            _bgmB = CreateSource(true);
            _sfxSource = CreateSource(false);
            _activeBgm = _bgmA;
            _inactiveBgm = _bgmB;
        }

        public void PlayBgm(string bgmId, float fadeSeconds = DefaultFadeSeconds)
        {
            if (string.IsNullOrWhiteSpace(bgmId) || _currentBgmId == bgmId) return;

            AudioClip clip = LoadClip(bgmId, BgmPaths);
            if (clip == null)
            {
                _currentBgmId = null;
                StopBgm(fadeSeconds);
                return;
            }

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CrossFadeBgm(bgmId, clip, Mathf.Max(0f, fadeSeconds)));
        }

        public void StopBgm(float fadeSeconds = DefaultFadeSeconds)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeOutBgm(Mathf.Max(0f, fadeSeconds)));
        }

        public void PlaySfx(string sfxId, float volumeScale = 1f)
        {
            if (string.IsNullOrWhiteSpace(sfxId)) return;

            AudioClip clip = LoadClip(sfxId, SfxPaths);
            if (clip == null) return;

            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(DefaultSfxVolume * volumeScale));
        }

        private AudioSource CreateSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = 0f;
            source.spatialBlend = 0f;
            return source;
        }

        private IEnumerator CrossFadeBgm(string bgmId, AudioClip nextClip, float seconds)
        {
            _inactiveBgm.clip = nextClip;
            _inactiveBgm.volume = 0f;
            _inactiveBgm.loop = true;
            _inactiveBgm.Play();

            float startVolume = _activeBgm.isPlaying ? _activeBgm.volume : 0f;
            if (seconds <= 0f)
            {
                _activeBgm.Stop();
                _activeBgm.volume = 0f;
                _inactiveBgm.volume = DefaultBgmVolume;
                SwapBgmSources();
                _currentBgmId = bgmId;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                _activeBgm.volume = Mathf.Lerp(startVolume, 0f, t);
                _inactiveBgm.volume = Mathf.Lerp(0f, DefaultBgmVolume, t);
                yield return null;
            }

            _activeBgm.Stop();
            _activeBgm.volume = 0f;
            _inactiveBgm.volume = DefaultBgmVolume;
            SwapBgmSources();
            _currentBgmId = bgmId;
        }

        private IEnumerator FadeOutBgm(float seconds)
        {
            float startVolume = _activeBgm.volume;
            if (seconds <= 0f)
            {
                _activeBgm.Stop();
                _activeBgm.volume = 0f;
                _currentBgmId = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                _activeBgm.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            _activeBgm.Stop();
            _activeBgm.volume = 0f;
            _currentBgmId = null;
        }

        private void SwapBgmSources()
        {
            AudioSource previous = _activeBgm;
            _activeBgm = _inactiveBgm;
            _inactiveBgm = previous;
        }

        private AudioClip LoadClip(string id, IReadOnlyDictionary<string, string> pathMap)
        {
            if (_clipCache.TryGetValue(id, out AudioClip cached)) return cached;
            if (!pathMap.TryGetValue(id, out string basePath)) return ReportMissing(id);

            AudioClip clip = null;
#if UNITY_EDITOR
            foreach (string extension in SupportedExtensions)
            {
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + extension);
                if (clip != null) break;
            }
#endif
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>(basePath);
            }

            if (clip == null) return ReportMissing(id);

            _clipCache[id] = clip;
            return clip;
        }

        private AudioClip ReportMissing(string id)
        {
            if (_reportedMissingClips.Add(id))
            {
                Debug.LogWarning($"[HuaPiAudio] Missing audio clip: {id}. Drop the generated file into Assets/HuaPi/Audio and use the planned filename.");
            }
            return null;
        }
    }
}
