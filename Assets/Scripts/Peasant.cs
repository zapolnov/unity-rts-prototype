
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
        }

        private Mode mMode = Mode.Default;
        private NavMeshAgent mNavMeshAgent;
        public GoldMine goldMine;
        public int goldCarryCapacity = 10;
        public int carriesGold = 0;

        void Awake()
        {
            mNavMeshAgent = GetComponent<NavMeshAgent>();
        }

        public bool setMode(Mode mode)
        {
            if (mode == mMode)
                return true;

            switch (mode) {
                case Mode.Default:
                    if (mMode != Mode.CollectingGold_InsideMine) {
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
            }
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
