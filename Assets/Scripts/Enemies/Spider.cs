using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Enemy
{
    private void Start()
    {
        Attack();
    }

    protected override void Attack()
    {
        base.Attack();
        Debug.Log("Spider class: Attack method called!");
    }

    protected override void Update()
    {

    }
}
