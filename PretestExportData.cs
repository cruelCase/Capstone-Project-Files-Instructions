using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PretestExportData
{
    public string username;
    public int pretestScore;
    public List<PreTestManager.PretestQuestionResult> pretestResults;
}

