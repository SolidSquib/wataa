using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDropManager : MonoBehaviour
{
	private static DragDropManager _DragDropMan = null;

	// Delegates
	public delegate void OnDropped(Prop prop, Vector3 dropSite, Collider2D[] recievers);
	private event OnDropped onDroppedEvent;

	// Components
	SpriteRenderer _DragObjectRenderer;
	BoxCollider2D _DragObjectCollider;
	[SerializeField] Transform _DragObjectTransform = null;

	// Members
	Vector3 _DragOffset;
	bool _IsDragDropActive = false;
	Prop _CurrentProp = null;

	public static DragDropManager Singleton => _DragDropMan;

	private void Awake()
	{
		if (!_DragDropMan)
		{
			_DragDropMan = this;
		}
		else if (_DragDropMan != this)
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		_DragObjectRenderer = _DragObjectTransform.GetComponent<SpriteRenderer>();
		_DragObjectCollider = _DragObjectTransform.GetComponent<BoxCollider2D>();
	}

	void OnMouseMove(Vector3 mousePosition, Vector3 delta)
	{
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mousePos.z = 1;
		_DragObjectTransform.position = mousePos + _DragOffset;
	}

	public void StartDragDropOp(Prop draggedProp, Vector3 mousePos, Vector2 offset, OnDropped callback = null)
	{
		if (_DragObjectRenderer && draggedProp && !_IsDragDropActive)
		{
			_IsDragDropActive = true;

			_CurrentProp = draggedProp;

			_DragObjectRenderer.gameObject.SetActive(true);
			_DragObjectRenderer.sprite = draggedProp.PropSpriteRenderer.sprite;
			_DragObjectRenderer.color = new Color(1, 1, 1, 0.3f);
			_DragObjectRenderer.size = draggedProp.PropSpriteRenderer.size;
			_DragObjectTransform.position = mousePos + _DragOffset;
			_DragObjectTransform.localScale = draggedProp.PropSpriteRenderer.transform.localScale;

			_DragObjectCollider.size = draggedProp.PropSpriteRenderer.size;

			InputManager.Singleton.OnMouseMoveEvent += OnMouseMove;

			_DragOffset = offset;

			onDroppedEvent = callback;
		}
	}

	public void StopDragDropOp()
	{
		if (_IsDragDropActive)
		{
			if (onDroppedEvent != null)
			{
				onDroppedEvent(
					_CurrentProp, 
					_DragObjectTransform.position, 
					Physics2D.OverlapBoxAll(_DragObjectCollider.transform.position, _DragObjectCollider.size, 0));
			}

			InputManager.Singleton.OnMouseMoveEvent -= OnMouseMove;
			_DragObjectRenderer.gameObject.SetActive(false);

			_IsDragDropActive = false;
		}
	}
}
