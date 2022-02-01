/*
 * This script makes sure to handle the FX from the player mechanics.
 */

using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    //Help variables, to cache the animator references
    private int _attackArcHash;
    private int _spawnLightHash;
    private Animator _animator;

    [Header("FX References")]
    [SerializeField]
    private SpriteRenderer _arcSprite;
    [SerializeField]
    private AnimatorOverrideController _animOverride;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator is null)
            Debug.Log("Animator is NULL!");

        _attackArcHash = Animator.StringToHash("DisplayArc");
        _spawnLightHash = Animator.StringToHash("Spawn");
    }

    public void DisplayArc(bool leftSide)
    {
        if (leftSide)
            _arcSprite.flipX = true;
        else
            _arcSprite.flipX = false;

        _animator.SetTrigger(_attackArcHash);
    }

    public void DisplaySpawnEffect()
    {
        _animator.SetTrigger(_spawnLightHash);
    }

    public void UpgradeEffects()
    {
        _animator.runtimeAnimatorController = _animOverride;
    }
}
