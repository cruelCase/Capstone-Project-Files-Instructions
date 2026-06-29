using UnityEngine;
using TMPro;

public class CristinaNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel;

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    private string[] dialogues = {
        "Hoy, bata! Nakikita kong nag-e-explore ka sa mga bundok.",
        "Mag-ingat ka, puwedeng maging mapanlinlang ang mga presyo rito!",
        "Ayokong malinlang ka sa mga sobrang mahal na mga bilihin.",
        "Naghanda ako ng ilang tanong para subukin ang kakayahan mo sa paghawak ng pera. Gusto mo bang subukan?"
    };

    private int dialogueIndex = 0;
    private bool showQuestNext = false;
    private bool questCompleted = false;

    public void StartDialogue()
    {
        dialoguePanel.SetActive(true);
        questChoicePanel.SetActive(false);

        dialogueIndex = 0;
        showQuestNext = false;

        HideObjects();
        NextDialogue();
    }

    public void NextDialogue()
    {
        if (dialogueIndex < dialogues.Length)
        {
            dialogueText.text = dialogues[dialogueIndex];
            dialogueIndex++;
            if (dialogueIndex == dialogues.Length && !questCompleted)
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
        questChoicePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        questCompleted = true;
        ShowObjects();

        MiniGameWiseManager.Instance.ShowStartScreen();
    }

    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Walang problema! Pwede mong sagutin ang mga tanong kapag handa ka na.";

        dialogueIndex = dialogues.Length;
        showQuestNext = false;
    }

    private void HideObjects()
    {
        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(false);
    }

    private void ShowObjects()
    {
        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(true);
    }
}
