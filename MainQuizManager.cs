using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class QuestionUI
{
    public GameObject panel;           // The parent panel (Number1…Number15)
    public TMP_Text questionText;      // Question text
    public Button[] answerButtons;     // Buttons (4 or 2)
    public string[] answerTexts;       // Text for each button
    public int correctIndex;           // Index of correct button (0-based)
}

public class MainQuizManager : MonoBehaviour
{
    [Header("Questions Setup")]
    public QuestionUI[] questions;     // 15 questions

    [Header("UI References")]
    public GameObject scorePanel;
    public TMP_Text scoreText;
    public TMP_Text explanationText;
    public TMP_Text questionNumberText;  // Indicator showing current question (e.g., "Question 1/15")

    [Header("Question Indicators")]
    public TMP_Text[] questionIndicators = new TMP_Text[15];  // Array of 15 question number texts
    public Color indicatorDefaultColor = Color.white;
    public Color indicatorActiveColor = Color.green;

    [Header("Answer Selection Colors")]
    public Color selectedAnswerColor = new Color(0.2f, 0.8f, 0.2f, 1f);  // Green for selected
    public Color defaultAnswerColor = Color.white;  // Default button color

    private int[] playerAnswers;       // Stores selected button index per question
    private int currentQuestionIndex = 0;

    private void Start()
    {
        playerAnswers = new int[15];
        for (int i = 0; i < playerAnswers.Length; i++)
        playerAnswers[i] = -1;

        // Check if the player already completed the main quiz
        string username = PlayerPrefs.GetString("ActiveUser", "");
        string path = Path.Combine(Application.persistentDataPath, username + "_profile.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);

            if (data.mainQuizCompleted)
            {
                ShowScorePanelOnly(data);
                return;
            }
        }

        // Initialize all question panels inactive
        foreach (var q in questions)
               

        SetupQuestions();
        ActivateQuestion(0);
    }

    private void SetupQuestions()
    {
        // Question 1-10: 4 buttons
        string[] q1to10 = new string[]
        {
            "May limitadong allowance si Jose ngayong linggo. Kailangan niyang bumili ng pagkain o maglaro. Ano ang kanyang KAILANGAN?",
            "Si Carlo gustong bumili ng bagong video game kahit may bayarin sa kuryente. Alin ang kanyang GUSTO?",
            "Kakailanganin ni Maria ang kagamitan sa paaralan para sa proyekto. Alin ang KAILANGAN?",
            "Nagdiwang si Ana ng kaarawan at gusto niya ng cake. Alin ang kanyang GUSTO?",
            "Kailangan bayaran ang upa sa bahay ngayong buwan. Alin ang nagpapakita ng KAILANGAN?",
            "May natitirang pera si Miguel at gusto niya ng ice cream. Alin ang kanyang GUSTO?",
            "Para sa masustansyang pagkain, alin ang KAILANGAN na dapat bilhin?",
            "May extra pocket money si Liza at gusto niya bumili ng juice. Alin ang kanyang GUSTO?",
            "Si Pedro ay naghahanap ng gatas para sa bata. Alin ang mas kinakailangan (KAILANGAN)?",
            "Naghahanda ka para sa online class at gusto mo ng bagong laptop. Alin ang iyong GUSTO?"
        };

        string[][] answers1to10 = new string[][]
        {
            new string[]{"PS5","Bigas","Candy","Laro"},
            new string[]{"Tubig","Kuryente","Video game","Gatas"},
            new string[]{"Gamit sa paaralan","Ice cream","Kotse laruan","Soda"},
            new string[]{"Gamot","Cake","Tinapay","Tubig"},
            new string[]{"Tsokolate","Upa","Tsokolate gatas","Komiks"},
            new string[]{"Bayarin sa telepono","Ice cream","Tubig","Bigas"},
            new string[]{"Cake","Gulay","Candy","Chips"},
            new string[]{"Upa","Juice","Tubig","Tinapay"},
            new string[]{"Tsokolate","Gatas","Soda","Chips"},
            new string[]{"Tinapay","Tubig","Laptop","Bigas"}
        };

        int[] correct1to10 = new int[]{1,2,0,1,1,1,1,1,1,2};

        // Question 11-15: 2 buttons (Need / Want)
        string[] q11to15 = new string[]
        {
        "Bumili ka ng bagong video game para sa kasiyahan.",

        "Nagbayad ka ng kuryente dahil kailangan ito sa bahay.",

        "Bumili ka ng kendi para sa meryenda.",

        "Nagbayad ka ng upa para sa bahay mo.",

        "Uminom ka ng tubig kapag nauuhaw."
        };

        string[][] answers11to15 = new string[][]
        {
            new string[]{"KAILANGAN","GUSTO"},
            new string[]{"KAILANGAN","GUSTO"},
            new string[]{"KAILANGAN","GUSTO"},
            new string[]{"KAILANGAN","GUSTO"},
            new string[]{"KAILANGAN","GUSTO"}
        };

        int[] correct11to15 = new int[]{1,0,1,0,0};

        // Fill in questions array
        for (int i = 0; i < 10; i++)
        {
            questions[i].questionText.text = q1to10[i];
            questions[i].answerTexts = answers1to10[i];
            questions[i].correctIndex = correct1to10[i];

            for (int j = 0; j < questions[i].answerButtons.Length; j++)
            {
                int capturedIndex = j;
                TMP_Text btnText = questions[i].answerButtons[j].GetComponentInChildren<TMP_Text>();
                btnText.text = answers1to10[i][j];
                questions[i].answerButtons[j].onClick.RemoveAllListeners();
                questions[i].answerButtons[j].onClick.AddListener(() => SelectAnswer(capturedIndex));
            }
        }

        for (int i = 10; i < 15; i++)
        {
            int idx = i - 10;
            questions[i].questionText.text = q11to15[idx];
            questions[i].answerTexts = answers11to15[idx];
            questions[i].correctIndex = correct11to15[idx];

            for (int j = 0; j < questions[i].answerButtons.Length; j++)
            {
                int capturedIndex = j;
                TMP_Text btnText = questions[i].answerButtons[j].GetComponentInChildren<TMP_Text>();
                btnText.text = answers11to15[idx][j];
                questions[i].answerButtons[j].onClick.RemoveAllListeners();
                questions[i].answerButtons[j].onClick.AddListener(() => SelectAnswer(capturedIndex));
            }
        }
    }

    private void ActivateQuestion(int index)
    {
        currentQuestionIndex = index;

        // Activate only the current panel
        for (int i = 0; i < questions.Length; i++)
            questions[i].panel.SetActive(i == index);

        // Update question number indicator
        UpdateQuestionNumberDisplay();
        UpdateQuestionIndicators();
        
        // Restore visual selection if this question was already answered
        RestoreAnswerSelection();
    }

    private void UpdateQuestionNumberDisplay()
    {
        if (questionNumberText != null)
        {
            questionNumberText.text = $"Tanong {currentQuestionIndex + 1}/{questions.Length}";
        }
    }

    private void UpdateQuestionIndicators()
    {
        if (questionIndicators == null || questionIndicators.Length == 0)
            return;

        for (int i = 0; i < questionIndicators.Length; i++)
        {
            if (questionIndicators[i] != null)
            {
                // Set color based on whether this is the current question
                if (i == currentQuestionIndex)
                    questionIndicators[i].color = indicatorActiveColor;
                else
                    questionIndicators[i].color = indicatorDefaultColor;
            }
        }
    }

    private void SelectAnswer(int buttonIndex)
    {
        // Clear previous selection highlighting
        ClearAnswerSelection();
        
        // Store the answer
        playerAnswers[currentQuestionIndex] = buttonIndex;
        
        // Highlight the selected button
        if (buttonIndex >= 0 && buttonIndex < questions[currentQuestionIndex].answerButtons.Length)
        {
            Button selectedButton = questions[currentQuestionIndex].answerButtons[buttonIndex];
            ColorBlock colors = selectedButton.colors;
            colors.normalColor = selectedAnswerColor;
            selectedButton.colors = colors;
        }

        if (currentQuestionIndex < questions.Length - 1)
            ActivateQuestion(currentQuestionIndex + 1);
    }
    
    private void ClearAnswerSelection()
    {
        // Clear highlight from all buttons in current question
        foreach (Button btn in questions[currentQuestionIndex].answerButtons)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = defaultAnswerColor;
            btn.colors = colors;
        }
    }
    
    private void RestoreAnswerSelection()
    {
        // If player already answered this question, highlight their choice
        if (playerAnswers[currentQuestionIndex] >= 0)
        {
            int selectedIndex = playerAnswers[currentQuestionIndex];
            if (selectedIndex < questions[currentQuestionIndex].answerButtons.Length)
            {
                Button selectedButton = questions[currentQuestionIndex].answerButtons[selectedIndex];
                ColorBlock colors = selectedButton.colors;
                colors.normalColor = selectedAnswerColor;
                selectedButton.colors = colors;
            }
        }
    }

    public void GoToQuestion(int index)
    {
        ActivateQuestion(index);
    }

    public void SubmitQuiz()
    {
        int score = 0;
        for (int i = 0; i < questions.Length; i++)
        {
            if (playerAnswers[i] == questions[i].correctIndex)
                score++;
        }

        // Show score panel
        scorePanel.SetActive(true);
        scoreText.text = $"Napakaganda! Nakakuha ka ng {score}/{questions.Length} Puntos!";

        string exp = "\n RESULTA NG IYONG UNANG PAGSUSULIT \n\n";
        exp += "Salamat sa pagsali sa quiz! Ikaw ay patunayan na ikaw ay may mataas na pag-unawa sa konsepto ng Kailangan at Gusto. Patuloy na matuto at mag-practice upang maging mas mahusay!\n\n";
        exp += "\n\n";
        
        for (int i = 0; i < questions.Length; i++)
        {
            string result = (playerAnswers[i] == questions[i].correctIndex) ? "✓ TAMA" : "✗ MALI";
            exp += $"Tanong {i + 1}: {result}\n";
            exp += $"  Tamang Sagot: {questions[i].answerTexts[questions[i].correctIndex]}\n\n";
        }
        
        exp += "\n";
        exp += "\n Magpatuloy sa pag-aaral! Ang iyong sipag ay magdudulot ng tagumpay! ";
        
        explanationText.text = exp;

        // Mark MainQuiz as completed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MarkMainQuizCompleted();
            Debug.Log("MainQuizManager: MarkMainQuizCompleted() called.");
        }
        else
        {
            Debug.LogWarning("MainQuizManager: GameManager.Instance is null, cannot mark main quiz completed.");
        }

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();
            ProfileManager.Instance.UpdateProgressBars();
            ProfileManager.Instance.UpdateBadges();
            Debug.Log("MainQuizManager: ProfileManager refreshed after quiz completion.");
        }

        // Add 50 currency reward after quiz submission
        AddCurrencyReward(50);
    }

    /// <summary>
    /// Adds the specified amount of currency to the active user's profile.
    /// </summary>
    private void AddCurrencyReward(int amount)
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("MainQuizManager: Cannot add currency reward - no active user.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"MainQuizManager: Profile file not found at {path}");
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        if (profile == null)
        {
            Debug.LogWarning("MainQuizManager: Failed to parse profile JSON.");
            return;
        }

        profile.currency += amount;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
        Debug.Log($"MainQuizManager: Added {amount} currency to {activeUser}. New total: {profile.currency}");

        // Refresh GameManager and ProfileManager if available
        if (GameManager.Instance != null)
            GameManager.Instance.RefreshProfileData();
    }

    private void ShowScorePanelOnly(ProfilePlayerData data)
    {
        scorePanel.SetActive(true);
        scoreText.text = "Natapos mo na ang quiz na ito!";
        explanationText.text = "\n Quiz na Tapos na \n\nPagkilala! Ikaw ay nag-aral at nakumpleto na ng malaking hamon na ito. Ang iyong pagsisikap at dedikasyon ay nagbunga ng tagumpay. Magpatuloy sa pagiging masipag at patuloy na mag-aral!\n\nMaaari mong tingnan ang iyong mga sagot dito. Kung nais mo ng karagdagang pag-aaral, magsimula ng bagong hamon o subukan ang ibang quiz upang mapahusay pa ang iyong kaalaman!\n\n Ikaw ay karapat-dapat! ";
    }
}
