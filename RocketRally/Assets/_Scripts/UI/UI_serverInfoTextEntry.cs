using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_serverInfoTextEntry : MonoBehaviour
{
    private TextMeshProUGUI m_serverInfoText;

    private void Awake()
    {
        m_serverInfoText = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text, float lifeTime)
    {
        m_serverInfoText.text = text;
        Destroy(gameObject, lifeTime);
    }
}
