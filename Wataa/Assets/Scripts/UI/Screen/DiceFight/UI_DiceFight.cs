using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DiceFight : MonoBehaviour
{
	// Gameplay settings
	public	Sprite[]	dieFaces		= new Sprite[6];
	public	Color		colorWin;
	public	Color		colorNeutral;

	// GUI elements
	public	Image[]		playerDice		= new Image[5];
	public	Text		playerScore;
	public	Image[]		opponentDice	= new Image[5];
	public	Text		opponentScore;

	private	Image		background;
	private	GameObject	canvasChild;

	// Start is called before the first frame update
	void Start()
	{
		// Retrieving elements
		background = GetComponent<Image>();
		canvasChild = transform.GetChild(0).gameObject;

		// Hiding the fight GUI
		CanvasShow(false);
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void CanvasShow ( bool show)
	{
		background.enabled = show;
		canvasChild.SetActive(show);

		if (!show)
		{
			playerScore.color   = colorNeutral;
			opponentScore.color = colorNeutral;
		}
	}

	private IEnumerator CanvasShow ( bool show, float delay)
	{
		yield return new WaitForSeconds(delay);
		CanvasShow(show);
	}

	public bool ShowRoll ( int[] rollPlayer, int[] rollOpponent)
	{
		// Check if args are correct
		if (rollPlayer	 == null || rollPlayer.Length	!= 5) return false;
		if (rollOpponent == null || rollOpponent.Length != 5) return false;
		for (int i=0; i<5; i++)
		{
			if(rollPlayer[i]   < 1 || rollPlayer[i]	  > 6)	  return false;
			if(rollOpponent[i] < 1 || rollOpponent[i] > 6)	  return false;
		}

		// Show the fight GUI
		CanvasShow(true);

		int totalPlayer   = 0;
		int totalOpponent = 0;

		for (int i=0; i<5; i++)
		{
			totalPlayer += rollPlayer[i];
			playerDice[i].sprite = dieFaces[rollPlayer[i]-1];

			totalOpponent += rollOpponent[i];
			opponentDice[i].sprite = dieFaces[rollOpponent[i]-1];
		}

		playerScore.text   = totalPlayer.ToString();
		opponentScore.text = totalOpponent.ToString();

		if (totalPlayer >= totalOpponent)	playerScore.color   = colorWin;
		if (totalOpponent >= totalPlayer)	opponentScore.color = colorWin;

		// Hide the fight GUI
		StartCoroutine(CanvasShow( false, 5f));

		return true;
	}
}
