using MonsterStates;
using UnityEngine;

namespace MonsterStates
{
public class Battle : MonsterState
{
    private TimeManager Timer = new TimeManager();
    private float delay;
    private bool readyToAttack;
    private bool isApproaching;



    public Battle(MonsterStateMachine monster) : base(monster) { }

    public override void Enter()
    {
        if (Monster.Targetplayer == null) { Monster.ChangeState<Return>(); return; }

        Monster.Rb.linearVelocity = new Vector3(0f, Monster.Rb.linearVelocity.y, 0f);
        delay = Mathf.Max(0.5f, Monster.status.atkdelay + Random.Range(-1f, 1f));
        Timer.Reset();
        if (Monster.IsReturningFromAttack)
        {
            Monster.IsReturningFromAttack = false;
            readyToAttack = false;
            isApproaching = true;
            Monster.animator.speed = 0.7f;  
            Monster.animator.CrossFade(Monster.move, 0.1f);
        }
        else
        {
            readyToAttack = true;
            isApproaching = false;
            Monster.animator.CrossFade(Monster.battle, 0.1f);
        }
    }

    public override void Exit() { }

    public override void LogicUpdate()
    {
        if (Monster.Targetplayer == null) { Monster.ChangeState<Return>(); return; }
        if (!Monster.IsInBattleRange()) { Monster.ChangeState<Chase>(); return; }

        float dist = Mathf.Abs(Monster.Targetplayer.transform.position.x - Monster.transform.position.x);

        if (readyToAttack)
        {
            if (dist <= Monster.status.minSeparation)
                Monster.ChangeState<Attack>();
            return;
        }

        if (Timer.Timer(delay))
        {
            readyToAttack = true;
            Monster.animator.speed = 1f;
            Monster.animator.CrossFade(Monster.battle, 0.1f);
        }

        // 접근/후퇴 전환
        if (isApproaching && dist <= Monster.status.minSeparation)
            isApproaching = false;
        else if (!isApproaching && dist > Monster.status.maxSeparation)
            isApproaching = true;
    }

    public override void PhysicalUpdate()
    {
        if (Monster.Targetplayer == null) return;

        float playerX = Monster.Targetplayer.transform.position.x;
        float myX = Monster.transform.position.x;
        float dist = Mathf.Abs(playerX - myX);

        float targetY = (playerX >= myX) ? 90f : -90f;
        Monster.Rb.rotation = Quaternion.Euler(0f, targetY, 0f);

        float moveX;
        if (readyToAttack || isApproaching)
            moveX = (playerX >= myX) ? 1f : -1f;
        else
            moveX = (playerX >= myX) ? -1f : 1f;

        float speed = readyToAttack ? Monster.status.chasespeed : Monster.status.battleWalkSpeed;
        Monster.Rb.linearVelocity = new Vector3(moveX * speed, Monster.Rb.linearVelocity.y, 0f);
    }
}
}
