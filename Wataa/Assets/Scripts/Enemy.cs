using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
	public delegate void OnEnemyDefeated(Enemy enemy);

	private static event OnEnemyDefeated EventOnEnemyDefeated;

	private PlayerCharacter _PlayerRef = null;
	
	void OnEnable()
	{
		_PlayerRef = FindObjectOfType<PlayerCharacter>();
	}

	public static void BindOnEnemyDefeated(OnEnemyDefeated callback)
	{
		if (callback != null)
		{
			EventOnEnemyDefeated += callback;
		}
	}

	public static void UnbindOnEnemyDefeated(OnEnemyDefeated callback)
	{
		if (callback != null)
		{
			EventOnEnemyDefeated -= callback; 
		}
	}

	protected override void Die()
	{
		if (EventOnEnemyDefeated != null)
		{
			EventOnEnemyDefeated(this);
		}
		
		base.Die();
	}
}
