using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

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
    private Image spellNameCard;
    private Image spellDescriptionCard;
    private ScrollRect spellDescriptionScroll;

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
                new Vector2(0f, -120f), new Vector2(300f, 36f), 22, font);
            statsLabel.alignment = TextAlignmentOptions.Center;
            statsLabel.overflowMode = TextOverflowModes.Ellipsis;

            spellNameCard = EnsurePanelImage(
                "SpellNameCard",
                rewardUI.transform,
                new Vector2(0f, -165f),
                new Vector2(320f, 52f),
                new Color(1f, 1f, 1f, 0.16f)
            );

            spellDescriptionCard = EnsurePanelImage(
                "SpellDescriptionCard",
                rewardUI.transform,
                new Vector2(0f, -320f),
                new Vector2(320f, 132f),
                new Color(1f, 1f, 1f, 0.1f)
            );

            // Spell icon (top-left area of panel)
            if (spellIconImage == null)
            {
                GameObject iconObj = new GameObject("SpellIcon");
                iconObj.transform.SetParent(rewardUI.transform, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                SetRectTransform(irt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(64f, 64f));
                spellIconImage = iconObj.AddComponent<Image>();
                spellIconImage.color = Color.white;
            }
            else
            {
                SetRectTransform(spellIconImage.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(64f, 64f));
            }

            // Spell name label
            if (spellNameText == null)
                spellNameText = CreateTMPLabel("SpellNameLabel", rewardUI.transform,
                    new Vector2(0f, -165f), new Vector2(300f, 44f), 21, font);
            else
                SetRectTransform(spellNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -165f), new Vector2(300f, 44f));

            spellNameText.alignment = TextAlignmentOptions.Left;
            spellNameText.enableAutoSizing = true;
            spellNameText.fontSizeMin = 10;
            spellNameText.fontSizeMax = 22;
            spellNameText.overflowMode = TextOverflowModes.Overflow;
            spellNameText.textWrappingMode = TextWrappingModes.Normal;
            spellNameText.margin = new Vector4(10f, 2f, 8f, 0f);
            spellNameText.maxVisibleLines = 999;

            // Spell description label
            if (spellDescriptionText == null)
            {
                spellDescriptionText = CreateTMPLabel("SpellDescLabel", rewardUI.transform,
                    new Vector2(0f, -320f), new Vector2(300f, 112f), 17, font);
            }
            else
            {
                SetRectTransform(spellDescriptionText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -320f), new Vector2(300f, 112f));
            }

            spellDescriptionText.alignment = TextAlignmentOptions.TopLeft;
            spellDescriptionText.textWrappingMode = TextWrappingModes.Normal;
            spellDescriptionText.overflowMode = TextOverflowModes.Overflow;
            spellDescriptionText.maxVisibleLines = 999;
            spellDescriptionText.margin = new Vector4(10f, 8f, 8f, 0f);
            spellDescriptionText.enableAutoSizing = false;

            EnsureDescriptionScrollArea();

            // Keep card layers behind text/icon even if objects already existed in scene.
            if (spellNameCard != null && spellNameText != null)
            {
                int textIdx = spellNameText.transform.GetSiblingIndex();
                spellNameCard.transform.SetSiblingIndex(Mathf.Max(0, textIdx - 1));
            }

            if (spellDescriptionCard != null && spellDescriptionText != null)
            {
                int textIdx = spellDescriptionText.transform.GetSiblingIndex();
                spellDescriptionCard.transform.SetSiblingIndex(Mathf.Max(0, textIdx - 1));
            }

            // Accept Spell button (if not assigned in Inspector)
            if (acceptButton == null)
            {
                acceptButton = CreateButton("AcceptButton", rewardUI.transform,
                    new Vector2(0f, -435f), new Vector2(170f, 42f), "Accept Spell", font);
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

    private Image EnsurePanelImage(string name, Transform parent, Vector2 anchoredPos, Vector2 size, Color color)
    {
        Transform existing = parent.Find(name);
        Image panelImage;

        if (existing != null)
        {
            panelImage = existing.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = existing.gameObject.AddComponent<Image>();
            }
        }
        else
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);
            panelImage = panelObj.AddComponent<Image>();
        }

        panelImage.color = color;

        RectTransform rt = panelImage.GetComponent<RectTransform>();
        SetRectTransform(
            rt,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 0.5f),
            anchoredPos,
            size
        );

        return panelImage;
    }

    private void SetRectTransform(
        RectTransform rt,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size
    )
    {
        if (rt == null)
        {
            return;
        }

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = size;
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
            spellNameText.text = NormalizeUIText(caster.pendingSpell.GetName());
        }

        if (spellDescriptionText != null && caster.pendingSpell != null)
        {
            StringBuilder descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append(NormalizeUIText(caster.pendingSpell.GetDescription()));

            if (caster.SpellCount >= SpellCaster.MaxEquippedSpells)
            {
                descriptionBuilder.Append("\n\nInventory full: click one of your spell slots to replace it.");
            }

            spellDescriptionText.text = descriptionBuilder.ToString();

            if (spellDescriptionScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                spellDescriptionScroll.verticalNormalizedPosition = 1f;
            }
        }

        if (spellIconImage != null && caster.pendingSpell != null)
        {
            GameManager.Instance.spellIconManager.PlaceSprite(caster.pendingSpell.GetIcon(), spellIconImage);
        }

        if (acceptButton != null)
        {
            bool inventoryFull = caster.SpellCount >= SpellCaster.MaxEquippedSpells;
            acceptButton.gameObject.SetActive(!inventoryFull);
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
            if (caster.SpellCount >= SpellCaster.MaxEquippedSpells)
            {
                Debug.Log("Spell inventory full: pick a slot to replace instead of accepting directly.");
                return;
            }

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

    private void EnsureDescriptionScrollArea()
    {
        if (spellDescriptionText == null || rewardUI == null)
        {
            return;
        }

        Transform scrollRoot = rewardUI.transform.Find("SpellDescriptionScroll");
        GameObject scrollObj;
        if (scrollRoot == null)
        {
            scrollObj = new GameObject("SpellDescriptionScroll");
            scrollObj.transform.SetParent(rewardUI.transform, false);
        }
        else
        {
            scrollObj = scrollRoot.gameObject;
        }

        RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
        if (scrollRT == null)
        {
            scrollRT = scrollObj.AddComponent<RectTransform>();
        }
        SetRectTransform(
            scrollRT,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -320f),
            new Vector2(300f, 112f)
        );

        spellDescriptionScroll = scrollObj.GetComponent<ScrollRect>();
        if (spellDescriptionScroll == null)
        {
            spellDescriptionScroll = scrollObj.AddComponent<ScrollRect>();
        }
        spellDescriptionScroll.horizontal = false;
        spellDescriptionScroll.vertical = true;
        spellDescriptionScroll.movementType = ScrollRect.MovementType.Clamped;
        spellDescriptionScroll.scrollSensitivity = 40f;

        Transform viewport = scrollObj.transform.Find("Viewport");
        GameObject viewportObj;
        if (viewport == null)
        {
            viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
        }
        else
        {
            viewportObj = viewport.gameObject;
        }

        RectTransform viewportRT = viewportObj.GetComponent<RectTransform>();
        if (viewportRT == null)
        {
            viewportRT = viewportObj.AddComponent<RectTransform>();
        }
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.pivot = new Vector2(0.5f, 0.5f);
        viewportRT.anchoredPosition = Vector2.zero;
        viewportRT.sizeDelta = Vector2.zero;

        RectMask2D rectMask = viewportObj.GetComponent<RectMask2D>();
        if (rectMask == null)
        {
            viewportObj.AddComponent<RectMask2D>();
        }

        // Ensure text lives under viewport and drives content size.
        spellDescriptionText.transform.SetParent(viewportObj.transform, false);
        RectTransform textRT = spellDescriptionText.rectTransform;
        textRT.anchorMin = new Vector2(0f, 1f);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.pivot = new Vector2(0.5f, 1f);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = new Vector2(0f, 0f);

        ContentSizeFitter fitter = spellDescriptionText.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = spellDescriptionText.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        spellDescriptionScroll.viewport = viewportRT;
        spellDescriptionScroll.content = textRT;

        if (spellDescriptionCard != null)
        {
            int scrollIdx = scrollObj.transform.GetSiblingIndex();
            spellDescriptionCard.transform.SetSiblingIndex(Mathf.Max(0, scrollIdx - 1));
        }
    }

    private string NormalizeUIText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return input.Replace("\r", "").Trim();
    }
}
