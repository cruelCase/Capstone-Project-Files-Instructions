using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TaskManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject taskPanel;
    public Button[] taskButtons = new Button[3];
    
    [Header("Confirmation Panel")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI confirmationText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Task Completion Images")]
    public Image[] taskCompletionImages = new Image[3];

    [Header("Task Accepted UI")]
    public GameObject taskAcceptedPanel;
    public TextMeshProUGUI taskAcceptedText;
    public float taskAcceptedPanelDuration = 1f;

    [Header("Task Settings")]
    public int[] taskRewards = new int[] { 50, 100, 250 }; // Points for each task
    public float completionImageDuration = 2f;
    public int maxActiveTasks = 2; // Maximum tasks player can have active at once

    private int[] taskCompletionState = new int[3]; // 0 = not complete, 1 = complete
    private bool[] tasksAccepted = new bool[3]; // Track which tasks have been accepted
    private int activeTaskCount = 0; // How many tasks are currently active
    private int pendingTaskIndex = -1; // Which task's confirmation dialog is open
    private Coroutine taskAcceptedCoroutine;
    
    private string[] taskDescriptions = new string[]
    {
        "Kumuha ng isang hamon - Kumuha ng isang hamon mula sa paligid!",
        "Buksan ang tindahan - Hanapin at buksan ang tindahan ?",
        "Kumpletuhin ang tatlong MiniGame - E kumpleto ang tatlong hamon sa paligid!"
    };

    private static TaskManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Initialize UI
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        if (taskPanel != null)
            taskPanel.SetActive(false);

        // Hook button listeners
        for (int i = 0; i < taskButtons.Length; i++)
        {
            int taskIndex = i;
            if (taskButtons[i] != null)
                taskButtons[i].onClick.AddListener(() => OnTaskButtonPressed(taskIndex));
        }

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);

        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmNo);

        // Hide completion images
        for (int i = 0; i < taskCompletionImages.Length; i++)
        {
            if (taskCompletionImages[i] != null)
                taskCompletionImages[i].gameObject.SetActive(false);
        }

        if (taskAcceptedPanel != null)
            taskAcceptedPanel.SetActive(false);

        // Load task completion states from profile
        LoadTaskStates();
    }

    private void OnTaskButtonPressed(int taskIndex)
    {
        // Can't take a task that's already completed
        if (taskCompletionState[taskIndex] == 1)
            return;

        // Can't take a task that's already accepted
        if (tasksAccepted[taskIndex])
            return;

        // Can't accept more than max active tasks
        if (activeTaskCount >= maxActiveTasks)
        {
            Debug.Log($"[TaskManager] Gawain {maxActiveTasks} ay nasa aktibo. Kumpleto muna ang isang gawain.");
            return;
        }

        pendingTaskIndex = taskIndex;
        
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);

        if (confirmationText != null)
            confirmationText.text = taskDescriptions[taskIndex];
    }

    private void OnConfirmYes()
    {
        if (pendingTaskIndex < 0 || pendingTaskIndex >= 3)
        {
            OnConfirmNo();
            return;
        }

        // Mark task as accepted
        tasksAccepted[pendingTaskIndex] = true;
        activeTaskCount++;

        // Disable the button for the taken task immediately
        if (taskButtons[pendingTaskIndex] != null)
            taskButtons[pendingTaskIndex].interactable = false;

        ShowTaskAcceptedPanel(pendingTaskIndex);

        // Disable buttons if we've hit max active tasks
        UpdateButtonInteractability();

        // Handle task-specific logic
        switch (pendingTaskIndex)
        {
            case 0: // Task 1: Take 1 quest
                Debug.Log("[TaskManager] Ang manlalaro ay tumanggap ng task na Hamon");
                break;
            case 1: // Task 2: Open the shop
                Debug.Log("[TaskManager] Ang manlalaro ay tumanggap ng task na Tindahan");
                break;
            case 2: // Task 3: Complete three MiniGames
                Debug.Log("[TaskManager] Ang manlalaro ay tumanggap ng task na MiniGames");
                break;
        }

        OnConfirmNo();
    }

    private void ShowTaskAcceptedPanel(int taskIndex)
    {
        if (taskAcceptedText != null)
            taskAcceptedText.text = $"Gawain {taskIndex + 1} ay nakuha!";

        if (taskAcceptedPanel != null)
            taskAcceptedPanel.SetActive(true);

        if (taskAcceptedCoroutine != null)
            StopCoroutine(taskAcceptedCoroutine);

        taskAcceptedCoroutine = StartCoroutine(HideTaskAcceptedPanelAfterDelay(taskAcceptedPanelDuration));
    }

    private IEnumerator HideTaskAcceptedPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (taskAcceptedPanel != null)
            taskAcceptedPanel.SetActive(false);

        taskAcceptedCoroutine = null;
    }

    private void OnConfirmNo()
    {
        pendingTaskIndex = -1;
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    /// <summary>
    /// Public method called by the confirmation panel's YES button.
    /// </summary>
    public void takeTask()
    {
        OnConfirmYes();
    }

    /// <summary>
    /// Public method to mark a task as complete. Only works if task was accepted.
    /// </summary>
    public static void CompleteTask(int taskIndex)
    {
        if (instance == null)
            return;

        if (taskIndex < 0 || taskIndex >= 3)
            return;

        // Task must have been accepted to be completed
        if (!instance.tasksAccepted[taskIndex])
        {
            Debug.LogWarning($"[TaskManager] Gawain {taskIndex + 1} ay hindi nakuha, Hindi ma kumpleto.");
            return;
        }

        if (instance.taskCompletionState[taskIndex] == 1)
            return; // Already completed

        instance.taskCompletionState[taskIndex] = 1;
        instance.tasksAccepted[taskIndex] = false; // No longer active
        instance.activeTaskCount--; // Decrement active task count

        // Show completion image
        instance.StartCoroutine(instance.ShowCompletionImage(taskIndex));

        // Add points to player
        instance.AddPointsToPlayer(instance.taskRewards[taskIndex]);

        // Disable task button (completed tasks can't be retaken)
        if (instance.taskButtons[taskIndex] != null)
            instance.taskButtons[taskIndex].interactable = false;

        // Re-enable buttons now that a slot is free
        instance.UpdateButtonInteractability();

        // Save state (both tasks and points)
        instance.SaveTaskStates();

        Debug.Log($"[TaskManager] Gawain {taskIndex + 1} ay nakumpleto! actibong gawain: {instance.activeTaskCount}/{instance.maxActiveTasks}");
    }

    /// <summary>
    /// Check and complete Task 3 (three MiniGames) by reading player profile
    /// </summary>
    public static void CheckMiniGamesCompletion()
    {
        if (instance == null)
            return;

        ProfilePlayerData profile = instance.LoadActiveUserProfile();
        if (profile != null && profile.miniQuiz1Completed && profile.miniQuiz2Completed && profile.miniQuiz3Completed)
        {
            CompleteTask(2); // Task 3 index is 2
        }
    }

    private IEnumerator ShowCompletionImage(int taskIndex)
    {
        if (taskCompletionImages[taskIndex] != null)
        {
            taskCompletionImages[taskIndex].gameObject.SetActive(true);
            yield return new WaitForSeconds(completionImageDuration);
            taskCompletionImages[taskIndex].gameObject.SetActive(false);
        }
    }

    private void AddPointsToPlayer(int points)
    {
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile != null)
        {
            profile.points += points;
            SaveActiveUserProfile(profile);
            Debug.Log($"[TaskManager] Idinagdag ang {points} puntos. Kabuuan: {profile.points}");
        }
    }

    private void LoadTaskStates()
    {
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile != null)
        {
            // Load task completion states from profile
            if (profile.task0_1Completed)
            {
                taskCompletionState[0] = 1;
                if (taskButtons[0] != null)
                    taskButtons[0].interactable = false;
            }
            if (profile.task0_2Completed)
            {
                taskCompletionState[1] = 1;
                if (taskButtons[1] != null)
                    taskButtons[1].interactable = false;
            }
            if (profile.task0_3Completed)
            {
                taskCompletionState[2] = 1;
                if (taskButtons[2] != null)
                    taskButtons[2].interactable = false;
            }
            
            // Also check task 3 completion via MiniGames (legacy check)
            if (profile.miniQuiz1Completed && profile.miniQuiz2Completed && profile.miniQuiz3Completed)
            {
                taskCompletionState[2] = 1;
                if (taskButtons[2] != null)
                    taskButtons[2].interactable = false;
            }
        }
    }

    private void SaveTaskStates()
    {
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile != null)
        {
            profile.task0_1Completed = taskCompletionState[0] == 1;
            profile.task0_2Completed = taskCompletionState[1] == 1;
            profile.task0_3Completed = taskCompletionState[2] == 1;
            SaveActiveUserProfile(profile);
        }
    }

    private void UpdateButtonInteractability()
    {
        for (int i = 0; i < taskButtons.Length; i++)
        {
            if (taskButtons[i] == null)
                continue;

            bool isCompleted = taskCompletionState[i] == 1;
            bool isAccepted = tasksAccepted[i];
            bool isAtMaxTasks = activeTaskCount >= maxActiveTasks;

            bool shouldDisable = isCompleted || isAccepted || (isAtMaxTasks && !isAccepted);
            taskButtons[i].interactable = !shouldDisable;
        }
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
}
