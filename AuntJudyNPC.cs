using UnityEngine;
using TMPro;

public class AuntJudyNPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject questChoicePanel; // Yes / No buttons

    [Header("Objects to Hide During Dialogue")]
    public GameObject[] objectsToHide;

    [Header("Dialogue Lines")]
    private string[] dialogues = {
       "Ay, kumusta! Hindi ako nag-asang may bisita ngayon.",
       "Ang taniman ng manggang ito ay nagpapaalala sa akin… alam mo ba ang pagkakaiba ng KAILANGAN at GUSTO?",
       "May maliit akong hamon para sa’yo. Handa ka na bang subukan?"
    };

    private int dialogueIndex = 0;
    private bool showQuestNext = false;

    // Checks if the quiz is already completed in GameManager / Profile
    private bool quizAlreadyTaken => GameManager.Instance.MiniQuiz3Completed;

    // Called when clicking the NPC
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

            // Last line, show quest choice if not completed
            if (dialogueIndex == dialogues.Length)
            {
                if (!quizAlreadyTaken)
                {
                    showQuestNext = true;
                }
                else
                {
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
            dialoguePanel.SetActive(false);
            ShowObjects();
        }
    }

    // YES button = accept quest
    public void StartMiniQuiz()
    {   
        if (quizAlreadyTaken) return;

        questChoicePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        ShowObjects();

        if (MiniQuizManager2.Instance != null)
        {
            MiniQuizManager2.Instance.ShowStartUI();
            TaskManager.CompleteTask(0);
        }
    }

    // NO button = decline quest
    public void DeclineQuest()
    {
        questChoicePanel.SetActive(false);
        dialogueText.text = "Walang problema! Babalik ka lang kapag handa ka na.";

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
