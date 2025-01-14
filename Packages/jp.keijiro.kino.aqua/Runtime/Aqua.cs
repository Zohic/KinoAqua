using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using SerializableAttribute = System.SerializableAttribute;

namespace Kino.PostProcessing {

[Serializable, VolumeComponentMenu("Post-processing/Kino/Aqua")]
public sealed class Aqua : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    #region Effect parameters

    public ClampedFloatParameter opacity = new ClampedFloatParameter(0, 0, 1);
    [Space]
    public ColorParameter edgeColor = new ColorParameter(Color.black);
    public ClampedFloatParameter edgeContrast = new ClampedFloatParameter(1.2f, 0.01f, 4);
    [Space]
    public ColorParameter fillColor = new ColorParameter(Color.white);
    public ClampedFloatParameter blurWidth = new ClampedFloatParameter(1, 0, 2);
    public ClampedFloatParameter blurFrequency = new ClampedFloatParameter(0.5f, 0, 1);
    public ClampedFloatParameter hueShift = new ClampedFloatParameter(0.1f, 0, 0.3f);
    [Space]
    public ClampedFloatParameter interval = new ClampedFloatParameter(1, 0.1f, 5);
    public ClampedIntParameter iteration = new ClampedIntParameter(20, 4, 32);

    #endregion

    #region Private members

    static class ShaderIDs
    {
        public static int EffectParams1 = Shader.PropertyToID("_EffectParams1");
        public static int EffectParams2 = Shader.PropertyToID("_EffectParams2");
        public static int EdgeColor = Shader.PropertyToID("_EdgeColor");
        public static int FillColor = Shader.PropertyToID("_FillColor");
        public static int Iteration = Shader.PropertyToID("_Iteration");
        public static int InputTexture = Shader.PropertyToID("_InputTexture");
        public static int NoiseTexture = Shader.PropertyToID("_NoiseTexture");
    }

    Material _material;

    #endregion

    #region Ttexture asset (via Resources)

    static Texture2D _noiseTexture;

    static Texture2D NoiseTexture
      => _noiseTexture = _noiseTexture ?? Resources.Load<Texture2D>("KinoAquaNoise");

    #endregion

    #region IPostProcessComponent implementation

    public bool IsActive()
      => _material != null && opacity.value > 0;

    #endregion

    #region CustomPostProcessVolumeComponent implementation

    public override CustomPostProcessInjectionPoint injectionPoint
      => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
      => _material = CoreUtils.CreateEngineMaterial("Hidden/Kino/PostProcess/Aqua");

    public override void Render
      (CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
    {
        var bfreq = Mathf.Exp((blurFrequency.value - 0.5f) * 6);

        _material.SetVector(ShaderIDs.EffectParams1,
          new Vector4(opacity.value, interval.value,blurWidth.value, bfreq));

        _material.SetVector(ShaderIDs.EffectParams2,
          new Vector2(edgeContrast.value, hueShift.value));

        _material.SetColor(ShaderIDs.EdgeColor, edgeColor.value);
        _material.SetColor(ShaderIDs.FillColor, fillColor.value);
        _material.SetInt(ShaderIDs.Iteration, iteration.value);
        _material.SetTexture(ShaderIDs.InputTexture, srcRT);
        _material.SetTexture(ShaderIDs.NoiseTexture, NoiseTexture);

        HDUtils.DrawFullScreen(cmd, _material, destRT, null, 0);
    }

    public override void Cleanup()
      => CoreUtils.Destroy(_material);

    #endregion
}

} // namespace Kino.PostProcessing
