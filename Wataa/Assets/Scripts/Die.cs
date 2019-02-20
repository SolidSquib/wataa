using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : MonoBehaviour
{
	[SerializeField] int _Sides = 6;

	bool _DieAvailable = false;

	public bool IsDieAvailable => _DieAvailable;

    public int Roll()
	{
		return Random.Range(1, _Sides);
	}

	public void DisableDie()
	{
		_DieAvailable = false;
	}

	public void EnableDie()
	{
		_DieAvailable = true;
	}
}
