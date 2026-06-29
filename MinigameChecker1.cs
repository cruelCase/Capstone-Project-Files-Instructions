using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MinigameChecker1 : MonoBehaviour
{
    [Header("Quiz Scene Asset")]
#if UNITY_EDITOR
    public SceneAsset quizSceneAsset;
#endif

    [HideInInspector]
    public string quizSceneName;

    [Header("Scene Load UI")]
    public GameObject loadingScreen;
    public Slider progressBar;

    [Header("Blocked UI")]
    public GameObject blockedPanel;

    public void LoadMainQuizSceneForActiveUser()
    {
        ProfilePlayerData profile = GetActiveUserProfile();
        if (profile == null)
            return;

        string sceneToLoad = GetMainQuizSceneForProfile(profile);
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("MinigameChecker1: Active user has not met the requirements for any main quiz scene.");
            ShowBlockedPanel();
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    private ProfilePlayerData GetActiveUserProfile()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogError("MinigameChecker1: No active user found. Cannot load main quiz scene.");
            ShowBlockedPanel();
            return null;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogError($"MinigameChecker1: Profile file not found for user '{activeUser}'. Cannot load main quiz scene.");
            ShowBlockedPanel();
            return null;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        if (profile == null)
        {
            Debug.LogError("MinigameChecker1: Failed to parse profile JSON. Cannot load main quiz scene.");
            ShowBlockedPanel();
            return null;
        }

        return profile;
    }

    private string GetMainQuizSceneForProfile(ProfilePlayerData profile)
    {
        if (profile.miniQuiz1bCompleted && profile.miniQuiz2bCompleted && profile.miniQuiz3bCompleted)
            return quizSceneName;

        return string.Empty;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (quizSceneAsset != null)
            quizSceneName = quizSceneAsset.name;
    }
#endif

    private void ShowBlockedPanel()
    {
        if (blockedPanel == null)
        {
            Debug.LogWarning("MinigameChecker1: blockedPanel is not assigned in the inspector.");
            return;
        }

        blockedPanel.SetActive(true);
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 2.8f;
        float targetProgress = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            targetProgress = Mathf.Max(targetProgress, realProgress);

            if (progressBar != null)
                progressBar.value = Mathf.Lerp(progressBar.value, targetProgress, Time.deltaTime * 2f);

            if (realProgress >= 1f && timer >= minLoadTime)
            {
                if (progressBar != null)
                    progressBar.value = 1f;

                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
