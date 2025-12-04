using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PlayerController : MonoBehaviour, IRestartGameElement
{
    public enum TPunchType
    {
        RIGHT_HAND = 0,
        LEFT_HAND,
        KICK
    }
    public Camera m_Camera;
    CharacterController m_CharacterController;
    Animator m_Animator;
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;
    public float m_RunSpeed;
    public float m_WalkSpeed;
    float m_VerticalSpeed = 0.0f;
    public Transform m_LookAt;
    [Range(0.0f, 1.0f)] public float m_RotationLerpPct = 0.1f;
    public float m_DampTime = 0.1f;
    CheckPoint m_CurrentCheckPoint;

    [Header("Jump")]
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    public float m_JumpSpeed = 5.0f;
    public float m_MaxAngleToKillGoomba = 45.0f;
    public float m_KillJumpSpeed = 7.0f;
    int m_CurrentJumpId;
    float m_LastJumpTime;
    public float m_MaxTimeToComboJump = 0.8f;
    bool m_IsGrounded = false;

    [Header("Punch")]
    public float m_MaxTimeToComboPunch = 0.8f;
    int m_CurrentPunchId;
    float m_LastPunchTime;
    public GameObject m_RightHandPunchCollider;
    public GameObject m_LeftHandPunchCollider;
    public GameObject m_KickPunchCollider;

    [Header("Input")]
    public int m_PunchMouseButton = 0;

    [Header("Health")]
    public int m_LocalLifes = 8;
    public int m_GlobalLifes = 5;
    LifeController m_LifeController = new LifeController();
    public GameObject m_CanvasGameOver;

    [Header("Coin")]
    public int m_Coins = 0;
    CoinsController m_CoinsController = new CoinsController();

    [Header("Attach")]
    public float m_MaxAngleToAttachElevator = 30.0f;
    Collider m_ElevatorCollider;

    [Header("Audio")]
    public AudioSource m_LeftFootStepAudioSource;
    public AudioSource m_RightFootStepAudioSource;

    [Header("Elevator")]
    public float m_MaxAngleToAttachToElevator = 30.0f;
    public float m_BridgeHitForce = 10.0f;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_LastPunchTime = -m_MaxTimeToComboPunch;
        m_RightHandPunchCollider.gameObject.SetActive(false);
        m_LeftHandPunchCollider.gameObject.SetActive(false);
        m_KickPunchCollider.gameObject.SetActive(false);
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        GameManager.GetGameManager().AddRestartGameElement(this);
        GameManager.GetGameManager().SetPlayer(this);
    }

    void Update()
    {
        if (m_GlobalLifes <= 0)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1.0f;
                GameManager.GetGameManager().RestartFullGame();
            }
            return;
        }

        Vector3 l_Right = m_Camera.transform.right;
        Vector3 l_Forward = m_Camera.transform.forward;
        Vector3 l_Movement = Vector3.zero;

        l_Right.y = 0;
        l_Forward.y = 0;
        l_Right.Normalize();
        l_Forward.Normalize();

        if (Input.GetKey(KeyCode.D))
            l_Movement += l_Right;
        else if (Input.GetKey(KeyCode.A))
            l_Movement -= l_Right;

        if (Input.GetKey(KeyCode.W))
            l_Movement += l_Forward;
        else if (Input.GetKey(KeyCode.S))
            l_Movement -= l_Forward;

        l_Movement.Normalize();
        float l_SpeedAnimatorValue = 0.5f;
        float l_Speed = m_WalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            l_Speed = m_RunSpeed;
            l_SpeedAnimatorValue = 1.0f;
        }

        if (l_Movement.sqrMagnitude == 0.0f)
            m_Animator.SetFloat("Speed", 0.0f, m_DampTime, Time.deltaTime);
        else
        {
            m_Animator.SetFloat("Speed", l_SpeedAnimatorValue, m_DampTime, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(l_Movement), m_RotationLerpPct);
        }

        if (Input.GetKeyDown(m_JumpKeyCode))
        {
            if (CanJump())
                Jump();
        }

        l_Movement *= l_Speed * Time.deltaTime;
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);
        if ((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0)
        {
            if (m_VerticalSpeed < 0.0f)
            {
                m_VerticalSpeed = 0.0f;
                m_IsGrounded = true;
            }
            else
            {
                m_IsGrounded = false;
            }
        }
        else if ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0.0f)
        {
            m_VerticalSpeed = 0.0f;
        }

        UpdatePunch();
        UpdateJump();
    }
    void LateUpdate()
    {
        UpdateElevator();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            Item l_Item = other.GetComponent<Item>();
            if (l_Item.CanPick())
                l_Item.Pick();
        }
        else if (other.CompareTag("DeadZone"))
            Kill();
        else if (other.CompareTag("Elevator"))
        {
            if (CanAttachToElevator(other))
                AttachToElevator(other);
        }
        else if (other.CompareTag("CheckPoint"))
        {
            m_CurrentCheckPoint=other.GetComponent<CheckPoint>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Elevator"))
            DetachFromElevator();
    }
    bool CanAttachToElevator(Collider ElevatorCollider)
    {

        return Vector3.Dot(ElevatorCollider.transform.up, Vector3.up) > Mathf.Cos(m_MaxAngleToAttachElevator * Mathf.Deg2Rad);
    }
    void AttachToElevator(Collider ElevatorCollider)
    {
        transform.SetParent(ElevatorCollider.transform.parent);
        m_ElevatorCollider = ElevatorCollider;
    }
    void DetachFromElevator()
    {
        transform.SetParent(null);
        transform.up = Vector3.up;
        m_ElevatorCollider = null;
    }
    void UpdateElevator()
    {
        if (m_ElevatorCollider != null)
        {
            Vector3 l_Direction = transform.forward;
            l_Direction.y = 0.0f;
            l_Direction.Normalize();
            transform.rotation = Quaternion.LookRotation(l_Direction, Vector3.up);
        }
    }
    void UpdatePunch()
    {
        if (CanPunch() && Input.GetMouseButtonDown(m_PunchMouseButton))
            Punch();
    }
    bool CanPunch()
    {
        return !m_Animator.IsInTransition(0) && m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Movement");
    }
    void Punch()
    {
        float l_DiffPunchTime = Time.time - m_LastPunchTime;
        if (l_DiffPunchTime < m_MaxTimeToComboPunch)
            m_CurrentPunchId = (m_CurrentPunchId + 1) % 3;
        else
            m_CurrentPunchId = 0;
        m_LastPunchTime = Time.time;
        m_Animator.SetTrigger("Punch");
        m_Animator.SetInteger("PunchId", m_CurrentPunchId);
    }
    public void SetActivePunch(TPunchType PunchType, bool Active)
    {
        if (PunchType == TPunchType.RIGHT_HAND)
            m_RightHandPunchCollider.SetActive(Active);
        else if (PunchType == TPunchType.LEFT_HAND)
            m_LeftHandPunchCollider.SetActive(Active);
        else if (PunchType == TPunchType.KICK)
            m_KickPunchCollider.SetActive(Active);
    }
    public void RestartGame()
    {
        if (m_CurrentCheckPoint != null)
        {
            m_StartPosition=m_CurrentCheckPoint.m_RestartPosition.position;
            m_StartRotation = m_CurrentCheckPoint.m_RestartPosition.rotation;
        }
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
    void UpdateJump()
    {
        if (CanJump() && Input.GetKeyDown(m_JumpKeyCode))
            Jump();
    }
    bool CanJump()
    {
        return m_IsGrounded && !m_Animator.IsInTransition(0);
    }
    void Jump()
    {
        float l_DiffJumpTime = Time.time - m_LastJumpTime;
        if (l_DiffJumpTime < m_MaxTimeToComboJump)
            m_CurrentJumpId = (m_CurrentJumpId + 1) % 3;
        else
            m_CurrentJumpId = 0;
        m_LastJumpTime = Time.time;

        m_Animator.SetTrigger("Jump");
        m_Animator.SetInteger("JumpId", m_CurrentJumpId);

        if (m_CurrentJumpId == 0)
            m_VerticalSpeed = m_JumpSpeed;
        else if (m_CurrentJumpId == 1)
            m_VerticalSpeed = m_JumpSpeed * 1.2f;
        else if (m_CurrentJumpId == 2)
            m_VerticalSpeed = m_JumpSpeed * 1.4f;

        m_IsGrounded = false;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Goomba"))
        {
            GoombaController l_GoombaEnemy = hit.collider.GetComponent<GoombaController>();
            if (CanKillWithFeet(hit))
            {
                l_GoombaEnemy.Kill();
                JumpOverEnemy();
            }
            Debug.DrawRay(hit.point, hit.normal, Color.magenta, 5.0f);
        }
        else if (hit.collider.CompareTag("Bridge"))
        {
            hit.rigidbody.AddForceAtPosition(-hit.normal * m_BridgeHitForce, hit.point);
        }
    }
    void JumpOverEnemy()
    {
        m_VerticalSpeed = m_KillJumpSpeed;
    }
    bool CanKillWithFeet(ControllerColliderHit hit)
    {
        float l_Dot = Vector3.Dot(hit.normal, Vector3.up);
        return m_VerticalSpeed < 0.0f && l_Dot > Mathf.Cos(m_MaxAngleToKillGoomba * Mathf.Deg2Rad);
    }

    public void AddLife(int Life)
    {
        m_LocalLifes += Life;
        if (m_LocalLifes > 8)
            m_LocalLifes = 8;

        m_LifeController.AddLife(-1);
        //GameManager.GetGameManager().m_GameUI.SetLifeBar(m_Life / 8.0f);
        //GameManager.GetGameManager().m_GameUI.ShowUI();
    }

    public void AddCoin(int Coin)
    {
        m_CoinsController.AddCoins(Coin);
        //GameManager.GetGameManager().m_GameUI.SetCoins(m_Coins);
        //GameManager.GetGameManager().m_GameUI.ShowUI();
    }
    public void Hit()
    {
        m_LifeController.AddLife(-1);
        //GameManager.GetGameManager().m_GameUI.SetLifeBar(m_Life / 8.0f);
        //GameManager.GetGameManager().m_GameUI.ShowUI();
    }

    public void Damage(int Damage)
    {
        m_LocalLifes -= Damage;

        if (m_LocalLifes < 0)
        {
            m_LocalLifes = 0;
        }

        GameManager.GetGameManager().m_GameUI.SetLifeBar(m_LocalLifes / 8.0f);
        GameManager.GetGameManager().m_GameUI.ShowUI();

        if (m_LocalLifes <= 0)
            Kill();
    }

    void Kill()
    {
        m_GlobalLifes--;
        if(m_GlobalLifes > 0)
        {
            m_LocalLifes = 8;
            GameManager.GetGameManager().RestartGame();
            GameManager.GetGameManager().m_GameUI.SetLifeBar(m_LocalLifes / 8.0f);
            GameManager.GetGameManager().m_GameUI.ShowUI();
        }
        else
        {
            Time.timeScale = 0;
            if (m_CanvasGameOver != null)
                m_CanvasGameOver.SetActive(true);
        }
    }

    public void Step(AnimationEvent _AnimationEvent)
    {
        /*AudioSource l_CurrentAudioSource = null;
        if (_AnimationEvent.stringParameter == "Left")
            l_CurrentAudioSource = m_LeftFootStepAudioSource;
        else if (_AnimationEvent.stringParameter == "Right")
            l_CurrentAudioSource = m_RightFootStepAudioSource;
        AudioClip l_AudioClip = (AudioClip)_AnimationEvent.objectReferenceParameter;
        l_CurrentAudioSource.clip = l_AudioClip;
        l_CurrentAudioSource.Play();*/
    }
}