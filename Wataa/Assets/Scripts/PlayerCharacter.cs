using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
	public delegate void RestDelegate();
	public event RestDelegate EventOnRest;

	protected override void Start()
	{
		if (InputManager.Singleton)
		{
			InputManager.Singleton.OnLeftMouseButtonDown += OnLeftMouseButtonDown;
		}
		base.Start();

		UIHealth.Singleton.DiceFill();
		UIHealth.Singleton.UpdateHealth(CurrentHealth);
	}

	private void OnDisable()
	{
		if (InputManager.Singleton)
		{
			InputManager.Singleton.OnLeftMouseButtonDown -= OnLeftMouseButtonDown;
		}
	}

	public void Rest()
	{
		// Iterate forwards through the dice pool and activate the first inactive die
		for (int i = 0; i < _DicePool.Count; ++i)
		{
			if (!_DicePool[i].IsDieAvailable)
			{
				_DicePool[i].EnableDie();
				break;
			}
		}

		if (EventOnRest != null)
		{
			EventOnRest();
		}		
	}

	protected override void OnAttackFailed(Character otherCharacter, int parryAmount)
	{
		base.OnAttackFailed(otherCharacter, parryAmount);

		// Iterate backwards through the dice pool and deactivate the last active die.
		for (int i = _DicePool.Count - 1; i >= 0; --i)
		{
			if (_DicePool[i].IsDieAvailable)
			{
				_DicePool[i].DisableDie();
				break;
			}
		}
	}

	protected override void OnTakeDamage(int damageAmount)
	{
		base.OnTakeDamage(damageAmount);
		UIHealth.Singleton.UpdateHealth(CurrentHealth);
	}

	private void OnLeftMouseButtonDown(Vector3 mouseLocation)
	{
		// Trace for the clicked object
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

		RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
		
		if (hit.collider != null)
		{
			Character hitCharacter = hit.transform.GetComponent<Character>();
			if (hitCharacter != null)
			{
				if (hitCharacter == this)
				{
					Rest();
				}
				else
				{
					Attack(hitCharacter);
				}				
			}
		}
	}	
}
