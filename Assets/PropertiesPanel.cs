using UnityEngine;

public class PropertiesPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject panel;

    // Update is called once per frame
    public void showPanel()
    {
        panel.SetActive(true);
    }
    public void hidePanel()
    {
        panel.SetActive(false);
    }
}
