
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class NavMeshPositionSetter : MonoBehaviour
    {
        private NavMeshAgent mAgent;
        private Vector3 mPosition;

        void Awake()
        {
            mAgent = this.GetOrAddComponent<NavMeshAgent>();
            mAgent.updatePosition = false;
            mAgent.updateRotation = false;
            mAgent.updateUpAxis = false;

            mPosition = transform.localPosition;
            mAgent.nextPosition = new Vector3(mPosition.x, 0.0f, mPosition.y);
        }

        void Update()
        {
            if (mAgent.hasPath) {
                Vector3 pos = mAgent.nextPosition;
                mPosition.x = pos.x;
                mPosition.y = pos.z;
            } else {
                mAgent.nextPosition = new Vector3(mPosition.x, 0.0f, mPosition.y);
            }

            transform.localPosition = mPosition;
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }

        void LateUpdate()
        {
            Update();
        }
    }
}
