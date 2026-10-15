// 피격 상태 전환 없이 HP만 바꿈 (지속 회복, 독 등). 공격 판정은 기존 OnHit 경로 사용
public interface IDamageable
{
    void Heal(float amount);
    void TakeDamage(float amount);
}
