﻿
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class TownHall : MonoBehaviour
    {
        public float peasantBuildTime = 5.0f;
        public Sprite normalSprite;
        public Sprite constructingSprite;
        public GameObject spawnPoint;
        private SpriteRenderer mSpriteRenderer;
        private int mPeasantsQueued = 0;
        private float mTimeLeft = 0.0f;

        private void Awake()
        {
            mSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void buildPeasant()
        {
            if (mPeasantsQueued++ == 0)
                mTimeLeft = peasantBuildTime;
        }

        public string statusText()
        {
            if (mPeasantsQueued == 0)
                return "";
            else {
                return string.Format("Building peasant: {0}%, {1} queued",
                    100 - (int)(mTimeLeft * 100.0f / peasantBuildTime), mPeasantsQueued - 1);
            }
        }

        void Update()
        {
            var construction = GetComponent<Construction>();
            if (construction && construction.enabled)
                mSpriteRenderer.sprite = constructingSprite;
            else
                mSpriteRenderer.sprite = normalSprite;

            if (mPeasantsQueued > 0) {
                mTimeLeft -= Time.deltaTime;
                if (mTimeLeft <= 0) {
                    Vector3 dest = spawnPoint.transform.position;
                    Vector3 dir = (dest - transform.position).normalized;

                    // FIXME: use object pool
                    var peasant = Instantiate(GameManager.instance.peasantPrefab);
                    peasant.GetComponent<NavMeshPositionSetter>().setPosition(dest - dir * 0.6f);
                    GameManager.instance.navigateAgentToPos(peasant.GetComponent<NavMeshAgent>(),
                        new Vector2(dest.x, dest.z));

                    if (--mPeasantsQueued > 0)
                        mTimeLeft = peasantBuildTime;
                }
            }
        }
    }
}
