﻿using UnityEngine;
using System.Collections;
using Gamekit3D.Message;

namespace Gamekit3D
{

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour,IMessageReceiver
    {
        protected static PlayerController s_Instance;
        public static PlayerController instance
        {
            get
            {
                return s_Instance;
            }
        }

        public bool respawning { get { return m_Respawning; } }

        protected bool m_Respawning;
        public float maxForwardSpeed = 8f;        // How fast Ellen can run.
        public float gravity = 20f;               // How fast Ellen accelerates downwards when airborne.
        public float jumpSpeed = 10f;             // How fast Ellen takes off when jumping.
        public float minTurnSpeed = 400f;         // How fast Ellen turns when moving at maximum speed.
        public float maxTurnSpeed = 1200f;        // How fast Ellen turns when stationary.
        public float idleTimeout = 5f;            // How long before Ellen starts considering random idles.
        public bool canAttack;                    // Whether or not Ellen can swing her staff.

        public CameraSettings cameraSettings;            // Reference used to determine the camera's direction.
        //public MeleeWeapon meleeWeapon;                  // Reference used to (de)activate the staff when attacking. 
        //public RandomAudioPlayer footstepPlayer;         // Random Audio Players used for various situations.
        //public RandomAudioPlayer hurtAudioPlayer;
        //public RandomAudioPlayer landingPlayer;
        //public RandomAudioPlayer emoteLandingPlayer;
        //public RandomAudioPlayer emoteDeathPlayer;
        //public RandomAudioPlayer emoteAttackPlayer;
        //public RandomAudioPlayer emoteJumpPlayer;

        protected AnimatorStateInfo m_CurrentStateInfo;    // Information about the base layer of the animator cached.
        protected AnimatorStateInfo m_NextStateInfo;
        protected bool m_IsAnimatorTransitioning;
        protected AnimatorStateInfo m_PreviousCurrentStateInfo;    // Information about the base layer of the animator from last frame.
        protected AnimatorStateInfo m_PreviousNextStateInfo;
        protected bool m_PreviousIsAnimatorTransitioning;
        protected bool m_IsGrounded = true;            // Whether or not Ellen is currently standing on the ground.
        protected bool m_PreviouslyGrounded = true;    // Whether or not Ellen was standing on the ground last frame.
        protected bool m_ReadyToJump;                  // Whether or not the input state and Ellen are correct to allow jumping.
        protected float m_DesiredForwardSpeed;         // How fast Ellen aims be going along the ground based on input.
        protected float m_ForwardSpeed;                // How fast Ellen is currently going along the ground.
        protected float m_VerticalSpeed;               // How fast Ellen is currently moving up or down.
        protected PlayerInput m_Input;                 // Reference used to determine how Ellen should move.
        protected CharacterController m_CharCtrl;      // Reference used to actually move Ellen.
        protected Animator m_Animator;                 // Reference used to make decisions based on Ellen's current animation and to set parameters.
        protected Material m_CurrentWalkingSurface;    // Reference used to make decisions about audio.
        protected Quaternion m_TargetRotation;         // What rotation Ellen is aiming to have based on input.
        protected float m_AngleDiff;                   // Angle in degrees between Ellen's current rotation and her target rotation.
        protected Collider[] m_OverlapResult = new Collider[8];    // Used to cache colliders that are near Ellen.
        protected bool m_InAttack;                     // Whether Ellen is currently in the middle of a melee attack.
        protected bool m_InCombo;                      // Whether Ellen is currently in the middle of her melee combo.
        protected Damageable m_Damageable;             // Reference used to set invulnerablity and health based on respawning.
        protected Renderer[] m_Renderers;              // References used to make sure Renderers are reset properly. 
        //protected Checkpoint m_CurrentCheckpoint;      // Reference used to reset Ellen to the correct position on respawn.
        protected float m_IdleTimer;                   // Used to count up to Ellen considering a random idle.

        // These constants are used to ensure Ellen moves and behaves properly.
        // It is advised you don't change them without fully understanding what they do in code.
        const float k_AirborneTurnSpeedProportion = 5.4f;
        const float k_GroundedRayDistance = 1f;
        const float k_JumpAbortSpeed = 10f;
        const float k_MinEnemyDotCoeff = 0.2f;
        const float k_InverseOneEighty = 1f / 180f;
        const float k_StickingGravityProportion = 0.3f;
        const float k_GroundAcceleration = 20f;
        const float k_GroundDeceleration = 25f;

        // Parameters

        readonly int m_HashAirborneVerticalSpeed = Animator.StringToHash("AirborneVerticalSpeed");
        readonly int m_HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
        readonly int m_HashAngleDeltaRad = Animator.StringToHash("AngleDeltaRad");
        readonly int m_HashTimeoutToIdle = Animator.StringToHash("TimeoutToIdle");
        readonly int m_HashGrounded = Animator.StringToHash("Grounded");
        readonly int m_HashInputDetected = Animator.StringToHash("InputDetected");
        readonly int m_HashMeleeAttack = Animator.StringToHash("MeleeAttack");
        readonly int m_HashHurt = Animator.StringToHash("Hurt");
        readonly int m_HashDeath = Animator.StringToHash("Death");
        readonly int m_HashRespawn = Animator.StringToHash("Respawn");
        readonly int m_HashHurtFromX = Animator.StringToHash("HurtFromX");
        readonly int m_HashHurtFromY = Animator.StringToHash("HurtFromY");
        readonly int m_HashStateTime = Animator.StringToHash("StateTime");
        readonly int m_HashFootFall = Animator.StringToHash("FootFall");

        // States
        readonly int m_HashLocomotion = Animator.StringToHash("Locomotion");
        readonly int m_HashAirborne = Animator.StringToHash("Airborne");
        readonly int m_HashLanding = Animator.StringToHash("Landing");    // Also a parameter.
        readonly int m_HashEllenCombo1 = Animator.StringToHash("EllenCombo1");
        readonly int m_HashEllenCombo2 = Animator.StringToHash("EllenCombo2");
        readonly int m_HashEllenCombo3 = Animator.StringToHash("EllenCombo3");
        readonly int m_HashEllenCombo4 = Animator.StringToHash("EllenCombo4");
        readonly int m_HashEllenDeath = Animator.StringToHash("EllenDeath");

        // Tags
        readonly int m_HashBlockInput = Animator.StringToHash("BlockInput");

        protected bool IsMoveInput
        {
            get { return !Mathf.Approximately(m_Input.MoveInput.sqrMagnitude, 0f); }
        }

        public void SetCanAttack(bool canAttack)
        {
            this.canAttack = canAttack;
        }

        private void Reset()
        {
            
            
           

        }

        private void Awake()
        {
            m_Input = GetComponent<PlayerInput>();
            m_Animator = GetComponent<Animator>();
            m_CharCtrl = GetComponent<CharacterController>();


            s_Instance = this;
        }

        private void OnEnable()
        {
            m_Damageable = GetComponent<Damageable>();
            m_Damageable.onDamageMessageReceivers.Add(this);
            m_Damageable.isInvulnerable = true;

            m_Renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnDisable()
        {
            m_Damageable.onDamageMessageReceivers.Remove(this);
            for (int i=0;i<m_Renderers.Length;i++)
            {
                m_Renderers[i].enabled = true;
            }
        }

        private void FixedUpdate()
        {
            CacheAnimatorState();
            UpdateInputBlocking();

            m_Animator.SetFloat(m_HashStateTime, Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
            m_Animator.ResetTrigger(m_HashMeleeAttack);

            if(m_Input.AttackInput && canAttack)
            {
                m_Animator.SetTrigger(m_HashMeleeAttack);
            }

            CalculateForwardMovement();
            CalculateVerticalMovement();

            SetTargetRotation();
            if(IsOrientationUpdated() && IsMoveInput)
            {
                UpdateOrientation();
            }
            TimeoutToIdle();
            m_PreviouslyGrounded = m_IsGrounded;
        }

        void CacheAnimatorState()
        {
            m_PreviousCurrentStateInfo = m_CurrentStateInfo;
            m_PreviousNextStateInfo = m_NextStateInfo;
            m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

            m_CurrentStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            m_NextStateInfo = m_Animator.GetNextAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = m_Animator.IsInTransition(0);
        }

        void UpdateInputBlocking()
        {
            bool inputBlocked = m_CurrentStateInfo.tagHash == m_HashBlockInput && !m_IsAnimatorTransitioning;
            inputBlocked |= m_NextStateInfo.tagHash == m_HashBlockInput;
            m_Input.playerControllerInputBlocked = inputBlocked;
        }

        //判断是否在播放攻击动画从而确定是否出现武器
        bool IsWeaponEquiped()
        {
            bool equipped = m_NextStateInfo.shortNameHash == m_HashEllenCombo1 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo1;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo2 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo2;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo3 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo3;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo4 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo4;

            return equipped;
        }

        void EquipMeleeWeapon(bool equip)
        {
            
        }

        void CalculateForwardMovement()
        {
            Vector2 moveInput = m_Input.MoveInput;
            if(moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            m_DesiredForwardSpeed = moveInput.magnitude * maxForwardSpeed;

            float acceleration = IsMoveInput ? k_GroundAcceleration : k_GroundDeceleration;

            m_ForwardSpeed = Mathf.MoveTowards(m_ForwardSpeed, m_DesiredForwardSpeed, acceleration * Time.deltaTime);

            m_Animator.SetFloat(m_HashForwardSpeed, m_ForwardSpeed);
        }

        void CalculateVerticalMovement()
        {
            if(!m_Input.JumpInput && m_IsGrounded)
            {
                m_ReadyToJump = true;
            }

            if(m_IsGrounded)
            {
                //当在地面时施加一个往地面的速度让Ellen粘在地面上
                m_VerticalSpeed = -gravity * k_StickingGravityProportion;

                if(m_Input.JumpInput && m_ReadyToJump && !m_InCombo)
                {
                    m_VerticalSpeed = jumpSpeed;
                    m_IsGrounded = false;
                    m_ReadyToJump = false;
                }
            }
            else
            {
                if(!m_Input.JumpInput && m_VerticalSpeed > 0.0f)
                {
                    //减少Ellen的垂直方向的速度
                    //这就是为什么持续按住跳跃键可以跳得更远
                    m_VerticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
                }

                if(Mathf.Approximately(m_VerticalSpeed,0f))
                {
                    m_VerticalSpeed = 0;
                }
                //当Ellen在空中时给她施加重力影响
                m_VerticalSpeed -= gravity * Time.deltaTime;
            }
        }

        void SetTargetRotation()
        {
            Vector2 moveInput = m_Input.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            Vector3 forward = Quaternion.Euler(0f, cameraSettings.Current.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0;
            forward.Normalize();

            Quaternion targetRotation;

            if(Mathf.Approximately(Vector3.Dot(localMovementDirection,Vector3.forward),-1f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }

            Vector3 resultingForward = targetRotation * Vector3.forward;

            if(m_InAttack)
            {
                Vector3 centre = transform.position + transform.forward * 2.0f + transform.up;
                Vector3 halfExtents = new Vector3(3.0f, 1.0f, 2.0f);
                int layerMask = 1 << LayerMask.NameToLayer("Enemy");
                int count = Physics.OverlapBoxNonAlloc(centre, halfExtents, m_OverlapResult, targetRotation, layerMask);
                
                float closestDot = 0.0f;
                Vector3 closestForward = Vector3.zero;
                int closest = -1;

                for(int i=0;i<count;i++)
                {
                    Vector3 playerToEnemy = m_OverlapResult[i].transform.position - transform.position;
                    playerToEnemy.y = 0;
                    playerToEnemy.Normalize();

                    float d = Vector3.Dot(resultingForward, playerToEnemy);

                    if(d>k_MinEnemyDotCoeff && d > closestDot)
                    {
                        closestForward = playerToEnemy;
                        closestDot = d;
                        closest = i;
                    }
                }

                if(closest != -1)
                {
                    resultingForward = closestForward;

                    transform.rotation = Quaternion.LookRotation(resultingForward);
                }
            }
            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;

            m_AngleDiff = Mathf.DeltaAngle(angleCurrent, targetAngle);
            m_TargetRotation = targetRotation;
        }

        bool IsOrientationUpdated()
        {
            bool updateOrientationForLocomotion = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashLocomotion || m_NextStateInfo.shortNameHash == m_HashLocomotion;
            bool updateOrientationForAirborne = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashAirborne || m_NextStateInfo.shortNameHash == m_HashAirborne;
            bool updateOrientationForLanding = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashLanding || m_NextStateInfo.shortNameHash == m_HashLanding;

            return updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding || m_InCombo && !m_InAttack;
        }

        void UpdateOrientation()
        {
            m_Animator.SetFloat(m_HashAngleDeltaRad, m_AngleDiff * Mathf.Deg2Rad);

            Vector3 localInput = new Vector3(m_Input.MoveInput.x, 0f, m_Input.MoveInput.y);
            float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, m_ForwardSpeed / m_DesiredForwardSpeed);
            float actualTurnSpeed = m_IsGrounded ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty * k_AirborneTurnSpeedProportion * groundedTurnSpeed;
            m_TargetRotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, actualTurnSpeed * Time.deltaTime);

            transform.rotation = m_TargetRotation;
        }

        //void PlayAudio()

        void TimeoutToIdle()
        {
            bool inputDetected = IsMoveInput || m_Input.AttackInput || m_Input.JumpInput;
            if(m_IsGrounded && !inputDetected)
            {
                m_IdleTimer += Time.deltaTime;
                if(m_IdleTimer >= idleTimeout)
                {
                    m_IdleTimer = 0;
                    m_Animator.SetTrigger(m_HashTimeoutToIdle);
                }
            }
            else
            {
                m_IdleTimer = 0;
                m_Animator.ResetTrigger(m_HashTimeoutToIdle);
            }
            m_Animator.SetBool(m_HashInputDetected, inputDetected);
        }

        private void OnAnimatorMove()
        {
            Vector3 movement;

            if (m_IsGrounded)
            {
                RaycastHit hit;
                Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
                if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    movement = Vector3.ProjectOnPlane(m_Animator.deltaPosition, hit.normal);

                    Renderer groundRenderer = hit.collider.GetComponent<Renderer>();
                    m_CurrentWalkingSurface = groundRenderer ? groundRenderer.sharedMaterial : null;
                }
                else
                {
                    movement = m_Animator.deltaPosition;
                    m_CurrentWalkingSurface = null;
                }
            }
            else
            {
                movement = m_ForwardSpeed * transform.forward * Time.deltaTime;
            }

            m_CharCtrl.transform.rotation *= m_Animator.deltaRotation;
            movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

            m_CharCtrl.Move(movement);
            m_IsGrounded = m_CharCtrl.isGrounded;

            if (!m_IsGrounded)
            {
                m_Animator.SetFloat(m_HashAirborneVerticalSpeed, m_VerticalSpeed);
            }

            m_Animator.SetBool(m_HashGrounded, m_IsGrounded);
        }




        public void OnReceiveMessage(MessageType type,object sender,object data)
        {
            switch(type)
            {
                case MessageType.DAMAGED:
                    {
                        Damageable.DamageMessage damageData = (Damageable.DamageMessage)data;
                        
                    }
                    break;
                case MessageType.DEAD:
                    {
                        Damageable.DamageMessage damageData = (Damageable.DamageMessage)data;

                    }
                    break;
            }
        }

        void Damaged(Damageable.DamageMessage damageMessage)
        {
            m_Animator.SetTrigger(m_HashHurt);

            Vector3 forward = damageMessage.damageSource - transform.position;
            forward.y = 0;

            Vector3 localHurt = transform.InverseTransformDirection(forward);

            m_Animator.SetFloat(m_HashHurtFromX, localHurt.x);
            m_Animator.SetFloat(m_HashHurtFromY, localHurt.y);

            //shake the camera
            

            //播放受伤音效



        }

        public void Die(Damageable.DamageMessage damageMessage)
        {
            m_Animator.SetTrigger(m_HashDeath);
            m_ForwardSpeed = 0f;
            m_VerticalSpeed = 0f;
            m_Respawning = true;
            m_Damageable.isInvulnerable = true;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


