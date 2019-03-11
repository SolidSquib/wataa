using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePunch : MonoBehaviour
{
	// Gameplay parameters
	public Texture2D[] punchTexture;
	public int maskResolution = 1024;
	public float blendPower = 0.2f;

	public ParticleSystem bloodDrops;

	private int punchSize;
	private Material faceMaterial;
	private Texture2D faceMask;

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

				bloodDrops.transform.position = hit.point;
				bloodDrops.Play();
			}
		}
	}

	void DrawPunch ( int hitX, int hitY)
	{
		Texture2D tempPunch = punchTexture[Random.Range( 0, punchTexture.Length)];
		punchSize = tempPunch.width;

		// Compute rect origin
		int x = hitX - Mathf.RoundToInt(punchSize / 2);
		int y = hitY - Mathf.RoundToInt(punchSize / 2);

		// Fixing origin and computing new rect size if origin is located negatively
		int deltaX = Mathf.Abs(x);
		int deltaY = Mathf.Abs(y);
		int width  = punchSize - ((x < 0) ? deltaX : 0);
		int height = punchSize - ((y < 0) ? deltaY : 0);
		x = Mathf.Max(x, 0);
		y = Mathf.Max(y, 0);

		// Fixing size in case the rect goes out of the texture
		if ((x + width)  >= maskResolution) width  = maskResolution - 1 - x;
		if ((y + height) >= maskResolution) height = maskResolution - 1 - y;

		float currentBlend;

		Color[] cols = faceMask.GetPixels( x, y, width, height);
		for (int i = 0; i < cols.Length; i++)
		{
			currentBlend = cols[i].r;
			cols[i] = Color.white * Mathf.Clamp01(currentBlend - tempPunch.GetPixel(i % width, Mathf.FloorToInt(i / width)).r * blendPower);
		}
		faceMask.SetPixels( x, y, width, height, cols);
		faceMask.Apply();
	}
}
