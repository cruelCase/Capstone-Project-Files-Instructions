using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementBoardUI : MonoBehaviour
{
    [Header("LEVEL")]
    public Image levelProgressBarFill;
    public TMP_Text levelText;
    public TMP_Text levelStageText; // NEW
    public Image[] levelStageImages;

    [Header("POINTS")]
    public Image pointsProgressBarFill;
    public TMP_Text pointsText;
    public TMP_Text pointsStageText; // NEW
    public Image[] pointsStageImages;

    [Header("CURRENCY")]
    public Image currencyProgressBarFill;
    public TMP_Text currencyText;
    public TMP_Text currencyStageText; // NEW
    public Image[] currencyStageImages;

    [Header("Stage Maximums")]
    public int[] levelStageMax = new int[5] { 5, 10, 15, 20, 25 };
    public int[] pointsStageMax = new int[5] { 100, 200, 400, 600, 1000 };
    public int[] currencyStageMax = new int[5] { 500, 1000, 2000, 3500, 5000 };

    private ProfilePlayerData currentProfile;

    void OnEnable()
    {
        LoadAchievementBoard();
    }

    void LoadAchievementBoard()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser)) return;

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path)) return;

        currentProfile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));

        // LEVEL
        UpdateStageProgress(currentProfile.level, levelStageMax, levelProgressBarFill, levelText, levelStageImages, levelStageText, "Antas");

        // POINTS
        UpdateStageProgress(currentProfile.points, pointsStageMax, pointsProgressBarFill, pointsText, pointsStageImages, pointsStageText, "Pontus");

        // CURRENCY
        UpdateStageProgress(currentProfile.currency, currencyStageMax, currencyProgressBarFill, currencyText, currencyStageImages, currencyStageText, "Pera");
    }

    void UpdateStageProgress(int value, int[] stageMax, Image fillBar, TMP_Text valueText, Image[] stageImages, TMP_Text stageText, string label)
    {
        int stageIndex = 0;

        for (int i = 0; i < stageMax.Length; i++)
        {
            if (value <= stageMax[i])
            {
                stageIndex = i;
                break;
            }
        }

        // Activate only the current stage image
        for (int i = 0; i < stageImages.Length; i++)
            stageImages[i].gameObject.SetActive(i == stageIndex);

        int prevStageMax = stageIndex == 0 ? 0 : stageMax[stageIndex - 1];
        int stageRange = stageMax[stageIndex] - prevStageMax;

        // Fill amount relative to current stage
        float fill = Mathf.Clamp01((float)(value - prevStageMax) / stageRange);
        fillBar.fillAmount = fill;

        // Update main value text
        valueText.text = $"{label}: {value}";

        // Update stage description text
        stageText.text = $"kasalukuyang yugto {stageIndex + 1}: {prevStageMax + 1} - {stageMax[stageIndex]}";
    }
}
