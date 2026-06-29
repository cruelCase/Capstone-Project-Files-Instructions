using UnityEngine;
using UnityEngine.UI;

public class Temporaryblock : MonoBehaviour
{
    [Tooltip("Optional ID to identify this block. If empty, GameObject.name will be used.")]
    public string blockId = "";

    [Tooltip("If true, will toggle Button.interactable instead of SetActive on the GameObject.")]
    public bool useInteractable = false;

    private Button btn;

    private string GetPlayerKey()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "Guest");
        string id = string.IsNullOrEmpty(blockId) ? gameObject.name : blockId;
        return activeUser + "_Blocked_" + id;
    }

    void Awake()
    {
        btn = GetComponent<Button>();
    }

    void Start()
    {
        string key = GetPlayerKey();
        bool blocked = PlayerPrefs.GetInt(key, 0) == 1;
        ApplyBlockedState(blocked);
    }

    // Call this from the button's OnClick to permanently block it for the active user.
    public void MarkBlockedForActiveUser()
    {
        string key = GetPlayerKey();
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        ApplyBlockedState(true);
    }

    // Optional helper to clear the block for current user (useful for debugging).
    public void ClearBlockForActiveUser()
    {
        string key = GetPlayerKey();
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        ApplyBlockedState(false);
    }

    private void ApplyBlockedState(bool blocked)
    {
        if (useInteractable && btn != null)
        {
            btn.interactable = !blocked;
        }
        else
        {
            gameObject.SetActive(!blocked);
        }
    }
}
