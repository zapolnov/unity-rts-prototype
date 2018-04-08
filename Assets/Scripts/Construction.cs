
using UnityEngine;

namespace Game
{
    public class Construction : MonoBehaviour
    {
        public float constructionTime;
        public int constructionPrice;
        public Peasant builder;
        private float mTimeLeft;

        void Start()
        {
            mTimeLeft = constructionTime;
        }

        public string statusText()
        {
            return string.Format("Construction: {0}%", 100 - (int)(mTimeLeft * 100.0f / constructionTime));
        }

        public void cancel()
        {
            if (enabled) {
                GameManager.instance.addGold(constructionPrice);
                if (GameManager.instance.currentSelected() == GetComponent<SelectableByPlayer>())
                    GameManager.instance.setCurrentSelected(null);
                if (builder != null) {
                    builder.gameObject.SetActive(true);
                    builder = null;
                }
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (builder == null)
                return;

            mTimeLeft -= Time.deltaTime;
            if (mTimeLeft <= 0.0f) {
                if (builder != null) {
                    builder.gameObject.SetActive(true);
                    builder = null;
                }

                enabled = false;

                var selectable = GetComponent<SelectableByPlayer>();
                if (GameManager.instance.currentSelected() == selectable)
                    GameManager.instance.setCurrentSelected(selectable, true);

                return;
            }
        }
    }
}
