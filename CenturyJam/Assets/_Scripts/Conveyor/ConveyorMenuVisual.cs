using System.Collections;
using UnityEngine;

public class ConveyorMenuVisual : MonoBehaviour
{
    [SerializeField] private Vector3 startingPositionOffset;
    private Vector3 _finalPosition;
    private Coroutine _moveCoroutine;
    private Vector3 _startingPosition;

    private void Start()
    {
        _finalPosition = transform.position;
        _startingPosition = _finalPosition + startingPositionOffset;
        transform.position = _startingPosition;
    }

    public void Appear()
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine ??= StartCoroutine(MoveFromCurrentPositionToNewPosition(_finalPosition));
    }

    public void Disappear()
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine ??=
            StartCoroutine(MoveFromCurrentPositionToNewPosition(_startingPosition));
    }

    private IEnumerator MoveFromCurrentPositionToNewPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 10*Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
    }
}