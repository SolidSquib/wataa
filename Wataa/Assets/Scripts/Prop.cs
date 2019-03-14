using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Prop : MonoBehaviour
{
	Collider2D _Collider;
	SpriteRenderer _SpriteRenderer;
	Sprite _Sprite;

	[SerializeField] int _ActivationValue = 0;
	[SerializeField] int _BaseDamage = 10;

	public Collider2D PropCollider => _Collider;
	public SpriteRenderer PropSpriteRenderer => _SpriteRenderer;
	public Sprite PropSprite => _Sprite;
	public int ActivationValue => _ActivationValue;
	public int BaseDamage => _BaseDamage;

    // Start is called before the first frame update
    void Start()
    {
		_Collider = GetComponent<Collider2D>();
		_SpriteRenderer = GetComponent<SpriteRenderer>();
		_Sprite = _SpriteRenderer.sprite;
    }

	public void Explode()
	{
		Destroy(gameObject);
	}

	public IEnumerator MoveAndAttack(Character target, int damageResult)
	{
		// first we need to move towards the target
		while (transform.position != target.transform.position)
		{
			float step = 10.0f * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
			transform.Rotate(Vector3.forward, 1000.0f * Time.deltaTime);
			yield return null;
		}

		Explode();
	}
}
