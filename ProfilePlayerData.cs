using System.Collections.Generic;


[System.Serializable]
public class ProfilePlayerData
{
    public string username = "Player";
    public string uniqueId = "";
    public string password = "";
    public int currency = 250;
    public int xp = 0;
    public int level = 1;
    public int points = 0;
    public string hero = "DEFAULT";
    public string selectedSpriteLibrary = "Original";
    public bool purchasedVillager2 = false;
    public bool purchasedVillager3 = false;
    public bool miniQuiz1Completed = false;
    public bool miniQuiz2Completed = false;
    public bool miniQuiz3Completed = false;
    public bool mainQuizCompleted = false;

    public bool miniQuiz1bCompleted = false;
    public bool miniQuiz2bCompleted = false;
    public bool miniQuiz3bCompleted = false;
    public bool mainQuizBCompleted = false;
    public bool hasDoneFirstWorld1ToWorld2Transition = false;
    public bool hasDoneFirstWorld2ToWorld3Transition = false;
    public bool hasShownAchievementW1 = false;
    public bool hasShownAchievementW2 = false;
    public bool hasShownAchievementW3 = false;
    public bool hasShownAnnouncementW1 = false;
    public bool hasShownAnnouncementW2 = false;
    
    public bool miniQuiz1cCompleted = false;
    public bool miniQuiz2cCompleted = false;
    public bool miniQuiz3cCompleted = false;
    public bool mainQuizCCompleted = false;
    public bool hasShownFinalAnnouncement = false;

    public bool badge1Shown = false;
    public bool badge2Shown = false;
    public bool badge3Shown = false;
    public bool badge4Shown = false;
    public bool badge5Shown = false;
    public bool badge6Shown = false;

    public string selectedBody = "DEFAULT_BODY";
    public string selectedHair = "DEFAULT_HAIR";
    public string selectedOutfit = "DEFAULT_OUTFIT";
    public string heroCostume = "DEFAULT";

    public int pretestScore;
    public bool pretestCompleted;
    public List<PreTestManager.PretestQuestionResult> pretestResults = new List<PreTestManager.PretestQuestionResult>();

    public int posttestScore;
    public bool posttestCompleted;
    public List<PostTestManager.PosttestQuestionResult> posttestResults = new List<PostTestManager.PosttestQuestionResult>();

    public bool world1Active;
    public bool world2Active;
    public bool world3Active;
    public bool world4Active;

    // Costume ownership tracking: "HeroName_CostumeName" format
    public string[] ownedCostumes = new string[0];  // Array of owned costume identifiers

    // New fields for character appearance
    // default body public string selectedBody = "BOK";   // default body
    // default hair public string selectedHair = "BOK";   // default hair
    // default outfit public string selectedOutfit = "BOK"; // default outfit

    // TaskManager (World 0) task completion tracking
    public bool task0_1Completed = false;
    public bool task0_2Completed = false;
    public bool task0_3Completed = false;

    // TaskManager1 (World 1) task completion tracking
    public bool task1_1Completed = false;
    public bool task1_2Completed = false;
    public bool task1_3Completed = false;

    public bool task2_1Completed = false;
    public bool task2_2Completed = false;   
    public bool task2_3Completed = false;

    // Inventory items
    public int item1 = 0;
    public int item2 = 0;


}
