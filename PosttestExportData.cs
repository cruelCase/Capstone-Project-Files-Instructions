using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PosttestExportData
{
    public string username;
    public int pretestScore;
    public List<PostTestManager.PosttestQuestionResult> posttestResults;
}
