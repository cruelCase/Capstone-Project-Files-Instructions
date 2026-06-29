using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MiniQuizManager1 : MonoBehaviour
{
    public static MiniQuizManager1 Instance;

    [Header("UI Elements")]
    public GameObject quizPanel;
    public TextMeshProUGUI instructionsText; // Lesson instruction display
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
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

    [Header("Start Screen")]
    public Transform startScreenPanel;
    public Transform startButtonTransform;
    public Transform continueButtonTransform;
    public TextMeshProUGUI countdownText;
    public string[] startInstructionsPages = new string[] {
        "I-tap ang Start upang simulan ang misyon.",
        "Isaayos ang bawat bagay sa kategoryang Pangangailangan o Kagustuhan sa tamang lugar."
    };
    public string startInstructionsText = "I-tap ang Start upang simulan ang misyon. Isaayos ang bawat bagay sa kategoryang Pangangailangan o Kagustuhan sa tamang lugar.";
    public float instructionsTypingSpeed = 0.04f;

    [Header("Score Panel")]
    public GameObject scorePanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI pointsText;     // Points earned display
    public TextMeshProUGUI lessonText;
    public Button exitButton;
    public Button retryButton;

    [Header("Points Animation")]
    public Color pointsStartColor = Color.yellow;
    public Color pointsEndColor = Color.white;
    public float pointsAnimationDuration = 1.5f;
    public bool pointsPulseLoop = true;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Items")]
    public GameObject[] needItems;  // Items representing pangangailangan (do NOT shoot)
    public GameObject[] wantItems;  // Items representing kagustuhan (shoot these)

    [Header("Tap Effects")]
    public GameObject scopePrefab; // prefab (circle + plus) to show where player tapped
    public AudioClip shootSfx;
    public AudioSource audioSource;
    public float scopeDuration = 0.5f;

    [Header("Ammo")]
    public Transform[] bulletIndicators;
    public Transform reloadingTextTransform;
    public float reloadTime = 2f;
    public Color bulletUnusedColor = Color.white;
    public Color bulletUsedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Quiz Settings")]
    public float spawnInterval = 1f;
    public float itemLifetime = 3f;
    public float quizDuration = 30f;
    public float instructionsDuration = 5f; // show lesson first

    private bool difficultyIncreased = false;

    private Dictionary<GameObject, Transform> spawnPointMap = new Dictionary<GameObject, Transform>();

    private int score = 0;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation
    private float timer;
    private bool quizRunning = false;
    private bool timerPaused = false;
    private int scoreMultiplier = 1;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;

    private int maxBullets = 5;
    private int bulletsRemaining;
    private bool isReloading = false;

    private TextMeshProUGUI countdownTextUI;
    private Button startButton;
    private Button continueButton;
    private int currentInstructionPage = 0;
    private Coroutine typingCoroutine;
    private Coroutine countdownCoroutine;
    private TextMeshProUGUI item1CountTextUI;
    private TextMeshProUGUI item2CountTextUI;
    private int item1Count = 0;
    private int item2Count = 0;
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;

    private List<Transform> availableSpawnPoints = new List<Transform>();
    private List<GameObject> activeSpawnedItems = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        quizPanel.SetActive(false);
        scorePanel.SetActive(false);
        exitButton.onClick.AddListener(CloseScorePanel);

        if (reloadingTextTransform != null)
            reloadingTextTransform.gameObject.SetActive(false);

        if (startButtonTransform != null)
            startButton = startButtonTransform.GetComponent<Button>();

        if (continueButtonTransform != null)
            continueButton = continueButtonTransform.GetComponent<Button>();

        if (countdownText != null)
            countdownTextUI = countdownText;

        if (item1CountTextTransform != null)
            item1CountTextUI = item1CountTextTransform.GetComponent<TextMeshProUGUI>();
        if (item2CountTextTransform != null)
            item2CountTextUI = item2CountTextTransform.GetComponent<TextMeshProUGUI>();

        if (item1ButtonTransform != null)
            item1Button = item1ButtonTransform.GetComponent<Button>();
        if (item2ButtonTransform != null)
            item2Button = item2ButtonTransform.GetComponent<Button>();

        if (item1Button != null)
            item1Button.onClick.AddListener(OnItem1Pressed);
        if (item2Button != null)
            item2Button.onClick.AddListener(OnItem2Pressed);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryQuiz);
            retryButton.gameObject.SetActive(false);
        }

        LoadActiveUserItems();
        ApplyHeroPrefabFromActiveUser();
        UpdateItemButtonStates();
        UpdateItemCountText();

        if (spawnPoints != null && spawnPoints.Length > 0)
            availableSpawnPoints = new List<Transform>(spawnPoints);

        HideStartUI();

        bulletsRemaining = maxBullets;
        UpdateBulletIndicators();
    }

    public void ShowStartUI()
    {
        currentInstructionPage = 0;

        if (startScreenPanel != null)
            startScreenPanel.gameObject.SetActive(true);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        if (scorePanel != null)
            scorePanel.SetActive(false);

        if (instructionsText != null)
            instructionsText.gameObject.SetActive(true);

        if (startButtonTransform != null)
            startButtonTransform.gameObject.SetActive(false);

        if (continueButtonTransform != null)
            continueButtonTransform.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        StartTypingInstructions(currentInstructionPage);
    }

    public void HideStartUI()
    {
        if (startScreenPanel != null)
            startScreenPanel.gameObject.SetActive(false);

        if (startButtonTransform != null)
            startButtonTransform.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
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

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
            return null;

        return JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
    }

    private void SaveActiveUserProfile(ProfilePlayerData profile)
    {
        if (profile == null)
            return;

        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
            return;

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
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
        if (item1Count <= 0 || !quizRunning)
            return;

        DeductItemCount(1);
        if (item1PauseCoroutine != null)
            StopCoroutine(item1PauseCoroutine);
        item1PauseCoroutine = StartCoroutine(PauseTimerForSeconds(5f));
        StartTyping(performanceText, "Ginamit ang Item1! Huminto muna ang oras sa loob ng 5 segundo.", 0.01f);
        ShowPrefabDisplay(item1PrefabDisplay, 1f);
    }

    public void OnItem2Pressed()
    {
        if (item2Count <= 0 || !quizRunning)
            return;

        DeductItemCount(2);
        if (item2BonusCoroutine != null)
            StopCoroutine(item2BonusCoroutine);
        item2BonusCoroutine = StartCoroutine(ActivateItem2Bonus(10f));
        StartTyping(performanceText, "Item2 na-activate! Doble puntos sa susunod na tama sa loob ng 10 segundo.", 0.01f);
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
        if (currentPrefabDisplayInstance == prefabInstance)
            currentPrefabDisplayInstance = null;
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

    void StartTypingInstructions(int pageIndex)
    {
        if (instructionsText == null)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        string text = GetInstructionPageText(pageIndex);
        typingCoroutine = StartCoroutine(TypeInstructionsText(text));
    }

    string GetInstructionPageText(int pageIndex)
    {
        if (startInstructionsPages != null && startInstructionsPages.Length > 0)
        {
            int index = Mathf.Clamp(pageIndex, 0, startInstructionsPages.Length - 1);
            return startInstructionsPages[index];
        }

        return startInstructionsText;
    }

    IEnumerator TypeInstructionsText(string text)
    {
        if (instructionsText == null)
            yield break;

        instructionsText.text = "";

        foreach (char c in text)
        {
            instructionsText.text += c;
            yield return new WaitForSecondsRealtime(instructionsTypingSpeed);
        }

        typingCoroutine = null;
        OnInstructionPageComplete();
    }

    public void OnStartButtonClicked()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (startButton != null)
            startButton.interactable = false;

        if (continueButtonTransform != null)
            continueButtonTransform.gameObject.SetActive(false);

        if (startButtonTransform != null)
            startButtonTransform.gameObject.SetActive(false);

        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        if (countdownText != null)
        {
            countdownText.text = "3";
            countdownText.gameObject.SetActive(true);
        }

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(StartCountdownThenBegin());
    }

    public void OnContinueButtonClicked()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentInstructionPage++;

        if (currentInstructionPage >= GetInstructionPageCount())
            currentInstructionPage = GetInstructionPageCount() - 1;

        if (continueButtonTransform != null)
            continueButtonTransform.gameObject.SetActive(false);

        if (startButtonTransform != null)
            startButtonTransform.gameObject.SetActive(false);

        StartTypingInstructions(currentInstructionPage);
    }

    int GetInstructionPageCount()
    {
        if (startInstructionsPages != null && startInstructionsPages.Length > 0)
            return startInstructionsPages.Length;
        return 1;
    }

    void OnInstructionPageComplete()
    {
        bool isLastPage = currentInstructionPage >= GetInstructionPageCount() - 1;

        if (isLastPage)
        {
            if (startButtonTransform != null)
                startButtonTransform.gameObject.SetActive(true);

            if (startButton != null)
                startButton.interactable = true;

            if (continueButtonTransform != null)
                continueButtonTransform.gameObject.SetActive(false);
        }
        else
        {
            if (continueButtonTransform != null)
                continueButtonTransform.gameObject.SetActive(true);

            if (startButtonTransform != null)
                startButtonTransform.gameObject.SetActive(false);

            if (continueButton != null)
                continueButton.interactable = true;
        }
    }

    IEnumerator StartCountdownThenBegin()
    {
        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
            yield return new WaitForSecondsRealtime(0.8f);
            countdownText.text = "2";
            yield return new WaitForSecondsRealtime(0.8f);
            countdownText.text = "1";
            yield return new WaitForSecondsRealtime(0.8f);
        }

        HideStartUI();
        countdownCoroutine = null;
        StartQuiz();
    }

    public void StartQuiz()
    {
        quizPanel.SetActive(true);
        scorePanel.SetActive(false);
        score = 0;
        quizRunning = false;
        isReloading = false;
        bulletsRemaining = maxBullets;
        UpdateBulletIndicators();
        if (reloadingTextTransform != null)
            reloadingTextTransform.gameObject.SetActive(false);

        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);

        timer = quizDuration;
        quizRunning = true;

        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        UpdateScoreText();
        UpdateTimerText();

        StartCoroutine(SpawnLoop());
        StartCoroutine(TimerLoop());
    }

    private IEnumerator PreQuizInstructions()
    {
        yield return new WaitForSeconds(instructionsDuration);

        // Start actual gameplay
        instructionsText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);

        timer = quizDuration;
        quizRunning = true;

        UpdateScoreText();
        UpdateTimerText();

        StartCoroutine(SpawnLoop());
        StartCoroutine(TimerLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (quizRunning)
        {
            SpawnRandomItem();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomItem()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (availableSpawnPoints == null || availableSpawnPoints.Count == 0) return;

        int index = Random.Range(0, availableSpawnPoints.Count);
        Transform spawnPoint = availableSpawnPoints[index];
        availableSpawnPoints.RemoveAt(index);

        bool spawnNeed = Random.value > 0.5f;
        GameObject prefab = spawnNeed ?
            needItems[Random.Range(0, needItems.Length)] :
            wantItems[Random.Range(0, wantItems.Length)];

        GameObject spawned = Instantiate(prefab, spawnPoint.position, Quaternion.identity, quizPanel.transform);
        // ensure spawned item renders above background panel
        spawned.transform.SetAsLastSibling();
        activeSpawnedItems.Add(spawned);
        spawnPointMap[spawned] = spawnPoint;
        StartCoroutine(DestroyAndFree(spawned, spawnPoint));
    }

    IEnumerator DestroyAndFree(GameObject obj, Transform point)
    {
        yield return new WaitForSeconds(itemLifetime);
        if (obj != null)
        {
            Destroy(obj);
            activeSpawnedItems.Remove(obj);
            ReleaseSpawnPoint(obj);
        }
        else
        {
            // If object is already destroyed (clicked early), ensure point is freed
            if (point != null && !availableSpawnPoints.Contains(point))
                availableSpawnPoints.Add(point);
        }
    }

    private void ReleaseSpawnPoint(GameObject obj)
    {
        if (obj == null)
            return;

        if (spawnPointMap.TryGetValue(obj, out Transform point))
        {
            if (point != null && !availableSpawnPoints.Contains(point))
                availableSpawnPoints.Add(point);

            spawnPointMap.Remove(obj);
        }
    }

    IEnumerator TimerLoop()
    {
        while (quizRunning && timer > 0)
        {
            if (!timerPaused)
                timer -= Time.deltaTime;
            UpdateTimerText();
            yield return null;
        }
        EndQuiz();
    }

    void CheckDifficultyIncrease()
    {
        if (!difficultyIncreased && score >= 5)
        {
            difficultyIncreased = true;

            itemLifetime = 2f;

            Debug.Log("Difficulty increased: Shorter item lifetime");
        }
    }

    void CheckHeaderSwitch()
    {
        // Score >= 3 → Girl ON
    }




    void UpdateTimerText()
    {
        timerText.text = $"Oras: {Mathf.CeilToInt(timer)}";
    }

    void UpdateScoreText()
    {
        scoreText.text = $"Puntos: {score}";
        UpdatePerformanceText();
    }

    void UpdatePerformanceText()
    {
        if (performanceText == null)
            return;

        string message = "";

        if (score >= 20)
        {
            message = "Napakagaling mo! Parang propesyonal ka na!";
        }
        else if (score >= 15)
        {
            message = "Sobrang ganda ng laro mo! Patuloy lang!";
        }
        else if (score >= 10)
        {
            message = "Maganda! Lumampas ka na sa target!";
        }
        else if (score >= 5)
        {
            message = "Maganda ang simula! Kaya mo pa!";
        }
        else if (score > 0)
        {
            message = "May puntos ka na! Patuloy at mag-improve!";
        }
        else
        {
            message = "Simula pa lang! Tara na, makakuha ng puntos!";
        }

        performanceText.text = message;
    }

    public void ItemClicked(ItemObject item)
    {
        if (!quizRunning) return;
        if (isReloading) return;

        if (bulletsRemaining <= 0)
        {
            StartCoroutine(ReloadCoroutine());
            return;
        }

        bulletsRemaining--;
        UpdateBulletIndicators();
        ShowScopeAt(item.transform.position);
        PlayShootSound();

        if (item.isNeed)
        {
            Debug.Log("You shot a NEED! This reduces your score.");
            score = Mathf.Max(0, score - 1);
        }
        else
        {
            Debug.Log("Correct! You shot a WANT!");
            score += scoreMultiplier;
        }

        UpdateScoreText();
        CheckDifficultyIncrease();
        CheckHeaderSwitch();
        ReleaseSpawnPoint(item.gameObject);
        Destroy(item.gameObject);
        activeSpawnedItems.Remove(item.gameObject);

        if (bulletsRemaining <= 0)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    void ShowScopeAt(Vector3 worldPos)
    {
        if (scopePrefab == null || quizPanel == null)
            return;

        GameObject scope = Instantiate(scopePrefab, quizPanel.transform);
        scope.transform.position = worldPos;
        scope.transform.SetAsLastSibling();
        Destroy(scope, scopeDuration);
    }

    void PlayShootSound()
    {
        if (shootSfx == null)
            return;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.PlayOneShot(shootSfx);
    }

    void EndQuiz()
    {
        quizRunning = false;

        // Immediately destroy all active spawned items
        foreach (GameObject item in activeSpawnedItems)
        {
            if (item != null)
                Destroy(item);
        }
        activeSpawnedItems.Clear();

        quizPanel.SetActive(false);
        scorePanel.SetActive(true);

        finalScoreText.text = $"Iyong Puntos: {score}";

        bool passed = score >= 10;
        if (passed)
        {
            lessonText.text =
                "Aralin: Pangangailangan at Kagustuhan\n" +
                "Yunit 1, Aralin 3 - Ekonomiks 9\n\n" +
                "PANGANGAILANGAN: Mga bagay na kailangan para mabuhay nang maayos\n" +
                "Halimbawa: Pagkain, damit, tahanan, tubig, edukasyon, kalusugan\n\n" +
                "KAGUSTUHAN: Mga bagay na gusto natin ngunit hindi lahat ay kailangan\n" +
                "Halimbawa: Gadgets, laruan, luxury items, entertainment\n\n" +
                "Mahalagang Leksyon:\n" +
                "• Ang pagkakaiba ay pundasyon ng smart spending.\n" +
                "• I-budget ang pera para sa PANGANGAILANGAN muna bago sa KAGUSTUHAN.\n" +
                "• Ang disiplina sa paggastos ay susi sa financial stability.\n" +
                "• Minsan, ang kagustuhan ay nagiging pangangailangan depende sa sitwasyon.\n\n" +
                "Ang pag-unawa sa konseptong ito ay tumutulong sa atin na gumawa ng matalinong desisyon!";

            // Rewards based on score
            pointsReward = 0;
            int xpReward = 0;

            switch (score)
            {
                case int n when n >= 7: pointsReward = 100; xpReward = 10; break;
                case 6: pointsReward = 80; xpReward = 8; break;
                case 5: pointsReward = 60; xpReward = 6; break;
                case 4: pointsReward = 40; xpReward = 4; break;
                case 3: pointsReward = 20; xpReward = 2; break;
                default: pointsReward = 10; xpReward = 1; break;
            }

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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPoints(pointsReward);
                GameManager.Instance.AddXP(xpReward);
                GameManager.Instance.MarkMiniQuiz2Completed();
                TaskManager.CheckMiniGamesCompletion();
            }

            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.LoadProfile();
                ProfileManager.Instance.UpdateProgressBars();
            }
        }
        else
        {
            lessonText.text =
                "Aralin: Pangangailangan at Kagustuhan\n" +
                "Yunit 1, Aralin 3 - Ekonomiks 9\n\n" +
                "Kailangan ng hindi bababa sa 10 puntos para makapasa.\n\n" +
                "Matuto muli mula sa quiz:\n" +
                "PANGANGAILANGAN: Pagkain, damit, tahanan, edukasyon, kalusugan\n" +
                "KAGUSTUHAN: Gadgets, laruan, entertainment, luxury items\n\n" +
                "Laging tandaan: Magplano at magbudget tayo. Pamilya muna, tapos ang sariling gusto!\n" +
                "Subukan muli at ipakita ang pag-unawa mo sa pangangailangan at kagustuhan!";
        }

        NotifyManagersQuizCompleted(passed);

        if (retryButton != null)
            retryButton.gameObject.SetActive(!passed);
    }



    IEnumerator ReloadCoroutine()
    {
        if (isReloading)
            yield break;

        isReloading = true;
        if (reloadingTextTransform != null)
            reloadingTextTransform.gameObject.SetActive(true);

        yield return new WaitForSeconds(reloadTime);

        bulletsRemaining = maxBullets;
        UpdateBulletIndicators();
        isReloading = false;

        if (reloadingTextTransform != null)
            reloadingTextTransform.gameObject.SetActive(false);
    }

    void UpdateBulletIndicators()
    {
        for (int i = 0; i < bulletIndicators.Length && i < maxBullets; i++)
        {
            Image img = bulletIndicators[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = i < bulletsRemaining ? bulletUnusedColor : bulletUsedColor;
            }
        }
    }

    void CloseScorePanel()
    {
        scorePanel.SetActive(false);
    }

    private void NotifyManagersQuizCompleted(bool passed)
    {
        if (GameManager.Instance != null)
        {
            if (passed)
                GameManager.Instance.MarkMiniQuiz2Completed();

            TaskManager.CheckMiniGamesCompletion();
        }

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
        if (pointsPulseLoop && scorePanel.activeSelf)
        {
            pointsColorCoroutine = StartCoroutine(AnimatePointsTextColor());
        }
    }

    public void RetryQuiz()
    {
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        scorePanel.SetActive(false);
        ShowStartUI();
    }
}
