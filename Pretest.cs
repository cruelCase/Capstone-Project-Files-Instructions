using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class QuestionUIB
{
    public GameObject panel;           // The parent panel (Number1…Number15)
    public TMP_Text questionText;      // Question text
    public Button[] answerButtons;     // Buttons (4 or 2)
    public string[] answerTexts;       // Text for each button
    public int correctIndex;           // Index of correct button (0-based)
}

public class Pretest : MonoBehaviour
{
    [Header("Questions Setup")]
    public QuestionUI[] questions;     // 15 questions

    [Header("UI References")]
    public GameObject scorePanel;
    public TMP_Text scoreText;
    public TMP_Text explanationText;

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
            "Alin dito ang KAILANGAN?",
            "Alin dito ang GUSTO?",
            "Alin ang KAILANGAN?",
            "Alin ang GUSTO?",
            "Tukuyin ang KAILANGAN",
            "Alin ang GUSTO?",
            "Alin ang KAILANGAN?",
            "Alin ang GUSTO?",
            "Piliin ang KAILANGAN",
            "Alin ang GUSTO?"
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

        "Nagbayad ka ng kuryente.",

        "Bumili ka ng kendi sa tindahan.",

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
    }

    private void SelectAnswer(int buttonIndex)
    {
        playerAnswers[currentQuestionIndex] = buttonIndex;

        if (currentQuestionIndex < questions.Length - 1)
            ActivateQuestion(currentQuestionIndex + 1);
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
        scoreText.text = $"You got {score}/{questions.Length} correct!";

        string exp = "";
        for (int i = 0; i < questions.Length; i++)
        {
            string result = (playerAnswers[i] == questions[i].correctIndex) ? "Correct" : "Wrong";
            exp += $"Q{i + 1}: {result} - Correct Answer: {questions[i].answerTexts[questions[i].correctIndex]}\n";
        }
        explanationText.text = exp;

        // ===============================
        // SAVE PRETEST COMPLETION
        // ===============================
        string username = PlayerPrefs.GetString("ActiveUser", "");
        if (!string.IsNullOrEmpty(username))
        {
            string path = Path.Combine(Application.persistentDataPath, username + "_profile.json");

            if (File.Exists(path))
            {
                ProfilePlayerData data =
                    JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));

                data.pretestCompleted = true;
                data.pretestScore = score;

                File.WriteAllText(path, JsonUtility.ToJson(data, true));
            }
        }

        
    }

    private void ShowScorePanelOnly(ProfilePlayerData data)
    {
        scorePanel.SetActive(true);
        scoreText.text = "Tapos mo na ang quiz na ito!";
        explanationText.text = "Oops! Hindi ka na makaka-review kasi natapos mo na ang quiz!";
    }
}
