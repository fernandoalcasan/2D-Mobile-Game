/*
 * This script contains the IDamageable interface, which
 * implements the health and damage system of the game
 */

using UnityEngine;

public interface IDamageable
{
    float Health { get; set; }
    void Damage(Vector2 attackPos, float damage);
}
