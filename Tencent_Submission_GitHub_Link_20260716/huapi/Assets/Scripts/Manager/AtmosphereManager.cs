using UnityEngine;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 场景恐惧/氛围管理器 - 控制恐怖氛围和紧张感
    /// </summary>
    public class AtmosphereManager : MonoBehaviour
    {
        public static AtmosphereManager Instance { get; private set; }

        [Header("氛围设置")]
        [SerializeField] [Range(0, 1)] private float tensionLevel;
        [SerializeField] private float maxTension = 1f;

        [Header("视觉效果")]
        [SerializeField] private UnityEngine.Rendering.Volume postProcessVolume;
        [SerializeField] private AnimationCurve vignetteIntensity;
        [SerializeField] private AnimationCurve chromaticAberration;
        [SerializeField] private AnimationCurve filmGrain;

        [Header("音效")]
        [SerializeField] private AudioSource ambientAudio;
        [SerializeField] private AudioClip normalAmbient;
        [SerializeField] private AudioClip tenseAmbient;
        [SerializeField] private AudioClip horrorAmbient;

        [Header("事件")]
        public System.Action<float> OnTensionChanged;

        public float TensionLevel => tensionLevel;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// 增加紧张度
        /// </summary>
        public void AddTension(float amount)
        {
            tensionLevel = Mathf.Clamp01(tensionLevel + amount);
            OnTensionChanged?.Invoke(tensionLevel);
            UpdateAtmosphere();
        }

        /// <summary>
        /// 设置紧张度
        /// </summary>
        public void SetTension(float value)
        {
            tensionLevel = Mathf.Clamp01(value);
            OnTensionChanged?.Invoke(tensionLevel);
            UpdateAtmosphere();
        }

        private void UpdateAtmosphere()
        {
            // 根据紧张度切换环境音
            if (ambientAudio != null)
            {
                if (tensionLevel >= 0.7f && ambientAudio.clip != horrorAmbient)
                {
                    ambientAudio.clip = horrorAmbient;
                    ambientAudio.Play();
                }
                else if (tensionLevel >= 0.3f && ambientAudio.clip != tenseAmbient)
                {
                    ambientAudio.clip = tenseAmbient;
                    ambientAudio.Play();
                }
                else if (tensionLevel < 0.3f && ambientAudio.clip != normalAmbient)
                {
                    ambientAudio.clip = normalAmbient;
                    ambientAudio.Play();
                }
            }

            // 后期特效
            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                // 根据紧张度调整 Vignette, Chromatic Aberration 等
                // 需要具体引用 Volume Profile 中的参数
            }
        }

        /// <summary>
        /// 触发惊吓事件
        /// </summary>
        public void TriggerScare(float tensionIncrease)
        {
            AddTension(tensionIncrease);
            // 可以在这里添加屏幕震动、闪白等效果
        }
    }
}
