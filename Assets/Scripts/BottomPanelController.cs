using UnityEngine;
using TMPro;

public class BottomPanelController : MonoBehaviour
{
    public CanvasGroup panel;                      // UI panel to show/hide
    public TextMeshProUGUI buttonLabel;            // Assign your TMP text here
    public KeyCode hideKey = KeyCode.X;            // Optional keyboard shortcut

    private bool isVisible = false;

    void Start()
    {
        HidePanel();
    }

    void Update()
    {
        if (Input.GetKeyDown(hideKey))
        {
            HidePanel();
            UpdateButtonLabel();
        }
    }

    public void TogglePanel()
    {
        if (isVisible)
            HidePanel();
        else
            ShowPanel();

        UpdateButtonLabel();
    }

    public void ShowPanel()
    {
        panel.alpha = 1;
        panel.interactable = true;
        panel.blocksRaycasts = true;
        isVisible = true;
    }

    public void HidePanel()
    {
        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        isVisible = false;
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel != null)
        {
            buttonLabel.text = isVisible ? "Tutup Daftar Barang" : "Buka Daftar Barang";
        }
    }

    public void OnCloseButton()
    {
        HidePanel();
        UpdateButtonLabel();
    }
}
