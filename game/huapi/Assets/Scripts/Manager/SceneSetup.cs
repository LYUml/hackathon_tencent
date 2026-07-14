using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// 场景初始化器 - 《画皮》版
    /// 自动创建必要的 Manager 对象
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("自动创建")]
        [SerializeField] private bool autoSetup = true;

        [Header("预制体")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;

        private void Awake()
        {
            if (autoSetup) SetupScene();
        }

        private void SetupScene()
        {
            // GameManager
            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                    Instantiate(gameManagerPrefab);
                else
                {
                    GameObject gm = new GameObject("GameManager");
                    gm.AddComponent<GameManager>();
                }
            }

            // DayNightCycle
            if (DayNightCycle.Instance == null)
            {
                GameObject dn = new GameObject("DayNightCycle");
                dn.AddComponent<DayNightCycle>();
            }

            // DialogueManager
            if (DialogueManager.Instance == null)
            {
                GameObject dm = new GameObject("DialogueManager");
                dm.AddComponent<DialogueManager>();
            }

            // InvestigationManager
            if (InvestigationManager.Instance == null)
            {
                GameObject im = new GameObject("InvestigationManager");
                im.AddComponent<InvestigationManager>();
            }

            // AtmosphereManager
            if (AtmosphereManager.Instance == null)
            {
                GameObject am = new GameObject("AtmosphereManager");
                am.AddComponent<AtmosphereManager>();
            }

            // HuapiSystem
            HuapiSystem huapi = FindObjectOfType<HuapiSystem>();
            if (huapi == null)
            {
                Debug.LogWarning("场景中未找到 HuapiSystem！画皮功能将不可用");
            }

            // 玩家
            if (GameObject.FindGameObjectWithTag("Player") == null && playerPrefab != null)
            {
                Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            }

            // UI Canvas
            if (FindObjectOfType<Canvas>() == null && uiCanvasPrefab != null)
            {
                Instantiate(uiCanvasPrefab);
            }
        }
    }
}
