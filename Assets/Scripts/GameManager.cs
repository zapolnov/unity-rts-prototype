
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
        public GameObject townHallPrefab;
        public GameObject barracksPrefab;
        public GameObject peasantPrefab;
        public GameObject warriorPrefab;
        public GameObject selectionRectanglePrefab;
        public GameObject clickTargetPrefab;
        public Text goldText;
        public GameObject townHallMenu;
        public Text townHallStatusText;
        public GameObject goldMineMenu;
        public Text goldMineStatusText;
        public GameObject peasantMenu;
        public GameObject peasantBuildBarracksButton;
        public GameObject peasantBuildTownHallButton;
        public GameObject constructionMenu;
        public Text constructionStatusText;
        public GameObject barracksMenu;
        public Text barracksStatusText;
        public int initialGoldCount = 1000;
        public float townHallConstructionTime = 10.0f;
        public float barracksConstructionTime = 5.0f;
        private bool mChoosingConstructionSite;
        private GameObject mObjectToBuild;
        private GameObject mNewBuildingPreview;
        public float mNewBuildingConstructionTime;
        public int mNewBuildingPrice;
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
            destroyCurrentBuildingPreview();

            if (instance == this)
                instance = null;
        }

        private void destroyCurrentBuildingPreview()
        {
            if (mNewBuildingPreview != null) {
                Destroy(mNewBuildingPreview);
                mNewBuildingPreview = null;
            }
        }

        public SelectableByPlayer currentSelected()
        {
            return mCurrentSelected;
        }

        public void setCurrentSelected(SelectableByPlayer selected, bool force = false)
        {
            if (mCurrentSelected != selected || force) {
                townHallMenu.SetActive(false);
                goldMineMenu.SetActive(false);
                peasantMenu.SetActive(false);
                constructionMenu.SetActive(false);
                barracksMenu.SetActive(false);

                if (mCurrentSelected != null)
                    mCurrentSelected.onDeselect();

                mCurrentSelected = selected;

                if (mCurrentSelected != null) {
                    mCurrentSelected.onSelect();

                    var construction = mCurrentSelected.GetComponent<Construction>();

                    townHallMenu.SetActive(mCurrentSelected.GetComponent<TownHall>());
                    goldMineMenu.SetActive(mCurrentSelected.GetComponent<GoldMine>());
                    peasantMenu.SetActive(mCurrentSelected.GetComponent<Peasant>());
                    constructionMenu.SetActive(construction && construction.enabled);
                    barracksMenu.SetActive(mCurrentSelected.GetComponent<Barracks>());
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

        public void onPeasant_BuildBarracks()
        {
            if (!mCurrentSelected)
                return;

            var targetPeasant = mCurrentSelected.GetComponent<Peasant>();
            if (!targetPeasant || !targetPeasant.canBuild())
                return;

            if (targetPeasant.setMode(Peasant.Mode.Default)) {
                mChoosingConstructionSite = true;
                mNewBuildingPrice = 200;
                mNewBuildingConstructionTime = barracksConstructionTime;
                mObjectToBuild = barracksPrefab;
                destroyCurrentBuildingPreview();
            }
        }

        public void onPeasant_BuildTownHall()
        {
            if (!mCurrentSelected)
                return;

            var targetPeasant = mCurrentSelected.GetComponent<Peasant>();
            if (!targetPeasant || !targetPeasant.canBuild())
                return;

            if (targetPeasant.setMode(Peasant.Mode.Default)) {
                mChoosingConstructionSite = true;
                mNewBuildingPrice = 800;
                mNewBuildingConstructionTime = townHallConstructionTime;
                mObjectToBuild = townHallPrefab;
                destroyCurrentBuildingPreview();
            }
        }

        public void onConstruction_CancelConstruction()
        {
            if (!mCurrentSelected)
                return;

            var targetConstruction = mCurrentSelected.GetComponent<Construction>();
            if (!targetConstruction || !targetConstruction.enabled)
                return;

            targetConstruction.cancel();
        }

        public void onBarracks_BuildWarrior()
        {
            if (!mCurrentSelected)
                return;

            var targetBarracks = mCurrentSelected.GetComponent<Barracks>();
            if (!targetBarracks)
                return;

            // FIXME: this value should be configured elsewhere
            if (removeGold(100))
                targetBarracks.buildWarrior();
        }

        void Update()
        {
            if (mChoosingConstructionSite && mObjectToBuild != null) {
                var peasant = (mCurrentSelected != null ? mCurrentSelected.GetComponent<Peasant>() : null);
                if (peasant == null || !peasant.canBuild()) {
                    destroyCurrentBuildingPreview();
                    mObjectToBuild = null;
                    mChoosingConstructionSite = false;
                }
            }

            if (mChoosingConstructionSite && mObjectToBuild != null) {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0.0f;

                if (mNewBuildingPreview == null) {
                    mNewBuildingPreview = Instantiate(mObjectToBuild, transform);
                    mNewBuildingPreview.GetComponent<SelectableByPlayer>().enabled = false;
                }

                mNewBuildingPreview.transform.localPosition = pos;

                if (Input.GetMouseButtonDown(0)) {
                    var peasant = (mCurrentSelected != null ? mCurrentSelected.GetComponent<Peasant>() : null);
                    if (peasant != null && peasant.canBuild() && removeGold(mNewBuildingPrice)) {
                        Vector3 constructionPos = mNewBuildingPreview.transform.position;

                        var newObject = Instantiate(mObjectToBuild);
                        newObject.transform.position = constructionPos;

                        var construction = newObject.AddComponent<Construction>();
                        construction.constructionPrice = mNewBuildingPrice;
                        construction.constructionTime = mNewBuildingConstructionTime;

                        if (peasant.setMode(Peasant.Mode.GoingToConstruction))
                            peasant.targetConstruction = construction;
                    }

                    destroyCurrentBuildingPreview();
                    mObjectToBuild = null;
                    mChoosingConstructionSite = false;
                }
            } else {
                destroyCurrentBuildingPreview();
            }

            if (mCurrentSelected != null) {
                var townHall = mCurrentSelected.GetComponent<TownHall>();
                if (townHall)
                    townHallStatusText.text = townHall.statusText();

                var goldMine = mCurrentSelected.GetComponent<GoldMine>();
                if (goldMine)
                    goldMineStatusText.text = goldMine.statusText();

                var barracks = mCurrentSelected.GetComponent<Barracks>();
                if (barracks)
                    barracksStatusText.text = barracks.statusText();

                var construction = mCurrentSelected.GetComponent<Construction>();
                if (construction && construction.enabled)
                    constructionStatusText.text = construction.statusText();

                var peasant = mCurrentSelected.GetComponent<Peasant>();
                peasantBuildBarracksButton.SetActive(peasant && peasant.canBuild());
                peasantBuildTownHallButton.SetActive(peasant && peasant.canBuild());
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
