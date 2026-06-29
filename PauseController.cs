using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool isGamePaused { get; private set; }

    [Header("Pause Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public GameObject pauseMenu;
    public bool freezeTime = true;

    void Start()
    {
        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isGamePaused = true;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }

        if (freezeTime)
        {
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        isGamePaused = false;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        if (freezeTime)
        {
            Time.timeScale = 1f;
        }
    }
}
