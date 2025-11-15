using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StoryTimeline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("References")]
    public GeminiClient geminiClient;
    public Slider timelineSlider;
    public Transform markerContainer; // Parent for chapter markers
    public GameObject markerPrefab; // Simple Image for each chapter marker

    [Header("Preview Settings")]
    public GameObject hoverPreview; // Panel that shows on hover
    public TMP_Text previewText; // Text to show snippet
    public Image previewImage; // Optional: tiny preview image

    [Header("Visual Settings")]
    public Color normalMarkerColor = Color.gray;
    public Color currentMarkerColor = Color.cyan;
    public Color hoverMarkerColor = Color.yellow;

    private int totalParagraphs = 0;
    private bool isHovering = false;
    private Image[] chapterMarkers;

    private void Start()
    {
        // Setup slider
        if (timelineSlider != null)
        {
            timelineSlider.onValueChanged.AddListener(OnSliderValueChanged);
            timelineSlider.interactable = true;
        }

        // Hide preview initially
        if (hoverPreview != null)
            hoverPreview.SetActive(false);
    }

    /// <summary>
    /// Initialize the timeline after story is generated
    /// </summary>
    public void InitializeTimeline(int paragraphCount)
    {
        totalParagraphs = paragraphCount;

        // Setup slider range
        if (timelineSlider != null)
        {
            timelineSlider.minValue = 0;
            timelineSlider.maxValue = paragraphCount - 1;
            timelineSlider.wholeNumbers = true;
            timelineSlider.value = 0;
        }

        // Create chapter markers
        CreateChapterMarkers();

        // Update to show first chapter
        UpdateTimelineVisual(0);
    }

    /// <summary>
    /// Create visual markers for each paragraph/chapter
    /// </summary>
    private void CreateChapterMarkers()
    {
        if (markerContainer == null || markerPrefab == null)
            return;

        // Clear existing markers
        foreach (Transform child in markerContainer)
        {
            Destroy(child.gameObject);
        }

        chapterMarkers = new Image[totalParagraphs];

        // Create a marker for each paragraph
        for (int i = 0; i < totalParagraphs; i++)
        {
            GameObject marker = Instantiate(markerPrefab, markerContainer);
            Image markerImage = marker.GetComponent<Image>();

            if (markerImage != null)
            {
                markerImage.color = normalMarkerColor;
                chapterMarkers[i] = markerImage;
            }

            // Position markers evenly along the timeline
            RectTransform rt = marker.GetComponent<RectTransform>();
            if (rt != null)
            {
                float xPosition = (float)i / (totalParagraphs - 1);
                rt.anchorMin = new Vector2(xPosition, 0);
                rt.anchorMax = new Vector2(xPosition, 1);
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// Update which chapter is currently active
    /// </summary>
    public void UpdateTimelineVisual(int currentIndex)
    {
        if (chapterMarkers == null) return;

        // Reset all markers to normal color
        for (int i = 0; i < chapterMarkers.Length; i++)
        {
            if (chapterMarkers[i] != null)
            {
                chapterMarkers[i].color = normalMarkerColor;
            }
        }

        // Highlight current marker
        if (currentIndex >= 0 && currentIndex < chapterMarkers.Length)
        {
            if (chapterMarkers[currentIndex] != null)
            {
                chapterMarkers[currentIndex].color = currentMarkerColor;
            }
        }

        // Update slider without triggering callback
        if (timelineSlider != null)
        {
            timelineSlider.SetValueWithoutNotify(currentIndex);
        }
    }

    /// <summary>
    /// Called when slider value changes (user drags or clicks)
    /// </summary>
    private void OnSliderValueChanged(float value)
    {
        int targetIndex = Mathf.RoundToInt(value);

        // Jump to that chapter in GeminiClient
        if (geminiClient != null)
        {
            geminiClient.JumpToIndex(targetIndex);
        }
    }

    /// <summary>
    /// Show preview when hovering over timeline
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (hoverPreview != null)
            hoverPreview.SetActive(true);
    }

    /// <summary>
    /// Hide preview when not hovering
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (hoverPreview != null)
            hoverPreview.SetActive(false);
    }

    /// <summary>
    /// Handle clicking on timeline
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Get click position and convert to index
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            float width = rt.rect.width;
            float clickX = localPoint.x + width / 2; // Adjust for pivot
            float normalizedX = Mathf.Clamp01(clickX / width);

            int targetIndex = Mathf.RoundToInt(normalizedX * (totalParagraphs - 1));

            if (geminiClient != null)
            {
                geminiClient.JumpToIndex(targetIndex);
            }
        }
    }

    private void Update()
    {
        // Update hover preview position and content
        if (isHovering && hoverPreview != null)
        {
            UpdateHoverPreview();
        }
    }

    /// <summary>
    /// Update the hover preview to show which chapter user is hovering over
    /// </summary>
    private void UpdateHoverPreview()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, Input.mousePosition, null, out localPoint))
        {
            float width = rt.rect.width;
            float hoverX = localPoint.x + width / 2;
            float normalizedX = Mathf.Clamp01(hoverX / width);

            int hoverIndex = Mathf.RoundToInt(normalizedX * (totalParagraphs - 1));

            // Update preview text
            if (previewText != null && geminiClient != null && geminiClient.imagePrompt != null)
            {
                if (hoverIndex >= 0 && hoverIndex < geminiClient.imagePrompt.Length)
                {
                    string snippet = geminiClient.imagePrompt[hoverIndex];

                    // Show first 50 characters as preview
                    if (snippet.Length > 50)
                        snippet = snippet.Substring(0, 50) + "...";

                    previewText.text = $"Part {hoverIndex + 1}/{totalParagraphs}\n{snippet}";
                }
            }

            // Update preview image if available
            if (previewImage != null && geminiClient != null && geminiClient.pollinationsAI != null)
            {
                if (hoverIndex >= 0 && hoverIndex < geminiClient.pollinationsAI.spriteBank.Count)
                {
                    Sprite sprite = geminiClient.pollinationsAI.spriteBank[hoverIndex];
                    if (sprite != null)
                    {
                        previewImage.sprite = sprite;
                        previewImage.enabled = true;
                    }
                    else
                    {
                        previewImage.enabled = false;
                    }
                }
            }

            // Position preview above mouse
            if (hoverPreview != null)
            {
                RectTransform previewRT = hoverPreview.GetComponent<RectTransform>();
                if (previewRT != null)
                {
                    Vector2 mousePos = Input.mousePosition;
                    previewRT.position = mousePos + new Vector2(0, 100); // Offset above cursor
                }
            }

            // Highlight marker being hovered over
            HighlightHoverMarker(hoverIndex);
        }
    }

    /// <summary>
    /// Highlight the marker being hovered over
    /// </summary>
    private void HighlightHoverMarker(int hoverIndex)
    {
        if (chapterMarkers == null) return;

        for (int i = 0; i < chapterMarkers.Length; i++)
        {
            if (chapterMarkers[i] != null)
            {
                if (i == hoverIndex)
                {
                    chapterMarkers[i].color = hoverMarkerColor;
                }
                else if (i == geminiClient.textIndex)
                {
                    chapterMarkers[i].color = currentMarkerColor;
                }
                else
                {
                    chapterMarkers[i].color = normalMarkerColor;
                }
            }
        }
    }
}