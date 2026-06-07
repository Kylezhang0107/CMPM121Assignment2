using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class RewardScreenManager : MonoBehaviour
{
    private class RelicChoiceWidget
    {
        public GameObject root;
        public Image icon;
        public TextMeshProUGUI label;
        public UnityEngine.UI.Button takeButton;
    }

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
    private readonly List<RelicChoiceWidget> relicChoiceWidgets = new List<RelicChoiceWidget>();
    private readonly List<PlayerController.RelicData> pendingRelicChoices = new List<PlayerController.RelicData>();
    private readonly List<GameObject> externalWaveHudObjects = new List<GameObject>();
    private bool waveHudCached;
    private bool relicTakenThisReward;
    private bool spellAcceptedThisReward;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (rewardUI != null && waveLabel != null && !waveLabel.transform.IsChildOf(rewardUI.transform))
        {
            // Prevent accidentally binding the HUD wave label instead of reward panel label.
            waveLabel = null;
        }

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

            // Some scenes still use a legacy child named "Text" for the wave title.
            if (waveLabel == null)
            {
                foreach (TextMeshProUGUI tmp in rewardUI.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (tmp.gameObject.name == "Text")
                    {
                        waveLabel = tmp;
                        break;
                    }
                }
            }
        }

        if (waveLabel != null)
        {
            NormalizeWaveLabelLayout(waveLabel);
        }

        if (rewardUI != null)
        {
            TMP_FontAsset font = waveLabel != null ? waveLabel.font : null;

            NormalizeRewardPanelLayout();

            // Stats label (enemies killed this wave)
            statsLabel = CreateTMPLabel("StatsLabel", rewardUI.transform,
                new Vector2(0f, -90f), new Vector2(300f, 36f), 22, font);
            statsLabel.alignment = TextAlignmentOptions.Center;
            statsLabel.overflowMode = TextOverflowModes.Ellipsis;

            spellNameCard = EnsurePanelImage(
                "SpellNameCard",
                rewardUI.transform,
                new Vector2(0f, -130f),
                new Vector2(320f, 52f),
                new Color(1f, 1f, 1f, 0.16f)
            );

            spellDescriptionCard = EnsurePanelImage(
                "SpellDescriptionCard",
                rewardUI.transform,
                new Vector2(0f, -280f),
                new Vector2(320f, 132f),
                new Color(1f, 1f, 1f, 0.1f)
            );

            // Spell icon (top-left area of panel)
            if (spellIconImage == null)
            {
                GameObject iconObj = new GameObject("SpellIcon");
                iconObj.transform.SetParent(rewardUI.transform, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                SetRectTransform(irt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(64f, 64f));
                spellIconImage = iconObj.AddComponent<Image>();
                spellIconImage.color = Color.white;
            }

            // Spell name label
            if (spellNameText == null)
                spellNameText = CreateTMPLabel("SpellNameLabel", rewardUI.transform,
                    new Vector2(0f, -130f), new Vector2(300f, 44f), 21, font);

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
                    new Vector2(0f, -280f), new Vector2(300f, 112f), 17, font);
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
                    new Vector2(0f, -370f), new Vector2(170f, 42f), "Accept Spell", font);
            }
            acceptButton.onClick.AddListener(OnAcceptSpell);

            EnsureRelicChoiceWidgets(font);
            NormalizePrimaryButtonsLayout();
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

    private void NormalizeWaveLabelLayout(TextMeshProUGUI label)
    {
        if (label == null)
        {
            return;
        }

        RectTransform rt = label.rectTransform;
        if (rt == null)
        {
            return;
        }

        bool stretched = rt.anchorMin != rt.anchorMax;
        if (stretched)
        {
            // Legacy scene setup sometimes stretches this label vertically, causing overlap.
            SetRectTransform(
                rt,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -60f),
                new Vector2(360f, 52f)
            );
        }

        label.alignment = TextAlignmentOptions.Center;
        label.overflowMode = TextOverflowModes.Overflow;
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private GameManager.GameState lastKnownState = (GameManager.GameState)(-1);

    void Update()
    {
        GameManager.GameState state = GameManager.Instance.state;
        bool stateChanged = state != lastKnownState;
        lastKnownState = state;

        if (state == GameManager.GameState.GAMEOVER)
        {
            SetExternalWaveHudVisible(false);
            if (endLabel != null)
                endLabel.text = GameManager.Instance.playerWon ? "You Win!" : "You Lose!";
            if (endUI != null) endUI.SetActive(true);
            if (rewardUI != null) rewardUI.SetActive(false);
        }
        else if (state == GameManager.GameState.WAVEEND)
        {
            SetExternalWaveHudVisible(false);
            if (waveLabel != null)
                waveLabel.text = $"Wave {GameManager.Instance.currentWave} Completed";
            if (statsLabel != null)
                statsLabel.text = $"Enemies killed: {GameManager.Instance.waveEnemiesKilled}";
            if (rewardUI != null) rewardUI.SetActive(true);
            if (endUI != null) endUI.SetActive(false);

            // Generate and display the spell reward once on state entry
            if (stateChanged)
            {
                EventBus.Instance.WaveComplete(
                    GameManager.Instance.currentWave
                );

                ShowReward();
            }
        }
        else
        {
            SetExternalWaveHudVisible(true);
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

        spellAcceptedThisReward = false;
        relicTakenThisReward = false;
        SetAcceptButtonLabel("Accept Spell");
        if (acceptButton != null)
        {
            acceptButton.interactable = true;
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

        bool isRelicWave = GameManager.Instance.currentWave >= 3 && GameManager.Instance.currentWave % 3 == 0;
        if (isRelicWave)
        {
            ShowRelicChoices(playerController.GetRelicChoices(3));
        }
        else
        {
            HideRelicChoices();
        }
    }

    public void OnAcceptSpell()
    {
        AudioManager.Instance.PlayButtonClick();

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
        if (spellAcceptedThisReward)
        {
            return;
        }

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
                spellAcceptedThisReward = true;
                SetAcceptButtonLabel("Spell Accepted");
                if (acceptButton != null)
                {
                    acceptButton.interactable = false;
                }
                return;
            }

            // If no slots are free, the player must click one of the slot drop buttons.
            Debug.Log("Spell inventory full: choose a slot drop button to replace a spell.");
        }
    }

    public void OnDeclineSpell()
    {
        AudioManager.Instance.PlayButtonClick();

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

    private void SetAcceptButtonLabel(string label)
    {
        if (acceptButton == null)
        {
            return;
        }

        TextMeshProUGUI text = acceptButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.text = label;
        }
    }

    private void NormalizeRewardPanelLayout()
    {
        if (rewardUI == null)
        {
            return;
        }

        RectTransform rewardRT = rewardUI.GetComponent<RectTransform>();
        if (rewardRT == null)
        {
            return;
        }

        // Keep reward panel at a stable centered size regardless of aspect ratio.
        SetRectTransform(
            rewardRT,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(1080f, 630f)
        );
    }

    private void NormalizePrimaryButtonsLayout()
    {
        if (acceptButton != null)
        {
            RectTransform acceptRT = acceptButton.GetComponent<RectTransform>();
            SetRectTransform(
                acceptRT,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -370f),
                new Vector2(170f, 42f)
            );
        }

        if (declineButton != null)
        {
            RectTransform declineRT = declineButton.GetComponent<RectTransform>();
            SetRectTransform(
                declineRT,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -370f),
                new Vector2(170f, 42f)
            );
        }
    }

    private void CacheExternalWaveHudObjects()
    {
        if (waveHudCached)
        {
            return;
        }

        externalWaveHudObjects.Clear();
        WaveLabelController[] waveHudControllers = FindObjectsByType<WaveLabelController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (WaveLabelController controller in waveHudControllers)
        {
            if (controller == null)
            {
                continue;
            }

            Transform t = controller.transform;
            if (rewardUI != null && t.IsChildOf(rewardUI.transform))
            {
                continue;
            }

            externalWaveHudObjects.Add(controller.gameObject);
        }

        waveHudCached = true;
    }

    private void SetExternalWaveHudVisible(bool visible)
    {
        if (!waveHudCached)
        {
            CacheExternalWaveHudObjects();
        }

        foreach (GameObject hudObj in externalWaveHudObjects)
        {
            if (hudObj == null)
            {
                continue;
            }

            hudObj.SetActive(visible);
        }
    }

    private void EnsureRelicChoiceWidgets(TMP_FontAsset font)
    {
        if (rewardUI == null || relicChoiceWidgets.Count > 0)
        {
            return;
        }

        Transform existingRoot = rewardUI.transform.Find("RelicChoices");
        GameObject rootObj = existingRoot != null ? existingRoot.gameObject : new GameObject("RelicChoices");
        if (existingRoot == null)
        {
            rootObj.transform.SetParent(rewardUI.transform, false);
        }

        RectTransform rootRT = rootObj.GetComponent<RectTransform>();
        if (rootRT == null)
        {
            rootRT = rootObj.AddComponent<RectTransform>();
        }
        if (existingRoot == null)
        {
            SetRectTransform(rootRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -520f), new Vector2(760f, 185f));
        }

        float[] xOffsets = { -250f, 0f, 250f };
        for (int i = 0; i < 3; ++i)
        {
            RelicChoiceWidget widget = new RelicChoiceWidget();
            widget.root = new GameObject($"RelicChoice{i}");
            widget.root.transform.SetParent(rootObj.transform, false);

            RectTransform itemRT = widget.root.AddComponent<RectTransform>();
            SetRectTransform(itemRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xOffsets[i], 0f), new Vector2(220f, 170f));

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(widget.root.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            SetRectTransform(iconRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(44f, 44f));
            widget.icon = iconObj.AddComponent<Image>();
            widget.icon.color = Color.white;

            widget.label = CreateTMPLabel($"RelicLabel{i}", widget.root.transform, new Vector2(0f, -78f), new Vector2(210f, 72f), 16f, font);
            widget.label.alignment = TextAlignmentOptions.Center;
            widget.label.textWrappingMode = TextWrappingModes.Normal;

            widget.takeButton = CreateButton($"TakeRelic{i}", widget.root.transform, new Vector2(0f, -130f), new Vector2(120f, 38f), "Take", font);
            int optionIndex = i;
            widget.takeButton.onClick.AddListener(() => OnTakeRelic(optionIndex));

            relicChoiceWidgets.Add(widget);
        }

        rootObj.SetActive(false);
    }

    private void ShowRelicChoices(List<PlayerController.RelicData> relicChoices)
    {
        pendingRelicChoices.Clear();
        if (relicChoices != null)
        {
            pendingRelicChoices.AddRange(relicChoices);
        }

        for (int i = 0; i < relicChoiceWidgets.Count; ++i)
        {
            RelicChoiceWidget widget = relicChoiceWidgets[i];
            bool hasRelic = i < pendingRelicChoices.Count;
            widget.root.SetActive(hasRelic);
            if (!hasRelic)
            {
                continue;
            }

            PlayerController.RelicData relic = pendingRelicChoices[i];
            widget.label.text = NormalizeUIText(relic.GetDescription());
            widget.takeButton.interactable = true;

            TextMeshProUGUI btnText = widget.takeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (btnText != null)
            {
                btnText.text = "Take";
            }

            if (widget.icon != null && GameManager.Instance.relicIconManager != null)
            {
                GameManager.Instance.relicIconManager.PlaceSprite(relic.sprite, widget.icon);
            }
        }

        if (relicChoiceWidgets.Count > 0 && relicChoiceWidgets[0].root != null)
        {
            relicChoiceWidgets[0].root.transform.parent.gameObject.SetActive(pendingRelicChoices.Count > 0);
        }
    }

    private void HideRelicChoices()
    {
        pendingRelicChoices.Clear();
        if (relicChoiceWidgets.Count > 0 && relicChoiceWidgets[0].root != null)
        {
            relicChoiceWidgets[0].root.transform.parent.gameObject.SetActive(false);
        }
    }

    private void OnTakeRelic(int optionIndex)
    {
        AudioManager.Instance.PlayButtonClick();

        if (relicTakenThisReward || optionIndex < 0 || optionIndex >= pendingRelicChoices.Count)
        {
            return;
        }

        if (GameManager.Instance.player == null)
        {
            return;
        }

        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            return;
        }

        PlayerController.RelicData pickedRelic = pendingRelicChoices[optionIndex];
        playerController.GrantRelic(pickedRelic);
        relicTakenThisReward = true;

        for (int i = 0; i < relicChoiceWidgets.Count; ++i)
        {
            RelicChoiceWidget widget = relicChoiceWidgets[i];
            if (!widget.root.activeSelf)
            {
                continue;
            }

            widget.takeButton.interactable = false;
            TextMeshProUGUI buttonText = widget.takeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (buttonText != null)
            {
                buttonText.text = i == optionIndex ? "Taken" : "Locked";
            }
        }
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
            new Vector2(0f, -280f),
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
