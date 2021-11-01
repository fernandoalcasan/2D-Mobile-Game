using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acid : MonoBehaviour
{
    [SerializeField]
    private float _timeToDestroy;
    [SerializeField]
    private float _speed;

    private void Start()
    {
        Destroy(gameObject, _timeToDestroy);
    }

    private void FixedUpdate()
    {
        transform.Translate(Time.fixedDeltaTime * _speed * Vector2.right);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable hit))
        {
            hit.Damage();
            Destroy(gameObject);
        }
    }
}
