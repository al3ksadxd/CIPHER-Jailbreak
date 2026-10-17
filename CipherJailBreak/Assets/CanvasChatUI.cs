using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasChatUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private AICompanion companion;
    [SerializeField] private MonoBehaviour mouseLookScript; // Prevuci MouseLook skriptu ovde!

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text responseText;
    [SerializeField] private Button sendButton;

    private bool isChatOpen = false;

    private void Start()
    {
        if (companion == null)
            companion = FindObjectOfType<AICompanion>();

        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendClicked);

        if (responseText != null)
            responseText.text = "Luna te sluša...";

        // Na startu zatvorimo chat i zaključamo kursor za igru
        CloseChat();
    }

    private void Update()
    {
        // Pritisak na TAB ili T otvara/zatvara chat
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.T))
        {
            if (!inputField.isFocused)
            {
                ToggleChat();
            }
        }

        // Slanje poruke na Enter dok je polje fokusirano
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

        if (isChatOpen)
        {
            OpenChat();
        }
        else
        {
            CloseChat();
        }
    }

    private void OpenChat()
    {
        isChatOpen = true;

        // 1. Privremeno ugasimo skriptu za gledanje mišem da ne zaključava kursor
        if (mouseLookScript != null)
            mouseLookScript.enabled = false;

        // 2. Otključamo kursor i prikažemo ga
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Automatski fokusiramo polje za kucanje
        if (inputField != null)
        {
            inputField.ActivateInputField();
        }
    }

    private void CloseChat()
    {
        isChatOpen = false;

        // 1. Ponovo upalimo skriptu za kameru
        if (mouseLookScript != null)
            mouseLookScript.enabled = true;

        // 2. Zaključamo kursor za First-Person kretanje
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private async void OnSendClicked()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text)) return;

        string userMsg = inputField.text;
        inputField.text = ""; // Očisti polje

        if (responseText != null)
            responseText.text = "Luna razmišlja...";

        // Pozivamo Ollamu
        string reply = await companion.TalkToCompanion(userMsg);

        if (responseText != null)
            responseText.text = reply;

        // Vratimo fokus u input field za sledeću poruku
        if (inputField != null)
            inputField.ActivateInputField();
    }
}