/*
 * This script makes sure to limit the zones where enemies can attack the player
 * It lists all the enemies in a zone and caches the player reference in order to 
 * let the enemies know where's the player and give the order to attack it.
 */


using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackZone : MonoBehaviour
{
    [SerializeField]
    private List<Enemy> _enemiesInZone;

    //Help variables, to cache the references
    private Transform _player;

    //Player enters the attack zone and this method notifies the remaining enemies
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if (_player is null)
                _player = other.transform;

            for (int i = _enemiesInZone.Count - 1; i >= 0; i--)
            {
                if (_enemiesInZone[i])
                    _enemiesInZone[i].IdentifyPlayer(_player);
                else
                    _enemiesInZone.RemoveAt(i);
            }

            if (_enemiesInZone.Count == 0)
                Destroy(gameObject);
        }
    }

    //Player leaves the attack zone and this method notifies the remaining enemies
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            for (int j = _enemiesInZone.Count - 1; j >= 0; j--)
            {
                if (_enemiesInZone[j])
                    _enemiesInZone[j].ReturnToPatrol();
                else
                    _enemiesInZone.RemoveAt(j);
            }

            if (_enemiesInZone.Count == 0)
                Destroy(gameObject);
        }
        //To don't allow enemies to get out of the attack zone
        else if(other.CompareTag("Enemy"))
        {
            if(other.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(other.transform.position, 10f);
            }
        }
    }
}
