using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class QuestionCUI
{
    public GameObject panel;           // The parent panel (Number1…Number15)
    public TMP_Text questionText;      // Question text
    public Button[] answerButtons;     // Buttons (2 or 3)
    public string[] answerTexts;       // Text for each button
    public int correctIndex;           // Index of correct button (0-based)
}

public class MainQuizCManager : MonoBehaviour
{
    [Header("Questions Setup")]
    public QuestionCUI[] questions;     // 15 questions

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

            if (data.mainQuizCCompleted)
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
        // True/False questions focusing on Pagkonsumo (Consumption) factors and wise choices
        string[] q1to10 = new string[]
        {
        "Ang presyo ay isa sa mga pangunahing salik na nakakaapekto sa pagkonsumo.",

        "Ang kita ay walang epekto sa kung magkano ang mako-consume mo.",

        "Ang pagkakaroon ng produkto ay nakaaapekto sa kung ano ang bibilhin mo.",

        "Ang pangangailangan at kagustuhan ay pareho, kaya hindi mahalaga ang pagkakaiba.",

        "Ang impluwensiya ng kaibigan o grupo ay maaaring makaapekto sa mga desisyon sa pagkonsumo.",

        "Ang mataas na presyo ay magpapataas ng pagkonsumo.",

        "Ang pag-unawa sa mga salik ng pagkonsumo ay nakatutulong sa tamang desisyon sa pagbili.",

        "Ang marketing at advertising ay hindi nakakaapekto sa asal ng mamimili.",

        "Ang pagbabago ng mga season ay maaaring makaapekto sa pagkakaroon ng produkto at sa pagkonsumo.",

        "Ang kalidad ng produkto ay importante sa pagpiling pagkonsumo."
        };

        string[][] answers1to10 = new string[][]
        {
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"},
            new string[]{"Tama","Mali"}
        };

        int[] correct1to10 = new int[]{0,1,0,1,0,1,0,1,0,0};

        // 2-3 choice questions for Q11-Q15
        string[] q11to15 = new string[]
        {
            "Alin ang pangunahing salik na nag-aapekto sa pagkonsumo?",

            "Paano nakakaapekto ang limitadong kita sa mga desisyon sa pagkonsumo?",

            "Ang pagkonsumo ay dapat na balanse sa kita at mga pangangailangan. Ito ba ay matalinong desisyon?",

            "Si Maria ay nais bumili ng mahal na gadget pero limited ang kita niya. Ano ang dapat gawin?",

            "Alin ang hindi dapat maging pangunahing salik sa mga desisyon sa pagkonsumo?"
        };

        string[][] answers11to15 = new string[][]
        {
            new string[]{"Presyo","Kulay","Laki ng shop"},
            new string[]{"Nag-encourage ng mas maraming gastos","Nag-limit ng choices","Walang epekto"},
            new string[]{"Oo, matalinong desisyon","Hindi, walang balanse","Depende"},
            new string[]{"Bilhin agad","Mag-ipon muna o hintayin","Balewalain"},
            new string[]{"Pangangailangan","Budget","Peer pressure lamang"}
        };

        int[] correct11to15 = new int[]{0,1,0,1,2};

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
        GameManager.Instance.MarkMainQuizCCompleted();

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
            Debug.LogWarning("MainQuizCManager: Cannot add currency reward - no active user.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"MainQuizCManager: Profile file not found at {path}");
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        if (profile == null)
        {
            Debug.LogWarning("MainQuizCManager: Failed to parse profile JSON.");
            return;
        }

        profile.currency += amount;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
        Debug.Log($"MainQuizCManager: Added {amount} currency to {activeUser}. New total: {profile.currency}");

        // Refresh GameManager and ProfileManager if available
        if (GameManager.Instance != null)
            GameManager.Instance.RefreshProfileData();
    }
}
