﻿namespace BeastHunter
{
    public class BossStunnedState : BossBaseState
    {
        #region Constants

        private const float STUN_TIME = 2f;

        #endregion


        #region Fields


        #endregion


        #region ClassLifeCycle

        public BossStunnedState(BossStateMachine stateMachine) : base(stateMachine)
        {
        }

        #endregion


        #region Methods

        public override void OnAwake()
        {
        }

        public override void Initialise()
        {
            CanExit = false;
            CanBeOverriden = false;
            _stateMachine._model.BossNavAgent.speed = 0f;
            _stateMachine._model.BossAnimator.Play("StunnedState");
            TimeRemaining stopStun = new TimeRemaining(StopStun, STUN_TIME);
            stopStun.AddTimeRemaining(STUN_TIME);
        }

        public override void Execute()
        {
        }

        public override void OnExit()
        {

        }

        public override void OnTearDown()
        {
        }

        private void StopStun()
        {
            CanBeOverriden = true;
            _stateMachine.SetCurrentStateOverride(BossStatesEnum.Chasing);
        }

        #endregion
    }
}

