using System.Collections;
using UnityEngine;

namespace BehaviorTree
{
    public class EnemyBehaviourTree : BehaviorTree, ICharacterController
    {
        // Layer mask of the walls of the level.
        [SerializeField] protected LayerMask wallLayer;

        // TODO: Add stun/thrown to Configuration library
        // Amount of time the enemy is stunned
        protected float stunTime = 1f;

        // Amount of time the enemy is immune to getting stunned after getting out of a stun
        protected float stunImmunityTime = 1f;

        // Amount of time the enemy is thrown
        protected float thrownTime = 1f;

        // Amount of time the enemy is immune to getting thrown after being thrown
        protected float thrownImmunityTime = 1f;

        // Distance how far the enemy can see
        [SerializeField] protected float viewDistance = 6f;

        // Distance how far the enemy can hear the player shots
        [SerializeField] protected float hearDistance = 20f;

        private float _stunImmunityEndTimestamp;
        private float _thrownImmunityEndTimestamp;

        protected override Node SetupTree()
        {
            EventManager.OnPlayerShotFired.Subscribe(HearPlayerShotFired);

            Node root = new Node();
            return root;
        }

        /// <summary>
        /// Sets appropriate data when the enemy is supposed to be stunned.
        /// </summary>
        /// <returns>True when the enemy could be stunned, false if not.</returns>
        public virtual bool StunCharacter()
        {
            return SetCharacterState(root.sharedData.IsStunned, stunTime, stunImmunityTime, ref _stunImmunityEndTimestamp);
        }

        /// <summary>
        /// Sets appropriate data when the enemy is supposed to be thrown.
        /// </summary>
        /// <returns>True when the enemy could be thrown, false if not.</returns>
        public virtual bool ThrowCharacter()
        {
            return SetCharacterState(root.sharedData.IsThrown, thrownTime, thrownImmunityTime, ref _thrownImmunityEndTimestamp);
        }

        /// <summary>
        /// Sets given state to true for given period of time if the enemy isn't still immune.
        /// If so, sets that state to false after time has passed and updates immunity to that state.
        /// </summary>
        /// <param name="state">State to set.</param>
        /// <param name="stateTime">Time the state is set true.</param>
        /// <param name="stateImmunityTime">Time the enemy is immune to the state after the state is over.</param>
        /// <param name="stateImmunityEndTimestamp">Timestamp marking the end of state immunity.</param>
        /// <returns></returns>
        private bool SetCharacterState(SharedDataType<bool> state, float stateTime, float stateImmunityTime, ref float stateImmunityEndTimestamp)
        {
            if (stateImmunityEndTimestamp <= Time.time)
            {
                StartCoroutine(SetStateForTime(state, stateTime));
                stateImmunityEndTimestamp = Time.time + stateTime + stateImmunityTime;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the given state to true, sets it back to false after a period of time.
        /// </summary>
        /// <param name="state">The state to set.</param>
        /// <param name="stateTime">Time in seconds.</param>
        /// <returns></returns>
        private IEnumerator SetStateForTime(SharedDataType<bool> state, float stateTime)
        {
            root.SetData(state, true);
            yield return new WaitForSeconds(stateTime);
            root.SetData(state, false);
        }

        /// <summary>
        /// Set appropriate data when the player fired a shot.
        /// </summary>
        private void HearPlayerShotFired()
        {
            root.SetData(root.sharedData.HasHeardPlayerShot, true);
        }
    }
}