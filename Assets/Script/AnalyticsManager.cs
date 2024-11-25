using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }
    private bool _isInitialized;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: If you want it to persist across scenes
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            _isInitialized = true;
            Debug.Log("Analytics initialized successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Analytics initialization failed: {ex.Message}");
        }
    }

    public void PlayTime(float playTime)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("Analytics is not initialized. Skipping PlayTime event.");
            return;
        }

        CustomEvent myEvent = new CustomEvent("playTime")
        {
            { "playtime", playTime }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void Difficulty(string difficulties)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("Analytics is not initialized. Skipping Difficulty event.");
            return;
        }

        CustomEvent myEvent = new CustomEvent("difficulties")
        {
            { "diff", difficulties }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void Match()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("Analytics is not initialized. Skipping Match event.");
            return;
        }

        AnalyticsService.Instance.RecordEvent("Match_Done");
        AnalyticsService.Instance.Flush();
    }
    public void Similarity(int similarity)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("Analytics is not initialized. Skipping Difficulty event.");
            return;
        }

        CustomEvent myEvent = new CustomEvent("Similiar")
        {
            { "Sim", similarity }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }
}
