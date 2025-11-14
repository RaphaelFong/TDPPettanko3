using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PollinationsAI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] outputImage;  // UI Image instead of Renderer
    public int counter = 0;

    private void Start()
    {
        // Example usage
        //GenerateImage("Moonlight loli");
        //GenerateImage("China dress loli");
        //GenerateImage("Cosmos loli");
        //GenerateImage("School uniform loli");
        //GenerateImage("Cat girl loli");
        //GenerateImage("Barista loli");
    }

    public void GenerateImage(string prompt)
    {
        StartCoroutine(GenerateImageCoroutine(prompt));
    }

    private IEnumerator GenerateImageCoroutine(string prompt)
    {
        string testUrl = "https://image.pollinations.ai/";
        using (UnityWebRequest testRequest = UnityWebRequest.Head(testUrl))
        {
            yield return testRequest.SendWebRequest();

            if (testRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Cannot reach server: {testRequest.error}");
                yield break;
            }
            else
            {
                Debug.Log("Successfully connected to server");
            }
        }

        Debug.Log("----------------------");


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

                // Apply to UI Image
                outputImage[counter].sprite = sprite;

                // Optional: Adjust image size to match texture dimensions
                outputImage[counter].SetNativeSize();

                counter++;
            }
            else
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
        }
    }
}