using UnityEngine;
using UnityEngine.U2D.Animation;
using System.IO;

public class SpriteLibraryPlayerApplier : MonoBehaviour
{
    public SpriteLibrary spriteLibrary;

    public SpriteLibraryAsset joseRizal;
    public SpriteLibraryAsset juanLuna;
    public SpriteLibraryAsset gabrielaSilang;
    public SpriteLibraryAsset melchoraAquino;

    [Header("Hero Customizer Reference (Optional)")]
    public HeroCustomizer heroCustomizer;  // Assign the HeroCustomizer script here

    private string GetProfilePath()
    {
        string user = PlayerPrefs.GetString("ActiveUser", "");
        return Path.Combine(Application.persistentDataPath, user + "_profile.json");
    }

    private void Awake()
    {
        ApplyFromProfile();
    }

    void ApplyFromProfile()
    {
        string path = GetProfilePath();
        if (!File.Exists(path)) return;

        ProfilePlayerData profile =
            JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));

        // First, try to apply costume if heroCostume is not default
        if (!string.IsNullOrEmpty(profile.heroCostume) && profile.heroCostume != "DEFAULT")
        {
            if (TryApplyCostume(profile.hero, profile.heroCostume))
                return;  // Costume applied successfully
        }

        // If no custom costume, apply base hero library
        switch (profile.hero)
        {
            case "Jose Rizal":
                spriteLibrary.spriteLibraryAsset = joseRizal;
                break;
            case "Juan Luna":
                spriteLibrary.spriteLibraryAsset = juanLuna;
                break;
            case "Gabriela Silang":
                spriteLibrary.spriteLibraryAsset = gabrielaSilang;
                break;
            case "Melchora Aquino":
                spriteLibrary.spriteLibraryAsset = melchoraAquino;
                break;
        }
    }

    bool TryApplyCostume(string heroName, string costumeName)
    {
        // Try to get costume from HeroCustomizer if assigned
        if (heroCustomizer != null)
        {
            SpriteLibraryAsset costumeLibrary = HeroCustomizer.GetActiveUserHeroCostumeSpriteLibrary(
                heroCustomizer, heroName, costumeName);

            if (costumeLibrary != null)
            {
                spriteLibrary.spriteLibraryAsset = costumeLibrary;
                Debug.Log($"Applied costume library: {costumeName}");
                return true;
            }
        }

        return false;
    }
}
