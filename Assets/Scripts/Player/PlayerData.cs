using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable][CreateAssetMenu(fileName = "New Player Data", menuName = "Data/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Player Properties")]
    public float maxHealth;
    public float health;

    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _speedUpgraded;

    public float Speed 
    { 
        get {
            if (gotWindBoots)
                return _speedUpgraded;
            else
                return _speed;
        }
    }

    [SerializeField]
    private float _attackPower;
    [SerializeField]
    private float _attackPowerUpgraded;

    public float AttackPower
    {
        get
        {
            if (gotAttackUpgrade)
                return _attackPowerUpgraded;
            else
                return _attackPower;
        }
    }


    public float jumpPower;

    [Range(1f, 5f)]
    public float jumpFallGravityMultiplier;
    public float knockbackForce;

    [Header("Inventory Properties")]

    public int diamonds;

    [Header("Upgrades")]
    public bool gotCastleKey;
    public bool gotWindBoots;
    public bool gotAttackUpgrade;
    public AnimatorOverrideController fireOverride;
}
