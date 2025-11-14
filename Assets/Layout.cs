using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldAutoResize : MonoBehaviour
{
    public TMP_InputField inputField;     // The TMP_InputField (root)
    public RectTransform textAreaRect;    // The Text Area RectTransform
    public RectTransform inputRect;       // InputField_Root RectTransform
    public float padding = 10f;           // extra space for background

    void Start()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(_ => RefreshHeight());
        RefreshHeight();
    }

    void RefreshHeight()
    {
        if (textAreaRect == null || inputRect == null) return;

        // Get preferred height of text
        float textHeight = inputField.textComponent.preferredHeight + padding;

        // Apply to InputField background
        inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, textHeight);
    }
}
