/*
 * This script contains the behaviors of the diamonds at the level and the ones that enemies drop
 * Once collected an event (C# Action) gets executed to communicate with other scripts about it
 */

using System.Collections;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Diamond : MonoBehaviour
{
    public static Action OnDiamondCollected;

    [Header("Diamond Behavior")]
    [SerializeField]
    private bool _isReward;

    [Header("Rewarded Options")]
    [SerializeField]
    private float _force;
    [SerializeField]
    private float _timeToBeEnabled;
    [SerializeField]
    private float _timeToBeLooted;

    //Help variables, to cache references and behavior
    [SerializeField]
    private SFX _collectSFX;
    private Rigidbody2D _rb;
    private bool _canBeLooted;
    private WaitForSeconds _wait;

    private void Start()
    {
        //Diamonds rewarded by enemies
        if (_isReward)
        {
            _wait = new WaitForSeconds(_timeToBeEnabled);
            StartCoroutine(EnableCollectionAfter());

            _rb = GetComponent<Rigidbody2D>();
            _rb.AddForce(new Vector2(UnityEngine.Random.Range(-1f, 1f), 1) * _force, ForceMode2D.Impulse);

            Destroy(gameObject, _timeToBeLooted);
        }
        //Diamonds existing in the level
        else
            _canBeLooted = true;
    }

    private IEnumerator EnableCollectionAfter()
    {
        yield return _wait;
        _canBeLooted = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_canBeLooted)
            return;

        if(other.CompareTag("Player"))
        {
            if (!(OnDiamondCollected is null))
                OnDiamondCollected();
            AudioManager.Instance.PlayOneShotSFX(_collectSFX.sound, _collectSFX.volume);
            Destroy(gameObject);
        }
    }
}
