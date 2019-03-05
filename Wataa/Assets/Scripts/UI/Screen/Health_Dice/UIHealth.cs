using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
	// Gameplay settings
	public	int		healthMax	= 50;
	public	int		diceMax		= 5;
	public	Sprite	dieOn;
	public	Sprite	dieOff;

	// GUI elements
	public	Text	UI_healthText;
	public	Image	UI_healthImage;
	public	Image[]	UI_diceImage 	= new Image[5];

	private	bool[]	dice		= new bool[]{ true, true, true, true, true};
	private int		diceQtt		= 5;

	private static UIHealth _UIHealthInstance;
	public static UIHealth Singleton => _UIHealthInstance;

	private void Awake()
	{
		if (_UIHealthInstance == null)
		{
			_UIHealthInstance = this;
		}
		else if (_UIHealthInstance != this)
		{
			Destroy(gameObject);
		}
	}

	void SwitchDice (int diceIndex)
	{
		if (diceIndex >= 0 && diceIndex < dice.Length) {
			dice[diceIndex] = !dice[diceIndex];
			UI_diceImage[diceIndex].sprite = dice[diceIndex]?dieOn:dieOff;
		}
	}

	public void DieLose ()
	{
		if (diceQtt > 0)
		{
			SwitchDice(--diceQtt);
		}
	}

	public void DieEarn ()
	{
		if (diceQtt < diceMax)
		{
			SwitchDice(diceQtt++);
		}
	}

	public void DiceFill ()
	{
		while (diceQtt < diceMax)
		{
			DieEarn();
		}
	}

	public void UpdateHealth (int healthNew)
	{
		int healthCurrent = Mathf.RoundToInt(Mathf.Clamp( healthNew, 0.0f, healthMax));

		UI_healthText.text = healthCurrent.ToString();
		UI_healthImage.fillAmount = Mathf.Clamp01((float)healthCurrent/(float)healthMax);
	}
}
