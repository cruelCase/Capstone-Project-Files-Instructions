// SceneAStateManager.cs
public static class SceneAStateManager
{
    public static bool[] buttonsVisible;      // true = visible
    public static bool[] buttonsInteractable; // true = interactable
    public static bool[] objectsActive;       // for other GameObjects

    public static void Initialize(int buttonCount, int objectCount)
    {
        if (buttonsVisible == null || buttonsVisible.Length != buttonCount)
        {
            buttonsVisible = new bool[buttonCount];
            buttonsInteractable = new bool[buttonCount];
            for (int i = 0; i < buttonCount; i++)
            {
                buttonsVisible[i] = true;       // default visible
                buttonsInteractable[i] = false; // default not interactable
            }
        }

        if (objectsActive == null || objectsActive.Length != objectCount)
        {
            objectsActive = new bool[objectCount];
            for (int i = 0; i < objectCount; i++)
                objectsActive[i] = true;
        }
    }
}
