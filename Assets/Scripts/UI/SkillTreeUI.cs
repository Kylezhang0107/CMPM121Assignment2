using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkillTreeUIBootstrap : MonoBehaviour
{
    private static SkillTreeUIBootstrap instance;
    private const string ButtonSkinSpriteName = "ProjectUtumno_full_1153";
    private const string PanelSkinSpriteName = "UIPanel";

    private Canvas skillTreeCanvas;
    private Button openButton;
    private GameObject panelOverlay;
    private bool uiBuilt;
    private Sprite cachedButtonSkin;
    private Sprite cachedPanelSkin;
    private bool loggedMissingButtonSkin;
    private bool loggedMissingPanelSkin;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject bootstrapObj = new GameObject("SkillTreeUIBootstrap");
        instance = bootstrapObj.AddComponent<SkillTreeUIBootstrap>();
        DontDestroyOnLoad(bootstrapObj);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiBuilt = false;

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
            TryBuildUI();
            return;
        }

        bool shouldShowButton = ShouldShowOpenButton();

        if (openButton != null)
        {
            openButton.gameObject.SetActive(shouldShowButton);
        }

        if (!shouldShowButton && panelOverlay != null && panelOverlay.activeSelf)
        {
            panelOverlay.SetActive(false);
        }
    }

    private void TryBuildUI()
    {
        skillTreeCanvas = CreateCanvas();
        if (skillTreeCanvas == null)
        {
            return;
        }

        BuildOpenButton(skillTreeCanvas.transform);
        BuildSkillTreePanel(skillTreeCanvas.transform);

        uiBuilt = true;
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("SkillTreeCanvas");
        DontDestroyOnLoad(canvasObj);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 220;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private void BuildOpenButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("OpenSkillTreeButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-95f, 82f);
        rt.sizeDelta = new Vector2(88f, 88f);

        Image image = buttonObj.AddComponent<Image>();
        ApplySkin(
            image,
            ButtonSkinSpriteName,
            ref cachedButtonSkin,
            ref loggedMissingButtonSkin,
            new Color(0.86f, 0.23f, 0.18f, 0.95f),
            Image.Type.Sliced
        );

        openButton = buttonObj.AddComponent<Button>();

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);

        RectTransform labelRT = labelObj.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(6f, 6f);
        labelRT.offsetMax = new Vector2(-6f, -6f);

        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = "Skill Tree";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24f;
        label.color = Color.white;

        openButton.onClick.AddListener(OpenPanel);
        openButton.gameObject.SetActive(false);
    }

    private void BuildSkillTreePanel(Transform parent)
    {
        panelOverlay = new GameObject("SkillTreeOverlay");
        panelOverlay.transform.SetParent(parent, false);

        RectTransform overlayRT = panelOverlay.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        Image overlayImage = panelOverlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.5f);

        Button overlayButton = panelOverlay.AddComponent<Button>();
        overlayButton.onClick.AddListener(ClosePanel);

        GameObject cardObj = new GameObject("SkillTreeCard");
        cardObj.transform.SetParent(panelOverlay.transform, false);

        RectTransform cardRT = cardObj.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot = new Vector2(0.5f, 0.5f);
        cardRT.anchoredPosition = Vector2.zero;
        cardRT.sizeDelta = new Vector2(980f, 620f);

        Image cardImage = cardObj.AddComponent<Image>();
        ApplySkin(
            cardImage,
            PanelSkinSpriteName,
            ref cachedPanelSkin,
            ref loggedMissingPanelSkin,
            new Color(0.12f, 0.14f, 0.2f, 0.98f),
            Image.Type.Sliced
        );

        GameObject titleObj = CreateText(
            "SkillTreeTitle",
            cardObj.transform,
            new Vector2(0f, 270f),
            new Vector2(780f, 56f),
            40f,
            TextAlignmentOptions.Center,
            "Skill Tree"
        );

        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.color = new Color(0.15f, 0.11f, 0.07f, 1f);
        titleText.fontStyle = FontStyles.Bold;

        GameObject subtitleObj = CreateText(
            "SkillTreeSubtitle",
            cardObj.transform,
            new Vector2(0f, 220f),
            new Vector2(840f, 42f),
            24f,
            TextAlignmentOptions.Center,
            "Stats only: choose upgrades that strengthen your build"
        );

        TextMeshProUGUI subtitleText = subtitleObj.GetComponent<TextMeshProUGUI>();
        subtitleText.color = new Color(0.25f, 0.2f, 0.14f, 1f);
        subtitleText.fontStyle = FontStyles.Bold;

        CreateNode(cardObj.transform, new Vector2(-250f, 70f), "Power", "+10% spell damage");
        CreateNode(cardObj.transform, new Vector2(0f, 70f), "Flow", "+20 max mana");
        CreateNode(cardObj.transform, new Vector2(250f, 70f), "Stride", "+10% move speed");

        CreateConnector(cardObj.transform, new Vector2(-125f, 70f), 240f);
        CreateConnector(cardObj.transform, new Vector2(125f, 70f), 240f);

        Button closeButton = CreatePanelButton(
            "CloseSkillTreeButton",
            cardObj.transform,
            new Vector2(0f, -250f),
            new Vector2(200f, 56f),
            "Close"
        );
        closeButton.onClick.AddListener(ClosePanel);

        panelOverlay.SetActive(false);
    }

    private void CreateNode(Transform parent, Vector2 position, string title, string description)
    {
        GameObject nodeObj = new GameObject(title + "Node");
        nodeObj.transform.SetParent(parent, false);

        RectTransform rt = nodeObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(220f, 170f);

        Image image = nodeObj.AddComponent<Image>();
        image.color = new Color(0.22f, 0.27f, 0.36f, 1f);

        CreateText(
            title + "Title",
            nodeObj.transform,
            new Vector2(0f, 44f),
            new Vector2(190f, 40f),
            28f,
            TextAlignmentOptions.Center,
            title
        );

        TextMeshProUGUI nodeTitleText = nodeObj.transform.Find(title + "Title")?.GetComponent<TextMeshProUGUI>();
        if (nodeTitleText != null)
        {
            nodeTitleText.color = new Color(0.96f, 0.97f, 1f, 1f);
            nodeTitleText.fontStyle = FontStyles.Bold;
        }

        GameObject descObj = CreateText(
            title + "Desc",
            nodeObj.transform,
            new Vector2(0f, -20f),
            new Vector2(190f, 80f),
            20f,
            TextAlignmentOptions.Center,
            description
        );

        TextMeshProUGUI desc = descObj.GetComponent<TextMeshProUGUI>();
        desc.color = new Color(0.9f, 0.93f, 1f, 1f);
        desc.fontStyle = FontStyles.Bold;
    }

    private void CreateConnector(Transform parent, Vector2 position, float width)
    {
        GameObject connectorObj = new GameObject("Connector");
        connectorObj.transform.SetParent(parent, false);

        RectTransform rt = connectorObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(width, 6f);

        Image image = connectorObj.AddComponent<Image>();
        image.color = new Color(0.6f, 0.7f, 0.88f, 0.8f);
    }

    private Button CreatePanelButton(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string text)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.78f, 0.24f, 0.2f, 1f);

        Button button = btnObj.AddComponent<Button>();

        CreateText(
            name + "Label",
            btnObj.transform,
            Vector2.zero,
            size,
            28f,
            TextAlignmentOptions.Center,
            text
        );

        return button;
    }

    private GameObject CreateText(
        string name,
        Transform parent,
        Vector2 anchoredPos,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        string content)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        return textObj;
    }

    private void OpenPanel()
    {
        if (panelOverlay == null)
        {
            return;
        }

        panelOverlay.SetActive(true);
    }

    private void ClosePanel()
    {
        if (panelOverlay == null)
        {
            return;
        }

        panelOverlay.SetActive(false);
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

        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        for (int i = 0; i < allSprites.Length; i++)
        {
            Sprite sprite = allSprites[i];
            if (sprite != null && sprite.name == spriteName)
            {
                cachedSkin = sprite;
                break;
            }
        }

        if (cachedSkin == null && !loggedMissing)
        {
            Debug.LogWarning($"SkillTreeUIBootstrap could not find sprite '{spriteName}'. Using fallback colors.");
            loggedMissing = true;
        }

        return cachedSkin;
    }

    private bool ShouldShowOpenButton()
    {
        if (GameManager.Instance.player == null)
        {
            return false;
        }

        GameManager.GameState state = GameManager.Instance.state;
        return state != GameManager.GameState.PREGAME
            && state != GameManager.GameState.GAMEOVER;
    }
}
