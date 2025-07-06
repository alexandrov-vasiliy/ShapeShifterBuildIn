using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

namespace _ShapeShifter.AI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class FarmAnimalAI : MonoBehaviour
    {
        /*────────── Настройки блуждания ──────────*/
        [SerializeField] private float wanderRadius = 36f;

        [MinMaxSlider(1f, 10f)] [SerializeField]
        private Vector2 wanderWaitRange = new Vector2(2f, 4f);

        /*────────── Настройки голода и еды ───────*/
        [MinMaxSlider(5f, 60f)] [SerializeField]
        private Vector2 hungerIntervalRange = new Vector2(10f, 120f);

        [MinMaxSlider(1f, 6f)] [SerializeField]
        private Vector2 eatTimeRange = new Vector2(2f, 4f);
        
        [Header("Approach to food")]
        [Range(0.1f, 2f)]
        [SerializeField] private float foodStopOffset = 0.6f;  

        /*────────── Диапазон скорости ────────────*/
        [MinMaxSlider(1f, 16f)] [SerializeField]
        private Vector2 speedRange = new Vector2(2f, 3.5f);

        /*────────── Параметры Animator ───────────*/
        [SerializeField] private string speedParam = "Speed_f";
        [SerializeField] private string eatParam = "Eat_b";
        
        

        /*────────── Приватные поля ───────────────*/
        private enum State
        {
            Wandering,
            Waiting,
            GoingToFood,
            Eating
        }

        private State state;

        private NavMeshAgent agent;
        private Animator animator;
        private Vector3 homePos; // точка, от которой считаем wanderRadius

        [ShowNonSerializedField]  private float wanderWaitTimer; // таймер паузы между точками
       [ShowNonSerializedField] private float hungerTimer; // таймер до голода

        private FoodItem currentFood;

        /*─────────────────────────────────────────*/
        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            homePos = transform.position;

            ResetHungerTimer();
            ChooseNewWanderTarget();
        }

        private void Update()
        {
            UpdateHunger(); // ↓ может перевести в GoingToFood

            switch (state)
            {
                case State.Wandering:
                    HandleWandering();
                    break;
                case State.Waiting:
                    HandleWaiting();
                    break;
                case State.GoingToFood:
                    CheckFoodArrival();
                    break;
                // State.Eating полностью в корутине
            }

            animator.SetFloat(speedParam, agent.velocity.magnitude);
        }

        /*────────────────── Логика состояний ──────────────────*/

        #region Wandering & Waiting

        private void ChooseNewWanderTarget()
        {
            Vector2 rnd = Random.insideUnitCircle * wanderRadius;
            Vector3 target = homePos + new Vector3(rnd.x, 0, rnd.y);
            
            agent.stoppingDistance = 0f;
            agent.speed = Random.Range(speedRange.x, speedRange.y);
            agent.isStopped = false;
            agent.SetDestination(target);

            state = State.Wandering;
        }

        private void HandleWandering()
        {
            if (ReachedDestination())
            {
                wanderWaitTimer = Random.Range(wanderWaitRange.x, wanderWaitRange.y);
                agent.isStopped = true;
                state = State.Waiting;
            }
        }

        private void HandleWaiting()
        {
            wanderWaitTimer -= Time.deltaTime;
            if (wanderWaitTimer <= 0f)
                ChooseNewWanderTarget();
        }

        #endregion

        #region Hunger / GoingToFood

        private void UpdateHunger()
        {
            hungerTimer -= Time.deltaTime;

            if (hungerTimer > 0f) return;

            // Уже голоден
            if (state == State.GoingToFood || state == State.Eating) return;

            FoodItem food = FindNearestAvailableFood();
            if (food == null) return; // еды нет → продолжим гулять

            BeginGoToFood(food);
        }

        private void BeginGoToFood(FoodItem food)
        {
            currentFood = food;
            agent.stoppingDistance = foodStopOffset;
            agent.speed = Random.Range(speedRange.x, speedRange.y);
            agent.isStopped = false;
            agent.SetDestination(food.transform.position);
            state = State.GoingToFood;
        }

        private void CheckFoodArrival()
        {
            // если еда исчезла/занята
            if (currentFood == null || !currentFood.IsAvailable)
            {
                state = State.Waiting; // встанем, подождём до след. wander
                wanderWaitTimer = 1.5f;
                return;
            }

            if (ReachedDestination())
                StartCoroutine(EatRoutine());
        }

        private IEnumerator EatRoutine()
        {
            state = State.Eating;
            agent.isStopped = true;

            float eatTime = Random.Range(eatTimeRange.x, eatTimeRange.y);
            animator.SetBool(eatParam, true);

            yield return new WaitForSeconds(eatTime);

            if (currentFood) currentFood.Consume();
            animator.SetBool(eatParam, false);

            currentFood = null;
            ResetHungerTimer();

            // сразу начнём паузу после еды
            wanderWaitTimer = Random.Range(wanderWaitRange.x, wanderWaitRange.y);
            state = State.Waiting;
        }

        #endregion

        /*────────────────── Вспомогательные ──────────────────*/

        private bool ReachedDestination() =>
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);

        private void ResetHungerTimer() =>
            hungerTimer = Random.Range(hungerIntervalRange.x, hungerIntervalRange.y);

        private FoodItem FindNearestAvailableFood()
        {
            float minDist = float.MaxValue;
            FoodItem nearest = null;

            foreach (var food in FoodItem.AllFood)
            {
                if (!food.IsAvailable) continue;
                float d = Vector3.SqrMagnitude(food.transform.position - transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = food;
                }
            }

            return nearest;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                Application.isPlaying ? homePos : transform.position,
                wanderRadius);
        }
#endif
    }
}