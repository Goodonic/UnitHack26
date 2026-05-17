using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    public GameObject victoryPanel;

    private void Start()
    {
        victoryPanel.SetActive(false);
    }

    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);
    }

    public void CloseVictoryScreen()
    {
        victoryPanel.SetActive(false);
    }
}
