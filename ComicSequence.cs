using UnityEngine;
using UnityEngine.UI;

public class ComicSequence2 : MonoBehaviour
{
    public UnityEngine.Sprite[] panels;           // comic images
    public float panelDuration = 2f;

    public UnityEngine.GameObject prevButton;     // assign Prev Button here
    public UnityEngine.GameObject nextButton;     // assign Next Button here
    public UnityEngine.GameObject doneButton;     // assign Done Button here (layered with nextButton)
    public UnityEngine.GameObject player;         // assign your Player empty here

    private UnityEngine.UI.Image img;
    private int index = 0;
    private float timer;
    private bool isComicActive = true;   // flag to control update

    private const string ComicPlayedSuffix = "_ComicPlayed2";

    void Start()
    {
        img = GetComponent<UnityEngine.UI.Image>();
        
        Debug.Log($"ComicSequence2 Start: img is {(img == null ? "NULL" : "FOUND")}");
        Debug.Log($"ComicSequence2 Start: panels.Length = {panels.Length}");

        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        string playedKey = string.IsNullOrEmpty(activeUser) ? "Guest" + ComicPlayedSuffix : activeUser + ComicPlayedSuffix;

        if (PlayerPrefs.GetInt(playedKey, 0) == 1)
        {
            // Skip comic for this user if already watched
            gameObject.SetActive(false);
            if (prevButton != null) prevButton.SetActive(false);
            if (nextButton != null) nextButton.SetActive(false);
            if (doneButton != null) doneButton.SetActive(false);
            if (player != null) player.SetActive(true);
            return;
        }

        PlayerPrefs.SetInt(playedKey, 1);
        PlayerPrefs.Save();

        timer = panelDuration;

        if (panels.Length > 0)
        {
            if (img != null)
            {
                img.sprite = panels[index];
                Debug.Log($"ComicSequence2: Set sprite to {panels[index].name}");
            }
            else
            {
                Debug.LogError("ComicSequence2: img is NULL! Cannot set sprite!");
            }
        }
        else
        {
            Debug.LogError("ComicSequence2: panels array is empty!");
        }

        // Show buttons
        UpdateButtonVisibility();

        // Hide player at start
        if (player != null)
            player.SetActive(false);
    }

    void Update()
    {
        // Comic panels now only change via button clicks, no automatic progression
    }

    public void SkipIntro()
    {
        EndComic();
    }

    public void OnPrevButtonPressed()
    {
        PrevPanel();
    }

    public void OnNextButtonPressed()
    {
        NextPanel();
    }

    public void OnDoneButtonPressed()
    {
        EndComic();
    }

    void UpdateButtonVisibility()
    {
        bool isFirstPanel = (index == 0);
        bool isLastPanel = (index >= panels.Length - 1);

        // Show/hide Prev button (hide on first panel)
        if (prevButton != null)
            prevButton.SetActive(!isFirstPanel);

        // Show/hide Next button (hide on last panel)
        if (nextButton != null)
            nextButton.SetActive(!isLastPanel);

        // Show/hide Done button (show only on last panel)
        if (doneButton != null)
            doneButton.SetActive(isLastPanel);
    }

    void PrevPanel()
    {
        if (index > 0)
        {
            index--;
            img.sprite = panels[index];
            timer = panelDuration;
            UpdateButtonVisibility();
        }
    }

    void NextPanel()
    {
        index++;

        if (index >= panels.Length)
        {
            EndComic();  // End automatically when done
            return;
        }

        img.sprite = panels[index];
        timer = panelDuration;
        UpdateButtonVisibility();
    }

    void EndComic()
    {
        isComicActive = false;

        // Hide comic + buttons
        gameObject.SetActive(false);
        if (prevButton != null)
            prevButton.SetActive(false);
        if (nextButton != null)
            nextButton.SetActive(false);
        if (doneButton != null)
            doneButton.SetActive(false);

        // Show player
        if (player != null)
            player.SetActive(true);

        // ⭐ Trigger HeroLoader AFTER comic finishes (search ALL types)
        TriggerHeroLoader();
    }

    void TriggerHeroLoader()
    {
        // Try HeroLoader (World 1)
        HeroLoader loader = FindObjectOfType<HeroLoader>();
        if (loader != null)
        {
            Debug.Log("ComicSequence: Found HeroLoader, calling LoadHero()");
            loader.LoadHero();
            return;
        }

        // Try HeroLoader1 (World 2)
        HeroLoader1 loader1 = FindObjectOfType<HeroLoader1>();
        if (loader1 != null)
        {
            Debug.Log("ComicSequence: Found HeroLoader1, calling LoadHero()");
            loader1.LoadHero();
            return;
        }

        // Try HeroLoader2 (World 3)
        HeroLoader2 loader2 = FindObjectOfType<HeroLoader2>();
        if (loader2 != null)
        {
            Debug.Log("ComicSequence: Found HeroLoader2, calling LoadHero()");
            loader2.LoadHero();
            return;
        }

        Debug.LogError("ComicSequence: No HeroLoader found in scene!");
    }

}
