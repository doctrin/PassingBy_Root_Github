using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Actor : MonoBehaviour {

    public bool isAlive = true;
    
    public Animator baseAnim;           //hero`s animator
    public Rigidbody body;              //rigidBody
    public SpriteRenderer shadowSprite; //그림자 스프라이트
    public SpriteRenderer baseSprite;   //Actor의 기본 스프라이트

    public float speed = 5;             //현재 속도    
    protected Vector3 frontVector;      //hero가 보고 있는 방향

    public bool isGrounded;             //Hero가 floor와 충돌했는지 체크 var

    public GameObject suplexDestination; //Suplex 도달 목표지점; Suplex가 있는 캐릭의 포물선 운동 도달 지점. 

    public float maxLife = 100.0f;
    public float currentLife = 100.0f;

    //public float attackDamage = 10f; //기본 공격 데미지

    //private Vector3 firstShadowPosition; //필요없는 것으로 보여 우선 주석 처리함

    public AttackData normalAttack;  //기본 공격 데이터 레퍼런스용

    protected Coroutine knockdownRoutine;
    public bool isKnockedOut;

    public Actor grabbedActor; //잡힌 Actor 변수
    public Vector3 grabAttackStartPoint;
    public Vector3 grabAttackDirection;

    //Effect 관련 변수
    [Header("+ Effect var.")]
    public GameObject hitWeakSparkPrefab;
    public GameObject hitMiddleSparkPrefab;
    public GameObject hitStrongSparkPrefab;
    public GameObject hitWeakGroundSmashPrefab;
    public GameObject hitMiddleGroundSmashPrefab;
    public GameObject hitStrongGroundSmashPrefab;

    [Header("+ Hit Sound.")]
    public AudioClip hitSound1_2;
    public AudioClip hitSound3;
    public AudioClip hitSound4;

    [Header("+ Knockout Bouncing")]
    public Vector3 takedHitVector;
    public int knockoutBouncingCnt = 0;

    protected virtual void Start()
    {
        currentLife = maxLife;
        isAlive = true;
        baseAnim.SetBool("IsAlive", isAlive);
    }

    public virtual void Update()
    {

        //우선, Layer가 Enemy(9) 라면, 그림자 관련 위치 조정은 하지 않도록 한다.
        //--> 아래의 if 문이 없어져서 Enemy도 그림자 관련 위치를 조정하게 되면
        //    그림자가 Enemy 객체 아래에 위치하게 되어, Enemy가 붕 떠있는 것 처럼 보인다.
        if (shadowSprite.gameObject.layer.Equals(9))
        {
            return;
        }

        //Jump 시에 그림자 transform.y 값 고정
        Vector3 shadowSpritePosition = shadowSprite.transform.position;
        shadowSpritePosition.y = 0f;
        shadowSprite.transform.position = shadowSpritePosition;

        //knockdown bouncing --> 일단 Robot.cs 에서 구현함. 왜인지 모르겠지만, 여기서는 작동을 하지 않는다.
                            
    }

    //바닥(floor)과 충돌 시 발동. 착지 체크.
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name.Equals("Floor"))
        {
            isGrounded = true;
            baseAnim.SetBool("IsGrounded", isGrounded);

            //착지 애니메이션 진행중일때에는 Walk로 되지 않도록 조정 --> 해봤지만 별 효과가 없음.
            //if ( baseAnim.GetCurrentAnimatorStateInfo(0).IsName("jump_land") || baseAnim.GetCurrentAnimatorStateInfo(0).IsName("dash_jump_land") )
            //{
            DidLand();
            //}
        }
    }

    //바닥(floor)과 떨어질 때 발동. 점프 체크.
    protected virtual void OnCollisionExit(Collision collision)
    {
        if (collision.collider.name.Equals("Floor"))
        {
            isGrounded = false;
            baseAnim.SetBool("IsGrounded", isGrounded);
        }
    }

    //착지하면 바로 걷기 상태로 전환.
    protected virtual void DidLand()
    {
        
    }

    //스프라이트 좌우 반전
    public void FlipSprite(bool isFacingLeft)
    {
        if (isFacingLeft)
        {
            frontVector = new Vector3(-1, 0, 0);
            this.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            frontVector = new Vector3(1, 0, 0);
            this.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    //공격 : animator에 Attack trigger 전달
    public virtual void Attack()
    {
        baseAnim.SetTrigger("Attack");
    }

    /// <summary>
    /// Actor가 어떤 GameObject를 공격했을 때 에 발동
    /// </summary>
    /// <param name="collider">공격을 '당한' gameObject의 collider</param>
    /// <param name="hitPoint">공격을 '당한' 지점</param>
    /// <param name="hitVector">공격을 '당한' 방향</param>
    public virtual void DidHitObject(Collider collider, Vector3 hitPoint, Vector3 hitVector)
    {
        Actor actor = collider.GetComponent<Actor>();
        
        if (actor != null 
            && actor.CanBeHit()
            && collider.tag != gameObject.tag
            )
        {


            if (collider.attachedRigidbody != null)
            {
                HitActor(actor, hitPoint, hitVector);
            }
        }
    }

    /// <summary>
    /// Actor가 어떤 GameObject를 잡은 상태에서, 타격 버튼을 눌렀을 경우 (현재는 only 타격)
    /// 추후 잡은 상태에서 던지기, 특수 커멘드 입력으로 인한 특수 공격도 가능하도록 구현 예정
    /// </summary>
    public virtual void DidGrabHitObject(Collider collider, Vector3 hitPoint, Vector3 hitVector)
    {
        //위의 DidHitObject를 그냥 활용하면 편하지 않을까 한다. 
    }

    /// <summary>
    /// 공격을 '당한' Actor 에게 데미지 누적
    /// </summary>
    /// <param name="actor">공격을 '당한' Actor</param>
    /// <param name="hitPoint">공격을 '당한' 지점</param>
    /// <param name="hitVector">공격을 '당한' 방향</param>
    protected virtual void HitActor(Actor actor, Vector3 hitPoint, Vector3 hitVector)
    {

        Debug.Log("- PARENT [HitActor] method !!!! --------------------");


        //Debug.Log("---> "+gameObject.name + " HIT " + actor.gameObject.name);
        actor.EvaluateAttackData(normalAttack, hitVector, hitPoint);
    }

    /// <summary>
    /// Actor가 어떤 GameObj를 잡았을 때에 발동
    /// </summary>
    /// <param name="collider">잡힘을 '당한' gameObject의 collider</param>
    /// <param name="hitPoint">잡힘을 '당한' 지점</param>
    /// <param name="hitVector">잡힘을 '당한' 방향</param>
    public virtual void DidGrabObject(Collider collider, Vector3 hitPoint, Vector3 hitVector)
    {
        //Grabbed Actor : 잡힘을 '당한' Actor
        Actor actor = collider.GetComponent<Actor>();

        if (actor != null
            && actor.CanBeGrab()
            && collider.tag != gameObject.tag
            )
        {
            if (collider.attachedRigidbody != null)
            {
                GrabActor(actor, hitPoint, hitVector);
            }
        }
    }

    /// <summary>
    /// 잡힘을 '당한' Actor을, 잡힌 상태로 변경 해 줌.
    /// - Hero.cs 에서 overriding 해서 Hero의 입장에서는 거기서 GrabActor 실행 되고,
    ///   잡힌놈의 ToGrabSates 실행 됨.
    /// </summary>
    /// <param name="actor">잡힘을 '당한' Actor</param>
    /// <param name="grabPoint">잡힘을 '당한' 지점</param>
    /// <param name="grabVector">잡힘을 '당한' 방향</param>
    protected virtual void GrabActor(Actor actor, Vector3 grabPoint, Vector3 grabVector)
    {

        //잡힌 놈에게 잡힌 상태로 만드는 메소드 실행!
        actor.ToGrabbedState(grabVector ,grabPoint);
        
    }

    //잡힌놈을 잡힌 상태로 만들어줌
    //grabPoint 공격을 받은 지점을 나타냄. (아마도 추후에 사용하리라 짐작)
    public virtual void ToGrabbedState(Vector3 grabVector, Vector3 grabPoint)
    {
        //잡힌 방향으로 뒤집고,
        FlipSprite(grabVector.x > 0); 
        
        //잡힌 상태의 Trigger를 날려서 잡힌 스프라이트로 변경.
        this.baseAnim.SetTrigger("Grabbed");

        //잡혔다는 Bool(true)을 Enemy > Animation에 전달.
        this.baseAnim.SetBool("IsGrabbed", true);

    }

    //Actor 죽을 때 발동
    protected virtual void Die()
    {
        //knockdownRoutine 이 있다면 코루틴을 정지 한다.
        if (knockdownRoutine != null)
        {
            StopCoroutine(knockdownRoutine);
        }

        isAlive = false;
        baseAnim.SetBool("IsAlive", isAlive);
        StartCoroutine(DeathFlicker());
    }

    //Sprite의 opacity 변경
    protected virtual void SetOpacity(float value)
    {
        Color color = baseSprite.color;
        color.a = value;
        baseSprite.color = color;
    }

    //Coroutine 실행
    private IEnumerator DeathFlicker()
    {
        int i = 5;
        while (i > 0)
        {
            SetOpacity(0.5f);
            yield return new WaitForSeconds(0.1f);
            SetOpacity(1.0f);
            yield return new WaitForSeconds(0.1f);

            i--;
        }
    }

    //데미지를 입었을 때 실행
    public virtual void TakeDamage(float value, Vector3 hitVector, bool knockdown = false)
    {
        FlipSprite(hitVector.x > 0); //맞은 방향으로 뒤집고,
        currentLife -= value; //데미지를 누적 시킨다.


        //살아있고 && 현재 체력이 0과 같거나 작아지면 Die() 실행.
        if (isAlive && currentLife <= 0)
        {
            Die();
        }
        else if (knockdown) 
        {
            //knockdown 공격 인 경우
            if (knockdownRoutine == null)
            {
                Vector3 pushbackVector = (hitVector + Vector3.up * 0.75f).normalized;
                body.AddForce(pushbackVector * 250);
                knockdownRoutine = StartCoroutine(KnockdownRoutine());
            }
        }
        else
        {
            baseAnim.SetTrigger("IsHurt"); //그렇지 않다면 IsHurt trigger setting -> Transfer to Animator 
        }
    }

    //Actor가 때릴 수 있는 상태인지 Bool 반환
    public bool CanBeHit()
    {
        return isAlive && !isKnockedOut;
    }

    //Actor가 잡을 수 있는 상태인지 Bool 반환
    public bool CanBeGrab()
    {
        return isAlive && !isKnockedOut;
    }

    //Actor가 걸을 수 있는 (이동할 수 있는) 상태인지 Bool 반환 ; Default는 true.
    public virtual bool IsCanWalk()
    {
        return true;
    }

    public virtual void FaceTarget(Vector3 targetPoint)
    {
        FlipSprite(transform.position.x - targetPoint.x > 0);
    }


    //AttackData 의 force 만큼, hitVector 방향으로 밀어내고, 
    //hitPoint는 공격을 받은 지점을 나타냄.
    public virtual void EvaluateAttackData(AttackData data, Vector3 hitVector, Vector3 hitPoint)
    {
        if (!data.knockdown)
        {
            //일반공격
            body.AddForce(data.force * hitVector);
        }
        else
        {
            //마지막 공격일 경우 한 번 띄워본다.
            //Vector3 throwVectorForce = new Vector3(this.grabAttackDirection.x * 3, 5, 0);

            //Original
            //Vector3 throwVectorForce = new Vector3(hitVector.x * 3, 5, 0);  

            //Test
            Vector3 throwVectorForce = new Vector3(hitVector.x * 5, 3, 0);

            body.AddForce(throwVectorForce, ForceMode.Impulse);
            isGrounded = false;
        }

        TakeDamage(data.attackDamage, hitVector, data.knockdown);

        //Effect 발생
        ShowHitEffects(data, hitPoint);

        //Hit Sound play
        PlayHitSound(data);

        //if(!data.knockdown) //날리기 공격이 아니라면, Shake 발생
        if (data.name.Equals("normalAttack3")) //3타 째에만 Shake 발생
        {
            DOTween.Init(); //DOTween 객체 생성

            //this.transform.DOShakePosition(0.37f, 0.3f, 30, 30, false, true); // 360도 랜덤 Shake
            this.transform.DOPunchPosition(this.transform.right, 0.23f, 10000, 1f, false); // 현재 Actor(피격 '당한' 객체) 의 오른쪽, 왼쪽으로 Shake

        }

        //hitVector 저장
        this.takedHitVector = hitVector;
    }

    //Actor의 get-up animation 끝날 때 Call.
    public void DidGetUp()
    {
        isKnockedOut = false;
        knockoutBouncingCnt = 0; //knockoutBouncingCnt 초기화 (정상적인 knockoutBouncing 을 위해서)
    }

    protected virtual IEnumerator KnockdownRoutine()
    {
        isKnockedOut = true;
        baseAnim.SetTrigger("Knockdown");

        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);

        baseAnim.SetTrigger("GetUp");
        knockdownRoutine = null;
    }

    //Hit Effect 표시
    protected void ShowHitEffects(AttackData attackData, Vector3 hitPosition)
    {
        GameObject hitEffectObj = null;
        
        if (attackData.name.Equals("normalAttack1") || attackData.name.Equals("normalAttack2"))
        {
            hitEffectObj = Instantiate(hitWeakSparkPrefab);
        }
        else if (attackData.name.Equals("normalAttack3"))
        {
            hitEffectObj = Instantiate(hitMiddleSparkPrefab);
        }
        else if (attackData.name.Equals("normalAttack4"))
        {
            hitEffectObj = Instantiate(hitStrongSparkPrefab);
        }


        hitPosition.y = hitPosition.y + 3; //발생 위치 상향

        if(hitEffectObj != null) hitEffectObj.transform.position = hitPosition;

    }

    //Hit Sound Play
    protected void PlayHitSound(AttackData attackData)
    {

        AudioSource audio = GetComponent<AudioSource>();

        //audio.volume = 1.0f;
        //if (audio.isPlaying)
        //{
        //    audio.Stop();
        //}

        if (attackData.name.Equals("normalAttack1") || attackData.name.Equals("normalAttack2"))
        {
            audio.PlayOneShot(hitSound1_2);
        }
        else if (attackData.name.Equals("normalAttack3"))
        {
            audio.PlayOneShot(hitSound3);
        }
        else if (attackData.name.Equals("normalAttack4"))
        {
            audio.PlayOneShot(hitSound4);
        }
    }

}

[System.Serializable]
public class AttackData
{
    public string name = "";
    public float attackDamage = 10;
    public float force = 50;
    public bool knockdown = false;
}

/*
        //extension methods to allow that usage:
        public static class AnimatorExtension
        {

            public static void SetTriggerOneFrame(this Animator anim, MonoBehaviour coroutineRunner, string trigger)
            {
                coroutineRunner.StartCoroutine(TriggerOneFrame(anim, trigger));
            }

            private static IEnumerator TriggerOneFrame(Animator anim, string trigger)
            {
                anim.SetTrigger(trigger);
                yield return null;
                if (anim != null)
                {
                    anim.ResetTrigger(trigger);
                }
            }
        }
*/