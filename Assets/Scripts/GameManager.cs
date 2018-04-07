
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public Tilemap tilemap;
        public GameObject peasantPrefab;
        public GameObject selectionRectanglePrefab;
        public GameObject clickTargetPrefab;
        public Text goldText;
        public GameObject townHallMenu;
        public Text townHallStatusText;
        public int initialGoldCount = 1000;
        private SelectableByPlayer mCurrentSelected;
        private int mGoldCount = 0;

        void Awake()
        {
            instance = this;
            mGoldCount = initialGoldCount;
            goldText.text = string.Format("Gold: {0}", mGoldCount);
            setCurrentSelected(null, true);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public void setCurrentSelected(SelectableByPlayer selected, bool force = false)
        {
            if (mCurrentSelected != selected || force) {
                townHallMenu.SetActive(false);
                if (mCurrentSelected != null)
                    mCurrentSelected.onDeselect();

                mCurrentSelected = selected;

                if (mCurrentSelected != null) {
                    mCurrentSelected.onSelect();
                    townHallMenu.SetActive(mCurrentSelected.GetComponent<TownHall>());
                }
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
            goldText.text = string.Format("Gold: {0}", mGoldCount);
        }

        public bool removeGold(int count)
        {
            if (mGoldCount < count)
                return false;
            mGoldCount -= count;
            goldText.text = string.Format("Gold: {0}", mGoldCount);
            return true;
        }

        public void onTownHall_BuildPeasant()
        {
            if (!mCurrentSelected)
                return;

            var targetTownHall = mCurrentSelected.GetComponent<TownHall>();
            if (!targetTownHall)
                return;

            // FIXME: this value should be configured elsewhere
            if (removeGold(40))
                targetTownHall.buildPeasant();
        }

        void Update()
        {
            if (mCurrentSelected != null) {
                var townHall = mCurrentSelected.GetComponent<TownHall>();
                if (townHall)
                    townHallStatusText.text = townHall.statusText();
            }

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
