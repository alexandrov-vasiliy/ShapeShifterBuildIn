using System;
using UnityEngine;
using UnityEngine.UI;

public class Eating : MonoBehaviour
{
    public float freezeDuration;
    public Hunger HungerBar;
    public PlayerMovement playerMovement;
    
    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            playerMovement.Freeze(freezeDuration);
            playerMovement.animator.Play("Eat");
            
            if (other.gameObject.GetComponent<Food>())
            {
                other.gameObject.SetActive(false);
                HungerBar.ResetHunger();
                
            }
        }
    }
    
    
    

}
