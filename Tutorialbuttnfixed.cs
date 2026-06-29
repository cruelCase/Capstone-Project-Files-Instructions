using UnityEngine;

public class Tutorialbuttnfixed : MonoBehaviour
{
    [SerializeField] private GameObject buttonA;
    [SerializeField] private GameObject buttonC;
    [SerializeField] private GameObject buttonE;
    private bool isRestoreEnabled = false;

    void Start()
    {
        // Load the saved restore state for the current user
        string currentUser = PlayerPrefs.GetString("ActiveUser", "DefaultUser");
        string restoreKey = "RestoreEnabled_" + currentUser;
        isRestoreEnabled = PlayerPrefs.HasKey(restoreKey) && PlayerPrefs.GetInt(restoreKey) == 1;

        // Only restore button states if restoration is enabled
        if (isRestoreEnabled)
        {
            RestoreSavedButtonStates();
        }
    }

    /// <summary>
    /// Activates a button permanently and saves the state so it persists across scene loads.
    /// State is account-specific, so each user has their own button activation.
    /// Attach this function to buttonB's onClick event and assign buttonA as the parameter.
    /// </summary>
    public void ActivateButtonPermanently(GameObject buttonToActivate)
    {
        if (buttonToActivate == null)
        {
            Debug.LogError("Button to activate is null!");
            return;
        }

        
        // Activate the button
        buttonToActivate.SetActive(true);

        // Save the state with the current user's username so it's account-specific
        string currentUser = PlayerPrefs.GetString("ActiveUser", "DefaultUser");
        string key = "ButtonState_" + currentUser + "_" + buttonToActivate.name;
        PlayerPrefs.SetInt(key, 1); // 1 = active
        PlayerPrefs.Save();

        Debug.Log("Button '" + buttonToActivate.name + "' activated and saved for user: " + currentUser);
    }

    /// <summary>
    /// Activates ButtonC permanently when ButtonD is pressed.
    /// Attach this to buttonD's onClick event.
    /// </summary>
    public void ActivateButtonCPermanently()
    {
        ActivateButtonPermanently(buttonC);
    }

    /// <summary>
    /// Activates ButtonE permanently when ButtonF is pressed.
    /// Attach this to buttonF's onClick event.
    /// </summary>
    public void ActivateButtonEPermanently()
    {
        ActivateButtonPermanently(buttonE);
    }

    /// <summary>
    /// Enables button state restoration for the current user.
    /// Attach this to a button's onClick event to enable restoration.
    /// Once called, RestoreSavedButtonStates() will run in Start() on every scene load.
    /// </summary>
    public void EnableButtonRestoration()
    {
        isRestoreEnabled = true;
        
        // Save this state so it persists for the current user across scene loads
        string currentUser = PlayerPrefs.GetString("ActiveUser", "DefaultUser");
        string restoreKey = "RestoreEnabled_" + currentUser;
        PlayerPrefs.SetInt(restoreKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Button restoration enabled for user: " + currentUser);
    }

    /// <summary>
    /// Restores all previously saved button states.
    /// Called automatically in Start().
    /// </summary>
    private void RestoreSavedButtonStates()
    {
        // Restore all three buttons' states for the current user
        if (buttonA != null)
            RestoreButtonState(buttonA);
        if (buttonC != null)
            RestoreButtonState(buttonC);
        if (buttonE != null)
            RestoreButtonState(buttonE);
    }

    /// <summary>
    /// Restores a specific button's saved state for the current user.
    /// </summary>
    public void RestoreButtonState(GameObject buttonToRestore)
    {
        if (buttonToRestore == null)
            return;

        string currentUser = PlayerPrefs.GetString("ActiveUser", "DefaultUser");
        string key = "ButtonState_" + currentUser + "_" + buttonToRestore.name;
        if (PlayerPrefs.HasKey(key))
        {
            int savedState = PlayerPrefs.GetInt(key);
            buttonToRestore.SetActive(savedState == 1);
            Debug.Log("Button '" + buttonToRestore.name + "' state restored for user '" + currentUser + "': " + (savedState == 1 ? "Active" : "Inactive"));
        }
    }
}
