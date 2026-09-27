using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class FlashlightRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material passMaterial;
    // 스프라이트/투명 오브젝트가 모두 그려진 뒤 처리
    [SerializeField] private RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
    private FlashlightPass _pass;

    public override void Create() => _pass = new FlashlightPass(passMaterial, passEvent);

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (passMaterial == null) return;
        renderer.EnqueuePass(_pass);
    }

    private class FlashlightPass : ScriptableRenderPass
    {
        private readonly Material _mat;

        private static readonly int BlitTextureID    = Shader.PropertyToID("_BlitTexture");
        private static readonly int BlitScaleBiasID  = Shader.PropertyToID("_BlitScaleBias");
        private static readonly int FlEnabledID     = Shader.PropertyToID("_FlashlightEnabled");
        private static readonly int FlPosID         = Shader.PropertyToID("_FlashlightPos");
        private static readonly int FlDirID         = Shader.PropertyToID("_FlashlightDir");
        private static readonly int FlRangeID       = Shader.PropertyToID("_FlashlightRange");
        private static readonly int FlOuterAngleID  = Shader.PropertyToID("_FlashlightOuterAngle");
        private static readonly int FlInnerAngleID  = Shader.PropertyToID("_FlashlightInnerAngle");
        private static readonly int FlIntensityID   = Shader.PropertyToID("_FlashlightIntensity");
        private static readonly int FlColorID       = Shader.PropertyToID("_FlashlightColor");
        private static readonly int PlCountID       = Shader.PropertyToID("_PointLightCount");
        private static readonly int PlDataID        = Shader.PropertyToID("_PointLightData");
        private static readonly int PlColorID       = Shader.PropertyToID("_PointLightColor");

        private const int MaxPointLights = 4;

        // 전역 벡터 배열을 되읽을 때 사용하는 버퍼.
        // Shader.GetGlobalVectorArray는 배열을 반환하는 형태로 호출하면 호출마다 새 배열을 할당하므로,
        // 리스트를 받는 형태로 호출하여 이 버퍼를 프레임마다 재사용한다.
        private static readonly List<Vector4> _plDataBuf  = new List<Vector4>(MaxPointLights);
        private static readonly List<Vector4> _plColorBuf = new List<Vector4>(MaxPointLights);

        public FlashlightPass(Material mat, RenderPassEvent evt)
        {
            _mat = mat;
            renderPassEvent = evt;
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        private class PassData
        {
            public Material        mat;
            public TextureHandle   source;
            public float           flEnabled;
            public Vector4         flPos;
            public Vector4         flDir;
            public float           flRange;
            public float           flOuterAngle;
            public float           flInnerAngle;
            public float           flIntensity;
            public Color           flColor;
            public float           plCount;
            // 렌더 그래프가 PassData 객체를 풀에서 재사용하므로 이 배열도 함께 재사용된다.
            public Vector4[]       plData  = new Vector4[MaxPointLights];
            public Vector4[]       plColor = new Vector4[MaxPointLights];
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_mat == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData   = frameData.Get<UniversalCameraData>();

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            // 현재 화면 스냅샷 복사
            TextureHandle sourceCopy  = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "FlashlightSourceCopy", false);
            TextureHandle activeColor = resourceData.activeColorTexture;
            renderGraph.AddBlitPass(activeColor, sourceCopy, Vector2.one, Vector2.zero);

            using var builder = renderGraph.AddRasterRenderPass<PassData>("FlashlightPass", out var passData, new ProfilingSampler("FlashlightPass"));
            builder.AllowGlobalStateModification(true);

            passData.mat        = _mat;
            passData.source     = sourceCopy;
            // 전역 값의 조회 또한 캐싱한 식별자를 사용하여 매 프레임 발생하는 문자열 해싱을 제거한다.
            passData.flEnabled  = Shader.GetGlobalFloat(FlEnabledID);
            passData.flPos      = Shader.GetGlobalVector(FlPosID);
            passData.flDir      = Shader.GetGlobalVector(FlDirID);
            passData.flRange      = Shader.GetGlobalFloat(FlRangeID);
            passData.flOuterAngle = Shader.GetGlobalFloat(FlOuterAngleID);
            passData.flInnerAngle = Shader.GetGlobalFloat(FlInnerAngleID);
            passData.flIntensity  = Shader.GetGlobalFloat(FlIntensityID);
            passData.flColor    = Shader.GetGlobalColor(FlColorID);

            passData.plCount = Shader.GetGlobalFloat(PlCountID);
            Shader.GetGlobalVectorArray(PlDataID,  _plDataBuf);
            Shader.GetGlobalVectorArray(PlColorID, _plColorBuf);
            for (int i = 0; i < MaxPointLights; i++)
            {
                passData.plData[i]  = i < _plDataBuf.Count  ? _plDataBuf[i]  : Vector4.zero;
                passData.plColor[i] = i < _plColorBuf.Count ? _plColorBuf[i] : Vector4.zero;
            }

            builder.SetRenderAttachment(activeColor, 0, AccessFlags.Write);
            builder.UseTexture(passData.source, AccessFlags.Read);

            builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
            {
                data.mat.SetTexture(BlitTextureID,   data.source);
                data.mat.SetVector(BlitScaleBiasID, new Vector4(1f, 1f, 0f, 0f));
                data.mat.SetFloat(FlEnabledID,      data.flEnabled);
                data.mat.SetVector(FlPosID,         data.flPos);
                data.mat.SetVector(FlDirID,         data.flDir);
                data.mat.SetFloat(FlRangeID,        data.flRange);
                data.mat.SetFloat(FlOuterAngleID,   data.flOuterAngle);
                data.mat.SetFloat(FlInnerAngleID,   data.flInnerAngle);
                data.mat.SetFloat(FlIntensityID,    data.flIntensity);
                data.mat.SetColor(FlColorID,        data.flColor);
                data.mat.SetFloat(PlCountID,        data.plCount);
                data.mat.SetVectorArray(PlDataID,   data.plData);
                data.mat.SetVectorArray(PlColorID,  data.plColor);

                ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, 0, MeshTopology.Triangles, 3);
            });
        }
    }
}
