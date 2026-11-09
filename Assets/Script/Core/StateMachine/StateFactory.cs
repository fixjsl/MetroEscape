using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StateFactory
{
    private readonly static Dictionary<Type, Type[]> cache = new Dictionary<Type, Type[]>();

    public static List<TState> CreateAll<TState>(object owner, Func<Type,bool> filter = null) where TState : State
    {
        Type baseType = typeof(TState);

        // ① ② 캐시에 없을 때만 GetTypes()
        if (!cache.TryGetValue(baseType, out Type[] types))
        {
            // 상태 클래스는 부모(PlayerState·MonsterState)와 같은 어셈블리(Game)에 있음.
            // GetExecutingAssembly()는 이 파일이 있는 Core를 가리켜서 asmdef 분리 후엔 아무것도 못 찾음
            types = baseType.Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract)
                .ToArray();
            cache[baseType] = types;
        }
        var result = new List<TState>();
        foreach (Type type in types)
        {
            if (filter != null && !filter(type)) continue;
            try
            {
                result.Add((TState)Activator.CreateInstance(type, owner));
            }
            catch (Exception e)
            {
                Debug.LogError($"{type.Name} 생성 실패 (생성자 확인): {e.Message}");
            }
        }
        return result;
    }
}
