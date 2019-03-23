using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
	static EnemySpawnManager _Instance = null;

	public static EnemySpawnManager Singleton => _Instance;

	[SerializeField] float _SpawnableWidthFromCenter = 2;
	[SerializeField] float _EnemyNoStandAroundPlayer = 1;

	private BoxCollider2D[] _EntryPoints;
	private SpriteRenderer _BackgroundRenderer = null;

	public float SpawnableExtents => _SpawnableWidthFromCenter;
	public float PlayerExtents => _EnemyNoStandAroundPlayer;

	private void Awake()
	{
		if (_Instance == null)
		{
			_Instance = this;
		}
		else if (_Instance == this)
		{
			Destroy(gameObject);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		_EntryPoints = GetComponents<BoxCollider2D>();
		_BackgroundRenderer = GetComponent<SpriteRenderer>();
    }

	private BoxCollider2D GetRandomEntrance()
	{
		List<BoxCollider2D> viableEntrances = new List<BoxCollider2D>();

		// Get all viable side entrances for the current screen position
		foreach (BoxCollider2D collider in _EntryPoints)
		{
			Vector3 viewportPosition = Camera.main.WorldToViewportPoint(collider.bounds.center);
			if (viewportPosition.y > 0 && viewportPosition.y < 1)
			{
				if (viewportPosition.x > 0 || viewportPosition.x < 1)
				{
					viableEntrances.Add(collider);
				}
			}
		}

		if (viableEntrances.Count == 0) 
			return null;

		return viableEntrances[Random.Range(0, viableEntrances.Count)];
	}

	private Vector3 GetRandomSpawnLocationFromEntrance(BoxCollider2D entrance, Vector2 objectDimensions)
	{
		if (entrance != null)
		{
			Vector3 entryPosition = entrance.bounds.center;
			float xDiff =  entryPosition.x - _BackgroundRenderer.bounds.center.x;

			entryPosition.x += (xDiff > 0)
				? Mathf.Max(entrance.bounds.extents.x, objectDimensions.x / 2)
				: -(Mathf.Max(entrance.bounds.extents.x, objectDimensions.x / 2));

			return entryPosition;
		}

		return Vector3.zero;
	}

	public Vector3 GetRandomSpawnLocation(Vector2 objectDimensions)
	{
		if (Random.Range(0, 10) < 5)
		{
			BoxCollider2D entrance = GetRandomEntrance();
			if (entrance != null)
			{
				return GetRandomSpawnLocationFromEntrance(entrance, objectDimensions);
			}
		}

		Vector3 viewportTopPosition =  Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight, Camera.main.nearClipPlane));
		
		Vector3 spawnPosition = viewportTopPosition;
		spawnPosition.y += objectDimensions.y;

		float scaledSpawnableWidth = Mathf.Max(0, _SpawnableWidthFromCenter - (objectDimensions.x / 2));
		spawnPosition.x += Random.Range(-scaledSpawnableWidth, scaledSpawnableWidth);

		spawnPosition.z = 0;

		return spawnPosition;
	}

	private void OnDrawGizmosSelected()
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer)
		{
			Gizmos.color = new Color(1, 0, 0, 0.3f);
			Gizmos.DrawCube(renderer.bounds.center, new Vector3(_SpawnableWidthFromCenter * 2, renderer.bounds.size.y, 10));

			Gizmos.color = new Color(0, 1, 0, 0.3f);
			Gizmos.DrawSphere(Camera.main.transform.position, _EnemyNoStandAroundPlayer);
		}		
	}
}
