using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PollinationsAI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] outputImage;
    public int counter = 0;
    public List<Sprite> spriteBank;

    [Header("Settings")]
    [Tooltip("Image width for generation")]
    public int imageWidth = 1920;

    [Tooltip("Image height for generation")]
    public int imageHeight = 1080;

    [Header("Retry Settings")]
    [Tooltip("Number of retry attempts before giving up")]
    public int maxRetries = 3;

    [Tooltip("Delay in seconds between retry attempts")]
    public float retryDelay = 2f;

    [Tooltip("Increase delay after each retry (exponential backoff)")]
    public bool useExponentialBackoff = true;

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
    /// Generate image with automatic retry on failure
    /// </summary>
    public IEnumerator GenerateImageCoroutine(string prompt)
    {
        int attemptCount = 0;
        bool success = false;
        Sprite generatedSprite = null;

        // Try up to maxRetries times
        while (attemptCount < maxRetries && !success)
        {
            attemptCount++;

            if (attemptCount > 1)
            {
                Debug.Log($"Retry attempt {attemptCount}/{maxRetries} for: {prompt}");
            }

            // Attempt to generate image
            yield return StartCoroutine(AttemptImageGeneration(prompt, (sprite) =>
            {
                generatedSprite = sprite;
                success = (sprite != null);
            }));

            // If failed and we have retries left, wait before trying again
            if (!success && attemptCount < maxRetries)
            {
                float delay = useExponentialBackoff
                    ? retryDelay * attemptCount  // 2s, 4s, 6s...
                    : retryDelay;                 // 2s, 2s, 2s...

                Debug.Log($"Waiting {delay}s before retry...");
                yield return new WaitForSeconds(delay);
            }
        }

        // Add the result (either sprite or null) to the bank
        spriteBank.Add(generatedSprite);

        if (success)
        {
            Debug.Log($"Image generated successfully after {attemptCount} attempt(s)! Total images: {spriteBank.Count}");
        }
        else
        {
            Debug.LogWarning($"Failed to generate image after {maxRetries} attempts. Adding null placeholder.");
        }
    }

    /// <summary>
    /// Single attempt to generate an image
    /// </summary>
    private IEnumerator AttemptImageGeneration(string prompt, System.Action<Sprite> callback)
    {
        string apiUrl = $"https://image.pollinations.ai/prompt/{UnityWebRequest.EscapeURL(prompt)}?width={imageWidth}&height={imageHeight}";

        Debug.Log($"Requesting image from Pollinations: {prompt}");

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                // Convert Texture2D to Sprite for UI Image
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                callback(sprite);
            }
            else
            {
                Debug.LogError($"Error generating image: {webRequest.error}");
                Debug.LogError($"Response code: {webRequest.responseCode}");

                callback(null);
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