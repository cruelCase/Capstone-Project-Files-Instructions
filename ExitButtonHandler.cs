using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButtonHandler : MonoBehaviour
{
    public enum TargetButton { SceneA, SceneD, SceneF } // Added SceneF
    public TargetButton targetButton;
    public string sceneToLoad;

    public void OnExitButtonClicked()
    {
        // Set the correct button to interactable
        switch (targetButton)
        {
            case TargetButton.SceneA:
                ButtonStateManager.sceneAButtonInteractable = true;
                break;
            case TargetButton.SceneD:
                ButtonStateManager.sceneDButtonInteractable = true;
                break;
            case TargetButton.SceneF:
                ButtonStateManager.sceneFButtonInteractable = true;
                break;
        }

        // Load the desired scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
