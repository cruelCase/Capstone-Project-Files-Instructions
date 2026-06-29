using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGameWise3Manager : MonoBehaviour
{
    public static MiniGameWise3Manager Instance;

    [Header("UI References")]
    public TextMeshProUGUI situationText;      // Situation description
    public TextMeshProUGUI scoreText;          // Optional for showing score in quiz
    public GameObject startPanel;
    public Button startButton;
    public Button retryButton;
    public TextMeshProUGUI startInstructionText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI timerText;          // Timer display
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
    public TextMeshProUGUI item1CountTextUI;
    public TextMeshProUGUI item2CountTextUI;
    public TextMeshProUGUI performanceText;
    public Button item1Button;
    public Button item2Button;

    [Header("Quiz Panel")]
    public GameObject quizPanel;

    [Header("Settings")]
    public int totalSituations = 10;
    public float swipeThreshold = 50f;  // Minimum swipe distance to register
    public float countdownDuration = 3f;
    public float gameDuration = 45f;  // 45 seconds for the quiz

    private int currentSituationIndex = 0;
    private int correctAnswers = 0;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation
    private List<YesNoSituation> situations = new List<YesNoSituation>();
    private bool gameStarted = false;
    private bool firstChoiceMade = false;
    private Coroutine situationTextCoroutine;
    private Coroutine textDisappearCoroutine;
    private Coroutine swipeAnimationCoroutine;
    private Coroutine countdownCoroutine;
    private Coroutine timerCoroutine;
    private float timeRemaining;
    private Color situationTextStartColor;
    private Color situationTextHighlightColor;
    private const float k_TextDisappearChance = 0.25f;
    private const float k_TextDisappearDuration = 0.5f;
    
    // Swipe tracking
    private Vector2 dragStartPosition;
    private bool isDragging = false;
    // hero/item private state
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private Coroutine typingCoroutine;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;
    private int item1Count = 0;
    private int item2Count = 0;
    private int scoreMultiplier = 1;
    private bool timerPaused = false;  // Flag to pause timer for Item1

    void Awake()
    {
        Instance = this;
        if (situationText != null)
            situationTextStartColor = situationText.color;

        if (startPanel != null)
            startPanel.SetActive(true);
        if (quizPanel != null)
            quizPanel.SetActive(false);
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        if (scorePanel != null && scorePanel.panel != null)
            scorePanel.panel.SetActive(false);
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryPressed);

        if (startInstructionText != null)
        {
            startInstructionText.text = "Sagutin ang mga katanungan sa pamamagitan ng pag-swipe. I-swipe ang kaliwa para sa \"Oo\" at kanan para sa \"Hindi\". Basahin nang mabuti ang bawat sitwasyon at gumawa ng tamang desisyon.";
        }

        // Load active user items and spawn hero from profile
        LoadActiveUserItems();
        ApplyHeroPrefabFromActiveUser();

        // Wire item buttons
        if (item1Button != null)
            item1Button.onClick.AddListener(OnItem1Pressed);
        if (item2Button != null)
            item2Button.onClick.AddListener(OnItem2Pressed);

        UpdateItemCountText();
        UpdateItemButtonStates();
    }

    void Update()
    {
        if (!gameStarted) return;

        // Handle swipe input
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Vector2 dragEndPosition = Input.mousePosition;
            Vector2 dragDelta = dragEndPosition - dragStartPosition;

            // Check if swipe is horizontal enough
            if (Mathf.Abs(dragDelta.x) > swipeThreshold)
            {
                bool isLeftSwipe = dragDelta.x < 0;  // Left swipe = yes
                PlaySwipeAnimation(isLeftSwipe);
                OnAnswerSelected(isLeftSwipe);
            }
        }
    }

    public void ShowStartScreen()
    {
        if (this.gameObject != null)
            this.gameObject.SetActive(true);

        if (startPanel != null)
            startPanel.SetActive(true);
        if (quizPanel != null)
            quizPanel.SetActive(true);
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        if (scorePanel != null && scorePanel.panel != null)
            scorePanel.panel.SetActive(false);
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        if (startInstructionText != null)
            startInstructionText.gameObject.SetActive(true);

        gameStarted = false;
        currentSituationIndex = 0;
        correctAnswers = 0;
    }

    void OnStartButtonClicked()
    {
        if (startInstructionText != null)
            startInstructionText.gameObject.SetActive(false);

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(CountdownBeforeGame());
    }

    IEnumerator CountdownBeforeGame()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < countdownDuration)
        {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0f, countdownDuration - elapsed);
            if (countdownText != null)
                countdownText.text = Mathf.Ceil(remaining).ToString();

            yield return null;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(false);

        countdownCoroutine = null;
        StartMiniGame();
    }

    void OnRetryPressed()
    {
        StopTimer();
        ShowStartScreen();
    }

    IEnumerator TimerCoroutine()
    {
        while (timeRemaining > 0f && gameStarted)
        {
            if (!timerPaused)  // Only count down if not paused
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining < 0f)
                    timeRemaining = 0f;
            }

            if (timerText != null)
                timerText.text = timeRemaining.ToString("F1");

            yield return null;
        }

        if (timeRemaining <= 0f && gameStarted)
        {
            // Time's up - end game as failure
            StopTimer();
            EndMiniGame(true);
        }

        timerCoroutine = null;
    }

    void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    public void StartMiniGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        currentSituationIndex = 0;
        correctAnswers = 0;
        timeRemaining = gameDuration;

        if (quizPanel != null)
            quizPanel.SetActive(true);

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        LoadSituations();
        ShowSituation();

        // Start timer
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    void LoadSituations()
    {
        situations.Clear();

        // Situations based on "Pagkunsumo" / factors affecting consumption
        situations.Add(new YesNoSituation("May limitadong allowance si Juan bawat linggo, kaya pinipili niya ang meryenda nang maingat. Nakakaapekto ba ang kita sa konsumo?", true));
        situations.Add(new YesNoSituation("Binili ni Maria ang paborito niyang laruan kahit mahal at wala siyang pera. Hindi ba naaapektuhan ng kita ang kanyang desisyon sa konsumo?", false));
        situations.Add(new YesNoSituation("Bumibili lang si Anna ng mga bagay na kailangan niya, hindi ang gusto niya. Nakakaapekto ba sa konsumo ang pagkakaiba ng pangangailangan at kagustuhan?", true));
        situations.Add(new YesNoSituation("Bumili si Pedro ng kuwaderno dahil iisa lamang ang available sa tindahan. Nakakaapekto ba sa konsumo ang availability?", true));
        situations.Add(new YesNoSituation("Inimbitahan ka ng mga kaibigan sa sinehan, kaya nagpasya kang pumunta kahit hindi plano mong gumastos. Nakakaapekto ba sa konsumo ang impluwensiya ng grupo?", true));
        situations.Add(new YesNoSituation("Pinili ni Maria ang laruan dahil paborito niyang kulay ito, kahit hindi hining presyo o kailangan. Nakakaapekto ba sa konsumo ang kulay?", false));
        situations.Add(new YesNoSituation("Nagdesisyon si Juan na hindi bumili ng mahal na meryenda dahil ubos na halos ang allowance niya. Nakakaapekto ba sa konsumo ang presyo?", true));
        situations.Add(new YesNoSituation("Hindi pinansin ni Anna ang suhestiyon ng mga kaibigan at bumili ayon sa kanyang kailangan. Hindi ba naaapektuhan ng social influence ang kanyang konsumo?", true));
        situations.Add(new YesNoSituation("Bumili si Pedro ng kahit anong available na kuwaderno kahit may isa na siya sa bahay. Nakakaapekto ba sa konsumo ang pangangailangan vs kagustuhan dito?", false));
        situations.Add(new YesNoSituation("Inimbitahan si Maria ng kaibigan niyang kumain sa labas, pero pumunta lang siya kung sapat ang pera niya. Nakakaapekto ba sa konsumo ang kita?", true));
    }

    void ShowSituation()
    {
        if (currentSituationIndex >= situations.Count)
        {
            EndMiniGame();
            return;
        }

        YesNoSituation s = situations[currentSituationIndex];
        situationText.text = s.description;

        // Always animate the situation text when shown
        if (situationText != null)
        {
            situationTextHighlightColor = (currentSituationIndex % 2 == 0)
                ? new Color(0.2f, 0.8f, 1f)
                : new Color(1f, 0.8f, 0.2f);

            StartSituationTextAnimation();
        }
    }

    void StartSituationTextAnimation()
    {
        if (situationText == null)
            return;

        if (situationTextCoroutine != null)
            StopCoroutine(situationTextCoroutine);

        situationTextCoroutine = StartCoroutine(AnimateSituationText());
    }

    void StartTextDisappearSequence()
    {
        if (situationText == null)
            return;

        if (textDisappearCoroutine != null)
            StopCoroutine(textDisappearCoroutine);

        textDisappearCoroutine = StartCoroutine(PlayTextDisappearThenAnimate());
    }

    IEnumerator PlayTextDisappearThenAnimate()
    {
        var originalColor = situationText.color;
        situationText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        yield return new WaitForSeconds(k_TextDisappearDuration);
        situationText.color = originalColor;
        textDisappearCoroutine = null;
        StartSituationTextAnimation();
    }

    IEnumerator AnimateSituationText()
    {
        float duration = 1.0f;
        float elapsed = 0f;

        // Reset scale to Vector3.one to prevent accumulation
        situationText.rectTransform.localScale = Vector3.one;
        float baseScale = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin(elapsed * Mathf.PI * 2f) * 0.5f + 0.5f;
            situationText.color = Color.Lerp(situationTextStartColor, situationTextHighlightColor, t);

            float scalePulse = 1f + Mathf.Sin(elapsed * Mathf.PI * 2f) * 0.05f;
            situationText.rectTransform.localScale = Vector3.one * baseScale * scalePulse;

            yield return null;
        }

        situationText.color = situationTextStartColor;
        situationText.rectTransform.localScale = Vector3.one * baseScale;
        situationTextCoroutine = null;
    }

    void OnAnswerSelected(bool playerAnswer)
    {
        YesNoSituation s = situations[currentSituationIndex];
        if (playerAnswer == s.correctAnswer)
            correctAnswers++;

        currentSituationIndex++;
        firstChoiceMade = true;

        // Check if quiz is done
        if (currentSituationIndex >= situations.Count)
        {
            StopTimer();
            EndMiniGame(false);
        }
        else
        {
            ShowSituation();
        }
    }

    void PlaySwipeAnimation(bool isLeftSwipe)
    {
        if (situationText == null) return;

        if (swipeAnimationCoroutine != null)
            StopCoroutine(swipeAnimationCoroutine);

        swipeAnimationCoroutine = StartCoroutine(SwipeAnimationCoroutine(isLeftSwipe));
    }

    IEnumerator SwipeAnimationCoroutine(bool isLeftSwipe)
    {
        float duration = 0.6f;
        float elapsed = 0f;
        Vector3 startScale = situationText.rectTransform.localScale;
        Vector2 startPos = situationText.rectTransform.anchoredPosition;
        float swipeDirection = isLeftSwipe ? -1f : 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Vibration: slow to fast (increasing frequency)
            float vibrationFrequency = 2f + t * 20f;  // 2Hz to 22Hz
            float vibrationAmount = Mathf.Sin(elapsed * vibrationFrequency * Mathf.PI * 2f) * (1f - t) * 8f;

            // Size change: scale up then shrink as it exits
            float scaleMultiplier = 1f + t * 0.3f;

            // Movement: move in swipe direction
            float moveDistance = t * 300f;

            situationText.rectTransform.localScale = startScale * scaleMultiplier;
            situationText.rectTransform.anchoredPosition = startPos + new Vector2(
                vibrationAmount + (swipeDirection * moveDistance),
                0f
            );

            yield return null;
        }

        swipeAnimationCoroutine = null;
        situationText.rectTransform.localScale = startScale;
        situationText.rectTransform.anchoredPosition = startPos;
    }

    void EndMiniGame(bool timedOut = false)
    {
        gameStarted = false;
        StopTimer();

        if (quizPanel != null)
            quizPanel.SetActive(false);

        int maxScore = situations.Count;
        bool passed = !timedOut && correctAnswers >= 6;  // Pass if 6 or more correct out of 10 (and not timed out)

        // Lesson-based comments about consumption
        string lesson = timedOut
            ? "Tapos na ang oras! Tandaan, ang matalinong desisyon sa pagkonsumo ay nangangailangan ng maingat na pag-iisip. Practice ang magpapahusay sa iyo!"
            : "Aralin: Ang pagkonsumo ay naaapektuhan ng iba't ibang salik tulad ng kita, presyo, pangangailangan kontra pagnanasa, pagkakaroon ng produkto, at impluwensiya ng lipunan. Ang pag-unawa dito ay tumutulong sa paggawa ng mas matalinong desisyon sa paggastos.";

        string title = timedOut
            ? "Tapos na ang Oras!"
            : (passed ? $"{correctAnswers} / {maxScore}" : $"Bigo: {correctAnswers} / {maxScore}");

        // Show score along with lesson
        scorePanel.ShowScore(title, lesson);

        if (retryButton != null)
            retryButton.gameObject.SetActive(!passed);

        if (!passed)
            return;

        // Reward points & XP only on pass
        pointsReward = correctAnswers * 10;
        int xpReward = correctAnswers * 2;

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

        GameManager.Instance.MarkMiniQuiz3cCompleted(); // mark Area 6
        TaskManager.CheckMiniGamesCompletion();
        if (ProfileManager.Instance != null && ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.miniQuiz3cCompleted = true;
            ProfileManager.Instance.UpdateProgressBars();
            ProfileManager.Instance.UpdateBadges();
        }
        ProfileManager.Instance.LoadProfile();
    }

    // ------------------ Profile & Item Helpers ------------------
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
        if (profile == null) return;
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser)) return;
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
        if (profile == null) return;

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
        else return;

        SaveActiveUserProfile(profile);
        UpdateItemButtonStates();
        UpdateItemCountText();
    }

    public void OnItem1Pressed()
    {
        if (!gameStarted || item1Count <= 0) return;
        DeductItemCount(1);
        if (item1PauseCoroutine != null) StopCoroutine(item1PauseCoroutine);
        item1PauseCoroutine = StartCoroutine(PauseTimerForSeconds(5f));
        StartTyping(performanceText, "Ginamit ang Item1! Hinto muna ang oras ng 5 segundo.", 0.01f);
        ShowPrefabDisplay(item1PrefabDisplay, 1f);
    }

    public void OnItem2Pressed()
    {
        if (!gameStarted || item2Count <= 0) return;
        DeductItemCount(2);
        if (item2BonusCoroutine != null) StopCoroutine(item2BonusCoroutine);
        item2BonusCoroutine = StartCoroutine(ActivateItem2Bonus(10f));
        StartTyping(performanceText, "Na-activate ang Item2! Dobleng puntos sa susunod na tama sa loob ng 10 segundo.", 0.01f);
        ShowPrefabDisplay(item2PrefabDisplay, 1f);
    }

    private IEnumerator PauseTimerForSeconds(float seconds)
    {
        timerPaused = true;  // Start pausing
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        timerPaused = false;  // Resume timer
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

    private void ShowPrefabDisplay(GameObject prefabDisplay, float duration)
    {
        if (prefabDisplay == null) return;
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
        if (prefabInstance != null) Destroy(prefabInstance);
        if (currentPrefabDisplayInstance == prefabInstance) currentPrefabDisplayInstance = null;
    }

    private void StartTyping(TextMeshProUGUI target, string content, float charDelay)
    {
        if (target == null) return;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
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
        if (pointsPulseLoop && scorePanel != null && scorePanel.panel.activeSelf)
        {
            pointsColorCoroutine = StartCoroutine(AnimatePointsTextColor());
        }
    }
}

[System.Serializable]
public class YesNoSituation
{
    public string description;
    public bool correctAnswer;

    public YesNoSituation(string desc, bool answer)
    {
        description = desc;
        correctAnswer = answer;
    }
}
