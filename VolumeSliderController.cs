using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VolumeSliderController : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;

    private const string VolumeKey = "MasterVolume";

    void Start()
    {
        // Load saved volume or default to 1
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        // Apply volume
        AudioListener.volume = savedVolume;

        // Set slider value
        volumeSlider.value = savedVolume;

        // Add listener
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    // --- Pretest Download Method ---
    public void DownloadPretestAsTxt()
    {
        string username = PlayerPrefs.GetString("ActiveUser", "Player");
        string profilePath = Path.Combine(Application.persistentDataPath, username + "_profile.json");

        if (!File.Exists(profilePath))
        {
            Debug.LogWarning("Profile file not found: " + profilePath);
            return;
        }

        string json = File.ReadAllText(profilePath);
        ProfilePlayerData profileData = JsonUtility.FromJson<ProfilePlayerData>(json);

        System.Text.StringBuilder report = new System.Text.StringBuilder();

        report.AppendLine("PRETEST RESULT REPORT");
        report.AppendLine("----------------------------------------");
        report.AppendLine("Username: " + profileData.username);
        report.AppendLine("Unique ID: " + profileData.uniqueId);
        report.AppendLine("Score: " + profileData.pretestScore + " / 15");
        report.AppendLine("");

        for (int i = 0; i < profileData.pretestResults.Count; i++)
        {
            var result = profileData.pretestResults[i];

            report.AppendLine("----------------------------------------");
            report.AppendLine("Question " + (i + 1) + ":");
            report.AppendLine(result.questionText);

            string answerLetter = ConvertAnswerIndex(result.userAnswer);

            report.AppendLine("Your Answer: " + answerLetter);

            if (result.isCorrect)
            {
                report.AppendLine("Result: Correct");
            }
            else
            {
                report.AppendLine("Result: Wrong");
                report.AppendLine("Correct Answer: " + result.correctAnswerText);
            }

            report.AppendLine("");
        }

        string exportPath = Path.Combine(Application.persistentDataPath, username + "_PretestReport.txt");
        File.WriteAllText(exportPath, report.ToString());

        Debug.Log("Pretest report downloaded to: " + exportPath);
    }

    // --- New PostTest Download Method ---
    public void DownloadPosttestAsTxt()
    {
        string username = PlayerPrefs.GetString("ActiveUser", "Player");
        string profilePath = Path.Combine(Application.persistentDataPath, username + "_profile.json");

        if (!File.Exists(profilePath))
        {
            Debug.LogWarning("Profile file not found: " + profilePath);
            return;
        }

        string json = File.ReadAllText(profilePath);
        ProfilePlayerData profileData = JsonUtility.FromJson<ProfilePlayerData>(json);

        System.Text.StringBuilder report = new System.Text.StringBuilder();

        report.AppendLine("POSTTEST RESULT REPORT");
        report.AppendLine("----------------------------------------");
        report.AppendLine("Username: " + profileData.username);
        report.AppendLine("Unique ID: " + profileData.uniqueId);
        report.AppendLine("Score: " + profileData.posttestScore + " / 15");
        report.AppendLine("");

        for (int i = 0; i < profileData.posttestResults.Count; i++)
        {
            var result = profileData.posttestResults[i];

            report.AppendLine("----------------------------------------");
            report.AppendLine("Question " + (i + 1) + ":");
            report.AppendLine(result.questionText);

            string answerLetter = ConvertAnswerIndex(result.userAnswer);

            report.AppendLine("Your Answer: " + answerLetter);

            if (result.isCorrect)
            {
                report.AppendLine("Result: Correct");
            }
            else
            {
                report.AppendLine("Result: Wrong");
                report.AppendLine("Correct Answer: " + result.correctAnswerText);
            }

            report.AppendLine("");
        }

        string exportPath = Path.Combine(Application.persistentDataPath, username + "_PosttestReport.txt");
        File.WriteAllText(exportPath, report.ToString());

        Debug.Log("Posttest report downloaded to: " + exportPath);
    }

    string ConvertAnswerIndex(int index)
    {
        switch (index)
        {
            case 0: return "A";
            case 1: return "B";
            case 2: return "C";
            case 3: return "D";
            default: return "No Answer";
        }
    }
}
