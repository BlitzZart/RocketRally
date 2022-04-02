using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScoreEntry : MonoBehaviour
{
    public string PlayerId { get => m_playerId; }
    private string m_playerId;

    private TextMeshProUGUI m_name, m_kills, m_deaths, m_score;

    public void SetColor(Color c)
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.color = c;
        }
    }

    public void Initilize(string playerId)
    {
        m_name = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        m_kills = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        m_deaths = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        m_score = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        m_playerId = playerId;
    }

    public void UpdateScore(int kills, int deaths, int score)
    {
        m_kills.text = kills.ToString();
        m_deaths.text = deaths.ToString();
        m_score.text = score.ToString();
    }

    public void UpdateName(string playerName)
    {
        m_name.text = playerName;
    }
}
