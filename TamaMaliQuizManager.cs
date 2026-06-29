using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TamaMaliQuizManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizPanel;        // Panel that contains the questions
    public GameObject scorePanel;       // Panel that shows score at the end
    public TextMeshProUGUI questionTMP; // Displays the current question
    public TextMeshProUGUI timerTMP;    // Shows remaining time
    public TextMeshProUGUI resultTMP;   // optional, for final result
    public TextMeshProUGUI answersTMP;  // Shows explanations for answers
    public TextMeshProUGUI scoreText;   // Shows score at the end

    [Header("Buttons")]
    public Button tamaButton;           // Button for "Tama" choice
    public Button maliButton;           // Button for "Mali" choice

    [Header("Simple Path Settings")]
    public int simplePathCurrencyCost = 0; // Higher currency cost for simple path
    public string world2SceneName = ""; // Scene to load after simple choice
    public int tamaMaliRewardCurrency = 250; // Reward amount for completing either path

    [Header("Quiz Settings")]
    public float quizTime = 60f; // Total time for the quiz in seconds

    private float timeRemaining;        // Timer countdown
    private int currentQuestionIndex = 0;  // Tracks current question
    private int score = 0;                 // Tracks player score

    [System.Serializable]
    public class Question
    {
        public string question;        // Question text
        public bool correctAnswer;     // true = Tama, false = Mali
        public string explanation;     // Explanation for the answer
    }

    public List<Question> questions = new List<Question>();

    private bool questionsLoaded = false;

    private void Awake()
    {
        timeRemaining = quizTime;

        // Hide panels at the start
        if (quizPanel != null) quizPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);

        // Clear texts at the start
        if (resultTMP != null) resultTMP.text = "";
        if (answersTMP != null) answersTMP.text = "";

        // Add button listeners
        if (tamaButton != null) tamaButton.onClick.AddListener(() => Answer(true));
        if (maliButton != null) maliButton.onClick.AddListener(() => Answer(false));

        // Load all questions once
        if (!questionsLoaded)
        {
            LoadQuestions();
            questionsLoaded = true;
        }
    }

    public void InitializeQuiz()
    {
        timeRemaining = quizTime;
        currentQuestionIndex = 0;
        score = 0;

        if (answersTMP != null)
            answersTMP.text = "";

        if (resultTMP != null)
            resultTMP.text = "";

        if (scorePanel != null)
            scorePanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        ShowQuestion();
    }

    // Method for simple choice - deducts currency and moves to World 2
    public void ChooseSimplePath()
    {
        Debug.Log($"Choosing simple path - deducting {simplePathCurrencyCost} currency");

        // Access GameManager to deduct currency
        if (GameManager.Instance != null)
        {
            // Check if player has enough currency
            if (GameManager.Instance.Currency >= simplePathCurrencyCost)
            {
                // Deduct currency
                GameManager.Instance.Currency -= simplePathCurrencyCost;
                Debug.Log($"Currency deducted. New balance: {GameManager.Instance.Currency}");

                AwardActiveUserCurrency(tamaMaliRewardCurrency);

                // Move to World 2
                SceneManager.LoadScene(world2SceneName);
            }
            else
            {
                Debug.LogWarning("Not enough currency for simple path!");
                // You might want to show a message to the player here
            }
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }
    }

    void Update()
    {
        if (quizPanel.activeSelf && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timerTMP != null)
                timerTMP.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
        }
        else if (quizPanel.activeSelf && timeRemaining <= 0)
        {
            EndQuiz();
        }
    }

    void LoadQuestions()
    {
        questions.Add(new Question {
            question = "Ang kita ay ang perang natatanggap ng pamilya mula sa trabaho, negosyo, o iba pang pinagkakakitaan.",
            correctAnswer = true,
            explanation = "Sa Ugnayan ng Kita, Pag-iimpok, at Pagkonsumo, ang kita ang pangunahing batayan ng pagkilos sa mga gastusin at ipon."
        });

        questions.Add(new Question {
            question = "Kapag pinili ng pamilya na higit na gumastos kaysa mag-impok, mas malaki ang tinatawag na pagbabanggit ng pag-iimpok.",
            correctAnswer = false,
            explanation = "Ang pagbabanggit ng pag-iimpok ay nangyayari kapag mas maliit ang ipinapong bahagi ng kita kaysa sa kinukunsumo."
        });

        questions.Add(new Question {
            question = "Ang pag-iimpok ay bahagi ng kita na hindi agad ginagastos at inilalagay para sa hinaharap.",
            correctAnswer = true,
            explanation = "Ito ang natitirang pera mula sa kita matapos bayaran ang mga kailangan at nais na gastusin."
        });

        questions.Add(new Question {
            question = "Ang pagkonsumo ay nangangahulugan lamang ng paggastos ng pera sa mga luho at hindi sa mga pangunahing pangangailangan.",
            correctAnswer = false,
            explanation = "Kasama ang pangunahing pangangailangan at kagustuhan sa pagkonsumo; hindi ito limitado sa luho lamang."
        });

        questions.Add(new Question {
            question = "Kung tumaas ang kita, may posibilidad din na tumaas ang pag-iimpok ng pamilya.",
            correctAnswer = true,
            explanation = "Mas malaki ang kakayahang mag-ipon kapag lumaki ang kita, lalo na kung may tamang pagplano."
        });

        questions.Add(new Question {
            question = "Ang tamang balanse ng kita, pag-iimpok, at pagkonsumo ay mahalaga para sa matatag na pamumuhay.",
            correctAnswer = true,
            explanation = "Kapag balansado ang kita, ipon, at gastos, nagiging mas matatag ang kalagayang pinansyal ng pamilya."
        });

        questions.Add(new Question {
            question = "Ang hindi planadong pagkonsumo kahit mataas ang kita ay maaaring magdulot ng kakulangan sa ipon.",
            correctAnswer = true,
            explanation = "Kahit mataas ang kita, nauubos ang pera kung hindi sinusuri ang gastusin, kaya nababawasan ang ipon."
        });

        questions.Add(new Question {
            question = "Ang pag-iimpok ay hindi kailangang isaalang-alang kapag sapat ang kita para sa lahat ng gastusin.",
            correctAnswer = false,
            explanation = "Mahalaga ang pag-iimpok kahit sapat ang kita para sa pang-araw-araw na pangangailangan at emergency."
        });

        questions.Add(new Question {
            question = "Ang pagkonsumo ay direktang nakakaapekto sa pag-iimpok dahil pareho itong bahagi ng mamayang kita.",
            correctAnswer = true,
            explanation = "Sa kita, ang natitirang bahagi pagkatapos kumonsumo ay kung ano ang naitatabi bilang ipon."
        });

        questions.Add(new Question {
            question = "Ang pagtaas ng pag-iimpok ay laging nangangahulugang mas mababa ang pagkonsumo ng pamilya.",
            correctAnswer = false,
            explanation = "Maaaring tumaas ang ipon nang hindi gaanong bumababa ang pagkonsumo kapag lumaki ang kita."
        });
    }

    private void AwardActiveUserCurrency(int amount)
    {
        if (amount <= 0)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(amount);
            Debug.Log($"Awarded {amount} currency to active user. New balance: {GameManager.Instance.Currency}");
        }
        else
        {
            Debug.LogWarning("GameManager instance not found - cannot award currency.");
        }
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            if (questionTMP != null)
                questionTMP.text = questions[currentQuestionIndex].question;
        }
        else
        {
            EndQuiz();
        }
    }

    void Answer(bool userAnswer)
    {
        if (currentQuestionIndex >= questions.Count) return;

        Question q = questions[currentQuestionIndex];

        if (userAnswer == q.correctAnswer)
        {
            score++;
        }

        if (answersTMP != null)
        {
            answersTMP.text +=
                "Q" + (currentQuestionIndex + 1) + ": " + q.question +
                "\nTamang Sagot: " + (q.correctAnswer ? "Tama" : "Mali") +
                "\nExplanasyon: " + q.explanation + "\n\n";
        }

        currentQuestionIndex++;
        ShowQuestion();
    }

    void EndQuiz()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (scorePanel != null)
            scorePanel.SetActive(true);

        if (scoreText != null)
            scoreText.text = $"Iyong Puntos: {score}/{questions.Count}";

        // Optionally mark quiz complete or update profile
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();
            ProfileManager.Instance.UpdateProgressBars();
        }

        AwardActiveUserCurrency(tamaMaliRewardCurrency);
    }
}
