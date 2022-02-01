/*
 * This script contains the behavior of areas that deal damage to 
 * damageable gameobjects (IDamageable interface) 
 * Some examples of this areas are the spikes and the water below the bridge
 */

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageArea : MonoBehaviour
{
    [SerializeField]
    private float _damageDealt;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.Damage(other.transform.position, _damageDealt);
        }
    }
}
