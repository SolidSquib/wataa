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
			InputManager.Singleton.OnLeftMouseButtonUp += OnLeftMouseButtonUp;
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
			InputManager.Singleton.OnLeftMouseButtonUp -= OnLeftMouseButtonUp;
		}
	}

	public List<Enemy> GetVisibleEnemies()
	{
		List<Enemy> visibleEnemies = new List<Enemy>();

		Enemy[] allEnemies = FindObjectsOfType<Enemy>();
		for (int i = 0; i < allEnemies.Length; ++i)
		{
			Vector3 viewportPosition = Camera.main.WorldToViewportPoint(allEnemies[i].transform.position);
			if (viewportPosition.x > 0 && viewportPosition.x < 1 &&
				viewportPosition.y > 0 && viewportPosition.y < 1 &&
				viewportPosition.z > 0)
			{
				visibleEnemies.Add(allEnemies[i]);
			}
		}

		return visibleEnemies;
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
		List<Enemy> visibleEnemies = GetVisibleEnemies();
		foreach (Enemy enemy in visibleEnemies)
		{
			yield return enemy.MoveAndAttack(this);
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
				//Character hitCharacter = hit.transform.GetComponent<Character>();
				if (hit.transform.GetComponent<Character>() is Character hitCharacter)
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
				else if (hit.transform.GetComponent<Prop>() is Prop hitProp)
				{
					Vector2 clickOffset = hitProp.transform.position - mousePos;
					DragDropManager.Singleton.StartDragDropOp(hitProp, mousePos, clickOffset, OnPropDropped);
				}
			}
		}
	}

	private void OnLeftMouseButtonUp(Vector3 mouseLocation)
	{
		DragDropManager.Singleton.StopDragDropOp();
	}

	void PlayerActionComplete()
	{
		_IsBusy = false;
	}

	void OnPropDropped(Prop prop, Vector3 dropLocation, Collider2D[] recievers)
	{
		Enemy enemy = null;
		foreach(Collider2D collider in recievers)
		{
			if (collider.gameObject.GetComponent<Enemy>() is Enemy enemyReciever)
			{
				enemy = enemyReciever;
				break;
			}
		}

		if (enemy)
		{
			StartCoroutine(MoveAndAttack(enemy, prop, PlayerActionComplete));
			
		}
	}
}
