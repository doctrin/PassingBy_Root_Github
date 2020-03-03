using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Robot : Enemy
{
    public RobotColor color;

    public SpriteRenderer smokeSprite;
    public SpriteRenderer beltSprite;

    //Robot 색상 설정
    public void SetColor(RobotColor color)
    {
        this.color = color;

        switch (color)
        {
            //Colorless Robot 속성 설정
            case RobotColor.Colorless:
                baseSprite.color = Color.white;
                smokeSprite.color = Color.white;
                beltSprite.color = Color.white;
                maxLife = 50.0f;
                normalAttack.attackDamage = 2;
                break;

            //Copper Robot 속성 설정
            case RobotColor.Copper:
                baseSprite.color = new Color(1.0f, 0.75f, 0.62f);
                smokeSprite.color = new Color(0.38f, 0.63f, 1.0f);
                beltSprite.color = new Color(0.86f, 0.85f, 0.71f);
                maxLife = 100.0f;
                normalAttack.attackDamage = 4;
                break;

            //Silver Robot 속성 설정
            case RobotColor.Silver:
                baseSprite.color = Color.white;
                smokeSprite.color = new Color(0.38f, 1.0f, 0.5f);
                beltSprite.color = new Color(0.5f, 0.5f, 0.5f);
                maxLife = 125.0f;
                normalAttack.attackDamage = 5;
                break;

            //Gold Robot 속성 설정
            case RobotColor.Gold:
                baseSprite.color = new Color(0.91f, 0.7f, 0.0f);
                smokeSprite.color = new Color(0.42f, 0.15f, 0.10f);
                beltSprite.color = new Color(0.86f, 0.5f, 0.32f);
                maxLife = 150.0f;
                normalAttack.attackDamage = 6;
                break;

            //Random color Robot 속성 설정
            case RobotColor.Random:
                baseSprite.color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
                smokeSprite.color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
                beltSprite.color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
                maxLife = Random.Range(100, 250);
                normalAttack.attackDamage = Random.Range(4, 10);
                break;
        }

        currentLife = maxLife;
    }

    [ContextMenu("Color: Copper")]
    void SetToCopper()
    {
        SetColor(RobotColor.Copper);
    }

    [ContextMenu("Color: Silver")]
    void SetToSilver()
    {
        SetColor(RobotColor.Silver);
    }

    [ContextMenu("Color: Gold")]
    void SetToGold()
    {
        SetColor(RobotColor.Gold);
    }

    [ContextMenu("Color: Random")]
    void SetToRandom()
    {

    }

    
    protected override IEnumerator KnockdownRoutine()
    {
        isKnockedOut = true;
        baseAnim.SetTrigger("Knockdown");
        ai.enabled = false;

        yield return new WaitForSeconds(2.0f);
        baseAnim.SetTrigger("GetUp");
        ai.enabled = true;

        knockdownRoutine = null;

    }

    public override void Update()
    {
        base.Update();
        
        if (isKnockedOut && isGrounded && knockoutBouncingCnt == 0)
        {
            //단 한 번만 knockoutBouncing 하기 위해 index cnt 증가. 
            //--> Actor > DidGetUp() 메서드에서 GetUp 할 때 다시 초기화 함.
            knockoutBouncingCnt++; 

            int bouncingDirection = 1;
            if (takedHitVector.x < 0)
            {
                bouncingDirection = -1;
            }

            Vector3 bouncingPointVector = new Vector3( (this.transform.position.x+(1* bouncingDirection)), this.transform.position.y, this.transform.position.z);

            //Original
            //this.transform.DOJump(bouncingPointVector, 0.5f, 1, 0.35f, false);

            //Test
            this.transform.DOJump(bouncingPointVector, 0.55f, 1, 0.35f, false);
        }

    }


}

/// <summary>
/// Robot color enum
/// </summary>
public enum RobotColor
{
    Colorless = 0,
    Copper,
    Silver,
    Gold,
    Random
}