
using UnityEngine;
using UnityEngine.AI;
using System;

namespace Game
{
    public class Warrior : MonoBehaviour
    {
        private NavMeshPositionSetter mNavMeshPosition;
        private Animator mAnimator;

        void Awake()
        {
            mNavMeshPosition = GetComponent<NavMeshPositionSetter>();
            mAnimator = GetComponent<Animator>();
        }

        void Update()
        {
            int angle = mNavMeshPosition.rotation;
            if (angle <= 4) {
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            } else {
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                angle -= 4;
                if (angle == 1)
                    angle = 3;
                else if (angle == 3)
                    angle = 1;
            }

            mAnimator.SetInteger("angle", angle);
            mAnimator.SetBool("walking", mNavMeshPosition.isFollowingPath());
        }
    }
}
