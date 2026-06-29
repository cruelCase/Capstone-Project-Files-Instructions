using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDLManager : MonoBehaviour
{
    [SerializeField] private Button targetButton;

    private void Start()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        // Set button to uninteractable by default
        if (targetButton != null)
        {
            targetButton.interactable = false;
        }

        // Check if post-test is completed and enable button if true
        CheckAndEnableButton();
    }

    /// <summary>
    /// Loads the active user and checks if posttestCompleted is true.
    /// If true, makes the target button interactable.
    /// </summary>
    private void CheckAndEnableButton()
    {
        string username = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("No active user found!");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, username + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("Profile JSON not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        // Enable button only if posttestCompleted is true
        if (profile.posttestCompleted && targetButton != null)
        {
            targetButton.interactable = true;
            Debug.Log($"Post-test completed for {username}. Button enabled.");
        }
        else
        {
            Debug.Log($"Post-test not completed for {username}. Button remains disabled.");
        }
    }
}
