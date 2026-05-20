using UnityEngine;
using System.Collections.Generic;

public class RelicUIManager : MonoBehaviour
{
    public GameObject relicUIPrefab;
    public PlayerController player;
    [SerializeField] private float startX = -450f;
    [SerializeField] private float spacing = 40f;

    private readonly List<RelicUI> relicUIs = new List<RelicUI>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            if (GameManager.Instance.player != null)
            {
                player = GameManager.Instance.player.GetComponent<PlayerController>();
            }

            if (player == null)
            {
                player = FindFirstObjectByType<PlayerController>();
            }
        }

        if (player != null)
        {
            player.OnRelicGranted += OnRelicGranted;
            RefreshAll();
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnRelicGranted -= OnRelicGranted;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }

        // Covers cases where relics were granted before this UI manager initialized.
        if (relicUIs.Count < player.RelicCount)
        {
            RefreshAll();
        }
    }

    public void OnRelicGranted(PlayerController.RelicData relic)
    {
        GameObject rui = Instantiate(relicUIPrefab, transform);
        int index = relicUIs.Count;
        rui.transform.localPosition = new Vector3(startX + spacing * index, 0, 0);

        RelicUI ruic = rui.GetComponent<RelicUI>();
        if (ruic != null)
        {
            ruic.player = player;
            ruic.index = index;
            ruic.RefreshNow();
            relicUIs.Add(ruic);
        }
    }

    private void RefreshAll()
    {
        for (int i = relicUIs.Count; i < player.RelicCount; ++i)
        {
            OnRelicGranted(player.GetRelicAt(i));
        }
    }
}
