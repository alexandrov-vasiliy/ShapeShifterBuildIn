using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class FarmAnimalAI : MonoBehaviour
{   /*───────────────────────────  НАСТРОЙКИ  ───────────────────────────*/

    [Header("Общее поведение")]
    [SerializeField] private float wanderRadius = 5f;          // радиус блуждания

    [Space]
    [Header("Диапазоны рандомизации")]

    [MinMaxSlider(0f, 10f)]                                    // ✔ слайдер с двумя ручками
    [SerializeField] private Vector2 waitTimeRange = new Vector2(2f, 5f);

    [MinMaxSlider(0.5f, 4f)]
    [SerializeField] private Vector2 speedRange = new Vector2(1.5f, 2.5f);

    [Space]
    [Header("Параметры Animator")]
    [SerializeField] private string speedParam = "Speed_f";
    [SerializeField] private string eatParam   = "Eat_b";

    /*───────────────────────────  ПОЛЯ  ────────────────────────────────*/

    private UnityEngine.AI.NavMeshAgent agent;
    private Animator animator;
    private Vector3 startPosition;

    /*───────────────────────────  ЖИЗНЕННЫЙ ЦИКЛ  ──────────────────────*/

    private void Start()
    {
        agent        = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator     = GetComponent<Animator>();
        startPosition = transform.position;

        StartCoroutine(BehaviorRoutine());
    }

    private void Update() => UpdateAnimations();

    /*───────────────────────────  ОСНОВНОЙ ЦИКЛ  ───────────────────────*/

    private IEnumerator BehaviorRoutine()
    {
        while (true)
        {
            Vector3 target = GetRandomPoint();     // 1. найти цель
            MoveTo(target);                        // 2. пойти к ней

            while (!HasReachedDestination())       // 3. ждать прибытия
                yield return null;

            yield return WaitAndEat();             // 4. постоять/поесть
        }
    }

    /*───────────────────────────  ЛОГИКА  ──────────────────────────────*/

    /// <summary>Случайная точка внутри круга wanderRadius.</summary>
    private Vector3 GetRandomPoint()
    {
        Vector2 random2D = Random.insideUnitCircle * wanderRadius;
        return startPosition + new Vector3(random2D.x, 0, random2D.y);
    }

    /// <summary>Задать новую цель и случайную скорость.</summary>
    private void MoveTo(Vector3 point)
    {
        agent.speed = Random.Range(speedRange.x, speedRange.y);
        agent.isStopped = false;
        agent.SetDestination(point);
    }

    /// <summary>Проверка, что агент добрался.</summary>
    private bool HasReachedDestination() =>
        !agent.pathPending &&
        agent.remainingDistance <= agent.stoppingDistance &&
        (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);

    /// <summary>Ждёт случайное время и запускает анимацию еды.</summary>
    private IEnumerator WaitAndEat()
    {
        float wait = Random.Range(waitTimeRange.x, waitTimeRange.y);

        agent.isStopped = true;
        animator.SetBool(eatParam, true);

        yield return new WaitForSeconds(wait);
        

        animator.SetBool(eatParam, false);
        yield return new WaitForSeconds(wait);
    }

    /*───────────────────────────  АНИМАЦИЯ  ────────────────────────────*/

    private void UpdateAnimations()
    {
        animator.SetFloat(speedParam, agent.velocity.magnitude);
    }

    /*───────────────────────────  GIZMOS (опционально)  ────────────────*/

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position,
                              wanderRadius);
    }
#endif
}
