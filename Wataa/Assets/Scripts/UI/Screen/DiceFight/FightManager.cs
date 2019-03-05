using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
	// Gameplay settings
	public	Color		_ColorWin;
	public	Color		_ColorNeutral;

	// GUI elements
	public	Image[]		_AttackerDice		= new Image[5];
	public	Text		_AttackerScore;
	public	Image[]		_DefenderDice	= new Image[5];
	public	Text		_DefenderScore;

	private	Image		_Background;
	private	GameObject	_CanvasChild;

	private static FightManager _DiceFightInstance;
	public static FightManager Singleton => _DiceFightInstance;

	private void Awake()
	{
		if (_DiceFightInstance == null)
		{
			_DiceFightInstance = this;
		}
		else if (_DiceFightInstance == this)
		{
			Destroy(gameObject);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		// Retrieving elements
		_Background = GetComponent<Image>();
		_CanvasChild = transform.GetChild(0).gameObject;

		// Hiding the fight GUI
		CanvasShow(false);
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void CanvasShow (bool show)
	{
		_Background.enabled = show;
		_CanvasChild.SetActive(show);

		if (!show)
		{
			_AttackerScore.color = _ColorNeutral;
			_DefenderScore.color = _ColorNeutral;
		}
	}

	private IEnumerator CanvasShow ( bool show, float delay)
	{
		yield return new WaitForSeconds(delay);
		CanvasShow(show);
	}

	private void HideAllDice()
	{
		foreach(Image dieFace in _AttackerDice)
		{
			dieFace.enabled = false;
		}

		foreach (Image dieFace in _DefenderDice)
		{
			dieFace.enabled = false;
		}
	}

	public int Fight (Character Attacker, Character Defender)
	{
		// Check if args are correct
		if (Attacker == null || Defender == null) return 0;

		// Show the fight GUI
		CanvasShow(true);

		RollResult attackerResults = Attacker.Roll();
		RollResult defenderResults = Defender.Roll();

		HideAllDice();

		for (int i = 0; i < Mathf.Max(attackerResults._Rolls.Count, defenderResults._Rolls.Count); ++i)
		{
			if (i < attackerResults._Rolls.Count)
			{
				_AttackerDice[i].enabled = true;
				_AttackerDice[i].sprite = attackerResults._FaceSprites[i][attackerResults._Rolls[i]];
			}

			if (i < defenderResults._Rolls.Count)
			{
				_DefenderDice[i].enabled = true;
				_DefenderDice[i].sprite = defenderResults._FaceSprites[i][defenderResults._Rolls[i]];
			}
		}

		_AttackerScore.text = attackerResults._Total.ToString();
		_DefenderScore.text = defenderResults._Total.ToString();

		if (attackerResults._Total >= defenderResults._Total)	_AttackerScore.color = _ColorWin;
		if (defenderResults._Total >= attackerResults._Total)	_DefenderScore.color = _ColorWin;

		// Hide the fight GUI
		StartCoroutine(CanvasShow(false, 5f));

		return attackerResults._Total - defenderResults._Total;
	}
}
