using System.Collections;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
	[SerializeField] Transform _Target = null;
	[SerializeField] float _FollowSpeed = 0.125f;
	[SerializeField] Vector3 _Offset = new Vector3(0, 0, -10);

	private Vector3 _Velocity = Vector3.zero;
	private bool _Follow = false;
	private Vector3 _TargetOffset = Vector3.zero;
	private Vector3 _TargetPosition = Vector3.zero;
	private Vector3 _CameraDeltaMovement = Vector3.zero;

	public Transform Following => _Target;

	private void Start()
	{
		if (_Target)
		{
			_TargetPosition = _Target.position + _Offset;
		}		
	}

	// Start is called before the first frame update
	public void StartFollowing()
    {
		_Follow = true;
    }

	public void StopFollowing()
	{
		_Follow = false;
	}

    // Update is called once per frame
    void LateUpdate()
    {
		if (_Follow)
		{
			_TargetPosition = _Target.position + _Offset;
		}

		Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, _TargetPosition, ref _Velocity, _FollowSpeed);

		_CameraDeltaMovement = transform.position - smoothedPosition;
		transform.position = smoothedPosition;
	}

	public IEnumerator WaitForCameraStop()
	{
		while (_CameraDeltaMovement != Vector3.zero)
		{
			yield return null;
		}
	}
}
