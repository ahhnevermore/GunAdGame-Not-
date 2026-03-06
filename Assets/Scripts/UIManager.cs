using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel; // A panel containing the game over text

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        UpdateScore(0);
    }

    public void UpdateScore(long newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore.ToString("N0"); // Format with commas
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
