using UnityEngine;
using System.Collections;

public class SidebarToggle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform sidebarTransform;   // Sidebar panel
    [SerializeField] private RectTransform iconTransform;      // Menu icon
    [SerializeField] private GameObject newInputButton;        // New Chat button (inside sidebar)
    [SerializeField] private GameObject newInputIconOnly;      // Optional: icon-only button when collapsed


    [Header("Positions")]
    [SerializeField] private float openSidebarX = -765f;       // Sidebar X when open
    [SerializeField] private float closedSidebarX = -1154f;    // Sidebar X when closed
    [SerializeField] private float iconOpenX = -634f;          // Icon X when sidebar open
    [SerializeField] private float iconClosedX = -898f;        // Icon X when sidebar closed
    [SerializeField] private float ySidebar = 0f;              // Sidebar Y
    [SerializeField] private float yIcon = 492f;               // Icon Y

    [Header("Animation Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private bool isOpen = true;  // Start open

    private void Start()
    {
        // Initialize positions for open sidebar
        if (sidebarTransform != null)
            sidebarTransform.anchoredPosition = new Vector2(openSidebarX, ySidebar);

        if (iconTransform != null)
            iconTransform.anchoredPosition = new Vector2(iconOpenX, yIcon);

        if (newInputButton != null)
            newInputButton.SetActive(true);  // Show full button

        if (newInputIconOnly != null)
            newInputIconOnly.SetActive(false); // Hide compact icon
    }

    public void ToggleSidebar()
    {
        isOpen = !isOpen;

        // Show/hide New Chat buttons
        if (newInputButton != null)
            newInputButton.SetActive(isOpen);

        if (newInputIconOnly != null)
            newInputIconOnly.SetActive(!isOpen);

        // Animate sidebar and icon
        StopAllCoroutines();
        StartCoroutine(MoveUIElement(sidebarTransform, isOpen ? openSidebarX : closedSidebarX, ySidebar));
        StartCoroutine(MoveUIElement(iconTransform, isOpen ? iconOpenX : iconClosedX, yIcon));
    }

    private IEnumerator MoveUIElement(RectTransform rect, float targetX, float targetY)
    {
        if (rect == null) yield break;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(targetX, targetY);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rect.anchoredPosition = endPos;
    }
}
