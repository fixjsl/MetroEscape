using UnityEngine;

// 몬스터 공통 애니메이션 상태 해시 (MonsterStateMachine에서 이름 그대로 옮김)
// 몬스터별로 다른 공격 해시는 status.AttackHashes, 보스 전용 해시는 BossStateMachine에 있음
public static class MonsterAnim
{
    public static readonly int idle = Animator.StringToHash("idle");
    public static readonly int move = Animator.StringToHash("move");
    public static readonly int sprint = Animator.StringToHash("chase");
    public static readonly int battle = Animator.StringToHash("battle");
    public static readonly int hit = Animator.StringToHash("hit");
    public static readonly int die = Animator.StringToHash("die");
    public static readonly int moveTurn = Animator.StringToHash("moveTurn");
    public static readonly int stun = Animator.StringToHash("stun");
}
