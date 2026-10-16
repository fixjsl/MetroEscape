using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStatus", menuName = "Scriptable Objects/WeaponStatus")]
public class WeaponStatus : ScriptableObject
{
    [Header("공격 스탯")]
    public int attack; //공격력
    public float attackSpeed; //공격속도
    public float attackRange; //공격범위
    public float executionRange; //처형 범위
    public float execution_m; //처형 배율
    [Header("상태이상 및 특수효과")]
    public int bloodStrenth; //출혈 강도
    public float stunStrength;//기절 강도
    public float GuardStrength;//가드 경감율

    [Header("스테미나")]
    public int attackStamina; //공격 스태미나
    public int guardStamina;//가드 스태미나
    public int parryStamina;//패링 스태미나

    [Header("애니메이션")]
    public AnimatorOverrideController WeaponAnimations; // 애니메이션

    public Vector3 holdPosition; // 잡는 위치
    public Quaternion holdRotation; // 잡는 회전

    [Header("SFX")]
    public AudioClip attackSound;//공격 사운드
    public AudioClip parrySound;//패링 사운드
}
