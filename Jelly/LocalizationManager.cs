using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Jelly.Localization;

namespace Jelly;

public static class LocalizationManager
{
    private static readonly Dictionary<string, LanguageSettings> loadedLanguages = [];

    private static readonly Dictionary<string, string> cachedValues = [];

    private static bool _initializing = false;

    public static Dictionary<string, LanguageSettings> AdditionalLanguageData { get; } = [];

    public static string LocalizationDataPath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Localization");

    public static bool IsInitialized { get; private set; } = false;

    public static event Action LocalizationDataReloaded;

    private static string _lang;

    public static string CurrentLanguage
    {
        get => _lang;
        set {
            ReloadAsync().Wait();

            if(loadedLanguages.ContainsKey(value))
                _lang = value;
            else
                throw new InvalidOperationException("Unknown language identifier");
        }
    }

    public static JsonSerializerOptions SerializerOptions => new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task ReloadAsync()
    {
        if(_initializing) return;
        _initializing = true;

        IsInitialized = false;
        JellyBackend.Logger.LogInfo("Reloading localization data...");

        loadedLanguages.Clear();
        cachedValues.Clear();

        Directory.CreateDirectory(LocalizationDataPath);

        foreach(var fullPath in Directory.EnumerateFiles(LocalizationDataPath))
        {
            if(Path.GetExtension(fullPath) == ".json")
            {
                var text = await File.ReadAllTextAsync(fullPath);

                LanguageSettings data = JsonSerializer.Deserialize<LanguageSettings>(text, SerializerOptions);

                loadedLanguages.Add(Path.GetFileNameWithoutExtension(fullPath), data);
            }
        }

        foreach(var item in AdditionalLanguageData)
        {
            // merge matching items (AdditionalLanguageData items take priority)
            if(loadedLanguages.TryGetValue(item.Key, out LanguageSettings settings) && item.Key == (item.Value.Identifier ?? item.Key))
            {
                if(item.Value.Fallback is not null) settings.Fallback = item.Value.Fallback;

                if(item.Value.Values is not null)
                {
                    foreach(var keyValue in item.Value.Values)
                    {
                        settings.Values[keyValue.Key] = keyValue.Value;
                    }
                }
            }
            else
                loadedLanguages[item.Key] = item.Value; // add unique items
        }

        _initializing = false;
        IsInitialized = true;

        JellyBackend.Logger.LogInfo("Localization data reloaded!");

        if(loadedLanguages.Count > 0)
        {
            string str = "";
            foreach(string key in loadedLanguages.Keys)
            {
                str += $"\n  - {loadedLanguages[key].Name ?? key}";
                if(loadedLanguages[key].Name is not null) str += $" ({key})";
            }

            JellyBackend.Logger.LogInfo("Loaded languages:" + str);
        }

        JellyBackend.Logger.LogInfo(string.Join(", ", loadedLanguages["en-us"].Values.Values));

        LocalizationDataReloaded?.Invoke();
    }

    public static string GetLocalizedValue(string token)
    {
        if(!IsInitialized)
        {
            _ = ReloadAsync();
            return token;
        }

        if(cachedValues.TryGetValue(token, out string value))
        {
            return value;
        }

        if(_lang is not null)
        {
            return TryGetValue(token, _lang);
        }

        return token;
    }

    private static string TryGetValue(string token, string langId)
    {
        List<string> failedLangs = [];

        if(!loadedLanguages.TryGetValue(langId, out LanguageSettings languageSettings))
        {
            cachedValues.Add(token, token);
            return token;
        }

        if(languageSettings.Values is null || !languageSettings.Values.TryGetValue(token, out string value))
        {
            if(languageSettings.Fallback is null || languageSettings.Fallback == langId || failedLangs.Contains(languageSettings.Fallback))
            {
                cachedValues.Add(token, token);
                return token;
            }

            failedLangs.Add(langId);
            return TryGetValue(token, languageSettings.Fallback);
        }

        cachedValues.Add(token, value);
        return value;
    }
}
