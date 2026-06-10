using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    [Serializable]
    private class SkillNodeConfig
    {
        public string title;
        public string description;
        public Vector2 position;
    }

    private class RuntimeSkillNode
    {
        public int index;
        public Image background;
        public Button button;
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI descriptionLabel;
    }

    [Serializable]
    private class SkillTreeDefaults
    {
        public string buttonSkinName = "ProjectUtumno_full_1153";
        public string buttonSkinAltName = "Project Utumno_full_1153";
        public string panelSkinName = "UIPanel";
        public string closeButtonSkinName = "button";

        public Vector2 referenceResolution = new Vector2(1920f, 1080f);
        public float widthHeightMatch = 0.5f;
        public int canvasSortingOrder = 220;

        public Vector2 openButtonAnchoredPosition = new Vector2(-95f, 82f);
        public Vector2 openButtonSize = new Vector2(88f, 88f);
        public string openButtonText = "Skill Tree";
        public float openButtonFontSize = 24f;
        public Color openButtonTextColor = Color.white;

        public Vector2 panelSize = new Vector2(980f, 620f);
        public Color overlayColor = new Color(0f, 0f, 0f, 0.5f);

        public string titleText = "Skill Tree";
        public Vector2 titlePosition = new Vector2(0f, 270f);
        public Vector2 titleSize = new Vector2(780f, 56f);
        public float titleFontSize = 40f;
        public Color titleColor = new Color(0.15f, 0.11f, 0.07f, 1f);

        public string subtitleText = "Stats only: choose upgrades that strengthen your build";
        public Vector2 subtitlePosition = new Vector2(0f, 220f);
        public Vector2 subtitleSize = new Vector2(840f, 42f);
        public float subtitleFontSize = 24f;
        public Color subtitleColor = new Color(0.25f, 0.2f, 0.14f, 1f);

        public Vector2 nodeSize = new Vector2(220f, 170f);
        public Color nodeBackgroundColor = new Color(0.22f, 0.27f, 0.36f, 1f);
        public Vector2 nodeTitlePosition = new Vector2(0f, 44f);
        public Vector2 nodeTitleSize = new Vector2(190f, 40f);
        public float nodeTitleFontSize = 28f;
        public Color nodeTitleColor = new Color(0.96f, 0.97f, 1f, 1f);
        public Vector2 nodeDescriptionPosition = new Vector2(0f, -20f);
        public Vector2 nodeDescriptionSize = new Vector2(190f, 80f);
        public float nodeDescriptionFontSize = 20f;
        public Color nodeDescriptionColor = new Color(0.9f, 0.93f, 1f, 1f);

        public Color connectorColor = new Color(0.6f, 0.7f, 0.88f, 0.8f);
        public float connectorHeight = 6f;

        public string closeButtonText = "Close";
        public Vector2 closeButtonPosition = new Vector2(0f, -250f);
        public Vector2 closeButtonSize = new Vector2(200f, 56f);
        public float closeButtonFontSize = 28f;
        public Color closeButtonTextColor = Color.white;
        public Color closeButtonFallbackColor = new Color(0.78f, 0.24f, 0.2f, 1f);

        public Color fallbackButtonColor = new Color(0.86f, 0.23f, 0.18f, 0.95f);
        public Color fallbackPanelColor = new Color(0.12f, 0.14f, 0.2f, 0.98f);
    }

    private static SkillTreeUI instance;

    [Header("Skins (optional, overrides auto-find)")]
    [SerializeField] private Sprite buttonSkin;
    [SerializeField] private Sprite panelSkin;
    [SerializeField] private Sprite closeButtonSkin;

    [Header("Config")]
    [SerializeField] private SkillTreeDefaults defaults = new SkillTreeDefaults();

    [Header("Nodes")]
    [SerializeField] private SkillNodeConfig[] nodes =
    {
        new SkillNodeConfig { title = "Power", description = "+10% spell damage", position = new Vector2(-250f, 70f) },
        new SkillNodeConfig { title = "Flow", description = "+20 max mana", position = new Vector2(0f, 70f) },
        new SkillNodeConfig { title = "Stride", description = "+10% move speed", position = new Vector2(250f, 70f) }
    };

    private Canvas skillTreeCanvas;
    private Button openButton;
    private Image openButtonImage;
    private Outline openButtonGlow;
    private TextMeshProUGUI openButtonLabel;
    private GameObject panelOverlay;
    private bool uiBuilt;
    private float openButtonBlinkTimer;
    private Color openButtonBaseColor = Color.white;
    private readonly List<RuntimeSkillNode> runtimeNodes = new List<RuntimeSkillNode>();
    private TextMeshProUGUI skillPointsText;
    private TextMeshProUGUI pathText;
    private TextMeshProUGUI hintText;

    private readonly Color arcaneColor = new Color(0.46f, 0.36f, 0.74f, 1f);
    private readonly Color iceColor = new Color(0.2f, 0.5f, 0.86f, 1f);
    private readonly Color fireColor = new Color(0.82f, 0.28f, 0.2f, 1f);
    private readonly Color disabledNodeColor = new Color(0.22f, 0.23f, 0.28f, 1f);
    private readonly Color dimWhite = new Color(0.48f, 0.48f, 0.48f, 1f);

    private Sprite cachedButtonSkin;
    private Sprite cachedPanelSkin;
    private Sprite cachedCloseButtonSkin;

    private bool loggedMissingButtonSkin;
    private bool loggedMissingPanelSkin;
    private bool loggedMissingCloseButtonSkin;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            instance.AssignDefaultSkinsIfMissing();
            return;
        }

        SkillTreeUI preferred = FindPreferredInstance();
        if (preferred != null)
        {
            instance = preferred;
            instance.AssignDefaultSkinsIfMissing();
            DontDestroyOnLoad(preferred.gameObject);
            return;
        }

        GameObject runtimeObj = new GameObject("SkillTreeUI");
        instance = runtimeObj.AddComponent<SkillTreeUI>();
        instance.AssignDefaultSkinsIfMissing();
        DontDestroyOnLoad(runtimeObj);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        AssignDefaultSkinsIfMissing();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SkillTreeManager.Instance.OnSkillsChanged += RefreshSkillNodes;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SkillTreeManager.Instance.OnSkillsChanged -= RefreshSkillNodes;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiBuilt = false;
        runtimeNodes.Clear();

        if (skillTreeCanvas != null)
        {
            Destroy(skillTreeCanvas.gameObject);
            skillTreeCanvas = null;
        }
    }

    private void Update()
    {
        if (!uiBuilt)
        {
            BuildUIIfNeeded();
            return;
        }

        bool shouldShowButton = ShouldShowOpenButton();
        if (openButton != null)
        {
            openButton.gameObject.SetActive(shouldShowButton);
        }

        UpdateOpenButtonBlink(shouldShowButton);

        if (!shouldShowButton && panelOverlay != null && panelOverlay.activeSelf)
        {
            panelOverlay.SetActive(false);
        }

        if (panelOverlay != null && panelOverlay.activeSelf)
        {
            RefreshSkillNodes();
        }
    }

    private void BuildUIIfNeeded()
    {
        if (uiBuilt)
        {
            return;
        }

        skillTreeCanvas = CreateCanvas();
        if (skillTreeCanvas == null)
        {
            return;
        }

        BuildOpenButton(skillTreeCanvas.transform);
        BuildPanel(skillTreeCanvas.transform);

        uiBuilt = true;
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("SkillTreeCanvas");
        DontDestroyOnLoad(canvasObj);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = defaults.canvasSortingOrder;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = defaults.referenceResolution;
        scaler.matchWidthOrHeight = defaults.widthHeightMatch;

        canvasObj.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void BuildOpenButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("OpenSkillTreeButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        SetAnchoredRect(rt, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), defaults.openButtonAnchoredPosition, defaults.openButtonSize);

        Image image = buttonObj.AddComponent<Image>();
        ApplySkin(image, defaults.buttonSkinName, ref cachedButtonSkin, ref loggedMissingButtonSkin, defaults.fallbackButtonColor, Image.Type.Sliced);
        openButtonImage = image;
        openButtonBaseColor = image.color;

        openButtonGlow = buttonObj.AddComponent<Outline>();
        openButtonGlow.effectColor = new Color(1f, 1f, 1f, 0f);
        openButtonGlow.effectDistance = new Vector2(2f, 2f);
        openButtonGlow.useGraphicAlpha = false;

        openButton = buttonObj.AddComponent<Button>();

        TextMeshProUGUI label = CreateText(
            "Label",
            buttonObj.transform,
            Vector2.zero,
            defaults.openButtonSize,
            defaults.openButtonFontSize,
            TextAlignmentOptions.Center,
            defaults.openButtonText,
            FontStyles.Bold,
            defaults.openButtonTextColor
        );
        openButtonLabel = label;
        StretchToParent(label.rectTransform, new Vector2(6f, 6f), new Vector2(-6f, -6f));

        openButton.onClick.AddListener(OpenPanel);
        openButton.gameObject.SetActive(false);
    }

    private void UpdateOpenButtonBlink(bool shouldShowButton)
    {
        if (openButtonImage == null)
        {
            return;
        }

        bool shouldBlink = shouldShowButton && SkillTreeManager.Instance.skillPoints > 0;
        if (!shouldBlink)
        {
            openButtonBlinkTimer = 0f;
            openButtonImage.color = openButtonBaseColor;
            openButtonImage.transform.localScale = Vector3.one;
            if (openButtonGlow != null)
            {
                openButtonGlow.effectColor = new Color(1f, 1f, 1f, 0f);
                openButtonGlow.effectDistance = new Vector2(2f, 2f);
            }
            if (openButtonLabel != null)
            {
                openButtonLabel.color = defaults.openButtonTextColor;
            }
            return;
        }

        openButtonBlinkTimer += Time.unscaledDeltaTime * 7f;
        float pulse = (Mathf.Sin(openButtonBlinkTimer) + 1f) * 0.5f;

        Color from = IsNearlyWhite(openButtonBaseColor) ? dimWhite : openButtonBaseColor;
        Color to = new Color(1f, 1f, 1f, from.a);
        openButtonImage.color = Color.Lerp(from, to, pulse);
        float scalePulse = Mathf.Lerp(1f, 1.12f, pulse);
        openButtonImage.transform.localScale = new Vector3(scalePulse, scalePulse, 1f);

        if (openButtonGlow != null)
        {
            float glowAlpha = Mathf.Lerp(0.15f, 0.85f, pulse);
            float glowSpread = Mathf.Lerp(2f, 6f, pulse);
            openButtonGlow.effectColor = new Color(1f, 1f, 1f, glowAlpha);
            openButtonGlow.effectDistance = new Vector2(glowSpread, glowSpread);
        }

        if (openButtonLabel != null)
        {
            Color labelFrom = IsNearlyWhite(defaults.openButtonTextColor) ? dimWhite : defaults.openButtonTextColor;
            Color labelTo = Color.white;
            openButtonLabel.color = Color.Lerp(labelFrom, labelTo, pulse);
        }
    }

    private bool IsNearlyWhite(Color color)
    {
        const float threshold = 0.03f;
        return Mathf.Abs(color.r - 1f) < threshold &&
               Mathf.Abs(color.g - 1f) < threshold &&
               Mathf.Abs(color.b - 1f) < threshold;
    }

    private void BuildPanel(Transform parent)
    {
        panelOverlay = new GameObject("SkillTreeOverlay");
        panelOverlay.transform.SetParent(parent, false);

        RectTransform overlayRT = panelOverlay.AddComponent<RectTransform>();
        StretchToParent(overlayRT, Vector2.zero, Vector2.zero);

        Image overlayImage = panelOverlay.AddComponent<Image>();
        overlayImage.color = defaults.overlayColor;

        Button overlayButton = panelOverlay.AddComponent<Button>();
        overlayButton.onClick.AddListener(ClosePanel);

        GameObject cardObj = new GameObject("SkillTreeCard");
        cardObj.transform.SetParent(panelOverlay.transform, false);

        RectTransform cardRT = cardObj.AddComponent<RectTransform>();
        SetAnchoredRect(cardRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, defaults.panelSize);

        Image cardImage = cardObj.AddComponent<Image>();
        ApplySkin(cardImage, defaults.panelSkinName, ref cachedPanelSkin, ref loggedMissingPanelSkin, defaults.fallbackPanelColor, Image.Type.Sliced);

        CreateText(
            "SkillTreeTitle",
            cardObj.transform,
            defaults.titlePosition,
            defaults.titleSize,
            defaults.titleFontSize,
            TextAlignmentOptions.Center,
            defaults.titleText,
            FontStyles.Bold,
            defaults.titleColor
        );

        CreateText(
            "SkillTreeSubtitle",
            cardObj.transform,
            defaults.subtitlePosition,
            defaults.subtitleSize,
            defaults.subtitleFontSize,
            TextAlignmentOptions.Center,
            defaults.subtitleText,
            FontStyles.Bold,
            defaults.subtitleColor
        );

        skillPointsText = CreateText(
            "SkillPointsText",
            cardObj.transform,
            new Vector2(0f, 178f),
            new Vector2(700f, 36f),
            24f,
            TextAlignmentOptions.Center,
            string.Empty,
            FontStyles.Bold,
            new Color(0.95f, 0.91f, 0.74f, 1f)
        );

        pathText = CreateText(
            "PathText",
            cardObj.transform,
            new Vector2(0f, 146f),
            new Vector2(700f, 32f),
            22f,
            TextAlignmentOptions.Center,
            string.Empty,
            FontStyles.Bold,
            new Color(0.9f, 0.93f, 1f, 1f)
        );

        hintText = CreateText(
            "HintText",
            cardObj.transform,
            new Vector2(0f, -126f),
            new Vector2(860f, 40f),
            19f,
            TextAlignmentOptions.Center,
            string.Empty,
            FontStyles.Normal,
            new Color(0.85f, 0.88f, 0.96f, 1f)
        );

        BuildNodesAndConnectors(cardObj.transform);

        Button closeButton = CreatePanelButton(cardObj.transform);
        closeButton.onClick.AddListener(ClosePanel);

        RefreshSkillNodes();
        panelOverlay.SetActive(false);
    }

    private void BuildNodesAndConnectors(Transform parent)
    {
        if (nodes == null || nodes.Length == 0)
        {
            return;
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            SkillNodeConfig node = nodes[i];
            if (node == null)
            {
                continue;
            }

            CreateNode(parent, node, i);
        }

        for (int i = 0; i < nodes.Length - 1; i++)
        {
            SkillNodeConfig left = nodes[i];
            SkillNodeConfig right = nodes[i + 1];
            if (left == null || right == null)
            {
                continue;
            }

            CreateConnector(parent, left.position, right.position);
        }
    }

    private void CreateNode(Transform parent, SkillNodeConfig node, int index)
    {
        GameObject nodeObj = new GameObject(node.title + "Node");
        nodeObj.transform.SetParent(parent, false);

        RectTransform rt = nodeObj.AddComponent<RectTransform>();
        SetAnchoredRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), node.position, defaults.nodeSize);

        Image image = nodeObj.AddComponent<Image>();
        image.color = defaults.nodeBackgroundColor;

        Button button = nodeObj.AddComponent<Button>();
        int capturedIndex = index;
        button.onClick.AddListener(() => HandleNodeSelected(capturedIndex));

        TextMeshProUGUI title = CreateText(
            node.title + "Title",
            nodeObj.transform,
            defaults.nodeTitlePosition,
            defaults.nodeTitleSize,
            defaults.nodeTitleFontSize,
            TextAlignmentOptions.Center,
            node.title,
            FontStyles.Bold,
            defaults.nodeTitleColor
        );
        title.enableAutoSizing = true;
        title.fontSizeMin = 16f;
        title.fontSizeMax = defaults.nodeTitleFontSize;

        TextMeshProUGUI description = CreateText(
            node.title + "Desc",
            nodeObj.transform,
            defaults.nodeDescriptionPosition,
            defaults.nodeDescriptionSize,
            defaults.nodeDescriptionFontSize,
            TextAlignmentOptions.Center,
            node.description,
            FontStyles.Bold,
            defaults.nodeDescriptionColor
        );
        description.textWrappingMode = TextWrappingModes.Normal;
        description.enableAutoSizing = true;
        description.fontSizeMin = 14f;
        description.fontSizeMax = defaults.nodeDescriptionFontSize;

        runtimeNodes.Add(new RuntimeSkillNode
        {
            index = index,
            background = image,
            button = button,
            titleLabel = title,
            descriptionLabel = description
        });
    }

    private void HandleNodeSelected(int nodeIndex)
    {
        SkillTreeManager manager = SkillTreeManager.Instance;
        bool success = false;

        if (!manager.pathChosen)
        {
            if (nodeIndex == 0)
            {
                success = manager.ChoosePath(ElementPath.Arcane);
            }
            else if (nodeIndex == 1)
            {
                success = manager.ChoosePath(ElementPath.Ice);
            }
            else if (nodeIndex == 2)
            {
                success = manager.ChoosePath(ElementPath.Fire);
            }
        }
        else
        {
            if (manager.currentPath == ElementPath.Arcane)
            {
                if (nodeIndex == 0)
                {
                    success = manager.UnlockSpellPower();
                }
                else if (nodeIndex == 1)
                {
                    success = manager.UnlockSpellSpeed();
                }
                else if (nodeIndex == 2)
                {
                    success = manager.UnlockHealChance();
                }
            }
            else if (manager.currentPath == ElementPath.Ice)
            {
                if (nodeIndex == 0)
                {
                    success = manager.UnlockFreezeDuration();
                }
                else if (nodeIndex == 1)
                {
                    success = manager.UnlockMana();
                }
                else if (nodeIndex == 2)
                {
                    success = manager.UnlockFreezePotency();
                }
            }
            else if (manager.currentPath == ElementPath.Fire)
            {
                if (nodeIndex == 0)
                {
                    success = manager.UnlockMoveSpeed();
                }
                else if (nodeIndex == 1)
                {
                    success = manager.UnlockBurnDuration();
                }
                else if (nodeIndex == 2)
                {
                    success = manager.UnlockBurnDamage();
                }
            }
        }

        if (!success)
        {
            Debug.Log("Skill Tree purchase failed. Check path or available skill points.");
            RefreshSkillNodes();
            return;
        }

        RefreshSkillNodes();
    }

    private void RefreshSkillNodes()
    {
        if (!uiBuilt || runtimeNodes.Count == 0)
        {
            return;
        }

        SkillTreeManager manager = SkillTreeManager.Instance;

        if (skillPointsText != null)
        {
            skillPointsText.text = "Skill Points: " + manager.skillPoints;
        }

        if (pathText != null)
        {
            pathText.text = manager.pathChosen
                ? "Path: " + manager.currentPath
                : "Path: Not Chosen";
        }

        if (hintText != null)
        {
            hintText.text = manager.pathChosen
                ? "Spend 1 point per upgrade. I / O / P hotkeys still work for quick testing."
                : "Choose one magic path first (cost: 1 point).";
        }

        for (int i = 0; i < runtimeNodes.Count; i++)
        {
            RuntimeSkillNode runtimeNode = runtimeNodes[i];
            string nodeTitle;
            string nodeDescription;
            Color nodeColor;

            if (!manager.pathChosen)
            {
                BuildPathSelectionNode(i, out nodeTitle, out nodeDescription, out nodeColor);
            }
            else
            {
                BuildUpgradeNode(manager, i, out nodeTitle, out nodeDescription, out nodeColor);
            }

            if (runtimeNode.titleLabel != null)
            {
                runtimeNode.titleLabel.text = nodeTitle;
            }

            if (runtimeNode.descriptionLabel != null)
            {
                runtimeNode.descriptionLabel.text = nodeDescription;
            }

            bool canSpend = manager.skillPoints > 0;
            if (runtimeNode.button != null)
            {
                runtimeNode.button.interactable = canSpend;
            }

            if (runtimeNode.background != null)
            {
                runtimeNode.background.color = canSpend ? nodeColor : disabledNodeColor;
            }
        }
    }

    private void BuildPathSelectionNode(int index, out string title, out string description, out Color color)
    {
        if (index == 0)
        {
            title = "Arcane Power";
            description = "Choose Arcane\nImmediate +Spell Power";
            color = arcaneColor;
            return;
        }

        if (index == 1)
        {
            title = "Cold Flow";
            description = "Choose Ice\nApplies Freeze + Slow";
            color = iceColor;
            return;
        }

        title = "Fiery Stride";
        description = "Choose Fire\nApplies Burn DoT";
        color = fireColor;
    }

    private void BuildUpgradeNode(SkillTreeManager manager, int index, out string title, out string description, out Color color)
    {
        if (manager.currentPath == ElementPath.Arcane)
        {
            color = arcaneColor;
            if (index == 0)
            {
                title = "Spell Power Lv." + manager.spellPowerLevels;
                description = "+10 power per level\nCurrent: +" + manager.GetSpellPowerBonus();
                return;
            }

            if (index == 1)
            {
                title = "Spell Speed Lv." + manager.spellSpeedLevels;
                description = "+5% projectile speed\nCurrent: x" + manager.GetSpellSpeedMultiplier().ToString("0.00");
                return;
            }

            title = "Heal Chance Lv." + manager.healChanceLevels;
            description = "+5% heal chance\nCurrent: " + (manager.GetHealChance() * 100f).ToString("0") + "%";
            return;
        }

        if (manager.currentPath == ElementPath.Ice)
        {
            color = iceColor;
            if (index == 0)
            {
                title = "Freeze Duration Lv." + manager.freezeDurationLevels;
                description = "+2s duration\nCurrent: " + manager.GetFreezeDuration().ToString("0.0") + "s";
                return;
            }

            if (index == 1)
            {
                title = "Mana Lv." + manager.manaLevels;
                description = "+25 max mana\nCurrent: +" + manager.GetManaBonus();
                return;
            }

            title = "Freeze Slow Lv." + manager.freezePotencyLevels;
            description = "+10% slow\nCurrent: " + (manager.GetFreezeSlowAmount() * 100f).ToString("0") + "%";
            return;
        }

        color = fireColor;
        if (index == 0)
        {
            title = "Move Speed Lv." + manager.moveSpeedLevels;
            description = "+25% move speed\nCurrent: x" + manager.GetMoveSpeedBonusMultiplier().ToString("0.00");
            return;
        }

        if (index == 1)
        {
            title = "Burn Duration Lv." + manager.burnDurationLevels;
            description = "+2s burn duration\nCurrent: " + manager.GetBurnDuration().ToString("0.0") + "s";
            return;
        }

        title = "Burn Damage Lv." + manager.burnDamageLevels;
        description = "+3 burn tick damage\nCurrent: " + manager.GetBurnTickDamage();
    }

    private void CreateConnector(Transform parent, Vector2 from, Vector2 to)
    {
        GameObject connectorObj = new GameObject("Connector");
        connectorObj.transform.SetParent(parent, false);

        Vector2 center = (from + to) * 0.5f;
        float width = Mathf.Abs(to.x - from.x);

        RectTransform rt = connectorObj.AddComponent<RectTransform>();
        SetAnchoredRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), center, new Vector2(width, defaults.connectorHeight));

        Image image = connectorObj.AddComponent<Image>();
        image.color = defaults.connectorColor;
    }

    private Button CreatePanelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("CloseSkillTreeButton");
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        SetAnchoredRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), defaults.closeButtonPosition, defaults.closeButtonSize);

        Image image = btnObj.AddComponent<Image>();
        ApplyCloseButtonSkin(image, defaults.closeButtonFallbackColor);

        Button button = btnObj.AddComponent<Button>();

        CreateText(
            "CloseLabel",
            btnObj.transform,
            Vector2.zero,
            defaults.closeButtonSize,
            defaults.closeButtonFontSize,
            TextAlignmentOptions.Center,
            defaults.closeButtonText,
            FontStyles.Bold,
            defaults.closeButtonTextColor
        );

        return button;
    }

    private void ApplyCloseButtonSkin(Image targetImage, Color fallbackColor)
    {
        Sprite skin = ResolveCloseButtonSkin();
        if (skin != null)
        {
            targetImage.sprite = skin;
            targetImage.type = Image.Type.Sliced;
            targetImage.color = Color.white;
            return;
        }

        targetImage.color = fallbackColor;
    }

    private TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        Vector2 anchoredPos,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        string content,
        FontStyles style,
        Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        SetAnchoredRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, size);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
        tmp.color = color;

        return tmp;
    }

    private void SetAnchoredRect(
        RectTransform rt,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPos,
        Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }

    private void StretchToParent(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private void OpenPanel()
    {
        if (panelOverlay != null)
        {
            RefreshSkillNodes();
            panelOverlay.SetActive(true);
        }
    }

    private void ClosePanel()
    {
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
        }
    }

    private void ApplySkin(
        Image targetImage,
        string spriteName,
        ref Sprite cachedSkin,
        ref bool loggedMissing,
        Color fallbackColor,
        Image.Type spriteType)
    {
        Sprite skin = ResolveSkin(spriteName, ref cachedSkin, ref loggedMissing);
        if (skin != null)
        {
            targetImage.sprite = skin;
            targetImage.type = spriteType;
            targetImage.color = Color.white;
            return;
        }

        targetImage.color = fallbackColor;
    }

    private Sprite ResolveSkin(string spriteName, ref Sprite cachedSkin, ref bool loggedMissing)
    {
        if (cachedSkin != null)
        {
            return cachedSkin;
        }

        cachedSkin = ResolveOverrideSkin(spriteName);
        if (cachedSkin != null)
        {
            return cachedSkin;
        }

        cachedSkin = FindSpriteByAnyName(spriteName);
        if (cachedSkin != null)
        {
            return cachedSkin;
        }

        if (!loggedMissing)
        {
            Debug.LogWarning($"SkillTreeUI could not find sprite '{spriteName}'. Using fallback colors.");
            loggedMissing = true;
        }

        return null;
    }

    private Sprite ResolveOverrideSkin(string spriteName)
    {
        if (spriteName == defaults.panelSkinName)
        {
            return panelSkin;
        }

        if (spriteName == defaults.buttonSkinName)
        {
            if (buttonSkin != null)
            {
                return buttonSkin;
            }

            return closeButtonSkin;
        }

        if (spriteName == defaults.closeButtonSkinName)
        {
            return closeButtonSkin;
        }

        return null;
    }

    private void AssignDefaultSkinsIfMissing()
    {
        if (buttonSkin == null)
        {
            buttonSkin = FindSpriteByAnyName(defaults.buttonSkinName, defaults.buttonSkinAltName);
        }

        if (panelSkin == null)
        {
            panelSkin = FindSpriteByAnyName(defaults.panelSkinName);
        }

        if (closeButtonSkin == null)
        {
            closeButtonSkin = FindSpriteByAnyName(defaults.closeButtonSkinName);
        }
    }

    private static SkillTreeUI FindPreferredInstance()
    {
        SkillTreeUI[] all = FindObjectsByType<SkillTreeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (all == null || all.Length == 0)
        {
            return null;
        }

        SkillTreeUI first = null;
        for (int i = 0; i < all.Length; i++)
        {
            SkillTreeUI candidate = all[i];
            if (candidate == null)
            {
                continue;
            }

            if (first == null)
            {
                first = candidate;
            }

            if (candidate.HasAnySkinAssigned())
            {
                return candidate;
            }
        }

        return first;
    }

    private bool HasAnySkinAssigned()
    {
        return buttonSkin != null || panelSkin != null || closeButtonSkin != null;
    }

    private static Sprite FindSpriteByAnyName(params string[] names)
    {
        if (names == null || names.Length == 0)
        {
            return null;
        }

        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        for (int i = 0; i < allSprites.Length; i++)
        {
            Sprite sprite = allSprites[i];
            if (sprite == null)
            {
                continue;
            }

            for (int j = 0; j < names.Length; j++)
            {
                string expected = names[j];
                if (string.IsNullOrWhiteSpace(expected))
                {
                    continue;
                }

                if (NamesMatch(sprite.name, expected))
                {
                    return sprite;
                }
            }
        }

        return null;
    }

    private static bool NamesMatch(string actualName, string expectedName)
    {
        if (string.Equals(actualName, expectedName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return NormalizeName(actualName) == NormalizeName(expectedName);
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = char.ToLowerInvariant(value[i]);
            if (c == '_' || c == '-' || c == ' ')
            {
                continue;
            }

            builder.Append(c);
        }

        return builder.ToString();
    }

    private Sprite ResolveCloseButtonSkin()
    {
        if (cachedCloseButtonSkin != null)
        {
            return cachedCloseButtonSkin;
        }

        if (closeButtonSkin != null)
        {
            cachedCloseButtonSkin = closeButtonSkin;
            return cachedCloseButtonSkin;
        }

        cachedCloseButtonSkin = FindSpriteByAnyName(defaults.closeButtonSkinName);
        if (cachedCloseButtonSkin == null && !loggedMissingCloseButtonSkin)
        {
            Debug.LogWarning($"SkillTreeUI could not find sprite '{defaults.closeButtonSkinName}' for Close button. Using fallback color.");
            loggedMissingCloseButtonSkin = true;
        }

        return cachedCloseButtonSkin;
    }

    private bool ShouldShowOpenButton()
    {
        if (GameManager.Instance.player == null)
        {
            return false;
        }

        GameManager.GameState state = GameManager.Instance.state;
        return state != GameManager.GameState.PREGAME && state != GameManager.GameState.GAMEOVER;
    }
}
