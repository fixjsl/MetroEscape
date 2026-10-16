using UnityEngine;

namespace BossStates
{
    public class BossBattle : BossState
    {
        private TimeManager attackTimer = new TimeManager();
        private float delay;
        private bool readyToAttack;
        private bool isApproaching;


    
        public BossBattle(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            if (Boss.Targetplayer == null) { Boss.ChangeState<BossReturn>(); return; }

            Boss.Rb.linearVelocity = new Vector3(0f, Boss.Rb.linearVelocity.y, 0f);

            if (Boss.PreserveBattleTimer)
            {
                Boss.PreserveBattleTimer = false;
            }
            else if (!Boss.IsReturningFromAttack)
            {
                delay = Mathf.Max(0.5f, Boss.status.atkdelay + Random.Range(-1f, 1f));
                attackTimer.Reset();
            }

            if (Boss.IsReturningFromAttack)
            {
                Boss.IsReturningFromAttack = false;
                readyToAttack = false;
                isApproaching = true;
                Boss.animator.CrossFade(Boss.move, 0.1f);
            }
            else
            {
                readyToAttack = true;
                Boss.animator.CrossFade(Boss.battle, 0.1f);
            }
        }

        public override void Exit() { }

        public override void LogicUpdate()
        {
            if (Boss.Targetplayer == null) { Boss.ChangeState<BossReturn>(); return; }
            if (!Boss.IsInBattleRange()) { Boss.ChangeState<BossChase>(); return; }

            float dist = Mathf.Abs(Boss.Targetplayer.transform.position.x - Boss.transform.position.x);

            if (readyToAttack)
            {
                if (dist <= Boss.status.minSeparation)
                {
                    if (Boss.IsSpecialReady && Random.value < Boss.SpecialTriggerChance)
                        Boss.ChangeState<BossSpecial>();
                    else
                        Boss.ChangeState<BossAttack>();
                }
                return;
            }

            if (attackTimer.Timer(delay))
            {
                readyToAttack = true;
                Boss.animator.CrossFade(Boss.battle, 0.1f);
            }

            if (isApproaching && dist <= Boss.status.minSeparation)
                isApproaching = false;
            else if (!isApproaching && dist > Boss.status.maxSeparation)
                isApproaching = true;
        }

        public override void PhysicalUpdate()
        {
            if (Boss.Targetplayer == null) return;

            float playerX = Boss.Targetplayer.transform.position.x;
            float myX = Boss.transform.position.x;
            float dist = Mathf.Abs(playerX - myX);

            float targetY = (playerX >= myX) ? 90f : -90f;
            Boss.Rb.rotation = Quaternion.Euler(0f, targetY, 0f);

            float moveX;
            if (readyToAttack || isApproaching)
                moveX = (playerX >= myX) ? 1f : -1f;
            else
                moveX = (playerX >= myX) ? -1f : 1f;

            float speed = readyToAttack ? Boss.status.speed : Boss.status.battleWalkSpeed;
            Boss.Rb.linearVelocity = new Vector3(moveX * speed, Boss.Rb.linearVelocity.y, 0f);
        }
    }
}
