using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;

    [Header("Patrol & Investigation")]
    public Transform[] waypoints;
    public float waypointReachedDistance = 0.6f;

    [Header("Detection")]
    public float proximityThreshold = 2f;
    public float detectRange = 8f;
    public LayerMask obstacleMask;
    public float fov = 90f;

    [Header("Chase memory")]
    public float lostTime = 2f;
    public float updateDestinationRate = 0.15f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float hysteresis = 0.5f;
    public float attackCooldown = 0.8f;
    
    [Header("Recover")]
    public float smallDistance = 0.5f;
    public float duration = 0.2f;

    // Mémoire
    public float lastSeenTime;
    public Vector3 lastSeenPosition;

    // FSM
    public StateMachine sm;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        sm = new StateMachine();
    }

    void Start()
    {
        sm.ChangeState(new PatrolState(this));
    }

    void Update()
    {
        sm.Tick();
        UpdateAnimatorRunning();
    }

    // ----- Helpers -----
    public float DistanceToPlayer()
    {
        if (player == null) return 999999f;
        return Vector3.Distance(transform.position, player.position);
    }

    public bool CanDetectPlayer()
    {
        if (player == null) return false;
        float distance = DistanceToPlayer();
        
        if (distance <= proximityThreshold)
        {
            //Debug.Log("Detection: Proximity Override (Player is right next to me)");
            return true; 
        }
        
        if (distance > detectRange)
        {
            //Debug.Log($"Player too far: {distance:F1}/{detectRange}");
            return false;
        }

        // Check Angle (Cone)
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fov/2)
        {
            //Debug.Log($"Player out of cone: {angle:F1}° > {fov/2}°");
            return false;
        }

        // Check Line of Sight (Raycast)
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectRange))
        {
            if (hit.transform == player) 
            {
                return true; // Detection successful! [cite: 48]
            }
            else 
            {
                // Log what is blocking the view [cite: 66, 67]
                //Debug.Log($"Sight blocked by: {hit.transform.name}");
            }
        }
        else 
        {
            //Debug.Log("Raycast hit nothing (Player might be missing a collider)");
        }
        
        return false;
    }

    public bool InAttackRange()
    {
        return DistanceToPlayer() <= attackRange;
    }

    public void UpdateLastSeen()
    {
        if (player == null) return;
        lastSeenTime = Time.time;
        lastSeenPosition = player.position;
    }

    public void SetAttack(bool value)
    {
        if (animator != null) animator.SetBool("Attack", value);
    }

    public void SetRunning(bool value)
    {
        if (animator != null) animator.SetBool("Running", value);
    }

    void UpdateAnimatorRunning()
    {
        if (agent == null || animator == null) return;

        bool running = (agent.isStopped == false) && (agent.velocity.sqrMagnitude > 0.01f);
        //animator.SetBool("Running", running);
    }

    public void PerformAttack()
    {
        Debug.Log("ATTACK!");
    }
    
    public void FallBack()
    {
        StopCoroutine("FallBackCoroutine");
        StartCoroutine(FallBackCoroutine());
    }
    
    System.Collections.IEnumerator FallBackCoroutine()
    {
        // petite reculade configurable ici


        Vector3 start = transform.position;
        Vector3 target = start - transform.forward.normalized * smallDistance;

        float elapsed = 0f;
        Vector3 previous = start;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // ease-out pour une sensation plus douce
            float eased = 1f - Mathf.Pow(1f - t, 2f);

            Vector3 next = Vector3.Lerp(start, target, eased);
            Vector3 delta = next - previous;

            if (agent != null && agent.isOnNavMesh)
                agent.Move(delta);
            else
                transform.Translate(delta, Space.World);

            previous = next;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // garantir la position finale exacte
        Vector3 finalDelta = target - previous;
        if (finalDelta.sqrMagnitude > 0.0001f)
        {
            if (agent != null && agent.isOnNavMesh)
                agent.Move(finalDelta);
            else
                transform.Translate(finalDelta, Space.World);
        }
    }
    
    void OnDrawGizmos()
    {
        // 1. Draw the detection range (White circle at the feet)
        Gizmos.color = Color.white;
        float segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = transform.position + new Vector3(detectRange, 0, 0);
    
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = transform.position + new Vector3(Mathf.Cos(angle) * detectRange, 0, Mathf.Sin(angle) * detectRange);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        // 2. Draw the Vision Cone (Yellow lines showing the angle)
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward;
        // We use viewAngle as the half-angle (total cone is 90 if viewAngle is 45) [cite: 25]
        Vector3 leftRayDirection = Quaternion.Euler(0, -(fov/2), 0) * forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, fov/2, 0) * forward;

        Gizmos.DrawLine(transform.position + Vector3.up, (transform.position + Vector3.up) + leftRayDirection * detectRange);
        Gizmos.DrawLine(transform.position + Vector3.up, (transform.position + Vector3.up) + rightRayDirection * detectRange);

        // 3. Draw the Line of Sight Ray to the Player
        if (player != null)
        {
            bool detected = CanDetectPlayer();
            Gizmos.color = detected ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
        }
    }
}

public static class GizmoExtensions
{
    public static void DrawWireCircle(this GameObject go, Vector3 position, Vector3 up, Color color, float radius)
    {
        Gizmos.color = color;
        float angle = 0f;
        Vector3 lastPoint = Vector3.zero;
        for (int i = 0; i < 50; i++)
        {
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
            Vector3 nextPoint = position + new Vector3(x, 0, z);
            if (i > 0) Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
            angle += 2f * Mathf.PI / 50f;
        }
    }
}