using MonsterStates;
using UnityEngine;

namespace MonsterStates
{
    public class Return : MonsterState
    {
        private Transform spawnPoint;
        private float arrivalThreshold = 0.5f;

        private bool isTurning;
        public Return(MonsterStateMachine monster) : base(monster)
        {
            isTurning = false;
        }
        public override void Enter()
        {


            isTurning = false;
            Monster.status.detection_gauge = 0f;
            Monster.StartDetection();
            this.spawnPoint = Monster.spawnpoint;
            CheckDirection();

        }
        public override void Exit() {
           
        }
        private void CheckDirection()
        {

            if (Monster.spawnpoint == null) return;

            isTurning = Monster.TurnToward(Monster.spawnpoint.position.x, Monster.ReturnAnim);
        }
        public override void LogicUpdate()
        {
            float distanceToSpawn = Vector3.Distance(Monster.transform.position, spawnPoint.transform.position);

            // 2. 도착했다면 idle 상태로 전환
            if (distanceToSpawn <= arrivalThreshold)
            {
                Monster.OnReturnArrived();
            }
        }
        public override void PhysicalUpdate()
        {
            if (!isTurning)
            {
                Vector3 direction = (spawnPoint.transform.position - Monster.transform.position).normalized;

                
                Monster.Rb.linearVelocity = new Vector3(
                    direction.x * Monster.ReturnSpeed,
                    Monster.Rb.linearVelocity.y,
                    0f
            );
            }


 
        }

        public override void OnTurnAnimationFinished()
        {
            if (Monster.spawnpoint != null)
                Monster.FinishTurn(Monster.spawnpoint.position.x, Monster.ReturnAnim);
            isTurning = false;
        }

    }
}

