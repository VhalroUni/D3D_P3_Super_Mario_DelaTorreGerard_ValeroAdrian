using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager m_GameManager;
    List<IRestartGameElement> m_RestartGameElements = new List<IRestartGameElement>();
    PlayerController m_Player;
    public GameUI m_GameUI;
    public AudioSource m_Music;

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
    private void Start()
    {
        m_Music.Play();
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
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) //Pregunta a IA para hacer que el restartfullgame y buscar al nuevo player Y GameUI.
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if(player != null)
        {
            SetPlayer(player);
        }

        GameUI UI = FindAnyObjectByType<GameUI>();
        if (UI != null)
        {
            m_GameUI = UI;  
        }
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