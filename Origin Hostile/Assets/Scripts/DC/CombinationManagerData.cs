using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombinationManager_SO", menuName = "Game/CombinationManager", order = 0)]
public class CombinationManagerData : ScriptableObject
{
    [System.Serializable]
    public struct StagePrefab
    {
        public int stage; // current stage
        public GameObject nextPrefab; // prefab to spawn when two of "stage" combine
    }

    public StagePrefab[] entries;
}
