using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

public class BudgetBalancerManager : MonoBehaviour
{
    public static BudgetBalancerManager Instance;

    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI questionText;

    public GameObject startPanel;
    public Button startButton;
    public TextMeshProUGUI startInstructionText;
    public TextMeshProUGUI countdownText;
    public Button retryButton;

    public Button optionAButton;
    public Button optionBButton;
    public Button optionCButton;
    public GameObject tappedAButton;
    public GameObject tappedBButton;
    public GameObject tappedCButton;

    public ScorePanel scorePanel;
    public TextMeshProUGUI pointsText;     // Points earned display

    [Header("Points Animation")]
    public Color pointsStartColor = Color.yellow;
    public Color pointsEndColor = Color.white;
    public float pointsAnimationDuration = 1.5f;
    public bool pointsPulseLoop = true;

    [Header("Effects")]
    public GameObject hammerPrefab;
    public float hammerDuration = 0.5f;
    public float tappedDuration = 2f;
    public Transform effectParent;

    [Header("Settings")]
    public int totalRounds = 5;
    public int gameDurationSeconds = 45;
    public TextMeshProUGUI timerText;

    [Header("Hero & Items")]
    public GameObject[] heroPrefabs;
    public string[] heroNames;
    public Transform heroSpawnPoint;

    public GameObject item1PrefabDisplay;
    public GameObject item2PrefabDisplay;
    public Button item1Button;
    public Button item2Button;
    private int item1Count = 0;
    private int item2Count = 0;
    public TextMeshProUGUI item1CountTextUI;
    public TextMeshProUGUI item2CountTextUI;
    public TextMeshProUGUI performanceText;

    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private Coroutine typingCoroutine;
    private Coroutine item1PauseCoroutine;
    private Coroutine item2BonusCoroutine;
    private bool timerPaused = false;
    private int scoreMultiplier = 1;

    private int currentRound = 0;
    private int score = 0;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation
    private bool gameStarted = false;
    private float remainingTime = 0f;
    private bool timerRunning = false;
    private Coroutine gameTimerCoroutine;

    private List<BudgetScenario> scenarios = new List<BudgetScenario>();

    void Awake()
    {
        Instance = this;

        if (effectParent == null && quizPanel != null)
            effectParent = quizPanel.transform;

        if (startPanel != null)
            startPanel.SetActive(true);
        if (quizPanel != null)
            quizPanel.SetActive(false);
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryPressed);

        if (startInstructionText != null)
        {
            // store the instruction content and clear the text for typing display when shown
            startInstructionText.text = "";
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

    public void StartMiniGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        score = 0;
        currentRound = 0;
        remainingTime = gameDurationSeconds;

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            UpdateTimerText();
        }

        LoadScenarios();
        quizPanel.SetActive(true);
        ResetOptionButtons();
        ShowScenario();

        if (gameTimerCoroutine != null)
            StopCoroutine(gameTimerCoroutine);
        gameTimerCoroutine = StartCoroutine(GameTimer());
    }

    public void ShowStartScreen()
    {
        if (startPanel != null)
            startPanel.SetActive(true);
        if (quizPanel != null)
            quizPanel.SetActive(true);
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (startButton != null)
            startButton.gameObject.SetActive(true);
        if (startInstructionText != null)
        {
            startInstructionText.gameObject.SetActive(true);
            StartTyping(startInstructionText,
                "Piliin ang tamang pagkonsumo na hindi binabawasan ang iyong pag-iimpok.\nBalansehin ang kita, pag-iimpok, at pagkonsumo para sa matalinong pagba-budget.",
                0.01f);
        }
    }

    /// <summary>
    /// SITUATIONS BASED ON THE LESSON:
    /// "UGNAYAN NG KITA, PAG-IIMPOK, AT PAGKONSUMO"
    ///
    /// - Ang kita ay nagbibigay-daan sa pagkonsumo.
    /// - Ngunit dapat laging may bahagi para sa pag-iimpok.
    /// - Ang tamang pagpili ng gastusin ay nagpapakita kung paano
    ///   binabalanse ng isang tao ang kanyang income at savings.
    ///
    /// LAYUNIN NG GAMES:
    /// Piliin ang gastusing HINDI lalampas sa kakayahang mag-impok.
    /// Nakakakuha ng puntos kapag tama ang balanse ng kita at gastos.
    /// </summary>
    void LoadScenarios()
    {
        scenarios.Clear();

        scenarios.Add(new BudgetScenario(
            500,
            "Si Maria ay may kita na ₱500. Kailangan niyang mag-impok ng ₱100, ngunit gusto rin niyang bumili ng konsumo kasi gutom siya. Anong pagpipilian ang nagpapakita ng tamang ugnayan ng kita, pag-iimpok, at pagkonsumo?",
            new string[] { "Meryenda ₱200", "Load ₱100", "Laruan ₱450" },
            new int[] { 200, 100, 450 },
            100
        ));

        scenarios.Add(new BudgetScenario(
            300,
            "Si Juan ay kumita ng ₱300 mula sa gawaing bahay. Nais niyang mag-impok ng ₱50 at bumili ng kailangan para sa paaralan. Alin ang nagpapakita ng balanse sa kita, impok, at konsumo?",
            new string[] { "Pampalamig ₱120", "Notebook ₱80", "Poster ₱250" },
            new int[] { 120, 80, 250 },
            50
        ));

        scenarios.Add(new BudgetScenario(
            700,
            "Si Anna ay nakatanggap ng ₱700. Plano niyang mag-impok ng ₱150 at bumili lamang ng kailangan niyang damit. Alin ang nagpapakita ng tamang paglalaan ng kita, impok, at konsumo?",
            new string[] { "Sapatos ₱600", "Blusa ₱300", "Headphones ₱650" },
            new int[] { 600, 300, 650 },
            150
        ));

        scenarios.Add(new BudgetScenario(
            400,
            "May natira si Pedro na ₱400 ngayong linggo. Nais niyang mag-impok ng ₱80 para sa emergency at bumili ng konsumo na makakatulong sa pag-aaral. Alin ang pinakamatipid at matalino?",
            new string[] { "Fast Food ₱180", "Gamit-Pansining ₱120", "Game Skin ₱350" },
            new int[] { 180, 120, 350 },
            80
        ));

        scenarios.Add(new BudgetScenario(
            600,
            "Si Liza ay may ₱600. Nais niyang mag-impok ng ₱120 para sa isang gawain sa paaralan at gumastos nang wasto. Anong pagpili ang nagpapakita ng tamang ugnayan ng kita, impok, at konsumo?",
            new string[] { "Libro ₱250", "Sumbrero ₱300", "Bag ₱580" },
            new int[] { 250, 300, 580 },
            120
        ));

        // Shuffle for variety
        for (int i = 0; i < scenarios.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, scenarios.Count);
            (scenarios[i], scenarios[r]) = (scenarios[r], scenarios[i]);
        }
    }

    /// <summary>
    /// Shows the scenario and buttons for the current round.
    ///
    /// LESSON CONNECTION:
    /// Ito ang bahagi kung saan natetest ang player's ability na
    /// i-balanse ang tatlo:
    ///  - Kita (Income)
    ///  - Pagkonsumo (Expenses)
    ///  - Pag-iimpok (Required Savings)
    ///
    /// Kapag tama ang choice → tama ang pag-manage ng resources.
    /// </summary>
    void ShowScenario()
    {
        if (currentRound >= totalRounds)
        {
            EndMiniGame();
            return;
        }

        var s = scenarios[currentRound];

        incomeText.text = $"Kita: ₱{s.income}";
        if (questionText != null)
            StartTyping(questionText, s.description, 0.01f);

        optionAButton.GetComponentInChildren<TextMeshProUGUI>().text = s.options[0];
        optionBButton.GetComponentInChildren<TextMeshProUGUI>().text = s.options[1];
        optionCButton.GetComponentInChildren<TextMeshProUGUI>().text = s.options[2];

        if (timerText != null)
            UpdateTimerText();

        ResetOptionButtons();

        optionAButton.onClick.RemoveAllListeners();
        optionBButton.onClick.RemoveAllListeners();
        optionCButton.onClick.RemoveAllListeners();

        optionAButton.onClick.AddListener(() => PressOption(optionAButton, tappedAButton, 0));
        optionBButton.onClick.AddListener(() => PressOption(optionBButton, tappedBButton, 1));
        optionCButton.onClick.AddListener(() => PressOption(optionCButton, tappedCButton, 2));
    }

    /// <summary>
    /// SCORING LOGIC BASED ON THE LESSON:
    ///
    /// Correct if:
    ///     Remaining Money = Income - Expense
    /// is STILL enough to meet the Required Savings.
    ///
    /// Ito mismo ang ugnayan:
    ///     Kita → nagpapahintulot sa paggasta
    ///     Gastos → binabawasan ang kita
    ///     Pag-iimpok → dapat laging may natitira
    ///
    /// Kung ang gastos ay masyadong mataas → bumabagsak ang savings.
    /// </summary>
    void PressOption(Button button, GameObject tappedButtonObject, int index)
    {
        if (button == null)
            return;

        ShowHammerEffect(button.transform);

        button.gameObject.SetActive(false);
        if (tappedButtonObject != null)
            tappedButtonObject.SetActive(true);

        StartCoroutine(ContinueAfterTapped(button.gameObject, tappedButtonObject, index));
    }

    System.Collections.IEnumerator ContinueAfterTapped(GameObject regularButton, GameObject tappedButtonObject, int index)
    {
        yield return new WaitForSeconds(tappedDuration);

        if (tappedButtonObject != null)
            tappedButtonObject.SetActive(false);
        if (regularButton != null)
            regularButton.SetActive(true);

        SelectOption(index);
    }

    void SelectOption(int index)
    {
        var s = scenarios[currentRound];
        int expense = s.expenseAmounts[index];

        int remaining = s.income - expense;
        bool correct = remaining >= s.requiredSavings;

        if (correct)
            score += 20 * scoreMultiplier;

        currentRound++;
        ShowScenario();
    }

    void ResetOptionButtons()
    {
        if (optionAButton != null)
            optionAButton.gameObject.SetActive(true);
        if (optionBButton != null)
            optionBButton.gameObject.SetActive(true);
        if (optionCButton != null)
            optionCButton.gameObject.SetActive(true);

        if (tappedAButton != null)
            tappedAButton.SetActive(false);
        if (tappedBButton != null)
            tappedBButton.SetActive(false);
        if (tappedCButton != null)
            tappedCButton.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        if (startInstructionText != null)
            startInstructionText.gameObject.SetActive(false);

        StartCoroutine(CountdownAndStart());
    }

    IEnumerator CountdownAndStart()
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

        StartMiniGame();
    }

    void OnRetryPressed()
    {
        if (scorePanel != null && scorePanel.gameObject != null)
            scorePanel.gameObject.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (gameTimerCoroutine != null)
            StopCoroutine(gameTimerCoroutine);

        ShowStartScreen();
        gameStarted = false;
        timerRunning = false;
        score = 0;
        currentRound = 0;
    }

    void ShowHammerEffect(Transform buttonTransform)
    {
        if (hammerPrefab == null) return;

        Transform parent = effectParent != null ? effectParent : quizPanel != null ? quizPanel.transform : null;
        if (parent == null)
            parent = this.transform;

        GameObject effect = Instantiate(hammerPrefab, parent);
        RectTransform effectRect = effect.GetComponent<RectTransform>();
        RectTransform buttonRect = buttonTransform as RectTransform;

        if (effectRect != null && buttonRect != null && effectRect.parent == buttonRect.parent)
        {
            effectRect.anchoredPosition = buttonRect.anchoredPosition;
        }
        else if (effectRect != null && buttonRect != null)
        {
            effectRect.position = buttonRect.position;
        }

        Destroy(effect, hammerDuration);
    }

    /// <summary>
    /// FINAL FEEDBACK:
    ///
    /// Lahat ng comments ay nakabase sa:
    /// "Mas nauunawaan ng mag-aaral ang koneksyon ng kita,
    ///  pag-iimpok, at pagkonsumo kapag marunong siyang pumili
    ///  ng gastusin na hindi sumisira sa kanyang savings."
    /// </summary>
    void EndMiniGame()
    {
        quizPanel.SetActive(false);
        ResetOptionButtons();

        int maxScore = totalRounds * 20;
        bool passed = score >= Mathf.CeilToInt(maxScore * 0.6f);

        string comment =
            passed ? "Mahusay! Naipakita mo ang tamang balanse ng kita, pag-iimpok, at pagkonsumo!" :
            score >= Mathf.CeilToInt(maxScore * 0.4f) ? "Magaling na effort! Subukang pagbutihin pa ang balanse ng kita at pag-iimpok." :
            "Kaya mo 'yan! Subukang piliin ang gastusin na hindi sumisira sa iyong ipon.";

        string title = passed ? $"{score} / {maxScore}" : $"Bigo: {score} / {maxScore}";
        scorePanel.ShowScore(title, comment);

        if (passed)
        {
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
            GameManager.Instance.AddXP(score / 5);
            GameManager.Instance.MarkMiniQuiz3bCompleted();
            TaskManager.CheckMiniGamesCompletion();
        }
        else if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
        }

        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (gameTimerCoroutine != null)
            StopCoroutine(gameTimerCoroutine);
        timerRunning = false;

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();
            ProfileManager.Instance.UpdateProgressBars();
        }
        gameStarted = false;
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
        if (item1Count <= 0 || !gameStarted)
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
        if (item2Count <= 0 || !gameStarted)
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

    private IEnumerator GameTimer()
    {
        timerRunning = true;
        while (remainingTime > 0f && gameStarted)
        {
            if (!timerPaused)
            {
                remainingTime -= Time.deltaTime;
                if (remainingTime < 0f)
                    remainingTime = 0f;
            }

            UpdateTimerText();
            yield return null;
        }

        timerRunning = false;
        if (remainingTime <= 0f && gameStarted)
        {
            EndMiniGame();
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int secs = Mathf.CeilToInt(remainingTime);
        timerText.text = $"Oras: {secs}";
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
        if (pointsPulseLoop && scorePanel != null && scorePanel.gameObject.activeSelf)
        {
            pointsColorCoroutine = StartCoroutine(AnimatePointsTextColor());
        }
    }
}

[System.Serializable]
public class BudgetScenario
{
    public int income;
    public string description;
    public string[] options;
    public int[] expenseAmounts;
    public int requiredSavings;

    public BudgetScenario(int inc, string desc, string[] opts, int[] costs, int save)
    {
        income = inc;
        description = desc;
        options = opts;
        expenseAmounts = costs;
        requiredSavings = save;
    }
}
