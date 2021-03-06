/*
 * Class meant to override virtual general behavior of the spider enemy in case its necessary
 */

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
        Instantiate(_acidPrefab, transform.position, transform.rotation);
    }
}
