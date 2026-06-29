
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject profilePanel;
    public TMP_Text usernameText;
    public TMP_Text usernameText2;
    public TMP_Text levelText;
    public TMP_Text pointsText;
    public TMP_Text currencyText;
    public TMP_Text heroNameText;
    public ScrollRect scrollRect;

    [Header("Hero Images")]
    public Image[] heroImages;
    public string[] heroNames;

    [Header("Topic 1 Progress")]
    public Image topic1ProgressBarFill;
    public Image topic1ProgressBarDuplicateFill;

    [Header("Topic 2 Progress")]
    public Image topic2ProgressBarFill;
    public Image topic2ProgressBarDuplicateFill;

    [Header("Topic 3 Progress")]
    public Image topic3ProgressBarFill;
    public Image topic3ProgressBarDuplicateFill;

    [Header("Badge Images")]
    public Image Badge1;
    public Image Badge2;
    public Image Badge3;
    public Image Badge4;
    public Image Badge5;
    public Image Badge6;

    [Header("Badge Colors")]
    public Color unachievedColor = new Color(0.3f, 0.3f, 0.3f); // Darker
    public Color achievedColor = Color.white; // Bright color

    public ProfilePlayerData currentProfile;

    public static ProfileManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        LoadProfile();
    }

    public void ShowProfilePanel()
    {
        profilePanel.SetActive(true);
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
        LoadProfile();
    }

    public void HideProfilePanel()
    {
        profilePanel.SetActive(false);
    }

    public void LoadProfile()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("No active user found.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("Profile file not found for user: " + activeUser);
            return;
        }

        currentProfile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));

        usernameText.text = currentProfile.username;
        if (usernameText2 != null) usernameText2.text = currentProfile.username;
        levelText.text = "Level: " + currentProfile.level;
        pointsText.text = currentProfile.points.ToString();
        currencyText.text = currentProfile.currency.ToString();
        heroNameText.text = currentProfile.hero;

        foreach (var img in heroImages)
            img.gameObject.SetActive(false);

        string heroValue = currentProfile.hero.Trim().ToLower();
        bool heroFound = false;

        for (int i = 0; i < heroNames.Length; i++)
        {
            string heroNameValue = heroNames[i].Trim().ToLower();
            if (heroNameValue == heroValue ||
                heroNameValue.Contains(heroValue) ||
                heroValue.Contains(heroNameValue))
            {
                if (i < heroImages.Length)
                {
                    heroImages[i].gameObject.SetActive(true);
                    heroFound = true;
                    break;
                }
                else
                {
                    Debug.LogWarning($"ProfileManager: heroNames index {i} exists but heroImages does not.");
                }
            }
        }

        if (!heroFound)
        {
            Debug.LogWarning($"ProfileManager: Hero '{currentProfile.hero}' not found in heroNames array.");
        }

        FixBadgeFlagsConsistency();
        UpdateProgressBars();
        UpdateBadges();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    public void UpdateProgressBars()
    {
        if (currentProfile == null) return;

        float topic1Fill = GetTopicFill(
            currentProfile.miniQuiz1Completed,
            currentProfile.miniQuiz2Completed,
            currentProfile.miniQuiz3Completed,
            currentProfile.mainQuizCompleted
        );
        if (topic1ProgressBarFill != null) topic1ProgressBarFill.fillAmount = topic1Fill;
        if (topic1ProgressBarDuplicateFill != null) topic1ProgressBarDuplicateFill.fillAmount = topic1Fill;

        float topic2Fill = GetTopicFill(
            currentProfile.miniQuiz1bCompleted,
            currentProfile.miniQuiz2bCompleted,
            currentProfile.miniQuiz3bCompleted,
            currentProfile.mainQuizBCompleted
        );
        if (topic2ProgressBarFill != null) topic2ProgressBarFill.fillAmount = topic2Fill;
        if (topic2ProgressBarDuplicateFill != null) topic2ProgressBarDuplicateFill.fillAmount = topic2Fill;

        float topic3Fill = GetTopicFill(
            currentProfile.miniQuiz1cCompleted,
            currentProfile.miniQuiz2cCompleted, 
            currentProfile.miniQuiz3cCompleted,
            currentProfile.mainQuizCCompleted
        );
        if (topic3ProgressBarFill != null) topic3ProgressBarFill.fillAmount = topic3Fill;
        if (topic3ProgressBarDuplicateFill != null) topic3ProgressBarDuplicateFill.fillAmount = topic3Fill;
    }


    private void FixBadgeFlagsConsistency()
    {
        if (currentProfile.points >= 200)
            currentProfile.badge1Shown = true;

        if (currentProfile.level >= 5)
            currentProfile.badge2Shown = true;

        if (currentProfile.miniQuiz1Completed)
            currentProfile.badge3Shown = true;

        bool badge4Achieved = currentProfile.miniQuiz1Completed &&
                            currentProfile.miniQuiz2Completed &&
                            currentProfile.miniQuiz3Completed &&
                            currentProfile.mainQuizCompleted;
        if (badge4Achieved)
            currentProfile.badge4Shown = true;

        bool badge5Achieved = currentProfile.miniQuiz1bCompleted &&
                            currentProfile.miniQuiz2bCompleted &&
                            currentProfile.miniQuiz3bCompleted &&
                            currentProfile.mainQuizBCompleted;
        if (badge5Achieved)
            currentProfile.badge5Shown = true;

        bool badge6Achieved = currentProfile.miniQuiz1cCompleted &&
                            currentProfile.miniQuiz2cCompleted &&
                            currentProfile.miniQuiz3cCompleted &&
                            currentProfile.mainQuizCCompleted;
        if (badge6Achieved)
            currentProfile.badge6Shown = true;
    }

    public void UpdateBadges()
    {
        if (currentProfile == null) return;

        // Badge 1
        CheckBadge(Badge1, currentProfile.points >= 100, ref currentProfile.badge1Shown);
        // Badge 2
        CheckBadge(Badge2, currentProfile.level >= 5, ref currentProfile.badge2Shown);
        // Badge 3
        CheckBadge(Badge3, currentProfile.miniQuiz1Completed, ref currentProfile.badge3Shown);
        // Badge 4
        bool badge4Achieved = currentProfile.miniQuiz1Completed &&
                              currentProfile.miniQuiz2Completed &&
                              currentProfile.miniQuiz3Completed &&
                              currentProfile.mainQuizCompleted;
        CheckBadge(Badge4, badge4Achieved, ref currentProfile.badge4Shown);
        // Badge 5
        bool badge5Achieved = currentProfile.miniQuiz1bCompleted &&
                              currentProfile.miniQuiz2bCompleted &&
                              currentProfile.miniQuiz3bCompleted &&
                              currentProfile.mainQuizBCompleted;
        CheckBadge(Badge5, badge5Achieved, ref currentProfile.badge5Shown);
        // Badge 6
        bool badge6Achieved = currentProfile.miniQuiz1cCompleted &&
                              currentProfile.miniQuiz2cCompleted &&
                              currentProfile.miniQuiz3cCompleted &&
                              currentProfile.mainQuizCCompleted;
        CheckBadge(Badge6, badge6Achieved, ref currentProfile.badge6Shown);

        SaveProfile();
    }

    private void CheckBadge(Image badgeImage, bool achieved, ref bool badgeShownFlag)
    {
        if (badgeImage == null) return;

        // Set color
        badgeImage.color = achieved ? achievedColor : unachievedColor;
        badgeShownFlag = achieved;
    }



    private float GetTopicFill(bool q1, bool q2, bool q3, bool main)
    {
        int completed = 0;
        if (q1) completed++;
        if (q2) completed++;
        if (q3) completed++;
        if (main) completed++;
        return (float)completed / 4f;
    }

    private void SaveProfile()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser) || currentProfile == null) return;

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        File.WriteAllText(path, JsonUtility.ToJson(currentProfile, true));
    }
}
