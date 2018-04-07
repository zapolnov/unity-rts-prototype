
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class NavMeshPositionSetter : MonoBehaviour
    {
        private NavMeshAgent mAgent;
        private Vector3 mPosition;
        public int rotation;

        void Awake()
        {
            mAgent = this.GetOrAddComponent<NavMeshAgent>();
            mAgent.updatePosition = false;
            mAgent.updateRotation = false;
            mAgent.updateUpAxis = false;

            mPosition = transform.localPosition;
            mAgent.nextPosition = new Vector3(mPosition.x, 0.0f, mPosition.y);
        }

        public void setPosition(Vector3 pos)
        {
            mPosition = pos;
            mAgent.nextPosition = pos;
        }

        public bool isFollowingPath()
        {
            if (!gameObject.activeInHierarchy)
                return false;

            if (!mAgent.pathPending) {
                if (mAgent.remainingDistance <= mAgent.stoppingDistance) {
                    if (!mAgent.hasPath || mAgent.velocity.sqrMagnitude < 0.0001f)
                        return false;
                }
            }

            return true;
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

            float angle = Vector3.Angle(mAgent.velocity.normalized, transform.forward);
            if (mAgent.velocity.normalized.x < transform.forward.x)
                angle *= -1.0f;
            angle = (angle + 180.0f) % 360.0f;
            rotation = ((int)(angle * 8.0f / 360.0f) + 4) % 8;

            transform.localPosition = mPosition;
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }

        void LateUpdate()
        {
            Update();
        }
    }
}
