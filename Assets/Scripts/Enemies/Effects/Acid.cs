using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acid : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float _timeToDestroy;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _attackPower;

    public float Health { get; set; }

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
            hit.Damage(transform.position, _attackPower);
            Destroy(gameObject);
        }
    }

    public void Damage(Vector2 attackPos, float damage)
    {
        //Play acid destruction audio

        Destroy(gameObject);
    }
}
