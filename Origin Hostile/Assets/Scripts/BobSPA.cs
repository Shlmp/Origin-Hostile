using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Rendering;
using TMPro;

public class BobSPA : MonoBehaviour
{
    private float health = 50;
    private float maxhealth = 50;

    public Transform player;
    public Transform[] patrolPoints = new Transform[4];
    public float fleeDistance = 4f;
    public float distanceCheck = 1f;

    private float distanceToPlayer;
    private bool LOS = false;
    private int patrolIndex = 0;
    private NavMeshAgent agent;

    public TextMeshProUGUI fleeText;
    public TextMeshProUGUI chaseText;

    public float viewDistance = 10f;
    public float viewAngle = 45f;

    public float criticalHealthLimit= 0.3f;

    private Dictionary<string, float> actionScores;

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
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        LOS = PlayerInFOV();

        if (Vector3.Distance(patrolPoints[patrolIndex].position, transform.position) < distanceCheck)
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

        riskFactor /= total;
        aggroFactor /= total;
        aggroFactor *= healthRatio > criticalHealthLimit ? 1 : 0;

        actionScores["Flee"] = riskFactor * 10 * (LOS == true ? 1 : 0);
        actionScores["Chase"] = aggroFactor * 10 * (LOS == true ? 1 : 0);
        actionScores["Patrol"] = 3f;

        fleeText.text = "FLEE = " + actionScores["Flee"];
        chaseText.text = "CHASE = " + actionScores["Chase"];

        string chosenAction = actionScores.Aggregate((l,r) => l.Value > r.Value ? l : r).Key;
        switch (chosenAction)
        {
            // ACT
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
        Vector3 fleDir = transform.position + (transform.position - player.position).normalized;
        agent.SetDestination(fleDir);
    }

    private void Chase()
    {
        agent.SetDestination(player.position);
    }

    private void Patrol()
    {
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }
}
