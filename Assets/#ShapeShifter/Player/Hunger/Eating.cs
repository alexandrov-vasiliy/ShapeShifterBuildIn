using System.Collections;
using _ShapeShifter.Player;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class Eating : MonoBehaviour
{
    public float freezeDuration;
    public float rechargeDuration;
    public Image rechargeBar;
    public Hunger HungerBar;
    public PlayerMovement playerMovement;
    private bool isRecharged = true;
    private bool _isEating = false;
    public bool IsEating => _isEating;

    [ShowNonSerializedField] private Food targetFood;

    private void OnTriggerEnter(Collider other)
    {
        if(targetFood) return;

        if (!other.gameObject.TryGetComponent<Food>(out var food)) return;
        
        targetFood = food;
        targetFood.outline.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if(targetFood == null) return;
        
        if (other.gameObject == targetFood.gameObject)
        {
            targetFood.outline.enabled = false;
            targetFood = null;
            
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isRecharged && targetFood != null)
        {
            StartCoroutine(EatRoutin());
            targetFood.Eat();
            PlayerEvents.RaisePlayerEat();
            targetFood = null;
            isRecharged = false;
            //playerMovement.Freeze(freezeDuration);
            //playerMovement.animator.Play("Eat");
            StartCoroutine(Recharge());
            HungerBar.ResetHunger();
            
        }
    }

    IEnumerator Recharge()
    {
        float elapsed = 0f;
        rechargeBar.fillAmount = 0f;
        Debug.Log("coroutine started");

        while (elapsed < rechargeDuration)
        {
            elapsed += Time.deltaTime;
            rechargeBar.fillAmount = Mathf.Clamp01(elapsed / rechargeDuration);
            yield return null; // ждем следующий кадр
        }

        rechargeBar.fillAmount = 1f;
        isRecharged = true;
    }


    private IEnumerator EatRoutin()
    {
        _isEating = true;
        yield return new WaitForSeconds(1f);
        _isEating = false;

    }
}