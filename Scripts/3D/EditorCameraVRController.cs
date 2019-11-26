
using UnityEngine;
using System.Collections;


namespace unexpected
{
    /// <summary>Note that this script will only work in editor mode, not in a build</summary>
    public class EditorCameraVRController : MonoBehaviour
    {
#if UNITY_EDITOR

        // ######################## LINKS TO UNITY OBJECTS ######################## //


        /// <summary>The key for enabling all the VR-control functions</summary>
        public KeyCode ControlKey = KeyCode.LeftShift;
        
        /// <summary>The key for resetting the cameras.</summary>
        public KeyCode ResetKey = KeyCode.R;

        /// <summary>The distance multiplier this camera may move per second at a maximum</summary>
        public float MoveDistance = 10f;


        // ######################## PRIVATE VARS ######################## //

        /// <summary>All the transform nodes that should be controlled by this script</summary>
        public Transform[] TheCamsForVR;

        /// <summary>Stores the position of the mouse at the previous frame</summary>
        private Vector3 lastMousePos = Vector3.zero;

        /// <summary>Stores the initial rotation of each transform</summary>
        private Quaternion[] initialRotations;

        /// <summary>Stores the initial position of each transform</summary>
        private Vector3[] initialPositions;


        // ######################## UNITY START & UPDATE ######################## //

        void Start() { Init(); }

        void Update() { DoEditorVR();  }



        // ######################## INITS ######################## //

        /// <summary>Does the init for this behaviour</summary>
        private void Init()
        {
            // If there was nothing selected, this script controls the game object that it is assigned to
            if (TheCamsForVR.Length == 0)
                TheCamsForVR = new Transform[] { this.transform };


            // Collect initial values
            initialRotations = new Quaternion[TheCamsForVR.Length];
            initialPositions = new Vector3[TheCamsForVR.Length];
            for (int i = 0; i < TheCamsForVR.Length; ++i)
            {
                initialRotations[i] = TheCamsForVR[i].rotation;
                initialPositions[i] = TheCamsForVR[i].position;
            }
        }


        /// <summary>Reset all objects' positions and rotations</summary>
        public void ResetPositions()
        {
            for (int i = 0; i < TheCamsForVR.Length; ++i)
            {
                TheCamsForVR[i].rotation = initialRotations[i];
                TheCamsForVR[i].position = initialPositions[i];
            }
        }


        // ######################## FUNCTIONALITY ######################## //

        /// <summary>Does everything that is needed for the VR-camera control in editor</summary>
        private void DoEditorVR()
        {
            // Only do something if the control key was pressed
            if (Input.GetKey(ControlKey))
            {
                // Move depending on axis
                if (Mathf.Abs(Input.GetAxis("Horizontal")) > .01f || Mathf.Abs(Input.GetAxis("Vertical")) > .01)
                {
                    Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                    for (int i = 0; i < TheCamsForVR.Length; ++i)
                    {
                        Vector3 newPos = Vector3.MoveTowards(TheCamsForVR[i].position, TheCamsForVR[i].position + TheCamsForVR[i].forward * 10f * MoveDistance, move.y * Time.deltaTime * MoveDistance);
                        newPos = Vector3.MoveTowards(newPos, TheCamsForVR[i].position + TheCamsForVR[i].right * 10f * MoveDistance, move.x * Time.deltaTime * MoveDistance);

                        TheCamsForVR[i].position = newPos;
                    }
                }

                // Rotate depending on mouse move
                if (Vector2.Distance(Input.mousePosition, lastMousePos) > .01f)
                {
                    Vector2 rotate = Input.mousePosition - lastMousePos;

                    for (int i = 0; i < TheCamsForVR.Length; ++i)
                    {
                        TheCamsForVR[i].Rotate(-rotate.y * Time.deltaTime * 2f, rotate.x * Time.deltaTime * 2f, 0, Space.Self);
                        Quaternion reRotate = Quaternion.Euler(TheCamsForVR[i].rotation.eulerAngles.x, TheCamsForVR[i].rotation.eulerAngles.y, 0);
                        TheCamsForVR[i].rotation = reRotate;
                    }
                }


                // Reset, if reset key was pressed
                if (Input.GetKeyUp(ResetKey))
                    ResetPositions();
            }


            // Store the mouse position (always)
            lastMousePos = Input.mousePosition;
        }

#endif
    }
}
