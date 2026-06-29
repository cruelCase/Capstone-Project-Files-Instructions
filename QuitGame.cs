using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Game is quitting...");
        Application.Quit();
    }
}