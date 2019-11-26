using UnityEngine;
using UnityEditor;

namespace unexpected.EditorUtilities
{
    public class EditorUtilities : ScriptableObject
    {
        [MenuItem("Tools/Reset UserPreferences")]
        static void ResetUserPreferences()
        {
            UserPreferences.DeleteAll(true);
        }
    }
}