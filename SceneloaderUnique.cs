using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneloaderUnique : MonoBehaviour
{
    [Header("UI")]
    public GameObject loadingScreen;
    public Slider progressBar;

    [Header("Three Scene Slots")]
#if UNITY_EDITOR
    public SceneAsset scene1_NoHero;      // Load when hero is empty/DEFAULT
    public SceneAsset scene2_HeroSelected; // Load when hero is set but pretest not completed
    public SceneAsset scene3_PretestDone;  // Load when hero is set AND pretest completed
#endif
    [HideInInspector]
    public string scene1Name;
    [HideInInspector]
    public string scene2Name;
    [HideInInspector]
    public string scene3Name;

    private string username;

    public void LoadAppropriateScene()
    {
        // Get the active user
        username = PlayerPrefs.GetString("ActiveUser", "");

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("SceneloaderUnique: No ActiveUser set. Cannot load scene.");
            return;
        }

        // Load user profile
        string path = Path.Combine(Application.persistentDataPath, username + "_profile.json");

        string sceneToLoad = scene1Name; // Default to scene 1

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

            if (profile != null)
            {
                // Check if hero is set
                bool hasHero = !string.IsNullOrEmpty(profile.hero) && profile.hero != "DEFAULT";

                if (hasHero && profile.pretestCompleted)
                {
                    // Both hero and pretest completed
                    sceneToLoad = scene3Name;
                    Debug.Log($"SceneloaderUnique: Hero set ({profile.hero}) and pretest completed. Loading Scene 3: {scene3Name}");
                }
                else if (hasHero && !profile.pretestCompleted)
                {
                    // Hero set but pretest not completed
                    sceneToLoad = scene2Name;
                    Debug.Log($"SceneloaderUnique: Hero set ({profile.hero}) but pretest not completed. Loading Scene 2: {scene2Name}");
                }
                else
                {
                    // No hero set
                    sceneToLoad = scene1Name;
                    Debug.Log($"SceneloaderUnique: No hero set. Loading Scene 1: {scene1Name}");
                }
            }
            else
            {
                Debug.LogWarning("SceneloaderUnique: Failed to parse profile data. Loading Scene 1.");
                sceneToLoad = scene1Name;
            }
        }
        else
        {
            Debug.LogWarning($"SceneloaderUnique: Profile file not found at {path}. Loading Scene 1.");
            sceneToLoad = scene1Name;
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("SceneloaderUnique: No scene assigned for the determined state!");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (scene1_NoHero != null)
            scene1Name = scene1_NoHero.name;

        if (scene2_HeroSelected != null)
            scene2Name = scene2_HeroSelected.name;

        if (scene3_PretestDone != null)
            scene3Name = scene3_PretestDone.name;
    }
#endif

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 2.8f;   // Minimum time to show loading screen
        float targetProgress = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smoothly move target toward real progress
            targetProgress = Mathf.Max(targetProgress, realProgress);

            // Smooth fill progress bar
            if (progressBar != null)
                progressBar.value = Mathf.Lerp(progressBar.value, targetProgress, Time.deltaTime * 2f);

            // Wait until BOTH: scene is ready AND min time passed
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
