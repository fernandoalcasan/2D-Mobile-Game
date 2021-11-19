using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable][CreateAssetMenu(fileName = "New Player Data", menuName = "Data/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Player Properties")]
    public float maxHealth;
    public float health;
    public float speed;
    public float jumpPower;
    [Range(1f, 5f)]
    public float jumpFallGravityMultiplier;
    public float attackPower;
    public float knockbackForce;

    [Header("Inventory Properties")]

    public int diamonds;
    public bool gotCastleKey;
}
