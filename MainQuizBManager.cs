using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class QuestionBUI
{
    public GameObject panel;           // The parent panel (Number1…Number15)
    public TMP_Text questionText;      // Question text
    public Button[] answerButtons;     // Buttons (3 or 2)
    public string[] answerTexts;       // Text for each button
    public int correctIndex;           // Index of correct button (0-based)
}

public class MainQuizBManager : MonoBehaviour
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

        // Check if player already completed this quiz
        string username = PlayerPrefs.GetString("ActiveUser", "");
        string path = Path.Combine(Application.persistentDataPath, username + "_profile.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);

            if (data.mainQuizBCompleted)
            {
                ShowScorePanelOnly(data);
                return;
            }
        }

        // Initialize all question panels inactive
        foreach (var q in questions)
            q.panel.SetActive(false);

        SetupQuestions();
        ActivateQuestion(0);
    }

    private void SetupQuestions()
    {
        // Example 4-button questions (Q1-Q10)
        string[] q1to10 = new string[]
        {
        "Si Maria ay kumita ng ₱1000 buwan-buwan. Kung mag-ipon siya ng ₱200, magkano ang ang maaari niyang gastusin?",

        "Ang kita ni Juan ay ₱800 buwan-buwan. Nag-ipon siya ng ₱300. Ano ang natitirang halaga para sa pagkonsumo?",

        "Si Rosa ay may ₱500 nakita at nag-ipon ng ₱100. Alin sa mga ito ang halagang ginastos?",

        "Kung ang income ay ₱2000 at ang ipon ay ₱500, magkano ang nagastos para sa pagkonsumo?",

        "Ang kabuuang pagkonsumo ni Pedro ay ₱400. Kung ang kita niya ay ₱800, magkano ang maaring niyang ma ipon?",

        "Sa ugnayan ng kita, ipon, at consumption, alin ang tamang pormula?",

        "Kung walang ipon, ano ang magiging ugnayan ng kita at pagkonsumo?",

        "Ang pagkonsumo ni Ana ay tumaas habang bumaba ang kita. Ano ang magiging epekto?",

        "Si Maria ay nagtrabaho bilang empleyada at nakatanggap ng ₱15,000 bawat buwan. Anong uri ng kita ito?",

        "Si Juan ay may sariling tindahan at kumikita ng ₱12,000 bawang buwan. Anong klasifikasyon ang kita na ito?"
        };

        string[][] answers1to10 = new string[][]
        {
            new string[]{"₱600","₱800","₱1200","₱400"},
            new string[]{"₱500","₱400","₱200","₱600"},
            new string[]{"₱300","₱100","₱400","₱500"},
            new string[]{"₱1500","₱1200","₱1000","₱2500"},
            new string[]{"₱300","₱200","₱100","₱400"},
            new string[]{"Kita = Ipon + Pagkonsumo","Pagkonsumo = Kita + Ipon","Ipon = Pagkonsumo - Kita","Kita = Pagkonsumo / Ipon"},
            new string[]{"Pagkonsumo ay katumbas ng kita","Pagkonsumo ay lumalaki","Pagkonsumo ay bumabalik","Walang alam"},
            new string[]{"Tumaas ang ipon","Bumaba ang ipon","Pareho ang ipon","Hindi malinaw"},
            new string[]{"Suporta ng magulang","Kita mula sa trabaho (Sahod/Wages)","Kita mula sa negosyo","Interes mula sa ipon"},
            new string[]{"Suporta ng magulang","Kita mula sa negosyo (Business Income)","Propesyonal na bayad","Kita mula sa pagpapaupa"}
        };

        int[] correct1to10 = new int[]{1,0,2,0,3,0,0,1,1,1};

        // 2-button questions (Q11-Q15)
        string[] q11to15 = new string[]
        {
        "Ang kita ay dapat balanseng ilagay sa pagkonsumo at pag-iimpok.",

        "Ang sobrang pagkonsumo ay makakabawas ng mga ipon.",

        "Ang pag-iimpok ay hindi mahalaga kung mataas ang kita.",
        
        "Ang tamang pagpaplano ng badyet ay nagsisiguro na may natitirang pera para sa pag-iimpok.",

        "Ang ugnayan ng kita, ipon, at pagkonsumo ay laging positibo."
        };

        string[][] answers11to15 = new string[][]
        {
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"}
        };

        int[] correct11to15 = new int[]{0,0,1,0,1};

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
        scoreText.text = $"Nakuha mo ay {score}/{questions.Length} na Puntos!";

        string exp = "";
        for (int i = 0; i < questions.Length; i++)
        {
            string result = (playerAnswers[i] == questions[i].correctIndex) ? "Tama!" : "Mali!";
            exp += $"Q{i + 1}: {result} - Tamang Sagot : {questions[i].answerTexts[questions[i].correctIndex]}\n";
        }
        explanationText.text = exp;

        // Mark this quiz as completed
        GameManager.Instance.MarkMainQuizBCompleted();

        // Add 50 currency reward after quiz submission
        AddCurrencyReward(50);
    }

    private void ShowScorePanelOnly(ProfilePlayerData data)
    {
        scorePanel.SetActive(true);
        scoreText.text = "Tapos mo na ang quiz na ito!";
        explanationText.text = "Oops! Hindi ka na makaka-review kasi natapos mo na ang quiz!";
    }

    /// <summary>
    /// Adds the specified amount of currency to the active user's profile.
    /// </summary>
    private void AddCurrencyReward(int amount)
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("MainQuizBManager: Cannot add currency reward - no active user.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"MainQuizBManager: Profile file not found at {path}");
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        if (profile == null)
        {
            Debug.LogWarning("MainQuizBManager: Failed to parse profile JSON.");
            return;
        }

        profile.currency += amount;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
        Debug.Log($"MainQuizBManager: Added {amount} currency to {activeUser}. New total: {profile.currency}");

        // Refresh GameManager and ProfileManager if available
        if (GameManager.Instance != null)
            GameManager.Instance.RefreshProfileData();
    }
}
