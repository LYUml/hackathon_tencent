using UnityEngine;
using UnityEngine.SceneManagement;

namespace TXGame
{
    /// <summary>
    /// 游戏全局管理器 - 《画皮》版
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            MainMenu,
            Playing,
            Dialogue,
            HuapiView,     // 画皮查看界面
            Investigation,  // 调查笔记界面
            Paused,
            GameOver
        }

        [Header("设置")]
        [SerializeField] private GameState currentState = GameState.Playing;
        [SerializeField] private GameObject pauseMenuPrefab;

        private GameState previousState;
        private GameObject pauseMenuInstance;

        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (HuapiFullUIInstaller.IsInstalled)
            {
                return;
            }

            // ESC 暂停/恢复
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (currentState == GameState.Playing)
                    PauseGame();
                else if (currentState == GameState.Paused)
                    ResumeGame();
                else if (currentState == GameState.HuapiView ||
                         currentState == GameState.Investigation)
                    ReturnToPlaying();
            }
        }

        /// <summary>
        /// 进入对话状态
        /// </summary>
        public void EnterDialogue()
        {
            previousState = currentState;
            currentState = GameState.Dialogue;
        }

        /// <summary>
        /// 退出对话状态
        /// </summary>
        public void ExitDialogue()
        {
            currentState = previousState;
        }

        /// <summary>
        /// 进入画皮查看状态
        /// </summary>
        public void EnterHuapiView()
        {
            previousState = currentState;
            currentState = GameState.HuapiView;
        }

        /// <summary>
        /// 进入调查笔记状态
        /// </summary>
        public void EnterInvestigation()
        {
            previousState = currentState;
            currentState = GameState.Investigation;
        }

        /// <summary>
        /// 返回游戏
        /// </summary>
        public void ReturnToPlaying()
        {
            currentState = GameState.Playing;
        }

        public void PauseGame()
        {
            if (currentState == GameState.Paused) return;
            previousState = currentState;
            currentState = GameState.Paused;
            Time.timeScale = 0f;

            if (pauseMenuPrefab != null)
                pauseMenuInstance = Instantiate(pauseMenuPrefab);
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            currentState = previousState;
            Time.timeScale = 1f;

            if (pauseMenuInstance != null)
                Destroy(pauseMenuInstance);
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
