using UnityEngine;

namespace UnityMovementAI
{
    public class ArriveUnit : MonoBehaviour
    {

        public Transform targetPosition;

        SteeringBasics steeringBasics;

        void Start()
        {
            steeringBasics = GetComponent<SteeringBasics>();
        }

        void FixedUpdate()
        {
            Vector3 direction = transform.position - targetPosition.position;
            direction.y = 0f;
            if (direction.magnitude > steeringBasics.targetRadius)
            {
                Debug.Log("go0");
                Vector3 accel = steeringBasics.Arrive(targetPosition.position);

                steeringBasics.Steer(accel);
                steeringBasics.LookWhereYoureGoing();
            }
        }
    }
}