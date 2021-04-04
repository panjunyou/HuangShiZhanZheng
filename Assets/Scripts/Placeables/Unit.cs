using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UnityRoyale
{
    // 人类或可移动物体
    public class Unit : ThinkingPlaceable
    {
        //data coming from the PlaceableData
        private float speed;

        private Animator animator;
        private NavMeshAgent navMeshAgent;

        private void Awake()
        {
            pType = Placeable.PlaceableType.Unit;

            //find references to components
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>(); //will be disabled until Activate is called
			audioSource = GetComponent<AudioSource>();
        }

        //called by GameManager when this Unit is played on the play field
        public void Activate(Faction pFaction, PlaceableData pData)
        {
            faction = pFaction;
            hitPoints = pData.hitPoints;
            targetType = pData.targetType;
            attackRange = pData.attackRange;
            attackRatio = pData.attackRatio;
            speed = pData.speed;
            damage = pData.damagePerAttack;
			attackAudioClip = pData.attackClip;
			dieAudioClip = pData.dieClip;
            //TODO: add more as necessary
            
            navMeshAgent.speed = speed;
            animator.SetFloat("MoveSpeed", speed); //will act as multiplier to the speed of the run animation clip

            state = States.Idle;
            navMeshAgent.enabled = true;
        }

        public override void SetTarget(ThinkingPlaceable t)
        {
            base.SetTarget(t);
        }

		//Unit moves towards the target
        public override void Seek()
        {
            if(target == null)
                return;

            base.Seek();

            navMeshAgent.SetDestination(target.transform.position);
            navMeshAgent.isStopped = false;
            animator.SetBool("IsMoving", true);
        }

		//Unit has gotten to its target. This function puts it in "attack mode", but doesn't delive any damage (see DealBlow)
        public override void StartAttack()
        {
            base.StartAttack();

            navMeshAgent.isStopped = true;
            animator.SetBool("IsMoving", false);
        }

		/// <summary>
		/// 开始攻击动画，其重复速率根据游戏单位的attackRatio
		/// </summary>
        public override void DealBlow()
        {
            base.DealBlow();

            animator.SetTrigger("Attack");
            transform.forward = (target.transform.position - transform.position).normalized; //turn towards the target
        }

		public override void Stop()
		{
			base.Stop();

			navMeshAgent.isStopped = true;
			animator.SetBool("IsMoving", false);
		}

        protected override void Die()
        {
            base.Die();

            navMeshAgent.enabled = false;
            animator.SetTrigger("IsDead");
        }
    }
}
