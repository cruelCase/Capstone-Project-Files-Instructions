using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class infiniteGame2 : MonoBehaviour
{
    public Button batoButton;
    public Button papelButton;
    public Button guntingButton;
    public Button startButton;

    public Transform mySpawn;
    public Transform enemySpawn;

    public GameObject myBatoPrefab;
    public GameObject myPapelPrefab;
    public GameObject myGuntingPrefab;
    public GameObject enemyBatoPrefab;
    public GameObject enemyPapelPrefab;
    public GameObject enemyGuntingPrefab;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI winLoseText;
    public TextMeshProUGUI pointsPopupText;
    public GameObject gameOverPanel;

    public float roundDuration = 5f;
    public float nextRoundDelay = 2f;
    public int winPoints = 10;

    private float remainingTime;
    private bool roundRunning;
    private bool isGameOver;
    private Move selectedMove = Move.None;
    private Coroutine popupCoroutine;
    private Coroutine continueCoroutine;
    private Coroutine gameOverCoroutine;

    private enum Move
    {
        None,
        Bato,
        Papel,
        Gunting
    }

    private void Awake()
    {
        if (batoButton != null) batoButton.onClick.AddListener(() => OnPlayerSelect(Move.Bato));
        if (papelButton != null) papelButton.onClick.AddListener(() => OnPlayerSelect(Move.Papel));
        if (guntingButton != null) guntingButton.onClick.AddListener(() => OnPlayerSelect(Move.Gunting));
        if (startButton != null) startButton.onClick.AddListener(OnStartButtonPressed);

        if (pointsPopupText != null)
            pointsPopupText.gameObject.SetActive(false);
        if (winLoseText != null)
            winLoseText.text = "";
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        PrepareForStart();
    }

    private void OnDestroy()
    {
        if (batoButton != null) batoButton.onClick.RemoveAllListeners();
        if (papelButton != null) papelButton.onClick.RemoveAllListeners();
        if (guntingButton != null) guntingButton.onClick.RemoveAllListeners();
        if (startButton != null) startButton.onClick.RemoveAllListeners();
    }

    private void PrepareForStart()
    {
        isGameOver = false;
        roundRunning = false;
        selectedMove = Move.None;
        remainingTime = roundDuration;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winLoseText != null)
            winLoseText.text = "Pindutin ang Start para magsimula.";

        if (pointsPopupText != null)
            pointsPopupText.gameObject.SetActive(false);

        SetChoiceButtonsInteractable(false);
        ClearSpawn(mySpawn);
        ClearSpawn(enemySpawn);
        UpdateTimerText();
    }

    private void Update()
    {
        if (!roundRunning || isGameOver)
            return;

        remainingTime -= Time.deltaTime;
        UpdateTimerText();

        if (remainingTime <= 0f)
            EndRound();
    }

    private void StartNewRound()
    {
        isGameOver = false;
        roundRunning = true;
        selectedMove = Move.None;
        remainingTime = roundDuration;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winLoseText != null)
            winLoseText.text = "Pumili kung Rock, Paper, o Scissor bago matapos ang oras!";

        if (pointsPopupText != null)
            pointsPopupText.gameObject.SetActive(false);

        SetChoiceButtonsInteractable(true);
        ClearSpawn(mySpawn);
        ClearSpawn(enemySpawn);
        UpdateTimerText();
    }

    private void OnPlayerSelect(Move move)
    {
        if (!roundRunning || isGameOver)
            return;

        if (selectedMove != Move.None)
            return;

        selectedMove = move;
        if (winLoseText != null)
            winLoseText.text = "Pinili " + move.ToString() + ". Naghihintay para sa dulo ng round...";
    }

    private void EndRound()
    {
        roundRunning = false;
        SetChoiceButtonsInteractable(false);

        Move enemyMove = GetRandomMove();
        DisplayChoice(mySpawn, selectedMove, true);
        DisplayChoice(enemySpawn, enemyMove, false);

        if (selectedMove == Move.None)
        {
            if (winLoseText != null)
                winLoseText.text = "Walang napiling galaw. Natalo ka!";
            StartGameOverDelay(1f);
            return;
        }

        if (selectedMove == enemyMove)
        {
            if (winLoseText != null)
                winLoseText.text = "Pantay! Subukan muli.";
            continueCoroutine = StartCoroutine(ContinueRoundAfterDelay(nextRoundDelay));
            return;
        }

        if (PlayerWins(selectedMove, enemyMove))
        {
            if (winLoseText != null)
                winLoseText.text = "Panalo! +10 puntos";

            AddWinPoints(winPoints);
            ShowPointsPopup($"+{winPoints} points");
            continueCoroutine = StartCoroutine(ContinueRoundAfterDelay(nextRoundDelay));
            return;
        }

        if (winLoseText != null)
            winLoseText.text = "Natalo ka!";

        StartGameOverDelay(1f);
    }

    private void StartGameOverDelay(float delay)
    {
        if (gameOverCoroutine != null)
            StopCoroutine(gameOverCoroutine);

        gameOverCoroutine = StartCoroutine(ShowGameOverAfterDelay(delay));
    }

    private IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerGameOver();
        gameOverCoroutine = null;
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        roundRunning = false;
        SetChoiceButtonsInteractable(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private IEnumerator ContinueRoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isGameOver)
            StartNewRound();
    }

    public void OnStartButtonPressed()
    {
        if (continueCoroutine != null)
            StopCoroutine(continueCoroutine);

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        if (gameOverCoroutine != null)
            StopCoroutine(gameOverCoroutine);

        StartNewRound();
    }

    public void StopGame()
    {
        if (continueCoroutine != null)
            StopCoroutine(continueCoroutine);

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        if (gameOverCoroutine != null)
            StopCoroutine(gameOverCoroutine);

        isGameOver = true;
        roundRunning = false;
        PrepareForStart();
    }

    private void AddWinPoints(int amount)
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
        {
            profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        }
        else
        {
            profile = new ProfilePlayerData { username = activeUser };
        }

        profile.points += amount;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
    }

    private void ShowPointsPopup(string message)
    {
        if (pointsPopupText == null)
            return;

        pointsPopupText.text = message;
        pointsPopupText.gameObject.SetActive(true);

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(HidePointsPopupAfterDelay(2f));
    }

    private IEnumerator HidePointsPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pointsPopupText != null)
            pointsPopupText.gameObject.SetActive(false);
        popupCoroutine = null;
    }

    private void SetChoiceButtonsInteractable(bool interactable)
    {
        if (batoButton != null) batoButton.interactable = interactable;
        if (papelButton != null) papelButton.interactable = interactable;
        if (guntingButton != null) guntingButton.interactable = interactable;
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = $"Oras: {Mathf.CeilToInt(Mathf.Max(0f, remainingTime))}s";
    }

    private bool PlayerWins(Move player, Move enemy)
    {
        return (player == Move.Bato && enemy == Move.Gunting) ||
               (player == Move.Papel && enemy == Move.Bato) ||
               (player == Move.Gunting && enemy == Move.Papel);
    }

    private Move GetRandomMove()
    {
        int value = Random.Range(0, 3);
        return (Move)(value + 1);
    }

    private void DisplayChoice(Transform spawnPoint, Move move, bool isPlayer)
    {
        if (spawnPoint == null)
            return;

        ClearSpawn(spawnPoint);

        GameObject prefab = null;
        if (isPlayer)
        {
            if (move == Move.Bato) prefab = myBatoPrefab;
            if (move == Move.Papel) prefab = myPapelPrefab;
            if (move == Move.Gunting) prefab = myGuntingPrefab;
        }
        else
        {
            if (move == Move.Bato) prefab = enemyBatoPrefab;
            if (move == Move.Papel) prefab = enemyPapelPrefab;
            if (move == Move.Gunting) prefab = enemyGuntingPrefab;
        }

        if (prefab == null)
            return;

        GameObject instance = Instantiate(prefab, spawnPoint);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    private void ClearSpawn(Transform spawnPoint)
    {
        if (spawnPoint == null)
            return;

        for (int i = spawnPoint.childCount - 1; i >= 0; i--)
        {
            Destroy(spawnPoint.GetChild(i).gameObject);
        }
    }
}
