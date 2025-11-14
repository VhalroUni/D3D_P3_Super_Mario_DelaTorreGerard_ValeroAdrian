using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera m_Camera;
    CharacterController m_CharacterController;
    Animator m_Animator;
    public float m_RunSpeed;
    public float m_WalkSpeed;
    float m_VerticalSpeed = 0.0f;
    public Transform m_LookAt;
    [Range(0.0f, 1.0f)] public float m_RotationLerpPct = 0.1f;
    public float m_DampTime = 0.1f;

    [Header("Input")]
    public float m_MaxTimeToComboPunch = 0.8f;
    int m_CurrentPunchId;
    float m_LastPunchTime;

    [Header("Input")]
    public int m_PunchMouseButton = 0;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
        m_LastPunchTime=-m_MaxTimeToComboPunch;
    }

    void Update()
    {
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
            m_Animator.SetFloat("Speed", l_SpeedAnimatorValue);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(l_Movement), m_RotationLerpPct);
        }

        l_Movement *= l_Speed * Time.deltaTime;
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);
        if ((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0)
            m_VerticalSpeed = 0.0f;
        else if ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0.0f)
            m_VerticalSpeed = 0.0f;

        UpdatePunch();
    }

    void UpdatePunch()
    {
        if (CanPunch() && Input.GetMouseButtonDown(m_PunchMouseButton))
            Punch();
    }
    bool CanPunch()
    {
        return !m_Animator.IsInTransition(0) && m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash==Animator.StringToHash("Movement");
    }
    void Punch()
    {
        float l_DiffPunchTime=Time.time-m_LastPunchTime;
        if (l_DiffPunchTime < m_MaxTimeToComboPunch)
            m_CurrentPunchId = (m_CurrentPunchId + 1) % 3;
        else
            m_CurrentPunchId=0;
        m_LastPunchTime = Time.time;
        m_Animator.SetTrigger("Punch");
        m_Animator.SetInteger("PunchId", m_CurrentPunchId);
    }
}