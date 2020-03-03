using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {

    float horizontal;
    float vertical;
    bool jump;
    bool attack;
    bool suplex;

    float lastJumpTime;
    bool isJumping;
    public float maxJumpDuration = 0.2f;

    //1
    public float GetVerticalAxis()
    {
        return vertical;
    }

    public float GetHorizontalAxis()
    {
        return horizontal;
    }

    public bool GetJumpButtonDown()
    {
        return jump;
    }

    public bool GetAttackButtonDown()
    {
        return attack;
    }

    public bool GetSuplexButtonDown()
    {
        return suplex;
    }

    private void Update()
    {
        //2
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical   = Input.GetAxisRaw("Vertical");
        attack     = Input.GetButtonDown("Attack");

        //3
        if (!jump && !isJumping && Input.GetButtonDown("Jump"))
        {
            jump = true;
            lastJumpTime = Time.time;
            isJumping = true;
        }
        else if (!Input.GetButtonDown("Jump"))
        {
            //4
            jump = false;
            isJumping = false;
        }

        if (!jump && !isJumping && Input.GetButtonDown("Suplex"))
        {
            suplex = true;
        }
        else if (!Input.GetButtonDown("Suplex"))
        {
            suplex = false;
        }



        //5 
        if (jump && Time.time > lastJumpTime + maxJumpDuration)
        {
            jump = false;
        }
        
    }
}
