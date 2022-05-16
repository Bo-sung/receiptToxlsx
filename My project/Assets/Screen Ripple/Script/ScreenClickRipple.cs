using UnityEngine;

public class ScreenClickRipple : MonoBehaviour
{
	public Material[] m_Mat;
	public Camera m_Camera;
	public enum ERippleType { Ripple1 = 0, Ripple2, Ripple3 };
	public ERippleType m_RippleType = ERippleType.Ripple1;
	
	public string m_RippleMaskName = "TransparentFX";
	public Shader m_SdrRippleMask;
	public bool m_EnableMaskObjects = false;
	public bool m_ShowInternalMaps = false;
	public bool m_AntiAliasing = false;
	[System.Serializable] public class Ripple1
	{
		[Range(0.1f, 1f)]    public float Spread = 0.3f;
		[Range(0.1f, 2f)]    public float Amplitude = 0.8f;
		[Range(0.02f, 0.1f)] public float Gap = 0.1f;
	}
	[System.Serializable] public class Ripple2
	{
		[Range(0.01f, 0.06f)] public float StrengthInit = 0.03f;
		[Range(0.0001f, 0.0008f)] public float StrengthDecay = 0.0002f;
		[Range(0.1f, 1f)] public float Spread = 0.3f;
		[Range(0f, 64f)]  public float Frequence = 24f;
		[Range(8f, 32f)]  public float Velocity = 16f;
	}
	[System.Serializable] public class Ripple3
	{
		public float Frequence = 60f;
		public float Speed = 30f;
		public float Strength = 1f;
		public float WaveWidth = 0.3f;
		[Range(0.1f, 1f)] public float Spread = 0.3f;
	}
	[Header("Ripple Effect 1")]
	public Ripple1 m_Ripple1;
	[Header("Ripple Effect 2")]
	public Ripple2 m_Ripple2;
	[Header("Ripple Effect 3")]
	public Ripple3 m_Ripple3;
	
	public class RippleClick
	{
		public float MouseX = 0.5f;
		public float MouseY = 0.5f;
		public float Progress = 1f;
		public float Strength = 0f;  // ripple2 need
	}
	private RippleClick[] m_RippleClick = new RippleClick[3];
	private int m_CurrentRippleClick = 0;
	private Camera m_RTCam;
	private RenderTexture m_RTRippleMask;
	
	void Start ()
	{
		int n = m_RippleClick.Length;
		for (int i = 0; i < n; i++)
			m_RippleClick[i] = new RippleClick();
		
		// prepare ripple mask
		if (m_Camera == null)
			m_Camera = Camera.main;
		m_RTCam = new GameObject().AddComponent<Camera> ();
		m_RTCam.name = "Ripple Mask Camera";
		m_RTCam.transform.parent = m_Camera.gameObject.transform;
        m_RTCam.enabled = false;
		m_RTRippleMask = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
		m_RTRippleMask.name = "Ripple Mask";
	}
    void Update ()
	{
		if (m_AntiAliasing)
			QualitySettings.antiAliasing = 8;
		else
			QualitySettings.antiAliasing = 0;

		if (Input.GetMouseButtonUp (0))
		{
			float mouseX = Input.mousePosition.x / Screen.width;
			float mouseY = Input.mousePosition.y / Screen.height;
			if (m_AntiAliasing)
				mouseY = 1f - mouseY;  // yes, unity build-in AntiAliasing will flip y coordinate, so we have to flip it here.
			m_RippleClick[m_CurrentRippleClick].MouseX = mouseX;
			m_RippleClick[m_CurrentRippleClick].MouseY = mouseY;
			m_RippleClick[m_CurrentRippleClick].Progress = 0f;
			if (ERippleType.Ripple2 == m_RippleType)  // only give it strength when you are playing ripple2
				m_RippleClick[m_CurrentRippleClick].Strength = m_Ripple2.StrengthInit;
			m_CurrentRippleClick++;
			m_CurrentRippleClick = (m_CurrentRippleClick >= 3) ? 0 : m_CurrentRippleClick;
		}
			
		// build ripple mask
		if (m_EnableMaskObjects)
		{
			m_RTCam.CopyFrom (m_Camera);
			m_RTCam.clearFlags = CameraClearFlags.Color;
			m_RTCam.backgroundColor = Color.black;
			m_RTCam.targetTexture = m_RTRippleMask;
			m_RTCam.cullingMask = 1 << LayerMask.NameToLayer (m_RippleMaskName);
			m_RTCam.RenderWithShader (m_SdrRippleMask, "");
			m_Mat[0].EnableKeyword ("SR_MASK");
			m_Mat[0].SetTexture ("_MaskTex", m_RTRippleMask);
			m_Mat[1].EnableKeyword ("SR_MASK");
			m_Mat[1].SetTexture ("_MaskTex", m_RTRippleMask);
			m_Mat[2].EnableKeyword ("SR_MASK");
			m_Mat[2].SetTexture ("_MaskTex", m_RTRippleMask);
		}
		else
		{
			m_Mat[0].DisableKeyword ("SR_MASK");
			m_Mat[1].DisableKeyword ("SR_MASK");
			m_Mat[2].DisableKeyword ("SR_MASK");
		}
			
		// update click
		int n = m_RippleClick.Length;
		for (int i = 0; i < n; i++)
		{
			m_RippleClick[i].Progress += 0.01f;
			m_RippleClick[i].Strength = Mathf.Max(m_RippleClick[i].Strength - m_Ripple2.StrengthDecay, 0f);
		}
		m_Mat[0].SetFloat ("_Spread", m_Ripple1.Spread);
		m_Mat[0].SetFloat ("_Amplitude", m_Ripple1.Amplitude);
		m_Mat[0].SetFloat ("_Gap", m_Ripple1.Gap);
		m_Mat[1].SetFloat ("_Spread", m_Ripple2.Spread);
		m_Mat[1].SetFloat ("_Frequence", m_Ripple2.Frequence);
		m_Mat[1].SetFloat ("_Velocity", m_Ripple2.Velocity);
		m_Mat[2].SetFloat ("_Frequence", m_Ripple3.Frequence);  
		m_Mat[2].SetFloat ("_Speed", m_Ripple3.Speed);
		m_Mat[2].SetFloat ("_Strength", m_Ripple3.Strength);
		m_Mat[2].SetFloat ("_WaveWidth", m_Ripple3.WaveWidth);
		m_Mat[2].SetFloat ("_CurWaveDis", m_RippleClick[0].Progress);
		m_Mat[2].SetFloat ("_Spread", m_Ripple3.Spread);
			
		// send click data to ripple shader
		m_Mat[0].SetVector ("_Ripple1", new Vector4 (m_RippleClick[0].MouseX, m_RippleClick[0].MouseY, m_RippleClick[0].Progress, 0));
		m_Mat[0].SetVector ("_Ripple2", new Vector4 (m_RippleClick[1].MouseX, m_RippleClick[1].MouseY, m_RippleClick[1].Progress, 0));
		m_Mat[0].SetVector ("_Ripple3", new Vector4 (m_RippleClick[2].MouseX, m_RippleClick[2].MouseY, m_RippleClick[2].Progress, 0));
		m_Mat[1].SetVector ("_Ripple1", new Vector4 (m_RippleClick[0].MouseX, m_RippleClick[0].MouseY, m_RippleClick[0].Progress, m_RippleClick[0].Strength));
		m_Mat[1].SetVector ("_Ripple2", new Vector4 (m_RippleClick[1].MouseX, m_RippleClick[1].MouseY, m_RippleClick[1].Progress, m_RippleClick[1].Strength));
		m_Mat[1].SetVector ("_Ripple3", new Vector4 (m_RippleClick[2].MouseX, m_RippleClick[2].MouseY, m_RippleClick[2].Progress, m_RippleClick[2].Strength));
		m_Mat[2].SetVector ("_Ripple1", new Vector4 (m_RippleClick[0].MouseX, m_RippleClick[0].MouseY, m_RippleClick[0].Progress, 0));
		m_Mat[2].SetVector ("_Ripple2", new Vector4 (m_RippleClick[1].MouseX, m_RippleClick[1].MouseY, m_RippleClick[1].Progress, 0));
		m_Mat[2].SetVector ("_Ripple3", new Vector4 (m_RippleClick[2].MouseX, m_RippleClick[2].MouseY, m_RippleClick[2].Progress, 0));
	}
	void OnRenderImage (RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit (src, dst, m_Mat[(int)m_RippleType]);
	}
	void OnGUI ()
	{
		if (m_ShowInternalMaps)
			GUI.DrawTextureWithTexCoords (new Rect (10, 10, 128, 128), m_RTRippleMask, new Rect (0, 0, 1, 1));
	}
}
