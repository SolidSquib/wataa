using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class FightManager : MonoBehaviour
{
	struct FightZone
	{
		public float _YPosition;
		public List<GameObject> _EnemyPrefabs;
	}

	// Gameplay settings
	[SerializeField] List<FightZone> _FightZones = new List<FightZone>();

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

	public int Fight (Character attacker, Character defender)
	{
		// Check if args are correct
		if (attacker == null || defender == null) return 0;

		// Show the fight GUI
		CanvasShow(true);

		bool bPlayerAttacking = attacker is PlayerCharacter;

		RollResult attackerResults = attacker.Roll();
		RollResult defenderResults = defender.Roll();

		HideAllDice();

		RollResult leftResults = bPlayerAttacking ? attackerResults : defenderResults;
		RollResult rightResults = bPlayerAttacking ? defenderResults : attackerResults;

		// Player results are always on the left.
		GameObject MinigameToInstanciate = DicePatternCheck(leftResults);
		if (MinigameToInstanciate != null) Instantiate<GameObject>(MinigameToInstanciate);

		for (int i = 0; i < Mathf.Max(leftResults._Rolls.Count, rightResults._Rolls.Count); ++i)
		{
			if (i < leftResults._Rolls.Count)
			{
				_AttackerDice[i].enabled = true;
				_AttackerDice[i].sprite = leftResults._FaceSprites[i][leftResults._Rolls[i]-1];
			}

			if (i < rightResults._Rolls.Count)
			{
				_DefenderDice[i].enabled = true;
				_DefenderDice[i].sprite = rightResults._FaceSprites[i][rightResults._Rolls[i]-1];
			}
		}

		_AttackerScore.text = leftResults._Total.ToString();
		_DefenderScore.text = rightResults._Total.ToString();

		if (leftResults._Total >= rightResults._Total)	_AttackerScore.color = _ColorWin;
		if (rightResults._Total >= leftResults._Total)	_DefenderScore.color = _ColorWin;

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
				_AttackerDice[i].sprite = attackerResults._FaceSprites[i][attackerResults._Rolls[i]-1];
			}
		}

		_AttackerScore.text = attackerResults._Total.ToString();
		_DefenderScore.text = prop.ActivationValue.ToString();

		if (attackerResults._Total >= prop.ActivationValue) _AttackerScore.color = _ColorWin;
		if (prop.ActivationValue >= attackerResults._Total) _DefenderScore.color = _ColorWin;

		// Hide the fight GUI
		StartCoroutine(CanvasShow(false, _GUIDelay));

		return Mathf.Max(/*(*/attackerResults._Total/* - prop.ActivationValue)*/ * prop.BaseDamage, 0);
	}

	public bool AnyEnemiesLeft()
	{
		return _FightZones.Count != 0;
	}

	public void ActivateFightZone()
	{

	}

	public bool GetNextFightZoneLocation(PlayerCharacter player, ref Vector3 OutLocation)
	{
		if (AnyEnemiesLeft())
		{
			OutLocation.x = player.transform.position.x;
			OutLocation.y = _FightZones[0]._YPosition;
			OutLocation.z = player.transform.position.z;
			return true;
		}

		return false;
	}

	public float GetDistanceFromNextFightZone(PlayerCharacter player)
	{
		if (_FightZones.Count > 0)
		{
			Vector3 playerLocation = player.transform.position;
			float distance = _FightZones[0]._YPosition - playerLocation.y;
			return distance;
		}

		return 0;
	}

	private void OnValidate()
	{
		for (int i = 0; i < _FightZones.Count; ++i)
		{
			if (i < 0)
			{
				FightZone PreviousZone = _FightZones[i - 1];
				FightZone CurrentZone = _FightZones[i];
				CurrentZone._YPosition = Mathf.Max(PreviousZone._YPosition + 5, CurrentZone._YPosition);
			}
		}
	}
}
