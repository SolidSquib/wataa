using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPickup : MonoBehaviour, IClickable
{
	[SerializeField] int RecoveryAmount = 5;

	public bool DoClickAction(PlayerCharacter Instigator)
	{
		Instigator.Heal(RecoveryAmount);
		Destroy(gameObject);
		return true;
	}
}
