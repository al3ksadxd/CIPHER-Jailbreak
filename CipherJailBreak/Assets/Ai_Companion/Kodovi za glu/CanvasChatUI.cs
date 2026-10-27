using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasChatUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private LLamaCompanion companion;
    [SerializeField] private MonoBehaviour mouseLookScript;

    [Header("UI Elements")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text responseText;
    [SerializeField] private Button sendButton;

    private bool isChatOpen = false;

    private void Start()
    {
        if (companion == null)
            companion = FindFirstObjectByType<LLamaCompanion>();

        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendClicked);

        if (responseText != null)
            responseText.text = "Luna te sluša...";

        CloseChat();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.T))
        {
            if (inputField == null || !inputField.isFocused)
            {
                ToggleChat();
            }
        }

        if (isChatOpen && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (inputField != null && inputField.isFocused)
            {
                OnSendClicked();
            }
        }
    }

    public void ToggleChat()
    {
        isChatOpen = !isChatOpen;
        if (isChatOpen) OpenChat();
        else CloseChat();
    }

    private void OpenChat()
    {
        isChatOpen = true;
        if (chatPanel != null) chatPanel.SetActive(true);
        if (mouseLookScript != null) mouseLookScript.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (inputField != null) inputField.ActivateInputField();
    }

    private void CloseChat()
    {
        isChatOpen = false;
        if (chatPanel != null) chatPanel.SetActive(false);
        if (mouseLookScript != null) mouseLookScript.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private async void OnSendClicked()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text)) return;

        string userMsg = inputField.text;
        inputField.text = "";

        if (responseText != null)
            responseText.text = "Luna razmišlja...";

        if (companion == null)
        {
            if (responseText != null) responseText.text = "Greška: Nema LLamaCompanion reference!";
            return;
        }

        // Pozivamo Lunu iz LLamaCompanion klase
        string reply = await companion.TalkToCompanion(userMsg);

        if (responseText != null)
            responseText.text = reply;

        if (inputField != null)
            inputField.ActivateInputField();
    }
}