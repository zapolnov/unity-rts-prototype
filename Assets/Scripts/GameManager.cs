
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

        void Update()
        {
            if (Input.GetMouseButtonDown(1)) {
                if (mCurrentSelected != null) {
                    NavMeshAgent agent = mCurrentSelected.GetComponent<NavMeshAgent>();
                    if (agent != null) {
                        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        NavMeshPath path = new NavMeshPath();
                        agent.CalculatePath(new Vector3(pos.x, 0.0f, pos.y), path);
                        if (path.status != NavMeshPathStatus.PathInvalid) {
                            spawnClickTarget(new Vector2(pos.x, pos.y));
                            agent.SetPath(path);
                        }
                    }
                }
            }
        }
    }
}
