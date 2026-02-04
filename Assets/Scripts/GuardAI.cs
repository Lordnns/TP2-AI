using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float waypointReachedDistance = 0.6f;

    [Header("Detection")]
    public float detectRange = 8f;

    [Header("Chase memory")]
    public float lostTime = 2f;
    public float updateDestinationRate = 0.15f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float hysteresis = 0.5f;
    public float attackCooldown = 0.8f;

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
        return DistanceToPlayer() <= detectRange;
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
        animator.SetBool("Running", running);
    }

    public void PerformAttack()
    {
        Debug.Log("ATTACK!");
        // plus tard : dégâts / projectile / animation events
    }
}