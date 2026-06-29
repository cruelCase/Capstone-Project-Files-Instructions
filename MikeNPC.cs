using UnityEngine;
using TMPro;

public class MikeNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel; // Panel with Yes / No buttons

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    [Header("Dialogue Lines")]
    private string[] dialogues = {
       "Hoy, bata! Mukhang marami ka nang napuntahan sa siyudad… parang marami ka na ring nagastos!",
        "Mapanlinlang ang siyudad. Kapag hindi ka naging maingat, mabilis mauubos ang pera mo.",
        "May hamon ako para sa’yo. Maaari kitang ipakita ang ilang paraan kung paano maayos na pamahalaan ang pera at makapag-ipon nang tama. Gusto mo bang subukan?"
    };

    private int dialogueIndex = 0;
    private bool showQuestNext = false;
    private bool questCompleted = false;

    private bool quizAlreadyTaken => GameManager.Instance.MiniQuiz1bCompleted;

    // Called when NPC is clicked
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
            dialogueText.text = "Natuto ka na kung paano pamahalaan ang iyong pera dito. Ipagpatuloy mo ang pag-practice!";
            questCompleted = true;
        }
        else
        {
            NextDialogue();
        }
    }

    // Called by OK button
    public void NextDialogue()
    {
        if (dialogueIndex < dialogues.Length)
        {
            dialogueText.text = dialogues[dialogueIndex];
            dialogueIndex++;

            if (dialogueIndex == dialogues.Length && !questCompleted && !quizAlreadyTaken)
            {
                showQuestNext = true;
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

    // Player accepts quest
    public void StartMiniQuiz()
    {
        if (quizAlreadyTaken) return; // safety check
        if (questChoicePanel == null || !questChoicePanel.activeSelf) return; // only start from the visible quest prompt

        questChoicePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        questCompleted = true;
        ShowObjects();

        // Show the Scene 2 minigame start screen; the game starts later when the player presses Start.
        if (MiniGameIncomeSortManager.Instance != null)
        {
            MiniGameIncomeSortManager.Instance.ShowStartScreen();
        }
    }

    // Player declines quest
    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Walang problema! Pwede kang maglibot muna sa siyudad at subukan mo ulit mamaya.";

        dialogueIndex = dialogues.Length;
        showQuestNext = false;
    }

    // --- Utility Methods ---
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
