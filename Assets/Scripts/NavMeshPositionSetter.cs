
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class NavMeshPositionSetter : MonoBehaviour
    {
        private NavMeshAgent mAgent;

        void Awake()
        {
            mAgent = this.GetOrAddComponent<NavMeshAgent>();
            mAgent.updatePosition = false;
            mAgent.updateRotation = false;
            mAgent.updateUpAxis = false;

            mAgent.nextPosition = transform.localPosition;
        }

        void Update()
        {
            Vector3 pos = mAgent.nextPosition;
            transform.localPosition = new Vector3(pos.x, pos.z, 0.0f);
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }
}
