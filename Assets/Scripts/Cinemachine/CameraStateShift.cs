using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraStateShift : MonoBehaviour
{
    public static Action<int> OnCameraShift;
    
    [SerializeField]
    private string _nextCameraState;

    [SerializeField]
    private bool _isDependant;

    [Header("Objectives if Dependant")]
    [SerializeField]
    private string _finalCameraState;
    [SerializeField]
    private GameObject _bounds;
    [SerializeField]
    private List<GameObject> _objectives;
    [SerializeField]
    private float _checkForObjsEvery;

    private int _NCSHash;
    private int _FCSHash;
    private BoxCollider2D _collider;
    private WaitForSeconds _waitToCheck;

    private void Start()
    {
        _NCSHash = Animator.StringToHash(_nextCameraState);

        if(_isDependant)
        {
            _FCSHash = Animator.StringToHash(_finalCameraState);
            _collider = GetComponent<BoxCollider2D>();
            _waitToCheck = new WaitForSeconds(_checkForObjsEvery);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if (!(OnCameraShift is null))
                OnCameraShift(_NCSHash);
            
            if(_isDependant)
            {
                _collider.enabled = false;

                if(_bounds)
                    _bounds.SetActive(true);
                
                StartCoroutine(CheckObjectives());
            }
        }
    }

    private IEnumerator CheckObjectives()
    {
        yield return _waitToCheck;

        for (int i = _objectives.Count - 1; i >= 0; i--)
        {
            if (!_objectives[i])
                _objectives.RemoveAt(i);
        }

        if (_objectives.Count == 0)
            ObjectiveCompleted();
        else
            StartCoroutine(CheckObjectives());
    }

    private void ObjectiveCompleted()
    {
        if (!(OnCameraShift is null))
            OnCameraShift(_FCSHash);

        if (_bounds)
            _bounds.SetActive(false);

        Destroy(gameObject);
    }

}
