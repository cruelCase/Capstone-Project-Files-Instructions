using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public Button targetButton;
    public enum SceneButton { SceneA, SceneD, SceneF } // Added SceneF
    public SceneButton buttonType;

    private void Start()
    {
        switch (buttonType)
        {
            case SceneButton.SceneA:
                targetButton.interactable = ButtonStateManager.sceneAButtonInteractable;
                break;
            case SceneButton.SceneD:
                targetButton.interactable = ButtonStateManager.sceneDButtonInteractable;
                break;
            case SceneButton.SceneF:
                targetButton.interactable = ButtonStateManager.sceneFButtonInteractable;
                break;
        }
    }
}
