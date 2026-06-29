using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroPanelManager : MonoBehaviour
{
    [Header("Hero Prefabs")]
    public GameObject[] heroPrefabs;
    public GameObject[] bigHeroPrefabs;
    public string[] heroNames;

    [Header("Display Slots")]
    public Transform bigHeroSlot;

    [Header("Navigation Buttons")]
    public Button leftButton;
    public Button rightButton;
    public Transform confirmButton;

    [Header("Notification")]
    public Transform heroLockedNotification;
    public float notificationDuration = 2f;

    [Header("Lock Presentation")]
    public Transform blackScreen;
    public float lockDisplayDelay = 2f;
    public float blackFadeDuration = 2f;
    public Transform selectedHeroBigSlot;
    public Transform selectedHeroLockDescriptionDisplay;
    public string[] selectedHeroLockGreetings;
    public string defaultSelectedHeroLockGreeting = "Hello, salamat sa pagpili sa akin!";
    public Transform selectedHeroImage;
    public float selectedHeroImageDelay = 1f;
    
    [Header("Lock Dialogue Next / Second Paragraph")]
    public Transform selectedHeroGreetingNextButton; // assign the Next button transform for greetings
    public Transform selectedHeroSecondParagraphDisplay; // assign transform where the second paragraph will be typed
    public string[] selectedHeroSecondParagraphs;
    public string defaultSelectedHeroSecondParagraph = "Salamat! Patuloy nating ipagdiwang ang iyong napiling bayani.";

    [Header("Lock Audio Sources")]
    public AudioSource[] selectedHeroLockGreetingsAudio; // Audio source for each hero's lock greeting
    public AudioSource[] selectedHeroSecondParagraphAudio; // Audio source for each hero's second paragraph

    [Header("Description")]
    public Transform heroDescriptionDisplay;
    public string[] heroDescriptions;
    public float typeDelay = 0.04f;
    public float typingStartDelay = 0.5f;

    private int currentIndex = 0;
    private int lockedHeroIndex = -1;
    private bool heroLocked = false;
    private Coroutine typingCoroutine;
    private Button confirmUIButton;

    private void Start()
    {
        if (leftButton != null)
            leftButton.onClick.AddListener(SelectPreviousHero);
        if (rightButton != null)
            rightButton.onClick.AddListener(SelectNextHero);

        if (confirmButton != null)
        {
            confirmUIButton = confirmButton.GetComponent<Button>();
            if (confirmUIButton != null)
                confirmUIButton.onClick.AddListener(ConfirmHeroSelection);
        }

        if (heroLockedNotification != null)
            heroLockedNotification.gameObject.SetActive(false);
        if (blackScreen != null)
            SetBlackScreenAlpha(0f);
        if (selectedHeroImage != null)
            selectedHeroImage.gameObject.SetActive(false);
        if (selectedHeroLockDescriptionDisplay != null)
            selectedHeroLockDescriptionDisplay.gameObject.SetActive(false);
        if (selectedHeroSecondParagraphDisplay != null)
            selectedHeroSecondParagraphDisplay.gameObject.SetActive(false);
        if (selectedHeroGreetingNextButton != null)
            selectedHeroGreetingNextButton.gameObject.SetActive(false);

        // Hook next button listener if provided
        if (selectedHeroGreetingNextButton != null)
        {
            Button nextBtn = selectedHeroGreetingNextButton.GetComponent<Button>();
            if (nextBtn != null)
            {
                nextBtn.onClick.AddListener(OnSelectedHeroGreetingNextClicked);
            }
        }
        if (selectedHeroBigSlot != null)
            selectedHeroBigSlot.gameObject.SetActive(false);

        LoadSavedHeroSelection();
        ShowHero(currentIndex);
    }

    private void OnDestroy()
    {
        if (leftButton != null)
            leftButton.onClick.RemoveListener(SelectPreviousHero);
        if (rightButton != null)
            rightButton.onClick.RemoveListener(SelectNextHero);

        if (confirmUIButton != null)
            confirmUIButton.onClick.RemoveListener(ConfirmHeroSelection);

        if (heroLockedNotification != null)
            heroLockedNotification.gameObject.SetActive(false);

        if (selectedHeroGreetingNextButton != null)
        {
            Button nextBtn = selectedHeroGreetingNextButton.GetComponent<Button>();
            if (nextBtn != null)
                nextBtn.onClick.RemoveListener(OnSelectedHeroGreetingNextClicked);
        }
    }

    public void SelectPreviousHero()
    {
        if (heroLocked || heroPrefabs == null || heroPrefabs.Length == 0)
            return;

        currentIndex = (currentIndex - 1 + heroPrefabs.Length) % heroPrefabs.Length;
        ShowHero(currentIndex);
    }

    public void SelectNextHero()
    {
        if (heroLocked || heroPrefabs == null || heroPrefabs.Length == 0)
            return;

        currentIndex = (currentIndex + 1) % heroPrefabs.Length;
        ShowHero(currentIndex);
    }

    public void ConfirmHeroSelection()
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0 || heroLocked)
            return;

        heroLocked = true;
        lockedHeroIndex = currentIndex;
        SetNavigationInteractable(false);
        SetConfirmInteractable(false);

        string heroName = GetCurrentHeroName();
        SaveSelectedHeroToProfile(heroName);

        if (GameManager.Instance != null)
            GameManager.Instance.SelectedCharacter = heroName;

        ShowHeroLockedNotification();
        if (selectedHeroImage != null)
            selectedHeroImage.gameObject.SetActive(false);

        StartCoroutine(HeroLockPresentationSequence(heroName));
        StartCoroutine(ShowImageAfterDelay(selectedHeroImageDelay));

        Debug.Log($"Hero locked: {heroName}");
    }

    private void ShowHeroLockedNotification()
    {
        if (heroLockedNotification == null)
            return;

        StopCoroutine(nameof(HeroLockedNotificationRoutine));
        heroLockedNotification.gameObject.SetActive(true);
        StartCoroutine(HeroLockedNotificationRoutine());
    }

    private IEnumerator HeroLockedNotificationRoutine()
    {
        yield return new WaitForSeconds(notificationDuration);
        if (heroLockedNotification != null)
            heroLockedNotification.gameObject.SetActive(false);
    }

    private IEnumerator HeroLockPresentationSequence(string heroName)
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            SetBlackScreenAlpha(0f);
        }

        yield return new WaitForSeconds(lockDisplayDelay);

        if (blackScreen != null)
            yield return StartCoroutine(FadeBlackScreen(0f, 1f, blackFadeDuration));

        ApplySelectedHeroLockDisplay(heroName);
        StartTypingLockedHeroDescription(heroName);
    }

    private IEnumerator ShowImageAfterDelay(float delay)
    {
        if (selectedHeroImage == null)
            yield break;

        yield return new WaitForSeconds(delay);
        selectedHeroImage.gameObject.SetActive(true);
    }

    private Image GetBlackScreenImage()
    {
        if (blackScreen == null)
            return null;

        Image image = blackScreen.GetComponent<Image>();
        if (image != null)
            return image;

        return blackScreen.GetComponentInChildren<Image>();
    }

    private IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        Image image = GetBlackScreenImage();
        if (image == null)
            yield break;

        Color color = image.color;
        color.a = Mathf.Clamp01(fromAlpha);
        image.color = color;

        if (blackScreen != null)
            blackScreen.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            image.color = color;
            yield return null;
        }

        color.a = Mathf.Clamp01(toAlpha);
        image.color = color;
    }

    private void SetBlackScreenAlpha(float alpha)
    {
        Image image = GetBlackScreenImage();
        if (image == null)
            return;

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;

        if (blackScreen != null)
            blackScreen.gameObject.SetActive(alpha > 0f);
    }

    private void ApplySelectedHeroLockDisplay(string heroName)
    {
        if (selectedHeroBigSlot != null)
        {
            ClearSlot(selectedHeroBigSlot);
            GameObject heroPrefab = GetCurrentBigHeroPrefab();
            InstantiateHeroInSlot(heroPrefab, selectedHeroBigSlot);
            if (selectedHeroBigSlot.localScale == Vector3.one)
            {
                selectedHeroBigSlot.localScale = Vector3.one;
            }
        }

        if (selectedHeroBigSlot != null)
            selectedHeroBigSlot.gameObject.SetActive(true);

        if (selectedHeroLockDescriptionDisplay != null)
            selectedHeroLockDescriptionDisplay.gameObject.SetActive(true);
    }

    private void StartTypingLockedHeroDescription(string heroName)
    {
        if (selectedHeroLockDescriptionDisplay == null)
            return;

        string greeting = defaultSelectedHeroLockGreeting;
        if (selectedHeroLockGreetings != null && currentIndex >= 0 && currentIndex < selectedHeroLockGreetings.Length &&
            !string.IsNullOrEmpty(selectedHeroLockGreetings[currentIndex]))
        {
            greeting = selectedHeroLockGreetings[currentIndex];
        }

        // Play the lock greeting audio for the selected hero
        if (selectedHeroLockGreetingsAudio != null && currentIndex >= 0 && currentIndex < selectedHeroLockGreetingsAudio.Length && selectedHeroLockGreetingsAudio[currentIndex] != null)
        {
            selectedHeroLockGreetingsAudio[currentIndex].Play();
        }

        StartCoroutine(TypeDescriptionAndEnableNext(selectedHeroLockDescriptionDisplay, greeting));
    }

    private IEnumerator TypeDescriptionAndEnableNext(Transform target, string text)
    {
        yield return StartCoroutine(TypeDescription(target, text));

        // After first greeting finishes, enable the Next button if a second paragraph exists
        bool hasSecond = selectedHeroSecondParagraphs != null && currentIndex >= 0 && currentIndex < selectedHeroSecondParagraphs.Length && !string.IsNullOrEmpty(selectedHeroSecondParagraphs[currentIndex]);
        if (!hasSecond && selectedHeroSecondParagraphDisplay == null)
            hasSecond = false;

        if (hasSecond && selectedHeroGreetingNextButton != null)
        {
            selectedHeroGreetingNextButton.gameObject.SetActive(true);
        }
    }

    private void OnSelectedHeroGreetingNextClicked()
    {
        if (selectedHeroGreetingNextButton != null)
            selectedHeroGreetingNextButton.gameObject.SetActive(false);

        if (selectedHeroSecondParagraphDisplay == null)
            return;

        string paragraph = defaultSelectedHeroSecondParagraph;
        if (selectedHeroSecondParagraphs != null && currentIndex >= 0 && currentIndex < selectedHeroSecondParagraphs.Length && !string.IsNullOrEmpty(selectedHeroSecondParagraphs[currentIndex]))
            paragraph = selectedHeroSecondParagraphs[currentIndex];

        // Play the second paragraph audio for the selected hero
        if (selectedHeroSecondParagraphAudio != null && currentIndex >= 0 && currentIndex < selectedHeroSecondParagraphAudio.Length && selectedHeroSecondParagraphAudio[currentIndex] != null)
        {
            selectedHeroSecondParagraphAudio[currentIndex].Play();
        }

        selectedHeroSecondParagraphDisplay.gameObject.SetActive(true);
        StartCoroutine(TypeDescription(selectedHeroSecondParagraphDisplay, paragraph));
    }

    private GameObject GetCurrentHeroPrefab()
    {
        if (heroPrefabs == null || currentIndex < 0 || currentIndex >= heroPrefabs.Length)
            return null;

        return heroPrefabs[currentIndex];
    }

    private GameObject GetCurrentBigHeroPrefab()
    {
        if (bigHeroPrefabs == null || currentIndex < 0 || currentIndex >= bigHeroPrefabs.Length)
            return null;

        return bigHeroPrefabs[currentIndex];
    }

    private IEnumerator TypeDescription(Transform target, string text)
    {
        TMP_Text textComponent = GetDescriptionTextComponent(target);
        if (textComponent == null)
            yield break;

        if (typingStartDelay > 0f)
            yield return new WaitForSeconds(typingStartDelay);

        textComponent.text = string.Empty;

        for (int i = 0; i < text.Length; i++)
        {
            textComponent.text += text[i];
            yield return new WaitForSeconds(typeDelay);
        }
    }

    private TMP_Text GetDescriptionTextComponent(Transform target)
    {
        if (target == null)
            return null;

        TMP_Text textComponent = target.GetComponent<TMP_Text>();
        if (textComponent != null)
            return textComponent;

        return target.GetComponentInChildren<TMP_Text>();
    }

    private void SetNavigationInteractable(bool interactable)
    {
        if (leftButton != null)
            leftButton.interactable = interactable;
        if (rightButton != null)
            rightButton.interactable = interactable;
    }

    private void SetConfirmInteractable(bool interactable)
    {
        if (confirmUIButton != null)
            confirmUIButton.interactable = interactable;
    }

    private string GetCurrentHeroName()
    {
        if (heroNames != null && currentIndex >= 0 && currentIndex < heroNames.Length && !string.IsNullOrEmpty(heroNames[currentIndex]))
            return heroNames[currentIndex];

        if (heroPrefabs != null && currentIndex >= 0 && currentIndex < heroPrefabs.Length && heroPrefabs[currentIndex] != null)
            return heroPrefabs[currentIndex].name;

        return string.Empty;
    }

    private int FindHeroIndexByName(string heroName)
    {
        if (string.IsNullOrEmpty(heroName) || heroPrefabs == null)
            return -1;

        if (heroNames != null)
        {
            for (int i = 0; i < heroNames.Length; i++)
            {
                if (heroNames[i] == heroName)
                    return i;
            }
        }

        for (int i = 0; i < heroPrefabs.Length; i++)
        {
            if (heroPrefabs[i] != null && heroPrefabs[i].name == heroName)
                return i;
        }

        return -1;
    }

    private void SaveSelectedHeroToProfile(string heroName)
    {
        if (string.IsNullOrEmpty(heroName))
            return;

        string activeUser = PlayerPrefs.GetString("ActiveUser", string.Empty);
        if (string.IsNullOrEmpty(activeUser))
            return;

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
            return;

        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        if (profile == null)
            profile = new ProfilePlayerData { username = activeUser };

        profile.hero = heroName;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
    }

    private void LoadSavedHeroSelection()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", string.Empty);
        if (string.IsNullOrEmpty(activeUser) || heroPrefabs == null || heroPrefabs.Length == 0)
            return;

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
            return;

        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        if (profile == null || string.IsNullOrEmpty(profile.hero))
            return;

        int savedIndex = FindHeroIndexByName(profile.hero);
        if (savedIndex >= 0)
            currentIndex = savedIndex;
    }

    private void ShowHero(int index)
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0)
            return;

        currentIndex = Mathf.Clamp(index, 0, heroPrefabs.Length - 1);

        ClearSlot(bigHeroSlot);
        InstantiateHeroInSlot(heroPrefabs[currentIndex], bigHeroSlot);

        SetConfirmInteractable(!heroLocked);
        UpdateDescription();
    }

    private void InstantiateHeroInSlot(GameObject heroPrefab, Transform slot)
    {
        if (heroPrefab == null || slot == null)
            return;

        GameObject instance = Instantiate(heroPrefab, slot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    private void ClearSlot(Transform slot)
    {
        if (slot == null)
            return;

        for (int i = slot.childCount - 1; i >= 0; i--)
        {
            Destroy(slot.GetChild(i).gameObject);
        }
    }

    private void UpdateDescription()
    {
        string description = string.Empty;
        if (heroDescriptions != null && currentIndex >= 0 && currentIndex < heroDescriptions.Length)
            description = heroDescriptions[currentIndex];

        DisplayDescriptionImmediately(description);
    }

    private void DisplayDescriptionImmediately(string description)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        TMP_Text textComponent = GetDescriptionTextComponent();
        if (textComponent != null)
            textComponent.text = description;
    }

    private IEnumerator TypeDescription(string text)
    {
        TMP_Text textComponent = GetDescriptionTextComponent();
        if (textComponent == null)
            yield break;

        if (typingStartDelay > 0f)
            yield return new WaitForSeconds(typingStartDelay);

        textComponent.text = string.Empty;

        for (int i = 0; i < text.Length; i++)
        {
            textComponent.text += text[i];
            yield return new WaitForSeconds(typeDelay);
        }

        typingCoroutine = null;
    }

    private TMP_Text GetDescriptionTextComponent()
    {
        if (heroDescriptionDisplay == null)
            return null;

        TMP_Text textComponent = heroDescriptionDisplay.GetComponent<TMP_Text>();
        if (textComponent != null)
            return textComponent;

        return heroDescriptionDisplay.GetComponentInChildren<TMP_Text>();
    }
}
