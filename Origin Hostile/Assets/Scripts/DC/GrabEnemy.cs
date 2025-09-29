using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GrabEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint; // assign in inspector (child transform above player)
    public float holdY = 1f;    // local Y offset while held

    private MyActions actions;
    private GameObject nearbyEnemy;      // candidate to pick
    private GameObject heldEnemy;        // currently held (only one)
    private EvolutionZone currentZone;   // set when entering an EvolutionZone

    private void Start()
    {
        actions = new MyActions();
        actions.Gameplay.Enable();

        if (holdPoint == null)
        {
            Debug.LogWarning("GrabEnemy: holdPoint not set. Creating a default at player's transform.");
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.parent = transform;
            hp.transform.localPosition = new Vector3(0, holdY, 0);
            holdPoint = hp.transform;
        }
    }

    private void Update()
    {
        bool interactPressed = false;

        // Prefer Input System action if available (use .triggered if present)
        try
        {
            if (actions != null && actions.Gameplay.Interact != null)
            {
                // If the InputAction supports .triggered (works for button), use it; otherwise fallback.
                interactPressed = actions.Gameplay.Interact.triggered;
            }
        }
        catch { /* ignore and fallback */ }

        if (interactPressed)
        {
            if (heldEnemy == null)
            {
                TryPickup();
            }
            else
            {
                // Attempt to drop into the current EvolutionZone only (Option A)
                if (currentZone != null)
                {
                    DropIntoZone(currentZone);
                }
                else
                {
                    // Not in zone -> do nothing (per your choice)
                    Debug.Log("Not inside EvolutionZone. Can't drop.");
                }
            }
        }
    }

    private void TryPickup()
    {
        if (nearbyEnemy == null) return;
        if (heldEnemy != null) return; // already holding one, do nothing

        // Parent to holdPoint and adjust local position
        heldEnemy = nearbyEnemy;
        heldEnemy.transform.SetParent(holdPoint);
        heldEnemy.transform.localPosition = new Vector3(0f, holdY, 0f);
        heldEnemy.transform.localRotation = Quaternion.identity;

        // Disable NavMeshAgent and AI script (BobSPA)
        var agent = heldEnemy.GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        var ai = heldEnemy.GetComponent<BobSPA>();
        if (ai != null) ai.enabled = false;

        // Disable collider so it doesn't affect the player (per your request)
        var col = heldEnemy.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log("Picked up: " + heldEnemy.name);
    }

    private void DropIntoZone(EvolutionZone zone)
    {
        if (heldEnemy == null) return;

        // Un-parent (the zone will re-parent to itself or place it)
        heldEnemy.transform.SetParent(null);

        // Let the zone handle final placement & immobilization
        zone.ReceiveDroppedEnemy(heldEnemy);

        // Clear local held reference
        Debug.Log("Dropped into zone: " + zone.name + " enemy: " + heldEnemy.name);
        heldEnemy = null;
    }

    // Track nearby enemy candidate
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<BobSPA>() != null)
        {
            // candidate to pick up
            // Only set nearby if we are not holding anything
            if (heldEnemy == null)
            {
                nearbyEnemy = other.gameObject;
                // do not pick here; wait for E
            }
        }

        // Detect if we are inside an evolution zone
        if (other.CompareTag("EvolutionZone"))
        {
            var ez = other.GetComponent<EvolutionZone>();
            if (ez != null)
                currentZone = ez;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // same as stay for zones (ensures detection)
        if (other.CompareTag("EvolutionZone"))
        {
            var ez = other.GetComponent<EvolutionZone>();
            if (ez != null)
                currentZone = ez;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyEnemy)
        {
            nearbyEnemy = null;
        }

        if (other.CompareTag("EvolutionZone"))
        {
            var ez = other.GetComponent<EvolutionZone>();
            if (ez != null && currentZone == ez)
                currentZone = null;
        }
    }
}
