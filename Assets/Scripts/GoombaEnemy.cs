using UnityEngine;

public class GoombaEnemy : MonoBehaviour, IRestartGameElement
{
    CharacterController m_CharacterController;
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;

    void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }
    private void Start()
    {
        GameManager.GetGameManager().AddRestartGameElement(this);
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
    }
    public void Update()
    {

    }
    public void RestartGame()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
        gameObject.SetActive(true);
    }
    public void Kill()
    {
        gameObject.SetActive(false);
        //Drop Coins
    }
}
