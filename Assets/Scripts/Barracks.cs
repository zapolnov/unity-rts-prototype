
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class Barracks : MonoBehaviour
    {
        public float warriorBuildTime = 7.0f;
        public Sprite normalSprite;
        public Sprite constructingSprite;
        public GameObject spawnPoint;
        private SpriteRenderer mSpriteRenderer;
        private int mWarriorsQueued = 0;
        private float mTimeLeft = 0.0f;

        void Awake()
        {
            mSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void buildWarrior()
        {
            if (mWarriorsQueued++ == 0)
                mTimeLeft = warriorBuildTime;
        }

        public string statusText()
        {
            if (mWarriorsQueued == 0)
                return "";
            else {
                return string.Format("Building warrior: {0}%, {1} queued",
                    100 - (int)(mTimeLeft * 100.0f / warriorBuildTime), mWarriorsQueued - 1);
            }
        }

        void Update()
        {
            var construction = GetComponent<Construction>();
            if (construction && construction.enabled)
                mSpriteRenderer.sprite = constructingSprite;
            else
                mSpriteRenderer.sprite = normalSprite;

            if (mWarriorsQueued > 0) {
                mTimeLeft -= Time.deltaTime;
                if (mTimeLeft <= 0) {
                    Vector3 dest = spawnPoint.transform.position;
                    Vector3 dir = (dest - transform.position).normalized;

                    // FIXME: use object pool
                    var warrior = Instantiate(GameManager.instance.warriorPrefab);
                    warrior.GetComponent<NavMeshPositionSetter>().setPosition(dest - dir * 0.6f);
                    GameManager.instance.navigateAgentToPos(warrior.GetComponent<NavMeshAgent>(),
                        new Vector2(dest.x, dest.z));

                    if (--mWarriorsQueued > 0)
                        mTimeLeft = warriorBuildTime;
                }
            }
        }
    }
}
