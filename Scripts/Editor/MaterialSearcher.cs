using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace unexpected
{
    public class MaterialSearcher : EditorWindow
    {

        /// <summary>These are the materials we look for in the scene</summary>
        private static List<Material> matsToLookFor = new List<Material>();

        /// <summary>These are the objects that have this material</summary>
        private static List<Renderer> objWithMats = new List<Renderer>();

        /// <summary>The material that should be used for replacement</summary>
        private static Material replaceMat;


        private static Vector2 scrollPos = Vector2.zero;

        /// <summary>Open up window</summary>
        [MenuItem("Tools/Material Search")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            MaterialSearcher window = (MaterialSearcher)EditorWindow.GetWindow(typeof(MaterialSearcher));
            window.titleContent = new GUIContent("Material Search");
            window.Show();
        }



        /// <summary>UI</summary>
        private void OnGUI()
        {
            bool listHasChanged = false;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Materials to look for:", EditorStyles.boldLabel);

            // Pick from selection
            if (GUILayout.Button("Pick from current selection"))
            {
                matsToLookFor.Clear();
                
                for (int i = 0; i < Selection.gameObjects.Length; ++i)
                {
                    Renderer ren = Selection.gameObjects[i].GetComponent<Renderer>();
                    if (ren != null)
                    {
                        for (int j = 0; j < ren.sharedMaterials.Length; ++j)
                            if (!matsToLookFor.Contains(ren.sharedMaterials[j]))
                                matsToLookFor.Add(ren.sharedMaterials[j]);
                    }
                }

                listHasChanged = true;
            }


            GUILayout.Space(10);

            // Show list of selected materials
            for (int i = 0; i < matsToLookFor.Count; ++i)
            {
                Material newMat = (Material)EditorGUILayout.ObjectField(matsToLookFor[i], typeof(Material), true);

                if (newMat != matsToLookFor[i])
                {
                    matsToLookFor[i] = newMat;
                    listHasChanged = true;
                }

                // remove empty values
                if (newMat == null)
                    matsToLookFor.RemoveAt(i);
            }

            // Add an empty one at the end
            Material additionalMat = (Material)EditorGUILayout.ObjectField(null, typeof(Material), true);
            // And add this one to our list, if the user does not want it to be emtpy anymore
            if (additionalMat != null)
            {
                if (!matsToLookFor.Contains(additionalMat))
                    matsToLookFor.Add(additionalMat);
                
                listHasChanged = true;
            }


            // Update list of renderers, if necessary
            if (listHasChanged)
                PickAllRenderers();


            GUILayout.Space(20);


            // Show list of renderers

            EditorGUILayout.LabelField("Renderers for these materials in scene:", EditorStyles.boldLabel);

            if (objWithMats.Count > 0)
            {
                // Show list of connected renderers
                for (int i = 0; i < objWithMats.Count; ++i)
                {
                    EditorGUILayout.ObjectField(objWithMats[i], typeof(Renderer), true);
                }

                GUILayout.Space(10);

                // Select button
                if (GUILayout.Button("Select these objects"))
                {
                    // Generate list
                    Object[] selGOs = new GameObject[objWithMats.Count];
                    for (int i = 0; i < objWithMats.Count; ++i)
                        selGOs[i] = objWithMats[i].gameObject;

                    Selection.objects = selGOs;
                }



                GUILayout.Space(30);


                // Material replacement
                EditorGUILayout.LabelField("Material replacement:", EditorStyles.boldLabel);

                EditorGUILayout.LabelField("Replace above materials with:");
                replaceMat = (Material)EditorGUILayout.ObjectField(replaceMat, typeof(Material), true);

                GUILayout.Space(5);

                // Replace button
                if (GUILayout.Button("Replace in current scene!"))
                {
                    int countMats = 0;
                    int countObjs = 0;

                    // Run replacement
                    for (int i = 0; i < objWithMats.Count; ++i)
                    {
                        bool anyReplacment = false;

                        Material[] matsHere = objWithMats[i].sharedMaterials;

                        for (int j = 0; j < matsHere.Length; ++j)
                        {
                            if (matsToLookFor.Contains(objWithMats[i].sharedMaterials[j]))
                            {
                                matsHere[j] = replaceMat;
                                ++countMats;
                                anyReplacment = true;
                            }
                        }

                        if (anyReplacment)
                        {
                            objWithMats[i].sharedMaterials = matsHere;
                            ++countObjs;
                        }
                    }

                    // Inform
                    Debug.LogFormat("Replaced {0} materials in {1} objects", countMats, countObjs);

                    // Update
                    PickAllRenderers();

                    // Set dirty
                    EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }

            } else
            {

                EditorGUILayout.LabelField("This applys to none of the objects in the current scene");

                GUILayout.Space(30);

                EditorGUILayout.LabelField("Replacement is not possible here");
            }


            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }



        /// <summary>Picks all the renderers of our current materials</summary>
        private static void PickAllRenderers()
        {
            // Clean up first
            objWithMats.Clear();


            // Get all game objects of current scene
            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            for (int rID = 0; rID < roots.Length; ++rID)
            {
                // Get all renderers in each root
                Renderer[] rens = roots[rID].GetComponentsInChildren<Renderer>(true);

                for (int renID = 0; renID < rens.Length; ++renID)
                {
                    // Go through every material of every renderer in every root
                    Material[] mats = rens[renID].sharedMaterials;
                    for (int matID = 0; matID < mats.Length; ++matID)
                    {
                        // Check if one material matches
                        if (matsToLookFor.Contains(mats[matID]))
                        {
                            // Yes, it does. Add to our list and don't look any further for this renderer
                            objWithMats.Add(rens[renID]);
                            break;
                        }
                    }
                }
            }
        }

    }
}