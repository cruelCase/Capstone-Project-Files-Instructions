using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveandQuitManager : MonoBehaviour
{
    public void SaveAndQuitToIntro()
    {
        // Save the current active user data before clearing the active user.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveData();
            GameManager.Instance.ResetProfile();
        }

        // Remove the active user so the intro/user list can select a new one.
        PlayerPrefs.DeleteKey("ActiveUser");
        PlayerPrefs.Save();

        // Send player back to the intro/main menu scene.
        SceneManager.LoadScene("Intro");
    }
}
