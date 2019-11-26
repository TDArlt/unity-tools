using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace unexpected
{
    /// <summary>
    /// This provides functionality for all types of touch gestures.
    /// Usage: Call the method for the information you like to have on every frame.
    /// That will deliver you a touch-object that holds all the information you like to have.
    /// </summary>
    public class TouchGestures
    {

        // ############################# SINGLE TOUCHES ############################# //

        /// <summary>
        /// This is an object holding all information about one touch
        /// </summary>
        private class Touch
        {
            /// <summary>This is the time when this touch happened for the first time</summary>
            private float touchBeginTime;
            /// <summary>This is the time when this touch happened for the first time</summary>
            public float TouchBeginTime { get { return touchBeginTime; } }

            /// <summary>This defines the position where this touch began</summary>
            private Vector2 touchBeginPosition;
            /// <summary>This defines the position where this touch began</summary>
            public Vector2 TouchBeginPosition { get { return touchBeginPosition; } }

            /// <summary>This is the time when the last update happened to the touch</summary>
            private float touchRecentTime;
            /// <summary>This is the time when the last update happened to the touch</summary>
            public float TouchRecentTime { get { return touchRecentTime; } }

            /// <summary>This defines the position where this touch was on the most recent moment</summary>
            private Vector2 touchRecentPosition;
            /// <summary>This defines the position where this touch was on the most recent moment</summary>
            public Vector2 TouchRecentPosition
            {
                get { return touchRecentPosition; }
                set
                {
                    // Store deltas
                    deltaMovement = value - touchRecentPosition;
                    deltaTime = Time.time - touchRecentTime;

                    // Set new values
                    touchRecentPosition = value;
                    touchRecentTime = Time.time;
                }
            }

            /// <summary>This shows the delta movement since TouchRecentTime and the moment before that</summary>
            private Vector2 deltaMovement = Vector2.zero;
            /// <summary>This shows the delta movement since TouchRecentTime and the moment before that</summary>
            public Vector2 DeltaMovement { get { return deltaMovement; } }

            /// <summary>This shows the delta time since TouchRecentTime and the moment before that</summary>
            private float deltaTime = 0;
            /// <summary>This shows the delta time since TouchRecentTime and the moment before that</summary>
            public float DeltaTime { get { return deltaTime; } }


            /// <summary>Create a new touch-object</summary>
            /// <param name="startPosition">is the start position this touch is right now</param>
            public Touch(Vector2 startPosition)
            {
                touchRecentPosition = touchBeginPosition = startPosition;
                touchRecentTime = touchBeginTime = Time.time;
            }

            /// <summary>Create a new touch-object</summary>
            /// <param name="startPosition">is the start position this touch is right now</param>
            /// <param name="startTime">is the time that should be defined as start</param>
            public Touch(Vector2 startPosition, float startTime)
            {
                touchRecentPosition = touchBeginPosition = startPosition;
                touchRecentTime = touchBeginTime = startTime;
            }
        }

        // ############################# MAIN LOGIC ############################# //

        /// <summary>This is a dictionary of all available touches at the moment</summary>
        private static Dictionary<int, Touch> touches = new Dictionary<int, Touch>();

        /// <summary>This stores the last time we did an update (preventing multiple updates on one frame)</summary>
        private static float recentUpdate;


        /// <summary>This is the main working function that updates all the touch information</summary>
        public static void UpdateTouches()
        {
            // If the time since last update is less than half a 90fps-frame (5 ms; "almost 0"), don't run update here
            if (Mathf.Abs(recentUpdate - Time.time) < .005f)
                return;

            // Init, if necessary
            if (touches == null)
                touches = new Dictionary<int, Touch>();

            // Save current time
            recentUpdate = Time.time;

            // Disable mouse-interpretation for touches
            Input.simulateMouseWithTouches = false;
            Input.multiTouchEnabled = true;


            // This list will enable to delete touches that are no longer valid.
            List<int> foundIDs = new List<int>();

            // Go through all the touches at the moment
            foreach (UnityEngine.Touch inputTouch in Input.touches)
            {
                int id = inputTouch.fingerId;

                // If this touch already exists in our list, update position
                if (touches.ContainsKey(id))
                {
                    touches[id].TouchRecentPosition = inputTouch.position;
                }
                else
                {
                    // If not in our list, create new
                    touches.Add(id, new Touch(inputTouch.position));
                }

                // If this touch has not just been ended, write this id in our save-list
                if (inputTouch.phase != TouchPhase.Ended)
                    foundIDs.Add(id);
            }

            // Check mouse
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            {
                // Left button clicked: Track as touch
                int mouseID = -42;

                // If this touch already exists in our list, update position
                if (touches.ContainsKey(mouseID))
                {
                    touches[mouseID].TouchRecentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                }
                else
                {
                    // If not in our list, create new
                    touches.Add(mouseID, new Touch(new Vector2(Input.mousePosition.x, Input.mousePosition.y)));
                }

                // Add to save-list
                foundIDs.Add(mouseID);

            }


            // Clean up list
            List<int> deletionKeys = new List<int>();
            foreach (int currID in touches.Keys)
            {
                bool match = false;

                // Find in save list
                foreach (int saveID in foundIDs)
                    if (saveID == currID)
                        match = true;

                // if not found in save list, kill entry
                if (!match)
                    deletionKeys.Add(currID);
            }
            foreach (int delID in deletionKeys)
                touches.Remove(delID);


        }




        // ############################# UTILITY FUNCTIONS ############################# //


        /// <summary>
        /// This will get you the delta values of a swipe gesture (one finger or mouse).
        /// </summary>
        /// <param name="areaToLookAt">This is the area in which the touch need to have started in to count it</param>
        /// <returns>The change of the values since last update and zero if there is no movement or not or more than one touch was identified</returns>
        public static Vector2 GetDeltaSwipe(RectTransform areaToLookAt)
        {
            // Call update
            UpdateTouches();

            // Find out, if we need to look at the touch-start-positions
            if (areaToLookAt != null)
            {
                // Store, if we found ONE (and only a single one!) matching touch
                bool foundOne = false;
                Vector2 returnVal = Vector2.zero;

                foreach (int tID in touches.Keys)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(areaToLookAt, touches[tID].TouchBeginPosition))
                    {
                        // If we already found one, quit
                        if (foundOne)
                            return Vector2.zero;

                        // Otherwise, store
                        returnVal = touches[tID].DeltaMovement;
                        foundOne = true;
                    }
                }

                // If we found a single touch, return value
                if (foundOne)
                    return returnVal;

            }
            // If we are not comparing the start point to a rectTransform, it only makes sense to look at single touches
            else if (touches.Count == 1)
            {
                // Return value (there is only one)
                foreach (int tID in touches.Keys)
                {
                    return touches[tID].DeltaMovement;
                }
            }

            // If we came here, we did not get any value. Send back zero
            return Vector2.zero;
        }

        /// <summary>
        /// This will get you the delta values of a swipe gesture (one finger or mouse).
        /// </summary>
        /// <returns>The change of the values since last update and zero if there is no movement or not or more than one touch was identified</returns>
        public static Vector2 GetDeltaSwipe() { return GetDeltaSwipe(null); }


        /// <summary>
        /// This will get you the delta value of a pinch gesture (two fingers or mouse wheel).
        /// </summary>
        /// <param name="areaToLookAt">This is the area in which the touch need to have started in to count it</param>
        /// <returns>The change of the value since last update and zero if there is no movement or less or more than two touches were identified</returns>
        public static float GetDeltaPinch(RectTransform areaToLookAt)
        {
            // Call update
            UpdateTouches();

            // If we have a mouse wheel change here, we only look at that one and ignore everything else
            if (touches.Count == 0 && (Input.mouseScrollDelta.y > .001f || Input.mouseScrollDelta.y < -.001f))
            {
                // Only look at that, if mouse is in desired area
                if (areaToLookAt == null || RectTransformUtility.RectangleContainsScreenPoint(areaToLookAt, Input.mousePosition))
                    return Input.mouseScrollDelta.y;
            }
            else if (touches.Count > 1)
            {
                // Usually, we only look at this if there is more than one touch

                // Pre-define vars
                Vector2 touchOnePosDelta = Vector2.zero;
                Vector2 touchTwoPosDelta = Vector2.zero;
                Vector2 touchOnePosNow = Vector2.zero;
                Vector2 touchTwoPosNow = Vector2.zero;
                int matchingTouches = 0;

                // Go through all touches
                foreach (int tID in touches.Keys)
                {
                    // Only look at the ones that started in our desired area (or all, if there is no desired area)
                    if (areaToLookAt == null || RectTransformUtility.RectangleContainsScreenPoint(areaToLookAt, touches[tID].TouchBeginPosition))
                    {
                        if (matchingTouches == 0)
                        {
                            // First touch
                            touchOnePosDelta = touches[tID].DeltaMovement;
                            touchOnePosNow = touches[tID].TouchRecentPosition;
                        }
                        if (matchingTouches == 1)
                        {
                            // Second touch
                            touchTwoPosDelta = touches[tID].DeltaMovement;
                            touchTwoPosNow = touches[tID].TouchRecentPosition;
                        }

                        matchingTouches++;
                    }

                    // Only calculate, if we have exactly two matching touches
                    if (matchingTouches == 2)
                    {
                        float nowDistance = Vector2.Distance(touchTwoPosNow, touchOnePosNow);
                        float distanceBefore = Vector2.Distance(touchTwoPosNow - touchTwoPosDelta, touchOnePosNow - touchOnePosDelta);

                        return nowDistance - distanceBefore;
                    }
                }
            }


            // If we came here, we did not get any value. Send back zero
            return 0;

        }

        /// <summary>
        /// This will get you the delta value of a pinch gesture (two fingers or mouse wheel).
        /// </summary>
        /// <returns>The change of the value since last update and zero if there is no movement or less or more than two touches were identified</returns>
        public static float GetDeltaPinch() { return GetDeltaPinch(null); }

    }
}