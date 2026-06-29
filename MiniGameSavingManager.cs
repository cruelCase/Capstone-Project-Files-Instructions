using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MiniGameSavingManager : MonoBehaviour
{
    public static MiniGameSavingManager Instance;

    [Header("UI References")]
    public GameObject startPanel;
    public Button startButton;
    public TextMeshProUGUI startInstructionText;
    public TextMeshProUGUI countdownText;

    public GameObject quizPanel;
    public TextMeshProUGUI budgetText;
    public ScorePanel scorePanel;
    public TextMeshProUGUI pointsText;     // Points earned display
    public Button retryButton;

    [Header("Points Animation")]
    public Color pointsStartColor = Color.yellow;
    public Color pointsEndColor = Color.white;
    public float pointsAnimationDuration = 1.5f;
    public bool pointsPulseLoop = true;

    [Header("Hero & Item UI")]
    public GameObject[] heroPrefabs;
    public string[] heroNames;
    public Transform heroSpawnPoint;
    public GameObject item1PrefabDisplay;
    public GameObject item2PrefabDisplay;
    public Transform item1CountTextTransform;
    public Transform item2CountTextTransform;
    public TextMeshProUGUI performanceText;
    public Transform item1ButtonTransform;
    public Transform item2ButtonTransform;
    public Button item1Button;
    public Button item2Button;

    [Header("Fruit Ninja Style Settings")]
    public GameObject[] sourcePrefabs; // e.g. Sahod, Allowance, Negosyo, Freelance work
    public GameObject[] avoidPrefabs;  // e.g. Scam, Utang, Bayarin sa kuryente, Gastos sa pagpapagamot
    public Transform spawnParent;
    public GameObject swipePrefab;
    public float swipeDuration = 0.5f;
    public float spawnInterval = 0.8f;
    public int totalSpawnedItems = 18;
    public float itemSpeed = 200f;
    public float itemLifetime = 3f;
    public int maxMistakes = 3;
    public int scorePerSource = 10;
    public int scorePenalty = 5;

    public Transform livesIndicatorTransform;
    public TextMeshProUGUI timerText;
    public float gameTimeSeconds = 45f;

    private int currentScore = 0;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation
    private int mistakes = 0;
    private int currentLives = 0;
    private float timeRemaining = 0f;
    private bool timerRunning = false;
    private bool timerPaused = false;
    private int scoreMultiplier = 1;
    private int movesCount = 0;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;
    private bool gameStarted = false;
    private int item1Count = 0;
    private int item2Count = 0;
    private TextMeshProUGUI item1CountTextUI;
    private TextMeshProUGUI item2CountTextUI;
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private Coroutine typingCoroutine;
    private Coroutine spawnCoroutine;
    private List<GameObject> activeItems = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryMiniGame);

        if (item1ButtonTransform != null)
            item1Button = item1ButtonTransform.GetComponent<Button>();
        if (item2ButtonTransform != null)
            item2Button = item2ButtonTransform.GetComponent<Button>();

        if (item1Button != null)
            item1Button.onClick.AddListener(OnItem1Pressed);
        if (item2Button != null)
            item2Button.onClick.AddListener(OnItem2Pressed);

        if (item1CountTextTransform != null)
            item1CountTextUI = item1CountTextTransform.GetComponent<TextMeshProUGUI>();
        if (item2CountTextTransform != null)
            item2CountTextUI = item2CountTextTransform.GetComponent<TextMeshProUGUI>();

        LoadActiveUserItems();
        ApplyHeroPrefabFromActiveUser();
        UpdateItemButtonStates();
        UpdateItemCountText();

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (livesIndicatorTransform != null)
            livesIndicatorTransform.gameObject.SetActive(false);
    }

    public void StartMiniGame()
    {
        if (gameStarted) return;

        ShowStartScreen();
    }

    public void ShowStartScreen()
    {
        if (startPanel != null)
            startPanel.SetActive(true);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (scorePanel != null)
            scorePanel.gameObject.SetActive(false);

        if (startButton != null)
            startButton.gameObject.SetActive(true);

        if (startInstructionText != null)
        {
            startInstructionText.gameObject.SetActive(true);
            StartTyping(startInstructionText,
                "Handa ka na bang piliin ang tunay na pinagkukunan ng kita? May 3 buhay ka — i-slice ang tamang sources at iwasaan ang hindi kita sources.",
                0.01f);
        }
    }

    void OnStartButtonClicked()
    {
        if (startInstructionText != null)
            startInstructionText.gameObject.SetActive(false);

        if (startButton != null)
            startButton.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        StartCoroutine(StartCountdownThenBegin());
    }

    IEnumerator StartCountdownThenBegin()
    {
        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(false);

        timeRemaining = gameTimeSeconds;
        timerRunning = true;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = FormatTime(timeRemaining);
        }

        BeginMiniGame();
    }

    void BeginMiniGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        currentScore = 0;
        mistakes = 0;
        currentLives = maxMistakes;
        movesCount = 0;
        activeItems.Clear();

        quizPanel.SetActive(true);
        if (livesIndicatorTransform != null)
            livesIndicatorTransform.gameObject.SetActive(true);

        UpdateScoreText();

        if (spawnParent == null && quizPanel != null)
            spawnParent = quizPanel.transform;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnItems());
    }

    IEnumerator SpawnItems()
    {
        int spawnedCount = 0;
        while (spawnedCount < totalSpawnedItems && gameStarted && currentLives > 0)
        {
            SpawnRandomItem();
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }

        while (gameStarted && activeItems.Count > 0 && currentLives > 0)
            yield return null;

        EndMiniGame();
    }

    void SpawnRandomItem()
    {
        GameObject prefab = null;
        bool isSource = Random.value > 0.35f;

        if (isSource && sourcePrefabs.Length > 0)
            prefab = sourcePrefabs[Random.Range(0, sourcePrefabs.Length)];
        else if (!isSource && avoidPrefabs.Length > 0)
            prefab = avoidPrefabs[Random.Range(0, avoidPrefabs.Length)];
        else if (sourcePrefabs.Length > 0)
        {
            prefab = sourcePrefabs[Random.Range(0, sourcePrefabs.Length)];
            isSource = true;
        }

        if (prefab == null || spawnParent == null)
            return;

        GameObject item = Instantiate(prefab, spawnParent);
        SavingSliceItem sliceItem = item.GetComponent<SavingSliceItem>();
        if (sliceItem == null)
            sliceItem = item.AddComponent<SavingSliceItem>();

        sliceItem.manager = this;
        sliceItem.isSource = isSource;

        RectTransform parentRect = spawnParent as RectTransform;
        RectTransform itemRect = item.GetComponent<RectTransform>();
        if (parentRect != null && itemRect != null)
        {
            float halfWidth = parentRect.rect.width * 0.5f;
            float x = Random.Range(-halfWidth + 50f, halfWidth - 50f);
            float y = parentRect.rect.height * 0.45f;
            itemRect.anchoredPosition = new Vector2(x, y);
        }
        else
        {
            item.transform.position = spawnParent.position + Vector3.up * 200f + Vector3.right * Random.Range(-200f, 200f);
        }

        activeItems.Add(item);
        StartCoroutine(DestroyAfterLifetime(item, itemLifetime));
    }

    void Update()
    {
        if (timerRunning)
        {
            if (!timerPaused)
                timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                timerRunning = false;
                EndMiniGame();
                return;
            }

            if (timerText != null)
                timerText.text = FormatTime(timeRemaining);
        }

        if (!gameStarted) return;

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            GameObject item = activeItems[i];
            if (item == null)
            {
                activeItems.RemoveAt(i);
                continue;
            }

            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.anchoredPosition += Vector2.down * itemSpeed * Time.deltaTime;
            }
            else
            {
                item.transform.Translate(Vector3.down * itemSpeed * Time.deltaTime);
            }
        }
    }

    private IEnumerator DestroyAfterLifetime(GameObject item, float lifetime)
    {
        yield return new WaitForSecondsRealtime(lifetime);
        if (item == null) yield break;
        if (activeItems.Contains(item))
        {
            activeItems.Remove(item);
            Destroy(item);
        }
    }

    public void ItemSliced(SavingSliceItem slicedItem)
    {
        if (!gameStarted || slicedItem == null) return;

        ShowSwipeEffect(slicedItem.transform);
        movesCount++;

        if (slicedItem.isSource)
        {
            currentScore += scorePerSource * scoreMultiplier;
            
            // Show encouraging dialogue every 2 correct moves
            if (movesCount % 2 == 0)
            {
                string[] encouragements = new string[]
                {
                    "Maganda! Kumita ng pera!",
                    "Tama! Pera na pera!",
                    "Napakaganda! Tayo na!",
                    "Tuluy-tuloy! Mas maraming kita!",
                    "Sobrang galing mo!",
                    "Ayan na! Tuloy-tuloy lang!"
                };
                int randomIndex = UnityEngine.Random.Range(0, encouragements.Length);
                StartTyping(performanceText, encouragements[randomIndex], 0.01f);
            }
        }
        else
        {
            currentScore -= scorePenalty;
            mistakes++;
            currentLives = Mathf.Max(0, currentLives - 1);
            UpdateLivesIndicator();
            
            // Show warning dialogue on mistakes
            string[] warnings = new string[]
            {
                "Hindi yan! Magsuri nang mabuti.",
                "Oops! Hindi yan kita!",
                "Huwag yan, bantayan mo!",
                "Malakas mo pero bantayan mo!"
            };
            int warningIndex = UnityEngine.Random.Range(0, warnings.Length);
            StartTyping(performanceText, warnings[warningIndex], 0.01f);
        }

        currentScore = Mathf.Max(0, currentScore);
        UpdateScoreText();

        if (currentLives <= 0)
        {
            EndMiniGame();
            return;
        }

        activeItems.Remove(slicedItem.gameObject);
        Destroy(slicedItem.gameObject);
    }

    void ShowSwipeEffect(Transform itemTransform)
    {
        if (swipePrefab == null || spawnParent == null) return;

        GameObject effect = Instantiate(swipePrefab, spawnParent);
        RectTransform effectRect = effect.GetComponent<RectTransform>();
        RectTransform itemRect = itemTransform as RectTransform;

        if (effectRect != null && itemRect != null)
            effectRect.anchoredPosition = itemRect.anchoredPosition;
        else if (itemTransform != null)
            effect.transform.position = itemTransform.position;

        Destroy(effect, swipeDuration);
    }

    void UpdateScoreText()
    {
        if (budgetText != null)
            budgetText.text = $"Score: {currentScore}   Lives: {currentLives}/{maxMistakes}";
    }

    private IEnumerator PauseTimerForSeconds(float seconds)
    {
        timerPaused = true;
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        timerPaused = false;
        item1PauseCoroutine = null;
    }

    private IEnumerator ActivateItem2Bonus(float seconds)
    {
        scoreMultiplier = 2;
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        scoreMultiplier = 1;
        item2BonusCoroutine = null;
    }

    private void LoadActiveUserItems()
    {
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile != null)
        {
            item1Count = profile.item1;
            item2Count = profile.item2;
        }
        UpdateItemCountText();
    }

    private ProfilePlayerData LoadActiveUserProfile()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
            return null;

        string path = System.IO.Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!System.IO.File.Exists(path))
            return null;

        return JsonUtility.FromJson<ProfilePlayerData>(System.IO.File.ReadAllText(path));
    }

    private void SaveActiveUserProfile(ProfilePlayerData profile)
    {
        if (profile == null)
            return;

        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
            return;

        string path = System.IO.Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(profile, true));
    }

    private void ApplyHeroPrefabFromActiveUser()
    {
        string heroName = null;
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile != null)
            heroName = profile.hero;
        else if (GameManager.Instance != null)
            heroName = GameManager.Instance.SelectedCharacter;

        if (string.IsNullOrEmpty(heroName))
            return;

        if (activeHeroInstance != null)
        {
            Destroy(activeHeroInstance);
            activeHeroInstance = null;
        }

        int heroIndex = FindHeroIndexByName(heroName);
        if (heroIndex < 0 || heroIndex >= heroPrefabs.Length)
            return;

        Transform targetParent = heroSpawnPoint != null ? heroSpawnPoint : (quizPanel != null ? quizPanel.transform : null);
        if (targetParent != null)
            activeHeroInstance = Instantiate(heroPrefabs[heroIndex], targetParent);
        else
            activeHeroInstance = Instantiate(heroPrefabs[heroIndex]);

        if (activeHeroInstance != null)
        {
            activeHeroInstance.transform.localPosition = Vector3.zero;
            activeHeroInstance.transform.localRotation = Quaternion.identity;
            activeHeroInstance.transform.localScale = Vector3.one;
        }
    }

    private int FindHeroIndexByName(string heroName)
    {
        if (string.IsNullOrEmpty(heroName) || heroNames == null)
            return -1;

        for (int i = 0; i < heroNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(heroNames[i]) && string.Equals(heroNames[i].Trim(), heroName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private void UpdateItemButtonStates()
    {
        if (item1Button != null)
            item1Button.interactable = item1Count > 0;
        if (item2Button != null)
            item2Button.interactable = item2Count > 0;
    }

    private void UpdateItemCountText()
    {
        if (item1CountTextUI != null)
            item1CountTextUI.text = item1Count.ToString();
        if (item2CountTextUI != null)
            item2CountTextUI.text = item2Count.ToString();
    }

    private void DeductItemCount(int itemId)
    {
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile == null)
            return;

        if (itemId == 1 && item1Count > 0)
        {
            item1Count--;
            profile.item1 = item1Count;
        }
        else if (itemId == 2 && item2Count > 0)
        {
            item2Count--;
            profile.item2 = item2Count;
        }
        else
        {
            return;
        }

        SaveActiveUserProfile(profile);
        UpdateItemButtonStates();
        UpdateItemCountText();
    }

    public void OnItem1Pressed()
    {
        if (!gameStarted || item1Count <= 0)
            return;

        DeductItemCount(1);
        if (item1PauseCoroutine != null)
            StopCoroutine(item1PauseCoroutine);
        item1PauseCoroutine = StartCoroutine(PauseTimerForSeconds(5f));
        StartTyping(performanceText, "Ginamit ang Item1! Hinto muna ang oras ng 5 segundo.", 0.01f);
        ShowPrefabDisplay(item1PrefabDisplay, 1f);
    }

    public void OnItem2Pressed()
    {
        if (!gameStarted || item2Count <= 0)
            return;

        DeductItemCount(2);
        if (item2BonusCoroutine != null)
            StopCoroutine(item2BonusCoroutine);
        item2BonusCoroutine = StartCoroutine(ActivateItem2Bonus(10f));
        StartTyping(performanceText, "Item2 activated! Dobleng puntos sa susunod na tama sa loob ng 10 segundo.", 0.01f);
        ShowPrefabDisplay(item2PrefabDisplay, 1f);
    }

    private void ShowPrefabDisplay(GameObject prefabDisplay, float duration)
    {
        if (prefabDisplay == null)
            return;

        if (currentPrefabDisplayInstance != null)
        {
            Destroy(currentPrefabDisplayInstance);
            currentPrefabDisplayInstance = null;
        }

        Transform parent = quizPanel != null ? quizPanel.transform : null;
        if (parent != null)
            currentPrefabDisplayInstance = Instantiate(prefabDisplay, parent);
        else
            currentPrefabDisplayInstance = Instantiate(prefabDisplay);

        if (currentPrefabDisplayInstance != null)
        {
            currentPrefabDisplayInstance.transform.localPosition = Vector3.zero;
            currentPrefabDisplayInstance.transform.localRotation = Quaternion.identity;
            currentPrefabDisplayInstance.transform.localScale = Vector3.one;
        }

        StartCoroutine(HidePrefabAfterDelay(currentPrefabDisplayInstance, duration));
    }

    private IEnumerator HidePrefabAfterDelay(GameObject prefabInstance, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (prefabInstance != null)
            Destroy(prefabInstance);
        if (prefabInstance == currentPrefabDisplayInstance)
            currentPrefabDisplayInstance = null;
    }

    private void StartTyping(TextMeshProUGUI target, string content, float charDelay)
    {
        if (target == null)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeTextCoroutine(target, content, charDelay));
    }

    private IEnumerator TypeTextCoroutine(TextMeshProUGUI target, string content, float charDelay)
    {
        target.text = "";
        for (int i = 0; i < content.Length; i++)
        {
            target.text += content[i];
            yield return new WaitForSecondsRealtime(charDelay);
        }

        typingCoroutine = null;
    }

    void UpdateLivesIndicator()
    {
        if (livesIndicatorTransform == null)
            return;

        int childCount = livesIndicatorTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject child = livesIndicatorTransform.GetChild(i).gameObject;
            child.SetActive(i < currentLives);
        }
    }

    string FormatTime(float seconds)
    {
        int wholeSeconds = Mathf.CeilToInt(seconds);
        return wholeSeconds.ToString("00");
    }

    void EndMiniGame()
    {
        if (!gameStarted) return;
        gameStarted = false;
        timerRunning = false;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        foreach (var item in activeItems)
            if (item != null)
                Destroy(item);

        activeItems.Clear();
        quizPanel.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (livesIndicatorTransform != null)
            livesIndicatorTransform.gameObject.SetActive(false);

        // Fail only when all lives are used AND points are below 100
        bool passed = !(currentLives == 0 && currentScore < 100);
        string comment = passed
            ? "Magaling! Nakita mo ang pinagkukunan ng kita at naiiwasan ang maling gastos."
            : "Subukan ulit: i-slice lang ang tunay na kita at iwasan ang mga maling item.";

        if (retryButton != null)
            retryButton.gameObject.SetActive(!passed);

        UpdateItemButtonStates();
        scorePanel.ShowScore($"{currentScore} pts", comment);

        if (passed)
        {
            GameManager.Instance.MarkMiniQuiz2bCompleted();
            TaskManager.CheckMiniGamesCompletion();
        }

        pointsReward = passed ? 80 : 20;
        
        // Display points earned
        if (pointsText != null)
        {
            pointsText.text = $"Pontus Nakuha: +{pointsReward}";
            
            // Stop previous animation if running
            if (pointsColorCoroutine != null)
                StopCoroutine(pointsColorCoroutine);
            
            // Start color animation
            pointsColorCoroutine = StartCoroutine(AnimatePointsTextColor());
        }

        GameManager.Instance.AddPoints(pointsReward);
        GameManager.Instance.AddXP(passed ? 10 : 4);
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();
            ProfileManager.Instance.UpdateProgressBars();
        }
    }

    private IEnumerator AnimatePointsTextColor()
    {
        if (pointsText == null)
            yield break;

        float elapsedTime = 0f;

        while (elapsedTime < pointsAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / pointsAnimationDuration;

            // Lerp between start and end color
            Color lerpedColor = Color.Lerp(pointsStartColor, pointsEndColor, t);
            pointsText.color = lerpedColor;

            yield return null;
        }

        // Ensure final color is set
        pointsText.color = pointsEndColor;

        // Loop animation if enabled
        if (pointsPulseLoop && scorePanel.gameObject.activeSelf)
        {
            pointsColorCoroutine = StartCoroutine(AnimatePointsTextColor());
        }
    }

    public void RetryMiniGame()
    {
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (scorePanel != null)
            scorePanel.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (livesIndicatorTransform != null)
            livesIndicatorTransform.gameObject.SetActive(false);

        ShowStartScreen();
    }
}

public class SavingSliceItem : MonoBehaviour, IPointerClickHandler
{
    public MiniGameSavingManager manager;
    public bool isSource;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
            manager.ItemSliced(this);
    }
}
