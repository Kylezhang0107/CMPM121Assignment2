using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicUI : MonoBehaviour
{
    public PlayerController player;
    public int index;

    public Image icon;
    public GameObject highlight;
    public TextMeshProUGUI label;

    private float lastRefresh;
    private const float RefreshDelay = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (icon == null)
        {
            Transform iconTransform = transform.Find("icon");
            if (iconTransform != null)
            {
                icon = iconTransform.GetComponent<Image>();
            }
        }

        if (label == null)
        {
            Transform labelTransform = transform.Find("label");
            if (labelTransform != null)
            {
                label = labelTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (highlight == null)
        {
            Transform highlightTransform = transform.Find("highlight");
            if (highlightTransform != null)
            {
                highlight = highlightTransform.gameObject;
            }
        }

        RefreshNow();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= lastRefresh + RefreshDelay)
        {
            RefreshNow();
        }
    }

    public void RefreshNow()
    {
        lastRefresh = Time.time;

        if (player == null)
        {
            gameObject.SetActive(false);
            return;
        }

        PlayerController.RelicData relic = player.GetRelicAt(index);
        if (relic == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (icon != null && GameManager.Instance.relicIconManager != null)
        {
            GameManager.Instance.relicIconManager.PlaceSprite(relic.sprite, icon);
        }

        if (label != null)
        {
            label.text = player.GetRelicLabelAt(index);
        }

        if (highlight != null)
        {
            highlight.SetActive(player.IsRelicActiveAt(index));
        }
    }
}
