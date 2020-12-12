using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Gamekit3D.Message;
using UnityEngine.Serialization;

namespace Gamekit3D
{
    public partial class Damageable : MonoBehaviour
    {
        public int maxHitPoints;
        [Tooltip("受到伤害后的无敌时间")]
        public float invulnerabiltyTime;

        [Range(0.0f, 360.0f)]
        public float hitAngle = 360.0f;

        [Range(0.0f, 360.0f)]
        [FormerlySerializedAs("hitForwardRotation")]
        public float hitForwardRotation = 360.0f;

        public bool isInvulnerable { get; set; }
        public int currentHitPoints { get; private set; }

        public UnityEvent OnDeath, OnReceiveDamage, OnHitWhileInvulnerable, OnBecomeVulnerable, OnResetDamage;

        [Tooltip("当此游戏物体受到伤害时，下面的游戏对象将收到通知")]
        public List<MonoBehaviour> onDamageMessageReceivers;

        protected float m_timeSinceLastHit = 0.0f;
        protected Collider m_Collider;

        System.Action schedule;

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }
        

        public void ResetDamage()
        {
            currentHitPoints = maxHitPoints;
            isInvulnerable = false;
            m_timeSinceLastHit = 0.0f;
            OnResetDamage.Invoke();
        }

        public void SetColliderState(bool enabled)
        {
            m_Collider.enabled = enabled;
        }

        public void ApplyDamage(DamageMessage data)
        {
            if(currentHitPoints <= 0)
            {
                return;
            }

            if(isInvulnerable)
            {
                OnHitWhileInvulnerable.Invoke();
                return;
            }

            Vector3 forward = transform.forward;
            forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;
            //将受击方向投影到xz平面上
            Vector3 positionToDamager = data.damageSource - transform.position;
            positionToDamager -= transform.up * Vector3.Dot(transform.up, positionToDamager);
            //判断攻击是否在hitAngle范围内
            if(Vector3.Angle(positionToDamager,forward) > hitAngle * 0.5f)
            {
                return;
            }

            isInvulnerable = true;
            currentHitPoints -= data.amount;

            if(currentHitPoints <= 0)
            {
                schedule += OnDeath.Invoke;
            }
            else
            {
                OnReceiveDamage.Invoke();
            }

            var message = currentHitPoints <= 0 ? MessageType.DEAD : MessageType.DAMAGED;
            for(var i=0;i<onDamageMessageReceivers.Count;++i)
            {
                var receiver = onDamageMessageReceivers[i] as IMessageReceiver;
                receiver.OnReceiveMessage(message,this,data);
            }
        }


        private void LateUpdate()
        {
            if(schedule != null)
            {
                schedule.Invoke();
                schedule = null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 forward = transform.forward;
            forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

            if (Event.current.type == EventType.Repaint)
            {
                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(forward), 1.0f, EventType.Repaint);
            }

            UnityEditor.Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            forward = Quaternion.AngleAxis(-hitAngle * 0.5f, transform.up) * forward;
            UnityEditor.Handles.DrawSolidArc(transform.position, transform.up, forward, hitAngle, 1.0f);
        }
#endif
    }

}

