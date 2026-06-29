using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class SceneAController : MonoBehaviour
{ // Assign in Inspector
    private string username;
    

    void Start()
    {
        username = PlayerPrefs.GetString("ActiveUser", "Guest");
    }

    /// <summary>
    /// Downloads both pretest and posttest results. Attach to button's OnClick event.
    /// </summary>
    public void DownloadTestResults()
    {
        Debug.Log("Starting download of pretest and posttest results for: " + username);
        StartCoroutine(DownloadResults());
    }

    /// <summary>
    /// Downloads pretest results only. Can be called independently or as part of DownloadTestResults().
    /// </summary>
    public void DownloadPretest()
    {
        StartCoroutine(DownloadPretestResults());
    }

    /// <summary>
    /// Downloads posttest results only. Can be called independently or as part of DownloadTestResults().
    /// </summary>
    public void DownloadPosttest()
    {
        StartCoroutine(DownloadPosttestResults());
    }

    private IEnumerator DownloadResults()
    {
        // Download Pretest Results
        yield return StartCoroutine(DownloadPretestResults());

        // Download Posttest Results
        yield return StartCoroutine(DownloadPosttestResults());

        Debug.Log("All results downloaded successfully!");
    }

    private IEnumerator DownloadPretestResults()
    {
        Debug.Log("Downloading pretest results for: " + username);
        
        string profilePath = Path.Combine(Application.persistentDataPath, username + "_profile.json");
        
        if (!File.Exists(profilePath))
        {
            Debug.LogWarning("Profile file not found: " + profilePath);
            yield break;
        }

        string json = File.ReadAllText(profilePath);
        ProfilePlayerData profileData = JsonUtility.FromJson<ProfilePlayerData>(json);

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("PRETEST RESULTS");
        report.AppendLine("Username: " + profileData.username);
        report.AppendLine("Score: " + profileData.pretestScore);
        report.AppendLine("Completed: " + profileData.pretestCompleted);
        report.AppendLine("");

        for (int i = 0; i < profileData.pretestResults.Count; i++)
        {
            var result = profileData.pretestResults[i];
            report.AppendLine("Question " + (i + 1) + ": " + result.questionText);
            report.AppendLine("Answer: " + result.userAnswer + " | Correct: " + result.isCorrect);
            report.AppendLine("");
        }

        string exportPath = Path.Combine(Application.persistentDataPath, username + "_PretestResults.txt");
        File.WriteAllText(exportPath, report.ToString());
        Debug.Log("Pretest results exported to: " + exportPath);
        
        yield return null;
    }

    private IEnumerator DownloadPosttestResults()
    {
        Debug.Log("Downloading posttest results for: " + username);
        
        string profilePath = Path.Combine(Application.persistentDataPath, username + "_profile.json");
        
        if (!File.Exists(profilePath))
        {
            Debug.LogWarning("Profile file not found: " + profilePath);
            yield break;
        }

        string json = File.ReadAllText(profilePath);
        ProfilePlayerData profileData = JsonUtility.FromJson<ProfilePlayerData>(json);

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("POSTTEST RESULTS");
        report.AppendLine("Username: " + profileData.username);
        report.AppendLine("Score: " + profileData.posttestScore);
        report.AppendLine("Completed: " + profileData.posttestCompleted);
        report.AppendLine("");

        for (int i = 0; i < profileData.posttestResults.Count; i++)
        {
            var result = profileData.posttestResults[i];
            report.AppendLine("Question " + (i + 1) + ": " + result.questionText);
            report.AppendLine("Answer: " + result.userAnswer + " | Correct: " + result.isCorrect);
            report.AppendLine("");
        }

        string exportPath = Path.Combine(Application.persistentDataPath, username + "_PosttestResults.txt");
        File.WriteAllText(exportPath, report.ToString());
        Debug.Log("Posttest results exported to: " + exportPath);
        
        yield return null;
    }
}
