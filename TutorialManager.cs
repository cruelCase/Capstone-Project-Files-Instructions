using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Panels - Assign Based on Hero")]
    public GameObject[] tutorialPanels;      // hero 0 (e.g., Gabriela Silang)
    public GameObject[] tutorialPanels2;     // hero 1 (e.g., Jose Rizal)
    public GameObject[] tutorialPanels3;     // hero 2 (e.g., Apolinario Mabini)
    public GameObject[] tutorialPanels4;     // hero 3 (e.g., Emilio Jacinto)

    [Header("Hero Names (Match Panel Set Order)")]
    public string[] heroNames;               // names matching the tutorial panel order above

    [Header("Main Buttons")]
    public Button profileButton;
    public Button achievementButton;
    public Button mapButton;
    public Button settingsButton;
    public Button finalButton;

    [Header("Temporary Block")]
    public Transform temporaryBlockButton;   // assign the button transform that closes the temporary block
    public Transform temporaryBlockButton2;  // additional temporary block button
    public Transform temporaryBlockButton3;  // additional temporary block button
    public Transform temporaryBlockButton4;  // additional temporary block button
    public Transform temporaryBlockObject;   // assign the temporary object to deactivate permanently for this user

    private int currentStep = 0;
    private string tutorialKey;
    private string temporaryBlockKey;
    private GameObject[] currentTutorialPanels;

    private void Start()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "Guest");
        tutorialKey = activeUser + "_TutorialCompleted";
        temporaryBlockKey = activeUser + "_TemporaryBlockCompleted";

        if (temporaryBlockObject != null && PlayerPrefs.GetInt(temporaryBlockKey, 0) == 1)
        {
            temporaryBlockObject.gameObject.SetActive(false);
        }

        if (temporaryBlockButton != null)
        {
            Button tempButton = temporaryBlockButton.GetComponent<Button>();
            if (tempButton != null)
                tempButton.onClick.AddListener(OnTemporaryBlockButtonClicked);
        }

        if (temporaryBlockButton2 != null)
        {
            Button tempButton2 = temporaryBlockButton2.GetComponent<Button>();
            if (tempButton2 != null)
                tempButton2.onClick.AddListener(OnTemporaryBlockButtonClicked);
        }

        if (temporaryBlockButton3 != null)
        {
            Button tempButton3 = temporaryBlockButton3.GetComponent<Button>();
            if (tempButton3 != null)
                tempButton3.onClick.AddListener(OnTemporaryBlockButtonClicked);
        }

        if (temporaryBlockButton4 != null)
        {
            Button tempButton4 = temporaryBlockButton4.GetComponent<Button>();
            if (tempButton4 != null)
                tempButton4.onClick.AddListener(OnTemporaryBlockButtonClicked);
        }

        SelectTutorialPanelsByHero();

        if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
        {
            DisableAllPanels();
            UnlockAllButtons();
            return;
        }

        currentStep = 0;

        DisableAllPanels();
        LockAllButtons();
        ShowCurrentStep();
    }


    private void DisableAllPanels()
    {
        DisablePanelArray(tutorialPanels);
        DisablePanelArray(tutorialPanels2);
        DisablePanelArray(tutorialPanels3);
        DisablePanelArray(tutorialPanels4);
    }

    private void DisablePanelArray(GameObject[] panels)
    {
        if (panels == null)
            return;

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(false);
            }
        }
    }

    private void LockAllButtons()
    {
        if (profileButton != null)
            profileButton.interactable = false;

        if (achievementButton != null)
            achievementButton.interactable = false;

        if (mapButton != null)
            mapButton.interactable = false;

        if (settingsButton != null)
            settingsButton.interactable = false;

        if (finalButton != null)
            finalButton.interactable = false;
    }

    private void UnlockAllButtons()
    {
        if (profileButton != null)
            profileButton.interactable = true;

        if (achievementButton != null)
            achievementButton.interactable = true;

        if (mapButton != null)
            mapButton.interactable = true;

        if (settingsButton != null)
            settingsButton.interactable = true;

        if (finalButton != null)
            finalButton.interactable = true;
    }

    private void ShowCurrentStep()
    {
        DisableAllPanels();
        LockAllButtons();

        if (currentTutorialPanels == null || currentTutorialPanels.Length == 0)
            return;

        if (currentStep < currentTutorialPanels.Length)
        {
            if (currentTutorialPanels[currentStep] != null)
            {
                currentTutorialPanels[currentStep].SetActive(true);
            }

            // Unlock only required button per step
            if (currentStep == 1) // Step2A - Open Profile
            {
                if (profileButton != null)
                    profileButton.interactable = true;
            }
            else if (currentStep == 3) // Step3A - Open Achievement
            {
                if (achievementButton != null)
                    achievementButton.interactable = true;
            }
            else if (currentStep == 5) // Step4A - Open Map
            {
                if (mapButton != null)
                    mapButton.interactable = true;
            }
            else if (currentStep == 7) // Step5 - Open Settings
            {
                if (settingsButton != null)
                    settingsButton.interactable = true;
            }
            else if (currentStep == 10) // Step8 - Final special button
            {
                if (finalButton != null)
                    finalButton.interactable = true;
            }
        }
        else
        {
            CompleteTutorial();
        }
    }

    // Called by Next buttons inside tutorial panels
    public void NextStep()
    {
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
            return;

        if (currentStep < currentTutorialPanels.Length)
        {
            if (currentTutorialPanels[currentStep] != null)
            {
                currentTutorialPanels[currentStep].SetActive(false);
            }
        }

        currentStep++;
        ShowCurrentStep();
    }

    // Called when Profile button is clicked
    public void OnProfileOpened()
    {
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
            return;

        if (currentStep == 1)
        {
            NextStep();
        }
    }

    // Called when Achievement button is clicked
    public void OnAchievementOpened()
    {
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
            return;

        if (currentStep == 3)
        {
            NextStep();
        }
    }

    // Called when Map button is clicked
    public void OnMapOpened()
    {
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
            return;

        if (currentStep == 5)
        {
            NextStep();
        }
    }

    // Called when Settings button is clicked
    public void OnSettingsOpened()
    {
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
            return;

        if (currentStep == 7)
        {
            NextStep();
        }
    }

    // Called by final required button
    public void OnFinalButtonPressed()
    {
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
            return;

        if (currentStep == 10)
        {
            NextStep();
        }
    }

    private void CompleteTutorial()
    {
        PlayerPrefs.SetInt(tutorialKey, 1);
        PlayerPrefs.Save();

        DisableAllPanels();
        UnlockAllButtons();

        Debug.Log("Tutorial Finished Permanently.");
    }

    // Called by the temporary block button when the user finishes the one-time block
    public void OnTemporaryBlockButtonClicked()
    {
        if (temporaryBlockObject != null)
        {
            temporaryBlockObject.gameObject.SetActive(false);
            PlayerPrefs.SetInt(temporaryBlockKey, 1);
            PlayerPrefs.Save();
            Debug.Log("Temporary block completed for active user.");
        }
    }

    // Optional testing function
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(tutorialKey);
        PlayerPrefs.DeleteKey(temporaryBlockKey);
        currentStep = 0;
        DisableAllPanels();
        LockAllButtons();
        ShowCurrentStep();
    }

    private void SelectTutorialPanelsByHero()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "Guest");
        if (string.IsNullOrEmpty(activeUser))
        {
            currentTutorialPanels = tutorialPanels;
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
        {
            currentTutorialPanels = tutorialPanels;
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        if (profile == null || string.IsNullOrEmpty(profile.hero))
        {
            currentTutorialPanels = tutorialPanels;
            return;
        }

        string selectedHero = profile.hero.Trim().ToLower();
        int panelSetIndex = 0;

        if (heroNames != null && heroNames.Length > 0)
        {
            for (int i = 0; i < heroNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(heroNames[i]))
                {
                    string heroNameValue = heroNames[i].Trim().ToLower();
                    if (heroNameValue == selectedHero || heroNameValue.Contains(selectedHero) || selectedHero.Contains(heroNameValue))
                    {
                        panelSetIndex = i;
                        break;
                    }
                }
            }
        }

        switch (panelSetIndex)
        {
            case 0:
                currentTutorialPanels = tutorialPanels;
                break;
            case 1:
                currentTutorialPanels = tutorialPanels2;
                break;
            case 2:
                currentTutorialPanels = tutorialPanels3;
                break;
            case 3:
                currentTutorialPanels = tutorialPanels4;
                break;
            default:
                currentTutorialPanels = tutorialPanels;
                break;
        }

        Debug.Log($"TutorialManager: Selected tutorial panel set {panelSetIndex} for hero '{profile.hero}'");
    }
}
