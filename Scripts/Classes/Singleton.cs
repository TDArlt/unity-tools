using UnityEngine;

namespace unexpected
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;

        /// <summary>The instance of this one. Might be null, if there is no instance yet</summary>
        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>Returns whether the instance has been initialized or not.</summary>
        public static bool IsInitialized
        {
            get
            {
                return Instance != null;
            }
        }

        /// <summary>
        /// Unity Awake method: Initialize Instance.
        /// Scripts that extend Singleton should be sure to call base.Awake() to ensure the static Instance reference is properly created.
        /// </summary>
        protected virtual void Awake()
        {
            if (IsInitialized && Instance != this)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(this);
                }
                else
                {
                    Destroy(this);
                }

                Debug.LogErrorFormat("Trying to instantiate a second instance of singleton class {0}. Additional Instance was destroyed", GetType().Name);
            }
            else if (!IsInitialized)
            {
                instance = (T)this;
            }
        }

        /// <summary>
        /// Called when this object gets destroyed. As we have no instance anymore then, clean up the var
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                instance = null;
            }
        }
    }
}