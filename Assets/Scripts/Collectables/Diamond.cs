using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Diamond : MonoBehaviour
{
    public static Action OnDiamondCollected;

    [SerializeField]
    private bool _isReward;
    [SerializeField]
    private float _force;
    [SerializeField]
    private float _timeToBeEnabled;

    private Rigidbody2D _rb;
    private bool _canBeLooted;
    private WaitForSeconds _wait;

    private void Start()
    {
        if (_isReward)
        {
            _wait = new WaitForSeconds(_timeToBeEnabled);
            StartCoroutine(EnableCollectionAfter());

            _rb = GetComponent<Rigidbody2D>();
            _rb.AddForce(new Vector2(UnityEngine.Random.Range(-1f, 1f), 1) * _force, ForceMode2D.Impulse);
        }
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

            Destroy(gameObject);
        }
    }
}
