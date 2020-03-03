using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Hero : Actor
{

    [Header("+ Speed setting")] //-----------------------------------------------------
    public float walkSpeed = 5;         //걸을 때의 속도
    public float runSpeed = 11;        //뛸 때의 속도
    private float jumpSpeed;             //jumpSpeed : speed or runSpeed

    [Header("+ State setting")] //-----------------------------------------------------
    bool isRunning;                      //Hero가 뛰는 상태를 정의 하기 위한 var
    bool isMoving;
    float lastWalk;
    public bool canRun = true;
    float tapAgainToRunTime = 0.2f;
    Vector3 lastWalkVector;

    [Header("+ Direction setting")] //-----------------------------------------------------
    Vector3 currentDir;                 //hero가 움직일 실제 방향
    public bool isFacingLeft;               //좌측을 보는지의 Boolean

    [Header("+ Jump Anim. Boolean")] //-----------------------------------------------------
    bool isJumpLandAnim;                // jump 착지 애니메이션이 플레이 되고 있는지 확인용 bool
    bool isJumpingAnim;                 // jump 애니메이션이 플레이 되고 있는지 아닌지 확인용 bool
    bool isNowJumpingAnim;              // 통틀어서 jump하고 있는지 확인용 bool

    [Header("+ 입력 처리기")] //-----------------------------------------------------
    public InputHandler input;          //InputHandler.cs 레퍼런스용 var

    [Header("+ Jump var setting")] //-----------------------------------------------------
    public float jumpForce = 600f;  //Jump용 var ; 레거시는 1750f, new는 500f
    private float jumpDuration = 0.2f;
    private float lastJumpTime;

    private bool isCanJump = true;      //orginal custom 변수 ; 뛸 수 있는지 여부 check용. //일단 남겨두기는 하겠는데 이걸로 로직을 짜봤으나 점프가 되지를 않는다. 2018.12.30

    [Header("+ Attk Anim. Boolean")] //-----------------------------------------------------
    bool isAttackingAnim;      //Attack 관련 var
    float lastAttackTime; //Actor.cs로 옮김 for HitForwarder.cs에서 사용하기 위해서
    float attackLimit = 0.14f;

    [Header("+ Suplex var setting")] //-----------------------------------------------------
    public float suplexJumpPower = 9.5f;
    public float suplexDuration = 0.35f;

    [Header("+ Hero`s sprite")] //-----------------------------------------------------
    public SpriteRenderer heroSprite;

    //공격 데이터 용 변수
    public AttackData normalAttack2;
    public AttackData normalAttack3;
    public AttackData normalAttack4;

    //Combo-chaining logic 용 변수
    float chainComboTimer;
    public float chainComboLimit = 0.38f; //Original value : 0.3f
    const int maxCombo = 4; //Original value : 3
    private int currentAttackChain = 1;
    public int evaluatedAttackChain = 0;

    private int default_currentAttackChain = 0;
    private int default_evaluatedAttackChain = 0;

    //공중공격 용 변수
    public bool canJumpAttack = true; //플레이어는 오직 한 번만 점프 중에 공격 할 수 있고, canJumpAttack 변수가 구분을 담당한다.
    public AttackData jumpAttack;

    //hurt animation 인지 아닌지 판단
    bool isHurtAnim;

    //대쉬 공격 관련 데이터
    public AttackData runAttack;
    public float runAttackForce = 1.8f;

    public bool isFirstAttackHitted = false;

    //잡기 관련 변수
    public bool isGrabAnim = false; //잡기 중인지 확인용 bool.
    float grabLimit = 1.2f; //잡기 유지 시간
    float grabTimer;
    const int maxGrabCombo = 3;
    public int currentGrabAttackChain = 0;
    public int evaluatedGrabAttackChain = 0;
    public AttackData grabAttack1;
    public AttackData grabAttack2;
    public AttackData grabAttack3; //마지막 잡기 공격은 KnockBack 공격. (Setting 정보는 Unity > Sunbi > Inspector에 있다. 
    public AttackData throwAttack; //잡기 후, 던지기 공격 (normal)

    public AudioClip suplexImpactSound;
    public GameObject grabbedEnemyPosition; //던질때의 적 포지션 위치


    //For heroic entrances
    public Walker walker;
    public bool isAutoPiloting;
    public bool controllable = true;

    public override void Update()
    {
        base.Update();

        if (!isAlive)
        {
            return;
        }

        isAttackingAnim = baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack1")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack2")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack3")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack4")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("run_attack")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("jump_attack");

        isJumpLandAnim = baseAnim.GetCurrentAnimatorStateInfo(0).IsName("jump_land")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("dash_jump_land");

        isJumpingAnim = baseAnim.GetCurrentAnimatorStateInfo(0).IsName("jump_rise")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("jump_fall")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("dash_jump_rise")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("dash_jump_fall");

        isHurtAnim = baseAnim.GetCurrentAnimatorStateInfo(0).IsName("hurt");

        isGrabAnim = baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grab_idle")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grab_attack_1")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grab_attack_2")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grab_attack_final")
                          || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grab_throw");


        if (isJumpLandAnim == true || isJumpingAnim == true)
        {
            isNowJumpingAnim = true;
        }
        else
        {
            isNowJumpingAnim = false;
        }

        //오토 파일럿 사용중이라면 이하의 로직은 배제한다.
        if (isAutoPiloting)
        {
            return;
        }

        float inputH = input.GetHorizontalAxis();
        float inputV = input.GetVerticalAxis();

        bool isJump = input.GetJumpButtonDown();
        bool isAttack = input.GetAttackButtonDown();
        bool isSuplex = input.GetSuplexButtonDown();

        currentDir = new Vector3(inputH, 0, inputV);
        currentDir.Normalize();


        //공격 애니메이션, 잡기 애니메이션, 또는 피격 애니메이션이 재생중이 아니라면 이동 가능. 
        //if (!isAttackingAnim && !isGrabAnim && !isHurtAnim)

        //NEW --> 조건 변경
        //if(isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim && (baseAnim.GetInteger("CurrentChain") < 1 && chainComboTimer <= 0))

        if (!isAttackingAnim && !isGrabAnim && !isHurtAnim && !isFirstAttackHitted)
        {

            if ((inputV == 0 && inputH == 0))
            {
                Stop();
                isMoving = false;
            }
            else if (!isMoving && (inputV != 0 || inputH != 0))
            {
                isMoving = true;
                float dotProduct = Vector3.Dot(currentDir, lastWalkVector);

                if (canRun && Time.time < lastWalk + tapAgainToRunTime && dotProduct > 0)
                {
                    Run();
                }
                else
                {
                    //AS-IS
                    //if(CanWalk()) Walk();

                    //Debug.Log("` : "+ baseAnim.GetInteger("CurrentChain"));

                    //TO-BE
                    //if (baseAnim.GetInteger("CurrentChain") < 1)
                    //if((Time.time + 0.5f) >= (lastAttackTime + attackLimit))


                    //if(lastAttackTime == 0)
                    //if (baseAnim.GetInteger("CurrentChain") < 1)

                    if (IsCanWalk())
                    {
                        Walk();
                    }

                    if (inputH != 0)
                    {
                        lastWalkVector = currentDir;
                        lastWalk = Time.time;
                    }
                }

            }
        }


        //Debug.Log(">> in Update method  /  chainComboTimer : "+ chainComboTimer);

        //1 -> tutorial 교재의 62% 되는 시점의 내용.
        //chainComboTimer가 0 보다 크면
        if (chainComboTimer > 0)
        {

            //Debug.Log("----> chainComboTimer 빼보자긔 : " + chainComboTimer);

            //시간만큼 뺀다.
            chainComboTimer -= Time.deltaTime;

            //만약, chainComboTimer가 0 보다 작게 되면,
            //chainCombo 관련 변수들을 reset 한다.
            if (chainComboTimer < 0)
            {
                AttackDataInit();
            }
        }

        //TO-DO: 잡기 후에 잡기 지속 타임을 지속적으로 줄여서
        //       '0' 보다 작게 되면, 잡기상태를 해제 하는 소스를 여기에 구현하자.


        //공격 타이머를 여기서 줄인다. 
        if (lastAttackTime > 0)
        {
            //시간만큼 뺀다.
            lastAttackTime -= Time.deltaTime;

            if (lastAttackTime < 0)
            {
                lastAttackTime = 0;
            }
            //이렇게 하지 않으면 공격이 매끄럽지 못하다.


            //lastAttackTime = 0; // <-- 이렇게 해보았지만 별로 효과는 없었다.(2019.08.21)

        }


        //조건에 따른 Jump method 실행
        if (isGrounded && isJump && !isKnockedOut && isNowJumpingAnim == false && !isAttackingAnim && !isHurtAnim && !isGrabAnim)
        {
            //Debug.Log("----------------> 1. isGrounded   :::  " + isGrounded + " / isCanJump   :::  "+ isCanJump);

            Jump(currentDir);
            //isCanJump = false;
        }



        //조건에 따른 Attack method 실행

        //AS-IS
        //if (isAttack && Time.time >= lastAttackTime + attackLimit && !isKnockedOut && !isHurtAnim)
        /*
                Debug.Log("==============================================================");
                Debug.Log("--------1 --------> isAttack : " + isAttack);
                Debug.Log("--------2 --------> Time.time+0.5 : " +(Time.time+0.5f));
                Debug.Log("--------3 --------> lastAttackTime+attackLimit : " + (lastAttackTime+ attackLimit));
                Debug.Log("--------4 --------> isKnockedOut : " + isKnockedOut);
                Debug.Log("--------5 --------> isHurtAnim : " + isHurtAnim);
                Debug.Log("==============================================================");
        */

        //TO-BE //잠깐 코딩의 신이 강림했나 보다... -ㅂ-);;;
        if (isAttack && (Time.time + 0.5f) >= (lastAttackTime + attackLimit) && !isKnockedOut && !isHurtAnim)
        //1. 어택버튼이 눌러졌고 (isAttack == true)
        //2. 현재시각 + 0.5f (Time.time +0.5f) <--- 크거나 같고 <--- 마지막 Attack한 시각 + attackLimit (0.14f)
        //3. KnockOut 상태가 아니고
        //4. 공격 당하고 있는 상태가 아니라면 작동!
        {
            lastAttackTime = Time.time;
            Attack();

            //여기서 일단 강제로 chainComboTimer의 Value를 지정해 보자. 
            //for CanWalk()를 활용한 펀치 버튼이 눌러지면 이동 못하게 하는 로직이 동작하게 하기 위해서
            //chainComboTimer = chainComboLimit; //<--- 아직 별 효과는 없는듯 하다.(2019.08.31)

        }

        //조건에 따른 Suplex method 실행 - Suplex의 가능 조건도 제한이 더 필요하다. 달릴때, 맞을때는 못한다든지 하는 식으로.
        //test condition : 한시적으로 착지 상태이고 Suplex 커멘드 입력 완료 하면 suplex 가능 --> TO-DO : 추후 여러 조건 추가 필요
        if (isGrounded && CommandSequences.SequenceIsCompleted("Suplex"))
        {
            Suplex();
        }


        // 공격 당하고 있다면 공격관련 Param 초기화
        if (isHurtAnim)
        {
            AttackDataInit();
        }
    }

    void Jump(Vector3 direction)
    {

        //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!> Jump 에서 확인 하는거!!! isJumpingAnim   :::::::::  " + isJumpingAnim);
        //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!> Jump 에서 확인 하는거 222 !!! jumpForce   :::::::::  " + jumpForce); 

        //실질적으로 여기서 Jump를 실행한다.
        if (!isJumpingAnim)
        {
            if (isRunning)
            {
                //뛰는 중이면, 대쉬 점프!
                baseAnim.SetTrigger("Dash_Jump");
                jumpSpeed = runSpeed + 3;
            }
            else
            {
                //뛰고 있는게 아니라면, normal 점프
                baseAnim.SetTrigger("Jump");
                jumpSpeed = speed;
            }
            lastJumpTime = Time.time;

            //force add -> X 축
            Vector3 horizontalVector = new Vector3(direction.x, 0, direction.z) * jumpSpeed * 40;
            body.AddForce(horizontalVector, ForceMode.Force);
        }

        //force add -> Y 축
        Vector3 verticalVector = Vector3.up * jumpForce * Time.deltaTime;
        //body.AddForce(verticalVector, ForceMode.Force);
        //body.AddForce(verticalVector, ForceMode.Impulse);

        body.velocity = Vector3.up * jumpForce;

    }

    //착지하면 바로 걷기 상태로 전환.
    protected override void DidLand()
    {
        base.DidLand();
        Walk();
    }


    public void Stop()
    {
        speed = 0;
        baseAnim.SetFloat("Speed", speed);
        isRunning = false;
        baseAnim.SetBool("IsRunning", isRunning);
    }

    public void Run()
    {
        speed = runSpeed;
        baseAnim.SetFloat("Speed", speed);
        isRunning = true;
        baseAnim.SetBool("IsRunning", isRunning);
    }

    public void Walk()
    {
        speed = walkSpeed;
        baseAnim.SetFloat("Speed", speed);
        isRunning = false;
        baseAnim.SetBool("IsRunning", isRunning);
    }

    void FixedUpdate()
    {
        if (!isAutoPiloting)
        {
            //Old code goes here

            if (!isAlive)
            {
                return;
            }

            Vector3 moveVector = currentDir * speed;

            //이동 ; TO-DO : 혹시 attack 중에 이동을 해야 한다면 하단의 !isAttackinAnim 조건 param을 제거 하여야 한다.
            if (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim)
            {
                body.MovePosition(this.transform.position + moveVector * Time.fixedDeltaTime);
                baseAnim.SetFloat("Speed", moveVector.magnitude);
            }

            //스프라이트 좌우 반전
            //if (moveVector != Vector3.zero && isGrounded && !isKnockedOut && !isAttackingAnim)
            if (moveVector != Vector3.zero && isGrounded && !isKnockedOut)
            {
                if (moveVector.x != 0)
                {
                    isFacingLeft = moveVector.x < 0;
                }
                FlipSprite(isFacingLeft);
            }
        }
    }

    //공격!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public override void Attack()
    {

        //잡기 상태 일때의 공격 - START - 


        //Debug.Log("++++++++++++++++++++++++++++++");
        //Debug.Log("++++++ 공격 버튼은 눌러졌고, 현재의 Anim state grab_idle ? : "+ baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grab_idle") + " +++++");
        //Debug.Log("++++++++++++++++++++++++++++++");

        //잡기 상태 일때의 공격, 더 정확하게는 잡기-idle 일때에만 공격이 가능하다. 
        if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grab_idle"))
        {
            //currentAttackChain = currentAttackChain+1;
            //evaluatedAttackChain = 0;

            //Debug.Log("++++++++++++++++++++++++++++++");
            //Debug.Log("++++++ 잡기 상태일때의 공격이 시작 되었다! +++++");
            //Debug.Log("++++++++++++++++++++++++++++++");

            //1. 우선 현재 잡기 상태의 Boolean 값을 false로 변경하여, 로직 상에서는 잡기 상태를 벗어나
            //   잡기 타격 상태로 돌아가게끔 한다. 
            //2. 그전에 일단 GrabCurrentChain 을 증가 시킴으로써, 잡기 1타 상태로 갈 param 을 준비 한다. 

            //Debug.Log("/1/_currentGrabAttackChain   : " + currentGrabAttackChain  );
            //Debug.Log("/2/_evaluatedGrabAttackChain : " + evaluatedGrabAttackChain);

            //if (input.GetVerticalAxis() != 0)
            //{
            //    //던지기
            //    baseAnim.SetTrigger("EnemyThrowTrigger");
            //    baseAnim.SetBool("IsEnemyGrab", false);
            //}
            //else
            //조건에 따른 던지기 실행

            //Debug.Log("+++++ CommandSequences.SequenceIsCompleted('EnemyThrow') : "+ CommandSequences.SequenceIsCompleted("EnemyThrow"));
            if (input.GetHorizontalAxis() != 0)
            {
                //던지기
                baseAnim.SetTrigger("EnemyThrowTrigger");
                baseAnim.SetBool("IsEnemyGrab", false);

                //던지기 실행 시, 던져지는 Enemy > Animation Bool(false)을 전달 하여 잡기가 끝났음을 전달.
                //grabbedActor.baseAnim.SetBool("IsGrabbed", false);  //--> 아래의 runRollingBackThrow method로 로직 위치 변동
                grabbedActor.transform.position = this.grabbedEnemyPosition.transform.position;

                //-- currentDir 으로 던지는 테스트 코드 - 시작 --------------------------



                /*
                //이렇게 하면 너무 천천히 떨어지고, 즉. 약하게 던져지고
                    this.grabAttackDirection.x = input.GetHorizontalAxis();
                    //this.grabAttackDirection.z = Vector3.up * jumpForce * Time.deltaTime;
                    //this.grabAttackDirection.y = 100 * Time.deltaTime;

                    Vector3 verticalVector = Vector3.up * jumpForce * Time.deltaTime;
                    grabbedActor.body.velocity = Vector3.up * jumpForce;

                    grabbedActor.EvaluateAttackData(throwAttack, this.grabAttackDirection, this.grabAttackStartPoint);
                */

                /*
                Vector3 throwVectorForce = new Vector3(input.GetHorizontalAxis()*9, 10, 0); 
                grabbedActor.GetComponent<Rigidbody>().AddForce(throwVectorForce, ForceMode.Impulse);
                */


                //Latest TEST
                this.grabAttackDirection.x = input.GetHorizontalAxis(); // * -1; //-1을 곱해준건 sunbi_grab_throw2 애니메이션일 경우 반대방향으로 던지는 거라서 그렇게 함.


                /*  //이걸 독립 method로 뽑을 거야.
                  
                    //실제적으로 던져지는 모션 로직 - START - 
                    Vector3 throwVectorForce = new Vector3(this.grabAttackDirection.x * 3, 5, 0);
                    grabbedActor.GetComponent<Rigidbody>().AddForce(throwVectorForce, ForceMode.Impulse);
                    grabbedActor.EvaluateAttackData(throwAttack, this.grabAttackDirection, this.grabAttackStartPoint);
                    //실제적으로 던져지는 모션 로직 - END   - 
                */

                //스프라이트 좌우 반전
                FlipSprite(this.grabAttackDirection.x > 0);

                /*
                //이렇게 하면 좌,우로 던져지지 않고.
                    this.grabAttackDirection.x = input.GetHorizontalAxis();
                    Vector3 horizontalVector = new Vector3(this.grabAttackDirection.x, 0, currentDir.z) * jumpSpeed * 40;
                    this.grabbedActor.body.AddForce(horizontalVector, ForceMode.Force);

                    Vector3 verticalVector = Vector3.up * jumpForce * Time.deltaTime;
                    grabbedActor.body.velocity = Vector3.up * jumpForce;
                */


                //-- currentDir 으로 던지는 테스트 코드 - 종료 --------------------------
            }
            else
            {
                //잡기공격
                currentGrabAttackChain = currentGrabAttackChain + 1;
                baseAnim.SetInteger("GrabCurrentChain", currentGrabAttackChain);
                baseAnim.SetInteger("GrabEvaluatedChain", evaluatedGrabAttackChain);
                //evaluatedGrabAttackChain = evaluatedGrabAttackChain + 1;
                //evaluatedGrabAttackChain++;
                //baseAnim.SetInteger("EvaluatedChain", evaluatedAttackChain);
            }



            //TO-DO: 일단 여기서 공격 정보를 전달 하기는 했느데 반응이 없다;;
            //--> Solution: 잡기 공격이 시작하기도 전에 HitActor를 호출한 관계로
            //              Animation의 state로 판단하는 HitActor가 판단할 수가 없다. 
            //              ==> Animation clip에서 HitActor()를 실행하도록 하자.

            //HitActor(grabbedActor, grabAttackStartPoint, grabAttackDirection);
            //--> DidGrabChain() 에서 수행하도록 한다. 

            return;
        }

        //잡기 상태 일때의 공격 - END -



        //maxCombo 만큼만 공격 가능 && 잡기 상태 아닐 때
        if (currentAttackChain <= maxCombo && !isGrabAnim)
        {

            //Hero 가 공중에 있는지 체크
            if (!isGrounded)
            {
                //다시 한 번 더 공중공격 할 수 있는지 체크
                if (isJumpingAnim && canJumpAttack)
                {
                    //오직 한 번의 공중공격만 하도록 제어
                    canJumpAttack = false;

                    //currentAttackChain과 EvaluatedAttackChain을 각각 1과 0으로 설정해 
                    //다른 공격과의 체인을 방지한다.
                    currentAttackChain = 1;
                    evaluatedAttackChain = 0;

                    baseAnim.SetInteger("CurrentChain", currentAttackChain);
                    baseAnim.SetInteger("EvaluatedChain", evaluatedAttackChain);

                    //Hero의 rigidbody에 적용 되던 Gravity를 제거. (<--- 쉽게 말하면 공중 발차기 할 동안에는 점프하는 방향으로 쭈우우욱~ 나갈듯...) 
                    //                                             역시나 우리 게임에서는 필요가 없다.
                    //body.velocity = Vector3.zero;
                    //body.useGravity = false;

                }

            }
            else
            {
                if (isRunning)
                {
                    // Running 공격 실행.

                    //1
                    body.AddForce((Vector3.up + (frontVector * 5)) * runAttackForce, ForceMode.Impulse);

                    //2
                    currentAttackChain = 1;
                    evaluatedAttackChain = 0;

                    baseAnim.SetInteger("EvaluatedChain", evaluatedAttackChain);
                    baseAnim.SetInteger("CurrentChain", currentAttackChain);
                }
                else
                {
                    // 러닝 공격 이외의 통상 공격 로직

                    /*
                            Debug.Log("++++++++++++++++++++++++++++++");
                            Debug.Log("++++++지상공격 & 기본 = 제일 중요 +++++");
                            Debug.Log("++++++++++++++++++++++++++++++");

                            Debug.Log("Before_/Hero.cs/ --- in [Attack()] method / EvaluatedChain   :  " + evaluatedAttackChain);
                            Debug.Log("    Before_/Hero.cs/ --- in [Attack()] method / CurrentChain   :  " + currentAttackChain);
                            Debug.Log("        Before_/Hero.cs/ --- chainComboTimer : "+ chainComboTimer);
                    */

                    //currentAttackChain, chainComboTimer reset
                    if (currentAttackChain == 0 || chainComboTimer == 0)
                    {

                        //Debug.Log("!!!!!!!!!!!!!! 어택 체인 INIT !!!!!!!!!!!!!!!!!!! /Hero.cs/[Attack()] - ????????????????? ---> 혹시 여기로 들어오나?");

                        currentAttackChain = 1;
                        evaluatedAttackChain = 0;

                        chainComboTimer = chainComboLimit; //NEW --> 새롭게 값을 대입하여 봄 -2019.09.18
                    }


                    /*
                            Debug.Log("111_Hero.cs --- in [Attack()] method / EvaluatedChain   :  " + evaluatedAttackChain);
                            Debug.Log("    222_Hero.cs --- in [Attack()] method / CurrentChain   :  " + currentAttackChain);
                    */
                    //TO-DO : TEST - START - 
                    // if (currentAttackChain >= 4)
                    // {
                    /*
                            Debug.Log("왔다왔다왔따왔다왔다왔다왔따왔다왔다왔다왔따왔다_Hero.cs --- in [Attack()] method / EvaluatedChain   :  " + evaluatedAttackChain);
                            Debug.Log("    왔다왔다왔따왔다왔다왔다왔따왔다왔다왔다왔따왔다왔다왔다왔따왔다_Hero.cs --- in [Attack()] method / CurrentChain   :  " + currentAttackChain);
                    */
                    //evaluatedAttackChain = 3; //임시 값 설정



                    // }
                    //TO-DO : TEST - END -


                    // 첫 번째 공격을 맞추지 못했고, 현재 Play 되고 있는 Animation이 "attack1" 이라면
                    if (!isFirstAttackHitted && baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack1"))
                    {
                        //baseAnim.ResetTrigger("1stAttrackTrigger");
                        //baseAnim.SetTriggerOneFrame("1stAttrackTrigger", this);


                        //1.2.3.4.5.6.번 모두 첫 번째 애니메이션을 replay 하는 방법 이지만,
                        //5번이 가장 효과적인것으로 보인다.

                        //1.
                        //baseAnim.SetTrigger("1stAttrackTrigger"); // 첫 번째 공격을 시작하라는 Trigger를 setting 한다. 

                        //2.
                        //baseAnim.ResetTrigger("1stAttrackTrigger"); // 첫 번째 공격을 시작하라는 Trigger를 setting 한다. 

                        //3.
                        //baseAnim.Play("attack1", -1, 0f);

                        //baseAnim.Play("attack1");

                        //4.
                        //baseAnim.CrossFade("attack1", 0f);

                        //5.
                        //baseAnim.CrossFadeInFixedTime("attack1", 0.129f); // AS-IS 해결책

                        //baseAnim.GetCurrentAnimatorClipInfo(0)[0].clip.wrapMode = WrapMode.Loop;

                        //6.
                        //baseAnim.CrossFadeInFixedTime("attack1", baseAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length+0.1f);


                        //Debug.Log("---------- 연타 시도?!??! ----------");

                        //7.
                        baseAnim.CrossFadeInFixedTime("attack1", 0f);
                        //baseAnim.CrossFade("attack1", 0f);


                        //chainComboTimer = chainComboLimit; //NEW --> 새롭게 값을 대입하여 봄 -2019.09.18


                        //---Test  -- Start --
                        //baseAnim.SetInteger("EvaluatedChain", 0);
                        //baseAnim.SetInteger("CurrentChain", 1);
                        //---Test  -- End --



                    }
                    else
                    {
                        // Animator에 Param 전달

                        //Debug.Log("++++++Animator에 Param 전달++++++");
                        //Debug.Log("++++++currentAttackChain ++++++ "+ currentAttackChain);
                        //Debug.Log("++++++evaluatedAttackChain ++++++ "+ evaluatedAttackChain);
                        




                        baseAnim.SetInteger("CurrentChain", currentAttackChain);
                        baseAnim.SetInteger("EvaluatedChain", evaluatedAttackChain);

                        //chainComboTimer = chainComboLimit; //NEW --> 새롭게 값을 대입하여 봄 -2019.09.18
                    }

                }

            }
        }



    }

    public void AnalyzeNormalAttack(AttackData attackData, int attackChain, Actor actor, Vector3 hitPoint, Vector3 hitVector)
    {
        //Debug.Log("-->>> /Hero.cs/ [AnalyzeNormalAttack] / attackChain ?  :  "+attackChain);

        actor.EvaluateAttackData(attackData, hitVector, hitPoint);

        if (!isGrabAnim) //잡기 공격이 아닐 경우
        {
            currentAttackChain = attackChain;

            if (attackChain != 0)
            {
                chainComboTimer = chainComboLimit; //NEW --> 새롭게 값을 대입하여 봄 -2019.09.18
            }
        }
        else //잡기 공격일 경우
        {
            //잡힌 상태의 Trigger를 날려서 잡힌 스프라이트로 변경.
            if (!attackData.knockdown)
            {
                this.baseAnim.SetTrigger("Grabbed");
                this.grabbedActor.baseAnim.SetBool("IsGrabbed", true);
            }
            else
            {
                //잡기 공격의 마지막 공격 시, 잡힌 Enemy > Animation Bool(false)을 전달 하여 잡기가 끝났음을 전달.
                this.grabbedActor.baseAnim.SetBool("IsGrabbed", false);
            }
        }
    }

    public void DidChain(int chain)
    {
        evaluatedAttackChain = chain;
        baseAnim.SetInteger("EvaluatedChain", evaluatedAttackChain);
        chainComboTimer = chainComboLimit;
    }

    public void DidGrabChain(int chain)
    {
        evaluatedGrabAttackChain = chain;
        baseAnim.SetInteger("GrabEvaluatedChain", chain);

        HitActor(grabbedActor, grabAttackStartPoint, grabAttackDirection);

        //마지막 공격일 경우, 잡기 상태를 풀어준다.
        if (chain == 3)
        {
            baseAnim.SetBool("IsEnemyGrab", false);
        }

    }

    //대망의 수플렉스 
    public void Suplex()
    {
        DOTween.Init(); //DOTween 객체 생성

        //수플렉스 목표지점의 좌표 Get
        Vector3 suplexDestinationV3 = this.suplexDestination.transform.position;

        //수플렉스 실행 으오오오오오오

        //this.transform.DOJump(suplexDestinationV3, suplexJumpPower, 11, suplexDuration, false).OnPlay(SuplexStripFlip);
        //this.transform.DOPunchPosition(this.transform.position, 0.25f, 5, 0, false); //DOPunchPosition 을 해봤지만, Y축을 움직여버리는지라 우리 게임에 잘 맞지 않음.

        this.transform.DOJump(suplexDestinationV3, suplexJumpPower, 1, suplexDuration, false).OnComplete(SuplexShock);

        //스프라이트 좌우 반전 코루틴 실행 - 주석처리 ( 좀 더 연구 필요함)
        //FlipSprite(this.grabAttackDirection.x < 0);
        //StartCoroutine("RunFlipSprite");


    }

    //스프라이트 좌우 반전 코루틴
    IEnumerator RunFlipSprite()
    {
        FlipSprite(this.grabAttackDirection.x < 0);
        yield return new WaitForSeconds(0.05f);

    }


    //수플렉스 중의 스프라이트 플립효과
    //TO-DO : 아직 구현완료 되지 않았으며, 필요시 에만 쓰면 된다.
    public void SuplexSpriteFlip()
    {
        heroSprite.flipX = !heroSprite.flipX;
    }

    //수플렉스가 끝나면 발생하는 진동
    public void SuplexShock()
    {
        GameObject suplexEffectObj = Instantiate(hitMiddleGroundSmashPrefab);
        suplexEffectObj.transform.position = this.transform.position;

        //Shake 발생
        this.transform.DOShakePosition(0.5f, 1, 15, 30, false, true);

        //Splex ground shock sound play
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(suplexImpactSound);

    }

    //바닥(Floor)에 닿으면 canJumpAttack 이 true.
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (collision.collider.name == "Floor")
        {
            canJumpAttack = true;
        }

        //공격 변수 초기화 - 불필요한 공격 체인 방지
        //AttackDataInit();
    }

    //점프 공격을 한 후에는 중력의 영향을 다시 true 로 설정한다. ; 글쎄 별 필요 없다.
    public void DidJumpAttack()
    {
        //body.useGravity = true;

        //공격 변수 초기화 - 불필요한 공격 체인 방지
        AttackDataInit();

    }

    private void AnalyzeSpecialAttack(AttackData attackData, Actor actor, Vector3 hitPoint, Vector3 hitVector)
    {
        actor.EvaluateAttackData(attackData, hitVector, hitPoint);
        chainComboTimer = chainComboLimit;
    }

    protected override void HitActor(Actor actor, Vector3 hitPoint, Vector3 hitVector)
    {
        /*
                Debug.Log("- @CHILD@ [HitActor] method !!!! --------------------");

                Debug.Log("==================================================================================================");
                Debug.Log("      is 1타 재생 중 ? : " + baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack1"));
                Debug.Log("      is 2타 재생 중 ? : " + baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack2"));
                Debug.Log("      is 3타 재생 중 ? : " + baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack3"));
                Debug.Log("      is 4타 재생 중 ? : " + baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack4"));
                Debug.Log("==================================================================================================");
        */

        if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack1"))
        {
            AnalyzeNormalAttack(normalAttack, 2, actor, hitPoint, hitVector);

            //Debug.Log("-------------1 타 !!");
            isFirstAttackHitted = true;

        }
        else if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack2"))
        {
            AnalyzeNormalAttack(normalAttack2, 3, actor, hitPoint, hitVector);

            //Debug.Log("-------------2 타 !!");
            //isFirstAttackHitted = true;
        }
        else if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack3"))
        {
            AnalyzeNormalAttack(normalAttack3, 4, actor, hitPoint, hitVector);

            //Debug.Log("-------------3 타 !!");
            //isFirstAttackHitted = true;
        }
        else if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("attack4"))
        {
            AnalyzeNormalAttack(normalAttack4, 1, actor, hitPoint, hitVector);

            //Debug.Log("-------------4 타 !!");
            isFirstAttackHitted = false;
        }


        else if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("jump_attack"))
        {
            AnalyzeSpecialAttack(jumpAttack, actor, hitPoint, hitVector);
        }
        else if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("run_attack"))
        {
            AnalyzeSpecialAttack(runAttack, actor, hitPoint, hitVector);
        }

        //잡기 공격
        else if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grabAttack1"))
        {
            AnalyzeNormalAttack(grabAttack1, 0, actor, hitPoint, hitVector);
        }
        else if (baseAnim.GetCurrentAnimatorStateInfo(0).IsName("grabAttack2"))
        {
            AnalyzeNormalAttack(grabAttack2, 0, actor, hitPoint, hitVector);
        }

    }

    protected override void GrabActor(Actor actor, Vector3 grabPoint, Vector3 grabVector)
    {
        //base.GrabActor(actor, grabPoint, grabVector);

        currentGrabAttackChain = 0; //잡기 공격 관련 변수 초기화
        evaluatedGrabAttackChain = 0;
        baseAnim.SetBool("IsEnemyGrab", true);

        //잡힌 놈에게 잡힌 상태로 만드는 메소드 실행!
        actor.ToGrabbedState(grabVector, grabPoint);

    }


    //공격 변수 초기화 - 불필요한 공격 체인 방지
    private void AttackDataInit()
    {
        chainComboTimer = 0;
        currentAttackChain = default_currentAttackChain;
        evaluatedAttackChain = default_evaluatedAttackChain;
        isFirstAttackHitted = false;

        baseAnim.SetInteger("CurrentChain", default_currentAttackChain);
        baseAnim.SetInteger("EvaluatedChain", default_evaluatedAttackChain);
    }

    public override void TakeDamage(float value, Vector3 hitVector, bool knockdown = false)
    {
        //즉, 공중에 있는 상태에서 맞으면 knockdown 상태가 된다. 
        if (!isGrounded)
        {
            knockdown = true;
        }

        //Hero가 맞게 되면, 공격 관련 Param 초기화
        AttackDataInit();

        base.TakeDamage(value, hitVector, knockdown);
    }

    //걸어다닐 수 있는 상태인지 체크
    public override bool IsCanWalk()
    {
        //return (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim && (baseAnim.GetInteger("CurrentChain") == 0) );
        //(Time.time + 0.5f) >= (lastAttackTime + attackLimit)
        //return (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim && (Time.time + 0.5f) >= (lastAttackTime + attackLimit)));
        //evaluatedAttackChain

        //Debug.Log("Hero.cs > CanWalk -> evaluatedAttackChain : "+ evaluatedAttackChain);

        //Debug.Log("Hero.cs > CanWalk -> baseAnim.GetInteger(\"CurrentChain\") : " + baseAnim.GetInteger("CurrentChain"));
        //baseAnim.SetInteger("EvaluatedChain"

        //Debug.Log("------- Hero.cs > CanWalk -> baseAnim.GetInteger(\"EvaluatedChain\") : " + baseAnim.GetInteger("EvaluatedChain"));
        //chainComboTimer

        //return (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim && (chainComboTimer == 0) );

        //Original
        //return (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim );

        return (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim && (baseAnim.GetInteger("CurrentChain") < 1));

        //return (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim && (baseAnim.GetInteger("CurrentChain") < 1 &&  chainComboTimer <= 0)   );

        //return (isGrounded && !isAttackingAnim && !isJumpLandAnim && !isKnockedOut && !isHurtAnim && baseAnim.GetInteger("CurrentChain") < 1 && chainComboTimer <= 0);


    }

    //KnockdownRoutine() 실행 시에, rigidBody에 gravity 사용을 재개 하도록 한다.
    protected override IEnumerator KnockdownRoutine()
    {
        body.useGravity = true;
        return base.KnockdownRoutine();
    }

    //실제적으로 던져버리자고!
    public void runRollingBackThrow()
    {
        Vector3 throwVectorForce = new Vector3(this.grabAttackDirection.x * 3, 5, 0);
        grabbedActor.GetComponent<Rigidbody>().AddForce(throwVectorForce, ForceMode.Impulse);

        grabbedActor.baseAnim.SetBool("IsGrabbed", false); //던지기 실행 시, 던져지는 Enemy > Animation Bool(false)을 전달 하여 잡기가 끝났음을 전달.
        grabbedActor.EvaluateAttackData(throwAttack, this.grabAttackDirection, this.grabAttackStartPoint);
    }

    //오토파일럿 실행시 걷기 or 뛰기 결정
    public void AnimateTo(Vector3 position, bool shouldRun, Action callback)
    {
        if (shouldRun)
        {
            Run();
        }
        else
        {
            Walk();
        }

        walker.MoveTo(position, callback);
    }

    //오토파일럿 토글
    public void UseAutopilot(bool useAutoPilot)
    {
        isAutoPiloting = useAutoPilot;
        walker.enabled = useAutoPilot;
    }
}
