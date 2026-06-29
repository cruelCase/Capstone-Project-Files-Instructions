using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    // Call this for the Main Menu button
    public void GoToMainMenu()
    {
        // Make sure your Main Menu scene is added to Build Settings
        SceneManager.LoadScene("Intro"); // Replace "MainMenu" with your scene name
    }

    // Call this for the Link button
    public void OpenLink(string url)
    {
        // Opens the URL in the mobile device's default browser
        Application.OpenURL(url);
    }
}
