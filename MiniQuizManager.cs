using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniQuizManager : MonoBehaviour
{
    public static MiniQuizManager Instance;

    [Header("UI Elements")]
    public GameObject quizPanel;
    public Image itemImage;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI itemNameText;
    public GameObject[] heroPrefabs;
    public string[] heroNames;
    public Transform heroSpawnPoint;
    public GameObject item1PrefabDisplay;
    public GameObject item2PrefabDisplay;
    public Transform item1CountTextTransform;
    public Transform item2CountTextTransform;
    public TextMeshProUGUI performanceText;

    [Header("Swipe Targets")]
    public RectTransform needZone;
    public RectTransform wantZone;
    public float swipeThreshold = 100f;   // use <= 1 for fraction of screen width, > 1 for pixels
    public float swipeMoveDistance = 350f; // use <= 1 for fraction of screen width, > 1 for pixels
    public float swipeAnimationDuration = 0.4f;
    public float swipeBounceHeight = 40f;
    public float postZoneFallDistance = 30f; // how much below zone the item falls before disappearing

    [Header("Score Panel")]
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI pointsText;     // Points earned display
    public TextMeshProUGUI explanationText;
    public TextMeshProUGUI timerText;
    public Button exitButton;
    public Transform retryButton;
    public Transform item1ButtonTransform;
    public Transform item2ButtonTransform;
    public Button item1Button;
    public Button item2Button;

    [Header("Points Animation")]
    public Color pointsStartColor = Color.yellow;
    public Color pointsEndColor = Color.white;
    public float pointsAnimationDuration = 1.5f;
    public bool pointsPulseLoop = true;

    [Header("Timer")]
    public float quizTimeSeconds = 45f;
    private float timeRemaining;
    private bool timerRunning = false;

    [Header("Start Screen")]
    public Transform instructionsImageTransform;
    public Transform startButtonTransform;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI countdownText;
    public string startInstructionsText = "Pindutin ang button upang simulan ang hamon. Isaayos ang bawat bagay sa kategoryang Pangangailangan o Kagustuhan sa pamamagitan ng pag-drag nito sa tamang lugar.";
    public float instructionsTypingSpeed = 0.04f;

    private TextMeshProUGUI item1CountTextUI;
    private TextMeshProUGUI item2CountTextUI;
    private Button startButton;
    private Coroutine countdownCoroutine;
    private Coroutine typingCoroutine;
    private Coroutine pauseTimerCoroutine;
    private Coroutine item2BonusCoroutine;
    private Coroutine prefabDisplayCoroutine;
    private GameObject activeHeroInstance;
    private GameObject currentPrefabDisplayInstance;
    private bool timerPaused = false;
    private int scoreMultiplier = 1;
    private int item1Count = 0;
    private int item2Count = 0;
    
    [Header("Quiz Items")]
    public Sprite[] items;           // Sprites for quiz
    public bool[] correctAnswers;    // true = Need, false = Want
    public string[] itemNames;       // Names of items (Milk, PS5, etc.)
    public string[] itemExplanations; // Explanation per item

    private int currentIndex = 0;
    private int score = 0;
    private int pointsReward = 0;   // Track points earned
    private Coroutine pointsColorCoroutine;  // Track points animation

    private Vector2 itemStartAnchoredPos;
    private Vector2 pointerDownPosition;
    private Vector2 pointerOffsetLocal;
    private RectTransform itemParentRect;
    private bool isPointerDown = false;
    private bool isSwipeProcessing = false;

    void Awake()
    {
        Instance = this;
        quizPanel.SetActive(false);
        scorePanel.SetActive(false);
        feedbackText.text = "";
        exitButton.onClick.AddListener(CloseScorePanel);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
            timerText.text = FormatTime(quizTimeSeconds);
        }

        if (itemImage != null)
            itemStartAnchoredPos = itemImage.rectTransform.anchoredPosition;
        if (itemImage != null && itemImage.rectTransform.parent != null)
            itemParentRect = itemImage.rectTransform.parent as RectTransform;

        if (startButtonTransform != null)
            startButton = startButtonTransform.GetComponent<Button>();

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

        if (item1CountTextTransform != null)
            item1CountTextUI = item1CountTextTransform.GetComponent<TextMeshProUGUI>();
        if (item2CountTextTransform != null)
            item2CountTextUI = item2CountTextTransform.GetComponent<TextMeshProUGUI>();

        LoadActiveUserItems();
        ApplyHeroPrefabFromActiveUser();
        UpdateItemButtonStates();
        HideStartUI();
    }

    public void StartQuiz()
    {
        HideStartUI();
        currentIndex = 0;
        score = 0;
        scoreMultiplier = 1;
        timerPaused = false;
        timeRemaining = quizTimeSeconds;
        timerRunning = true;

        LoadActiveUserItems();
        UpdateItemButtonStates();
        ApplyHeroPrefabFromActiveUser();

        quizPanel.SetActive(true);
        scorePanel.SetActive(false);
        ShowItem();

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = FormatTime(timeRemaining);
        }
    }

    void ShowItem()
    {
        feedbackText.text = "";
        isSwipeProcessing = false;

        if (currentIndex < items.Length)
        {
            if (itemNameText != null)
            {
                itemNameText.text = itemNames[currentIndex]; // <-- Add this
            }

            itemImage.sprite = items[currentIndex];
            if (itemImage != null)
                itemImage.gameObject.SetActive(true);
            if (itemImage != null)
            {
                itemImage.rectTransform.anchoredPosition = itemStartAnchoredPos;
                itemImage.transform.SetAsLastSibling();
            }
        }
        else
        {
            EndQuiz();
        }
    }

    public void OnStartButtonClicked()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (startButton != null)
            startButton.interactable = false;

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(StartCountdownThenBegin());
    }

    System.Collections.IEnumerator StartCountdownThenBegin()
    {
        SetTextActive(countdownText, true);
        SetTextActive(instructionsText, false);  // Hide instructions so countdown is visible

        if (countdownText != null)
        {
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

    public void ShowStartUI()
    {
        if (quizPanel != null)
            quizPanel.SetActive(true);

        SetTransformActive(instructionsImageTransform, true);
        SetTransformActive(startButtonTransform, true);
        SetTextActive(instructionsText, true);
        SetTextActive(countdownText, false);

        if (startButton != null)
            startButton.interactable = true;

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        StartTypingInstructions();
    }

    void StartTypingInstructions()
    {
        if (instructionsText == null)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeInstructionsText(startInstructionsText));
    }

    System.Collections.IEnumerator TypeInstructionsText(string text)
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
    }

    string FormatTime(float seconds)
    {
        int wholeSeconds = Mathf.CeilToInt(seconds);
        return wholeSeconds.ToString("00");
    }

    public void HideStartUI()
    {
        SetTransformActive(instructionsImageTransform, false);
        SetTransformActive(startButtonTransform, false);
        SetTextActive(instructionsText, false);
        SetTextActive(countdownText, false);
    }

    void SetTransformActive(Transform t, bool active)
    {
        if (t != null)
            t.gameObject.SetActive(active);
    }

    void SetTextActive(TextMeshProUGUI textObject, bool active)
    {
        if (textObject != null)
            textObject.gameObject.SetActive(active);
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

        // Destroy previously spawned hero instance
        if (activeHeroInstance != null)
        {
            Destroy(activeHeroInstance);
            activeHeroInstance = null;
        }

        // Find hero index by name
        int heroIndex = FindHeroIndexByName(heroName);
        if (heroIndex < 0 || heroIndex >= heroPrefabs.Length)
            return;

        // Instantiate hero at spawn point
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
        if (item1Count <= 0 || timerPaused)
            return;

        DeductItemCount(1);
        StartCoroutine(PauseTimerForSeconds(5f));
        StartTyping(performanceText, "Ginamit ang Item1! Hinto muna ang oras ng 5 segundo.", 0.01f);
        ShowPrefabDisplay(item1PrefabDisplay, 1f);
    }

    public void OnItem2Pressed()
    {
        if (item2Count <= 0)
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

        if (prefabDisplayCoroutine != null)
            StopCoroutine(prefabDisplayCoroutine);

        // Destroy previous instance if exists
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

        prefabDisplayCoroutine = StartCoroutine(HidePrefabAfterDelay(currentPrefabDisplayInstance, duration));
    }

    private IEnumerator HidePrefabAfterDelay(GameObject prefabInstance, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (prefabInstance != null)
            Destroy(prefabInstance);
        currentPrefabDisplayInstance = null;
        prefabDisplayCoroutine = null;
    }

    private IEnumerator PauseTimerForSeconds(float seconds)
    {
        timerPaused = true;
        float pausedTime = 0f;
        while (pausedTime < seconds)
        {
            pausedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        timerPaused = false;
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

    private void UpdatePerformanceText()
    {
        if (performanceText == null)
            return;

        // Show different messages based on items processed (currentIndex tracks progress)
        // Messages update every 2 items dragged
        int itemsProcessed = currentIndex;
        
        // Calculate accuracy ratio
        float ratio = (items.Length > 0) ? (float)score / items.Length : 0f;

        // Choose message based on progress and accuracy
        string message = "";
        
        if (itemsProcessed <= 2)
        {
            if (ratio >= 0.8f)
                message = "Ang galing mo! Magaling na Panimula!";
            else if (ratio >= 0.5f)
                message = "Magaling! Patuloy lang!";
            else
                message = "Simula pa lang, kaya mo yan!";
        }
        else if (itemsProcessed <= 4)
        {
            if (ratio >= 0.8f)
                message = "Napakagaling mo! Tuloy-tuloy ang iyong kahusayan!";
            else if (ratio >= 0.5f)
                message = "Okay naman! Pagbutihin pa!";
            else
                message = "Konti na lang, tiyaga!";
        }
        else if (itemsProcessed <= 6)
        {
            if (ratio >= 0.8f)
                message = "Wow! Halos wala nang kapintasan!";
            else if (ratio >= 0.5f)
                message = "Maganda ang pag-unlad mo!";
            else
                message = "Dapat pag-isipan pa ang sagot!";
        }
        else if (itemsProcessed <= 8)
        {
            if (ratio >= 0.8f)
                message = "Sobrang galing! Tapos na!";
            else if (ratio >= 0.5f)
                message = "Malapit na, ituloy mo lang!";
            else
                message = "Huwag susuko, kaya mo!";
        }
        else
        {
            if (ratio >= 0.8f)
                message = "Ang galing mo! Halos walang mali na sagot!";
            else if (ratio >= 0.5f)
                message = "Ayos! Medyo magaling ang iyong laro.";
            else
                message = "Kailangan pa ng practice, subukan muli!";
        }

        performanceText.text = message;
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

    void Update()
    {
        if (timerRunning && !timerPaused)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                timerRunning = false;
                EndQuiz();
            }

            if (timerText != null)
                timerText.text = FormatTime(timeRemaining);
        }

        if (!quizPanel.activeSelf || isSwipeProcessing)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleSwipeInput(touch.position, touch.phase == TouchPhase.Began, touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary, touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled);
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                HandleSwipeInput(Input.mousePosition, true, false, false);
            else if (Input.GetMouseButton(0))
                HandleSwipeInput(Input.mousePosition, false, true, false);
            else if (Input.GetMouseButtonUp(0))
                HandleSwipeInput(Input.mousePosition, false, false, true);
        }
    }

    void HandleSwipeInput(Vector2 position, bool start, bool move, bool end)
    {
        if (start)
        {
            isPointerDown = true;
            pointerDownPosition = position;
            if (itemImage != null && itemParentRect != null)
            {
                Canvas c = itemImage.canvas;
                Camera cam = c != null ? c.worldCamera : null;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(itemParentRect, position, cam, out Vector2 localPoint);
                pointerOffsetLocal = itemImage.rectTransform.anchoredPosition - localPoint;
            }
        }
        else if (move && isPointerDown)
        {
            if (itemImage != null && itemParentRect != null)
            {
                Canvas c = itemImage.canvas;
                Camera cam = c != null ? c.worldCamera : null;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(itemParentRect, position, cam, out Vector2 localPoint);
                itemImage.rectTransform.anchoredPosition = localPoint + pointerOffsetLocal;
            }
        }
        else if (end && isPointerDown)
        {
            isPointerDown = false;
            Vector2 swipeDelta = position - pointerDownPosition;
            if (Mathf.Abs(swipeDelta.x) >= GetSwipeThresholdPixels() && Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                StartCoroutine(AnimateSwipe(swipeDelta.x < 0));
            }
            else
            {
                ResetItemPosition();
            }
        }
    }

    float GetSwipeThresholdPixels()
    {
        return swipeThreshold <= 1f ? Screen.width * swipeThreshold : swipeThreshold;
    }

    float GetSwipeMoveDistancePixels()
    {
        return swipeMoveDistance <= 1f ? Screen.width * swipeMoveDistance : swipeMoveDistance;
    }

    System.Collections.IEnumerator AnimateSwipe(bool toNeed)
    {
        isSwipeProcessing = true;
        Vector2 startPos = itemImage.rectTransform.anchoredPosition;
        RectTransform targetZone = toNeed ? needZone : wantZone;

        // Prefer moving directly to the zone's anchored position; fall back to calculated move distance
        Vector2 zonePos = (targetZone != null) ? targetZone.anchoredPosition : (itemStartAnchoredPos + (toNeed ? Vector2.left : Vector2.right) * GetSwipeMoveDistancePixels());

        // 1) Move directly from spawn to zone
        float elapsed = 0f;
        while (elapsed < swipeAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / swipeAnimationDuration);
            itemImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, zonePos, t);
            yield return null;
        }

        // 2) Jump up from the zone, then fall a little below the zone
        float jumpUpDuration = swipeAnimationDuration * 0.5f;
        float fallDuration = swipeAnimationDuration * 0.8f;

        // Jump up
        elapsed = 0f;
        Vector2 jumpApex = new Vector2(zonePos.x, zonePos.y + swipeBounceHeight);
        while (elapsed < jumpUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / jumpUpDuration);
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);
            itemImage.rectTransform.anchoredPosition = Vector2.Lerp(zonePos, jumpApex, eased);
            yield return null;
        }

        // Fall down a bit below the zone
        elapsed = 0f;
        Vector2 fallPos = new Vector2(zonePos.x, zonePos.y - postZoneFallDistance);
        while (elapsed < fallDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            float eased = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            itemImage.rectTransform.anchoredPosition = Vector2.Lerp(jumpApex, fallPos, eased);
            yield return null;
        }

        // Hide the item slightly below the zone and register answer
        itemImage.gameObject.SetActive(false);
        CheckAnswer(toNeed);
        isSwipeProcessing = false;
    }

    public void ChooseNeed()
    {
        if (!isSwipeProcessing)
            StartCoroutine(AnimateSwipe(true));
    }

    public void ChooseWant()
    {
        if (!isSwipeProcessing)
            StartCoroutine(AnimateSwipe(false));
    }

    void ResetItemPosition()
    {
        if (itemImage != null)
            itemImage.rectTransform.anchoredPosition = itemStartAnchoredPos;
    }

    // tap effects removed

    void CheckAnswer(bool playerChoice)
    {
        if (currentIndex >= correctAnswers.Length)
        {
            EndQuiz();
            return;
        }

        if (playerChoice == correctAnswers[currentIndex])
        {
            feedbackText.text = "Tama!";
            score += scoreMultiplier;
        }
        else
        {
            feedbackText.text = "Nakuu!";
        }

        UpdatePerformanceText();

        if (currentIndex == items.Length - 1)
        {
            Invoke("EndQuiz", 1f);
        }
        else
        {
            currentIndex++;
            Invoke("ShowItem", 1f);
        }
    }


    void EndQuiz()
    {
        quizPanel.SetActive(false);

        int passThreshold = 6; // Score of 5 or below is a fail
        bool passed = score >= passThreshold;
        if (passed)
        {
            OnQuizCompleted(); // Notify GameManager and ProfileManager about quiz completion
        }

        // Build explanation text
        string explanation = "";
        for (int i = 0; i < items.Length; i++)
        {
            explanation += $"{i + 1}. {itemNames[i]} → {itemExplanations[i]}\n";
        }

        if (passed)
        {
            scoreText.text = $"Tagumpay! Nakakuha ka {score}/{items.Length} na tama.";
        }
        else
        {
            scoreText.text = $"Hindi pumasa, nakuha mo ay {score}/{items.Length} tama lamang. Subukan muli.";
        }
 
        UpdatePerformanceText();
        explanationText.text = explanation;
        timerRunning = false;
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(!passed);
        scorePanel.SetActive(true);
    }


    public void OnQuizCompleted()
    {
        
        // Reward depends on score
        pointsReward = score * 10;
        int xpReward = score * 2;

        if (score == items.Length)
        {
            pointsReward += 20;
            xpReward += 5;
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

        string activeUser = PlayerPrefs.GetString("ActiveUser", "<none>");
        if (GameManager.Instance != null)
        {
            Debug.Log($"MiniQuizManager: Completing quiz for active user '{activeUser}'");

            GameManager.Instance.AddPoints(pointsReward);
            GameManager.Instance.AddXP(xpReward);
            GameManager.Instance.MarkMiniQuiz1Completed();  // update GameManager
            TaskManager.CheckMiniGamesCompletion();
            Debug.Log("MiniQuizManager: MarkMiniQuiz1Completed() called.");
        }
        else
        {
            Debug.LogWarning("MiniQuizManager: GameManager.Instance is null, cannot mark quiz completed.");
        }

        SaveMiniQuiz1CompletionToProfile(activeUser, true);

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();        // sync latest data
            ProfileManager.Instance.UpdateProgressBars(); // update the progress bar
            ProfileManager.Instance.UpdateBadges();       // update any badges
        }
    }

    void SaveMiniQuiz1CompletionToProfile(string activeUser, bool completed)
    {
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("MiniQuizManager: cannot save miniQuiz1 completion because ActiveUser is empty.");
            return;
        }

        if (!completed)
        {
            Debug.Log("MiniQuizManager: Quiz failed, not saving miniQuiz1Completed to profile.");
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
            Debug.LogWarning($"MiniQuizManager: profile file not found, creating new profile for '{activeUser}' at '{path}'");
        }

        profile.miniQuiz1Completed = true;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
        Debug.Log($"MiniQuizManager: Saved miniQuiz1Completed=true for '{activeUser}' to '{path}'");
    }

    void CloseScorePanel()
    {
        scorePanel.SetActive(false);
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        timerRunning = false;
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
        quizPanel.SetActive(true);
        timerRunning = false;
        ShowStartUI();
    }
}
