using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MiniQuizManager2 : MonoBehaviour
{
    public static MiniQuizManager2 Instance;

    [Header("UI Elements")]
    public GameObject quizPanel;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject[] heroPrefabs;
    public string[] heroNames;
    public Transform heroSpawnPoint;
    public GameObject item1PrefabDisplay;
    public GameObject item2PrefabDisplay;
    public Transform item1CountTextTransform;
    public Transform item2CountTextTransform;
    public Transform item1ButtonTransform;
    public Transform item2ButtonTransform;
    public TextMeshProUGUI performanceText;

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
    public Button item1Button;
    public Button item2Button;

    [Header("Start Screen")]
    public Transform startScreenPanel;
    public Transform startButtonTransform;
    private Button startButton;
    public TextMeshProUGUI countdownText;
    private TextMeshProUGUI countdownTextUI;
    public float instructionsTypingSpeed = 0.04f;
    private Coroutine typingCoroutine;
    private Coroutine countdownCoroutine;
    public string startInstructionsText = "Gamitin ang basket para hulihin ang mga Pangangailangan at iwasan ang mga Kagustuhan! Kapag nahuli ang berde, bawas 2 puntos; kapag nahuli ang kulay ginto, dagdag 2 puntos.";

    [Header("Catcher")]
    public Transform catcherTransform;
    public float catcherMoveSpeed = 12f;
    public float catcherMinX = -4f;
    public float catcherMaxX = 4f;
    // Optional RectTransform to constrain the catcher's horizontal movement
    public RectTransform catcherSlot;
    // Optional UI area that defines the playable catcher region (e.g. bg game object/quiz panel)
    public RectTransform catcherArea;
    public Button catcherLeftButton;
    public Button catcherRightButton;
    public float catcherButtonMoveAmount = 0.5f;
    private bool catcherLeftHeld = false;
    private bool catcherRightHeld = false;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Items")]
    public GameObject[] needItems;   // catch for points
    public GameObject[] wantItems;   // avoid; deduct points
    public GameObject[] greenItems;  // deduct 2 points when caught
    public GameObject[] goldItems;   // add 2 points when caught

    [Header("Quiz Settings")]
    public float spawnInterval = 1f;
    public float itemLifetime = 2.5f;
    public float quizDuration = 30f;
    public float instructionsDuration = 4f;
    public int passingScore = 10;

    private int score = 0;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation
    private float timer;
    private bool quizRunning = false;
    private bool timerPaused = false;
    private int scoreMultiplier = 1;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private List<GameObject> activeSpawnedItems = new List<GameObject>();

    private TextMeshProUGUI item1CountTextUI;
    private TextMeshProUGUI item2CountTextUI;
    private int item1Count = 0;
    private int item2Count = 0;
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private Coroutine prefabDisplayCoroutine;

    private bool catcherGrabbed = false;
    private float catcherGrabOffset = 0f;

    // GREEN FEATURE
    private bool greenUnlocked = false;
    private bool speedBoostActive = false;

    private void Awake()
    {
        Instance = this;
        quizPanel.SetActive(false);
        scorePanel.SetActive(false);
        exitButton.onClick.AddListener(CloseScorePanel);
        if (startButtonTransform != null)
            startButton = startButtonTransform.GetComponent<Button>();

        if (countdownText != null)
            countdownTextUI = countdownText;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (item1ButtonTransform != null)
            item1Button = item1ButtonTransform.GetComponent<Button>();
        if (item2ButtonTransform != null)
            item2Button = item2ButtonTransform.GetComponent<Button>();

        if (item1Button != null)
            item1Button.onClick.AddListener(OnItem1Pressed);
        if (item2Button != null)
            item2Button.onClick.AddListener(OnItem2Pressed);

        if (catcherLeftButton != null)
        {
            catcherLeftButton.onClick.AddListener(MoveCatcherLeftOneStep);
            AddButtonHoldEvents(catcherLeftButton.gameObject, StartMoveLeft, StopMoveLeft);
        }
        if (catcherRightButton != null)
        {
            catcherRightButton.onClick.AddListener(MoveCatcherRightOneStep);
            AddButtonHoldEvents(catcherRightButton.gameObject, StartMoveRight, StopMoveRight);
        }

        if (item1CountTextTransform != null)
            item1CountTextUI = item1CountTextTransform.GetComponent<TextMeshProUGUI>();
        if (item2CountTextTransform != null)
            item2CountTextUI = item2CountTextTransform.GetComponent<TextMeshProUGUI>();

        LoadActiveUserItems();
        ApplyHeroPrefabFromActiveUser();
        UpdateItemButtonStates();
        UpdateItemCountText();

        if (spawnPoints != null && spawnPoints.Length > 0)
            availableSpawnPoints = new List<Transform>(spawnPoints);

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryQuiz);
            retryButton.gameObject.SetActive(false);
        }

        HideStartUI();
    }

    public void StartQuiz()
    {
        score = 0;
        quizRunning = false;
        greenUnlocked = false;
        speedBoostActive = false;
        quizPanel.SetActive(false);
        scorePanel.SetActive(false);
        timerText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);

        ShowStartUI();
    }

    public void OnStartButtonClicked()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (startButtonTransform != null)
            startButtonTransform.gameObject.SetActive(false);

        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(StartCountdownThenBegin());
    }

    IEnumerator StartCountdownThenBegin()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        if (countdownTextUI != null)
        {
            countdownTextUI.text = "3";
            yield return new WaitForSecondsRealtime(0.8f);
            countdownTextUI.text = "2";
            yield return new WaitForSecondsRealtime(0.8f);
            countdownTextUI.text = "1";
            yield return new WaitForSecondsRealtime(0.8f);
        }

        HideStartUI();
        StartGame();
        countdownCoroutine = null;
    }

    void StartGame()
    {
        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        quizPanel.SetActive(true);
        scorePanel.SetActive(false);
        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);

        LoadActiveUserItems();
        ApplyHeroPrefabFromActiveUser();
        UpdateItemButtonStates();
        UpdateItemCountText();

        timer = quizDuration;
        quizRunning = true;

        UpdateScoreText();
        UpdateTimerText();

        StartCoroutine(SpawnLoop());
        StartCoroutine(TimerLoop());
    }

    public void ShowStartUI()
    {
        if (startScreenPanel != null)
            startScreenPanel.gameObject.SetActive(true);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        if (scorePanel != null)
            scorePanel.SetActive(false);

        if (startButtonTransform != null)
            startButtonTransform.gameObject.SetActive(true);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (instructionsText != null)
        {
            instructionsText.gameObject.SetActive(true);
            instructionsText.text = string.Empty;
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeInstructionsText(startInstructionsText));
        }
    }

    public void HideStartUI()
    {
        if (startScreenPanel != null)
            startScreenPanel.gameObject.SetActive(false);

        if (startButtonTransform != null)
            startButtonTransform.gameObject.SetActive(false);

        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private IEnumerator TypeInstructionsText(string text)
    {
        if (instructionsText == null)
            yield break;

        instructionsText.text = string.Empty;

        foreach (char c in text)
        {
            instructionsText.text += c;
            yield return new WaitForSecondsRealtime(instructionsTypingSpeed);
        }

        typingCoroutine = null;
    }

    public void RetryQuiz()
    {
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        scorePanel.SetActive(false);
        StartQuiz();
    }

    private void Update()
    {
        if (catcherTransform == null || !quizRunning)
            return;

        if (catcherLeftHeld)
            MoveCatcher(-catcherMoveSpeed * Time.deltaTime);
        if (catcherRightHeld)
            MoveCatcher(catcherMoveSpeed * Time.deltaTime);

        float moveAmount = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
            moveAmount -= catcherMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow))
            moveAmount += catcherMoveSpeed * Time.deltaTime;

        if (moveAmount != 0f)
            MoveCatcher(moveAmount);
    }

    public void MoveCatcherLeftOneStep()
    {
        MoveCatcher(-catcherButtonMoveAmount);
    }

    public void MoveCatcherRightOneStep()
    {
        MoveCatcher(catcherButtonMoveAmount);
    }

    private void MoveCatcher(float deltaX)
    {
        RectTransform catcherRect = catcherTransform as RectTransform;
        if (catcherRect == null)
            return;

        // Use catcherMinX and catcherMaxX from Inspector
        float currentX = catcherRect.localPosition.x;
        float targetX = Mathf.Clamp(currentX + deltaX, catcherMinX, catcherMaxX);

        Vector3 targetPos = new Vector3(targetX, catcherRect.localPosition.y, catcherRect.localPosition.z);
        catcherRect.localPosition = targetPos;
    }

    public void StartMoveLeft()
    {
        catcherLeftHeld = true;
    }

    public void StopMoveLeft()
    {
        catcherLeftHeld = false;
    }

    public void StartMoveRight()
    {
        catcherRightHeld = true;
    }

    public void StopMoveRight()
    {
        catcherRightHeld = false;
    }

    private void AddButtonHoldEvents(GameObject buttonObject, UnityEngine.Events.UnityAction startAction, UnityEngine.Events.UnityAction stopAction)
    {
        if (buttonObject == null)
            return;

        EventTrigger trigger = buttonObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = buttonObject.AddComponent<EventTrigger>();

        var pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((data) => startAction());
        trigger.triggers.Add(pointerDownEntry);

        var pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((data) => stopAction());
        trigger.triggers.Add(pointerUpEntry);

        var pointerExitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExitEntry.callback.AddListener((data) => stopAction());
        trigger.triggers.Add(pointerExitEntry);
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
        StartTyping(performanceText, "Ginamit ang Item1! Hinto muna ang oras ng 5 segundo.", 0.01f);
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

        if (prefabDisplayCoroutine != null)
            StopCoroutine(prefabDisplayCoroutine);

        prefabDisplayCoroutine = StartCoroutine(HidePrefabAfterDelay(currentPrefabDisplayInstance, duration));
    }

    private IEnumerator HidePrefabAfterDelay(GameObject prefabInstance, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (prefabInstance != null)
            Destroy(prefabInstance);
        if (prefabInstance == currentPrefabDisplayInstance)
            currentPrefabDisplayInstance = null;
        prefabDisplayCoroutine = null;
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

    private IEnumerator PreQuizInstructions()
    {
        yield return null;

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
        if (spawnPoints.Length == 0) return;

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        float roll = Random.value;

        if (greenUnlocked && roll < 0.2f && greenItems.Length > 0)
        {
            SpawnFromPool(greenItems, spawn);
        }
        else if (roll < 0.35f && goldItems.Length > 0)
        {
            SpawnFromPool(goldItems, spawn);
        }
        else
        {
            bool spawnNeed = Random.value > 0.5f;
            GameObject[] pool = spawnNeed ? needItems : wantItems;
            SpawnFromPool(pool, spawn);
        }
    }

    void SpawnFromPool(GameObject[] pool, Transform spawn)
    {
        if (pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];
        GameObject obj = Instantiate(prefab, spawn.position, Quaternion.identity, quizPanel.transform);
        activeSpawnedItems.Add(obj);
        // ensure spawned item renders above the panel children
        obj.transform.SetAsLastSibling();

        // Do not auto-destroy here; let the item's own `SwipeItem` behaviour move it down
        // and destroy when it falls offscreen. Avoid altering Rigidbody2D gravity here.
    }

    IEnumerator DestroyAndFree(GameObject obj, Transform point)
    {
        yield return new WaitForSeconds(itemLifetime);
        if (obj != null)
        {
            Destroy(obj);
            activeSpawnedItems.Remove(obj);
        }
        if (!availableSpawnPoints.Contains(point))
            availableSpawnPoints.Add(point);
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

    void UpdateTimerText()
    {
        timerText.text = $"Oras: {Mathf.CeilToInt(timer)}";
    }

    void UpdateScoreText()
    {
        scoreText.text = $"Puntos: {score}";
    }

    public void CollectNeedItem(GameObject item)
    {
        score += scoreMultiplier;
        CheckGreenUnlock();
        UpdateScoreText();
        Destroy(item);
        activeSpawnedItems.Remove(item);
    }

    public void CollectWantItem(GameObject item)
    {
        score = Mathf.Max(0, score - 1);
        UpdateScoreText();
        Destroy(item);
        activeSpawnedItems.Remove(item);
    }

    // 🟢 CALLED WHEN GREEN ITEM IS CAUGHT
    public void CollectGreenItem(GameObject item)
    {
        score = Mathf.Max(0, score - 2);
        CheckGreenUnlock();
        UpdateScoreText();
        Destroy(item);
        activeSpawnedItems.Remove(item);

        if (!speedBoostActive)
        {
            StartCoroutine(SpawnSpeedBoost());
        }
    }

    public void CollectGoldItem(GameObject item)
    {
        score += 2 * scoreMultiplier;
        CheckGreenUnlock();
        UpdateScoreText();
        Destroy(item);
        activeSpawnedItems.Remove(item);
    }

    public void RemoveSpawnedItem(GameObject item)
    {
        if (item != null)
            activeSpawnedItems.Remove(item);
    }

    void CheckGreenUnlock()
    {
        if (!greenUnlocked && score >= 10)
        {
            greenUnlocked = true;
            Debug.Log("🟢 Green items unlocked!");
        }
    }

    IEnumerator SpawnSpeedBoost()
    {
        speedBoostActive = true;
        float originalInterval = spawnInterval;

        spawnInterval = 0.5f;
        yield return new WaitForSeconds(2f);

        spawnInterval = originalInterval;
        speedBoostActive = false;
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
        HideStartUI();

        finalScoreText.text = $"Iyong Puntos: {score}";

        bool passed = score >= passingScore;
        bool canComplete = passed;

        Debug.Log($"MiniQuizManager2: EndQuiz score={score}, passingScore={passingScore}, passed={passed}");

        lessonText.text =
            "Aralin:\n" +
            "- Pangangailangan (Needs) ay mga bagay na kailangan para sa buhay at kabuhayan.\n" +
            "- Kagustuhan (Wants) ay mga bagay na nais ngunit hindi pangunahing kailangan.\n" +
            "- Tandaan: Ang kagustuhan ay nagiging personal na pangangailangan depende sa sitwasyon.\n" +
            "  Halimbawa: Ang laruan ay WANT, pero nagiging NEED kapag umiiyak ang bata at kailangan niyang kumalma.";

        if (!passed)
        {
            lessonText.text =
                "Nabigo ang mini-quiz. Kailangan ng hindi bababa sa 10 puntos para makapasa.\n" +
                "Maaari mong subukan muli gamit ang Retry button.";
        }

        if (retryButton != null)
            retryButton.gameObject.SetActive(!passed);
        else
            Debug.LogWarning("MiniQuizManager2: retryButton is not assigned in the inspector.");

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
        }

        NotifyManagersQuizCompleted(canComplete);
    }

    private void NotifyManagersQuizCompleted(bool passed)
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");

        if (passed)
        {
            Debug.Log("MiniQuizManager2: Passed - marking mini quiz 3 completed.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.MarkMiniQuiz3Completed();
            }
            else
            {
                Debug.LogError("MiniQuizManager2: GameManager.Instance is null in NotifyManagersQuizCompleted.");
            }

            SaveMiniQuiz3CompletionToProfile(activeUser, true);
        }
        else
        {
            Debug.Log("MiniQuizManager2: Failed - not marking completion.");
        }

        if (GameManager.Instance != null)
        {
            TaskManager.CheckMiniGamesCompletion();
        }

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();           // Load latest saved data
            ProfileManager.Instance.UpdateProgressBars();   // Update progress bar UI
        }
        else
        {
            Debug.LogWarning("MiniQuizManager2: ProfileManager.Instance is null. Cannot refresh profile UI.");
        }
    }

    private void SaveMiniQuiz3CompletionToProfile(string activeUser, bool completed)
    {
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("MiniQuizManager2: cannot save miniQuiz3 completion because ActiveUser is empty.");
            return;
        }

        if (!completed)
        {
            Debug.Log("MiniQuizManager2: Quiz failed, not saving miniQuiz3Completed to profile.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        ProfilePlayerData profile;

        if (File.Exists(path))
        {
            profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        }
        else
        {
            profile = new ProfilePlayerData { username = activeUser };
            Debug.LogWarning($"MiniQuizManager2: profile file not found, creating new profile for '{activeUser}' at '{path}'");
        }

        profile.miniQuiz3Completed = true;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
        Debug.Log($"MiniQuizManager2: Saved miniQuiz3Completed=true for '{activeUser}' to '{path}'");
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

    void CloseScorePanel()
    {
        scorePanel.SetActive(false);
    }
}
