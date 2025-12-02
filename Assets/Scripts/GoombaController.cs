using System.Collections.Generic;
using UnityEngine;

public class GoombaController : MonoBehaviour, IRestartGameElement
{
    enum TState
    {
        PATROL,
        CHASE,
        ATTACK,
        DIE
    }

    TState m_State;

    CharacterController m_CharacterController;
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;

    [Header("Distance")]
    public float m_DetectPlayer = 10.0f;
    public float m_MinDistanceToChase = 2.0f;

    [Header("Patrol")]
    public List<Transform> m_PatrolPositions;
    int m_CurrentPatrolPositionId = 0;
    public float m_PatrolSpeed = 1.0f;

    [Header("Chase")]
    public float m_ChaseSpeed = 2.0f;

    [Header("Sight")]
    public float m_SightAngle = 90.0f;
    public LayerMask m_SightLayerMask;
    public float m_EyesHeight = 1.2f;

    [Header("AttackCooldowns")]
    public float m_AttackCooldown = 1.0f;
    private float m_AttackTimer = 0;

    [Header("Life")]
    public int m_Life = 1;

    [Header("Loot")]
    public GameObject m_ItemDrop;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        GameManager.GetGameManager().AddRestartGameElement(this);
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;

        SetPatrolState();
    }

    private void Update()
    {
        if (m_AttackTimer > 0f)
            m_AttackTimer -= Time.deltaTime;

        switch (m_State)
        {
            case TState.PATROL:
                UpdatePatrolState();
                break;
            case TState.CHASE:
                UpdateChaseState();
                break;
            case TState.DIE:
                UpdateDieState();
                break;
        }
    }
    void SetPatrolState()
    {
        m_State = TState.PATROL;
        m_CurrentPatrolPositionId = 0;
    }
    void UpdatePatrolState()
    {
        if (SeesPlayer())
        {
            SetChaseState();
            return;
        }

        if (m_PatrolPositions.Count == 0)
            return;

        Vector3 l_Target = m_PatrolPositions[m_CurrentPatrolPositionId].position;
        GoombaMove(l_Target, m_PatrolSpeed);

        float distance = Vector3.Distance(transform.position, l_Target);
        if (distance < 0.3f)
        {
            MoveToNextPatrolPosition();
        }
    }
    void SetChaseState()
    {
        Debug.Log("SetChase");
        m_State = TState.CHASE;
    }
    void UpdateChaseState()
    {
        Debug.Log("Chasing");
        float distance = Vector3.Distance(transform.position, GameManager.GetGameManager().GetPLayer().transform.position);

        if (!SeesPlayer() || distance > m_DetectPlayer)
        {
            SetPatrolState();
            return;
        }
        GoombaMove(GameManager.GetGameManager().GetPLayer().transform.position, m_ChaseSpeed);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Player"))
        {
            if (m_AttackTimer <= 0)
            {
                GameManager.GetGameManager().GetPLayer().Damage(1);
                m_AttackTimer = m_AttackCooldown;
            }
        }
    }

    void SetDieState()
    {
        m_State = TState.DIE;
        if (m_ItemDrop != null)
        {
            Vector3 m_DropPosition = transform.position + Vector3.up;
            Instantiate(m_ItemDrop, m_DropPosition, Quaternion.identity);
        }
    }
    void UpdateDieState()
    {
        gameObject.SetActive(false);
    }

    void GoombaMove(Vector3 Target, float Speed)
    {
        Vector3 l_Direction = Target - transform.position;
        l_Direction.y = 0;

        l_Direction.Normalize();

        m_CharacterController.Move(l_Direction * Speed * Time.deltaTime);

        if (l_Direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(l_Direction);
    }
    void MoveToNextPatrolPosition()
    {
        ++m_CurrentPatrolPositionId;
        if (m_CurrentPatrolPositionId >= m_PatrolPositions.Count)
            m_CurrentPatrolPositionId = 0;
    }
    bool SeesPlayer()
    {
        Vector3 l_PlayerPosition = GameManager.GetGameManager().GetPLayer().transform.position;
        Vector3 l_Direction = l_PlayerPosition - transform.position;
        float l_Distance = l_Direction.magnitude;
        //l_Direction.Normalize();
        l_Direction /= l_Distance;
        float l_DotValue = Vector3.Dot(l_Direction, transform.forward);
        if (l_DotValue >= Mathf.Cos(m_SightAngle * 0.5f * Mathf.Deg2Rad))
        {
            Ray l_Ray = new Ray(transform.position + Vector3.up * m_EyesHeight, l_Direction);
            //float l_Distance=Vector3.Distance(l_PlayerPosition, transform.position);
            if (!Physics.Raycast(l_Ray, l_Distance, m_SightLayerMask.value))
                return true;
        }
        return false;
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
        SetDieState();
    }
}
