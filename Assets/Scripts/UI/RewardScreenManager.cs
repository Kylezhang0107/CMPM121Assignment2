using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    // for spell assignment
    public static RewardScreenManager Instance;
    public TextMeshProUGUI spellNameText;
    public TextMeshProUGUI spellDescriptionText;
    public Image spellIconImage;
    
    [SerializeField] private UnityEngine.UI.Button acceptButton;
    [SerializeField] private UnityEngine.UI.Button declineButton;

    public GameObject rewardUI;
    public GameObject endUI;
    public TextMeshProUGUI waveLabel;

    private TextMeshProUGUI endLabel;
    private TextMeshProUGUI statsLabel;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Find the Wave label inside the reward panel
        if (waveLabel == null && rewardUI != null)
        {
            foreach (TextMeshProUGUI tmp in rewardUI.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.gameObject.name == "Wave")
                {
                    waveLabel = tmp;
                    break;
                }
            }
        }

        if (rewardUI != null)
        {
            TMP_FontAsset font = waveLabel != null ? waveLabel.font : null;

            // Stats label (enemies killed this wave)
            statsLabel = CreateTMPLabel("StatsLabel", rewardUI.transform,
                new Vector2(0f, -130f), new Vector2(280f, 50f), 24, font);

            // Spell icon (top-left area of panel)
            if (spellIconImage == null)
            {
                GameObject iconObj = new GameObject("SpellIcon");
                iconObj.transform.SetParent(rewardUI.transform, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.5f, 1f);
                irt.anchorMax = new Vector2(0.5f, 1f);
                irt.pivot = new Vector2(0.5f, 1f);
                irt.anchoredPosition = new Vector2(-80f, -200f);
                irt.sizeDelta = new Vector2(64f, 64f);
                spellIconImage = iconObj.AddComponent<Image>();
                spellIconImage.color = Color.white;
            }

            // Spell name label
            if (spellNameText == null)
                spellNameText = CreateTMPLabel("SpellNameLabel", rewardUI.transform,
                    new Vector2(30f, -210f), new Vector2(180f, 40f), 22, font);

            // Spell description label
            if (spellDescriptionText == null)
            {
                spellDescriptionText = CreateTMPLabel("SpellDescLabel", rewardUI.transform,
                    new Vector2(0f, -260f), new Vector2(260f, 80f), 18, font);
                spellDescriptionText.textWrappingMode = TextWrappingModes.Normal;
            }

            // Accept Spell button (if not assigned in Inspector)
            if (acceptButton == null)
            {
                acceptButton = CreateButton("AcceptButton", rewardUI.transform,
                    new Vector2(0f, -360f), new Vector2(160f, 40f), "Accept Spell", font);
            }
            acceptButton.onClick.AddListener(OnAcceptSpell);
        }

        // Decline button wires to the existing "Next Wave" button if present
        if (declineButton == null && rewardUI != null)
        {
            foreach (UnityEngine.UI.Button btn in rewardUI.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                if (btn.gameObject.name == "Next Wave" || btn.gameObject.name == "NextWave")
                {
                    declineButton = btn;
                    break;
                }
            }
        }
        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineSpell);

        // End screen win/lose label
        if (endUI != null)
        {
            TMP_FontAsset font = waveLabel != null ? waveLabel.font : null;
            GameObject labelObj = new GameObject("EndMessage");
            labelObj.transform.SetParent(endUI.transform, false);
            RectTransform rt = labelObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -60f);
            rt.sizeDelta = new Vector2(300f, 80f);
            endLabel = labelObj.AddComponent<TextMeshProUGUI>();
            endLabel.alignment = TextAlignmentOptions.Center;
            endLabel.fontSize = 36;
            endLabel.color = Color.black;
            endLabel.font = font;
        }
    }

    private TextMeshProUGUI CreateTMPLabel(string name, Transform parent,
        Vector2 anchoredPos, Vector2 size, float fontSize, TMP_FontAsset font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = Color.black;
        tmp.font = font;
        return tmp;
    }

    private UnityEngine.UI.Button CreateButton(string name, Transform parent,
        Vector2 anchoredPos, Vector2 size, string label, TMP_FontAsset font)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.7f, 0.5f, 0.3f);
        UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.font = font;

        return btn;
    }

    private GameManager.GameState lastKnownState = (GameManager.GameState)(-1);

    void Update()
    {
        GameManager.GameState state = GameManager.Instance.state;
        bool stateChanged = state != lastKnownState;
        lastKnownState = state;

        if (state == GameManager.GameState.GAMEOVER)
        {
            if (endLabel != null)
                endLabel.text = GameManager.Instance.playerWon ? "You Win!" : "You Lose!";
            if (endUI != null) endUI.SetActive(true);
            if (rewardUI != null) rewardUI.SetActive(false);
        }
        else if (state == GameManager.GameState.WAVEEND)
        {
            if (waveLabel != null)
                waveLabel.text = $"Wave {GameManager.Instance.currentWave} Completed";
            if (statsLabel != null)
                statsLabel.text = $"Enemies killed: {GameManager.Instance.waveEnemiesKilled}";
            if (rewardUI != null) rewardUI.SetActive(true);
            if (endUI != null) endUI.SetActive(false);

            // Generate and display the spell reward once on state entry
            if (stateChanged)
            {
                ShowReward();
            }
        }
        else
        {
            if (rewardUI != null) rewardUI.SetActive(false);
            if (endUI != null) endUI.SetActive(false);
        }
    }

    public void ShowReward()
    {
        if (GameManager.Instance.player == null)
        {
            Debug.LogError("ShowReward: Player not found!");
            return;
        }

        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("ShowReward: PlayerController not found!");
            return;
        }

        SpellCaster caster = playerController.spellcaster;
        if (caster == null)
        {
            Debug.LogError("ShowReward: SpellCaster not found!");
            return;
        }

        caster.pendingSpell =
            new SpellBuilder().BuildRandom(
                caster,
                caster.spellPower,
                Mathf.Max(1, GameManager.Instance.currentWave)
            );

        if (spellNameText != null && caster.pendingSpell != null)
        {
            spellNameText.text = caster.pendingSpell.GetName();
        }

        if (spellDescriptionText != null && caster.pendingSpell != null)
        {
            spellDescriptionText.text = caster.pendingSpell.GetDescription();

            if (caster.SpellCount >= SpellCaster.MaxEquippedSpells)
            {
                spellDescriptionText.text += "\n\nInventory full: click a slot drop button to replace.";
            }
        }

        if (spellIconImage != null && caster.pendingSpell != null)
        {
            GameManager.Instance.spellIconManager.PlaceSprite(caster.pendingSpell.GetIcon(), spellIconImage);
        }
    }

    public void OnAcceptSpell()
    {
        if (GameManager.Instance.player == null)
        {
            return;
        }

        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (playerController == null || playerController.spellcaster == null)
        {
            return;
        }

        SpellCaster caster = playerController.spellcaster;
        if (caster.pendingSpell != null)
        {
            if (caster.AcceptPendingSpell())
            {
                Debug.Log("Spell accepted: " + caster.ActiveSpell.GetName());
                rewardUI.SetActive(false);
                return;
            }

            // If no slots are free, the player must click one of the slot drop buttons.
            Debug.Log("Spell inventory full: choose a slot drop button to replace a spell.");
        }
    }

    public void OnDeclineSpell()
    {
        if (GameManager.Instance.player == null)
        {
            return;
        }

        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (playerController == null || playerController.spellcaster == null)
        {
            return;
        }

        playerController.spellcaster.pendingSpell = null;
        Debug.Log("Spell declined!");

        rewardUI.SetActive(false);
    }
}
