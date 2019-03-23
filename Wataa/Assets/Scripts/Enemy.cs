using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Character
{
	public Text _textHealth;
	public Text _textDice;
	
	private PlayerCharacter _PlayerRef = null;
	
	private void OnEnable()
	{
		_PlayerRef = FindObjectOfType<PlayerCharacter>();
		FightManager.Singleton.RegisterEnemy(this);
	}

	private void OnDisable()
	{
		FightManager.Singleton.UnregisterEnemy(this);
	}

	private void Update()
	{
		_textHealth.text = _CurrentHealth.ToString();
		_textDice.text = _DicePool.Count.ToString();
	}

	protected override void Die()
	{		
		base.Die();
	}
}
