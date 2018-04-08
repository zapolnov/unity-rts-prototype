
using UnityEngine;
using UnityEngine.AI;
using System;

namespace Game
{
    public class Peasant : MonoBehaviour
    {
        public enum Mode
        {
            Default,
            CollectingGold_GoingToMine,
            CollectingGold_InsideMine,
            CollectingGold_GoingToTownHall,
            GoingToConstruction,
            Constructing,
        }

        private Mode mMode = Mode.Default;
        private NavMeshAgent mNavMeshAgent;
        private NavMeshPositionSetter mNavMeshPosition;
        private Animator mAnimator;
        public GoldMine goldMine;
        public Construction targetConstruction;
        public int goldCarryCapacity = 10;
        public int carriesGold = 0;

        void Awake()
        {
            mNavMeshAgent = GetComponent<NavMeshAgent>();
            mNavMeshPosition = GetComponent<NavMeshPositionSetter>();
            mAnimator = GetComponent<Animator>();
        }

        public bool setMode(Mode mode)
        {
            if (mode == mMode)
                return true;

            switch (mode) {
                case Mode.Default:
                case Mode.GoingToConstruction:
                    if (mMode != Mode.CollectingGold_InsideMine && mMode != Mode.Constructing) {
                        mMode = mode;
                        return true;
                    }
                    return false;

                case Mode.CollectingGold_GoingToMine:
                    mMode = mode;
                    return true;
            }

            return false;
        }

        public bool canBuild()
        {
            switch (mMode) {
                case Mode.Default:
                case Mode.CollectingGold_GoingToMine:
                case Mode.CollectingGold_GoingToTownHall:
                case Mode.GoingToConstruction:
                    return true;

                case Mode.CollectingGold_InsideMine:
                case Mode.Constructing:
                    break;
            }

            return false;
        }

        void Update()
        {
            switch (mMode) {
                case Mode.CollectingGold_InsideMine:
                    if (!goldMine.hasGuy(gameObject)) {
                        var townhall = GameManager.instance.findNearest<TownHall>(transform.localPosition);
                        if (townhall == null)
                            mMode = Mode.Default;
                        else {
                            Vector3 pos = townhall.transform.localPosition;
                            Vector2 pos2D = new Vector2(pos.x, pos.y);
                            if (!GameManager.instance.navigateAgentToPos(mNavMeshAgent, pos2D)) {
                                mMode = Mode.Default;
                                goldMine = null;
                            } else
                                mMode = Mode.CollectingGold_GoingToTownHall;
                        }
                    }
                    break;

                case Mode.GoingToConstruction:
                    if (!targetConstruction || targetConstruction.builder != null || !targetConstruction.enabled)
                        mMode = Mode.Default;
                    else if (!mNavMeshPosition.isFollowingPath()) {
                        Vector3 pos = targetConstruction.transform.position;
                        GameManager.instance.navigateAgentToPos(mNavMeshAgent, new Vector2(pos.x, pos.y));
                    }
                    break;

                case Mode.Constructing:
                    if (targetConstruction == null || targetConstruction.builder == null) {
                        gameObject.SetActive(true);
                        mMode = Mode.Default;
                    }
                    break;
            }

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
            mAnimator.SetBool("hasGold", carriesGold > 0);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (mMode) {
                case Mode.CollectingGold_GoingToMine:
                    var collidingGoldMine = collision.gameObject.GetComponent<GoldMine>();
                    if (collidingGoldMine) {
                        if (collidingGoldMine.goldLeft <= 0)
                            mMode = Mode.Default;
                        else {
                            if (carriesGold < goldCarryCapacity) {
                                int amount = Math.Min(collidingGoldMine.goldLeft, goldCarryCapacity - carriesGold);
                                carriesGold += amount;
                                collidingGoldMine.goldLeft -= amount;
                            }
                            mMode = Mode.CollectingGold_InsideMine;
                            collidingGoldMine.addGuy(gameObject);
                            goldMine = collidingGoldMine;
                        }
                    }
                    break;

                case Mode.GoingToConstruction:
                    var collidingConstruction = collision.gameObject.GetComponent<Construction>();
                    if (collidingConstruction && collidingConstruction.enabled && collidingConstruction == targetConstruction) {
                        if (collidingConstruction.builder != null)
                            mMode = Mode.Default;
                        else {
                            mMode = Mode.Constructing;
                            collidingConstruction.builder = this;
                            gameObject.SetActive(false);
                        }
                    }
                    break;

                case Mode.CollectingGold_GoingToTownHall:
                    var townHall = collision.gameObject.GetComponent<TownHall>();
                    if (townHall) {
                        if (carriesGold <= 0)
                            mMode = Mode.Default;
                        else {
                            GameManager.instance.addGold(carriesGold);
                            carriesGold = 0;

                            if (goldMine == null)
                                mMode = Mode.Default;
                            else {
                                Vector3 pos = goldMine.transform.localPosition;
                                Vector2 pos2D = new Vector2(pos.x, pos.y);
                                if (!GameManager.instance.navigateAgentToPos(mNavMeshAgent, pos2D))
                                    mMode = Mode.Default;
                                else
                                    mMode = Mode.CollectingGold_GoingToMine;
                            }
                        }
                    }
                    break;
            }
        }
    }
}
