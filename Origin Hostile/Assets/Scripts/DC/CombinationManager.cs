using System.Collections.Generic;
using UnityEngine;

// Non-SO singleton wrapper so it's easy to reference at runtime
public class CombinationManager : MonoBehaviour
{
    public static CombinationManager Instance { get; private set; }

    [Header("Reference a ScriptableObject mapping stages -> next prefabs (optional)")]
    public CombinationManagerData dataSource;

    [Header("Or directly assign pairs here (fallback)")]
    public List<CombinationManagerData.StagePrefab> directEntries = new List<CombinationManagerData.StagePrefab>();

    private Dictionary<int, GameObject> lookup = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build lookup from data source + direct entries
        lookup.Clear();
        if (dataSource != null && dataSource.entries != null)
        {
            foreach (var e in dataSource.entries)
            {
                if (!lookup.ContainsKey(e.stage) && e.nextPrefab != null)
                    lookup.Add(e.stage, e.nextPrefab);
            }
        }

        foreach (var e in directEntries)
        {
            if (!lookup.ContainsKey(e.stage) && e.nextPrefab != null)
                lookup.Add(e.stage, e.nextPrefab);
        }
    }

    public GameObject GetNextPrefabForStage(int stage)
    {
        if (lookup.TryGetValue(stage, out var prefab)) return prefab;
        return null;
    }
}
