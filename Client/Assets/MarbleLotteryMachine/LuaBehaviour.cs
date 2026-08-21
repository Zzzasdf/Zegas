using UnityEngine;

public class LuaBehaviour: MonoBehaviour
{
    public static T getLuaBehaviour<T>(GameObject prefab_GameObject) where T : LuaBehaviour
    {
        return prefab_GameObject.GetComponent<T>();
    }
}
