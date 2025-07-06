using System;
using System.Collections;
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

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.Space) && isRecharged)
        {
            isRecharged = false;
            playerMovement.Freeze(freezeDuration);
            playerMovement.animator.Play("Eat");
            StartCoroutine(Recharge());
            if (other.gameObject.GetComponent<Food>())
            {
                other.gameObject.SetActive(false);
                HungerBar.ResetHunger();
            }
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
}