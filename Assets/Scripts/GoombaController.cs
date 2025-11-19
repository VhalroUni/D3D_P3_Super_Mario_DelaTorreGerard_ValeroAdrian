using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoombaController : MonoBehaviour
{
    enum TState
    {
        IDLE = 0,
        PATROL,
        ALERT,
        ATTACK,
        CHASE,
        HIT,
        DIE
    }
    TState m_State;
    NavMeshAgent m_NavMeshAgent;
    public Transform m_Target;

    [Header("Distance")]
    public float m_MinDistanceToAttack = 5.0f;
    public float m_MinDistanceToPlayer = 2.0f;

    [Header("Patrol")]
    public List<Transform> m_PatrolPositoins;
    int m_CurrentPatrolPositionId = 0;

    [Header("Chase")]
    public float m_AlertTimer = 0f;
    public float m_Speed = 2f;
    public float m_MaxDistanceToChase = 15f;

    [Header("Sight")]
    public float m_SightAngle = 60.0f;
    public LayerMask m_SightLayerMask;
    public float m_EyesHeight = 1.8f;

    [Header("Life")]
    public int m_Life = 100;
    public int m_MaxLife = 100;

    [Header("AttackCooldowns")]
    public float m_AttackCooldown = 0.3f;
    private float m_AttackTimer = 0;

    [Header("Dead")]
    public List<MeshRenderer> m_MeshRenderers;
    float m_CurrentTime;
    public float m_DeadTime = 1.5f;

    [Header("Loot")]
    public GameObject m_ItemDrop;

    private void Awake()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (m_AttackCooldown > 0f)
            m_AttackTimer -= Time.deltaTime;

        switch (m_State)
        {
            case TState.PATROL:
                UpdatePatrolState();
                break;
            case TState.ATTACK:
                UpdateAttackState();
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
        MoveToNextPatrolPosition();
    }
    void UpdatePatrolState()
    {
        if (!m_NavMeshAgent.hasPath && m_NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
            MoveToNextPatrolPosition();
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
        if (distance <= m_MinDistanceToAttack)
            SetAttackState();
        else if (distance >= m_MaxDistanceToChase)
        {
            SetPatrolState();
        }
        SetNextChasePosition();
    }
    void SetAttackState()
    {
        m_State = TState.ATTACK;
    }
    void UpdateAttackState()
    {
        //Si le pegas un hit se queda en alert, si no te ve despues de esto = patrol, si te ve = chase.
        float distance = Vector3.Distance(transform.position, GameManager.GetGameManager().GetPLayer().transform.position);
        if (distance > m_MinDistanceToAttack && distance <= m_MaxDistanceToChase)
        {
            SetChaseState();
            SetNextChasePosition();
        }
        else if (distance > m_MaxDistanceToChase)
            SetPatrolState();
        else
        {
            if (m_AttackTimer <= 0)
            {
                //El player recibe damage
                Debug.Log("Damaga al Player");
                GameManager.GetGameManager().GetPLayer().Damage(10);
                m_AttackTimer = m_AttackCooldown;
            }
        }
    }
    void SetDieState()
    {
        m_State = TState.DIE;
        m_CurrentTime = 0.0f;
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

    void SetNextChasePosition()
    {
        Vector3 l_PlayerPosition = GameManager.GetGameManager().GetPLayer().transform.position;
        Vector3 l_Direction = l_PlayerPosition - transform.position;
        l_Direction.Normalize();
        Vector3 l_Position = l_PlayerPosition - l_Direction * m_MinDistanceToAttack;
        m_NavMeshAgent.destination = l_Position;
    }
    void MoveToNextPatrolPosition()
    {
        Vector3 l_Destination = m_PatrolPositoins[m_CurrentPatrolPositionId].position;
        m_NavMeshAgent.destination = l_Destination;
        ++m_CurrentPatrolPositionId;
        if (m_CurrentPatrolPositionId >= m_PatrolPositoins.Count)
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
}

