
using UnityEngine;

namespace Game
{
    public class SelectableByPlayer : MonoBehaviour
    {
        private GameObject mSelectionRectangle;

        public void onSelect()
        {
            if (mSelectionRectangle == null) {
                // FIXME: we should probably use object pool instead of prefab instantiation in production
                mSelectionRectangle = Instantiate(GameManager.instance.selectionRectanglePrefab, transform);
            }
        }

        public void onDeselect()
        {
            if (mSelectionRectangle != null) {
                Destroy(mSelectionRectangle);
                mSelectionRectangle = null;
            }
        }

        void OnMouseDown()
        {
            if (!enabled)
                return;

            GameManager.instance.setCurrentSelected(this);
        }
    }
}
