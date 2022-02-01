/*
 * This script makes sure to handle the enemy behavior when
 * the attack animation state ends
 */

using UnityEngine;

public class AttackBehavior : StateMachineBehaviour
{
    private Enemy _enemy;

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_enemy is null)
        {
            _enemy = animator.GetComponent<Enemy>();
        }

        _enemy.StopAttacking();
    }
}
