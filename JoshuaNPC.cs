using UnityEngine;
using TMPro;

public class JoshuaNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel;

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    private string[] dialogues = {
       "Uy, bata! Mukhang magha-hiking adventure ka ah!",
       "Ang saya mag-hiking sa mga daan dito. Ang ganda ng mga tanawin!",
       "Pero tandaan mo, puwedeng gumastos nang malaki kapag hindi ka nagplano.",
       "May munting quiz ako para sa’yo — tungkol sa mga gastos sa hiking. Gusto mo bang subukan?"
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

        MiniGameWise2Manager.Instance.ShowStartScreen();
    }

    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Walang problema! Nandito ang hiking challenge kapag handa ka na.";

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
