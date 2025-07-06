/*
 * FarmerAI.cs — updated to use a boolean parameter b_Shoot instead of a trigger
 * При стрельбе анимация запускается установкой bool‑параметра b_Shoot = true;
 * после выстрела он возвращается в false.
 * Остальной функционал без изменений.
 */

using System.Collections;
using _ShapeShifter.Player;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class FarmerAI : MonoBehaviour
{
    /*───────── PATROL ───────────────────────────────────────────────*/
    [Header("Patrol (если массив пуст — блуждание)")]
    public Transform[] waypoints;

    [Range(2f, 50f)] public float patrolRadius = 12f;
    [MinMaxSlider(0.5f, 8f)] public Vector2 waitRange = new Vector2(1f, 2.5f);
    [Range(0.5f, 5f)] public float patrolSpeed = 2.4f;
    [Range(0.5f, 10f)] public float minPatrolDistance = 3f;

    /*───────── HEARING / VISION ─────────────────────────────────────*/
    [Header("Hearing & Vision")] [Range(2f, 40f)]
    public float hearingDistance = 18f;

    [Range(1f, 15f)] public float suspicionDuration = 5f;

    [Range(5f, 40f)] public float viewDistance = 25f;
    [Range(10f, 180f)] public float viewAngle = 90f;
    [Range(10f, 180f)] public float lookOffset = 90f;

    /*───────── SHOOTING ─────────────────────────────────────────────*/
    [Header("Shooting")] [Range(5f, 25f)] public float shootingDistance = 20f;
    [Range(0.3f, 3f)] public float fireRate = 1f;
    [Range(0f, 30f)] public float shootAngleTolerance = 5f;

    public Transform muzzle;
    public GameObject bulletPrefab;
    public GameObject shotParticlePrefab; // Префаб партикла

    /*───────── ANIMATOR ────────────────────────────────────────────*/
    [Header("Animator")] public string speedParam = "f_Speed";
    public string shootBool = "b_Shoot"; // заменили Trigger на Bool

    /*───────── INTERNAL ─────────────────────────────────────────────*/
    [Tag] public string playerTag = "Player";
   [ShowNonSerializedField] private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 homePos;
    private int wpIndex;
    private float waitTimer;
    private float suspicionTimer;
    private bool canShoot;

    private enum State
    {
        Patrol,
        Suspicious,
        Attack
    }

    private State state = State.Patrol;
    public AudioClip[] ShootClips;

    /*───────── EVENT SUBSCRIPTION ───────────────────────────────────*/
    private void OnEnable() => PlayerEvents.OnPlayerEat += OnPlayerEat;
    private void OnDisable() => PlayerEvents.OnPlayerEat -= OnPlayerEat;

    /*───────── UNITY LIFECYCLE ──────────────────────────────────────*/
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        homePos = transform.position;
        if (waypoints.Length > 0) GoToNextWaypoint();
        else PickRandomPoint();
    }

    private void Update()
    {
        switch (state)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Suspicious:
                SuspiciousUpdate();
                break;
            case State.Attack:
                AttackUpdate();
                break;
        }

        animator.SetFloat(speedParam, agent.velocity.magnitude);
    }

    /*───────── EVENT HANDLER ────────────────────────────────────────*/
    private void OnPlayerEat()
    {
        if (CanSeePlayer())
            BeginAttack();
        else if (InHearingRange())
            BeginSuspicion();
    }

    /*───────── PATROL STATE ─────────────────────────────────────────*/
    private void PatrolUpdate()
    {
        if (agent.isStopped)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                agent.isStopped = false;
                if (waypoints.Length > 0) GoToNextWaypoint();
                else PickRandomPoint();
            }
        }
        else if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                 agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;
            waitTimer = Random.Range(waitRange.x, waitRange.y);
        }
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
            Vector3 dir = Random.insideUnitSphere * patrolRadius;
            dir.y = 0;
            Vector3 target = homePos + dir;

            if (!NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas)) continue;
            if ((hit.position - transform.position).sqrMagnitude < minPatrolDistance * minPatrolDistance) continue;

            agent.SetDestination(hit.position);
            return;
        }

        agent.isStopped = true;
        waitTimer = 1f;
    }

    /*───────── SUSPICIOUS STATE ─────────────────────────────────────*/
    private void BeginSuspicion()
    {
        state = State.Suspicious;
        suspicionTimer = suspicionDuration;
        agent.isStopped = true;
    }

    private void SuspiciousUpdate()
    {
        suspicionTimer -= Time.deltaTime;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(lookDir),
                540f * Time.deltaTime);

        if (CanSeePlayer())
        {
            BeginAttack();
            return;
        }

        if (suspicionTimer <= 0f)
        {
            state = State.Patrol;
            agent.isStopped = true;
            waitTimer = Random.Range(waitRange.x, waitRange.y);
        }
    }

    /*───────── ATTACK STATE ─────────────────────────────────────────*/
    private void BeginAttack()
    {
        state = State.Attack;
        canShoot = true;
    }

    private void AttackUpdate()
    {

        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            // Базовый поворот на цель
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // Добавляем смещение по Y (композиция кватернионов)
            targetRot *= Quaternion.Euler(0f, lookOffset, 0f);

            // Плавно вращаемся к получившемуся углу
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                480f * Time.deltaTime);
        }

        StartCoroutine(EndCorutine());

        animator.SetBool(shootBool, true);
    }

    private IEnumerator EndCorutine()
    {
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(0);
    }

    public void Fire()
    {
        GetComponent<AudioSource>().PlayOneShot(ShootClips[Random.Range(0, ShootClips.Length)]);

        if (bulletPrefab && muzzle)
            Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        
        if (shotParticlePrefab && player)
        {
            Vector3 dir = (player.position - muzzle.position).normalized;
            // Создаём партикл, уже ориентированный на игрока
            GameObject particle = Instantiate(
                shotParticlePrefab,
                muzzle.position,
                Quaternion.LookRotation(dir, Vector3.up));

            // Если у префаба есть Rigidbody — задаём ему скорость в нужном направлении
            if (particle.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = dir * 2;
            }
            else
            {
                // Если нет Rigidbody, а это обычный ParticleSystem в World Space,
                // то сориентировать достаточно, он будет «лететь» за счёт собственного Emission
            }
        }
    }

    /*───────── HELPERS ──────────────────────────────────────────────*/
    private bool InHearingRange() =>
        player && (player.position - transform.position).sqrMagnitude <= hearingDistance * hearingDistance;

    private bool CanSeePlayer()
    {
        if (!player) return false;

        Vector3 origin = transform.position;
        Vector3 to = player.position - origin;
        if (to.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, to) > viewAngle * 0.5f) return false;

        return Physics.Raycast(origin, to.normalized, out var hit, viewDistance) &&
               hit.transform == player;
    }

    /*───────── GIZMOS ───────────────────────────────────────────────*/
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 o = transform.position;

        bool sees = Application.isPlaying && CanSeePlayer();
        Handles.color = sees
            ? new Color(1, .2f, .2f, .15f)
            : new Color(1, .7f, .1f, .15f);
        Handles.DrawSolidArc(o, Vector3.up,
            Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward,
            viewAngle, viewDistance);

        Gizmos.color = sees ? Color.red : Color.yellow;
        Vector3 l = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
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