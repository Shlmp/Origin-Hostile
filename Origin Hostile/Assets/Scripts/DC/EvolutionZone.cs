using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EvolutionZone : MonoBehaviour
{
    // Current enemies that have been dropped and are waiting inside the zone
    private List<GameObject> droppedEnemies = new List<GameObject>();

    // Vertical offset for placement inside zone
    public float placedY = 0.5f;

    private void Reset()
    {
        // ensure collider is trigger by default
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    public void ReceiveDroppedEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        // Place enemy at center of zone (keep zone's world-space center)
        enemy.transform.position = transform.position + Vector3.up * placedY;
        enemy.transform.rotation = Quaternion.identity;

        // Parent to zone so it stays in place; also keep it immobile
        enemy.transform.SetParent(this.transform);

        // Ensure enemy AI/agent are disabled (defensive)
        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        var ai = enemy.GetComponent<BobSPA>();
        if (ai != null) ai.enabled = false;

        var col = enemy.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Register
        droppedEnemies.Add(enemy);

        // Attempt to find a matching stage pair
        TryCombine();
    }

    private void TryCombine()
    {
        if (droppedEnemies.Count < 2) return;

        // Find two with same evolutionStage
        for (int i = 0; i < droppedEnemies.Count; i++)
        {
            var e1 = droppedEnemies[i];
            if (e1 == null) continue;
            var b1 = e1.GetComponent<BobSPA>();
            if (b1 == null) continue;

            for (int j = i + 1; j < droppedEnemies.Count; j++)
            {
                var e2 = droppedEnemies[j];
                if (e2 == null) continue;
                var b2 = e2.GetComponent<BobSPA>();
                if (b2 == null) continue;

                if (b1.evolutionStage == b2.evolutionStage)
                {
                    int stage = b1.evolutionStage;
                    // Remove both from list
                    droppedEnemies.Remove(e1);
                    droppedEnemies.Remove(e2);

                    // Destroy the two old ones
                    Destroy(e1);
                    Destroy(e2);

                    // Ask CombinationManager for next prefab
                    var prefab = CombinationManager.Instance.GetNextPrefabForStage(stage);
                    if (prefab != null)
                    {
                        // instantiate at zone center
                        var spawned = Instantiate(prefab, transform.position + Vector3.up * placedY, Quaternion.identity);
                        // If the spawned prefab has a BobSPA with evolutionStage, rely on prefab to set it;
                        // otherwise, attempt to increment:
                        var spawnedBob = spawned.GetComponent<BobSPA>();
                        if (spawnedBob != null)
                        {
                            // leave as prefab's stage, but if zero, you may want to set it:
                            // spawnedBob.evolutionStage = stage + 1;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"CombinationManager returned no prefab for stage {stage}. Both enemies destroyed.");
                    }

                    // Only combine one pair per drop; exit
                    return;
                }
            }
        }
    }

    // Optional: if you want to clear dropped list on zone reset or externally
    public void ClearAll()
    {
        foreach (var e in droppedEnemies)
        {
            if (e != null) Destroy(e);
        }
        droppedEnemies.Clear();
    }
}
