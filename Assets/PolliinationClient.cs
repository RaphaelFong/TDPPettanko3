using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PollinationsAI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] outputImage;  // UI Image instead of Renderer
    public int counter = 0;
    public List<Sprite> spriteBank; // bank to store all the images generated from Pollination

    [Header("Settings")]
    [Tooltip("Image width for generation")]
    public int imageWidth = 1920;

    [Tooltip("Image height for generation")]
    public int imageHeight = 1080;

    private void Start()
    {
        spriteBank = new List<Sprite>();
    }

    /// <summary>
    /// Public method to start image generation
    /// </summary>
    public void GenerateImage(string prompt)
    {
        StartCoroutine(GenerateImageCoroutine(prompt));
    }

    /// <summary>
    /// Made public so it can be yielded from other coroutines
    /// This allows sequential generation with yield return
    /// </summary>
    public IEnumerator GenerateImageCoroutine(string prompt)
    {
        // Format the API URL with your prompt
        string apiUrl = $"https://image.pollinations.ai/prompt/{UnityWebRequest.EscapeURL(prompt)}?width={imageWidth}&height={imageHeight}";

        Debug.Log($"Requesting image from Pollinations: {prompt}");

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(apiUrl))
        {
            // Send the request and wait for it to complete
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                // Convert Texture2D to Sprite for UI Image
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)  // Center pivot
                );

                spriteBank.Add(sprite);
                Debug.Log($"Image generated successfully! Total images: {spriteBank.Count}");
            }
            else
            {
                Debug.LogError($"Error generating image: {webRequest.error}");
                Debug.LogError($"Response code: {webRequest.responseCode}");

                // Add a null sprite as placeholder so indices stay aligned
                spriteBank.Add(null);
            }
        }
    }

    /// <summary>
    /// Clear all generated images
    /// </summary>
    public void ClearSpriteBank()
    {
        spriteBank.Clear();
        counter = 0;
    }
}