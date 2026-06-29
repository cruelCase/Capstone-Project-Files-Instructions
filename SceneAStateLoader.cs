using UnityEngine;
using UnityEngine.UI;

public class SceneAStateLoader : MonoBehaviour
{
    [Header("Buttons to control")]
    public Button[] buttons;

    [Header("GameObjects to control")]
    public GameObject[] objects;

    private bool appliedOnce = false;

    void Awake()
    {
        SceneAStateManager.Initialize(buttons.Length, objects.Length);
    }

    void Start()
    {
        ApplyState();
    }

    public void ToggleObject(int index)
    {
        if (index < 0 || index >= objects.Length) return;

        bool newState = !SceneAStateManager.objectsActive[index]; // switch state
        SceneAStateManager.objectsActive[index] = newState;

        if (objects[index] != null)
            objects[index].SetActive(newState);
    }

    public void ApplyState()
    {
        if (appliedOnce) return;

        // Apply buttons
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(SceneAStateManager.buttonsVisible[i]);
            buttons[i].interactable = SceneAStateManager.buttonsInteractable[i];
        }

        // Apply GameObjects
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(SceneAStateManager.objectsActive[i]);
        }

        appliedOnce = true;
    }

    // Call these whenever you update a button in runtime
    public void SetButtonState(int index, bool visible, bool interactable, bool applyImmediately = true)
    {
        if (index < 0 || index >= buttons.Length) return;

        SceneAStateManager.buttonsVisible[index] = visible;
        SceneAStateManager.buttonsInteractable[index] = interactable;

        if (applyImmediately)
        {
            buttons[index].gameObject.SetActive(visible);
            buttons[index].interactable = interactable;
        }
    }

    public void SetObjectState(int index, bool active, bool applyImmediately = true)
    {
        if (index < 0 || index >= objects.Length) return;

        SceneAStateManager.objectsActive[index] = active;

        if (applyImmediately && objects[index] != null)
            objects[index].SetActive(active);
    }
}
