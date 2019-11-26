using UnityEngine;

namespace unexpected
{
    /// <summary>
    /// Sets custom display properties wherever you need them
    /// </summary>
    public class DisplaySetup : MonoBehaviour
    {
        // ######################## ENUMS & DELEGATES ######################## //


        // ######################## LINKS TO UNITY OBJECTS ######################## //


        // ######################## PUBLIC VARS ######################## //

        /// <summary>Set to true to enable this script</summary>
        public bool OverwriteHere = false;


        /// <summary>Defines, if this application should use multiple displays</summary>
        public bool UseMultipleDisplays = false;

        /// <summary>These are the target sizes you like the displays to have</summary>
        public Vector2[] TargetSizes = new Vector2[] { new Vector2(1920, 1080) };


        /// <summary>Should we make sure that the aspect ration is preserved?</summary>
        public bool PreserveAspect = false;

        /// <summary>Should we make sure that the displays have exact sizes that were defined here?</summary>
        public bool PreserveSizes = false;


        /// <summary>Tells you, if this application uses multiple displays</summary>
        public static bool UsesMultidisplay { get; protected set; }

        // ######################## PRIVATE VARS ######################## //




        // ######################## UNITY START & UPDATE ######################## //

        void Start() { Init(); }
        



        // ######################## INITS ######################## //

        /// <summary>Does the init for this behaviour</summary>
        private void Init()
        {
            if (!OverwriteHere)
                return;

            UsesMultidisplay = UseMultipleDisplays;

            if (UseMultipleDisplays)
            {
                // If we should activate multiple displays, do so
                if (Display.displays.Length < TargetSizes.Length)
                    Debug.LogWarning(Display.displays.Length + " displays connected!");
                else
                    Debug.Log(Display.displays.Length + " displays connected.");
            }


            for (int i = 0; i < TargetSizes.Length; ++i)
            {
                if (Display.displays.Length > i)
                {
                    if (UseMultipleDisplays || i == 0)
                    {
                        // Activate display
                        Display.displays[i].Activate();

                        // Get current resolution
                        int width = Display.displays[i].systemWidth;
                        int height = Display.displays[i].systemHeight;
                        int fullWidth = width;
                        int fullHeight = height;

                        int desiredWidth = (int)TargetSizes[i].x;
                        int desiredHeight = (int)TargetSizes[i].y;



                        if (PreserveAspect && !PreserveSizes)
                        {
                            // We like to preserve the aspect ration. Find out what we should go for

                            if ((float)width / height < (float)desiredWidth / desiredHeight)
                            {
                                // Fit vertically
                                width = fullWidth;
                                height = (width * desiredHeight) / desiredWidth;

                            } else
                            {
                                // Fit horizontally
                                height = fullHeight;
                                width = (height * desiredWidth) / desiredHeight;
                            }
                        } else if (PreserveSizes)
                        {
                            // We only like to preserve the size. Ignore display properties
                            height = desiredHeight;
                            width = desiredWidth;
                        }

                        // If nothing was changed above, it means, we will fill out the full display

                        // Set
                        Debug.LogFormat("Display {0}: Showing on resolution {1}x{2}", i, width, height);
                        Display.displays[i].SetRenderingResolution(width, height);
                    }
                }
            }
        }


        // ######################## FUNCTIONALITY ######################## //


            


        // ######################## UTILITIES ######################## //

        /// <summary>Validation for setting up multiple displays</summary>
        /// <returns>true, if there is something wrong</returns>
        public bool MultidisplaySetupError()
        {
            return UseMultipleDisplays && !OverwriteHere;
        }
    }
}