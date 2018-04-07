
using UnityEngine;

namespace Game
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go)
            where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null)
                c = go.AddComponent<T>();
            return c;
        }
    }
}
