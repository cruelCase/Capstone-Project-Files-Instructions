using UnityEngine;
using TMPro;

public class ScorePanel : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI commentText; // <-- add this in Inspector

    public void OnExitScorePanel()
    {

        // Refresh profile and UI
        GameManager.Instance.RefreshProfileData();
    }

    public void ShowScore(string scoreDisplay, string comment)
    {
        panel.SetActive(true);
        scoreText.text = "Your Score:\n" + scoreDisplay;
        commentText.text = comment;
    }
}
