using UnityEngine;
using UnityEditor;

namespace unexpected
{
    public class TransformResetter : ScriptableObject
    {
        /// <summary>Resets the position of the transform node(s)</summary>
        [MenuItem("Tools/Reset Transform/Reset Local Position %&w")]
        public static void ResetPosition()
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                Undo.RecordObject(Selection.gameObjects[i].transform, "Reset transform");
                Selection.gameObjects[i].transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>Resets the position of the transform node(s)</summary>
        [MenuItem("Tools/Reset Transform/Reset Local Rotation %&e")]
        public static void ResetRotation()
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                Undo.RecordObject(Selection.gameObjects[i].transform, "Reset transform");
                Selection.gameObjects[i].transform.localRotation = Quaternion.identity;
            }
        }


        /// <summary>Resets the position of the transform node(s)</summary>
        [MenuItem("Tools/Reset Transform/Reset Local Scale %&r")]
        public static void ResetScale()
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                Undo.RecordObject(Selection.gameObjects[i].transform, "Reset transform");
                Selection.gameObjects[i].transform.localScale = Vector3.one;
            }
        }


        /// <summary>Resets the position of the transform node(s)</summary>
        [MenuItem("Tools/Reset Transform/Reset Everything %&q")]
        public static void ResetEverything()
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                Undo.RecordObject(Selection.gameObjects[i].transform, "Reset transform");
                Selection.gameObjects[i].transform.localScale = Vector3.one;
                Selection.gameObjects[i].transform.localRotation = Quaternion.identity;
                Selection.gameObjects[i].transform.localPosition = Vector3.zero;
            }
        }

    }
}