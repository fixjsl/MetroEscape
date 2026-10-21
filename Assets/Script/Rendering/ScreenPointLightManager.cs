using System.Collections.Generic;
using UnityEngine;

public class ScreenPointLightManager : SingletonBehaviour<ScreenPointLightManager>
{

    private const int MaxLights = 4;
    private readonly List<ScreenPointLight> lights = new();

    private readonly Vector4[] lightData   = new Vector4[MaxLights];
    private readonly Vector4[] lightColors = new Vector4[MaxLights];

    protected override void Awake()
    {
        base.Awake();
        Shader.SetGlobalFloat("_PointLightCount", 0f);
        Shader.SetGlobalVectorArray("_PointLightData", lightData);
        Shader.SetGlobalVectorArray("_PointLightColor", lightColors);
    }

    public void Register(ScreenPointLight light)
    {
        if (!lights.Contains(light)) lights.Add(light);
    }

    public void Unregister(ScreenPointLight light)
    {
        lights.Remove(light);
    }

    void LateUpdate()
    {
        Camera cam = Camera.main ?? Camera.current;
        if (cam == null) return;

        int count = Mathf.Min(lights.Count, MaxLights);
        for (int i = 0; i < count; i++)
        {
            var l   = lights[i];
            Vector3 vp = cam.WorldToViewportPoint(l.transform.position);
            lightData[i]   = new Vector4(vp.x, vp.y, l.Range, l.Intensity);
            lightColors[i] = l.LightColor;
        }
        for (int i = count; i < MaxLights; i++)
            lightData[i] = Vector4.zero;

        Shader.SetGlobalFloat("_PointLightCount", count);
        Shader.SetGlobalVectorArray("_PointLightData", lightData);
        Shader.SetGlobalVectorArray("_PointLightColor", lightColors);
    }
}
