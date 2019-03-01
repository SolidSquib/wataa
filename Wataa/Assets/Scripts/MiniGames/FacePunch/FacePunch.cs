using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePunch : MonoBehaviour
{
	// Gameplay parameters
	public Texture2D punchTexture;
	public int punchSize = 256;
	public int maskResolution = 1024;
	public float blendPower = 0.2f;

	private Material faceMaterial;
	private Texture2D faceMask;
	private Texture2D punchResized;

    // Start is called before the first frame update
    void Start()
    {
		// Get material from the gameobject
		faceMaterial = GetComponent<Renderer>().material;

		// Create new texture mask
		faceMask = new Texture2D( maskResolution, maskResolution, TextureFormat.RGBA32, false);
		Color[] cols = new Color[maskResolution * maskResolution];
		for (int i=0; i<cols.Length; i++)
		{
			cols[i] = Color.white;
		}
		faceMask.SetPixels(cols);
		faceMask.Apply();

		faceMaterial.SetTexture("_Mask", faceMask);

		// Resize punch mask
		punchResized = punchTexture;
		punchResized.Resize(punchSize, punchSize);
		punchResized.Apply();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 50f))
			{
				// (-2.8f, 2.8f => 0, maskResolution)
				float hitX = hit.point[0]+2.8f;
				float hitY = hit.point[1]+2.8f;

				hitX = hitX * maskResolution / 5.6f;
				hitY = hitY * maskResolution / 5.6f;

				DrawPunch(Mathf.RoundToInt(hitX), Mathf.RoundToInt(hitY));
			}
		}
	}

	void DrawPunch ( int hitX, int hitY)
	{
		int x = hitX - Mathf.RoundToInt(punchSize / 2);
		int y = hitY - Mathf.RoundToInt(punchSize / 2);

		float currentBlend;

		Color[] cols = faceMask.GetPixels( x, y, punchSize, punchSize);
		for (int i = 0; i < cols.Length; i++)
		{
			currentBlend = cols[i].r;
			cols[i] = Color.white * Mathf.Clamp01(currentBlend - punchResized.GetPixel(i % punchSize, Mathf.FloorToInt(i / punchSize)).a * blendPower);
		}
		faceMask.SetPixels( x, y, punchSize, punchSize, cols);
		faceMask.Apply();
	}
}
