using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References (TextMeshProUGUI)")]
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI pointsText;

    [Header("XP Bar")]
    public Image xpBarFill;

    [Header("Player Reference")]
    public GameObject player;

    // Backing fields
    private string _username;
    private int _currency;
    private int _xp;
    private int _level;
    private int _points;
    private string _selectedCharacter;
    private string _password;
    private string _uniqueId;
    private int _item1 = 0;
    private int _item2 = 0;


    // Outfit tracking
    public int SelectedOutfitIndex = 0;

    // Quiz Completion Flags
    private bool _miniQuiz1 = false;
    private bool _miniQuiz2 = false;
    private bool _miniQuiz3 = false;
    private bool _mainQuiz = false;
    private bool _miniQuiz1b = false;
    private bool _miniQuiz2b = false;
    private bool _miniQuiz3b = false;
    private bool _mainQuizB = false;
    private bool _hasDoneFirstWorld1ToWorld2Transition = false;
    private bool _hasDoneFirstWorld2ToWorld3Transition = false;
    private bool _miniQuiz1c = false;
    private bool _miniQuiz2c = false;
    private bool _miniQuiz3c = false;
    private bool _mainQuizC = false;
    private bool _purchasedVillager2 = false;
    private bool _purchasedVillager3 = false;
    private bool _hasShownAchievementW1 = false;
    private bool _hasShownAchievementW2 = false;
    private bool _hasShownAchievementW3 = false;
    private bool _hasShownAnnouncementW1 = false;
    private bool _hasShownAnnouncementW2 = false;
    private bool _hasShownFinalAnnouncement = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadData();
        ApplyDataToUI();
        UpdateXPBar();
    }

        public string Password
    {
        get => _password;
        set => _password = value; // optional: you might skip SaveData() here
    }

    public string UniqueId
    {
        get => _uniqueId;
        set => _uniqueId = value;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reload UI references after scene load
        usernameText = GameObject.Find("UsernameText")?.GetComponent<TextMeshProUGUI>();
        currencyText = GameObject.Find("CurrencyText")?.GetComponent<TextMeshProUGUI>();
        levelText = GameObject.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        pointsText = GameObject.Find("PointsText")?.GetComponent<TextMeshProUGUI>();
        xpBarFill = GameObject.Find("XPBarFill")?.GetComponent<Image>();

        ApplyDataToUI();
        UpdateXPBar();
    }

    #region Properties

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            if (usernameText != null) usernameText.text = _username;
            SaveData();
        }
    }

    public int Currency
    {
        get => _currency;
        set
        {
            _currency = value;
            if (currencyText != null) currencyText.text = _currency.ToString();
            SaveData();
        }
    }

    public int XP
    {
        get => _xp;
        set
        {
            _xp = value;
            CheckLevelUp();
            SaveData();
            UpdateXPBar();
        }
    }

    public int Level
    {
        get => _level;
        private set
        {
            _level = value;
            if (levelText != null) levelText.text = "Level: " + _level;
            SaveData();
        }
    }

    public int Points
    {
        get => _points;
        set
        {
            _points = value;
            if (pointsText != null) pointsText.text = _points.ToString();
            SaveData();
        }
    }

    public int Item1
    {
        get => _item1;
        set
        {
            _item1 = value;
            SaveData();
        }
    }

    public int Item2
    {
        get => _item2;
        set
        {
            _item2 = value;
            SaveData();
        }
    }

    public string SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            _selectedCharacter = value;
            SaveData();
        }
    }

    #endregion
    #region World Active

    // Track which world the player is currently in
    private bool _world1 = false;
    private bool _world2 = false;
    private bool _world3 = false;
    private bool _world4 = false;

    // Properties to access them (optional)
    public bool World1Active => _world1;
    public bool World2Active => _world2;
    public bool World3Active => _world3;
    public bool World4Active => _world4;

    #endregion
    public void SetActiveWorld(int worldNumber)
    {
        // Reset all worlds
        _world1 = false;
        _world2 = false;
        _world3 = false;
        _world4 = false;

        // Activate the selected world
        switch (worldNumber)
        {
            case 1: _world1 = true; break;
            case 2: _world2 = true; break;
            case 3: _world3 = true; break;
            case 4: _world4 = true; break;
        }

        SaveData(); // Save immediately so progress is remembered
    }

    #region Quiz Completion

    public void MarkMiniQuiz1Completed()
    {
        _miniQuiz1 = true;
        Debug.Log($"GameManager: MarkMiniQuiz1Completed called for user '{_username}'");
        SaveData();
    }

    public void MarkMiniQuiz2Completed()
    {
        _miniQuiz2 = true;
        Debug.Log($"GameManager: MarkMiniQuiz2Completed called for user '{_username}'");
        SaveData();
    }

    public void MarkMiniQuiz3Completed()
    {
        _miniQuiz3 = true;
        Debug.Log($"GameManager: MarkMiniQuiz3Completed called for user '{_username}'");
        SaveData();
    }

    public void MarkMainQuizCompleted()
    {
        _mainQuiz = true;
        Debug.Log($"GameManager: MarkMainQuizCompleted called for user '{_username}'");
        SaveData();
    }

    public void MarkMiniQuiz1bCompleted()
    {
        _miniQuiz1b = true;
        SaveData();
    }

    public void MarkMiniQuiz2bCompleted()
    {
        _miniQuiz2b = true;
        SaveData();
    }

    public void MarkMiniQuiz3bCompleted()
    {
        _miniQuiz3b = true;
        SaveData();
    }

    public void MarkMainQuizBCompleted()
    {
        _mainQuizB = true;
        SaveData();
    }

    public void MarkMiniQuiz1cCompleted()
    {
        _miniQuiz1c = true;
        SaveData();
    }

    public void MarkMiniQuiz2cCompleted()
    {
        _miniQuiz2c = true;
        SaveData();
    }

    public void MarkMiniQuiz3cCompleted()
    {
        _miniQuiz3c = true;
        SaveData();
    }
    public void MarkMainQuizCCompleted() { _mainQuizC = true; SaveData(); }

    public bool MiniQuiz1Completed => _miniQuiz1;
    public bool MiniQuiz2Completed => _miniQuiz2;
    public bool MiniQuiz3Completed => _miniQuiz3;
    public bool MainQuizCompleted => _mainQuiz;
    public bool MiniQuiz1bCompleted => _miniQuiz1b;
    public bool MiniQuiz2bCompleted => _miniQuiz2b;
    public bool MiniQuiz3bCompleted => _miniQuiz3b;
    public bool MainQuizBCompleted => _mainQuizB;
    public bool MiniQuiz1cCompleted => _miniQuiz1c;
    public bool MiniQuiz2cCompleted => _miniQuiz2c;
    public bool MiniQuiz3cCompleted => _miniQuiz3c;
    public bool MainQuizCCompleted => _mainQuizC;

    public bool HasDoneFirstWorld1ToWorld2Transition => _hasDoneFirstWorld1ToWorld2Transition;
    public bool HasDoneFirstWorld2ToWorld3Transition => _hasDoneFirstWorld2ToWorld3Transition;
    public bool HasShownAchievementW1 => _hasShownAchievementW1;
    public bool HasShownAchievementW2 => _hasShownAchievementW2;
    public bool HasShownAchievementW3 => _hasShownAchievementW3;
    public bool HasShownAnnouncementW1 => _hasShownAnnouncementW1;
    public bool HasShownAnnouncementW2 => _hasShownAnnouncementW2;
    public bool HasShownFinalAnnouncement => _hasShownFinalAnnouncement;

    public void MarkFirstWorld1ToWorld2TransitionDone() { _hasDoneFirstWorld1ToWorld2Transition = true; SaveData(); }
    public void MarkFirstWorld2ToWorld3TransitionDone() { _hasDoneFirstWorld2ToWorld3Transition = true; SaveData(); }
    public void MarkAchievementShown(int world)
    {
        switch (world)
        {
            case 1: _hasShownAchievementW1 = true; break;
            case 2: _hasShownAchievementW2 = true; break;
            case 3: _hasShownAchievementW3 = true; break;
        }
        SaveData();
    }

    public void MarkAnnouncementShown(int world)
    {
        switch (world)
        {
            case 1: _hasShownAnnouncementW1 = true; break;
            case 2: _hasShownAnnouncementW2 = true; break;
        }
        SaveData();
    }

    public void MarkFinalAnnouncementShown()
    {
        _hasShownFinalAnnouncement = true;
        SaveData();
    }
    #endregion

    #region Scene Loading

    public void LoadIntroScene() => SceneManager.LoadScene("Intro");
    public void LoadQuizScene() => SceneManager.LoadScene("Quiz");
    public void LoadScene1() => SceneManager.LoadScene("Scene1");
    public void LoadScene2() => SceneManager.LoadScene("Scene2");
    public void LoadScene3() => SceneManager.LoadScene("Scene3");
    public void LoadQuiz1scene() => SceneManager.LoadScene("Quiz1");
    public void LoadQuiz2scene() => SceneManager.LoadScene("Quiz2");
    public void Loadshop() => SceneManager.LoadScene("Shop");

    #endregion

    #region Profile Methods

    public void RefreshProfileData()
    {
        string path = GetUserSavePath();

        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found for " + _username);
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);

        _username = data.username;
        _password = data.password; // preserve password from JSON
        _currency = data.currency;
        _xp = data.xp;
        _level = data.level;
        _points = data.points;
        _selectedCharacter = data.hero;
        _uniqueId = data.uniqueId;
        SelectedOutfitIndex = int.TryParse(data.selectedOutfit.Replace("Outfit_", ""), out int index) ? index : 0;
        _item1 = data.item1;
        _item2 = data.item2;

        _purchasedVillager2 = data.purchasedVillager2;
        _purchasedVillager3 = data.purchasedVillager3;

        _miniQuiz1 = data.miniQuiz1Completed;
        _miniQuiz2 = data.miniQuiz2Completed;
        _miniQuiz3 = data.miniQuiz3Completed;
        _mainQuiz = data.mainQuizCompleted;
        _miniQuiz1b = data.miniQuiz1bCompleted;
        _miniQuiz2b = data.miniQuiz2bCompleted;
        _miniQuiz3b = data.miniQuiz3bCompleted;
        _mainQuizB = data.mainQuizBCompleted;
        _miniQuiz1c = data.miniQuiz1cCompleted;
        _miniQuiz2c = data.miniQuiz2cCompleted;
        _miniQuiz3c = data.miniQuiz3cCompleted;
        _mainQuizC = data.mainQuizCCompleted;

        ApplyDataToUI();
        UpdateXPBar();
        Debug.Log("Profile refreshed from file.");
    }

    public void ResetProfile()
    {
        _username = "";
        _currency = 0;
        _xp = 0;
        _level = 1;
        _points = 0;
        _selectedCharacter = "";
        SelectedOutfitIndex = 0;
        _item1 = 0;
        _item2 = 0;

        _miniQuiz1 = false;
        _miniQuiz2 = false;
        _miniQuiz3 = false;
        _mainQuiz = false;
        _miniQuiz1b = false;
        _miniQuiz2b = false;
        _miniQuiz3b = false;
        _mainQuizB = false;
        _miniQuiz1c = false;
        _miniQuiz2c = false;
        _miniQuiz3c = false;
        _mainQuizC = false;

        ApplyDataToUI();
        UpdateXPBar();
    }

    public void SetLevelDirectly(int level)
    {
        _level = level;
        if (levelText != null) levelText.text = "Level: " + _level;
        SaveData();
    }

    #endregion

    #region Data Management

    private string GetUserSavePath()
    {
        if (string.IsNullOrEmpty(_username) && PlayerPrefs.HasKey("ActiveUser"))
        {
            _username = PlayerPrefs.GetString("ActiveUser");
        }

        if (string.IsNullOrEmpty(_username))
        {
            Debug.LogWarning("GameManager has no active username; save path cannot be determined.");
            return null;
        }

        return Path.Combine(Application.persistentDataPath, _username + "_profile.json");
    }

    public void SaveData()
    {
        string path = GetUserSavePath();
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("SaveData skipped because there is no active username.");
            return;
        }

        ProfilePlayerData data = null;
        if (File.Exists(path))
        {
            string existingJson = File.ReadAllText(path);
            data = JsonUtility.FromJson<ProfilePlayerData>(existingJson);
        }

        if (data == null)
            data = new ProfilePlayerData();

        data.username = _username;
        data.uniqueId = _uniqueId;
        data.password = _password; // preserve password from JSON
        data.currency = _currency;
        data.xp = _xp;
        data.level = _level;
        data.points = _points;
        data.hero = _selectedCharacter;
        data.selectedOutfit = "Outfit_" + SelectedOutfitIndex;
        data.miniQuiz1Completed = _miniQuiz1;
        data.miniQuiz2Completed = _miniQuiz2;
        data.miniQuiz3Completed = _miniQuiz3;
        data.mainQuizCompleted = _mainQuiz;
        data.miniQuiz1bCompleted = _miniQuiz1b;
        data.miniQuiz2bCompleted = _miniQuiz2b;
        data.miniQuiz3bCompleted = _miniQuiz3b;
        data.mainQuizBCompleted = _mainQuizB;
        data.hasDoneFirstWorld1ToWorld2Transition = _hasDoneFirstWorld1ToWorld2Transition;
        data.hasDoneFirstWorld2ToWorld3Transition = _hasDoneFirstWorld2ToWorld3Transition;
        data.hasShownAchievementW1 = _hasShownAchievementW1;
        data.hasShownAchievementW2 = _hasShownAchievementW2;
        data.hasShownAchievementW3 = _hasShownAchievementW3;
        data.hasShownAnnouncementW1 = _hasShownAnnouncementW1;
        data.hasShownAnnouncementW2 = _hasShownAnnouncementW2;
        data.hasShownFinalAnnouncement = _hasShownFinalAnnouncement;
        data.miniQuiz1cCompleted = _miniQuiz1c;
        data.miniQuiz2cCompleted = _miniQuiz2c;
        data.miniQuiz3cCompleted = _miniQuiz3c;
        data.mainQuizCCompleted = _mainQuizC;
        data.world1Active = _world1;
        data.world2Active = _world2;
        data.world3Active = _world3;
        data.world4Active = _world4;
        data.purchasedVillager2 = _purchasedVillager2;
        data.purchasedVillager3 = _purchasedVillager3;
        data.item1 = _item1;
        data.item2 = _item2;

        string json = JsonUtility.ToJson(data, true);
        Debug.Log($"GameManager: Saving profile for '{_username}' to '{path}'");
        File.WriteAllText(path, json);
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("ActiveUser"))
            _username = PlayerPrefs.GetString("ActiveUser");

        if (string.IsNullOrEmpty(_username))
        {
            Debug.Log("No active user found in PlayerPrefs. Skipping GameManager profile load.");
            return;
        }

        string path = GetUserSavePath();
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("LoadData skipped because save path could not be determined.");
            return;
        }

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);

            _username = data.username;
            _password = data.password; // preserve password from JSON
            _currency = data.currency;
            _xp = data.xp;
            _level = data.level;
            _points = data.points;
            _selectedCharacter = data.hero;
            _uniqueId = data.uniqueId;
            SelectedOutfitIndex = int.TryParse(data.selectedOutfit.Replace("Outfit_", ""), out int index) ? index : 0;

            _miniQuiz1 = data.miniQuiz1Completed;
            _miniQuiz2 = data.miniQuiz2Completed;
            _miniQuiz3 = data.miniQuiz3Completed;
            _mainQuiz = data.mainQuizCompleted;
            _miniQuiz1b = data.miniQuiz1bCompleted;
            _miniQuiz2b = data.miniQuiz2bCompleted;
            _miniQuiz3b = data.miniQuiz3bCompleted;
            _mainQuizB = data.mainQuizBCompleted;
            _hasDoneFirstWorld1ToWorld2Transition = data.hasDoneFirstWorld1ToWorld2Transition;
            _hasDoneFirstWorld2ToWorld3Transition = data.hasDoneFirstWorld2ToWorld3Transition;
            _hasShownAchievementW1 = data.hasShownAchievementW1;
            _hasShownAchievementW2 = data.hasShownAchievementW2;
            _hasShownAchievementW3 = data.hasShownAchievementW3;
            _hasShownAnnouncementW1 = data.hasShownAnnouncementW1;
            _hasShownAnnouncementW2 = data.hasShownAnnouncementW2;
            _hasShownFinalAnnouncement = data.hasShownFinalAnnouncement;
            _miniQuiz1c = data.miniQuiz1cCompleted;
            _miniQuiz2c = data.miniQuiz2cCompleted;
            _miniQuiz3c = data.miniQuiz3cCompleted;
            _mainQuizC = data.mainQuizCCompleted;
            _world1 = data.world1Active;
            _world2 = data.world2Active;
            _world3 = data.world3Active;
            _world4 = data.world4Active;
            _item1 = data.item1;
            _item2 = data.item2;

            Scene currentScene = SceneManager.GetActiveScene();
            bool isIntroOrMenuScene = currentScene.name == "Intro" || currentScene.name == "MainMenu" || currentScene.name == "Title";

            if (!isIntroOrMenuScene)
            {
                if (_world1) SceneManager.LoadScene("NewScene1");
                else if (_world2) SceneManager.LoadScene("NewScene2");
                else if (_world3) SceneManager.LoadScene("NewScene3");
                else if (_world4) SceneManager.LoadScene("NewScene4");
            }
            else
            {
                Debug.Log("Loaded save: " + path + " - staying on current intro/menu scene.");
                ApplyDataToUI();
            }
        }
        else
        {
            SaveData();
            Debug.Log("Created new save for: " + _username);
            ApplyDataToUI();
        }
    }

    #endregion

    #region XP / Level

    private int GetXPForNextLevel(int level) => level * 10;

    private void CheckLevelUp()
    {
        bool leveledUp = false;
        while (_xp >= GetXPForNextLevel(_level))
        {
            _xp -= GetXPForNextLevel(_level);
            Level++;
            leveledUp = true;
        }

        UpdateXPBar();

        if (leveledUp) Debug.Log("Level Up! Now Level: " + _level);
    }

    public void UpdateXPBar()
    {
        if (xpBarFill != null)
        {
            float fillAmount = (_xp > 0) ? (float)_xp / GetXPForNextLevel(_level) : 0f;
            xpBarFill.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }

    public void AddXP(int amount) { XP += amount; }

    #endregion

    #region Currency Spending

    /// <summary>
    /// Deducts currency if the player has enough. Returns true if successful, false if not enough currency.
    /// </summary>
    public bool SpendCurrency(int amount)
    {
        if (_currency >= amount)
        {
            Currency -= amount; // Will automatically update UI and save
            Debug.Log($"Spent {amount} currency. Remaining: {_currency}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough currency! You have {_currency}, need {amount}");
            return false;
        }
    }

    #endregion

    #region Helper Methods

    public void AddCurrency(int amount) { Currency += amount; }
    public void AddPoints(int amount) { Points += amount; }
    public void SetUsername(string name) { Username = name; }

    public void ApplyDataToUI()
    {
        if (usernameText != null) usernameText.text = _username;
        if (currencyText != null) currencyText.text = _currency.ToString();
        if (levelText != null) levelText.text = "Level: " + _level;
        if (pointsText != null) pointsText.text = _points.ToString();
    }

    #endregion
}
