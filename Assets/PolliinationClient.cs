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

    private void Start()
    {
        spriteBank = new List<Sprite>();
        // Example usage
        //GenerateImage("Moonlight loli");
    }

    public void GenerateImage(string prompt)
    {
        StartCoroutine(GenerateImageCoroutine(prompt));
    }

    public IEnumerator GenerateImageCoroutine(string prompt)
    {
        // Format the API URL with your prompt
        string apiUrl = $"https://image.pollinations.ai/prompt/{UnityWebRequest.EscapeURL(prompt)}?width=1920&height=1080";

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
                    new Vector2(0.5f, 0.5f)  // Center pivot
                );

                spriteBank.Add(sprite);
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
}