using UnityEngine;

public class UI_ShowFps : MonoBehaviour
{
    TMPro.TextMeshProUGUI m_text;

    private void Awake()
    {
        m_text = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Update()
    {
        float fps = 1.0f / Time.smoothDeltaTime;
        if (fps > 99.5f && fps < 100.1f)
        {
            fps = 100.0f;
        }
        m_text.text = fps.ToString("0") + " fps";
    }
}
