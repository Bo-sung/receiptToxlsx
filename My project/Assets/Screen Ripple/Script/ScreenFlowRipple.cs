using UnityEngine;

public class ScreenFlowRipple : MonoBehaviour
{
	public Material m_Mat;
	[System.Serializable] public class RippleParameters
	{
		[Range(0f, 1f)]   public float SizeX = 0.33f;
		[Range(0f, 1f)]   public float SizeY = 0.48f;
		[Range(1f, 8f)]   public float Speed = 3.6f;
		[Range(32f, 64f)] public float Distortion = 45f;
		public Color Overlay = Color.white;
	}
	public RippleParameters m_Parameters;

    void Update ()
	{
		m_Mat.SetFloat ("_SizeX", m_Parameters.SizeX);
		m_Mat.SetFloat ("_SizeY", m_Parameters.SizeY);
		m_Mat.SetFloat ("_Speed", m_Parameters.Speed);
		m_Mat.SetFloat ("_Distortion", m_Parameters.Distortion);
		m_Mat.SetColor ("_Overlay", m_Parameters.Overlay);
	}
	void OnRenderImage (RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit (src, dst, m_Mat);
	}
}
