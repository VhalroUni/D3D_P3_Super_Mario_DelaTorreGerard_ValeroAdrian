using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager m_GameManager;
    List<IRestartGameElement> m_RestartGameElements = new List<IRestartGameElement>();
    PlayerController m_Player;
    public GameUI m_GameUI;

    private void Awake()
    {
        if (m_GameManager != null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        m_GameManager = this;
        DontDestroyOnLoad(gameObject);
    }
    static public GameManager GetGameManager()
    {
        return m_GameManager;
    }
    public void AddRestartGameElement(IRestartGameElement RestartGameElement)
    {
        m_RestartGameElements.Add(RestartGameElement);
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
            RestartGame();

        if (Input.GetKeyUp(KeyCode.H))
            m_Player.Hit();

        if (Input.GetKeyUp(KeyCode.J))
            m_Player.AddCoin(1);

        if (Input.GetKeyUp(KeyCode.K))
            m_Player.AddLife(1);
    }
    public void RestartGame()
    {
        foreach (IRestartGameElement l_RestartGameElement in m_RestartGameElements)
            l_RestartGameElement.RestartGame();
    }
    public void RestartFullGame()
    {
        SceneManager.LoadScene(0);
    }
    public PlayerController GetPLayer()
    {
        return m_Player;
    }
    public void SetPlayer(PlayerController Player)
    {
        m_Player = Player;
    }

}