using UnityEngine;

public class ScreenReflection : MonoBehaviour
{
	public Material m_Mat;
	[System.Serializable] public class RippleParameters
	{
		[Range(0f, 1f)]    public float Level = 0.2f;
		[Range(0f, 0.02f)] public float Amplitude = 0.005f;
		[Range(1f, 16f)]   public float Velocity = 3f;
		[Range(0f, 1f)]    public float ReflectionLevel = 0.4f;
		public Color Overlay = Color.white;
	}
	public RippleParameters m_Parameters;

    void Update ()
	{
		QualitySettings.antiAliasing = 0;  // hmmm...force disable AA since Y flip
		m_Mat.SetFloat ("_Amplitude", m_Parameters.Amplitude);
		m_Mat.SetFloat ("_Level", m_Parameters.Level);
		m_Mat.SetFloat ("_ReflectionLevel", m_Parameters.ReflectionLevel);
		m_Mat.SetFloat ("_Velocity", m_Parameters.Velocity);
		m_Mat.SetColor ("_Overlay", m_Parameters.Overlay);
	}
	void OnRenderImage (RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit (src, dst, m_Mat);
	}
}
