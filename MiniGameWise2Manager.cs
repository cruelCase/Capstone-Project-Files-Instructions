using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGameWise2Manager : MonoBehaviour
{
    public static MiniGameWise2Manager Instance;

    [Header("UI References")]
    public GameObject quizPanel;
    public GameObject startPanel;
    public Button startButton;
    public Button retryButton;
    public TextMeshProUGUI startInstructionText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI situationText;
    public Button choiceButton1;
    public Button choiceButton2;
    public TextMeshProUGUI choiceText1;
    public TextMeshProUGUI choiceText2;
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

    [Header("Images (2 per situation, total 10)")]
    public Image[] choiceImages = new Image[10];

    [Header("Settings")]
    public float countdownDuration = 3f;
    public float gameTimeSeconds = 45f;
    public TextMeshProUGUI timerText;

    private int currentSituationIndex = 0;
    private int score = 0;
    private int pointsReward = 0;   // Track points earned
    private int wrongAnswerCount = 0;  // Track wrong answers for fail condition
    private Coroutine pointsColorCoroutine;  // Track points animation
    private List<WaisSituation> situations = new List<WaisSituation>();
    private List<string> collectedLessons = new List<string>();
    private bool gameStarted = false;
    private Coroutine buttonWaveCoroutine;
    private Coroutine answerFeedbackCoroutine;
    private Coroutine countdownCoroutine;
    private Coroutine gameTimerCoroutine;
    private float timeRemaining = 0f;
    private bool timerRunning = false;
    private bool timerPaused = false;
    private RectTransform choiceButton1Rect;
    private RectTransform choiceButton2Rect;
    private Vector2 choiceButton1StartPos;
    private Vector2 choiceButton2StartPos;
    private Vector3 choiceButton1StartScale;
    private Vector3 choiceButton2StartScale;
    private Image choiceButton1Image;
    private Image choiceButton2Image;
    private Color choiceButton1StartColor;
    private Color choiceButton2StartColor;
    // hero/item private state
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private Coroutine typingCoroutine;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;
    private int item1Count = 0;
    private int item2Count = 0;
    private int scoreMultiplier = 1;

    void Awake()
    {
        Instance = this;

        if (choiceButton1 != null)
        {
            choiceButton1Rect = choiceButton1.GetComponent<RectTransform>();
            choiceButton1StartScale = choiceButton1Rect != null ? choiceButton1Rect.localScale : Vector3.one;
            choiceButton1StartPos = choiceButton1Rect != null ? choiceButton1Rect.anchoredPosition : Vector2.zero;
            choiceButton1Image = choiceButton1.GetComponent<Image>();
            if (choiceButton1Image != null)
                choiceButton1StartColor = choiceButton1Image.color;
            if (choiceText1 == null)
                choiceText1 = choiceButton1.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (choiceButton2 != null)
        {
            choiceButton2Rect = choiceButton2.GetComponent<RectTransform>();
            choiceButton2StartScale = choiceButton2Rect != null ? choiceButton2Rect.localScale : Vector3.one;
            choiceButton2StartPos = choiceButton2Rect != null ? choiceButton2Rect.anchoredPosition : Vector2.zero;
            choiceButton2Image = choiceButton2.GetComponent<Image>();
            if (choiceButton2Image != null)
                choiceButton2StartColor = choiceButton2Image.color;
            if (choiceText2 == null)
                choiceText2 = choiceButton2.GetComponentInChildren<TextMeshProUGUI>();
        }

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

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryPressed);

        if (startInstructionText != null)
        {
            startInstructionText.text = "Kilalanin ang mga salik na nakakaapekto sa pagkonsumo. Pumili ng tamang sagot para makakuha ng puntos.";
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
        
        // Stop timer
        timerRunning = false;
        timerPaused = false;
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        gameStarted = false;
        currentSituationIndex = 0;
        score = 0;
        wrongAnswerCount = 0;
        collectedLessons.Clear();
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

    public void StartMiniGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        currentSituationIndex = 0;
        score = 0;
        wrongAnswerCount = 0;
        collectedLessons.Clear();

        LoadSituations();
        ShowSituation();

        if (quizPanel != null)
            quizPanel.SetActive(true);

        choiceButton1.gameObject.SetActive(true);
        choiceButton2.gameObject.SetActive(true);

        // Start game timer
        timeRemaining = gameTimeSeconds;
        timerRunning = true;
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

        // Situation 1: Kita
        situations.Add(new WaisSituation(
            "Si Juan ay gustong bumili ng meryenda. Alin ang nakakaapekto sa kanyang konsumo?",
            "Lingguhang allowance (Kita)", 
            "Kulay ng balot ng meryenda", 
            0, 1,
            "Ang kita ay nakakaapekto sa konsumo dahil kapag mas maraming pera ang mayroon si Juan, mas marami siyang mabibili."
        ));

        // Situation 2: Presyo
        situations.Add(new WaisSituation(
            "Pinipili ni Maria ang isang laruan. Alin sa mga ito ang salik sa kanyang konsumo?",
            "Presyo ng laruan", 
            "Paboritong kulay ng kanyang backpack", 
            2, 3,
            "Ang presyo ay nakakaapekto sa konsumo dahil kapag mataas ang presyo, maaaring limitado ang mabibili ni Maria."
        ));

        // Situation 3: Pangangailangan vs Pagnanais
        situations.Add(new WaisSituation(
            "Nagdesisyon si Anna kung ano ang kakainin sa tanghalian. Alin ang mas nakakaapekto sa kanyang konsumo?",
            "Poster sa dingding",
            "Gutóm / pangangailangan ng nutrisyon",
            5, 4,
            "Ang pangangailangan laban sa pagnanasa ay nakakaapekto sa konsumo dahil inuuna ng tao ang mahahalagang pangangailangan."
        ));

        // Situation 4: Availability
        situations.Add(new WaisSituation(
            "Kailangan ni Pedro ng notebook. Alin sa mga ito ang maaaring makaapekto sa kanyang desisyon sa konsumo?",
            "Isa lamang notebook ang available sa tindahan",
            "Kulay ng ballpen sa kanyang lapisera",
            6, 7,
            "Ang pagkakaroon o kakulangan ng supply ay nakakaapekto sa konsumo dahil kapag limitado ang stock, nababawasan ang pagpipilian."
        ));

        // Situation 5: Impluwensiya ng Lipunan
        situations.Add(new WaisSituation(
            "Inanyayahan kang manood ng sine ng mga kaibigan. Alin ang salik na nakakaapekto sa iyong konsumo?",
            "Impluwensiya ng kaibigan / pagsama sa sinehan",
            "Panahon sa labas",
            8, 9,
            "Ang impluwensiya ng lipunan ay nakakaapekto sa konsumo dahil naaapektuhan ang tao ng kaniyang mga kaibigan o grupo."
        ));
    }

    void ShowSituation()
    {
        if (currentSituationIndex >= situations.Count)
        {
            EndMiniGame();
            return;
        }

        WaisSituation s = situations[currentSituationIndex];

        situationText.text = s.description;

        // Deactivate all images first
        for (int i = 0; i < choiceImages.Length; i++)
            choiceImages[i].gameObject.SetActive(false);

        // Activate only current situation images
        choiceImages[s.correctImageIndex].gameObject.SetActive(true);
        choiceImages[s.wrongImageIndex].gameObject.SetActive(true);

        // Remove old listeners
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();

        // Update button labels
        if (choiceText1 != null)
            choiceText1.text = s.correctChoice;
        if (choiceText2 != null)
            choiceText2.text = s.wrongChoice;

        // Reset style for new situation
        ResetChoiceButtonState();

        // Assign listeners
        choiceButton1.onClick.AddListener(() => StartCoroutine(OnChoiceSelectedRoutine(true, s.lesson, choiceButton1)));
        choiceButton2.onClick.AddListener(() => StartCoroutine(OnChoiceSelectedRoutine(false, s.lesson, choiceButton2)));

        // Add a subtle wave animation when the new choice appears
        StartButtonWave();
    }

    IEnumerator OnChoiceSelectedRoutine(bool correct, string lesson, Button selectedButton)
    {
        if (answerFeedbackCoroutine != null)
        {
            StopCoroutine(answerFeedbackCoroutine);
            answerFeedbackCoroutine = null;
        }

        answerFeedbackCoroutine = StartCoroutine(PlayAnswerFeedback(correct, selectedButton));

        if (correct)
        {
            score += 20;
            collectedLessons.Add(lesson); // store lesson only if correct
        }
        else
        {
            wrongAnswerCount++;
        }

        yield return new WaitForSeconds(0.35f);

        currentSituationIndex++;

        if (currentSituationIndex >= situations.Count)
        {
            EndMiniGame();
            yield break;
        }

        ShowSituation();
    }

    IEnumerator PlayAnswerFeedback(bool correct, Button selectedButton)
    {
        if (selectedButton == null)
            yield break;

        var selectedRect = selectedButton.GetComponent<RectTransform>();
        var selectedImage = selectedButton.GetComponent<Image>();
        var otherButton = selectedButton == choiceButton1 ? choiceButton2 : choiceButton1;
        var otherImage = otherButton != null ? otherButton.GetComponent<Image>() : null;

        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float scaleAmount = 1f + Mathf.Sin(progress * Mathf.PI) * 0.12f;
            float tint = Mathf.Sin(progress * Mathf.PI);

            if (selectedRect != null)
                selectedRect.localScale = Vector3.one * scaleAmount;

            if (selectedImage != null)
                selectedImage.color = Color.Lerp(
                    selectedButton == choiceButton1 ? choiceButton1StartColor : choiceButton2StartColor,
                    correct ? Color.green : Color.red,
                    tint);

            if (!correct && otherImage != null)
            {
                otherImage.color = Color.Lerp(
                    otherButton == choiceButton1 ? choiceButton1StartColor : choiceButton2StartColor,
                    Color.green * 0.6f + Color.white * 0.4f,
                    tint * 0.5f);
            }

            yield return null;
        }

        ResetChoiceButtonState();
        answerFeedbackCoroutine = null;
    }

    void ResetChoiceButtonState()
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

        if (choiceButton1Image != null)
            choiceButton1Image.color = choiceButton1StartColor;
        if (choiceButton2Image != null)
            choiceButton2Image.color = choiceButton2StartColor;
    }

    IEnumerator ButtonWaveCoroutine()
    {
        if (choiceButton1Rect == null || choiceButton2Rect == null)
            yield break;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float wave = Mathf.Sin(elapsed * 8f) * 6f;
            float pulse = 1f + Mathf.Sin(elapsed * 6f) * 0.03f;

            choiceButton1Rect.anchoredPosition = choiceButton1StartPos + new Vector2(0f, wave);
            choiceButton2Rect.anchoredPosition = choiceButton2StartPos + new Vector2(0f, -wave);
            choiceButton1Rect.localScale = choiceButton1StartScale * pulse;
            choiceButton2Rect.localScale = choiceButton2StartScale * pulse;

            yield return null;
        }

        ResetChoiceButtonState();
        buttonWaveCoroutine = null;
    }

    void StartButtonWave()
    {
        if (buttonWaveCoroutine != null)
        {
            StopCoroutine(buttonWaveCoroutine);
            buttonWaveCoroutine = null;
        }

        ResetChoiceButtonState();
        buttonWaveCoroutine = StartCoroutine(ButtonWaveCoroutine());
    }

    void EndMiniGame()
    {
        gameStarted = false;

        // Stop timer
        timerRunning = false;
        timerPaused = false;
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(false);

        foreach (var img in choiceImages)
            img.gameObject.SetActive(false);

        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);

        bool passed = wrongAnswerCount < 2 && score >= 60;  // Fail if 2+ wrongs OR not enough points

        // Combine all lessons
        string lessonText = collectedLessons.Count > 0
            ? string.Join("\n\n", collectedLessons)
            : "Hindi ka nakapili ng kahit isang tamang sagot. Tandaan ang mga salik na nakakaapekto sa konsumo!";

        string title = passed ? $"{score} / {situations.Count * 20}" : $"Bigo: {score} / {situations.Count * 20}";
        
        if (scorePanel != null)
            scorePanel.ShowScore(title, lessonText);

        if (retryButton != null)
            retryButton.gameObject.SetActive(!passed);

        if (!passed)
            return;

        pointsReward = score;
        
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
        GameManager.Instance.AddXP(pointsReward / 5);
        GameManager.Instance.MarkMiniQuiz2cCompleted();
        TaskManager.CheckMiniGamesCompletion();
        if (ProfileManager.Instance != null && ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.miniQuiz2cCompleted = true;
            ProfileManager.Instance.UpdateProgressBars();
            ProfileManager.Instance.UpdateBadges();
        }
        ProfileManager.Instance.LoadProfile();
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
            EndMiniGame();
        }
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
public class WaisSituation
{
    public string description;
    public string correctChoice;
    public string wrongChoice;
    public int correctImageIndex;
    public int wrongImageIndex;
    public string lesson;

    public WaisSituation(string desc, string correct, string wrong, int correctImg, int wrongImg, string lesson)
    {
        description = desc;
        correctChoice = correct;
        wrongChoice = wrong;
        correctImageIndex = correctImg;
        wrongImageIndex = wrongImg;
        this.lesson = lesson;
    }
}
