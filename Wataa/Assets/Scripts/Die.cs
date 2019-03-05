using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : MonoBehaviour
{
	[SerializeField] int _Sides = 6;
	[SerializeField] List<Sprite> _FaceSprites = new List<Sprite>(6);

	bool _DieAvailable = false;
	int _LastRoll = 0;

	public bool IsDieAvailable => _DieAvailable;
	public int LastRoll => _LastRoll;
	public List<Sprite> FaceSprites => _FaceSprites;

	public int Roll()
	{
		_LastRoll = Random.Range(1, _Sides);
		return LastRoll;
	}

	public void DisableDie()
	{
		_DieAvailable = false;
		UIHealth.Singleton.DieLose();
	}

	public void EnableDie()
	{
		_DieAvailable = true;
		UIHealth.Singleton.DieEarn();
	}

	void OnValidate()
	{
		while (_FaceSprites.Count != _Sides)
		{
			if (_FaceSprites.Count < _Sides)
			{
				_FaceSprites.Add(null);
			}
			else
			{
				_FaceSprites.RemoveAt(_FaceSprites.Count - 1);
			}
		}
	}
}
