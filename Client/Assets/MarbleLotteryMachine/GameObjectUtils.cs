using UnityEngine;

namespace LuaFramework
{
    public class GameObjectUtils
    {
        public static bool IsDestroyed(Object go)
        {
            return !go;
        }
    }
}
