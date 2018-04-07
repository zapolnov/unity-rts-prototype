
using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public class GoldMine : MonoBehaviour
    {
        public class GuyInside
        {
            public float timeLeft;
            public GuyInside(float t) { timeLeft = t; }
        }

        public Sprite normalState;
        public Sprite activeState;
        private SpriteRenderer mSpriteRenderer;
        public float miningTime = 3.0f;
        public int goldLeft = 10000;
        private Dictionary<GameObject, GuyInside> guysInside = new Dictionary<GameObject, GuyInside>();

        void Awake()
        {
            mSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void addGuy(GameObject guy)
        {
            if (!guysInside.ContainsKey(guy)) {
                guysInside.Add(guy, new GuyInside(miningTime));
                guy.SetActive(false);
            }
        }

        public bool hasGuy(GameObject guy)
        {
            return guysInside.ContainsKey(guy);
        }

        void Update()
        {
            // FIXME: filling a set each update creates a pressure on the GC, but who cares in the prototype :)
            HashSet<GameObject> guysDone = new HashSet<GameObject>();

            foreach (var it in guysInside) {
                it.Value.timeLeft -= Time.deltaTime;
                if (it.Value.timeLeft <= 0.0f)
                    guysDone.Add(it.Key);
            }

            foreach (var it in guysDone) {
                it.SetActive(true);
                guysInside.Remove(it);
            }

            mSpriteRenderer.sprite = (guysInside.Count > 0 ? activeState : normalState);
        }
    }
}
