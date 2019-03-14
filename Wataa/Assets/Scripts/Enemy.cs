using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Character
{
	public Text _textHealth;
	public Text _textDice;

	public delegate void OnEnemyDefeated(Enemy enemy);

	private static event OnEnemyDefeated EventOnEnemyDefeated;

	private PlayerCharacter _PlayerRef = null;
	
	void OnEnable()
	{
		_PlayerRef = FindObjectOfType<PlayerCharacter>();
	}

	private void Update()
	{
		_textHealth.text = _CurrentHealth.ToString();
		_textDice.text = _DicePool.Count.ToString();
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
