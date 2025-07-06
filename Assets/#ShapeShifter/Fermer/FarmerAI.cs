/*────────────────────────────────────────────────────────────────────────
 *  FarmerAI.cs
 *────────────────────────────────────────────────────────────────────────*/

using System.Collections;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace _ShapeShifter.Fermer
{
   /*────────────────────────────────────────────────────────────────────────
 *  FarmerAI.cs   —  Random‑Patrol version
 *────────────────────────────────────────────────────────────────────────*/
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;     // для гизмосов
#endif

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class FarmerAI : MonoBehaviour
{
    /*────────────  СЛУЧАЙНЫЙ ПАТРУЛЬ  ────────────*/
    [Header("Random Patrol")]
    [Range(2f, 50f)]
    [SerializeField] private float patrolRadius = 12f;

    [MinMaxSlider(0.5f, 8f)]
    [SerializeField] private Vector2 waitRange = new Vector2(1f, 2.5f);

    [Range(0.5f, 5f)]
    [SerializeField] private float patrolSpeed = 2.4f;    
    [Range(4f, 15f)]
    [SerializeField] private float attackSpeed = 2.4f;

    /*────────────  ОБЗОР  ─────────────*/
    [Header("Detection")]
    [Range(5f, 40f)]
    [SerializeField] private float viewDistance = 25f;

    [Range(10f, 180f)]
    [SerializeField] private float viewAngle = 90f;

    [Range(0.5f, 2f)]
    [SerializeField] private float eyesHeight = 1.6f;

    [Tag] public string playerTag = "Player";

    /*────────────  ОГОНЬ  ─────────────*/
    [Header("Shooting")]
    [Range(5f, 25f)]
    [SerializeField] private float shootingDistance = 20f;

    [Range(0.3f, 3f)]
    [SerializeField] private float fireRate = 1f;                // выстрелов/сек

    public Transform muzzle;
    public GameObject bulletPrefab;

    /*────────────  АНИМАЦИЯ  ──────────*/
    [Header("Animator")]
    [SerializeField] private string speedParam   = "f_Speed";
    [SerializeField] private string shootTrigger = "Shoot";

    /*────────────  ВНУТРЕННЕЕ  ────────*/
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    private Vector3 homePos;
  [SerializeField]  private float waitTimer;

    private bool  canShoot;

    private enum State { Patrol, Attack }
    private State state = State.Patrol;

    /*────────────────────────────────────────────*/
    private void Start()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player   = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        if (!player) Debug.LogError("FarmerAI: Player not found by tag!");

        homePos = transform.position;
        PickNewPatrolPoint();
    }

    private void Update()
    {
        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Attack: AttackUpdate(); break;
        }

        animator.SetFloat(speedParam, agent.velocity.magnitude);
    }

    /*────────────────────  PATROL  ───────────────────*/
    private void PatrolUpdate()
    {
        // если стоим на месте и ждём
        if (agent.isStopped)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                agent.isStopped = false;
                PickNewPatrolPoint();
            }
        }
        else
        {
            // дошёл до цели
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;
                waitTimer = Random.Range(waitRange.x, waitRange.y);
            }
        }

        // замечаем «едящего» игрока?
        if (PlayerIsEating() && CanSeePlayer())
        {
            state = State.Attack;
            agent.isStopped = true;
            canShoot = true;
        }
    }

    private void PickNewPatrolPoint()
    {
        
        agent.speed = patrolSpeed;
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir.y = 0f;
        Vector3 target = homePos + randomDir;

        // ищем ближайшую точку на NavMesh
        if (NavMesh.SamplePosition(target, out var hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(transform.position);   // fallback
    }

    /*────────────────────  ATTACK  ──────────────────*/
    private void AttackUpdate()
    {
        if (!PlayerIsEating() || !CanSeePlayer())
        {
            state = State.Patrol;
            agent.isStopped = true;          // начнём с паузы
            waitTimer = Random.Range(waitRange.x, waitRange.y);
            return;
        }

        // прицел
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(dir),
                480f * Time.deltaTime);

        // стрельба
        if (dir.magnitude <= shootingDistance && canShoot)
            StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        canShoot = false;
        animator.SetTrigger(shootTrigger);

        // задержка под анимацию
        yield return new WaitForSeconds(0.1f);
         if (bulletPrefab && muzzle)
            Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

        yield return new WaitForSeconds(1f / fireRate);
        canShoot = true;
    }

    /*────────────────────  ВСПОМОГАТЕЛЬНОЕ  ─────────*/
    private bool PlayerIsEating()
    {
        if (!player) return false;
        var eater = player.GetComponent<IPlayerEater>();
        return eater != null && eater.IsEating;
    }

    private bool CanSeePlayer()
    {
        if (!player) return false;

        Vector3 origin    = transform.position;                
        Vector3 toPlayer  = player.position - origin;
        float   distance  = toPlayer.magnitude;
        if (distance > viewDistance) return false;

        // угол обзора
        if (Vector3.Angle(transform.forward, toPlayer) > viewAngle * 0.5f)
            return false;

        // прямой луч
        if (Physics.Raycast(origin, toPlayer.normalized, out var hit, viewDistance))
            return hit.transform == player;

        return false;
    }

    /*────────────────────  GIZMOS  ──────────────────*/
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 eye = transform.position + Vector3.up * eyesHeight;

        bool sees = Application.isPlaying && CanSeePlayer();
        Color fill = sees ? new Color(1f, 0.2f, 0.2f, 0.15f)
                          : new Color(1f, 0.7f, 0.1f, 0.15f);

        Handles.color = fill;
        Handles.DrawSolidArc(
            eye,
            Vector3.up,
            Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward,
            viewAngle,
            viewDistance);

        Color edge = sees ? Color.red : Color.yellow;
        Gizmos.color = edge;
        Vector3 left  = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0,  viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawLine(eye, eye + left  * viewDistance);
        Gizmos.DrawLine(eye, eye + right * viewDistance);
        Gizmos.DrawWireSphere(eye, viewDistance);

        // радиус случайного патруля
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
        Gizmos.DrawWireSphere(Application.isPlaying ? homePos : transform.position,
                              patrolRadius);
    }
#endif
}

/*──────────  Интерфейс флага «едим» у игрока  ──────────*/
public interface IPlayerEater
{
    bool IsEating { get; }
}

}