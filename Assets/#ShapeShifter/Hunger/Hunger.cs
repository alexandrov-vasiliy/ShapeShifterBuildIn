using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Hunger : MonoBehaviour
{
    public Image HungerBar;

    public float HungerCount;
    public float MAXHunger;

    public float Cost;

    private void Start()
    {
        StartCoroutine(Starving());
    }

    IEnumerator Starving()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            HungerCount -= Cost;
            if (HungerCount < 0) HungerCount = 0;
            HungerBar.fillAmount = HungerCount / MAXHunger;
        }
    }
    
}
