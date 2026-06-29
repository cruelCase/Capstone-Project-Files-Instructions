using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TaskManager1 : MonoBehaviour
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
    public int[] taskRewards = new int[] { 50, 100, 250 }; // Points for each task completion
    public float completionImageDuration = 2f;
    public int maxActiveTasks = 2;

    private int[] taskCompletionState = new int[3]; // 0 = not complete, 1 = complete
    private bool[] tasksAccepted = new bool[3];
    private int activeTaskCount = 0;
    private int pendingTaskIndex = -1;
    private Coroutine taskAcceptedCoroutine;
    
    private string[] taskDescriptions = new string[]
    {
        "Kumpletuhin ang task: Makakuha ng 1500 puntos.",
        "Kumpletuhin ang task: Magkaroon ng 750 pera.",
        "Kumpletuhin ang task: Tapusin ang 3 mini-laro (miniQuiz1b, miniQuiz2b, miniQuiz3b)."
    };

    private static TaskManager1 instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        if (taskPanel != null)
            taskPanel.SetActive(false);

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

        for (int i = 0; i < taskCompletionImages.Length; i++)
        {
            if (taskCompletionImages[i] != null)
                taskCompletionImages[i].gameObject.SetActive(false);
        }

        if (taskAcceptedPanel != null)
            taskAcceptedPanel.SetActive(false);

        LoadTaskStates();
    }

    private void OnTaskButtonPressed(int taskIndex)
    {
        if (taskCompletionState[taskIndex] == 1)
            return;

        if (tasksAccepted[taskIndex])
            return;

        if (activeTaskCount >= maxActiveTasks)
        {
            Debug.Log($"[TaskManager1] Maximum {maxActiveTasks} gawain ay aktibo. Kumpletuhin muna ang isa.");
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

        tasksAccepted[pendingTaskIndex] = true;
        activeTaskCount++;

        if (taskButtons[pendingTaskIndex] != null)
            taskButtons[pendingTaskIndex].interactable = false;

        ShowTaskAcceptedPanel(pendingTaskIndex);
        UpdateButtonInteractability();

        switch (pendingTaskIndex)
        {
            case 0:
                Debug.Log("[TaskManager1] Tinanggap ang gawain: 1500 puntos.");
                break;
            case 1:
                Debug.Log("[TaskManager1] Tinanggap ang gawain: 750 pera.");
                break;
            case 2:
                Debug.Log("[TaskManager1] Tinanggap ang gawain: 3 mini-laro.");
                break;
        }

        OnConfirmNo();
    }

    private void ShowTaskAcceptedPanel(int taskIndex)
    {
        if (taskAcceptedText != null)
            taskAcceptedText.text = $"Gawain {taskIndex + 1} ay tinanggap!";

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

    public void takeTask()
    {
        OnConfirmYes();
    }

    public static void CompleteTask(int taskIndex)
    {
        if (instance == null)
            return;

        if (taskIndex < 0 || taskIndex >= 3)
            return;

        if (!instance.tasksAccepted[taskIndex])
        {
            Debug.LogWarning($"[TaskManager1] Gawain {taskIndex + 1} hindi pa tinanggap. Hindi pwedeng kumpletuhin.");
            return;
        }

        if (instance.taskCompletionState[taskIndex] == 1)
            return;

        instance.taskCompletionState[taskIndex] = 1;
        instance.tasksAccepted[taskIndex] = false;
        instance.activeTaskCount--;

        instance.StartCoroutine(instance.ShowCompletionImage(taskIndex));
        instance.AddPointsToPlayer(instance.taskRewards[taskIndex]);

        if (instance.taskButtons[taskIndex] != null)
            instance.taskButtons[taskIndex].interactable = false;

        instance.UpdateButtonInteractability();
        instance.SaveTaskStates();

        Debug.Log($"[TaskManager1] Gawain {taskIndex + 1} kumpleto na! Aktibong gawain: {instance.activeTaskCount}/{instance.maxActiveTasks}");
    }

    public static void CheckTaskProgress()
    {
        if (instance == null)
            return;

        ProfilePlayerData profile = instance.LoadActiveUserProfile();
        if (profile == null)
            return;

        if (profile.points >= 1500 && instance.tasksAccepted[0])
            CompleteTask(0);

        if (profile.currency >= 750 && instance.tasksAccepted[1])
            CompleteTask(1);

        if (profile.miniQuiz1bCompleted && profile.miniQuiz2bCompleted && profile.miniQuiz3bCompleted && instance.tasksAccepted[2])
            CompleteTask(2);
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
            Debug.Log($"[TaskManager1] Idinagdag ang {points} puntos. Kabuuan: {profile.points}");
        }
    }

    private void LoadTaskStates()
    {
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile != null)
        {
            // Load task completion states from profile (not from game progress)
            if (profile.task1_1Completed)
            {
                taskCompletionState[0] = 1;
                if (taskButtons[0] != null)
                    taskButtons[0].interactable = false;
            }
            if (profile.task1_2Completed)
            {
                taskCompletionState[1] = 1;
                if (taskButtons[1] != null)
                    taskButtons[1].interactable = false;
            }
            if (profile.task1_3Completed)
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
            profile.task1_1Completed = taskCompletionState[0] == 1;
            profile.task1_2Completed = taskCompletionState[1] == 1;
            profile.task1_3Completed = taskCompletionState[2] == 1;
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
