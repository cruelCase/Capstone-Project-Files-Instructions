using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class Question
{
    public string questionText;
    public string choiceA;
    public string choiceB;
    public string choiceC;
    public string choiceD;
    public int correctAnswer;
    public string type;
    public int topic;
}

[Serializable]
public class QuestionWrapper
{
    public List<Question> questions = new List<Question>();
}

public class PreTestManager : MonoBehaviour
{
    [Serializable]
    public class PretestQuestionResult
    {
        public string questionText;
        public int userAnswer;
        public bool isCorrect;
        public string correctAnswerText;
    }

    [Header("UI REFERENCES")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI numberText;

    public TextMeshProUGUI textA;
    public TextMeshProUGUI textB;
    public TextMeshProUGUI textC;
    public TextMeshProUGUI textD;

    [Header("Identifiers (A/B/C/D)")]
    public Transform[] identifierTransforms; // assign 4 transforms that contain the identifier TMPs
    public Button[] answerButtons; // A, B, C, D buttons
    public Button submitButton;    // optional submit button

    [Header("Post Submit UI")]
    public Transform[] heroPrefabs; // assign the 4 hero transforms for the selected user
    public string[] heroNames; // hero name labels matching heroPrefabs order (e.g., "Melchora Aquino", "Gabriela Silang")
    [TextArea(3, 5)]
    public string dialogueText; // the dialogue/paragraph text to display after submit
    public Transform typingTextTransform; // text object where dialogueText will display
    public Transform typingTextBackgroundTransform; // background image for the text

    [Header("Progress Bar")]
    public Slider[] progressBars;  // One progress bar for each hero - assign in same order as heroNames
    public string[] progressBarHeroNames;  // Developer names matching progressBars order (e.g., "Gabriela Silang", "Jose Rizal")

    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.03f, 1f, 0f); // default green

    public GameObject questionsPanel;
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultsText;

    private List<Question> allQuestions = new List<Question>();
    private List<Question> selectedQuestions = new List<Question>();

    private int[] userAnswers = new int[15];
    private int currentIndex = 0;

    private string username;

    void Awake()
    {
        EnsureAnswerButtons();
    }

    void EnsureAnswerButtons()
    {
        if (answerButtons != null && answerButtons.Length > 0)
            return;

        Button buttonA = textA != null ? textA.GetComponentInParent<Button>() : null;
        Button buttonB = textB != null ? textB.GetComponentInParent<Button>() : null;
        Button buttonC = textC != null ? textC.GetComponentInParent<Button>() : null;
        Button buttonD = textD != null ? textD.GetComponentInParent<Button>() : null;

        if (buttonA != null || buttonB != null || buttonC != null || buttonD != null)
        {
            answerButtons = new Button[4] { buttonA, buttonB, buttonC, buttonD };
            return;
        }

        if (questionsPanel != null)
        {
            Button[] foundButtons = questionsPanel.GetComponentsInChildren<Button>(true);
            if (foundButtons.Length >= 4)
            {
                answerButtons = new Button[4] { foundButtons[0], foundButtons[1], foundButtons[2], foundButtons[3] };
                return;
            }
        }
    }

    // ======================================================
    // START
    // ======================================================
    void Start()
    {
        username = PlayerPrefs.GetString("ActiveUser", "");

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("PreTestManager: ActiveUser is not set. Please select a user first.");
            return;
        }

        if (IsPretestAlreadyCompleted())
        {
            ShowLockedScreen();
            return;
        }

        CreateAllQuestions();
        LoadOrGenerateQuestionSet();

        for (int i = 0; i < 15; i++)
            userAnswers[i] = -1;

        // Lock submit at the very start
        if (submitButton != null)
            submitButton.interactable = false;

        if (questionsPanel != null)
            questionsPanel.SetActive(true);

        if (scorePanel != null)
            scorePanel.SetActive(false);

        HidePostSubmitUI();
        DisplayQuestion();
    }

    // ======================================================
    // SUBMIT BUTTON LOCK — checks all 15 answers
    // ======================================================
    void UpdateSubmitButton()
    {
        if (submitButton == null) return;

        for (int i = 0; i < 15; i++)
        {
            if (userAnswers[i] == -1)
            {
                submitButton.interactable = false;
                return;
            }
        }

        submitButton.interactable = true;
    }

    // ======================================================
    // DOUBLE CHECK SYSTEM (Mobile Safe)
    // ======================================================
    bool IsPretestAlreadyCompleted()
    {
        bool playerPrefFlag = PlayerPrefs.GetInt(username + "_PretestCompleted", 0) == 1;

        string path = GetProfilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);

            if (data.pretestCompleted)
            {
                PlayerPrefs.SetInt(username + "_PretestCompleted", 1);
                PlayerPrefs.Save();
                return true;
            }
        }

        if (playerPrefFlag)
            return true;

        return false;
    }

    void ShowLockedScreen()
    {
        HidePostSubmitUI();
        questionsPanel.SetActive(false);
        scorePanel.SetActive(true);

        int savedScore = GetSavedScore();
        scoreText.text = "Pretest Already Completed\nScore: " + savedScore + " / 15";
    }

    void HidePostSubmitUI()
    {
        if (heroPrefabs != null)
        {
            foreach (var hero in heroPrefabs)
                if (hero != null)
                    hero.gameObject.SetActive(false);
        }

        if (typingTextTransform != null)
            typingTextTransform.gameObject.SetActive(false);

        if (typingTextBackgroundTransform != null)
            typingTextBackgroundTransform.gameObject.SetActive(false);
    }

    IEnumerator ShowPostSubmitUIAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        ShowSelectedHero();

        if (typingTextTransform != null)
        {
            TextMeshProUGUI textComponent = typingTextTransform.GetComponent<TextMeshProUGUI>();
            if (textComponent != null && !string.IsNullOrEmpty(dialogueText))
            {
                textComponent.text = dialogueText;
            }
            typingTextTransform.gameObject.SetActive(true);
        }

        if (typingTextBackgroundTransform != null)
            typingTextBackgroundTransform.gameObject.SetActive(true);
    }

    void ShowSelectedHero()
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0)
            return;

        string heroName = LoadSelectedHeroName();
        if (string.IsNullOrEmpty(heroName))
            return;

        string heroValue = heroName.Trim().ToLower();
        bool foundHero = false;

        // Use heroNames array for matching if available
        if (heroNames != null && heroNames.Length == heroPrefabs.Length)
        {
            for (int i = 0; i < heroPrefabs.Length; i++)
            {
                if (heroPrefabs[i] == null)
                    continue;

                string candidateName = heroNames[i].Trim().ToLower();
                bool shouldShow = candidateName == heroValue || candidateName.Contains(heroValue) || heroValue.Contains(candidateName);
                heroPrefabs[i].gameObject.SetActive(shouldShow);

                if (shouldShow)
                    foundHero = true;
            }
        }
        else
        {
            // Fallback to prefab object names if heroNames not set
            foreach (var hero in heroPrefabs)
            {
                if (hero == null)
                    continue;

                string candidateName = hero.name.Trim().ToLower();
                bool shouldShow = candidateName == heroValue || candidateName.Contains(heroValue) || heroValue.Contains(candidateName);
                hero.gameObject.SetActive(shouldShow);

                if (shouldShow)
                    foundHero = true;
                else
                    hero.gameObject.SetActive(false);
            }
        }

        if (!foundHero)
            Debug.LogWarning($"PreTestManager: No hero prefab found matching '{heroName}'. Ensure the hero name labels or prefab names match the selected hero.");
    }

    string LoadSelectedHeroName()
    {
        if (string.IsNullOrEmpty(username))
            username = PlayerPrefs.GetString("ActiveUser", "");

        if (string.IsNullOrEmpty(username))
            return null;

        string path = GetProfilePath();
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        return profile != null ? profile.hero : null;
    }

    int GetSavedScore()
    {
        string path = GetProfilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);
            return data.pretestScore;
        }

        return 0;
    }

    // ======================================================
    // CREATE FULL 45 QUESTIONS
    // ======================================================
    void CreateAllQuestions()
    {
        // ===== TOPIC 1: Pangangailangan at Kagustuhan (Needs and Wants) =====

        // Recap — direct knowledge/definition questions
        allQuestions.Add(new Question
        {
            questionText = "Nakakita si Andrea ng damit na uso sa kasalukuyan. Bagama't marami pa siyang maayos na damit, nais niya itong bilhin. Ang damit na ito ay maituturing na:",
            choiceA = "Pangangailangan",
            choiceB = "Kagustuhan",
            choiceC = "Produksiyon",
            choiceD = "Serbisyo",
            correctAnswer = 1,
            type = "Recap",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang pinakamalapit na halimbawa ng kagustuhan?",
            choiceA = "Tirahan",
            choiceB = "Tubig",
            choiceC = "Mamahaling telepono",
            choiceD = "Gamot",
            correctAnswer = 2,
            type = "Recap",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "May natanggap na pera si Mia bilang gantimpala. Alin sa mga sumusunod ang higit na maituturing na kagustuhan?",
            choiceA = "Uniporme",
            choiceB = "Pagkain",
            choiceC = "Mamahaling relo",
            choiceD = "Mga aklat",
            correctAnswer = 2,
            type = "Recap",
            topic = 1
        });

        // Evaluation — judgment/reasoning questions
        allQuestions.Add(new Question
        {
            questionText = "Bakit mahalagang matukoy kung alin ang pangangailangan at alin ang kagustuhan?",
            choiceA = "Upang mabili ang lahat ng ninanais",
            choiceB = "Upang magamit nang wasto ang magagamit na salapi",
            choiceC = "Upang makasabay sa uso",
            choiceD = "Upang magkaroon ng mas maraming ari-arian",
            correctAnswer = 1,
            type = "Evaluation",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang nagpapakita ng wastong pagpapasya sa paggastos?",
            choiceA = "Pagbili ng mamahaling gamit kahit kulang ang pera para sa proyekto",
            choiceB = "Paglalaan ng pera para sa mahahalagang gastusin bago bumili ng luho",
            choiceC = "Paggastos ng buong baon sa isang araw",
            choiceD = "Pagbili ng mga bagay dahil lamang sa impluwensiya ng kaibigan",
            correctAnswer = 1,
            type = "Evaluation",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang nagpapakita ng wastong pagtukoy sa pangangailangan?",
            choiceA = "Pagbili ng gamit dahil uso ito",
            choiceB = "Paglalaan ng pera para sa mga pangunahing gastusin",
            choiceC = "Pagsunod sa mga kagustuhan ng kaibigan",
            choiceD = "Pagbili ng maraming palamuti",
            correctAnswer = 1,
            type = "Evaluation",
            topic = 1
        });

        // Applied — scenario-based application questions
        allQuestions.Add(new Question
        {
            questionText = "Nakatanggap si Joshua ng karagdagang baon. Napansin niyang malapit nang masira ang kaniyang sapatos na ginagamit sa pagpasok, ngunit nais din niyang bumili ng bagong headset. Ano ang higit na nararapat niyang unahin?",
            choiceA = "Bagong headset",
            choiceB = "Sapatos na ginagamit sa paaralan",
            choiceC = "Pagkain sa labas",
            choiceD = "Palamuti sa silid",
            correctAnswer = 1,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "May limitadong halaga ng pera si Carla. Alin ang higit niyang dapat paglaanan?",
            choiceA = "Kagamitan sa pag-aaral",
            choiceB = "Palamuti sa telepono",
            choiceC = "Laruang matagal na niyang gusto",
            choiceD = "Bagong hikaw",
            correctAnswer = 0,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Nais bumili ni Paulo ng bagong sapatos kahit maayos pa ang kaniyang ginagamit. Samantala, kailangan niyang maglaan ng pera para sa proyekto sa paaralan. Ano ang mas nararapat niyang unahin?",
            choiceA = "Bagong sapatos",
            choiceB = "Proyekto sa paaralan",
            choiceC = "Pagkain sa labas",
            choiceD = "Laruan",
            correctAnswer = 1,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Kung limitado ang pera, alin ang pinakamainam na desisyon?",
            choiceA = "Bilhin muna ang nais na bagay",
            choiceB = "Ipagpaliban ang mahahalagang gastusin",
            choiceC = "Unahin ang mga pangunahing pangangailangan",
            choiceD = "Gastusin agad ang buong halaga",
            correctAnswer = 2,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Kung may labis na pera pagkatapos ng lahat ng pangunahing gastusin, ano ang pinakamatalinong gawin?",
            choiceA = "Gastusin agad sa pagkain sa labas",
            choiceB = "Itabi para sa hinaharap na pangangailangan",
            choiceC = "Bilhin lahat ng ninanais",
            choiceD = "Ibigay sa kaibigan",
            correctAnswer = 1,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang nagpapakita ng tamang priyoridad sa paggastos?",
            choiceA = "Pagbili ng gadget bago ang gamot",
            choiceB = "Pagbili ng pagkain bago ng alahas",
            choiceC = "Pagbili ng damit bago ng aklat",
            choiceD = "Pagbili ng laruan bago ng uniporme",
            correctAnswer = 1,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Si Juan ay may ₱500. Kailangan niya ng papel at lapis para sa klase, pero nais din niya ang isang bagong laro. Ano ang dapat niyang gawin?",
            choiceA = "Bilhin ang laruan",
            choiceB = "Bilhin ang papel at lapis",
            choiceC = "Huwag na lang bumili ng kahit ano",
            choiceD = "Humingi ng dagdag na pera",
            correctAnswer = 1,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Kapag may tinatanggap na limitadong baon, ano ang tamang unang hakbang?",
            choiceA = "Bilhin ang pinakagusto mo",
            choiceB = "Alamin ang mga pangunahing pangangailangan bago gumastos",
            choiceC = "Gastusin agad bago pa maubos",
            choiceD = "Ibahagi sa mga kaibigan",
            correctAnswer = 1,
            type = "Applied",
            topic = 1
        });

        allQuestions.Add(new Question
        {
            questionText = "Ang pagbili ng bagay na hindi mo kailangan sa kasalukuyan ay nagpapakita ng:",
            choiceA = "Matalinong pag-iisip",
            choiceB = "Wastong paggastos",
            choiceC = "Pagbibigay-priyoridad sa kagustuhan kaysa pangangailangan",
            choiceD = "Maingat na pagpaplano",
            correctAnswer = 2,
            type = "Applied",
            topic = 1
        });

        // ===== TOPIC 2: Ugnayan ng Kita, Pag-iimpok, at Pagkonsumo =====

        // Recap — direct knowledge/definition questions
        allQuestions.Add(new Question
        {
            questionText = "Tuwing bakasyon ay tumutulong si Leo sa maliit na tindahan ng kaniyang tiyahin at binibigyan siya ng kabayaran. Ang perang kaniyang natatanggap ay tinatawag na:",
            choiceA = "Gastusin",
            choiceB = "Kita",
            choiceC = "Ipon",
            choiceD = "Bayarin",
            correctAnswer = 1,
            type = "Recap",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Tuwing tumatanggap ng baon si Maria, agad siyang nagtatabi ng bahagi nito para sa mga susunod na pangangailangan. Ano ang ipinakikita ng kaniyang ginagawa?",
            choiceA = "Pagkonsumo",
            choiceB = "Pag-iimpok",
            choiceC = "Pag-utang",
            choiceD = "Pagnenegosyo",
            correctAnswer = 1,
            type = "Recap",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Nakatanggap si Ben ng ₱500. Kaagad niyang itinabi ang ₱100 at ginamit ang natitira para sa kaniyang mga gastusin. Ano ang ipinakikita ng kaniyang ginawa?",
            choiceA = "Pag-utang",
            choiceB = "Pag-iimpok",
            choiceC = "Pag-aaksaya",
            choiceD = "Pagnenegosyo",
            correctAnswer = 1,
            type = "Recap",
            topic = 2
        });

        // Evaluation — judgment/reasoning questions
        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang maaaring maging dahilan kung bakit mahalaga ang pag-iimpok?",
            choiceA = "Upang magkaroon ng pondo sa oras ng pangangailangan",
            choiceB = "Upang maubos ang salapi",
            choiceC = "Upang makabili agad ng lahat ng nais",
            choiceD = "Upang maiwasan ang pagpaplano",
            correctAnswer = 0,
            type = "Evaluation",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Kung mas malaki ang kinikita ng isang tao ngunit mas mabilis din ang kaniyang paggastos, ano ang maaaring mangyari?",
            choiceA = "Madaragdagan agad ang kaniyang ipon",
            choiceB = "Magkakaroon siya ng sapat na naitatabing salapi",
            choiceC = "Maaaring wala pa rin siyang maiipon",
            choiceD = "Hindi na niya kailangang magplano ng badyet",
            correctAnswer = 2,
            type = "Evaluation",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Kapag tumataas ang kita ng isang tao ngunit tumataas din ang kaniyang paggastos, ano ang maaaring mangyari?",
            choiceA = "Dadami ang kaniyang ipon",
            choiceB = "Maaaring manatiling kaunti ang kaniyang naitatabi",
            choiceC = "Mawawala ang kaniyang kita",
            choiceD = "Hindi na niya kailangang magbadyet",
            correctAnswer = 1,
            type = "Evaluation",
            topic = 2
        });

        // Applied — scenario-based application questions
        allQuestions.Add(new Question
        {
            questionText = "Nakatanggap si Mark ng ₱2,000. Gumastos siya ng ₱1,200 at itinabi ang natitira. Magkano ang kaniyang naipon?",
            choiceA = "₱600",
            choiceB = "₱700",
            choiceC = "₱800",
            choiceD = "₱900",
            correctAnswer = 2,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang nagpapakita ng maayos na pamamahala ng salapi?",
            choiceA = "Inuubos agad ang natanggap na pera",
            choiceB = "Nagtatabi ng bahagi ng kita bago gumastos",
            choiceC = "Nangungutang para sa mga hindi mahalagang bagay",
            choiceD = "Bumibili agad ng anumang nais makita",
            correctAnswer = 1,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Ano ang pinakamainam na gawin kapag nakatanggap ng karagdagang pera?",
            choiceA = "Gastusin agad ang lahat",
            choiceB = "Maglaan ng bahagi para sa ipon",
            choiceC = "Bumili ng anumang nais",
            choiceD = "Ipahiram agad sa iba",
            correctAnswer = 1,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang nagpapakita ng tamang pamamahala ng salapi?",
            choiceA = "Pagpaplano ng paggastos at pag-iimpok",
            choiceB = "Paggastos nang walang badyet",
            choiceC = "Pagbili ng luho bago ang pangangailangan",
            choiceD = "Pagwawalang-bahala sa ipon",
            correctAnswer = 0,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Si Ana ay may ₱1,000 na baon para sa buong linggo. Alin ang pinakamatalinong gagawin niya?",
            choiceA = "Gastusin ang lahat sa unang araw",
            choiceB = "Gumawa ng badyet para sa bawat araw",
            choiceC = "Huwag nang kumain upang makatipid",
            choiceD = "Mangutang kapag naubos",
            correctAnswer = 1,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Kung nais mong makapag-ipon ng ₱200 bawat linggo mula sa iyong ₱500 na baon, magkano lang ang maaari mong gastusin?",
            choiceA = "₱400",
            choiceB = "₱300",
            choiceC = "₱200",
            choiceD = "₱100",
            correctAnswer = 1,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Ang regular na pag-iimpok kahit maliit na halaga ay nagdudulot ng:",
            choiceA = "Mas maraming utang",
            choiceB = "Pinansyal na seguridad sa hinaharap",
            choiceC = "Mas mabilis na paggastos",
            choiceD = "Pagkawala ng pera",
            correctAnswer = 1,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Bakit dapat maglaan ng ipon bago gumastos sa ibang bagay?",
            choiceA = "Upang maging mayaman agad",
            choiceB = "Upang may pondo para sa mahahalagang pangangailangan sa hinaharap",
            choiceC = "Upang mawalan ng gastusin",
            choiceD = "Upang makabili ng mas maraming luho",
            correctAnswer = 1,
            type = "Applied",
            topic = 2
        });

        allQuestions.Add(new Question
        {
            questionText = "Kung palagi kang gumagastos nang higit sa iyong kita, ano ang maaaring mangyari?",
            choiceA = "Lalago ang iyong ipon",
            choiceB = "Magiging mas masagana ka",
            choiceC = "Maaaring mahulog ka sa utang",
            choiceD = "Wala itong epekto",
            correctAnswer = 2,
            type = "Applied",
            topic = 2
        });

        // ===== TOPIC 3: Pagkonsumo (Smart Consumption) =====

        // Recap — direct knowledge/definition questions
        allQuestions.Add(new Question
        {
            questionText = "Bumili si Ana ng mga kagamitang kakailanganin niya para sa isang gawaing pampaaralan at ginamit niya ang mga ito. Ang kaniyang ginawa ay isang halimbawa ng:",
            choiceA = "Pag-iimpok",
            choiceB = "Pagkonsumo",
            choiceC = "Produksiyon",
            choiceD = "Pamumuhunan",
            correctAnswer = 1,
            type = "Recap",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang katangian ng isang matalinong mamimili?",
            choiceA = "Bumibili agad kapag may diskuwento",
            choiceB = "Pinaghahambing ang presyo at kalidad bago bumili",
            choiceC = "Inuuna ang mga bagay na uso",
            choiceD = "Ginagastos agad ang lahat ng salapi",
            correctAnswer = 1,
            type = "Recap",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang katangian ng isang responsableng mamimili?",
            choiceA = "Bumibili nang hindi naghahambing ng presyo",
            choiceB = "Sinusuri muna ang kalidad ng produkto",
            choiceC = "Sumusunod lamang sa uso",
            choiceD = "Inuubos agad ang pera",
            correctAnswer = 1,
            type = "Recap",
            topic = 3
        });

        // Evaluation — judgment/reasoning questions
        allQuestions.Add(new Question
        {
            questionText = "Bakit mahalagang pag-isipang mabuti ang isang bibilhing produkto bago ito bilhin?",
            choiceA = "Upang masunod ang uso",
            choiceB = "Upang maiwasan ang hindi kinakailangang paggastos",
            choiceC = "Upang maubos agad ang salapi",
            choiceD = "Upang makabili ng mas maraming bagay",
            correctAnswer = 1,
            type = "Evaluation",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Ano ang maaaring maging bunga ng hindi planadong paggastos?",
            choiceA = "Pagdami ng ipon",
            choiceB = "Pagiging handa sa mga pangangailangan sa hinaharap",
            choiceC = "Kakulangan ng salapi para sa mahahalagang gastusin",
            choiceD = "Mas maayos na badyet",
            correctAnswer = 2,
            type = "Evaluation",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Ano ang maaaring mangyari kung palaging bumibili ng mga bagay na hindi naman kailangan?",
            choiceA = "Mas magiging maayos ang badyet",
            choiceB = "Dadami ang ipon",
            choiceC = "Mababawasan ang perang maaaring ilaan sa mahahalagang pangangailangan",
            choiceD = "Lalago ang kita",
            correctAnswer = 2,
            type = "Evaluation",
            topic = 3
        });

        // Applied — scenario-based application questions
        allQuestions.Add(new Question
        {
            questionText = "Nais bumili ni Ryan ng bag. Bago siya bumili, sinuri muna niya ang kalidad, presyo, at pakinabang nito. Ano ang ipinakikita ng kaniyang kilos?",
            choiceA = "Pagiging maingat na mamimili",
            choiceB = "Pagiging magastos",
            choiceC = "Pagiging maluho",
            choiceD = "Pagiging mapagpaliban",
            correctAnswer = 0,
            type = "Applied",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Bago bumili ng isang produkto, alin ang dapat munang isaalang-alang?",
            choiceA = "Kung uso ito",
            choiceB = "Kung kailangan ito at pasok sa badyet",
            choiceC = "Kung binili rin ito ng mga kaibigan",
            choiceD = "Kung maganda ang kulay nito",
            correctAnswer = 1,
            type = "Applied",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Nais bumili ni Carla ng bagong bag. Bago siya bumili, ikinumpara muna niya ang iba't ibang uri at presyo nito. Ano ang ipinakikita nito?",
            choiceA = "Pagiging maingat sa paggastos",
            choiceB = "Pagiging maluho",
            choiceC = "Pagiging magastos",
            choiceD = "Pagiging padalus-dalos",
            correctAnswer = 0,
            type = "Applied",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang nagpapakita ng matalinong pagkonsumo?",
            choiceA = "Pagbili ng produkto dahil lamang sa patalastas",
            choiceB = "Pagbili ng produktong angkop sa pangangailangan at badyet",
            choiceC = "Pagbili ng pinakamahal na produkto",
            choiceD = "Pagbili ng maraming produkto nang sabay-sabay",
            correctAnswer = 1,
            type = "Applied",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Nakita ni Marco ang isang produkto sa sale na 50% off. Hindi naman niya ito kailangan ngayon. Ano ang dapat niyang gawin?",
            choiceA = "Bilhin agad dahil mura na",
            choiceB = "Pag-isipan muna kung kailangan nga niya ito bago bumili",
            choiceC = "Hiramin ang pera ng kaibigan para mabili",
            choiceD = "Bilhin ang marami para maibenta",
            correctAnswer = 1,
            type = "Applied",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Alin sa mga sumusunod ang pagkilos ng isang matalinong mamimili bago bumili ng mahal na produkto?",
            choiceA = "Bumili agad bago pa maubusan",
            choiceB = "Hanapin ang pinakamababang presyo at suriin ang kalidad",
            choiceC = "Sundan ang rekomendasyon ng influencer",
            choiceD = "Bilhin kung gusto ng kaibigan",
            correctAnswer = 1,
            type = "Applied",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Kapag binibili ang isang produkto na nasa badyet mo at kailangan mo talaga ito, ito ay nagpapakita ng:",
            choiceA = "Pag-aaksaya",
            choiceB = "Matalinong pagkonsumo",
            choiceC = "Pagkakamali sa paggastos",
            choiceD = "Maluho at padalus-dalos na pagbili",
            correctAnswer = 1,
            type = "Applied",
            topic = 3
        });

        allQuestions.Add(new Question
        {
            questionText = "Si Luis ay nag-shopping online at nakita ang isang bagay na hindi naman nasa listahan niya. Ano ang dapat niyang gawin bilang matalinong mamimili?",
            choiceA = "Bilhin agad para hindi mawala ang alok",
            choiceB = "Tanggalin sa cart at unahin ang mga nasa listahan ng pangangailangan",
            choiceC = "Hiramin ang pera para mabili",
            choiceD = "Bilhin para may regalo sa kaibigan",
            correctAnswer = 1,
            type = "Applied",
            topic = 3
        });
    }

    // ======================================================
    // RANDOM GENERATION (ONLY ONCE)
    // ======================================================
    void LoadOrGenerateQuestionSet()
    {
        if (PlayerPrefs.HasKey(username + "_PretestSet"))
        {
            string json = PlayerPrefs.GetString(username + "_PretestSet");
            selectedQuestions = JsonUtility.FromJson<QuestionWrapper>(json).questions;
        }
        else
        {
            GenerateQuestionSet();
            SaveQuestionSet();
        }
    }

    string GetProfilePath()
    {
        return Path.Combine(Application.persistentDataPath, username + "_profile.json");
    }

    void GenerateQuestionSet()
    {
        selectedQuestions.Clear();

        for (int topic = 1; topic <= 3; topic++)
        {
            AddRandom(topic, "Recap", 1);
            AddRandom(topic, "Evaluation", 1);
            AddRandom(topic, "Applied", 3);
        }
    }

    void AddRandom(int topic, string type, int count)
    {
        List<Question> pool = allQuestions.FindAll(q => q.topic == topic && q.type == type);

        for (int i = 0; i < pool.Count; i++)
        {
            Question temp = pool[i];
            int randomIndex = UnityEngine.Random.Range(i, pool.Count);
            pool[i] = pool[randomIndex];
            pool[randomIndex] = temp;
        }

        for (int i = 0; i < count; i++)
            selectedQuestions.Add(pool[i]);
    }

    void SaveQuestionSet()
    {
        QuestionWrapper wrapper = new QuestionWrapper();
        wrapper.questions = selectedQuestions;

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(username + "_PretestSet", json);
        PlayerPrefs.Save();
    }

    // ======================================================
    // UI
    // ======================================================
    void DisplayQuestion()
    {
        Question q = selectedQuestions[currentIndex];

        numberText.text = "Tanong " + (currentIndex + 1) + " / 15";
        questionText.text = q.questionText;

        textA.text = q.choiceA;
        textB.text = q.choiceB;
        textC.text = q.choiceC;
        textD.text = q.choiceD;

        if (answerButtons == null || answerButtons.Length == 0)
        {
            EnsureAnswerButtons();
            if (answerButtons == null || answerButtons.Length == 0)
            {
                Debug.LogWarning("PreTestManager: answerButtons array is not assigned or empty, and automatic fallback did not find the answer buttons.");
                return;
            }
        }

        // reset identifier colors (do not change button colors anymore)
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (identifierTransforms != null && i < identifierTransforms.Length && identifierTransforms[i] != null)
            {
                TMP_Text idText = identifierTransforms[i].GetComponentInChildren<TMP_Text>();
                if (idText != null)
                    idText.color = normalColor;
            }
        }

        int saved = userAnswers[currentIndex];
        if (saved >= 0 && saved < answerButtons.Length)
        {
            if (identifierTransforms != null && saved < identifierTransforms.Length && identifierTransforms[saved] != null)
            {
                TMP_Text idText = identifierTransforms[saved].GetComponentInChildren<TMP_Text>();
                if (idText != null)
                    idText.color = selectedColor;
            }
        }

        // Update progress bar based on current question
        UpdateProgressBar();
    }

    void UpdateProgressBar()
    {
        // Get the active user's hero
        string userHero = LoadSelectedHeroName();
        if (string.IsNullOrEmpty(userHero))
            return;

        // Find the matching progress bar
        if (progressBars == null || progressBarHeroNames == null || progressBars.Length == 0)
            return;

        int heroIndex = -1;
        for (int i = 0; i < progressBarHeroNames.Length; i++)
        {
            if (progressBarHeroNames[i].Trim().ToLower() == userHero.Trim().ToLower())
            {
                heroIndex = i;
                break;
            }
        }

        if (heroIndex < 0 || heroIndex >= progressBars.Length)
        {
            Debug.LogWarning($"PreTestManager: No progress bar found for hero '{userHero}'");
            return;
        }

        progressBars[heroIndex].gameObject.SetActive(true);
        // Update the slider value: current question / total questions
        // currentIndex is 0-14, so question is currentIndex + 1, and total is 15
        float progress = (currentIndex + 1) / 15f;
        progressBars[heroIndex].value = progress;
    }

    public void SelectAnswer(int index)
    {
        userAnswers[currentIndex] = index;

        if (answerButtons == null || answerButtons.Length == 0)
        {
            NextQuestion();
            return;
        }

        // reset identifier colors (do not change button colors anymore)
        if (identifierTransforms != null)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < identifierTransforms.Length && identifierTransforms[i] != null)
                {
                    TMP_Text idText = identifierTransforms[i].GetComponentInChildren<TMP_Text>();
                    if (idText != null)
                        idText.color = normalColor;
                }
            }
        }

        if (index >= 0 && index < answerButtons.Length)
        {
            if (identifierTransforms != null && index < identifierTransforms.Length && identifierTransforms[index] != null)
            {
                TMP_Text idText = identifierTransforms[index].GetComponentInChildren<TMP_Text>();
                if (idText != null)
                    idText.color = selectedColor;
            }
        }

        UpdateSubmitButton();
    }

    public void NextQuestion()
    {
        // Prevent advancing if current question is not answered yet
        if (userAnswers == null) return;
        if (currentIndex < 14)
        {
            if (userAnswers[currentIndex] == -1)
                return;

            currentIndex++;
            DisplayQuestion();
        }
    }

    public void PrevQuestion()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            DisplayQuestion();
        }
    }


    // ======================================================
    // SUBMIT (MOBILE SAFE SAVE)
    // ======================================================
    public void SubmitTest()
    {
        int score = 0;
        List<PretestQuestionResult> results = new List<PretestQuestionResult>();

        for (int i = 0; i < 15; i++)
        {
            bool isCorrect = userAnswers[i] == selectedQuestions[i].correctAnswer;
            if (isCorrect)
                score++;

            string correctAnswerText = "";
            switch (selectedQuestions[i].correctAnswer)
            {
                case 0: correctAnswerText = selectedQuestions[i].choiceA; break;
                case 1: correctAnswerText = selectedQuestions[i].choiceB; break;
                case 2: correctAnswerText = selectedQuestions[i].choiceC; break;
                case 3: correctAnswerText = selectedQuestions[i].choiceD; break;
            }

            results.Add(new PretestQuestionResult
            {
                questionText = selectedQuestions[i].questionText,
                userAnswer = userAnswers[i],
                isCorrect = isCorrect,
                correctAnswerText = correctAnswerText
            });
        }

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("PreTestManager: ActiveUser is not set. Cannot save pretest results.");
            return;
        }

        string path = GetProfilePath();
        Debug.Log($"PreTestManager: Saving pretest results for user '{username}' to '{path}'");

        ProfilePlayerData profile;
        if (File.Exists(path))
        {
            profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        }
        else
        {
            profile = new ProfilePlayerData { username = username };
            Debug.LogWarning($"PreTestManager: Profile file not found, creating new one for {username}: {path}");
        }

        profile.pretestCompleted = true;
        profile.pretestScore = score;
        profile.pretestResults = results;

        File.WriteAllText(path, JsonUtility.ToJson(profile, true));

        PlayerPrefs.SetInt(username + "_PretestCompleted", 1);
        PlayerPrefs.Save();

        questionsPanel.SetActive(false);
        scorePanel.SetActive(true);
        scoreText.text = "Ang iyong puntos ay: " + score + " / 15";

        // Hide all progress bars after submission
        if (progressBars != null)
        {
            foreach (var progressBar in progressBars)
                if (progressBar != null)
                    progressBar.gameObject.SetActive(false);
        }

        StartCoroutine(ShowPostSubmitUIAfterDelay());

        // Detailed per-question results are still saved to the profile, but not displayed in the UI.
    }

    void SaveResult(int score)
    {
        string path = GetProfilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);

            data.pretestScore = score;
            data.pretestCompleted = true;
            data.pretestResults = new List<PretestQuestionResult>();

            string updatedJson = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, updatedJson);
        }

        PlayerPrefs.SetInt(username + "_PretestCompleted", 1);
        PlayerPrefs.Save();
    }
}