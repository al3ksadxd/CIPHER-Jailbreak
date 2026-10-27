using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using LLama;
using LLama.Common;
using LLama.Native;

public class LLamaCompanion : MonoBehaviour
{
    private LLamaWeights model;
    private LLamaContext context;
    private InteractiveExecutor executor;
    private bool isInitialized = false;

    async void Start()
    {
        // Ručno podešavanje putanje do nativnih biblioteka iz korena projekta
        string rootDir = Directory.GetCurrentDirectory();
        
        // Putanja do .gguf modela u StreamingAssets folderu
        string modelPath = Path.Combine(Application.dataPath, "Ai_Companion", "StreamingAssets", "AI_model.gguf");

        if (!File.Exists(modelPath))
        {
            Debug.LogError($"[Luna AI]: Model nije pronađen na putanji: {modelPath}");
            return;
        }

        Debug.Log("[Luna AI]: Inicijalizacija LLamaSharp drajvera i učitavanje modela...");

        try
        {
            // Ovde gde učitavaš model / inicijalizujesz LLamaSharp
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 2048,
                GpuLayerCount = 0
            };

            // Asinhrono učitavanje težina modela
            model = await LLamaWeights.LoadFromFileAsync(parameters);
            context = model.CreateContext(parameters);
            executor = new InteractiveExecutor(context);

            isInitialized = true;
            Debug.Log("[Luna AI]: Luna je spremna i puštena sa lanca! GPU ubrzanje aktivno.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"UNUTRAŠNJA GREŠKA: {ex.Message} | Inner: {ex.InnerException?.Message}");
        }
    }

    // Metoda preko koje igra šalje poruku Luni i dobijamo odgovor
    // Metoda preko koje igra šalje poruku Luni i dobijamo odgovor
    public async Task<string> TalkToCompanion(string prompt)
    {
        if (!isInitialized)
        {
            return "Sačekaj bre, Luna se još uvek učitava u memoriju!";
        }

        string response = "";
        try
        {
            // Qwen obožava ChatML format (system, user, assistant tagove)
            string formattedPrompt = $"<|im_start|>system\nTi si Luna, inteligentni i prisni AI saputnik u igri. Odgovaraj kratko, jasno i prirodno na srpskom jeziku.<|im_end|>\n<|im_start|>user\n{prompt}<|im_end|>\n<|im_start|>assistant\n";

            var inferParams = new InferenceParams()
            {
                MaxTokens = 150,
                AntiPrompts = new System.Collections.Generic.List<string> { "<|im_end|>", "<|im_start|>" }
            };

            await foreach (var text in executor.InferAsync(formattedPrompt, inferParams))
            {
                response += text;
            }

            // Čišćenje za svaki slučaj
            response = System.Text.RegularExpressions.Regex.Replace(response, "<.*?>", string.Empty);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Luna AI]: Puklo tokom generisanja odgovora -> {e.Message}");
            response = "Greška u sistemu, Luna se zbunila.";
        }

        return response.Trim();
    }

    private void OnDestroy()
    {
        // Oslobađamo resurse kad se scena gasi
        context?.Dispose();
        model?.Dispose();
    }
}