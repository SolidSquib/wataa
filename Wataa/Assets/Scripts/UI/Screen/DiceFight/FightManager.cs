using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
	// Gameplay settings
	public	Color		_ColorWin;
	public	Color		_ColorNeutral;
	public	float		_GUIDelay = 1f;

	// GUI elements
	public	Image[]		_AttackerDice	= new Image[5];
	public	Text		_AttackerScore;
	public	Image[]		_DefenderDice	= new Image[5];
	public	Text		_DefenderScore;

	private	Image		_Background;
	private	GameObject	_CanvasChild;

	public enum _DicePattern
	{
		All,
		AllButOne,
		AllButTwo,
		HalfHalf,
		SerieMajor,
		Serie,
		None
	}
	public Dictionary<_DicePattern, GameObject> _PatternToMinigame = new Dictionary<_DicePattern, GameObject>();
	public GameObject tempPrefab;

	private static FightManager _DiceFightInstance;
	public static FightManager Singleton => _DiceFightInstance;

	private void Awake()
	{
		if (_DiceFightInstance == null)
		{
			_DiceFightInstance = this;
		}
		else if (_DiceFightInstance != this)
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

		// Matching dice patterns to minigames
		_PatternToMinigame.Add( _DicePattern.SerieMajor, tempPrefab);
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

	private GameObject DicePatternCheck (RollResult RollToCheck)
	{
		GameObject toReturn;
		_DicePattern currentPattern = _DicePattern.None;

		// BIG WARNING! Using temporary variables here
		int maxDieScore = 5;

		// Analysing the current roll
		List<int> sortedRoll = RollToCheck._Rolls;
		sortedRoll.Sort((a ,b) => b.CompareTo(a));  // Sorting in descending order

		// Checking if the roll is a serie
		bool isSerie = true;
		int listSize = sortedRoll.Count;
		for (int i = 0; i < listSize-2; i++)
		{
			if (sortedRoll[i] - sortedRoll[i+1] != 1)
			{
				isSerie = false;
				break;
			}
		}
		if (isSerie)
		{
			if (sortedRoll[0] == maxDieScore)	currentPattern = _DicePattern.SerieMajor;
			else								currentPattern = _DicePattern.Serie;
		}

		// !!!!!!!!!!!!!! For test purposes, will always trigger a minigame here
		currentPattern = _DicePattern.SerieMajor;

		_PatternToMinigame.TryGetValue(currentPattern, out toReturn);
		return toReturn;
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

		// BIG WARNING! I need player vs enemy, not att vs defender. So I can send the player roll there!
		GameObject MinigameToInstanciate = DicePatternCheck(attackerResults);
		if (MinigameToInstanciate != null) Instantiate<GameObject>(MinigameToInstanciate);

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
		StartCoroutine(CanvasShow(false, _GUIDelay));
		
		return attackerResults._Total - defenderResults._Total;
	}

	public int FightWithProp(Character attacker, Character defender, Prop prop)
	{
		// Check if args are correct
		if (attacker == null || defender == null || prop == null) return 0;

		// Show the fight GUI
		CanvasShow(true);

		// Roll a single die for the roll off with the prop.
		RollResult attackerResults = attacker.Roll(1);

		HideAllDice();

		for (int i = 0; i < attackerResults._Rolls.Count; ++i)
		{
			if (i < attackerResults._Rolls.Count)
			{
				_AttackerDice[i].enabled = true;
				_AttackerDice[i].sprite = attackerResults._FaceSprites[i][attackerResults._Rolls[i]];
			}
		}

		_AttackerScore.text = attackerResults._Total.ToString();
		_DefenderScore.text = prop.ActivationValue.ToString();

		if (attackerResults._Total >= prop.ActivationValue) _AttackerScore.color = _ColorWin;
		if (prop.ActivationValue >= attackerResults._Total) _DefenderScore.color = _ColorWin;

		// Hide the fight GUI
		StartCoroutine(CanvasShow(false, _GUIDelay));

		return Mathf.Max((attackerResults._Total - prop.ActivationValue) * prop.BaseDamage, 0);
	}
}
