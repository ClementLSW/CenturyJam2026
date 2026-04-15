using System.Collections;
using UnityEngine;

public class ConveyorMenuVisual : MonoBehaviour
{
    //Position Reference
    //32  23
    //01  10
    
    //01  10
    //32  23
    [SerializeField] private Vector3 pos0, pos1, pos2, pos3;
    private Vector3 _finalPosition;
    private Coroutine _moveCoroutine;
    private Vector3 _startingPosition;
    [HideInInspector] public bool isConveyorShown;

    private void Start()
    {
        transform.position = pos0;
    }

    public void AnimatePosition(int startPosition, int finalPosition)
    {
        switch (startPosition)
        {
            case 0:
                transform.position = pos0;
                break;
            case 1:
                transform.position = pos1;
                break;
            case 2:
                transform.position = pos2;
                break;
            case 3:
                transform.position = pos3;
                break;
        }
        switch (finalPosition)
        {
            case 0:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(pos0));
                break;
            case 1:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(pos1));
                break;
            case 2:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(pos2));
                break;
            case 3:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(pos3));
                break;
        }
    }

    public void AnimatePosition(int startPosition, int intermediatePosition, int finalPosition)
    {
        switch (startPosition)
        {
            case 0:
                transform.position = pos0;
                break;
            case 1:
                transform.position = pos1;
                break;
            case 2:
                transform.position = pos2;
                break;
            case 3:
                transform.position = pos3;
                break;
        }

        Vector3 lIntermediate = new();
        switch (intermediatePosition)
        {
            case 0:
                lIntermediate = pos0;
                break;
            case 1:
                lIntermediate = pos1;
                break;
            case 2:
                lIntermediate = pos2;
                break;
            case 3:
                lIntermediate = pos3;
                break;
        }
        switch (finalPosition)
        {
            case 0:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(lIntermediate,pos0));
                break;
            case 1:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(lIntermediate,pos1));
                break;
            case 2:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(lIntermediate,pos2));
                break;
            case 3:
                if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
                _moveCoroutine ??=
                    StartCoroutine(AnimatePosition(lIntermediate,pos3));
                break;
        }
    }

    IEnumerator AnimatePosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 10*Time.deltaTime);
            yield return null;
        }

        _moveCoroutine = null;
        transform.position = targetPosition;
    }
    
    IEnumerator AnimatePosition(Vector3 intermediateTargetPosition, Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, intermediateTargetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, intermediateTargetPosition, 10*Time.deltaTime);
            yield return null;
        }
        transform.position = intermediateTargetPosition;
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 10*Time.deltaTime);
            yield return null;
        }

        _moveCoroutine = null;
        transform.position = targetPosition;
    }
    
}