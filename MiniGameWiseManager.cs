using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGameWiseManager : MonoBehaviour
{
    public static MiniGameWiseManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI situationText;      // Situation description
    public TextMeshProUGUI budgetText;         // Shows remaining budget
    public TextMeshProUGUI deductionText1;     // Deduction for choice1
    public TextMeshProUGUI deductionText2;     // Deduction for choice2
    public Button choiceButton1;
    public Button choiceButton2;
    public ScorePanel scorePanel;
    public TextMeshProUGUI pointsText;     // Points earned display

    [Header("Points Animation")]
    public Color pointsStartColor = Color.yellow;
    public Color pointsEndColor = Color.white;
    public float pointsAnimationDuration = 1.5f;
    public bool pointsPulseLoop = true;

    [Header("Quiz Panel")]
    public GameObject quizPanel; 
    public GameObject startPanel;
    public Button startButton;
    public Button retryButton;
    public TextMeshProUGUI startInstructionText;
    public TextMeshProUGUI countdownText;

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

    [Header("Images (2 per situation, total 10)")]
    public Image[] choiceImages = new Image[10]; // Assign in inspector, 2 per situation

    [Header("Cut Line Mechanic")]
    public Canvas uiCanvas;
    public RectTransform scissorSpawnPoint;
    public GameObject scissorPrefab;
    public Transform scissorParent;
    public GameObject[] lineUncutSlots = new GameObject[2];
    public GameObject[] lineCutSlots = new GameObject[2];
    public GameObject cutEffectPrefab;
    public Transform[] cutEffectSpawnPoints = new Transform[2];

    [Header("Settings")]
    public int startingBudget = 700;
    public float countdownDuration = 3f;
    public float gameTimeSeconds = 45f;
    public TextMeshProUGUI timerText;

    private int currentBudget;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation
    private int currentSituationIndex = 0;
    private List<WiseSituation> situations = new List<WiseSituation>();
    private bool gameStarted = false;
    private bool firstChoiceMade = false;
    private float timeRemaining = 0f;
    private bool timerRunning = false;
    private bool timerPaused = false;
    private Coroutine gameTimerCoroutine;
    private bool canCutLine = false;
    private Coroutine buttonJitterCoroutine;
    private Coroutine countdownCoroutine;
    private GameObject currentScissor;
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private Coroutine typingCoroutine;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;
    private int item1Count = 0;
    private int item2Count = 0;
    private int scoreMultiplier = 1;
    private RectTransform choiceButton1Rect;
    private RectTransform choiceButton2Rect;
    private Vector2 choiceButton1StartPos;
    private Vector2 choiceButton2StartPos;
    private Vector3 choiceButton1StartScale;
    private Vector3 choiceButton2StartScale;

    void Awake()
    {
        Instance = this;
        if (choiceButton1 != null)
        {
            choiceButton1Rect = choiceButton1.GetComponent<RectTransform>();
            choiceButton1StartScale = choiceButton1Rect.localScale;
        }

        if (choiceButton2 != null)
        {
            choiceButton2Rect = choiceButton2.GetComponent<RectTransform>();
            choiceButton2StartScale = choiceButton2Rect.localScale;
        }

        if (startPanel != null)
            startPanel.SetActive(true);
        if (quizPanel != null)
            quizPanel.SetActive(false);
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        if (scorePanel != null && scorePanel.panel != null)
            scorePanel.panel.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryPressed);

        if (startInstructionText != null)
        {
            startInstructionText.text = "Gamitin ang gunting upang putulin ang tamang linya at ipakita ang mas matalinong pagpili sa paggastos. Pumili sa pagitan ng dalawang pagpipilian at tiyaking may matitira kang ipon.";
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

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    public void StartMiniGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        currentBudget = startingBudget;
        currentSituationIndex = 0;
        firstChoiceMade = false;
        UpdateBudgetText();

        if (startPanel != null)
            startPanel.SetActive(false);
        if (scorePanel != null && scorePanel.panel != null)
            scorePanel.panel.SetActive(false);
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        LoadSituations();
        SpawnScissor();
        ShowSituation();

        if (quizPanel != null)
            quizPanel.SetActive(true);

        // start game timer
        timeRemaining = gameTimeSeconds;
        timerRunning = true;
        timerPaused = false;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
        if (gameTimerCoroutine != null) StopCoroutine(gameTimerCoroutine);
        gameTimerCoroutine = StartCoroutine(GameTimer());
    }

    void LoadSituations()
    {
        situations.Clear();

        // Situations inspired by Grade 9 Ekonomiks Unit 1 Aralin 5: Pagkonsumo
        // Mixed scenarios: sometimes wise = cheap, sometimes wise = expensive (quality over price)
        // Optimal total: ~420 (leaving budget ~280)
        
        // Situation 1: Need vs Want - Cheap is wise (no need to overpay for school supplies)
        situations.Add(new WiseSituation(
            "Kailangan mo ng school supplies para sa klase. Alin ang mas matalinong konsumo?",
            "Premium Branded na Set", 100,
            "Simpleng ngunit Gumaganang Set", 30
        ));

        // Situation 2: Quality vs Price - Expensive is wise (shoes worn daily, durability matters)
        situations.Add(new WiseSituation(
            "Bumibili ka ng sapatos na gamit mo araw-araw sa school. Alin ang mas matalinong desisyon?",
            "De-kalidad at Matibay na Sapatos", 160,
            "Mumurahing Sapatos (Madaling Masira)", 70
        ));

        // Situation 3: Hunger/Need - Cheap is wise (both options satisfy, so save money)
        situations.Add(new WiseSituation(
            "Gutom ka at kailangan ng merienda pagkatapos ng klase. Alin ang mas praktikal?",
            "Branded na Meryenda (Mahal)", 120,
            "Gawang Bahay (Abot-kaya)", 40
        ));

        // Situation 4: Entertainment - Cheap is wise (want vs need, unnecessary expense)
        situations.Add(new WiseSituation(
            "Gusto mong mag-enjoy pagkatapos ng exams. Alin ang mas matalinong gastusin?",
            "Mahal na Sinehan at Pagkain", 200,
            "Mga Gawain na Libre o Abot-kaya kasama ang Kaibigan", 50
        ));

        // Situation 5: Technology/Safety - Expensive is wise (reliability, safety, durability)
        situations.Add(new WiseSituation(
            "Kailangan mo ng power charger para sa online learning. Alin ang mas matalinong bili?",
            "De-kalidad at Maaasahang Charger", 140,
            "Charger na Kaduda-duda ang Kalidad", 60
        ));

        // Situations appear in order defined above (no shuffle)
    }

    void ResetAllImagePositions()
    {
        // Reset all images to their original positions
        for (int i = 0; i < choiceImages.Length; i++)
        {
            if (choiceImages[i] != null)
            {
                RectTransform rect = choiceImages[i].GetComponent<RectTransform>();
                if (rect != null)
                    rect.localPosition = Vector3.zero;
            }
        }
    }

    void ShowSituation()
    {
        WiseSituation s = situations[currentSituationIndex];

        situationText.text = s.description;
        deductionText1.text = $"-{s.cost1}";
        deductionText2.text = $"-{s.cost2}";

        // Activate only the current situation images
        for (int i = 0; i < choiceImages.Length; i++)
            choiceImages[i].gameObject.SetActive(false);

        int imgIndex = currentSituationIndex * 2;
        if (imgIndex < choiceImages.Length) choiceImages[imgIndex].gameObject.SetActive(true);
        if (imgIndex + 1 < choiceImages.Length) choiceImages[imgIndex + 1].gameObject.SetActive(true);

        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();

        if (lineUncutSlots != null && lineCutSlots != null)
        {
            for (int i = 0; i < lineUncutSlots.Length; i++)
            {
                if (lineUncutSlots[i] != null)
                    lineUncutSlots[i].SetActive(true);
                if (lineCutSlots[i] != null)
                {
                    lineCutSlots[i].SetActive(false);
                    RectTransform cutRect = lineCutSlots[i].GetComponent<RectTransform>();
                    if (cutRect != null)
                        cutRect.localPosition = Vector3.zero;
                }
            }
        }

        if (choiceButton1Rect != null)
            choiceButton1StartPos = choiceButton1Rect.anchoredPosition;
        if (choiceButton2Rect != null)
            choiceButton2StartPos = choiceButton2Rect.anchoredPosition;

        if (firstChoiceMade)
            StartChoiceButtonJitter();
    }

    void UpdateBudgetText()
    {
        budgetText.text = $"Badyet: {currentBudget}";
    }

    void SpawnScissor()
    {
        if (scissorPrefab == null || scissorSpawnPoint == null)
            return;

        if (currentScissor != null)
            Destroy(currentScissor);

        Transform parent = scissorParent != null ? scissorParent : scissorSpawnPoint != null ? scissorSpawnPoint : (quizPanel != null ? quizPanel.transform : null);
        currentScissor = Instantiate(scissorPrefab, parent, false);
        RectTransform scissorRect = currentScissor.GetComponent<RectTransform>();
        if (scissorRect != null)
        {
            scissorRect.localScale = Vector3.one;
            if (scissorSpawnPoint != null)
                scissorRect.position = scissorSpawnPoint.position;
        }

        ScissorDragHandler dragHandler = currentScissor.GetComponent<ScissorDragHandler>();
        if (dragHandler == null)
            dragHandler = currentScissor.AddComponent<ScissorDragHandler>();

        Canvas targetCanvas = uiCanvas;
        if (targetCanvas == null)
            targetCanvas = currentScissor.GetComponentInParent<Canvas>()
                         ?? scissorParent?.GetComponentInParent<Canvas>()
                         ?? scissorSpawnPoint?.GetComponentInParent<Canvas>()
                         ?? quizPanel?.GetComponentInParent<Canvas>();

        dragHandler.Initialize(this, targetCanvas);
        currentScissor.SetActive(true);
        canCutLine = true;
    }

    void ResetScissor()
    {
        if (currentScissor == null)
            return;

        Transform parent = scissorParent != null ? scissorParent : scissorSpawnPoint != null ? scissorSpawnPoint : (quizPanel != null ? quizPanel.transform : null);
        currentScissor.transform.SetParent(parent, false);
        RectTransform scissorRect = currentScissor.GetComponent<RectTransform>();
        if (scissorRect != null)
        {
            scissorRect.localScale = Vector3.one;
            if (scissorSpawnPoint != null)
            {
                scissorRect.position = scissorSpawnPoint.position;
                scissorRect.rotation = scissorSpawnPoint.rotation;
            }
        }

        currentScissor.SetActive(true);
    }

    void ApplyChoiceSelection(int cost)
    {
        currentBudget -= cost;
        if (currentBudget < 0) currentBudget = 0;
        UpdateBudgetText();

        if (!firstChoiceMade)
            firstChoiceMade = true;

        currentSituationIndex++;
    }

    public void OnScissorReleased(PointerEventData eventData)
    {
        if (!canCutLine || currentScissor == null || currentSituationIndex >= situations.Count)
        {
            ResetScissor();
            return;
        }

        bool droppedOnChoice = false;
        if (choiceButton1Rect != null && RectTransformUtility.RectangleContainsScreenPoint(choiceButton1Rect, eventData.position, eventData.pressEventCamera))
        {
            droppedOnChoice = true;
            StartCoroutine(HandleLineChoice(0));
        }
        else if (choiceButton2Rect != null && RectTransformUtility.RectangleContainsScreenPoint(choiceButton2Rect, eventData.position, eventData.pressEventCamera))
        {
            droppedOnChoice = true;
            StartCoroutine(HandleLineChoice(1));
        }

        if (!droppedOnChoice)
            ResetScissor();
    }

    IEnumerator HandleLineChoice(int choiceIndex)
    {
        canCutLine = false;
        if (currentScissor != null)
            currentScissor.SetActive(false);

        int selectedImageIndex = currentSituationIndex * 2 + choiceIndex;
        Image selectedChoiceImage = selectedImageIndex < choiceImages.Length ? choiceImages[selectedImageIndex] : null;

        if (cutEffectPrefab != null && cutEffectSpawnPoints != null && choiceIndex >= 0 && choiceIndex < cutEffectSpawnPoints.Length)
        {
            Transform effectSpawn = cutEffectSpawnPoints[choiceIndex];
            if (effectSpawn != null)
            {
                GameObject effect = Instantiate(cutEffectPrefab, effectSpawn);
                effect.transform.localPosition = Vector3.zero;
                effect.transform.localScale = Vector3.one;
                Destroy(effect, 2.2f);
            }
        }

        WiseSituation current = situations[currentSituationIndex];
        int selectedCost = choiceIndex == 0 ? current.cost1 : current.cost2;
        ApplyChoiceSelection(selectedCost);

        // Pass condition: budget left should be in target range (optimal play leaves ~280)
        // Acceptable range: 160-300 (close to optimal)
        bool passed = currentBudget >= 160 && currentBudget <= 340;

        yield return AnimateLineCut(choiceIndex, selectedChoiceImage);

        if (currentSituationIndex >= situations.Count)
        {
            EndMiniGame(passed);
            yield break;
        }

        ShowSituation();
        ResetScissor();
        canCutLine = true;
    }

    IEnumerator AnimateLineCut(int choiceIndex, Image selectedChoiceImage)
    {
        if (choiceIndex < 0 || choiceIndex >= lineUncutSlots.Length)
            yield break;

        GameObject uncutLine = lineUncutSlots[choiceIndex];
        GameObject cutLine = lineCutSlots[choiceIndex];
        RectTransform cutRect = cutLine != null ? cutLine.GetComponent<RectTransform>() : null;
        Vector3 originalCutPosition = cutRect != null ? cutRect.localPosition : Vector3.zero;

        if (uncutLine != null)
            uncutLine.SetActive(false);
        if (cutLine != null)
            cutLine.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (cutLine != null)
        {
            cutLine.SetActive(false);
            if (cutRect != null)
                cutRect.localPosition = originalCutPosition;
        }

        if (selectedChoiceImage != null)
        {
            selectedChoiceImage.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimateLineUp(RectTransform target, Vector3 startPosition, float duration)
    {
        if (target == null)
            yield break;

        Vector3 endPosition = startPosition + new Vector3(0f, 120f, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        target.localPosition = endPosition;
    }

    IEnumerator AnimateChoiceImageUp(Image image, float duration)
    {
        if (image == null)
            yield break;

        RectTransform rect = image.GetComponent<RectTransform>();
        if (rect == null)
            yield break;

        Vector3 startPosition = rect.localPosition;
        Vector3 endPosition = startPosition + new Vector3(0f, 120f, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        rect.localPosition = endPosition;
    }

    IEnumerator JitterChoiceButtonsCoroutine()
    {
        if (choiceButton1Rect == null || choiceButton2Rect == null)
            yield break;

        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(8f, 0f, elapsed / duration);
            float offset = Mathf.Sin(elapsed * 20f) * strength;
            float pulse = 1f + Mathf.Sin(elapsed * 12f) * Mathf.Lerp(0.08f, 0f, elapsed / duration);

            choiceButton1Rect.anchoredPosition = choiceButton1StartPos + new Vector2(-offset, 0f);
            choiceButton2Rect.anchoredPosition = choiceButton2StartPos + new Vector2(offset, 0f);
            choiceButton1Rect.localScale = choiceButton1StartScale * pulse;
            choiceButton2Rect.localScale = choiceButton2StartScale * pulse;

            yield return null;
        }

        ResetButtonPositions();
        buttonJitterCoroutine = null;
    }

    void StartChoiceButtonJitter()
    {
        if (buttonJitterCoroutine != null)
        {
            StopCoroutine(buttonJitterCoroutine);
            ResetButtonPositions();
        }

        buttonJitterCoroutine = StartCoroutine(JitterChoiceButtonsCoroutine());
    }

    void ResetButtonPositions()
    {
        if (choiceButton1Rect != null)
        {
            choiceButton1Rect.anchoredPosition = choiceButton1StartPos;
            choiceButton1Rect.localScale = choiceButton1StartScale;
        }

        if (choiceButton2Rect != null)
        {
            choiceButton2Rect.anchoredPosition = choiceButton2StartPos;
            choiceButton2Rect.localScale = choiceButton2StartScale;
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
        if (startInstructionText != null)
            startInstructionText.gameObject.SetActive(true);

        if (currentScissor != null)
        {
            Destroy(currentScissor);
            currentScissor = null;
        }

        gameStarted = false;
        canCutLine = false;
        firstChoiceMade = false;
        // stop and hide timer
        timerRunning = false;
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }
        if (timerText != null)
            timerText.gameObject.SetActive(false);
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
        ShowStartScreen();
    }

    private IEnumerator GameTimer()
    {
        while (timerRunning && timeRemaining > 0f)
        {
            if (!timerPaused)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining < 0f) timeRemaining = 0f;
                if (timerText != null)
                    timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
            }
            yield return null;
        }

        timerRunning = false;
        gameTimerCoroutine = null;

        if (timeRemaining <= 0f && gameStarted)
        {
            EndMiniGame(false);
        }
    }

    void EndMiniGame(bool passed)
    {
        gameStarted = false;
        // stop timer
        timerRunning = false;
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (buttonJitterCoroutine != null)
        {
            StopCoroutine(buttonJitterCoroutine);
            buttonJitterCoroutine = null;
            ResetButtonPositions();
        }

        if (quizPanel != null)
            quizPanel.SetActive(false);

        choiceButton1.interactable = false;
        choiceButton2.interactable = false;

        if (currentScissor != null)
        {
            Destroy(currentScissor);
            currentScissor = null;
        }

        if (retryButton != null)
            retryButton.gameObject.SetActive(!passed);

        if (scorePanel != null)
        {
            string title, comment;
            
            if (passed)
            {
                title = $"{currentBudget} / {startingBudget}";
                
                // Detailed feedback based on how well they did
                if (currentBudget >= 260)
                    comment = "Napakagaling! Gumawa ka ng mahusay na mga pagpipilian. Napanatili mo ang halos 280 pesos. Ang tunay na pagkonsumo ay hindi lamang bumili, kundi pag-isipang mabuti kung ano ang kailangan at kung ano ang gusto.";
                else if (currentBudget >= 220)
                    comment = "Maganda ang iyong pagpili! Naiwan mo ang 220+ pesos. Patuloy na pag-isipan kung aling bagay ay tunay na kailangan at kung saan mas makakamura nang hindi siyang bumagsak ang kalidad.";
                else if (currentBudget >= 180)
                    comment = "Okay! Lumampas ka sa hamon. Minsan, ang mamahaling presyo ay sulit kung matibay at matagal tumatagal. Subukan muli at harapin kung alin ang mas importante: ang presyo o ang kalidad?";
                else
                    comment = "Pumasa ka ngunit kailangan mong mas maging matalinong mamimili. Pag-aralan kung paano makakatipid ng mas maraming pera sa pamamagitan ng mas matalinong pagpili.";
            }
            else
            {
                title = $"Bigo: {currentBudget} / {startingBudget}";
                comment = $"Natipid mo lang ang {currentBudget} pesos. Napakababang ipon! Siguraduhin na pumipili ka ng mas mura at mas praktikal na mga bagay. Hindi lahat ng mahal ay kailangan, at hindi lahat ng mura ay pangmatagalan.";
            }
            
            scorePanel.ShowScore(title, comment);
        }

        if (!passed)
        {
            if (startPanel != null)
                startPanel.SetActive(false);

            return;
        }

        pointsReward = 0;
        int xpReward = 0;

        // Reward tiers based on proximity to optimal budget (280 left)
        if (currentBudget >= 260) { pointsReward = 100; xpReward = 12; }        // Excellent - made mostly wise choices
        else if (currentBudget >= 220) { pointsReward = 80; xpReward = 10; }    // Very good
        else if (currentBudget >= 180) { pointsReward = 60; xpReward = 8; }     // Good
        else if (currentBudget >= 160) { pointsReward = 40; xpReward = 6; }     // Acceptable - passed but room for improvement

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

        GameManager.Instance.MarkMiniQuiz1cCompleted();
        TaskManager.CheckMiniGamesCompletion();
        if (ProfileManager.Instance != null && ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.miniQuiz1cCompleted = true;
            ProfileManager.Instance.UpdateProgressBars();
            ProfileManager.Instance.UpdateBadges();
        }
        ProfileManager.Instance.LoadProfile();

        if (this.gameObject != null)
            this.gameObject.SetActive(false);
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
        // Pause the game timer for a short duration using unscaled time
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
public class WiseSituation
{
    public string description;
    public string choice1;
    public int cost1;
    public string choice2;
    public int cost2;

    public WiseSituation(string desc, string c1, int cost1, string c2, int cost2)
    {
        description = desc;
        choice1 = c1;
        this.cost1 = cost1;
        choice2 = c2;
        this.cost2 = cost2;
    }
}
