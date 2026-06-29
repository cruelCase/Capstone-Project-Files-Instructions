using UnityEngine;
using TMPro;

public class TwinUnclesNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel; // Yes / No buttons

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    [Header("Dialogue Lines")]
    private string[] dialogues = {
       "Ay! Kumusta diyan, iho/iha! Hindi namin inakala na may mag-iikot dito nang ganito kaaga.",
        "Inaayos lang namin ang bahay… o sinusubukan, sa totoo lang. Matagal nang gustong-gusto ng apo namin na magkaroon ng toy room! Talaga kaming kailangan ng isa.",
        "Dahil diyan—alam mo ba kung paano matukoy ang pagkakaiba ng kailangan at gusto?",
        "Kung game ka, ipapakita namin sa’yo ang ilang bagay na pinagtatalunan namin. Ikaw ang magsasabi kung ito ba ay KAILANGAN o GUSTO!"
    };

    private int dialogueIndex = 0;
    private bool showQuestNext = false;
    private bool questCompleted = false;

    private bool quizAlreadyTaken => GameManager.Instance.MiniQuiz2Completed;

    // Called when clicking the twin uncles
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

            // On the last line, show YES/NO next if quiz not completed
            if (dialogueIndex == dialogues.Length)
            {
                if (!questCompleted && !quizAlreadyTaken)
                {
                    showQuestNext = true;
                }
                else
                {
                    questChoicePanel.SetActive(false);
                    questCompleted = true; // ensure quest is considered done
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
            dialoguePanel.SetActive(false);
            ShowObjects();
        }
    }

    // YES button = accept quest
    public void StartMiniQuiz()
    {
        if (quizAlreadyTaken) return; // safety check

        questChoicePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        questCompleted = true;
        ShowObjects();

        if (MiniQuizManager1.Instance != null)
        {
            MiniQuizManager1.Instance.ShowStartUI();
            TaskManager.CompleteTask(0);
        }
    }

    // NO button = decline
    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text =
            "Ayos lang! Maaari kang bumalik anumang oras kung gusto mong tulungan kaming lutasin ang usapan.";

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
