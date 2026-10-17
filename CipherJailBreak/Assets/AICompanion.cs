using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class OllamaRequest
{
    public string model;
    public string prompt;
    public bool stream = false;
}

[System.Serializable]
public class OllamaResponse
{
    public string response;
}

public class AICompanion : MonoBehaviour
{
    [Header("Ollama Config")]
    [SerializeField] private string ollamaUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "qwen2.5:3b";

    [Header("Dependencies")]
    [SerializeField] private SQLiteManager dbManager;

    private void Start()
    {
        if (dbManager == null) dbManager = GetComponent<SQLiteManager>();
        if (dbManager == null) dbManager = gameObject.AddComponent<SQLiteManager>();

        if (string.IsNullOrEmpty(dbManager.GetMemory("PlayerName")))
        {
            dbManager.SaveMemory("PlayerName", "Aleksa");
            dbManager.SaveMemory("InsideJoke", "Aleksa stalno spamuje Dash na E taster.");
        }
    }

    public async Task<string> TalkToCompanion(string userMessage)
    {
        string playerName = dbManager.GetMemory("PlayerName", "Aleksa");
        string insideJoke = dbManager.GetMemory("InsideJoke", "");

        // Prompt koji definiše tačan stil i ton Lune
        string systemPrompt =
            "Ti si Luna — opuštena, pametna, odana i blago sarkastična devojka/companion u igri.\n" +
            "Pričaš direktno sa tvojim igračem koji se zove " + playerName + ".\n" +
            "Znaš o njemu: " + insideJoke + "\n\n" +
            "STIL PRIČANJA:\n" +
            "- Pričaj potpuno prirodno na srpskom, opušteno kao da ste dobri ortaci (koristi 'E', 'Brate', 'Ma daj', 'Cuti tu').\n" +
            "- Budi duhovita, podržavajuća, ali i spremna da ga malo bockaš kad pravi gluposti.\n" +
            "- Kratki odgovori (1-2 rečenice max).\n\n" +
            "PRIMERI DIJALOGA:\n" +
            "Aleksa: Luna šta ima?\n" +
            "Luna: E brate, evo gledam te kako opet spamuješ taj E taster kao da ti život zavisi od njega! Šta ima kod tebe?\n\n" +
            "Aleksa: Kako ti se čini ovaj fajt?\n" +
            "Luna: Iskreno? Ako nastaviš ovako da ulećeš bez stamine, skupljaću te u delovima sa poda!\n\n" +
            playerName + ": " + userMessage + "\n" +
            "Luna:";

        OllamaRequest req = new OllamaRequest
        {
            model = modelName,
            prompt = systemPrompt,
            stream = false
        };

        string json = JsonUtility.ToJson(req);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                OllamaResponse res = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                return res.response.Trim();
            }
            else
            {
                Debug.LogError("Ollama Error: " + request.error);
                return "Luna: E jbg, pukla mi veza na sekund, ponovi?";
            }
        }
    }
}