using UnityEngine;
using TMPro;

public class KevinNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel; // Panel with Yes / No buttons

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    [Header("Dialogue Lines")]
    private string[] dialogues = {
       "Uy! Hindi ko inakala na makikita kita sa labas nang ganito kaaga. Nag-iikot ka ba sa paligid?",
        "Alam mo ba ang pagkakaiba ng mga bagay na kailangan at mga bagay na gusto?",
        "May maliit akong hamon para sa’yo. Ipapakita ko sa’yo ang ilang mga bagay, at kailangan mong mabilis na magpasya kung ito ba ay KAILANGAN o GUSTO. Sa tingin mo, kaya mo?"
    };

    private int dialogueIndex = 0;
    private bool showQuestNext = false;

    private bool quizAlreadyTaken => GameManager.Instance.MiniQuiz1Completed;

    // Called when Kevin is clicked
    public void StartDialogue()
    {
        dialoguePanel.SetActive(true);
        questChoicePanel.SetActive(false);

        dialogueIndex = 0;
        showQuestNext = false;

        HideObjects();
        NextDialogue();
    }

    // Called by OK button
    public void NextDialogue()
    {
        if (dialogueIndex < dialogues.Length)
        {
            dialogueText.text = dialogues[dialogueIndex];
            dialogueIndex++;

            // Last dialogue line
            if (dialogueIndex == dialogues.Length)
            {
                if (!quizAlreadyTaken)
                {
                    // Offer the quest only if not completed
                    showQuestNext = true;
                }
                else
                {
                    // If quiz was already taken, skip showing quest panel
                    questChoicePanel.SetActive(false);
                }
            }
        }
        else if (showQuestNext)
        {
            questChoicePanel.SetActive(true);
            showQuestNext = false;
        }
        else
        {
            // End of dialogue
            dialoguePanel.SetActive(false);
            ShowObjects();
        }
    }

    // Player accepts quest
    public void StartMiniQuiz()
    {
        if (quizAlreadyTaken) return; // Safety check

        questChoicePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        ShowObjects();

        if (MiniQuizManager.Instance != null)
        {
            MiniQuizManager.Instance.ShowStartUI();
            TaskManager.CompleteTask(0);
        }
    }

    // Player declines quest
    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Walang problema! Maglibot muna ka muna at subukan mo ulit mamaya.";

        dialogueIndex = dialogues.Length;
        showQuestNext = false;
    }

    // --- Utility Methods ---
    private void HideObjects()
    {
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    public void ShowObjects()
    {
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}
