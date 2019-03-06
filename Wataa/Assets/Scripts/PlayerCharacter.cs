using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
	private bool _IsBusy = false;

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

	/// <summary>
	/// Rest up, re-enabling the first disabled die in the character's pool. In reaction, all enemies on
	/// screen make an attack against the resting character.
	/// </summary>
	/// <returns></returns>
	public IEnumerator Rest(ActionComplete callback = null)
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

		// All enemies in the viewport get an attack of opportunity.
		Enemy[] AllEnemies = FindObjectsOfType<Enemy>();
		for (int i = 0; i < AllEnemies.Length; ++i)
		{
			Vector3 viewportPosition = Camera.main.WorldToViewportPoint(AllEnemies[i].transform.position);
			if (viewportPosition.x > 0 && viewportPosition.x < 1 &&
				viewportPosition.y > 0 && viewportPosition.y < 1 &&
				viewportPosition.z > 0)
			{
				yield return AllEnemies[i].MoveAndAttack(this);
			}
		}
		
		if (callback != null)
		{
			callback();
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
		if (!_IsBusy)
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
					_IsBusy = true;

					if (hitCharacter == this)
					{
						StartCoroutine(Rest(PlayerActionComplete));
					}
					else
					{
						StartCoroutine(MoveAndAttack(hitCharacter, PlayerActionComplete));
					}
				}
			}
		}
	}

	void PlayerActionComplete()
	{
		_IsBusy = false;
	}
}
