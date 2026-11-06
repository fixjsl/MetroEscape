using UnityEngine;

// 플레이어 애니메이션 상태 해시 (PlayerStateMachine에서 이름 그대로 옮김)
public static class PlayerAnim
{
    public static readonly int idle  = Animator.StringToHash("idle");
    public static readonly int move = Animator.StringToHash("move");
    public static readonly int moveTurn = Animator.StringToHash("moveTurn");
    public static readonly int hit  = Animator.StringToHash("hit");
    public static readonly int die = Animator.StringToHash("die");
    public static readonly int tired = Animator.StringToHash("tired");
    public static readonly int[] attackHashes = {
    Animator.StringToHash("attack1"),
    Animator.StringToHash("attack2"),
    Animator.StringToHash("attack3")
    };
    public static readonly int execution = Animator.StringToHash("execution");
    public static readonly int sprint = Animator.StringToHash("sprint");
    public static readonly int sprintTurn = Animator.StringToHash("sprintTurn");
    public static readonly int incrunch = Animator.StringToHash("incrunch");
    public static readonly int outcrunch = Animator.StringToHash("outcrunch");
    public static readonly int crunchturn = Animator.StringToHash("crunchturn");
    public static readonly int crunch = Animator.StringToHash("crunch");
    public static readonly int crunchmove = Animator.StringToHash("crunchmove");
    public static readonly int parrysucess = Animator.StringToHash("parrysucess");
    public static readonly int parrying = Animator.StringToHash("parrying");
    public static readonly int guard = Animator.StringToHash("guard");
    public static readonly int dodge = Animator.StringToHash("dodge");
}
