using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSceneButtonLoader : MonoBehaviour
{
[Header("Button Reference")]
public Button button;


[Header("Scene to Load")]  
public string sceneToLoad;  // e.g., "Scene1", "Scene2", "Scene3"  

void Start()  
{  
    if (button != null && !string.IsNullOrEmpty(sceneToLoad))  
    {  
        button.onClick.RemoveAllListeners();  // Clear old listeners just in case  
        button.onClick.AddListener(() => LoadTargetScene());  
    }  
    else  
    {  
        Debug.LogWarning("Button or SceneToLoad is not set on " + gameObject.name);  
    }  
}  

void LoadTargetScene()  
{  
    SceneManager.LoadScene(sceneToLoad);  
}  


}
