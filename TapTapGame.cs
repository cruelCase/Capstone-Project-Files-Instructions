using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;

public class TapTapGame : MonoBehaviour
{
    [Header("Ball Settings")]
    public Rigidbody2D ballRigidbody;
    public float firstTapJumpForce = 6f;
    public float tapJumpForce = 5f;
    public float gravityScale = 1.5f;
    public float maximumFallSpeed = -12f;
    public float lossY = -6f;

    [Header("UI")]
    public TextMeshProUGUI tapCounterText;
    public TextMeshProUGUI tapBonusText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    [Header("Optional Start Position")]
    public Transform ballStartPoint;

    private bool gameStarted;
    private bool isGameOver;
    private int tapCount;
    private Coroutine bonusCoroutine;

    private void Awake()
    {
        if (ballRigidbody == null)
            ballRigidbody = GetComponent<Rigidbody2D>();

        ResetBallState();
        UpdateTapCounterText();

        if (tapBonusText != null)
            tapBonusText.text = string.Empty;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (!gameStarted || isGameOver || ballRigidbody == null)
            return;

        if (ballRigidbody.linearVelocity.y < maximumFallSpeed)
            ballRigidbody.linearVelocity = new Vector2(ballRigidbody.linearVelocity.x, maximumFallSpeed);

        if (ballRigidbody.position.y <= lossY)
            EndGame();
    }

    public void TapBall()
    {
        if (isGameOver)
            return;

        if (!gameStarted)
            StartGame();

        if (ballRigidbody == null)
            return;

        ballRigidbody.bodyType = RigidbodyType2D.Dynamic;
        ballRigidbody.gravityScale = gravityScale;
        ballRigidbody.linearVelocity = new Vector2(ballRigidbody.linearVelocity.x, tapJumpForce);

        tapCount++;
        UpdateTapCounterText();
        ShowTapBonus();
        AddPoints(1);
    }

    public void RestartGame()
    {
        isGameOver = false;
        gameStarted = false;
        tapCount = 0;

        ResetBallState();
        UpdateTapCounterText();

        if (tapBonusText != null)
            tapBonusText.text = string.Empty;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (ballStartPoint != null)
            ballRigidbody.position = ballStartPoint.position;
    }

    public void StartGameButton()
    {
        if (ballRigidbody == null)
            return;

        if (isGameOver)
            RestartGame();

        gameStarted = true;
        isGameOver = false;

        ballRigidbody.bodyType = RigidbodyType2D.Dynamic;
        ballRigidbody.gravityScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameOverText != null)
            gameOverText.text = string.Empty;
    }

    private void StartGame()
    {
        gameStarted = true;
        isGameOver = false;

        if (ballRigidbody != null)
            ballRigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    private void EndGame()
    {
        isGameOver = true;
        gameStarted = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = "Game Over";
    }

    private void ResetBallState()
    {
        if (ballRigidbody == null)
            return;

        ballRigidbody.linearVelocity = Vector2.zero;
        ballRigidbody.angularVelocity = 0f;
        ballRigidbody.gravityScale = 0f;
        ballRigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    private void UpdateTapCounterText()
    {
        if (tapCounterText != null)
            tapCounterText.text = $"Taps: {tapCount}";
    }

    private void ShowTapBonus()
    {
        if (tapBonusText == null)
            return;

        tapBonusText.text = "+1";

        if (bonusCoroutine != null)
            StopCoroutine(bonusCoroutine);

        bonusCoroutine = StartCoroutine(HideTapBonusAfterDelay(0.5f));
    }

    private IEnumerator HideTapBonusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tapBonusText != null)
            tapBonusText.text = string.Empty;

        bonusCoroutine = null;
    }

    private void AddPoints(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(amount);
            return;
        }

        SavePointsToProfile(amount);
    }

    private void SavePointsToProfile(int amount)
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
            return;

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        ProfilePlayerData profile = null;

        if (File.Exists(path))
            profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        else
            profile = new ProfilePlayerData { username = activeUser };

        profile.points += amount;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
    }

    private void OnMouseDown()
    {
        TapBall();
    }
}
