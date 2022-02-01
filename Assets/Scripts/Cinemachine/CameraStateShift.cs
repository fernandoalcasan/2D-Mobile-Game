/*
 * This script makes sure to trigger one or two camera transitions by executing the respective event (C# Action)
 * It works by attaching it to a 2D collider that will trigger the 1st transition when the player enters
 * The 2nd transition is optional and can depend on existing gameobjects or on certain time
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraStateShift : MonoBehaviour
{
    public static Action<int> OnCameraShift;
    
    //Name of the animation state for 1st transition of the state driven camera (Cinemachine)
    [SerializeField]
    private string _nextCameraState;

    //To set the camera shift dependant on time or on gameobjects (enemies) that will be destroyed
    [SerializeField]
    private bool _isDependant;

    [Header("Objectives if Dependant")]

    //Name of the animation state for 2nd transition
    [SerializeField]
    private string _finalCameraState;

    //Bounds (gameobject) to enable when the 1st transition happens (optional)
    [SerializeField]
    private GameObject _bounds;

    //Existing gameobjects (to check if they're destroyed) before the 2nd transition (optional)
    [SerializeField]
    private List<GameObject> _objectives;
    
    //Time to check if gameobjects are destroyed (loop) or before 2nd transition (if objectives are empty)
    [SerializeField]
    private float _checkForObjsEvery;

    [Header("Stop Player Options")]

    //If enabled, the Game Events to stop the player movement will be raised (Event bus pattern)
    [SerializeField]
    private bool _stopsPlayer;
    [SerializeField]
    private GameEvent _stopPlayerEvent;
    [SerializeField]
    private GameEvent _restorePlayerEvent;

    //Help variables, to cache references
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

            if (_stopsPlayer)
                _stopPlayerEvent.Raise();
        }
    }

    //This coroutine goes in loop while there are objectives or time remaining
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

    //2nd camera transition
    private void ObjectiveCompleted()
    {
        if (!(OnCameraShift is null))
            OnCameraShift(_FCSHash);

        if(_stopsPlayer)
            _restorePlayerEvent.Raise();

        if (_bounds)
            _bounds.SetActive(false);

        Destroy(gameObject);
    }

}
