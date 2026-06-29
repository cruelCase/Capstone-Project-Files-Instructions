using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadNewScene1()
    {
        SceneManager.LoadScene("NewScene1"); // Make sure Scene2 is in Build Settings
        
    }
    public void LoadNewScene2()
    {
        SceneManager.LoadScene("NewScene2"); // Make sure Scene2 is in Build Settings
    }
    public void LoadNewScene3()
    {
        SceneManager.LoadScene("NewScene3"); // Make sure Scene3 is in Build Settings
    }
    public void LoadNewScene4()
    {
        SceneManager.LoadScene("Scene4"); // Make sure Scene2 is in Build Settings
    }
    public void LoadSceneH()
    {
        SceneManager.LoadScene("House"); // Make sure Scene2 is in Build Settings
    }
    public void LoadSceneI()
    {
        SceneManager.LoadScene("Intro"); // Make sure Scene2 is in Build Settings
    }
}
