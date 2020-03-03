using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabEnabler : MonoBehaviour
{
    public Actor actor;
    public Collider triggerCollider;
    public Animator baseAnim;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void update()
    {

        Debug.Log("이거 작동하긴 하냐?2222");

        if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {

            triggerCollider.gameObject.SetActive(true);
        }
        else
        {
            triggerCollider.gameObject.SetActive(false);
        }
    }
    
}
