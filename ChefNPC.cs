using UnityEngine;
using TMPro;

public class ChefNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel;

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    private string[] dialogues = {
       "Kumusta! Ako si Chef Marco, ang pinakamahusay na kusinero sa Bundok.",
        "Gusto kong mag-share ng masasarap na pagkain, pero sa mga marunong gumawa ng matatalinong desisyon lang!",
        "May ilang tanong ako para sa’yo — sagutin mo ng tama at bibigyan kita ng libreng pagkain.",
        "Handa ka na bang subukan ang hamon ko?"
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

        MiniGameWise3Manager.Instance.ShowStartScreen();
    }

    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Walang problema! Nandito ang kusina kapag handa ka nang tikman ang lasa.";

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
