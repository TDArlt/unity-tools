using UnityEngine;
using System.Collections;

namespace unexpected
{
    public class MouseKiller : MonoBehaviour
    {
        // ######################## ENUMS & DELEGATES ######################## //


        // ######################## LINKS TO UNITY OBJECTS ######################## //


        // ######################## PUBLIC VARS ######################## //

        public Texture2D CursorTex;

        // ######################## UNITY START & UPDATE ######################## //

        void Start()
        {
            StartCoroutine(LateInit());

        }

        /// <summary>Initializes a moment later to enable other scripts to be set up correctly</summary>
        /// <returns></returns>
        private IEnumerator LateInit()
        {
            yield return null;


            if (!Debugger.DebugEnabled && !Application.isEditor)
            {
                // Lock down cursor to game window, but only if there is just a single instance
                if (!DisplaySetup.UsesMultidisplay)
                    Cursor.lockState = CursorLockMode.Confined;

                // Hide cursor brutally
                Cursor.SetCursor(CursorTex, Vector2.zero, CursorMode.ForceSoftware);
                Cursor.visible = false;
            }

        }


        private void Update()
        {
            // Enable showing the cursor, if user presses space bar
            if (!Debugger.DebugEnabled && !Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    Cursor.visible = true;

                }
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    Cursor.SetCursor(CursorTex, Vector2.zero, CursorMode.ForceSoftware);
                    Cursor.visible = false;
                }
            }
        }
    }
}