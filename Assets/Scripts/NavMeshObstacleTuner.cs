
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class NavMeshObstacleTuner : MonoBehaviour
    {
        private NavMeshObstacle mObstacle;

        void Awake()
        {
            mObstacle = GetComponent<NavMeshObstacle>();
            Vector3 size = mObstacle.size;
            mObstacle.size = new Vector3(size.x, 2.0f, size.y);
        }

        void Update()
        {
            Vector3 pos = -transform.localPosition;
            pos.x += transform.localPosition.x;
            pos.z += transform.localPosition.y;
            mObstacle.center = pos;
        }
    }
}
