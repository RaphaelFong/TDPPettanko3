using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class LoadingScreen : MonoBehaviour
{
    [Header("Loading Screen Elements")]
    public GameObject loadingPanel; // The entire loading screen UI
    public VideoPlayer loadingVideo; // Video player component
    public AudioSource loadingAudio; // Audio source component
    public TMP_Text serverMessage;

    [Header("Settings")]
    public bool loopVideo = true;
    public bool loopAudio = true;

    private static LoadingScreen instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Start hidden
        Hide();
    }

    public static void SetServerMessage(string message)
    {
        if (instance != null && instance.serverMessage != null)
        {
            instance.serverMessage.text = message;
        }
    }

    /// <summary>
    /// Show loading screen and start playing audio/video
    /// </summary>
    public static void Show()
    {
        if (instance != null)
        {
            instance.ShowLoadingScreen();
        }
    }

    /// <summary>
    /// Hide loading screen and stop audio/video
    /// </summary>
    public static void Hide()
    {
        if (instance != null)
        {
            instance.HideLoadingScreen();
        }
    }

    private void ShowLoadingScreen()
    {
        // Show the panel
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Start video
        if (loadingVideo != null)
        {
            loadingVideo.isLooping = loopVideo;
            loadingVideo.Play();
        }

        // Start audio
        if (loadingAudio != null)
        {
            loadingAudio.loop = loopAudio;
            loadingAudio.Play();
        }

        Debug.Log("Loading screen started");
    }

    private void HideLoadingScreen()
    {
        // Hide the panel
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        // Stop video
        if (loadingVideo != null)
        {
            loadingVideo.Stop();
        }

        // Stop audio
        if (loadingAudio != null)
        {
            loadingAudio.Stop();
        }

        Debug.Log("Loading screen stopped");
    }
}