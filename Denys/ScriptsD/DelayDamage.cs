using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDamage : MonoBehaviour
{
	public int zombDamage = 15;

	void OnTriggerEnter(Collider other){
		if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().TakeDamage(zombDamage, "Zombie", true);
        }
	}
}
