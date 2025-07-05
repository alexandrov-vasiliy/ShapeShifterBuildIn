using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace _ShapeShifter.AI
{
    public class FoodItem : MonoBehaviour
    {
        [MinMaxSlider(3f, 60f)]
        [SerializeField] private Vector2 respawnTimeRange = new Vector2(8f, 25f);

        /*──────────  Статический список  ───────────*/
        public static readonly List<FoodItem> AllFood = new List<FoodItem>();

        /*──────────  Состояние & кеши  ─────────────*/
        public bool IsAvailable { get; private set; } = true;

        private Renderer[] renderers;
        private Collider[] colliders;

        /*────────────────────────────────────────────*/
        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            colliders = GetComponentsInChildren<Collider>(true);
        }

        private void OnEnable()  => AllFood.Add(this);
        private void OnDisable() => AllFood.Remove(this);

        /*────────────────────────────────────────────*/
        public void Consume()
        {
            if (!IsAvailable) return;          // вдруг параллельный вызов
            IsAvailable = false;

            SetVisible(false);
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            float wait = Random.Range(respawnTimeRange.x, respawnTimeRange.y);
            yield return new WaitForSeconds(wait);

            IsAvailable = true;
            SetVisible(true);
        }

        private void SetVisible(bool value)
        {
            foreach (var r in renderers) r.enabled = value;
            foreach (var c in colliders) c.enabled = value;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = IsAvailable ? Color.yellow : new Color(1f, 0.7f, 0.2f, 0.2f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.25f, new Vector3(0.5f, 0.5f, 0.5f));
        }
#endif
    }
}