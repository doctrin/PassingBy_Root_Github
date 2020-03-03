using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroCallback : MonoBehaviour
{
    public Hero hero;
    public AudioClip whoosh1_2;
    public AudioClip whoosh3;
    public AudioClip whoosh4;

    //펀치 휘두르는 Sound 재생
    public void DidChain(int chain)
    {
        //체인콤보 index 전달
        hero.DidChain(chain);

        //공격 Sound PlayOneShot
        AudioSource audio = GetComponent<AudioSource>();
        //audio.volume = 1.0f;

        if (!audio.isPlaying)
        {
            if (chain == 1 || chain == 2)
            {
                audio.PlayOneShot(whoosh1_2);
            }
            else if (chain == 3)
            {
                audio.PlayOneShot(whoosh3);
            }
            else if (chain == 4)
            {
                audio.PlayOneShot(whoosh4);
            }
        }

    }

    public void DidChainComplete()
    {
        hero.AttackDataInit();
    }

    public void DidJumpAttack()
    {
        hero.DidJumpAttack();
    }

    public void DidGrabCahin(int chain)
    {
        hero.DidGrabChain(chain);
    }

    // 잡기 패기는 여기서 구현한다!
    public void DoGrabAttack(int grabAttackCount)
    {
        if (grabAttackCount == 1)
        {
            hero.AnalyzeNormalAttack(hero.grabAttack1, 0, hero.grabbedActor, hero.grabAttackStartPoint, hero.grabAttackDirection);
        }
        else if (grabAttackCount == 2)
        {
            hero.AnalyzeNormalAttack(hero.grabAttack2, 0, hero.grabbedActor, hero.grabAttackStartPoint, hero.grabAttackDirection);
        }
        else if (grabAttackCount == 3)
        {
            hero.AnalyzeNormalAttack(hero.grabAttack3, 0, hero.grabbedActor, hero.grabAttackStartPoint, hero.grabAttackDirection);
        }


    }

    //잡기상태 유지
    public void KeepGrab()
    {
        hero.baseAnim.SetBool("EnemyGrab", true);
    }

    //던져버려!
    public void DidThrow()
    {
        hero.GetComponent<Hero>().runRollingBackThrow();
    }

}
