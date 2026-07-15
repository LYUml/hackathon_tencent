using UnityEngine;
using UnityEngine.Events;

namespace TXGame
{
    /// <summary>
    /// 昼夜双循环系统 - 游戏核心循环
    /// 
    /// 白天: 探索 → 对话 → 收集线索
    /// 夜晚: 整理线索 → 解锁档案 → 揭露真相
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        [Header("循环设置")]
        [SerializeField] private int startingDay = 1;
        [SerializeField] private float dayDurationSeconds = 600f;   // 白天时长(秒)，0=不自动切换
        [SerializeField] private float nightDurationSeconds = 300f;  // 夜晚时长(秒)，0=不自动切换

        [Header("视觉效果")]
        [SerializeField] private Color dayAmbientColor = new Color(0.8f, 0.75f, 0.6f);
        [SerializeField] private Color nightAmbientColor = new Color(0.1f, 0.05f, 0.15f);
        [SerializeField] private float transitionDuration = 2f;

        [Header("事件")]
        public UnityEvent<int> OnDayStart;        // 白天开始 (参数: 第几天)
        public UnityEvent<int> OnNightStart;      // 夜晚开始 (参数: 第几天)
        public UnityEvent<TimeOfDay> OnPhaseChange; // 阶段变化

        public enum TimeOfDay
        {
            Day,    // 白天 - 探索、对话、收集线索
            Night,  // 夜晚 - 观皮、整理线索、揭露真相
            Dawn    // 黎明 - 过渡阶段
        }

        public int CurrentDay { get; private set; }
        public TimeOfDay CurrentPhase { get; private set; }
        public float PhaseElapsedTime { get; private set; }

        private float phaseTimer;
        private Camera mainCamera;
        private Color targetAmbientColor;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            CurrentDay = startingDay;
            StartDay();
        }

        private void Update()
        {
            phaseTimer += Time.deltaTime;
            PhaseElapsedTime = phaseTimer;

            // 平滑过渡环境光
            if (mainCamera != null)
            {
                RenderSettings.ambientLight = Color.Lerp(
                    RenderSettings.ambientLight,
                    targetAmbientColor,
                    Time.deltaTime / transitionDuration
                );
            }

            // 自动切换（如果设置了时长）
            if (CurrentPhase == TimeOfDay.Day && dayDurationSeconds > 0 && phaseTimer >= dayDurationSeconds)
            {
                StartNight();
            }
            else if (CurrentPhase == TimeOfDay.Night && nightDurationSeconds > 0 && phaseTimer >= nightDurationSeconds)
            {
                AdvanceDay();
            }
        }

        /// <summary>
        /// 开始白天阶段
        /// </summary>
        public void StartDay()
        {
            CurrentPhase = TimeOfDay.Day;
            phaseTimer = 0f;
            targetAmbientColor = dayAmbientColor;

            OnDayStart?.Invoke(CurrentDay);
            OnPhaseChange?.Invoke(TimeOfDay.Day);
            Debug.Log($"=== 第 {CurrentDay} 天 - 白天 ===");
            Debug.Log("探索戏班，与角色对话，收集线索。");
        }

        /// <summary>
        /// 开始夜晚阶段
        /// </summary>
        public void StartNight()
        {
            CurrentPhase = TimeOfDay.Night;
            phaseTimer = 0f;
            targetAmbientColor = nightAmbientColor;

            OnNightStart?.Invoke(CurrentDay);
            OnPhaseChange?.Invoke(TimeOfDay.Night);
            Debug.Log($"=== 第 {CurrentDay} 天 - 夜晚 ===");
            Debug.Log("整理线索，观皮揭露，接近真相。");
        }

        /// <summary>
        /// 进入下一天
        /// </summary>
        public void AdvanceDay()
        {
            CurrentDay++;
            StartDay();
        }

        /// <summary>
        /// 强制切换到指定阶段
        /// </summary>
        public void ForcePhase(TimeOfDay phase)
        {
            switch (phase)
            {
                case TimeOfDay.Day:
                    StartDay();
                    break;
                case TimeOfDay.Night:
                    StartNight();
                    break;
            }
        }

        /// <summary>
        /// 玩家手动跳过当前阶段（通过休息/睡觉）
        /// </summary>
        public void SkipToNextPhase()
        {
            if (CurrentPhase == TimeOfDay.Day)
            {
                StartNight();
            }
            else
            {
                AdvanceDay();
            }
        }
    }
}
