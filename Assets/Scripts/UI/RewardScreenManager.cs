using Unity.VisualScripting;
using UnityEngine;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public GameObject endUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            endUI.SetActive(true);
            rewardUI.SetActive(false);
        }
        else if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            
            rewardUI.SetActive(true);
            endUI.SetActive(false);
        }
        else
        {
            rewardUI.SetActive(false);
            endUI.SetActive(false);
        }
    }
}
