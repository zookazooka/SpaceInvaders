using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using TMPro;

[System.Serializable]
public class HighScoreEntry
{
    public string player_name;
    public int score;
    public string timestamp;
}

public class HighScoresTable : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Transform scoreContainer;
    [SerializeField] private GameObject scoreEntryPrefab;
    
    private int currentScore;
    private string serverUrl = "http://18.175.247.141:3000/score"; 

    void Start()
    {
        submitButton.onClick.AddListener(SubmitScore);
        FetchHighScores();
        currentScore = PlayerPrefs.GetInt("playerScore", 0);

    }

    void FetchHighScores()
    {
        StartCoroutine(GetHighScores());
    }

    IEnumerator GetHighScores()
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{serverUrl}/highScores"))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                HighScoreEntry[] highScores = JsonHelper.FromJson<HighScoreEntry>(jsonResponse);
                PopulateHighScoreTable(highScores);
            }
        }
    }

    void PopulateHighScoreTable(HighScoreEntry[] highScores)
    {        
        foreach (Transform child in scoreContainer)
        {
{
            if (child.gameObject != scoreEntryPrefab)
            {
                Destroy(child.gameObject);
            }
    }   } 

        for (int i = 0; i < highScores.Length; i++)
        {
            GameObject entryObject = Instantiate(scoreEntryPrefab, scoreContainer);
            entryObject.SetActive(true);
            TextMeshProUGUI[] texts = entryObject.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = $"#{i+1}"; 
            texts[1].text = highScores[i].player_name;
            texts[2].text = highScores[i].score.ToString();
            texts[3].text = highScores[i].timestamp.Substring(0, 10);
            foreach (var text in texts)
            {
                text.enabled = true;
                text.fontSize = 26;
            }
        }
    }

    void SubmitScore()
    {
        if (string.IsNullOrEmpty(usernameInput.text))
        {
            Debug.Log("Please enter a username");
            return;
        }

        StartCoroutine(PostScore(usernameInput.text, currentScore));
    }

    IEnumerator PostScore(string playerName, int score)
    {
        HighScoreEntry newScore = new HighScoreEntry
        {
            player_name = playerName,
            score = score
        };

        string json = JsonUtility.ToJson(newScore);
        
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"{serverUrl}/addScore", json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                Debug.Log("Score submitted successfully");
                FetchHighScores();
            }
        }
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}