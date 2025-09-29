using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using TMPro;

public class BobSPA : MonoBehaviour
{
    // Health logic retained but ignored per your request (not used to kill)
    private float health = 50;
    private float maxhealth = 50;

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints = new Transform[4];
    public float fleeDistance = 4f;
    public float distanceCheck = 1f;

    [Header("FOV")]
    public float viewDistance = 10f;
    public float viewAngle = 45f;

    [Header("UI (optional)")]
    public TextMeshProUGUI fleeText;
    public TextMeshProUGUI chaseText;

    [Header("AI params")]
    public float criticalHealthLimit = 0.3f;

    [Header("Evolution")]
    // Added - inspector editable
    public int evolutionStage = 0;

    private float distanceToPlayer;
    private bool LOS = false;
    private int patrolIndex = 0;
    private NavMeshAgent agent;

    private Dictionary<string, float> actionScores;

    private void Awake()
    {
        var bob = GetComponent<BobSPA>();
        if (bob != null)
        {
            // Assign player
            var player = FindObjectOfType<PlayerMovement>();
            if (player != null)
                bob.player = player.transform;

            // Assign patrol points
            var patrols = GameObject.FindGameObjectsWithTag("PatrolPoint");
            if (patrols.Length > 0)
            {
                bob.patrolPoints = new Transform[patrols.Length];
                for (int i = 0; i < patrols.Length; i++)
                    bob.patrolPoints[i] = patrols[i].transform;
            }
        }

    }

    private void Start()
    {
        actionScores = new Dictionary<string, float>()
        {
            { "Flee", 0f },
            { "Chase", 0f },
            { "Patrol", 0f }
        };
        health = maxhealth;

        gameObject.TryGetComponent(out agent);
    }

    private void Update()
    {
        // SENSE
        if (player == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        LOS = PlayerInFOV();

        if (patrolPoints != null && patrolPoints.Length > 0 &&
            Vector3.Distance(patrolPoints[patrolIndex].position, transform.position) < distanceCheck)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        // PLAN
        float healthRatio = Mathf.Clamp01(health / maxhealth);
        float distanceRatio = Mathf.Clamp01(distanceToPlayer / fleeDistance);

        if (healthRatio <= criticalHealthLimit)
        {
            distanceRatio = 0;
        }

        float riskFactor = (1 - healthRatio) * (1 - distanceRatio);
        float aggroFactor = healthRatio * distanceRatio;
        float total = riskFactor + aggroFactor;
        if (total == 0) total = 1f;

        riskFactor /= total;
        aggroFactor /= total;
        aggroFactor *= healthRatio > criticalHealthLimit ? 1 : 0;

        actionScores["Flee"] = riskFactor * 10 * (LOS ? 1 : 0);
        actionScores["Chase"] = aggroFactor * 10 * (LOS ? 1 : 0);
        actionScores["Patrol"] = 3f;

        if (fleeText) fleeText.text = "FLEE = " + actionScores["Flee"].ToString("F2");
        if (chaseText) chaseText.text = "CHASE = " + actionScores["Chase"].ToString("F2");

        string chosenAction = actionScores.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        switch (chosenAction)
        {
            case "Flee":
                Flee();
                break;
            case "Chase":
                Chase();
                break;
            case "Patrol":
                Patrol();
                break;
            default:
                break;
        }
    }

    private bool PlayerInFOV()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        if (distanceToPlayer > viewDistance) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2) return false;

        if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, distanceToPlayer))
        {
            if (hit.collider.gameObject.TryGetComponent(out PlayerMovement _))
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public void GetHit()
    {
        health -= 9;
        Debug.Log("Was Hit! Health now at: " + health);
    }

    private void Flee()
    {
        if (agent != null && agent.enabled)
        {
            Vector3 fleDir = transform.position + (transform.position - player.position).normalized;
            agent.SetDestination(fleDir);
        }
    }

    private void Chase()
    {
        if (agent != null && agent.enabled)
        {
            agent.SetDestination(player.position);
        }
    }

    private void Patrol()
    {
        if (agent != null && agent.enabled && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }
}
