using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            types = Assembly.GetExecutingAssembly().GetTypes()
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
