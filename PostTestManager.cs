using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class PostQuestion
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
public class PostQuestionWrapper
{
    public List<PostQuestion> questions = new List<PostQuestion>();
}

public class PostTestManager : MonoBehaviour
{
    [Serializable]
    public class PosttestQuestionResult
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

    public Button[] answerButtons; // A, B, C, D buttons
    public Button submitButton;    // optional submit button

    [Header("Progress Bar")]
    public Slider[] progressBars;  // One progress bar for each hero - assign in same order as heroNames
    public string[] progressBarHeroNames;  // Developer names matching progressBars order (e.g., "Gabriela Silang", "Jose Rizal")

    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.03f, 1f, 0f); // default green

    public GameObject questionsPanel;
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultsText;

    private List<PostQuestion> allQuestions = new List<PostQuestion>();
    private List<PostQuestion> selectedQuestions = new List<PostQuestion>();

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
            if (foundButtons.Length == 4)
            {
                answerButtons = foundButtons;
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
            Debug.LogError("PostTestManager: ActiveUser is not set. Please select a user before starting the posttest.");
            return;
        }

        if (IsPosttestAlreadyCompleted())
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
    bool IsPosttestAlreadyCompleted()
    {
        bool playerPrefFlag = PlayerPrefs.GetInt(username + "_PosttestCompleted", 0) == 1;

        string path = GetProfilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);

            if (data.posttestCompleted)
            {
                PlayerPrefs.SetInt(username + "_PosttestCompleted", 1);
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
        questionsPanel.SetActive(false);
        scorePanel.SetActive(true);

        int savedScore = GetSavedScore();
        scoreText.text = "Posttest Already Completed\nScore: " + savedScore + " / 15";
    }

    int GetSavedScore()
    {
        string path = GetProfilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ProfilePlayerData data = JsonUtility.FromJson<ProfilePlayerData>(json);
            return data.posttestScore;
        }

        return 0;
    }

    // ======================================================
    // CREATE FULL 15 VERIFIED QUESTIONS
    // ======================================================
    void CreateAllQuestions()
    {
        // ===== TOPIC 1: Pangangailangan at Kagustuhan (Needs and Wants) =====

        // Recap — direct knowledge/definition questions
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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

        allQuestions.Add(new PostQuestion
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
        bool needsGeneration = true;

        // First try to load the EXACT same questions from PreTest
        if (PlayerPrefs.HasKey(username + "_PretestSet"))
        {
            try
            {
                string pretestJson = PlayerPrefs.GetString(username + "_PretestSet");
                // Parse the JSON string directly to extract questions
                selectedQuestions = ConvertPreTestQuestionsToPostTest(pretestJson);
                
                if (selectedQuestions != null && selectedQuestions.Count == 15)
                {
                    SaveQuestionSet();
                    needsGeneration = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"PostTestManager: Could not load PreTest questions for '{username}': {e.Message}");
            }
        }

        // If no PreTest set exists, try to load saved PostTest set
        if (needsGeneration && PlayerPrefs.HasKey(username + "_PosttestSet"))
        {
            string json = PlayerPrefs.GetString(username + "_PosttestSet");
            PostQuestionWrapper wrapper = JsonUtility.FromJson<PostQuestionWrapper>(json);

            if (wrapper != null && wrapper.questions != null && wrapper.questions.Count == 15)
            {
                selectedQuestions = wrapper.questions;
                needsGeneration = false;
            }
            else
            {
                Debug.LogWarning($"PostTestManager: Saved posttest set for '{username}' was invalid or incomplete and will be regenerated.");
            }
        }

        if (needsGeneration)
        {
            Debug.LogWarning($"PostTestManager: No PreTest set found for '{username}'. Generating new questions instead.");
            GenerateQuestionSet();
            SaveQuestionSet();
        }
    }

    List<PostQuestion> ConvertPreTestQuestionsToPostTest(string pretestJson)
    {
        List<PostQuestion> converted = new List<PostQuestion>();
        
        // Simple string-based JSON parsing to extract questions
        // Look for the questions array in the JSON
        int startIndex = pretestJson.IndexOf("\"questions\":");
        if (startIndex == -1) return null;
        
        startIndex = pretestJson.IndexOf("[", startIndex);
        int endIndex = pretestJson.LastIndexOf("]");
        if (startIndex == -1 || endIndex == -1) return null;
        
        string questionsArray = pretestJson.Substring(startIndex + 1, endIndex - startIndex - 1);
        
        // Split by question objects (simple approach)
        // This is a workaround - ideally we'd have a proper QuestionWrapper deserialization
        // For now, let's use a more direct approach
        
        try
        {
            // Create a wrapper with the questions array and deserialize
            string wrappedJson = "{\"questions\":[" + questionsArray + "]}";
            PostQuestionWrapper wrapper = JsonUtility.FromJson<PostQuestionWrapper>(wrappedJson);
            
            if (wrapper != null && wrapper.questions != null)
                return wrapper.questions;
        }
        catch { }
        
        return null;
    }

    void GenerateQuestionSet()
    {
        selectedQuestions.Clear();

        // Select the SAME questions as PreTest: 1 Recap + 1 Evaluation + 3 Applied per topic
        for (int topic = 1; topic <= 3; topic++)
        {
            AddRandom(topic, "Recap", 1);
            AddRandom(topic, "Evaluation", 1);
            AddRandom(topic, "Applied", 3);
        }

        // Now shuffle the final 15 questions to create a different sequence
        for (int i = 0; i < selectedQuestions.Count; i++)
        {
            PostQuestion temp = selectedQuestions[i];
            int randomIndex = UnityEngine.Random.Range(i, selectedQuestions.Count);
            selectedQuestions[i] = selectedQuestions[randomIndex];
            selectedQuestions[randomIndex] = temp;
        }
    }

    void AddRandom(int topic, string type, int count)
    {
        List<PostQuestion> pool = allQuestions.FindAll(q => q.topic == topic && q.type == type);

        for (int i = 0; i < pool.Count; i++)
        {
            PostQuestion temp = pool[i];
            int randomIndex = UnityEngine.Random.Range(i, pool.Count);
            pool[i] = pool[randomIndex];
            pool[randomIndex] = temp;
        }

        for (int i = 0; i < count; i++)
            selectedQuestions.Add(pool[i]);
    }

    void SaveQuestionSet()
    {
        PostQuestionWrapper wrapper = new PostQuestionWrapper();
        wrapper.questions = selectedQuestions;

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(username + "_PosttestSet", json);
        PlayerPrefs.Save();
    }

    string GetProfilePath()
    {
        return Path.Combine(Application.persistentDataPath, username + "_profile.json");
    }

    string LoadSelectedHeroName()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
            return "";

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
            return "";

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        return profile.hero;
    }

    // ======================================================
    // UI
    // ======================================================
    void DisplayQuestion()
    {
        PostQuestion q = selectedQuestions[currentIndex];

        numberText.text = "Question " + (currentIndex + 1) + " / 15";
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

        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button btn = answerButtons[i];
            if (btn == null)
                continue;

            Graphic target = btn.targetGraphic;
            if (target != null)
                target.color = normalColor;

            if (btn.image != null)
                btn.image.color = normalColor;

            ColorBlock cb = btn.colors;
            cb.normalColor = normalColor;
            cb.highlightedColor = normalColor;
            cb.pressedColor = normalColor;
            cb.selectedColor = normalColor;
            cb.disabledColor = normalColor;
            btn.colors = cb;
        }

        int saved = userAnswers[currentIndex];
        if (saved >= 0 && saved < answerButtons.Length)
        {
            Button btn = answerButtons[saved];
            if (btn != null)
            {
                Graphic target = btn.targetGraphic;
                if (target != null)
                    target.color = selectedColor;

                if (btn.image != null)
                    btn.image.color = selectedColor;

                ColorBlock cb = btn.colors;
                cb.normalColor = selectedColor; 
                cb.highlightedColor = selectedColor;
                cb.pressedColor = selectedColor;
                cb.selectedColor = selectedColor;
                cb.disabledColor = selectedColor;
                btn.colors = cb;
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
            Debug.LogWarning($"PostTestManager: No progress bar found for hero '{userHero}'");
            return;
        }

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
            return;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button btn = answerButtons[i];
            if (btn == null)
                continue;

            Graphic target = btn.targetGraphic;
            if (target != null)
                target.color = normalColor;

            if (btn.image != null)
                btn.image.color = normalColor;

            ColorBlock cb = btn.colors;
            cb.normalColor = normalColor;
            cb.highlightedColor = normalColor;
            cb.pressedColor = normalColor;
            cb.selectedColor = normalColor;
            cb.disabledColor = normalColor;
            btn.colors = cb;
        }

        if (index >= 0 && index < answerButtons.Length)
        {
            Button btn = answerButtons[index];
            if (btn != null)
            {
                Graphic target = btn.targetGraphic;
                if (target != null)
                    target.color = selectedColor;

                if (btn.image != null)
                    btn.image.color = selectedColor;

                ColorBlock cb = btn.colors;
                cb.normalColor = selectedColor;
                cb.highlightedColor = selectedColor;
                cb.pressedColor = selectedColor;
                cb.selectedColor = selectedColor;
                cb.disabledColor = selectedColor;
                btn.colors = cb;
            }
        }

        UpdateSubmitButton();
    }

    public void NextQuestion()
    {
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
        List<PosttestQuestionResult> results = new List<PosttestQuestionResult>();

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

            results.Add(new PosttestQuestionResult
            {
                questionText = selectedQuestions[i].questionText,
                userAnswer = userAnswers[i],
                isCorrect = isCorrect,
                correctAnswerText = correctAnswerText
            });
        }

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("PostTestManager: ActiveUser is not set. Cannot save posttest results.");
            return;
        }

        string path = GetProfilePath();
        ProfilePlayerData profile;

        if (File.Exists(path))
        {
            profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        }
        else
        {
            profile = new ProfilePlayerData { username = username };
            Debug.LogWarning($"PostTestManager: Profile file not found, creating new one for {username}: {path}");
        }

        profile.posttestCompleted = true;
        profile.posttestScore = score;
        profile.posttestResults = results;

        File.WriteAllText(path, JsonUtility.ToJson(profile, true));

        PlayerPrefs.SetInt(username + "_PosttestCompleted", 1);
        PlayerPrefs.Save();

        questionsPanel.SetActive(false);
        scorePanel.SetActive(true);
        scoreText.text = "Ang iyong pontus ay: " + score + " / 15";

        // Display detailed results
        if (resultsText != null)
        {
            string resultDisplay = "";
            for (int i = 0; i < results.Count; i++)
            {
                // Get user's answer text
                string userAnswerText = "";
                switch (results[i].userAnswer)
                {
                    case 0: userAnswerText = selectedQuestions[i].choiceA; break;
                    case 1: userAnswerText = selectedQuestions[i].choiceB; break;
                    case 2: userAnswerText = selectedQuestions[i].choiceC; break;
                    case 3: userAnswerText = selectedQuestions[i].choiceD; break;
                }

                // Get answer letter
                char answerLetter = (char)('A' + results[i].userAnswer);
                string status = results[i].isCorrect ? "Tama" : "Mali";

                resultDisplay += $"{i + 1}. {results[i].questionText}\n";
                resultDisplay += $"Answer: {answerLetter}. {userAnswerText} {status}\n";

                if (!results[i].isCorrect)
                {
                    // Get correct answer letter
                    char correctLetter = (char)('A' + selectedQuestions[i].correctAnswer);
                    resultDisplay += $"Correct Answer: {correctLetter}. {results[i].correctAnswerText}\n";
                }

                resultDisplay += "\n";
            }
            resultsText.text = resultDisplay;
        }

        // Hide all progress bars after submission
        if (progressBars != null)
        {
            foreach (var progressBar in progressBars)
                if (progressBar != null)
                    progressBar.gameObject.SetActive(false);
        }
    }

    void SaveResult(int score)
    {
        string path = GetProfilePath();

        ProfilePlayerData data;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<ProfilePlayerData>(json);
        }
        else
        {
            data = new ProfilePlayerData { username = username };
        }

        data.posttestScore = score;
        data.posttestCompleted = true;

        string updatedJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, updatedJson);

        PlayerPrefs.SetInt(username + "_PosttestCompleted", 1);
        PlayerPrefs.Save();
    }
}