using UnityEngine;

public class ScorePanelWatcher : MonoBehaviour
{
    public KevinNPC kevinNPC;              // optional
    public TwinUnclesNPC twinUnclesNPC;    // optional
    public void OnExitScorePanel()
{
    // Refresh profile and update all UI elements
    GameManager.Instance.RefreshProfileData();
}
    private void OnDisable()
    {
        // ScorePanel turned OFF → show objects back

        if (kevinNPC != null)
            kevinNPC.ShowObjects();

        if (twinUnclesNPC != null)
            twinUnclesNPC.ShowObjects();
    }
}
