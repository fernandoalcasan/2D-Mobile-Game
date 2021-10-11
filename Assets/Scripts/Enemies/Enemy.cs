using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField]
    protected float health;
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float gems;

    protected virtual void Attack()
    {
        Debug.Log("Enemy class: Attack method called!");
    }

    protected abstract void Update();
}
