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

    [Header("Timeline")]
    public StoryTimeline storyTimeline; // Reference to timeline

    // Replace with your actual API key
    private string apiKey = "AIzaSyAWmxgOX0F_-ie-bJ_gkhJfSB2jEJs4xwM";
    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    public string[] imagePrompt;

    [Header("Image Generation Settings")]
    [Tooltip("Delay in seconds between each image generation to avoid rate limits")]
    public float delayBetweenImages = 1.5f;

    [Serializable]
    public class GeminiRequest
    {
        public Content[] contents;
    }

    [Serializable]
    public class Content
    {
        public Part[] parts;
        public string role;
    }

    [Serializable]
    public class Part
    {
        public string text;
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
        LoadingScreen.Show();

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
                LoadingScreen.SetServerMessage("Gemini API Error: " + www.error + " – " + www.downloadHandler.text);
                LoadingScreen.Hide();
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Gemini response: " + responseText);
                LoadingScreen.SetServerMessage("Gemini response: " + responseText);


                // Parse JSON
                GeminiResponse resp = JsonUtility.FromJson<GeminiResponse>(responseText);
                if (resp != null && resp.candidates.Length > 0)
                {
                    string result = resp.candidates[0].content.parts[0].text;
                    Debug.Log("Result text: " + result);

                    string fileName = "GeneratedStory_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
                    string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

                    try
                    {
                        System.IO.File.WriteAllText(filePath, result);
                        Debug.Log($"Story saved to: {filePath}");
                        LoadingScreen.SetServerMessage($"Story saved!");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to save story: {e.Message}");
                    }

                    // Split result
                    imagePrompt = result.Split(';');
                    imagePrompt[imagePrompt.Length - 1] = "End of story"; // Need this to avoid empty last entry

                    // Display first paragraph
                    textIndex = 0;
                    outputText.GetComponent<TextMeshProUGUI>().text = imagePrompt[0];

                    // Initialize timeline with paragraph count
                    if (storyTimeline != null)
                    {
                        storyTimeline.InitializeTimeline(imagePrompt.Length);
                    }

                    // Generate images sequentially to avoid rate limiting
                    StartCoroutine(GenerateImagesSequentially());
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
        LoadingScreen.SetServerMessage($"Starting to generate {imagePrompt.Length} images sequentially...");

        for (int i = 0; i < imagePrompt.Length; i++)
        {
            Debug.Log($"Generating image {i + 1}/{imagePrompt.Length}: {imagePrompt[i]}");
            LoadingScreen.SetServerMessage($"Generating image {i + 1}/{imagePrompt.Length}");

            // Start generating this image
            yield return StartCoroutine(pollinationsAI.GenerateImageCoroutine(imagePrompt[i]));

            // Wait before generating the next one (except after the last image)
            if (i < imagePrompt.Length - 1)
            {
                Debug.Log($"Waiting {delayBetweenImages} seconds before next image...");
                LoadingScreen.SetServerMessage($"Waiting {delayBetweenImages} seconds before next image...");

                yield return new WaitForSeconds(delayBetweenImages);
            }
        }

        Debug.Log("All images generated successfully!");
        LoadingScreen.SetServerMessage("All images generated successfully!");

        // Display the first image once all are loaded
        if (pollinationsAI.spriteBank.Count > 0)
        {
            imageObject.GetComponent<Image>().sprite = pollinationsAI.spriteBank[0];
        }
        LoadingScreen.Hide();
    }

    /// <summary>
    /// Jump to a specific index (called by timeline or keyboard)
    /// </summary>
    public void JumpToIndex(int index)
    {
        if (imagePrompt == null || index < 0 || index >= imagePrompt.Length)
            return;

        textIndex = index;
        UpdateDisplay();
    }

    /// <summary>
    /// Update the text and image display
    /// </summary>
    private void UpdateDisplay()
    {
        // Update text
        if (outputText != null && imagePrompt != null && textIndex < imagePrompt.Length)
        {
            outputText.GetComponent<TextMeshProUGUI>().text = imagePrompt[textIndex];
        }

        // Update image if available
        if (imageObject != null && pollinationsAI != null && textIndex < pollinationsAI.spriteBank.Count)
        {
            Sprite sprite = pollinationsAI.spriteBank[textIndex];
            if (sprite != null)
            {
                imageObject.GetComponent<Image>().sprite = sprite;
            }
        }

        // Update timeline visual
        if (storyTimeline != null)
        {
            storyTimeline.UpdateTimelineVisual(textIndex);
        }
    }

    public void PrevPage()
    {
        if (textIndex > 0)
        {
            textIndex--;
            UpdateDisplay();
        }
    }
    public void NextPage()
    {
        if (textIndex < imagePrompt.Length - 1)
        {
            textIndex++;
            UpdateDisplay();
        }
    }

    public void ResetAll()
    {
        // Clear generated text data
        imagePrompt = null;
        textIndex = 0;

        // Clear text display
        if (outputText != null)
        {
            outputText.text = "";
        }

        // Clear image display
        if (imageObject != null)
        {
            Image img = imageObject.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = null;
            }
        }

        // Clear generated images
        if (pollinationsAI != null)
        {
            pollinationsAI.ClearSpriteBank();
        }

        // Reset timeline
        if (storyTimeline != null)
        {
            storyTimeline.InitializeTimeline(0);
        }

        Debug.Log("All content has been reset!");
    }

    public void Update()
    {
        if (imagePrompt == null || imagePrompt.Length == 0)
            return;

        // Previous chapter (O key)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PrevPage();
            //if (textIndex > 0)
            //{
            //    textIndex--;
            //    UpdateDisplay();
            //}
        }

        // Next chapter (P key)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPage();
            //if (textIndex < imagePrompt.Length - 1)
            //{
            //    textIndex++;
            //    UpdateDisplay();
            //}
        }
    }
}