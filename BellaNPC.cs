using UnityEngine;
using TMPro;

public class BellaNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel;

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    [Header("Dialogue Lines")]
    private string[] dialogues = {
      "Hello! Ako si Bella, isang financial banker dito sa siyudad.",
      "Marami na akong nakitang mga estudyanteng nahihirapang mag-ipon ng pera nang maayos.",
      "Pero huwag kang mag-alala — matututo kang mag-ipon nang matalino sa kaunting gabay!",
      "Gusto mo bang subukan ang isang mabilis na saving challenge? Bibigyan kita ng mga sitwasyon at ikaw ang magpapasya kung magkano ang iyong iipunin."
    };

    private int dialogueIndex = 0;
    private bool showQuestNext = false;
    private bool questCompleted = false;

    private bool quizAlreadyTaken => GameManager.Instance.MiniQuiz2bCompleted;

    public void StartDialogue()
    {
        dialoguePanel.SetActive(true);
        questChoicePanel.SetActive(false);

        dialogueIndex = 0;
        showQuestNext = false;
        questCompleted = false;  // Reset so quest panel can show again if quiz not completed

        HideObjects();

        if (quizAlreadyTaken)
        {
            dialogueText.text = "Natapos mo na ang saving na hamon! Ipagpatuloy mo ang pag-practice sa natutunan mo.";
            questCompleted = true;
        }
        else
        {
            NextDialogue();
        }
    }

    public void NextDialogue()
    {
        if (dialogueIndex < dialogues.Length)
        {
            dialogueText.text = dialogues[dialogueIndex];
            dialogueIndex++;

            if (dialogueIndex == dialogues.Length && !questCompleted && !quizAlreadyTaken)
                showQuestNext = true;
        }
        else if (showQuestNext)
        {
            questChoicePanel.SetActive(true);
            showQuestNext = false;
        }
        else
        {
            dialoguePanel.SetActive(false);
            ShowObjects();
        }
    }

    public void StartMiniQuiz()
    {
        if (quizAlreadyTaken) return;

        questChoicePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        questCompleted = true;
        ShowObjects();

        if (MiniGameSavingManager.Instance != null)
        {
            MiniGameSavingManager.Instance.ShowStartScreen();
        }
    }

    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Ayos lang! Ang pag-iimpok ay nandito lagi kapag handa ka na.";

        dialogueIndex = dialogues.Length;
        showQuestNext = false;
    }

    private void HideObjects()
    {
        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(false);
    }

    public void ShowObjects()
    {
        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(true);
    }
}
