/*
 * FarmerAI.cs —  Waypoint / Random Patrol + Hearing + Vision + EVENT‑based “player is eating”
 * Unity 2021+  (NaughtyAttributes optional for MinMaxSlider)
 */
using System.Collections;
using _ShapeShifter.Player;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;            // gizmos
#endif

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class FarmerAI : MonoBehaviour
{
    /*──────────────────── PATROL ───────────────────*/
    [Header("Patrol (waypoints optional)")]
    public Transform[] waypoints;                            // if empty → random patrol

    [Range(2f, 50f)]   public float patrolRadius      = 12f;
    [MinMaxSlider(0.5f, 8f)]
    public Vector2 waitRange = new Vector2(1f, 2.5f);
    [Range(0.5f, 5f)]  public float patrolSpeed       = 2.4f;
    [Range(0.5f, 10f)] public float minPatrolDistance = 3f;

    /*──────────────────── HEARING ──────────────────*/
    [Header("Hearing")]
    [Range(2f, 40f)]   public float hearingDistance   = 18f;
    [Range(1f, 15f)]   public float suspicionDuration = 5f;

    /*──────────────────── VISION ───────────────────*/
    [Header("Vision")]
    [Range(5f, 40f)]   public float viewDistance = 25f;
    [Range(10f, 180f)] public float viewAngle    = 90f;

    /*──────────────────── SHOOTING ────────────────*/
    [Header("Shooting")]
    [Range(5f, 25f)]   public float shootingDistance = 20f;
    [Range(0.3f, 3f)]  public float fireRate         = 1f;
    public Transform muzzle;
    public GameObject bulletPrefab;

    /*──────────────────── ANIMATOR ────────────────*/
    [Header("Animator params")]
    public string speedParam = "f_Speed";
    public string shootTrig  = "Shoot";

    /*──────────────────── EVENTS  ────────────────*/
    [Header("Eating Event Memory")]
    [Range(0.5f, 10f)]
    public float eatMemoryDuration = 3f;   // «игрок ест» считается активным столько секунд после события

    /*──────────────────── RUNTIME  ───────────────*/
    [Tag] public string playerTag = "Player";
    private Transform    player;
    private NavMeshAgent agent;
    private Animator     animator;

    private Vector3 homePos;
    private int     wpIndex;
    private float   waitTimer;

    private bool  canShoot;
    private float suspicionTimer;

    private bool  playerEating;      // обновляется через событие
    private float eatTimer;          // отсчёт памяти

    private enum State { Patrol, Suspicious, Attack }
    private State state = State.Patrol;

    /*──────────────────── LIFECYCLE ───────────────*/
    private void OnEnable()  => PlayerEvents.OnPlayerEat += HandlePlayerEat;
    private void OnDisable() => PlayerEvents.OnPlayerEat -= HandlePlayerEat;

    private void Start()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player   = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        homePos  = transform.position;

        if (waypoints.Length > 0) GoToNextWaypoint();
        else                      PickRandomPoint();
    }

    private void Update()
    {
        TickEatingMemory();

        switch (state)
        {
            case State.Patrol:     PatrolUpdate();     break;
            case State.Suspicious: SuspiciousUpdate(); break;
            case State.Attack:     AttackUpdate();     break;
        }

        animator.SetFloat(speedParam, agent.velocity.magnitude);
    }

    /*──────────────────── EVENT HANDLER ───────────*/
    private void HandlePlayerEat()
    {
        playerEating = true;
        eatTimer     = eatMemoryDuration;
    }

    private void TickEatingMemory()
    {
        if (!playerEating) return;
        eatTimer -= Time.deltaTime;
        if (eatTimer <= 0f) playerEating = false;
    }

    /*──────────────────── PATROL ──────────────────*/
    private void PatrolUpdate()
    {
        if (agent.isStopped)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                agent.isStopped = false;
                if (waypoints.Length > 0) GoToNextWaypoint();
                else                      PickRandomPoint();
            }
        }
        else if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                 agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;
            waitTimer = Random.Range(waitRange.x, waitRange.y);
        }

        if (playerEating && InHearingRange() && !CanSeePlayer())
            BeginSuspicion();
        else if (playerEating && CanSeePlayer())
            BeginAttack();
    }

    private void GoToNextWaypoint()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(waypoints[wpIndex].position);
        wpIndex = (wpIndex + 1) % waypoints.Length;
    }

    private void PickRandomPoint()
    {
        agent.speed = patrolSpeed;

        for (int i = 0; i < 10; i++)
        {
            Vector3 dir = Random.insideUnitSphere * patrolRadius; dir.y = 0;
            Vector3 target = homePos + dir;
            if (!NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas)) continue;
            if ((hit.position - transform.position).sqrMagnitude < minPatrolDistance * minPatrolDistance) continue;
            agent.SetDestination(hit.position);
            return;
        }
        agent.isStopped = true; waitTimer = 1f;
    }

    /*──────────────────── SUSPICIOUS ──────────────*/
    private void BeginSuspicion()
    {
        state = State.Suspicious;
        suspicionTimer = suspicionDuration;
        agent.isStopped = true;
    }

    private void SuspiciousUpdate()
    {
        suspicionTimer -= Time.deltaTime;

        Vector3 lookDir = player.position - transform.position; lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                                          Quaternion.LookRotation(lookDir),
                                                          540f * Time.deltaTime);

        if (playerEating && CanSeePlayer())
        {
            BeginAttack();
            return;
        }

        if (suspicionTimer <= 0f || !playerEating)
        {
            state = State.Patrol;
            agent.isStopped = true;
            waitTimer = Random.Range(waitRange.x, waitRange.y);
        }
    }

    /*──────────────────── ATTACK ──────────────────*/
    private void BeginAttack()
    {
        state = State.Attack;
        agent.isStopped = true;
        canShoot = true;
    }

    private void AttackUpdate()
    {
        if (!playerEating || !CanSeePlayer())
        {
            state = State.Patrol;
            agent.isStopped = true;
            waitTimer = Random.Range(waitRange.x, waitRange.y);
            return;
        }

        Vector3 dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                                          Quaternion.LookRotation(dir),
                                                          480f * Time.deltaTime);

        if (dir.magnitude <= shootingDistance && canShoot)
            StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        canShoot = false;
        animator.SetTrigger(shootTrig);
        yield return new WaitForSeconds(0.1f);          // sync with muzzle flash
        if (bulletPrefab && muzzle)
            Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        yield return new WaitForSeconds(1f / fireRate);
        canShoot = true;
    }

    /*──────────────────── HELPERS ────────────────*/
    private bool InHearingRange() =>
        player && (player.position - transform.position).sqrMagnitude <= hearingDistance * hearingDistance;

    private bool CanSeePlayer()
    {
        if (!player) return false;
        Vector3 origin = transform.position;
        Vector3 to     = player.position - origin;
        if (to.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, to) > viewAngle * 0.5f) return false;
        return Physics.Raycast(origin, to.normalized, out var hit, viewDistance) && hit.transform == player;
    }

    /*──────────────────── GIZMOS ─────────────────*/
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 o = transform.position;

        bool sees = Application.isPlaying && CanSeePlayer();
        Handles.color = sees ? new Color(1, .2f, .2f, .15f) : new Color(1, .7f, .1f, .15f);
        Handles.DrawSolidArc(o, Vector3.up,
            Quaternion.Euler(0, -viewAngle * .5f, 0) * transform.forward,
            viewAngle, viewDistance);

        Gizmos.color = sees ? Color.red : Color.yellow;
        Vector3 l = Quaternion.Euler(0, -viewAngle * .5f, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0,  viewAngle * .5f, 0) * transform.forward;
        Gizmos.DrawLine(o, o + l * viewDistance);
        Gizmos.DrawLine(o, o + r * viewDistance);
        Gizmos.DrawWireSphere(o, viewDistance);

        Gizmos.color = new Color(0, 1, 0, .25f);
        Gizmos.DrawWireSphere(o, hearingDistance);

        Gizmos.color = new Color(0, .5f, 1, .15f);
        Gizmos.DrawWireSphere(Application.isPlaying ? homePos : transform.position,
                              patrolRadius);
    }
#endif
}
