using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Enemy
{
    [SerializeField]
    private GameObject _acidPrefab;
    
    protected override void PerformAttack(Vector2 finalPos)
    {
        base.PerformAttack(finalPos);
    }

    //Method called from attack animation as event trigger
    private void ThrowAcid()
    {
        if(Random.value > 0.33f)
            Instantiate(_acidPrefab, transform.position, transform.rotation);
    }
}
