using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 角色档案面板 - 显示角色信息和揭露进度
    /// </summary>
    public class CharacterProfileUI : MonoBehaviour
    {
        [Header("档案面板")]
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI faceTypeText;
        [SerializeField] private TextMeshProUGUI publicIdentityText;
        [SerializeField] private TextMeshProUGUI trueIdentityText;
        [SerializeField] private TextMeshProUGUI secretText;
        [SerializeField] private Slider revealProgressBar;
        [SerializeField] private Image faceMaskImage;

        [Header("角色列表")]
        [SerializeField] private Transform characterButtonContainer;
        [SerializeField] private GameObject characterListButtonPrefab;

        private HuapiSystem huapiSystem;
        private List<GameObject> characterButtons = new List<GameObject>();

        private void Start()
        {
            huapiSystem = FindObjectOfType<HuapiSystem>();
            profilePanel?.SetActive(false);
        }

        /// <summary>
        /// 显示角色列表
        /// </summary>
        public void ShowCharacterList()
        {
            profilePanel?.SetActive(true);
            RefreshCharacterList();
        }

        /// <summary>
        /// 刷新角色列表按钮
        /// </summary>
        private void RefreshCharacterList()
        {
            // 清除旧按钮
            foreach (var btn in characterButtons)
                Destroy(btn);
            characterButtons.Clear();

            if (huapiSystem == null || characterButtonContainer == null) return;

            string[] characterIDs = huapiSystem.GetAllCharacterIDs();
            foreach (string id in characterIDs)
            {
                CharacterData data = huapiSystem.GetCharacter(id);
                if (data == null) continue;

                GameObject btnObj = Instantiate(characterListButtonPrefab, characterButtonContainer);
                characterButtons.Add(btnObj);

                Button btn = btnObj.GetComponent<Button>();
                TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null)
                {
                    float progress = huapiSystem.GetRevealProgress(id);
                    label.text = $"{data.characterName} ({progress:P0})";
                }

                if (btn != null)
                {
                    string capturedID = id;
                    btn.onClick.AddListener(() => SelectCharacter(capturedID));
                }
            }
        }

        /// <summary>
        /// 选择一个角色查看
        /// </summary>
        private void SelectCharacter(string characterID)
        {
            CharacterData data = huapiSystem?.GetCharacter(characterID);
            if (data == null) return;

            // 更新档案信息
            float progress = huapiSystem.GetRevealProgress(characterID);

            if (characterNameText != null)
                characterNameText.text = data.characterName;

            if (faceTypeText != null)
                faceTypeText.text = data.GetFaceTypeDescription();

            if (publicIdentityText != null)
                publicIdentityText.text = $"公开身份: {data.publicIdentity}";

            if (trueIdentityText != null)
            {
                // 揭露超过一定阈值才显示真实身份
                trueIdentityText.text = progress >= 0.8f
                    ? $"真实身份: {data.trueIdentity}"
                    : "真实身份: ???";
            }

            if (secretText != null)
            {
                secretText.text = progress >= 0.5f
                    ? data.secret
                    : "???";
            }

            if (revealProgressBar != null)
                revealProgressBar.value = progress;

            if (faceMaskImage != null && data.faceMaskSprite != null)
                faceMaskImage.sprite = data.faceMaskSprite;

            // 打开画皮CG查看
            huapiSystem?.OpenHuapiView(characterID);
        }

        public void Hide()
        {
            profilePanel?.SetActive(false);
            huapiSystem?.CloseHuapiView();
        }
    }
}
