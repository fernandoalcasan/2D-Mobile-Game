using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMovementBehavior : StateMachineBehaviour
{
    private Player _player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player is null)
            _player = animator.GetComponent<Player>();

        _player.HandleControls(false);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player is null)
            Debug.LogError("Player is NULL!");

        _player.HandleControls(true);
    }
}
