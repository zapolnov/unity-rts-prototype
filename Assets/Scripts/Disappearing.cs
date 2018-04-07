
using UnityEngine;

namespace Game
{
    public class Disappearing : MonoBehaviour
    {
        public float timeout;
        private float mTimeLeft;

        void Awake()
        {
            mTimeLeft = timeout;
        }

        void Update()
        {
            mTimeLeft -= Time.deltaTime;
            if (mTimeLeft <= 0.0f)
                Destroy(gameObject);
            else {
                float scale = 1.0f - (timeout - mTimeLeft) / timeout;
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
