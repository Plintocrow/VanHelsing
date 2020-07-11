﻿using UnityEngine;


namespace BeastHunter
{
    public class BossAttackingState : BossBaseState, IDealDamage
    {
        #region Constants

        private const float DISTANCE_TO_START_ATTACK = 2.5f;
        private const float LOOK_TO_TARGET_SPEED = 1f;
        private const float PART_OF_NONE_ATTACK_TIME = 3f;

        #endregion


        #region Fields

        private Vector3 _lookDirection;
        private Quaternion _toRotation;

        private WeaponItem _leftWeapon;
        private WeaponItem _rightWeapon;

        private bool _isCurrentAttackRight;

        private int _attackNumber;

        private float _currentAttackTime;

        #endregion


        #region ClassLifeCycle

        public BossAttackingState(BossStateMachine stateMachine) : base(stateMachine)
        {
        }

        #endregion


        #region Methods

        public override void OnAwake()
        {
            _leftWeapon = _stateMachine._model.LeftHandWeapon;
            _rightWeapon = _stateMachine._model.RightHandWeapon;

            _stateMachine._model.LeftWeaponBehavior.OnFilterHandler += OnHitBoxFilter;
            _stateMachine._model.RightWeaponBehavior.OnFilterHandler += OnHitBoxFilter;
            _stateMachine._model.LeftWeaponBehavior.OnTriggerEnterHandler += OnLeftHitBoxHit;
            _stateMachine._model.RightWeaponBehavior.OnTriggerEnterHandler += OnRightHitBoxHit;
        }

        public override void Initialise()
        {
            CanExit = false;
            CanBeOverriden = true;

            _stateMachine._model.BossNavAgent.SetDestination(_stateMachine._model.BossTransform.position);
            _stateMachine._model.BossNavAgent.speed = 0f;
            _isCurrentAttackRight = UnityEngine.Random.Range(0, 2) == 1? true : false;

            if (_isCurrentAttackRight)
            {
                _attackNumber = UnityEngine.Random.Range(0, _rightWeapon.AttacksRight.Length);
                _currentAttackTime = _rightWeapon.AttacksRight[_attackNumber].Time;
                _stateMachine._model.BossAnimator.Play(_rightWeapon.SimpleAttackAnimationName + "Right_" + _attackNumber, 0, 0f);
                _rightWeapon.CurrentAttack = _rightWeapon.AttacksRight[_attackNumber];

                TimeRemaining enableWeapon = new TimeRemaining(SetRightWeaponInteractable, _currentAttackTime / 
                    PART_OF_NONE_ATTACK_TIME);
                enableWeapon.AddTimeRemaining(_currentAttackTime / PART_OF_NONE_ATTACK_TIME);
            }
            else
            {
                _attackNumber = UnityEngine.Random.Range(0, _leftWeapon.AttacksLeft.Length);
                _currentAttackTime = _leftWeapon.AttacksRight[_attackNumber].Time;
                _stateMachine._model.BossAnimator.Play(_leftWeapon.SimpleAttackAnimationName + "Left_" + _attackNumber, 0, 0f);
                _leftWeapon.CurrentAttack = _leftWeapon.AttacksLeft[_attackNumber];

                TimeRemaining enableWeapon = new TimeRemaining(SetLeftWeaponInteractable, _currentAttackTime / 
                    PART_OF_NONE_ATTACK_TIME);
                enableWeapon.AddTimeRemaining(_currentAttackTime / PART_OF_NONE_ATTACK_TIME);
            }           
        }

        public override void Execute()
        {
            CheckNextMove();
            CheckDirection();
        }

        public override void OnExit()
        {
        }

        public override void OnTearDown()
        {
            _stateMachine._model.LeftWeaponBehavior.OnFilterHandler -= OnHitBoxFilter;
            _stateMachine._model.RightWeaponBehavior.OnFilterHandler -= OnHitBoxFilter;
            _stateMachine._model.LeftWeaponBehavior.OnTriggerEnterHandler -= OnLeftHitBoxHit;
            _stateMachine._model.RightWeaponBehavior.OnTriggerEnterHandler -= OnRightHitBoxHit;
        }

        private void SetLeftWeaponInteractable()
        {
            _stateMachine._model.LeftWeaponBehavior.IsInteractable = true;
        }

        private void SetRightWeaponInteractable()
        {
            _stateMachine._model.RightWeaponBehavior.IsInteractable = true;
        }

        private void CheckNextMove()
        {
            if(_currentAttackTime > 0)
            {
                _currentAttackTime -= Time.deltaTime;

                if(_currentAttackTime <= 0)
                {
                    DecideNextMove();
                }
            }
        }

        private void CheckDirection()
        {
            _stateMachine._model.BossTransform.LookAt(_stateMachine._context.CharacterModel.CharacterTransform);
        }

        private void DecideNextMove()
        {
            _stateMachine._model.LeftWeaponBehavior.IsInteractable = false;
            _stateMachine._model.RightWeaponBehavior.IsInteractable = false;

            if (!_stateMachine._model.IsDead)
            {
                if (Mathf.Sqrt((_stateMachine._model.BossTransform.position - _stateMachine.
                    _context.CharacterModel.CharacterTransform.position).sqrMagnitude) <= DISTANCE_TO_START_ATTACK)
                {
                    Initialise();
                }
                else
                {
                    _stateMachine.SetCurrentStateOverride(BossStatesEnum.Chasing);
                }
            }
        }

        private bool OnHitBoxFilter(Collider hitedObject)
        {
            bool isEnemyColliderHit = hitedObject.CompareTag(TagManager.PLAYER);

            if (hitedObject.isTrigger || _stateMachine.CurrentState != _stateMachine.States[BossStatesEnum.Attacking])
            {
                isEnemyColliderHit = false;
            }

            return isEnemyColliderHit;
        }

        private void OnLeftHitBoxHit(ITrigger hitBox, Collider enemy)
        {
            if (enemy.transform.GetComponent<InteractableObjectBehavior>() != null && hitBox.IsInteractable)
            {
                InteractableObjectBehavior enemyBehavior = enemy.transform.GetComponent<InteractableObjectBehavior>();

                DealDamage(enemyBehavior, Services.SharedInstance.AttackService.CountDamage(_stateMachine._model.LeftHandWeapon,
                    _stateMachine._model.BossStats.BaseStats, _stateMachine._context.CharacterModel.PlayerBehavior.Stats));
                hitBox.IsInteractable = false;
            }
        }

        private void OnRightHitBoxHit(ITrigger hitBox, Collider enemy)
        {
            if (enemy.transform.GetComponent<InteractableObjectBehavior>() != null && hitBox.IsInteractable)
            {
                InteractableObjectBehavior enemyBehavior = enemy.transform.GetComponent<InteractableObjectBehavior>();

                DealDamage(enemyBehavior, Services.SharedInstance.AttackService.CountDamage(_stateMachine._model.RightHandWeapon,
                     _stateMachine._model.BossStats.BaseStats, _stateMachine._context.CharacterModel.PlayerBehavior.Stats));
                hitBox.IsInteractable = false;
            }
        }


        #region IDealDamage

        public void DealDamage(InteractableObjectBehavior enemy, Damage damage)
        {
            if (enemy != null && damage != null)
            {
                enemy.TakeDamageEvent(damage);
            }
        }

        #endregion

        #endregion
    }
}

