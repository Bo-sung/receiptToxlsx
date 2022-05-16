using UnityEngine;
//using UnityEditor;   // AssetDatabase need, only for demo usage

public class ScreenWaterDistortion : MonoBehaviour
{
	public enum EWaterType { EWT_Corner, EWT_Splatter };
	public Material m_Mat;
	[System.Serializable] public class RippleParameters
	{
		[Range(0f, 36f)] public float Strength = 20f;
		[Range(0f, 1f)]  public float Relief = 0.3f;
		[Range(0f, 8f)]  public float Darkness = 2.5f;
		public Color Color = Color.white;
		public EWaterType Type = EWaterType.EWT_Corner;
	}
	public RippleParameters m_Parameters;
	[Header("Internal")]
	public Texture2D m_CornerNormal;
	public Texture2D m_CornerRelief;
	public Texture2D m_SplatterNormal;
	public Texture2D m_SplatterRelief;

	void Start ()
	{
		//m_CornerNormal = AssetDatabase.LoadAssetAtPath("Assets/Screen Ripple/Texture/Water Distortion Corner Normal.tga", typeof(Texture2D)) as Texture2D;
		//m_CornerRelief = AssetDatabase.LoadAssetAtPath("Assets/Screen Ripple/Texture/Water Distortion Corner.tga", typeof(Texture2D)) as Texture2D;
		//m_SplatterNormal = AssetDatabase.LoadAssetAtPath("Assets/Screen Ripple/Texture/Water Distortion Splatter Normal.tga", typeof(Texture2D)) as Texture2D;
		//m_SplatterRelief = AssetDatabase.LoadAssetAtPath("Assets/Screen Ripple/Texture/Water Distortion Splatter.tga", typeof(Texture2D)) as Texture2D;
	}
    void Update ()
	{
		m_Mat.SetFloat ("_Strength", m_Parameters.Strength);
		m_Mat.SetFloat ("_Relief", m_Parameters.Relief);
		m_Mat.SetFloat ("_Darkness", m_Parameters.Darkness);
		m_Mat.SetColor ("_Color", m_Parameters.Color);
		if (m_Parameters.Type == EWaterType.EWT_Corner)
		{
			m_Mat.SetTexture ("_NormalTex", m_CornerNormal);
			m_Mat.SetTexture ("_ReliefTex", m_CornerRelief);
		}
		if (m_Parameters.Type == EWaterType.EWT_Splatter)
		{
			m_Mat.SetTexture ("_NormalTex", m_SplatterNormal);
			m_Mat.SetTexture ("_ReliefTex", m_SplatterRelief);
		}
	}
	void OnRenderImage (RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit (src, dst, m_Mat);
	}
}
