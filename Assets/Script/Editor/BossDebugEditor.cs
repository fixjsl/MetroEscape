#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BossStates;

[CustomEditor(typeof(BossStateMachine))]
public class BossDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var boss = (BossStateMachine)target;
        if (!Application.isPlaying) return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("─── Debug: 상태 강제 전환 ───", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Idle"))    boss.ChangeState<MonsterStates.Idle>();
        if (GUILayout.Button("Chase"))   boss.ChangeState<MonsterStates.Chase>();
        if (GUILayout.Button("Battle"))  boss.ChangeState<MonsterStates.Battle>();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Attack"))  boss.ChangeState<MonsterStates.Attack>();
        if (GUILayout.Button("Special")) boss.ChangeState<BossSpecial>();
        if (GUILayout.Button("Hit"))
        {
            var hit = boss.fsm.Get<MonsterStates.Hit>() as MonsterStates.Hit;
            hit.SetHitduration(0.5f);
            boss.ChangeState<MonsterStates.Hit>();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Stun"))    boss.ChangeState<MonsterStates.Stun>();
        if (GUILayout.Button("Return"))  boss.ChangeState<MonsterStates.Return>();
        if (GUILayout.Button("Die"))     boss.ChangeState<MonsterStates.Die>();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("─── Debug: 애니메이션 직접 재생 ───", EditorStyles.boldLabel);

        if (boss.animator == null)
        {
            EditorGUILayout.HelpBox("animator가 null입니다.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("anim: idle"))    boss.animator.CrossFade(boss.idle, 0.01f);
            if (GUILayout.Button("anim: chase"))   boss.animator.CrossFade(boss.sprint, 0.01f);
            if (GUILayout.Button("anim: hit"))     boss.animator.CrossFade(boss.hit, 0.01f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (boss.AttackHashes != null && boss.AttackHashes.Length > 0 && GUILayout.Button("anim: atk1")) boss.animator.CrossFade(boss.AttackHashes[0], 0.01f);
            if (boss.AttackHashes != null && boss.AttackHashes.Length > 1 && GUILayout.Button("anim: atk2")) boss.animator.CrossFade(boss.AttackHashes[1], 0.01f);
            if (boss.AttackHashes != null && boss.AttackHashes.Length > 2 && GUILayout.Button("anim: atk3")) boss.animator.CrossFade(boss.AttackHashes[2], 0.01f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("anim: firstAtk")) boss.animator.CrossFade(boss.firstAttackHash, 0.01f);
            if (GUILayout.Button("anim: special"))  boss.animator.CrossFade(boss.specialHash, 0.01f);
            if (GUILayout.Button("anim: die"))      boss.animator.CrossFade(boss.die, 0.01f);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("─── 현재 상태 ───", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"ActiveState : {boss.ActiveState?.GetType().Name ?? "null"}");
        EditorGUILayout.LabelField($"IsFirstAttack : {boss.IsFirstAttack}");
        EditorGUILayout.LabelField($"IsSpecialReady : {boss.IsSpecialReady}");
        EditorGUILayout.LabelField($"HP : {boss.status?.Hp:F0} / {boss.status?.Maxhp}");

        Repaint();
    }
}
#endif
