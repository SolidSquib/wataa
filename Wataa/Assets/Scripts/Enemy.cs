using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
	private PlayerCharacter _PlayerRef = null;
	
	void OnEnable()
	{
		_PlayerRef = FindObjectOfType<PlayerCharacter>();
	}
}
