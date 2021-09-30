using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    private int _attackArcHash;

    private Animator _animator;

    [SerializeField]
    private SpriteRenderer _arcSprite;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator is null)
            Debug.Log("Animator is NULL!");

        _attackArcHash = Animator.StringToHash("DisplayArc");
    }

    public void DisplayArc(bool leftSide)
    {
        if (leftSide)
            _arcSprite.flipX = true;
        else
            _arcSprite.flipX = false;

        _animator.SetTrigger(_attackArcHash);
    }
}
