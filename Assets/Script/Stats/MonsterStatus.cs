using System;
using UnityEngine;
[CreateAssetMenu(fileName = "NewMonsterStatus", menuName = "MonsterStatus")]
public class MonsterStatus : ScriptableObject
{
    public string Name;
    public float Maxhp;
    private float hp;
    public float Hp
    {
        get => hp;
        set
        {
            bool wasAlive = hp > 0;
            hp = Mathf.Clamp(value, 0, Maxhp);
            ChangeHP?.Invoke(hp);
            if (wasAlive && hp <= 0) OnDie?.Invoke();
        }
    }
    public float speed;
    public float chasespeed;
    public float atk;
    public float atk_range;
    public float atkdelay;

    public Vector3 detectHalfExtents;
    private float _detection_gauge;
    public float detection_gauge
    {
        get => _detection_gauge;
        set
        {
            _detection_gauge = Mathf.Clamp(value, 0, 100);
            ChangeDetectionGauge?.Invoke(_detection_gauge);
        }
    }
    public float recovery;
    public float detectionSpeed = 10f;
    public float instantDetectRange = 10f;

    public Vector3 battleHalfExtents;
    public float stunTime;

    public float knockbackForce;
    public float minSeparation = 20f;
    public float maxSeparation = 25f;
    public float battleWalkSpeed = 2f;
    public string[] attackAnimNames = { "attack1", "attack2", "attack3" };
    private int[] _attackHashes;
    public int[] AttackHashes => _attackHashes ??= System.Array.ConvertAll(attackAnimNames, UnityEngine.Animator.StringToHash);

    public event Action<float> ChangeHP;
    public event Action<float> ChangeDetectionGauge;
    public event Action OnDie;
}