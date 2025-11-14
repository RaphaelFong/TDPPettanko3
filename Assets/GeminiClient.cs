using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using TMPro;

public class GeminiClient : MonoBehaviour
{
    public TMP_Text outputText;
    public PollinationsAI pollinationsAI;
    int textIndex = 0;

    // Replace with your actual API key
    private string apiKey = "AIzaSyAWmxgOX0F_-ie-bJ_gkhJfSB2jEJs4xwM";
    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    public string[] imagePrompt;

    [Serializable]
    public class GeminiRequest
    {
        public Content[] contents;
        // you can add additional configuration fields
    }
    [Serializable]
    public class Content
    {
        public Part[] parts;
        public string role;  // e.g. "user"
    }
    [Serializable]
    public class Part
    {
        public string text;
        // other union fields if needed
    }

    [Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;
    }
    [Serializable]
    public class Candidate
    {
        public Content content;
    }

    public void SendPrompt(string prompt)
    {
        StartCoroutine(SendRequestCoroutine(prompt));
    }

    private IEnumerator SendRequestCoroutine(string prompt)
    {
        // Build request object
        GeminiRequest req = new GeminiRequest();
        req.contents = new Content[]
        {
            new Content {
                role = "user",
                parts = new Part[] {
                    new Part { text = prompt }
                }
            }
        };

        // Convert to JSON string
        string json = JsonUtility.ToJson(req);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(endpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-goog-api-key", apiKey);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Gemini API Error: " + www.error + " – " + www.downloadHandler.text);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Gemini response: " + responseText);

                // Parse JSON
                GeminiResponse resp = JsonUtility.FromJson<GeminiResponse>(responseText);
                if (resp != null && resp.candidates.Length > 0) 
                {
                    string result = resp.candidates[0].content.parts[0].text;
                    Debug.Log("Result text: " + result);

                    // Split result and send to Pollinations
                    imagePrompt = result.Split(';');

                    //outputText.GetComponent<TextMeshProUGUI>().text = result;
                    outputText.GetComponent<TextMeshProUGUI>().text = imagePrompt[0];

                    // Might need to put http timeout in between requests
                    for (int i = 0; i < 5; i++)
                    {
                        pollinationsAI.GenerateImage(imagePrompt[i]);
                    }
                    
                }
            }
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (textIndex > 0)
                textIndex--;

            outputText.GetComponent<TextMeshProUGUI>().text = imagePrompt[textIndex];
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (textIndex < imagePrompt.Length - 1)
                textIndex++;

            outputText.GetComponent<TextMeshProUGUI>().text = imagePrompt[textIndex];
        }
    }
}
