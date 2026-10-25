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
        }
    }

    public async Task<string> TalkToCompanion(string userMessage)
    {
        string playerName = dbManager.GetMemory("PlayerName", "Aleksa");

        // Jednostavan i direktan prompt - bez komplikovanih pravila koja zbunjuju model
        string fullPrompt = $"System: Ti si Luna, sarkastični i opušteni AI pratilac u igri. Tvoj igrač je {playerName}. Odgovaraj na srpskom jeziku, kratko i prirodno.\n\nUser: {userMessage}\nAssistant:";

        OllamaRequest req = new OllamaRequest
        {
            model = modelName,
            prompt = fullPrompt,
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
                return "Luna: E jbg, pukla mi veza na sekund.";
            }
        }
    }
}