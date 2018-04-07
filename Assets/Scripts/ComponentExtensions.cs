
using UnityEngine;

namespace Game
{
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this Component component)
            where T : Component
        {
            var c = component.GetComponent<T>();
            if (c == null)
                c = component.gameObject.AddComponent<T>();
            return c;
        }
    }
}
