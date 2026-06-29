using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapBehavior : MonoBehaviour
{
    [Header("World Identification")]
    [SerializeField] private int currentWorldNumber = 1; // 1=Town, 2=Central, 3=Mountain

    [Header("Location Buttons (Reused across worlds)")]
    [SerializeField] private Button locationButton1; // Top-left button
    [SerializeField] private Button locationButton2; // Top-right button
    [SerializeField] private Button locationButton3; // Bottom-left button
    [SerializeField] private Button locationButton4; // Bottom-right button

    [Header("Pinpoint GameObjects (Reused across worlds)")]
    [SerializeField] private GameObject pinpoint1; // Activates based on current world's quiz completion
    [SerializeField] private GameObject pinpoint2; // Activates based on current world's quiz completion
    [SerializeField] private GameObject pinpoint3; // Activates based on current world's quiz completion
    [SerializeField] private GameObject pinpoint4; // Activates based on current world's quiz completion

    [Header("Achievement Popup (Shows for 2 seconds when all quizzes completed)")]
    [SerializeField] private GameObject achievementPopupW1; // Shows when World 1 all quizzes completed
    [SerializeField] private GameObject achievementPopupW2; // Shows when World 2 all quizzes completed
    [SerializeField] private GameObject achievementPopupW3; // Shows when World 3 all quizzes completed

    [Header("Announcement Popup")]
    [SerializeField] private GameObject announcementPopup; // Shows after world progression achievement
    [SerializeField] private GameObject finalAnnouncementPopup; // Shows after World 3 achievement (game completion)

    [Header("World Progression Choice Panels (World 1 Only)")]
    [SerializeField] private GameObject choicePanel; // Main choice panel (Simple/Difficult)
    [SerializeField] private GameObject verificationPanel; // Verification panel for difficult choice
    [SerializeField] private Button simpleButton; // Simple choice button
    [SerializeField] private Button difficultButton; // Difficult choice button
    [SerializeField] private Button confirmDifficultButton; // Confirm difficult choice
    [SerializeField] private Button cancelDifficultButton; // Cancel difficult choice

    [Header("World Progression Choice Panels (World 2 Only)")]
    [SerializeField] private GameObject choicePanel2; // World 2 choice panel (Simple/Difficult)
    [SerializeField] private GameObject verificationPanel2; // World 2 verification panel
    [SerializeField] private Button simpleButton2; // World 2 simple choice button
    [SerializeField] private Button difficultButton2; // World 2 difficult choice button
    [SerializeField] private Button confirmDifficultButton2; // World 2 confirm difficult choice
    [SerializeField] private Button cancelDifficultButton2; // World 2 cancel difficult choice

    [Header("Scene References (Drag scene files here - more reliable than strings)")]
    [SerializeField] private UnityEngine.Object world1Scene; // Drag Town (World 1) scene file here
    [SerializeField] private UnityEngine.Object world2Scene; // Drag Central (World 2) scene file here
    [SerializeField] private UnityEngine.Object world3Scene; // Drag Mountain (World 3) scene file here

    [Header("Optional Location Scenes (Assign if you want specific scenes for these locations)")]
    [SerializeField] private UnityEngine.Object mountainScene; // Optional: specific scene for Mountain location
    [SerializeField] private UnityEngine.Object parkScene; // Optional: specific scene for Park location

    private GameManager gameManager;

    // Achievement popup tracking
    private float achievementPopupTimer = 0f;
    private const float ACHIEVEMENT_DISPLAY_TIME = 2f;
    private int currentAchievementPopupWorld = 0; // Tracks which world's achievement is currently showing
    private bool announcementQueued = false;

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        // Validate scene references
        ValidateSceneReferences();

        // Set up the correct world
        gameManager.SetActiveWorld(currentWorldNumber);

        // Configure buttons and pinpoints based on world
        ConfigureWorld();

        // Set up choice panel buttons
        SetupChoicePanelButtons();
    }

    private void SetupChoicePanelButtons()
    {
        // World 1 choice panel buttons
        if (simpleButton != null)
        {
            simpleButton.onClick.AddListener(OnSimpleChoiceSelected);
        }

        if (difficultButton != null)
        {
            difficultButton.onClick.AddListener(OnDifficultChoiceSelected);
        }

        if (confirmDifficultButton != null)
        {
            confirmDifficultButton.onClick.AddListener(OnConfirmDifficultChoice);
        }

        if (cancelDifficultButton != null)
        {
            cancelDifficultButton.onClick.AddListener(OnCancelDifficultChoice);
        }

        // World 2 choice panel buttons
        if (simpleButton2 != null)
        {
            simpleButton2.onClick.AddListener(OnSimpleChoiceSelectedWorld2);
        }

        if (difficultButton2 != null)
        {
            difficultButton2.onClick.AddListener(OnDifficultChoiceSelectedWorld2);
        }

        if (confirmDifficultButton2 != null)
        {
            confirmDifficultButton2.onClick.AddListener(OnConfirmDifficultChoiceWorld2);
        }

        if (cancelDifficultButton2 != null)
        {
            cancelDifficultButton2.onClick.AddListener(OnCancelDifficultChoiceWorld2);
        }

        // Hide panels initially
        if (choicePanel != null) choicePanel.SetActive(false);
        if (verificationPanel != null) verificationPanel.SetActive(false);
        if (choicePanel2 != null) choicePanel2.SetActive(false);
        if (verificationPanel2 != null) verificationPanel2.SetActive(false);
    }

    private void ValidateSceneReferences()
    {
        if (world1Scene == null)
        {
            Debug.LogWarning("MapBehavior: World 1 scene reference is not assigned.");
        }

        if (world2Scene == null)
        {
            Debug.LogWarning("MapBehavior: World 2 scene reference is not assigned.");
        }

        if (world3Scene == null)
        {
            Debug.LogWarning("MapBehavior: World 3 scene reference is not assigned.");
        }
    }

    private void Update()
    {
        // Continuously check quiz completion to enable Central button
        UpdateButtonInteractability();
        UpdatePinpoints();
        UpdateAchievementPopups();
    }

    private void ConfigureWorld()
    {
        // Clear all existing listeners first
        ClearButtonListeners();

        switch (currentWorldNumber)
        {
            case 1:
                ConfigureWorld1();
                break;
            case 2:
                ConfigureWorld2();
                break;
            case 3:
                ConfigureWorld3();
                break;
        }
    }

    private void ClearButtonListeners()
    {
        if (locationButton1 != null) locationButton1.onClick.RemoveAllListeners();
        if (locationButton2 != null) locationButton2.onClick.RemoveAllListeners();
        if (locationButton3 != null) locationButton3.onClick.RemoveAllListeners();
        if (locationButton4 != null) locationButton4.onClick.RemoveAllListeners();
    }

    private void ConfigureWorld1()
    {
        // World 1 Layout: Town (Current) | Central | Mountain | Park

        if (locationButton1 != null)
        {
            locationButton1.interactable = true;
            locationButton1.onClick.AddListener(() => OnLocationClicked("CurrentLoc", 1));
            // You can set button text/image here if needed
        }

        if (locationButton2 != null)
        {
            locationButton2.interactable = false; // Disabled until all quizzes complete
            locationButton2.onClick.AddListener(() => OnLocationClicked("Central", 1));
        }

        if (locationButton3 != null)
        {
            locationButton3.interactable = false;
            locationButton3.onClick.AddListener(() => OnLocationClicked("Mountain", 1));
        }

        if (locationButton4 != null)
        {
            locationButton4.interactable = false;
            locationButton4.onClick.AddListener(() => OnLocationClicked("Park", 1));
        }
    }

    private void ConfigureWorld2()
    {
        // World 2 Layout: Town | Central (Current) | Mountain | Park

        if (locationButton1 != null)
        {
            locationButton1.interactable = true; // Town Loc is always interactable
            locationButton1.onClick.AddListener(() => OnLocationClicked("Town", 2));
        }

        if (locationButton2 != null)
        {
            locationButton2.interactable = true; // Current loc always interactable but does nothing
            locationButton2.onClick.AddListener(() => OnLocationClicked("CurrentLoc", 2));
        }

        if (locationButton3 != null)
        {
            locationButton3.interactable = false;
            locationButton3.onClick.AddListener(() => OnLocationClicked("Mountain", 2));
        }

        if (locationButton4 != null)
        {
            locationButton4.interactable = false;
            locationButton4.onClick.AddListener(() => OnLocationClicked("Park", 2));
        }
    }

    private void ConfigureWorld3()
    {
        // World 3 Layout: Town | Central | Mountain (Current) | Park

        if (locationButton1 != null)
        {
            locationButton1.interactable = false;
            locationButton1.onClick.AddListener(() => OnLocationClicked("Town", 3));
        }

        if (locationButton2 != null)
        {
            locationButton2.interactable = false;
            locationButton2.onClick.AddListener(() => OnLocationClicked("Central", 3));
        }

        if (locationButton3 != null)
        {
            locationButton3.interactable = true;
            locationButton3.onClick.AddListener(() => OnLocationClicked("CurrentLoc", 3));
        }

        if (locationButton4 != null)
        {
            locationButton4.interactable = false;
            locationButton4.onClick.AddListener(() => OnLocationClicked("Mountain", 3));
        }
    }

    private void UpdateButtonInteractability()
    {
        if (gameManager == null) return;

        switch (currentWorldNumber)
        {
            case 1:
                UpdateWorld1Buttons();
                break;
            case 2:
                UpdateWorld2Buttons();
                break;
            case 3:
                UpdateWorld3Buttons();
                break;
        }
    }

    private void UpdateWorld1Buttons()
    {
        // Enable Central button (locationButton2) only when ALL quizzes in World 1 are completed
        bool allQuizzesCompleted = gameManager.MiniQuiz1Completed && 
                                  gameManager.MiniQuiz2Completed && 
                                  gameManager.MiniQuiz3Completed && 
                                  gameManager.MainQuizCompleted;

        if (locationButton2 != null)
        {
            locationButton2.interactable = allQuizzesCompleted;
        }
    }

    private void UpdateWorld2Buttons()
    {
        // In World 2, enable Mountain (locationButton3) and Park (locationButton4) based on World 2 quiz completion
        bool allQuizzesCompleted = gameManager.MiniQuiz1bCompleted && 
                                  gameManager.MiniQuiz2bCompleted && 
                                  gameManager.MiniQuiz3bCompleted && 
                                  gameManager.MainQuizBCompleted;

        if (locationButton3 != null)
        {
            locationButton3.interactable = allQuizzesCompleted;
        }

        if (locationButton4 != null)
        {
            locationButton4.interactable = allQuizzesCompleted;
        }
    }

    private void UpdateWorld3Buttons()
    {
        // In World 3, enable Town (locationButton1) and Central (locationButton2) based on World 3 quiz completion
        bool allQuizzesCompleted = gameManager.MiniQuiz1cCompleted && 
                                  gameManager.MiniQuiz2cCompleted && 
                                  gameManager.MiniQuiz3cCompleted && 
                                  gameManager.MainQuizCCompleted;

        if (locationButton1 != null)
        {
            locationButton1.interactable = allQuizzesCompleted;
        }

        if (locationButton2 != null)
        {
            locationButton2.interactable = allQuizzesCompleted;
        }
    }

    private void UpdatePinpoints()
    {
        if (gameManager == null) return;

        // Deactivate all pinpoints first
        DeactivateAllPinpoints();

        // Activate pinpoints based on current world and quiz completion
        switch (currentWorldNumber)
        {
            case 1:
                UpdateWorld1Pinpoints();
                break;
            case 2:
                UpdateWorld2Pinpoints();
                break;
            case 3:
                UpdateWorld3Pinpoints();
                break;
        }
    }

    private void DeactivateAllPinpoints()
    {
        if (pinpoint1 != null) pinpoint1.SetActive(false);
        if (pinpoint2 != null) pinpoint2.SetActive(false);
        if (pinpoint3 != null) pinpoint3.SetActive(false);
        if (pinpoint4 != null) pinpoint4.SetActive(false);
    }

    private void UpdateWorld1Pinpoints()
    {
        // Activate pinpoints based on World 1 quiz completion
        if (pinpoint1 != null)
        {
            pinpoint1.SetActive(gameManager.MiniQuiz1Completed);
        }

        if (pinpoint2 != null)
        {
            pinpoint2.SetActive(gameManager.MiniQuiz2Completed);
        }

        if (pinpoint3 != null)
        {
            pinpoint3.SetActive(gameManager.MiniQuiz3Completed);
        }

        if (pinpoint4 != null)
        {
            pinpoint4.SetActive(gameManager.MainQuizCompleted);
        }
    }

    private void UpdateWorld2Pinpoints()
    {
        // Activate pinpoints based on World 2 quiz completion
        if (pinpoint1 != null)
        {
            pinpoint1.SetActive(gameManager.MiniQuiz1bCompleted);
        }

        if (pinpoint2 != null)
        {
            pinpoint2.SetActive(gameManager.MiniQuiz2bCompleted);
        }

        if (pinpoint3 != null)
        {
            pinpoint3.SetActive(gameManager.MiniQuiz3bCompleted);
        }

        if (pinpoint4 != null)
        {
            pinpoint4.SetActive(gameManager.MainQuizBCompleted);
        }
    }

    private void UpdateWorld3Pinpoints()
    {
        // Activate pinpoints based on World 3 quiz completion
        if (pinpoint1 != null)
        {
            pinpoint1.SetActive(gameManager.MiniQuiz1cCompleted);
        }

        if (pinpoint2 != null)
        {
            pinpoint2.SetActive(gameManager.MiniQuiz2cCompleted);
        }

        if (pinpoint3 != null)
        {
            pinpoint3.SetActive(gameManager.MiniQuiz3cCompleted);
        }

        if (pinpoint4 != null)
        {
            pinpoint4.SetActive(gameManager.MainQuizCCompleted);
        }
    }

    private void UpdateAchievementPopups()
    {
        if (gameManager == null) return;

        // Handle achievement popup timer
        if (currentAchievementPopupWorld > 0)
        {
            achievementPopupTimer -= Time.deltaTime;
            if (achievementPopupTimer <= 0)
            {
                HideCurrentAchievementPopup();
                currentAchievementPopupWorld = 0;
            }
        }

        // Check and show achievement popups based on current world
        switch (currentWorldNumber)
        {
            case 1:
                CheckAndShowAchievementW1();
                break;
            case 2:
                CheckAndShowAchievementW2();
                break;
            case 3:
                CheckAndShowAchievementW3();
                break;
        }

        if (currentAchievementPopupWorld == 0 && !announcementQueued)
        {
            ShowAnnouncementIfEligible();
        }
    }

    private void CheckAndShowAchievementW1()
    {
        bool allQuizzesCompleted = gameManager.MiniQuiz1Completed && 
                                  gameManager.MiniQuiz2Completed && 
                                  gameManager.MiniQuiz3Completed && 
                                  gameManager.MainQuizCompleted;

        if (allQuizzesCompleted && !gameManager.HasShownAchievementW1)
        {
            ShowAchievementPopup(1);
            gameManager.MarkAchievementShown(1);
            // Schedule announcement popup to show after achievement popup disappears
            StartCoroutine(ShowAnnouncementAfterDelay(1, ACHIEVEMENT_DISPLAY_TIME + 0.5f));
        }
    }

    private void CheckAndShowAchievementW2()
    {
        bool allQuizzesCompleted = gameManager.MiniQuiz1bCompleted && 
                                  gameManager.MiniQuiz2bCompleted && 
                                  gameManager.MiniQuiz3bCompleted && 
                                  gameManager.MainQuizBCompleted;

        if (allQuizzesCompleted && !gameManager.HasShownAchievementW2)
        {
            ShowAchievementPopup(2);
            gameManager.MarkAchievementShown(2);
            // Schedule announcement popup to show after achievement popup disappears
            StartCoroutine(ShowAnnouncementAfterDelay(2, ACHIEVEMENT_DISPLAY_TIME + 0.5f));
        }
    }

    private void CheckAndShowAchievementW3()
    {
        bool allQuizzesCompleted = gameManager.MiniQuiz1cCompleted && 
                                  gameManager.MiniQuiz2cCompleted && 
                                  gameManager.MiniQuiz3cCompleted && 
                                  gameManager.MainQuizCCompleted;

        if (allQuizzesCompleted && !gameManager.HasShownAchievementW3)
        {
            ShowAchievementPopup(3);
            gameManager.MarkAchievementShown(3);
            // Schedule final announcement popup to show after achievement popup disappears
            StartCoroutine(ShowFinalAnnouncementAfterDelay(ACHIEVEMENT_DISPLAY_TIME + 0.5f));
        }
    }

    private void ShowAchievementPopup(int world)
    {
        // Hide the previous popup if one is showing
        HideCurrentAchievementPopup();

        // Show the appropriate achievement popup
        GameObject popupToShow = null;
        switch (world)
        {
            case 1:
                popupToShow = achievementPopupW1;
                break;
            case 2:
                popupToShow = achievementPopupW2;
                break;
            case 3:
                popupToShow = achievementPopupW3;
                break;
        }

        if (popupToShow != null)
        {
            popupToShow.SetActive(true);
            currentAchievementPopupWorld = world;
            achievementPopupTimer = ACHIEVEMENT_DISPLAY_TIME;
            Debug.Log($"Achievement popup shown for World {world}");
        }
    }

    private void HideCurrentAchievementPopup()
    {
        if (currentAchievementPopupWorld == 1 && achievementPopupW1 != null)
        {
            achievementPopupW1.SetActive(false);
        }
        else if (currentAchievementPopupWorld == 2 && achievementPopupW2 != null)
        {
            achievementPopupW2.SetActive(false);
        }
        else if (currentAchievementPopupWorld == 3 && achievementPopupW3 != null)
        {
            achievementPopupW3.SetActive(false);
        }
    }

    private void ShowAnnouncementIfEligible()
    {
        // Show announcement popup if it's not already shown and conditions are met
        if (announcementPopup != null && !announcementPopup.activeSelf)
        {
            // Only show if player can progress to next world and announcement hasn't been shown
            if (currentWorldNumber == 1)
            {
                bool canProgressToWorld2 = gameManager.MiniQuiz1Completed && 
                                         gameManager.MiniQuiz2Completed && 
                                         gameManager.MiniQuiz3Completed && 
                                         gameManager.MainQuizCompleted;

                if (canProgressToWorld2 && !gameManager.HasShownAnnouncementW1)
                {
                    announcementPopup.SetActive(true);
                    gameManager.MarkAnnouncementShown(1);
                    Debug.Log("Announcement: You can now go to Central!");
                }
            }
            else if (currentWorldNumber == 2)
            {
                bool canProgressToWorld3 = gameManager.MiniQuiz1bCompleted && 
                                         gameManager.MiniQuiz2bCompleted && 
                                         gameManager.MiniQuiz3bCompleted && 
                                         gameManager.MainQuizBCompleted;

                if (canProgressToWorld3 && !gameManager.HasShownAnnouncementW2)
                {
                    announcementPopup.SetActive(true);
                    gameManager.MarkAnnouncementShown(2);
                    Debug.Log("Announcement: You can now go to Mountain!");
                }
            }
        }
    }

    private System.Collections.IEnumerator ShowAnnouncementAfterDelay(int world, float delay)
    {
        announcementQueued = true;
        yield return new WaitForSeconds(delay);

        if (announcementPopup != null && !announcementPopup.activeSelf)
        {
            announcementPopup.SetActive(true);
            if (world == 1)
            {
                gameManager.MarkAnnouncementShown(1);
                Debug.Log("Announcement: You can now go to Central!");
            }
            else if (world == 2)
            {
                gameManager.MarkAnnouncementShown(2);
                Debug.Log("Announcement: You can now go to Mountain!");
            }
        }

        announcementQueued = false;
    }

    private System.Collections.IEnumerator ShowFinalAnnouncementAfterDelay(float delay)
    {
        announcementQueued = true;
        yield return new WaitForSeconds(delay);

        if (finalAnnouncementPopup != null && !finalAnnouncementPopup.activeSelf)
        {
            finalAnnouncementPopup.SetActive(true);
            gameManager.MarkFinalAnnouncementShown();
            Debug.Log("Final Announcement: You have completed all worlds!");
        }

        announcementQueued = false;
    }

    private void OnSimpleChoiceSelected()
    {
        Debug.Log("Simple choice selected - deducting currency and moving to World 2");

        // Find and call TamaMaliQuizManager's simple choice function
        TamaMaliQuizManager quizManager = FindObjectOfType<TamaMaliQuizManager>();
        if (quizManager != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MarkFirstWorld1ToWorld2TransitionDone();
            }
            quizManager.ChooseSimplePath();
        }
        else
        {
            Debug.LogError("TamaMaliQuizManager not found in scene!");
            // Fallback: just load World 2
            if (world2Scene != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.MarkFirstWorld1ToWorld2TransitionDone();
                }
                SceneManager.LoadScene(world2Scene.name);
            }
        }

        // Hide choice panel
        if (choicePanel != null) choicePanel.SetActive(false);
    }

    private void OnDifficultChoiceSelected()
    {
        Debug.Log("Difficult choice selected - showing verification panel");

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MapBehavior: choicePanel reference is missing.");
        }

        if (verificationPanel != null)
        {
            verificationPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MapBehavior: verificationPanel reference is missing.");
        }
    }

    private void OnConfirmDifficultChoice()
    {
        Debug.Log("Difficult choice confirmed - starting quiz");

        // Find and start the quiz
        TamaMaliQuizManager quizManager = FindObjectOfType<TamaMaliQuizManager>();
        if (quizManager != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MarkFirstWorld1ToWorld2TransitionDone();
            }
            quizManager.InitializeQuiz();
        }
        else
        {
            Debug.LogError("TamaMaliQuizManager not found in scene!");
        }

        // Hide verification panel
        if (verificationPanel != null) verificationPanel.SetActive(false);
    }

    private void OnCancelDifficultChoice()
    {
        Debug.Log("Difficult choice cancelled - returning to choice panel");

        // Hide verification panel and show choice panel again
        if (verificationPanel != null) verificationPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(true);
    }

    private void OnSimpleChoiceSelectedWorld2()
    {
        Debug.Log("Simple choice selected for World 2 -> World 3 - moving to World 3");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.MarkFirstWorld2ToWorld3TransitionDone();
        }

        if (world3Scene != null)
        {
            SceneManager.LoadScene(world3Scene.name);
        }
        else
        {
            Debug.LogWarning("World 3 scene not assigned!");
        }

        // Hide choice panel
        if (choicePanel2 != null) choicePanel2.SetActive(false);
    }

    private void OnDifficultChoiceSelectedWorld2()
    {
        Debug.Log("Difficult choice selected for World 2 - showing verification panel");

        if (choicePanel2 != null)
        {
            choicePanel2.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MapBehavior: choicePanel2 reference is missing.");
        }

        if (verificationPanel2 != null)
        {
            verificationPanel2.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MapBehavior: verificationPanel2 reference is missing.");
        }
    }

    private void OnConfirmDifficultChoiceWorld2()
    {
        Debug.Log("Difficult choice confirmed for World 2 - starting FourPicsOneWordGame");

        // Find and start the Four Pics One Word game
        FourPicsOneWordManager fourPicsManager = FindObjectOfType<FourPicsOneWordManager>();
        if (fourPicsManager != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MarkFirstWorld2ToWorld3TransitionDone();
            }
            fourPicsManager.StartGame();
        }
        else
        {
            Debug.LogError("FourPicsOneWordManager not found in scene!");
        }

        // Hide verification panel
        if (verificationPanel2 != null) verificationPanel2.SetActive(false);
    }

    private void OnCancelDifficultChoiceWorld2()
    {
        Debug.Log("Difficult choice cancelled for World 2 - returning to choice panel");

        // Hide verification panel and show choice panel again
        if (verificationPanel2 != null) verificationPanel2.SetActive(false);
        if (choicePanel2 != null) choicePanel2.SetActive(true);
    }

    private void OnLocationClicked(string location, int fromWorld)
    {
        Debug.Log($"Clicked {location} in World {fromWorld}");

        switch (location)
        {
            case "CurrentLoc":
                // Does nothing - player is already at current location
                Debug.Log("Already at current location");
                break;

            case "Central":
                // Special handling for World 1 - show choice panel on first transition
                if (fromWorld == 1)
                {
                    // Check if this is the first-time World 1 -> World 2 transition for this user
                    bool isFirstTransition = !gameManager.HasDoneFirstWorld1ToWorld2Transition;

                    if (isFirstTransition && choicePanel != null)
                    {
                        // Show choice panel instead of directly loading World 2
                        choicePanel.SetActive(true);
                        Debug.Log("Showing choice panel for first World 1 to World 2 transition");
                    }
                    else
                    {
                        // Not first time or no choice panel - go directly to World 2
                        if (world2Scene != null)
                        {
                            SceneManager.LoadScene(world2Scene.name);
                        }
                        else
                        {
                            Debug.LogWarning("World 2 scene not assigned!");
                        }
                    }
                }
                else
                {
                    // For other worlds, handle Central location normally
                    Debug.Log("Central location selected from World " + fromWorld);
                }
                break;

            case "Town":
                // Go back to World 1
                if ((fromWorld == 2 || fromWorld == 3) && world1Scene != null)
                {
                    SceneManager.LoadScene(world1Scene.name);
                }
                else
                {
                    Debug.LogWarning("World 1 scene not assigned!");
                }
                break;

            case "Mountain":
                if (fromWorld == 2)
                {
                    // First-time World 2 -> World 3 transition shows choice panel
                    bool isFirstTransition = !gameManager.HasDoneFirstWorld2ToWorld3Transition;
                    if (isFirstTransition && choicePanel2 != null)
                    {
                        // Show choice panel instead of directly loading World 3
                        choicePanel2.SetActive(true);
                        Debug.Log("Showing choice panel for first World 2 to World 3 transition");
                    }
                    else
                    {
                        // Not first time or no choice panel - go directly to World 3
                        if (world3Scene != null)
                        {
                            SceneManager.LoadScene(world3Scene.name);
                        }
                        else if (mountainScene != null)
                        {
                            SceneManager.LoadScene(mountainScene.name);
                        }
                        else
                        {
                            Debug.LogWarning("World 3 scene not assigned for Mountain transition.");
                        }
                    }
                }
                else
                {
                    // Handle Mountain location loading for other worlds
                    if (mountainScene != null)
                    {
                        SceneManager.LoadScene(mountainScene.name);
                    }
                    else
                    {
                        Debug.Log("Mountain location selected - no specific scene assigned");
                    }
                }
                break;

            case "Park":
                // Handle Park location loading
                if (parkScene != null)
                {
                    SceneManager.LoadScene(parkScene.name);
                }
                else
                {
                    Debug.Log("Park location selected - no specific scene assigned");
                }
                break;
        }
    }
}
