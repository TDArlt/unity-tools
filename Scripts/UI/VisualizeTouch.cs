using UnityEngine;
using System.Collections;

namespace unexpected
{

    public class VisualizeTouch : MonoBehaviour
    {
        // ######################## ENUMS & DELEGATES ######################## //


        // ######################## LINKS TO UNITY OBJECTS ######################## //

        /// <summary>This is the rect transform for the touch graphic</summary>
        public RectTransform TouchGraphic;

        /// <summary>This is the canvas for fading the touch graphic</summary>
        public CanvasGroup TouchGraphicCanvas;


        // ######################## PUBLIC VARS ######################## //


        // ######################## PRIVATE VARS ######################## //




        // ######################## UNITY START & UPDATE ######################## //

        private void Start()
        {
            TouchGraphic.localScale = Vector3.one * 4f;
            TouchGraphicCanvas.alpha = 0;
            TouchGraphicCanvas.gameObject.SetActive(false);
        }

        void Update()
        {
            // Move to correct position
            if (Input.touchCount == 1)
            {
                TouchGraphic.position = Input.touches[0].position;
            }
            else if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
            {
                TouchGraphic.position = Input.mousePosition;
            }


            // React on touch and release
            if ((Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                StopAllCoroutines();
                StartCoroutine(Show());
            }
            else if ((Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Ended) || Input.GetMouseButtonUp(0))
            {
                StopAllCoroutines();
                StartCoroutine(Hide());
            }
        }



        // ######################## COROUTINES ######################## //


        /// <summary>Shows the touch graphic</summary>
        private IEnumerator Show()
        {
            float progress = TouchGraphicCanvas.alpha;
            Vector3 scale = TouchGraphic.localScale;
            Vector3 startScale = Vector3.one * 2f;
            TouchGraphicCanvas.gameObject.SetActive(true);

            while (progress < 1)
            {
                scale = Vector3.Lerp(startScale, Vector3.one, progress);
                TouchGraphic.localScale = scale;
                TouchGraphicCanvas.alpha = progress;

                yield return null;
                progress += Time.deltaTime * 8f;
            }

            TouchGraphic.localScale = Vector3.one;
            TouchGraphicCanvas.alpha = 1;
        }

        /// <summary>Hides the touch graphic</summary>
        private IEnumerator Hide()
        {
            float progress = 0;
            Vector3 scale = TouchGraphic.localScale;
            Vector3 finalScale = Vector3.one * 4f;
            TouchGraphicCanvas.gameObject.SetActive(true);

            while (progress < 1)
            {
                scale = Vector3.Lerp(Vector3.one, finalScale, progress);
                TouchGraphic.localScale = scale;
                TouchGraphicCanvas.alpha = 1f - progress;

                yield return null;
                progress += Time.deltaTime * 4f;
            }

            TouchGraphic.localScale = finalScale;
            TouchGraphicCanvas.alpha = 0;
            TouchGraphicCanvas.gameObject.SetActive(false);
        }

        // ######################## UTILITIES ######################## //


    }
}