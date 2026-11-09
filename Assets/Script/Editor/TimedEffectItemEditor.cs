using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// TimedEffectItem의 [SerializeReference] effects 리스트를 편집하는 인스펙터
// 기본 인스펙터는 리스트에 null만 추가할 수 있어서, 효과 종류를 골라 추가하는 메뉴를 제공함
[CustomEditor(typeof(ConsumableItem))]
public class TimedEffectItemEditor : Editor
{
    private SerializedProperty effectsProp;
    private static Type[] effectTypes;

    private void OnEnable()
    {
        effectsProp = serializedObject.FindProperty("effects");

        // TimedEffect를 상속한 클래스 목록 (새 효과 클래스를 만들면 자동으로 추가됨)
        effectTypes ??= TypeCache.GetTypesDerivedFrom<TimedEffect>()
            .Where(t => !t.IsAbstract && !t.IsGenericType)
            .OrderBy(t => t.Name)
            .ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "effects");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("효과", EditorStyles.boldLabel);

        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            if (DrawEffect(effectsProp.GetArrayElementAtIndex(i), i))
            {
                effectsProp.DeleteArrayElementAtIndex(i);
                break;
            }
        }

        if (GUILayout.Button("효과 추가"))
            ShowAddMenu();

        serializedObject.ApplyModifiedProperties();
    }

    // 효과 하나를 상자로 그림. 삭제 버튼을 눌렀으면 true
    private bool DrawEffect(SerializedProperty element, int index)
    {
        bool remove = false;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string typeName = element.managedReferenceValue?.GetType().Name ?? "(비어 있음)";
                EditorGUILayout.LabelField($"{index}. {typeName}", EditorStyles.boldLabel);
                if (GUILayout.Button("삭제", GUILayout.Width(50)))
                    remove = true;
            }

            if (!remove)
            {
                EditorGUI.indentLevel++;
                SerializedProperty child = element.Copy();
                SerializedProperty end = element.GetEndProperty();
                if (child.NextVisible(true))
                {
                    while (!SerializedProperty.EqualContents(child, end))
                    {
                        EditorGUILayout.PropertyField(child, true);
                        if (!child.NextVisible(false)) break;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        return remove;
    }

    private void ShowAddMenu()
    {
        var menu = new GenericMenu();
        foreach (Type type in effectTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                serializedObject.Update();
                int index = effectsProp.arraySize;
                effectsProp.arraySize++;
                effectsProp.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);
                serializedObject.ApplyModifiedProperties();
            });
        }
        if (effectTypes.Length == 0)
            menu.AddDisabledItem(new GUIContent("TimedEffect를 상속한 클래스가 없음"));
        menu.ShowAsContext();
    }
}
