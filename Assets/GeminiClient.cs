using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using TMPro;
using UnityEngine.UI;


public class GeminiClient : MonoBehaviour
{
    public TMP_Text outputText;
    public PollinationsAI pollinationsAI;
    public int textIndex = 0;
    public GameObject imageObject;

    // Replace with your actual API key
    private string apiKey = "AIzaSyAWmxgOX0F_-ie-bJ_gkhJfSB2jEJs4xwM";
    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    public string[] imagePrompt;

    [Header("Image Generation Settings")]
    [Tooltip("Delay in seconds between each image generation to avoid rate limits")]
    public float delayBetweenImages = 1.5f;
    public bool isAllResourcesGenerated = false; // Check if all text/img are received from gemini and pollination 

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

                    pollinationsAI.GenerateImage(imagePrompt[0]);

                    StartCoroutine(GenerateImagesSequentially());

                    //for (int i = 0; i < imagePrompt.Length - 1; i++)
                    //{
                    //    pollinationsAI.GenerateImage(imagePrompt[i]);
                    //}
                }
            }
        }
    }

    /// <summary>
    /// Generates images one at a time with delays to avoid API rate limits
    /// </summary>
    private IEnumerator GenerateImagesSequentially()
    {
        Debug.Log($"Starting to generate {imagePrompt.Length} images sequentially...");

        for (int i = 0; i < imagePrompt.Length; i++)
        {
            Debug.Log($"Generating image {i + 1}/{imagePrompt.Length}: {imagePrompt[i]}");

            // Start generating this image
            yield return StartCoroutine(pollinationsAI.GenerateImageCoroutine(imagePrompt[i]));

            // Wait before generating the next one (except after the last image)
            if (i < imagePrompt.Length - 1)
            {
                Debug.Log($"Waiting {delayBetweenImages} seconds before next image...");
                yield return new WaitForSeconds(delayBetweenImages);
            }
        }

        Debug.Log("All images generated successfully!");
        isAllResourcesGenerated = true;

        // Display the first image once all are loaded
        if (pollinationsAI.spriteBank.Count > 0)
        {
            imageObject.GetComponent<Image>().sprite = pollinationsAI.spriteBank[0];
        }
    }

    public void Update()    
    {
        if (isAllResourcesGenerated)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (textIndex > 0)
                    textIndex--;

                outputText.GetComponent<TextMeshProUGUI>().text = imagePrompt[textIndex];
                imageObject.GetComponent<Image>().sprite = pollinationsAI.spriteBank[textIndex];

            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (textIndex < imagePrompt.Length - 1)
                    textIndex++;

                outputText.GetComponent<TextMeshProUGUI>().text = imagePrompt[textIndex];
                imageObject.GetComponent<Image>().sprite = pollinationsAI.spriteBank[textIndex];
            }
        }
    }
}
