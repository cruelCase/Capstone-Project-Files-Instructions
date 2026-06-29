using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FourPicsOneWordManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject gamePanel;
    public GameObject scorePanel;

    [Header("Images")]
    public Image pic1;
    public Image pic2;
    public Image pic3;
    public Image pic4;

    [Header("UI")]
    public TMP_InputField answerInput;
    public TMP_Text scoreText;
    public TMP_Text questionText;
    public TMP_Text resultText;

    [Header("Button")]
    public Button submitButton;

    private int currentIndex = 0;
    private int score = 0;

    [System.Serializable]
    public class Puzzle
    {
        public Sprite[] images; // 4 images
        public string answer;
    }

    public List<Puzzle> puzzles = new List<Puzzle>();

    private void Awake()
    {
        gamePanel.SetActive(false);
        scorePanel.SetActive(false);

        if (submitButton != null)
            submitButton.onClick.AddListener(CheckAnswer);
    }

    public void InitializeGame()
    {
        currentIndex = 0;
        score = 0;

        if (gamePanel != null)
            gamePanel.SetActive(true);

        if (scorePanel != null)
            scorePanel.SetActive(false);

        LoadPuzzle();
    }

    public void StartGame()
    {
        InitializeGame();
    }


    void LoadPuzzle()
    {
        if (currentIndex >= puzzles.Count)
        {
            EndGame();
            return;
        }

        Puzzle p = puzzles[currentIndex];

        pic1.sprite = p.images[0];
        pic2.sprite = p.images[1];
        pic3.sprite = p.images[2];
        pic4.sprite = p.images[3];

        answerInput.text = "";
        resultText.text = "";

        questionText.text = $"Tanong: {currentIndex + 1} / {puzzles.Count}";
        scoreText.text = $"Iskor: {score}";
    }

    void CheckAnswer()
    {
        string userAnswer = answerInput.text.Trim().ToLower();
        string correctAnswer = puzzles[currentIndex].answer.ToLower();

        if (userAnswer == correctAnswer)
        {
            score++;
            resultText.text = "✔ Tama!";
        }
        else
        {
            resultText.text = $"✖ Mali! Sagot: {puzzles[currentIndex].answer}";
        }

        Invoke(nameof(NextPuzzle), 1.5f);
    }

    void NextPuzzle()
    {
        currentIndex++;
        LoadPuzzle();
    }

    void EndGame()
    {
        gamePanel.SetActive(false);
        scorePanel.SetActive(true);
        scoreText.text = $"Pinal na Iskor: {score} / {puzzles.Count}";
    }
}
