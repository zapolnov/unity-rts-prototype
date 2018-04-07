
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public Tilemap tilemap;
        public GameObject selectionRectanglePrefab;
        public GameObject clickTargetPrefab;
        private SelectableByPlayer mCurrentSelected;
        private int mGoldCount = 0;

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public void setCurrentSelected(SelectableByPlayer selected)
        {
            if (mCurrentSelected != selected) {
                if (mCurrentSelected != null)
                    mCurrentSelected.onDeselect();
                mCurrentSelected = selected;
                if (mCurrentSelected != null)
                    mCurrentSelected.onSelect();
            }
        }

        private void spawnClickTarget(Vector2 pos)
        {
            GameObject clickBait = Instantiate(clickTargetPrefab, transform);
            clickBait.transform.localPosition = new Vector3(pos.x, pos.y, 0.0f);
        }

        public bool navigateAgentToPos(NavMeshAgent agent, Vector2 pos)
        {
            if (!agent.gameObject.activeInHierarchy)
                return false;

            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(new Vector3(pos.x, 0.0f, pos.y), path);
            if (path.status != NavMeshPathStatus.PathInvalid)
                return agent.SetPath(path);
            return false;
        }

        public T findNearest<T>(Vector3 pos)
            where T : Component
        {
            T[] candidates = FindObjectsOfType<T>();

            T nearest = null;
            float nearestDistance = 0.0f;

            foreach (var candidate in candidates) {
                float distance = Vector3.SqrMagnitude(pos - candidate.transform.localPosition);
                if (nearest == null || distance < nearestDistance) {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public void addGold(int count)
        {
            mGoldCount += count;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1) && mCurrentSelected != null && mCurrentSelected.isActiveAndEnabled) {
                NavMeshAgent agent = mCurrentSelected.GetComponent<NavMeshAgent>();
                if (agent != null) {
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 pos2D = new Vector2(pos.x, pos.y);

                    Collider2D overlaps = Physics2D.OverlapPoint(pos2D);
                    if (overlaps != null) {
                        GameObject objectAtMouse = overlaps.gameObject;

                        var agentPeasant = mCurrentSelected.GetComponent<Peasant>();
                        var targetGoldMine = objectAtMouse.GetComponent<GoldMine>();
                        var targetTownHall = objectAtMouse.GetComponent<TownHall>();

                        if (targetGoldMine && agentPeasant) {
                            if (agentPeasant.setMode(Peasant.Mode.CollectingGold_GoingToMine)) {
                                if (navigateAgentToPos(agent, pos2D)) {
                                    agentPeasant.goldMine = targetGoldMine;
                                    spawnClickTarget(pos);
                                } else
                                    agentPeasant.setMode(Peasant.Mode.Default);
                            }
                        }

                        if (targetTownHall && agentPeasant) {
                            Peasant.Mode mode = Peasant.Mode.CollectingGold_GoingToTownHall;
                            if (agentPeasant.carriesGold <= 0 || agentPeasant.goldMine == null)
                                mode = Peasant.Mode.Default;
                            if (agentPeasant.setMode(mode)) {
                                if (navigateAgentToPos(agent, pos2D))
                                    spawnClickTarget(pos);
                                else
                                    agentPeasant.setMode(Peasant.Mode.Default);
                            }
                        }
                    } else {
                        if (navigateAgentToPos(agent, pos2D))
                            spawnClickTarget(pos);
                    }
                }
            }
        }
    }
}
