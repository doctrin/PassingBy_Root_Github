using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabForwarder : MonoBehaviour
{

    public Actor actor;
    public Collider triggerCollider;
    public Animator baseAnim;

    private void OnTriggerEnter(Collider enemyGrabCollider)
    {

        //Debug.Log("----> GrabForwarder.cs / START / Attacker:" + this.gameObject.name + " -> Grabbed:" + actor.gameObject.name);
        //Debug.Log("      GrabForwarder.cs // Grab COLLIDER NAME!  :  " + hitCollider.name);

        //잡기를 할 수 있는 경우는, '걷기' 일 때만으로 한정한다. 
        if (!baseAnim.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {
            return;
        }

        Vector3 direction = new Vector3(enemyGrabCollider.transform.position.x - actor.transform.position.x, 0, 0);
        direction.Normalize();

        BoxCollider collider = triggerCollider as BoxCollider;
        Vector3 centerPoint = this.transform.position;
        
        if (collider)
        {
            centerPoint = transform.TransformDirection(collider.center);
            Vector3 startPoint = enemyGrabCollider.ClosestPointOnBounds(centerPoint);
        
            //잡은 적의 Actor 객체를 grabbedActor에 지정.
            actor.grabbedActor = enemyGrabCollider.GetComponent<Actor>();
            
            //잡은 적의 기타 포지션 정보 Actor 객체에 저장
            actor.grabAttackStartPoint = startPoint;
            actor.grabAttackDirection = direction;

            //잡은 적의 후속처리
            actor.DidGrabObject(enemyGrabCollider, startPoint, direction);

            
            
        }

        

        //잡기 공격 cnt 초기화
        //Hero heroCs = gameObject.GetComponentInParent(typeof(Hero)) as Hero;
        //heroCs.currentGrabAttackChain = 0;



    }
}
