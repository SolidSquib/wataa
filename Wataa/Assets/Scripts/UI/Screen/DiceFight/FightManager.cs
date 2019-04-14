using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class FightManager : MonoBehaviour
{
	[System.Serializable]
	public class FightZone
	{
		public float _YPosition = 0;
		public List<GameObject> _EnemyPrefabs = new List<GameObject>();
	}

	// Gameplay settings
	[SerializeField] List<FightZone> _FightZones = new List<FightZone>();
	[SerializeField] float _FightDelay = 2.0f;

	public	Color		_ColorWin;
	public	Color		_ColorNeutral;

	// GUI elements
	public	Image[]		_AttackerDice	= new Image[5];
	public	Text		_AttackerScore;
	public	Image[]		_DefenderDice	= new Image[5];
	public	Text		_DefenderScore;

	private	Image		_Background;
	private	GameObject	_CanvasChild;

	private List<Enemy> _CurrentEnemies = new List<Enemy>();
	private int _EnemiesAwaitingMove = 0;

	public float FightDelay => _FightDelay;

	public enum EDicePattern
	{
		All,
		AllButOne,
		AllButTwo,
		HalfHalf,
		SerieMajor,
		Serie,
		None
	}
	public Dictionary<EDicePattern, GameObject> _PatternToMinigame = new Dictionary<EDicePattern, GameObject>();
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
		_PatternToMinigame.Add( EDicePattern.SerieMajor, tempPrefab);
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
		// #TODO: reimpliment this properly after next checkpoint.
		return null;

		GameObject toReturn;
		EDicePattern currentPattern = EDicePattern.None;

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
			if (sortedRoll[0] == maxDieScore)	currentPattern = EDicePattern.SerieMajor;
			else								currentPattern = EDicePattern.Serie;
		}

		// !!!!!!!!!!!!!! For test purposes, will always trigger a minigame here
		currentPattern = EDicePattern.SerieMajor;

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
		StartCoroutine(CanvasShow(false, FightDelay));
		
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
		StartCoroutine(CanvasShow(false, FightDelay));

		return attackerResults._Total > prop.ActivationValue
			? Mathf.Max(attackerResults._Total * prop.BaseDamage, 0)
			: -1;
	}

	public bool AnyEnemiesLeft()
	{
		return _CurrentEnemies.Count != 0 || _FightZones.Count != 0;
	}

	public List<Enemy> GetCurrentEnemies()
	{
		return _CurrentEnemies;
	}

	public bool ActivateFightZone()
	{
		if (AnyEnemiesLeft())
		{
			InputManager.Singleton.DisableInput();

			//Grab the next fight zone and then remove it from the list.
			FightZone Zone = _FightZones[0];
			_FightZones.RemoveAt(0);

			// Spawn the enemies.
			foreach (GameObject prefab in Zone._EnemyPrefabs)
			{				
				GameObject newEnemy = Instantiate(prefab);
				newEnemy.transform.position = Vector3.zero;
				Enemy enemyScript = newEnemy.GetComponent<Enemy>();
				Debug.Assert(enemyScript != null, "Instantiated a non-enemy type prefab in a fight zone.");

				Vector2 enemyDimensions = enemyScript.GetDimensions();
				Vector3 spawnLocation = EnemySpawnManager.Singleton.GetRandomSpawnLocation(enemyDimensions);

				newEnemy.transform.position = spawnLocation;

				Vector3 centerScreenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));
				Vector3 topScreenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight, 0));
				Vector3 bottomScreenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, 0, 0));

				Vector3 targetPosition = Vector3.zero;
				float minX = 0, maxX = 0, minY = 0, maxY = 0;

				int targetChoice = Random.Range(0, 3);
				switch (targetChoice)
				{
					case 0:
						// Stand above the player.
						minX = centerScreenPosition.x - EnemySpawnManager.Singleton.SpawnableExtents + (enemyDimensions.x / 2);
						maxX = centerScreenPosition.x - EnemySpawnManager.Singleton.SpawnableExtents - (enemyDimensions.x / 2);

						minY = centerScreenPosition.y + EnemySpawnManager.Singleton.PlayerExtents + (enemyDimensions.y / 2);
						maxY = topScreenPosition.y - (enemyDimensions.y / 2);
						break;

					case 1:
						// Stand to the left of the player.
						minX = centerScreenPosition.x - EnemySpawnManager.Singleton.SpawnableExtents + (enemyDimensions.x / 2);
						maxX = centerScreenPosition.x - EnemySpawnManager.Singleton.PlayerExtents - (enemyDimensions.x / 2);

						minY = bottomScreenPosition.y + (enemyDimensions.y / 2);
						maxY = topScreenPosition.y - (enemyDimensions.y / 2);
						break;

					case 2:
						// Stand to the right of the player.
						minX = centerScreenPosition.x + EnemySpawnManager.Singleton.SpawnableExtents - (enemyDimensions.x / 2);
						maxX = centerScreenPosition.x + EnemySpawnManager.Singleton.PlayerExtents + (enemyDimensions.x / 2);

						minY = bottomScreenPosition.y + (enemyDimensions.y / 2);
						maxY = topScreenPosition.y - (enemyDimensions.y / 2);
						break;
				}

				Coroutine coroutine = StartCoroutine(enemyScript.Move(new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0), true, EnemyActivationCompleted));
			}
		}

		return false;
	}

	private void EnemyActivationCompleted()
	{
		_EnemiesAwaitingMove -= 1;
		if (_EnemiesAwaitingMove <= 0)
		{
			InputManager.Singleton.EnableInput();
		}
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
			if (i > 0)
			{
				FightZone PreviousZone = _FightZones[i - 1];
				_FightZones[i]._YPosition = Mathf.Max(PreviousZone._YPosition + 5, _FightZones[i]._YPosition);
			}
			else
			{
				_FightZones[i]._YPosition = Mathf.Max(0, _FightZones[i]._YPosition);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		foreach (FightZone zone in _FightZones)
		{
			Vector3 startLocation = new Vector3(-1000, zone._YPosition, 0);
			Vector3 endLocation = new Vector3(1000, zone._YPosition, 0);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(startLocation, endLocation);
		}
	}

	public void RegisterEnemy(Enemy enemy)
	{
		if (!_CurrentEnemies.Contains(enemy))
		{
			_CurrentEnemies.Add(enemy);
		}		
	}

	public void UnregisterEnemy(Enemy enemy)
	{
		_CurrentEnemies.Remove(enemy);
	}
}
