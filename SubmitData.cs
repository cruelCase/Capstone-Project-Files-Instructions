using System.IO;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public class SubmitData : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void Submit()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("SubmitData: No active user found in PlayerPrefs.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("SubmitData: Profile file not found at " + path);
            return;
        }

        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        if (profile == null)
        {
            Debug.LogWarning("SubmitData: Failed to load profile data.");
            return;
        }

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "username", profile.username },
            { "uniqueId", profile.uniqueId },
            { "pretestScore", profile.pretestScore },
            { "posttestScore", profile.posttestScore }
        };

        db.Collection("students").AddAsync(data).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("SubmitData: Data sent successfully.");
            }
            else
            {
                Debug.LogError("SubmitData: Failed to send data: " + task.Exception);
            }
        });
    }
}