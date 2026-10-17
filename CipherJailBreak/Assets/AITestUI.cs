using UnityEngine;

public class AITestUI : MonoBehaviour
{
    [SerializeField] private AICompanion companion;
    private string inputMessage = "";
    private string lastResponse = "Luna te sluša...";

    private void Start()
    {
        if (companion == null)
        {
            companion = GetComponent<AICompanion>();
        }
    }

    private async void OnGUI()
    {
        // Sprečava da klikovi u prozoru pomeraju kameru ili igrača u igri
        GUI.Box(new Rect(20, 20, 400, 200), "Luna AI Companion Chat");

        GUI.Label(new Rect(30, 50, 380, 70), lastResponse);

        // Polje za kucanje
        inputMessage = GUI.TextField(new Rect(30, 130, 280, 30), inputMessage);

        // Dugme za slanje (koristimo Event proweru da sigurno registruje klik)
        bool buttonClicked = GUI.Button(new Rect(320, 130, 80, 30), "Pošalji");
        bool enterPressed = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

        if ((buttonClicked || enterPressed) && !string.IsNullOrEmpty(inputMessage))
        {
            string msgToSend = inputMessage;
            inputMessage = "";
            lastResponse = "Luna razmišlja...";

            // Šalje poruku Luni i čeka odgovor
            lastResponse = await companion.TalkToCompanion(msgToSend);

            // Fokusira ponovo prozor da možeš nastaviti kucanje
            GUI.FocusControl(null);
        }
    }
}