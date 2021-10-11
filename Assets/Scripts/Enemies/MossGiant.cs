using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MossGiant : Enemy
{
    private void Start()
    {
        Attack();
    }

    protected override void Attack()
    {
        base.Attack();
        Debug.Log("Moss Giant class: Attack method called!");
    }

    protected override void Update()
    {
        //implement Update functionality here
    }

}
