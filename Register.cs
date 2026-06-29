using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_Text messageText;
    public float messageDuration = 2.5f;
    public string successSceneName = ""; // Leave empty to stay on intro scene after registration
    public int maxUsernameLength = 12;

    [Header("Error Message Images (Assign in Inspector)")]
    public Image errorEmptyUsernameImage;
    public Image errorEmptyPasswordImage;
    public Image errorPasswordMismatchImage;
    public Image errorUsernameTooLongImage;
    public Image errorUsernameExistsImage;
    public Image errorUsernameMinCharImage;
    public Image errorPasswordMinCharImage;
    public Image successMessageImage;

    public void RegisterUser()
    {
        string username = usernameInput != null ? usernameInput.text.Trim() : string.Empty;
        string password = passwordInput != null ? passwordInput.text : string.Empty;

        if (string.IsNullOrEmpty(username))
        {
            ShowErrorImage(errorEmptyUsernameImage);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowErrorImage(errorEmptyPasswordImage);
            return;
        }

        if (username.Length < 3)
        {
            ShowErrorImage(errorUsernameMinCharImage);
            return;
        }

        if (password.Length < 4)
        {
            ShowErrorImage(errorPasswordMinCharImage);
            return;
        }

        string confirm = confirmPasswordInput != null ? confirmPasswordInput.text : string.Empty;
        if (password != confirm)
        {
            ShowErrorImage(errorPasswordMismatchImage);
            return;
        }

        if (username.Length > maxUsernameLength)
        {
            ShowErrorImage(errorUsernameTooLongImage);
            return;
        }

        if (IsUsernameTaken(username))
        {
            ShowErrorImage(errorUsernameExistsImage);
            return;
        }

        ProfilePlayerData profile = new ProfilePlayerData
        {
            username = username,
            password = password,
            uniqueId = System.Guid.NewGuid().ToString("N"),
            currency = 250,
            xp = 0,
            level = 1,
            points = 0,
            hero = string.Empty,
            selectedSpriteLibrary = "Original",
            selectedBody = "DEFAULT_BODY",
            selectedHair = "DEFAULT_HAIR",
            selectedOutfit = "DEFAULT_OUTFIT",
            heroCostume = "DEFAULT",
            miniQuiz1Completed = false,
            miniQuiz2Completed = false,
            miniQuiz3Completed = false,
            mainQuizCompleted = false,
            miniQuiz1bCompleted = false,
            miniQuiz2bCompleted = false,
            miniQuiz3bCompleted = false,
            mainQuizBCompleted = false,
            miniQuiz1cCompleted = false,
            miniQuiz2cCompleted = false,
            miniQuiz3cCompleted = false,
            mainQuizCCompleted = false,
            pretestCompleted = false,
            pretestScore = 0,
            posttestCompleted = false,
            posttestScore = 0,
            world1Active = false,
            world2Active = false,
            world3Active = false,
            world4Active = false
            ,
            item1 = 0,
            item2 = 0
        };

        File.WriteAllText(GetProfilePath(username), JsonUtility.ToJson(profile, true));
        AddUsernameToUserList(username);

        // Do NOT automatically log in the user — they must log in separately
        // PlayerPrefs.SetString("ActiveUser", username);
        // PlayerPrefs.Save();

        usernameInput.text = string.Empty;
        passwordInput.text = string.Empty;
        if (confirmPasswordInput != null)
            confirmPasswordInput.text = string.Empty;

        ShowErrorImage(successMessageImage);

        // If a Login manager exists in the scene, delay then show the login panel and hide the register panel.
        Login loginManager = FindObjectOfType<Login>();
        StartCoroutine(HandlePostRegistration(loginManager));
    }

    private IEnumerator HandlePostRegistration(Login loginManager)
    {
        // Wait so the success popup is visible
        yield return new WaitForSeconds(2f);

        if (loginManager != null)
            loginManager.OnRegistrationSuccess_ShowLogin();

        // Only load scene if successSceneName is explicitly set
        // if (!string.IsNullOrEmpty(successSceneName))
        //     SceneManager.LoadScene(successSceneName);
    }

    private bool IsUsernameTaken(string username)
    {
        string path = GetProfilePath(username);
        return File.Exists(path);
    }

    private string GetProfilePath(string username)
    {
        return Path.Combine(Application.persistentDataPath, username + "_profile.json");
    }

    private void AddUsernameToUserList(string username)
    {
        UserData data = LoadUserList();
        if (data.users == null)
            data.users = new List<string>();

        if (!data.users.Contains(username))
            data.users.Add(username);

        File.WriteAllText(Path.Combine(Application.persistentDataPath, "users.json"), JsonUtility.ToJson(data, true));
    }

    private UserData LoadUserList()
    {
        string path = Path.Combine(Application.persistentDataPath, "users.json");
        if (!File.Exists(path))
            return new UserData { users = new List<string>() };

        return JsonUtility.FromJson<UserData>(File.ReadAllText(path)) ?? new UserData { users = new List<string>() };
    }

    private void ShowErrorImage(Image errorImage)
    {
        if (errorImage != null)
        {
            StopAllCoroutines();
            errorImage.gameObject.SetActive(true);
            StartCoroutine(HideImageAfterDelay(errorImage));
        }
    }

    private IEnumerator HideImageAfterDelay(Image errorImage)
    {
        yield return new WaitForSeconds(messageDuration);

        if (errorImage != null)
            errorImage.gameObject.SetActive(false);
    }

    [System.Serializable]
    private class UserData
    {
        public List<string> users = new List<string>();
    }
}
