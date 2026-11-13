using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public static class GameSceneLoadManager
{
    public static event Action<List<String>> LoadScene;


    private static void LoadingMap(List<String> scenes)
    {
        foreach(var i in scenes)
        {
            SceneFlow.LoadAdditive(i);
        }
        foreach (var i in Last_Scenes) ;
    }
}
