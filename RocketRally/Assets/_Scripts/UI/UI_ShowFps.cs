using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ShowFps : MonoBehaviour
{
    TMPro.TextMeshProUGUI m_text;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        m_text = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Update()
    {
        m_text.text = (1.0f / Time.smoothDeltaTime).ToString("0");
    }
}
