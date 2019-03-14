using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RollResult
{
	public List<List<Sprite>> _FaceSprites;
	public int _Total;
	public List<int> _Rolls;

	public RollResult(int NumDice)
	{
		_FaceSprites = new List<List<Sprite>>();
		_Total = 0;
		_Rolls = new List<int>();
	}
}

[RequireComponent(typeof(Collider2D))]
public class Character : MonoBehaviour
{
	public delegate void ActionComplete();

	[SerializeField] List<Die> _SavedDice = new List<Die>();
	[SerializeField] int _MaxHealth = 50;

	protected int _CurrentHealth = 0;
	protected List<Die> _DicePool = new List<Die>();
	Collider2D _Collider = null;

	public int MaxHealth => _MaxHealth;
	public int CurrentHealth => _CurrentHealth;
	public List<Die> DicePool => _DicePool;
	public Collider2D CharacterCollider => _Collider;

	// Start is called before the first frame update
	protected virtual void Start()
    {
        foreach (Die die in _SavedDice)
		{
			Die newDie = Instantiate(die, transform);
			newDie.EnableDie();
			_DicePool.Add(newDie);
		}

		_CurrentHealth = _MaxHealth;
		_Collider = GetComponent<Collider2D>();
	}

	public Vector3 GetTargetFightLocation(Character targetCharacter)
	{
		Collider2D targetCollider = targetCharacter.GetComponent<Collider2D>();
		return GetTargetFightLocation(targetCollider);
	}

	public Vector3 GetTargetFightLocation(Prop targetProp)
	{
		Collider2D targetCollider = targetProp.GetComponent<Collider2D>();
		return GetTargetFightLocation(targetCollider);
	}

	public Vector3 GetTargetFightLocation(Collider2D targetCollider)
	{
		Vector3 targetLocation = new Vector3(transform.position.x, targetCollider.transform.position.y, transform.position.z);
		if (targetLocation.x > targetCollider.transform.position.x)
		{
			targetLocation.x = targetCollider.transform.position.x + targetCollider.bounds.extents.x + CharacterCollider.bounds.extents.x;
			return targetLocation;
		}
		else
		{
			targetLocation.x = targetCollider.transform.position.x - targetCollider.bounds.extents.x - CharacterCollider.bounds.extents.x;
			return targetLocation;
		}
	}

	/// <summary>
	/// Attempt to attack a character, initiating a roll off between each character's combined active dice-pools.
	/// </summary>
	/// <param name="TargetCharacter">The character to attack</param>
	/// <returns>Did we deal damage?</returns>
	public IEnumerator MoveAndAttack(Character targetCharacter, ActionComplete callback = null)
	{
		Vector3 startingLocation = transform.position;
		Vector3 targetLocation = GetTargetFightLocation(targetCharacter);

		// first we need to move towards the target
		while (transform.position != targetLocation)
		{
			float step = 10.0f * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);
			yield return null;
		}

		// Calculate the results of the fight.
		int difference = FightManager.Singleton.Fight(this, targetCharacter);
		yield return new WaitForSeconds(5.0f);

		// Call the correct event.
		if (difference > 0)
		{
			// We won the roll-off and should deal damage to the target.
			OnAttackSuccess(targetCharacter, Mathf.Abs(difference));
		}
		else if (difference < 0)
		{
			// We lost the roll-off and should face the consequences.
			OnAttackFailed(targetCharacter, Mathf.Abs(difference));
		}
		else
		{
			// The roll-off was a draw.
			OnAttackDraw(targetCharacter);
		}		

		// return the character to its start location.
		while (transform.position != startingLocation)
		{
			float step = 10.0f * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, startingLocation, step);
			yield return null;
		}

		if (callback != null)
		{
			callback();
		}
	}

	public IEnumerator MoveAndAttack(Character targetCharacter, Prop useProp, ActionComplete callback = null)
	{
		Vector3 startingLocation = transform.position;
		Vector3 targetLocation = GetTargetFightLocation(useProp);

		// first we need to move towards the target
		while (transform.position != targetLocation)
		{
			float step = 10.0f * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);
			yield return null;
		}

		// Calculate the results of the fight.
		int damage = FightManager.Singleton.FightWithProp(this, targetCharacter, useProp);
		yield return new WaitForSeconds(5.0f);

		if (damage > 0)
		{
			yield return useProp.MoveAndAttack(targetCharacter, damage);
			OnAttackSuccess(targetCharacter, damage);
		}
		else
		{
			OnAttackFailed(useProp);
			useProp.Explode();
		}

		// return the character to its start location.
		while (transform.position != startingLocation)
		{
			float step = 10.0f * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, startingLocation, step);
			yield return null;
		}

		if (callback != null)
		{
			callback();
		}
	}

	/// <summary>
	/// Called when an attack is successful, i.e. the attacker rolled higher than the defendant.
	/// </summary>
	/// <param name="otherCharacter">The defending character</param>
	/// <param name="damageAmount">The amount the roll won by</param>
	protected virtual void OnAttackSuccess(Character otherCharacter, int damageAmount)
	{
		if (otherCharacter)
		{
			otherCharacter.OnTakeDamage(damageAmount);
		}		
	}

	/// <summary>
	/// An attack performed by this character has failed, give the defendant a chance to parry.
	/// </summary>
	/// <param name="otherCharacter">The defendant</param>
	/// <param name="parryAmount">The amount lost by</param>
	protected virtual void OnAttackFailed(Character otherCharacter, int parryAmount)
	{
		if (otherCharacter)
		{
			otherCharacter.OnParry(this, parryAmount);
		}		
	}

	protected virtual void OnAttackFailed(Prop prop)
	{
		if (prop)
		{
			OnTakeDamage(prop.BaseDamage);
		}
	}

	/// <summary>
	/// Called when an attack comes out perfectly even.
	/// </summary>
	/// <param name="otherCharacter">The other character in this combat.</param>
	protected virtual void OnAttackDraw(Character otherCharacter)
	{
		// Play animations?
	}

	/// <summary>
	/// Called on successful defence of an incoming attack
	/// </summary>
	/// <param name="attacker">The character performing the attack</param>
	/// <param name="parryValue">The value amount in favor of the defendant</param>
	protected virtual void OnParry(Character attacker, int parryValue)
	{

	}

	/// <summary>
	/// Take damage from an attack. Protected and can only be called from internal sources.
	/// </summary>
	/// <param name="damageAmount">The value amount of an attack in the favor of the attacker.</param>
	protected virtual void OnTakeDamage(int damageAmount)
	{
		_CurrentHealth -= Mathf.Min(damageAmount, _CurrentHealth);

		if (_CurrentHealth <= 0)
		{
			Die();
		}
	}

	/// <summary>
	/// Perform animations and remove this character.
	/// </summary>
	protected virtual void Die()
	{
		Destroy(gameObject);
	}

	/// <summary>
	/// Roll all active dice.
	/// </summary>
	/// <returns>The total number rolled</returns>
	public RollResult Roll(int numDice = -1)
	{
		RollResult results = new RollResult(DicePool.Count);

		for (int i = 0; i < (numDice == -1 ? DicePool.Count : Mathf.Min(numDice, DicePool.Count)); ++i)
		{
			if (DicePool[i].IsDieAvailable)
			{
				int result = DicePool[i].Roll();
				results._FaceSprites.Add(DicePool[i].FaceSprites);
				results._Total += result;
				results._Rolls.Add(result);
			}
		}

		return results;
	}
}
