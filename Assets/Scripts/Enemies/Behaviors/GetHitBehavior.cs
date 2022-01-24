using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetHitBehavior : StateMachineBehaviour
{
    private Enemy _enemy;

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemy is null)
        {
            _enemy = animator.GetComponent<Enemy>();
        }

        _enemy.StopHitFeedback();
    }
}
