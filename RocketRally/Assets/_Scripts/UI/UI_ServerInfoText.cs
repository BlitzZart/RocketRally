using UnityEngine;

public class UI_ServerInfoText : MonoBehaviour
{
    [SerializeField]
    private UI_serverInfoTextEntry m_serverInfoTextPrefab;

    private GameManager m_gameManager;

    private void Start()
    {
        m_gameManager = FindObjectOfType<GameManager>();
        m_gameManager.ServerRestartInS.OnValueChanged += OnRestartTimeChanged;
    }

    private void OnDestroy()
    {
        if (m_gameManager != null)
        {
            m_gameManager.ServerRestartInS.OnValueChanged -= OnRestartTimeChanged;
        }
    }

    private void OnRestartTimeChanged(int previousValue, int newValue)
    {
        UI_serverInfoTextEntry entry = Instantiate(m_serverInfoTextPrefab, transform);

        if (newValue > 120)
        {
            entry.SetText("Server restart in " + newValue / 60 + " min", 3.0f);
        }
        else
        {
            entry.SetText("Server restart in " + newValue + " sec", 5.0f);
        }
    }
}
