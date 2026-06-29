using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MiniGameIncomeSortManager : MonoBehaviour
{
    public static MiniGameIncomeSortManager Instance;

    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI countdownText;

    [Header("Game Settings")]
    public int totalItems = 12;
    public float spawnDelay = 0.45f;

    [Header("References")]
    public Transform spawnPoint;
    public Transform canvasParent;
    public List<GameObject> incomeItems;
    public ScorePanel scorePanel;
    public TextMeshProUGUI pointsText;     // Points earned display

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

    [Header("Start Screen")]
    public GameObject startPanel;
    public Button startButton;
    public Transform continueButtonTransform;
    public TextMeshProUGUI instructionsText;
    public Button leftButton;
    public Button rightButton;
    public Button shootButton;
    public TextMeshProUGUI selectedDirectionText;
    public Graphic cannonGraphic;
    public GameObject[] cannonDirectionImages;
    public string[] startInstructionsPages = new string[] {
        "May tatlong uri ng kita: \"Aktibong Kita (Active Income)\", \"Pasibong Kita (Passive Income)\", at \"Walang Kita (None Income)\".",
        "Ipapakita namin ang isang uri ng pinagkakakitaan, at kailangan mong tukuyin kung alin sa tatlong kategoryang ito ito nabibilang.",
        "Maaari mong igalaw ang kanyon gamit ang mga button na Kaliwa at Kanan, at pindutin ang \"Putok (Shoot)\" upang piliin ang uri ng kita na sa tingin mo ay tama."
    };
    public float instructionsTypingSpeed = 0.04f;
    public Transform[] shootTargets;
    public GameObject cannonballPrefab;
    public Transform cannonballSpawnPoint;
    public float itemMoveDuration = 0.3f;
    public float ballFlyDuration = 0.5f;
    public float ballArcHeight = 0.4f;
    public Color invalidColor = Color.red;
    public float invalidFlashDuration = 0.3f;
    [Header("Timer")]
    public float gameDuration = 45f;
    public TextMeshProUGUI timerText;
    public Button retryButton;

    private List<GameObject> itemsPool = new List<GameObject>();
    private int currentIndex = 0;
    private int score = 0;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation
    private bool gameStarted = false;
    private bool timerPaused = false;
    private int scoreMultiplier = 1;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;

    private int item1Count = 0;
    private int item2Count = 0;
    private TextMeshProUGUI item1CountTextUI;
    private TextMeshProUGUI item2CountTextUI;
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private Coroutine typingCoroutine;
    private int currentInstructionPage = 0;
    private Button continueButton;

    private int itemsAssigned = 0; // Track how many items have been assigned
    private int currentDirectionIndex = 1;
    private IncomeItem currentItem;
    private Color normalCannonColor = Color.white;
    private bool invalidFlashActive = false;
    private float remainingTime = 0f;
    private Coroutine gameTimerCoroutine = null;

    private readonly IncomeCategory[] directionCategories = new IncomeCategory[]
    {
        IncomeCategory.Active,
        IncomeCategory.Passive,
        IncomeCategory.NonIncome
    };

    private readonly string[] directionLabels = new string[]
    {
        "Aktibong Kita",
        "Pasibong Kita",
        "Walang Kita"
    };

    void Awake()
    {
        Instance = this;

        // Start screen initial state
        if (startPanel != null)
            startPanel.SetActive(true);
        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (leftButton != null)
            leftButton.onClick.AddListener(OnLeftButtonPressed);

        if (rightButton != null)
            rightButton.onClick.AddListener(OnRightButtonPressed);

        if (shootButton != null)
            shootButton.onClick.AddListener(OnShootButtonPressed);

        if (continueButtonTransform != null)
        {
            continueButton = continueButtonTransform.GetComponent<Button>();
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

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

        if (cannonGraphic != null)
            normalCannonColor = cannonGraphic.color;

        SetCannonDirection(1);
    }

    public void StartMiniGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        score = 0;
        currentIndex = 0;
        itemsAssigned = 0;
        currentItem = null;

        LoadActiveUserItems();
        ApplyHeroPrefabFromActiveUser();
        UpdateItemButtonStates();
        UpdateItemCountText();

        // Show quiz panel
        quizPanel.SetActive(true);
        UpdateSelectedDirectionText();
        SetCannonDirection(1);

        StartGameplay();
    }

    public void OnStartButtonClicked()
    {
        // Hide instruction text
        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (startButton != null)
            startButton.interactable = false;

        if (continueButtonTransform != null)
            continueButtonTransform.gameObject.SetActive(false);

        StartCoroutine(CountdownAndStart());
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

        if (startButton != null)
            startButton.gameObject.SetActive(false);

        StartTypingInstructions(currentInstructionPage);
    }

    // Show the start screen without starting the game (used by NPCs)
    public void ShowStartScreen()
    {
        currentInstructionPage = 0;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        if (instructionsText != null)
            instructionsText.gameObject.SetActive(true);

        if (startButton != null)
            startButton.gameObject.SetActive(true);

        if (continueButtonTransform != null)
            continueButtonTransform.gameObject.SetActive(false);

        StartTypingInstructions(currentInstructionPage);
    }

    IEnumerator CountdownAndStart()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

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

        // Begin the main game setup
        StartMiniGame();
        yield break;
    }

    void StartGameplay()
    {
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        // Start game timer
        remainingTime = gameDuration;
        if (timerText != null)
            timerText.gameObject.SetActive(true);
        if (gameTimerCoroutine != null)
            StopCoroutine(gameTimerCoroutine);
        gameTimerCoroutine = StartCoroutine(GameTimer());

        // Build pool and spawn first item
        BuildItemPool();
        SpawnNextItem();
    }

    IEnumerator GameTimer()
    {
        while (remainingTime > 0f)
        {
            if (!timerPaused)
                remainingTime -= Time.deltaTime;
            UpdateTimerText();
            yield return null;
        }

        remainingTime = 0f;
        UpdateTimerText();
        OnTimeUp();
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;
        timerText.text = Mathf.CeilToInt(remainingTime).ToString();
    }

    void OnTimeUp()
    {
        // Time ran out — end game and treat as failure unless score is high enough
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }

        EndGame();
    }

    public void OnLeftButtonPressed()
    {
        if (currentDirectionIndex <= 0)
        {
            StartCoroutine(FlashInvalidDirection());
            return;
        }

        SetCannonDirection(currentDirectionIndex - 1);
    }

    public void OnRightButtonPressed()
    {
        if (currentDirectionIndex >= directionCategories.Length - 1)
        {
            StartCoroutine(FlashInvalidDirection());
            return;
        }

        SetCannonDirection(currentDirectionIndex + 1);
    }

    public void OnShootButtonPressed()
    {
        if (!gameStarted || currentItem == null)
            return;

        bool correct = currentItem.category == directionCategories[currentDirectionIndex];
        Transform target = (shootTargets != null && shootTargets.Length > currentDirectionIndex)
            ? shootTargets[currentDirectionIndex]
            : null;

        StartCoroutine(AnimateShootAndSpawn(currentItem, correct, target));
        currentItem = null;
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

    IEnumerator AnimateShootAndSpawn(IncomeItem item, bool correct, Transform target)
    {
        if (item == null)
            yield break;

        GameObject itemObject = item.gameObject;
        Vector3 startPos = itemObject.transform.position;
        Vector3 endPos = cannonballSpawnPoint != null ? cannonballSpawnPoint.position : startPos;
        Vector3 startScale = itemObject.transform.localScale;

        float elapsed = 0f;
        while (elapsed < itemMoveDuration)
        {
            float t = elapsed / itemMoveDuration;
            itemObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            itemObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        itemObject.transform.position = endPos;
        itemObject.transform.localScale = Vector3.zero;

        if (cannonballPrefab != null && cannonballSpawnPoint != null && target != null)
        {
            GameObject ball = Instantiate(cannonballPrefab, cannonballSpawnPoint.position, Quaternion.identity, canvasParent);
            yield return StartCoroutine(MoveBallToTarget(ball.transform, target.position));
            Destroy(ball);
        }

        if (correct)
            score += 100 * scoreMultiplier;

        Destroy(itemObject);
        currentIndex++;
        itemsAssigned++;

        if (currentIndex >= itemsPool.Count)
            EndGame();
        else
            SpawnNextItem();
    }

    IEnumerator MoveBallToTarget(Transform ballTransform, Vector3 targetPosition)
    {
        if (ballTransform == null)
            yield break;

        Vector3 startPos = ballTransform.position;
        float elapsed = 0f;
        while (elapsed < ballFlyDuration)
        {
            float t = elapsed / ballFlyDuration;
            Vector3 basePosition = Vector3.Lerp(startPos, targetPosition, t);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * ballArcHeight;
            ballTransform.position = basePosition + Vector3.up * heightOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        ballTransform.position = targetPosition;
    }

    void SetCannonDirection(int newIndex)
    {
        currentDirectionIndex = Mathf.Clamp(newIndex, 0, directionCategories.Length - 1);
        UpdateCannonVisual();
        UpdateSelectedDirectionText();
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

    void UpdateCannonVisual()
    {
        if (cannonDirectionImages != null && cannonDirectionImages.Length > 0)
        {
            for (int i = 0; i < cannonDirectionImages.Length; i++)
            {
                if (cannonDirectionImages[i] != null)
                    cannonDirectionImages[i].SetActive(i == currentDirectionIndex);
            }
        }
    }

    void UpdateSelectedDirectionText()
    {
        if (selectedDirectionText != null)
        {
            selectedDirectionText.text = "Target: " + directionLabels[currentDirectionIndex];
        }
    }

    IEnumerator FlashInvalidDirection()
    {
        if (invalidFlashActive) yield break;

        Graphic flashTarget = cannonGraphic;
        if (flashTarget == null && cannonDirectionImages != null && cannonDirectionImages.Length > 0)
        {
            var go = cannonDirectionImages[Mathf.Clamp(currentDirectionIndex, 0, cannonDirectionImages.Length - 1)];
            if (go != null)
                flashTarget = go.GetComponent<Graphic>();
        }

        if (flashTarget == null) yield break;

        invalidFlashActive = true;
        Color original = flashTarget.color;
        flashTarget.color = invalidColor;
        yield return new WaitForSeconds(invalidFlashDuration);
        flashTarget.color = original;
        invalidFlashActive = false;
    }

    void BuildItemPool()
    {
        itemsPool.Clear();

        foreach (var item in incomeItems)
            itemsPool.Add(item);

        // Shuffle items
        for (int i = 0; i < itemsPool.Count; i++)
        {
            int rand = Random.Range(0, itemsPool.Count);
            var temp = itemsPool[i];
            itemsPool[i] = itemsPool[rand];
            itemsPool[rand] = temp;
        }
    }

    void SpawnNextItem()
    {
        if (currentIndex >= itemsPool.Count)
        {
            EndGame();
            return;
        }

        GameObject go = Instantiate(
            itemsPool[currentIndex],
            spawnPoint.position,
            Quaternion.identity,
            canvasParent
        );

        go.transform.localScale = Vector3.one;
        go.transform.SetAsLastSibling();

        currentItem = go.GetComponent<IncomeItem>();
        UpdateSelectedDirectionText();
    }


    void EndGame()
    {
        // Stop timer
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }

        // Hide quiz panel
        if (quizPanel != null)
            quizPanel.SetActive(false);

        // Calculate score
        int maxScore = Mathf.Max(1, itemsPool.Count) * 100;
        float percentage = (float)score / maxScore;

        bool passed = percentage >= 0.6f;

        string lesson =
            "Aralin: Ang pag-unawa sa iba't ibang uri ng kita ay tumutulong sa iyo na mas maayos na pagplanuhin ang iyong pera.\n" +
            "Sahod = kita mula sa trabaho\nNegosyo = kita mula sa entrepreneurship\nPasibo = kita na kinikita nang may kaunting aktibidad lamang.";

        // Show score panel with pass/fail message
        string title = passed ? $"{score} / {maxScore}" : $"Bigo: {score} / {maxScore}";
        scorePanel.ShowScore(title, lesson);

        if (passed)
        {
            // Rewards
            pointsReward = 0;
            int xpReward = 0;

            if (percentage >= 0.8f) { pointsReward = 100; xpReward = 12; }
            else if (percentage >= 0.6f) { pointsReward = 80; xpReward = 10; }
            else if (percentage >= 0.4f) { pointsReward = 60; xpReward = 8; }
            else if (percentage >= 0.2f) { pointsReward = 40; xpReward = 6; }
            else { pointsReward = 20; xpReward = 4; }

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
            GameManager.Instance.AddXP(xpReward);

            GameManager.Instance.MarkMiniQuiz1bCompleted();
            TaskManager.CheckMiniGamesCompletion();
            if (retryButton != null)
                retryButton.gameObject.SetActive(false);
        }
        else
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryPressed);
                retryButton.gameObject.SetActive(true);
            }
        }

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();
            ProfileManager.Instance.UpdateProgressBars();
        }

        // Reset flags
        gameStarted = false;
    }

    void OnRetryPressed()
    {
        // Hide score panel (some ScorePanel implementations are components; disable its GameObject if present)
        if (scorePanel != null && scorePanel.gameObject != null)
            scorePanel.gameObject.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        // Show the start screen so player can press Start again
        ShowStartScreen();

        // Reset internal state
        gameStarted = false;
        score = 0;
        currentIndex = 0;
        itemsAssigned = 0;
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

        return "";
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

    void OnInstructionPageComplete()
    {
        bool isLastPage = currentInstructionPage >= GetInstructionPageCount() - 1;

        if (isLastPage)
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(true);
                startButton.interactable = true;
            }

            if (continueButtonTransform != null)
                continueButtonTransform.gameObject.SetActive(false);
        }
        else
        {
            if (continueButtonTransform != null)
                continueButtonTransform.gameObject.SetActive(true);

            if (startButton != null)
                startButton.gameObject.SetActive(false);

            if (continueButton != null)
                continueButton.interactable = true;
        }
    }

    int GetInstructionPageCount()
    {
        if (startInstructionsPages != null && startInstructionsPages.Length > 0)
            return startInstructionsPages.Length;
        return 1;
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
        if (pointsPulseLoop && scorePanel != null && scorePanel.gameObject.activeSelf)
        {
            pointsColorCoroutine = StartCoroutine(AnimatePointsTextColor());
        }
    }
}
