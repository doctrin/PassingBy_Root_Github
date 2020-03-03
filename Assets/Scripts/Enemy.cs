using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Actor
{
    public EnemyAI ai;
    public static int TotalEnemies;
    public Walker walker;

    public bool stopMovementWhenHit = true;

    public void RegisterEnemy()
    {
        TotalEnemies++;
    }

    protected override void Die()
    {
        base.Die();
        ai.enabled = false;
        walker.enabled = false;
        TotalEnemies--;
    }

    public void MoveTo (Vector3 targetPosition)
    {
        walker.MoveTo(targetPosition);
    }

    public void MoveToOffset (Vector3 targetPosition, Vector3 offset)
    {
        if (!walker.MoveTo(targetPosition + offset))
        {
            walker.MoveTo(targetPosition - offset);
        }
    }

    public void Wait()
    {
        walker.StopMovement();
    }

    public override void TakeDamage(float value, Vector3 hitVector, bool knockdown = false)
    {
        if (stopMovementWhenHit)
        {
            walker.StopMovement();
        }

        base.TakeDamage(value, hitVector, knockdown);
    }

    public override bool IsCanWalk()
    {
        return !baseAnim.GetCurrentAnimatorStateInfo(0).IsName("hurt") 
               && !baseAnim.GetCurrentAnimatorStateInfo(0).IsName("getup");
    }

}
