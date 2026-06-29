using UnityEngine;
using TMPro;

public class CarlaNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel;

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    private string[] dialogues = {
       "Hi! Ako si Carla, at tinutulungan ko ang mga estudyante na maunawaan ang pera sa pang-araw-araw na buhay.",
        "Minsan mahirap malaman kung alin ang dapat ipunin at alin ang dapat gastusin nang matalino.",
        "May masaya akong hamon para sa’yo — magbabasa ka ng ilang sitwasyon at pipili ng tamang desisyong pinansyal.",
        "Gusto mo bang subukan ang mini game na ito?"
    };

    private int dialogueIndex = 0;
    private bool showQuestNext = false;
    private bool questCompleted = false;

    // Check if mini-game already done
    private bool quizAlreadyTaken => GameManager.Instance.MiniQuiz3bCompleted;

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
            dialogueText.text = "Natapos mo na ang aking hamon! Ipagpatuloy mo ang paghasa ng iyong kasanayang pinansyal.";
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

        BudgetBalancerManager.Instance.ShowStartScreen();
    }

    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Walang problema! Pwede mong subukan ang hamon kahit kailan.";

        dialogueIndex = dialogues.Length;
        showQuestNext = false;
    }

    private void HideObjects()
    {
        foreach (var obj in objectsToHide)
            if (obj != null) obj.SetActive(false);
    }

    private void ShowObjects()
    {
        foreach (var obj in objectsToHide)
            if (obj != null) obj.SetActive(true);
    }
}
