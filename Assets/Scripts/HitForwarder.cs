using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitForwarder : MonoBehaviour
{
    //1
    public Actor actor;
    public Collider triggerCollider;

    //2
    private void OnTriggerEnter(Collider hitCollider)
    {
        /*
                Debug.Log("----> HitForwarder.cs / START / Attacker:"+this.gameObject.name + " -> HITTED:" + actor.gameObject.name);
                Debug.Log("      HitForwarder.cs // HIT COLLIDER NAME!  :  " + hitCollider.name);
        */

        Vector3 direction = new Vector3(hitCollider.transform.position.x - actor.transform.position.x, 0, 0);
        direction.Normalize();

        BoxCollider collider = triggerCollider as BoxCollider;
        Vector3 centerPoint = this.transform.position;

        if (collider)
        {
            //Debug.Log("  HitForwarder.cs  /// 근데 여기까지는 옴? -- > hitCollider.gameObject.name  :  " + hitCollider.gameObject.name);
            centerPoint = transform.TransformDirection(collider.center);
        }

        Vector3 startPoint = hitCollider.ClosestPointOnBounds(centerPoint);
        //actor.lastAttackTime = 0;
        actor.DidHitObject(hitCollider, startPoint, direction);


    }
}
