using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Character : MonoBehaviour
{
	[SerializeField] List<Die> _SavedDice = new List<Die>();
	[SerializeField] int _MaxHealth = 50;

	protected int _CurrentHealth = 0;
	protected List<Die> _DicePool = new List<Die>();
	Collider2D _Collider = null;

	public int MaxHealth => _MaxHealth;
	public int CurrentHealth => _CurrentHealth;
	public List<Die> DicePool => _DicePool;

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

		Debug.Log(ToString() + " Init");
	}

	/// <summary>
	/// Attempt to attack a character, initiating a roll off between each character's combined active dice-pools.
	/// </summary>
	/// <param name="TargetCharacter">The character to attack</param>
	/// <returns>Did we deal damage?</returns>
	public bool Attack(Character TargetCharacter)
	{
		Debug.Log(ToString() + " attacking " + TargetCharacter.ToString() + ".");

		int myTotal = Roll();
		int targetTotal = TargetCharacter.Roll();
		int difference = myTotal - targetTotal;

		if (difference > 0)
		{
			// We won the roll-off and should deal damage to the target.
			OnAttackSuccess(TargetCharacter, Mathf.Abs(difference));
		}
		else if (difference < 0)
		{
			// We lost the roll-off and should face the consequences.
			OnAttackFailed(TargetCharacter, Mathf.Abs(difference));
		}
		else
		{
			// The roll-off was a draw.
			OnAttackDraw(TargetCharacter);
		}

		return false;
	}

	/// <summary>
	/// Called when an attack is successful, i.e. the attacker rolled higher than the defendant.
	/// </summary>
	/// <param name="otherCharacter">The defending character</param>
	/// <param name="damageAmount">The amount the roll won by</param>
	protected virtual void OnAttackSuccess(Character otherCharacter, int damageAmount)
	{
		otherCharacter.OnTakeDamage(damageAmount);
	}

	/// <summary>
	/// An attack performed by this character has failed, give the defendant a chance to parry.
	/// </summary>
	/// <param name="otherCharacter">The defendant</param>
	/// <param name="parryAmount">The amount lost by</param>
	protected virtual void OnAttackFailed(Character otherCharacter, int parryAmount)
	{
		otherCharacter.OnParry(this, parryAmount);
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
	public int Roll()
	{
		int TotalAmount = 0;
		
		foreach (Die die in _DicePool)
		{
			if (die.IsDieAvailable)
			{
				TotalAmount += die.Roll();
			}
		}

		Debug.Log(ToString() + " Rolled a total of " + TotalAmount.ToString());

		return TotalAmount;
	}
}
