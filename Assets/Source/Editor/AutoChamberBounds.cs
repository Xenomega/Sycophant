using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Globalization;
using System;
using UnityEngine.SceneManagement;

public class AutoChamberBounds : Editor
{
    public class SceneViewExtenderEditorIOHooks : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            int uniqueId = 0;
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefab in prefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefab);
                UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object obj in objects)
                {
                    GameObject gameObject = obj as GameObject;
                    if (gameObject != null)
                    {
                        // If any object has a chamber component..
                        Chamber chamber = gameObject.GetComponent<Chamber>();
                        if (chamber != null)
                        {
                            // Set position and orientation.
                            chamber.transform.position = Vector3.zero;
                            chamber.transform.rotation = Quaternion.identity;
                            chamber.transform.localScale = Vector3.one;

                            // Recalculate bounds.
                            Bounds bounds = new Bounds();
                            Renderer[] renderers = chamber.GetComponentsInChildren<Renderer>();
                            foreach (Renderer renderer in renderers)
                            {
                                if (renderer != null)
                                    bounds.Encapsulate(renderer.bounds);
                            }
                            chamber.Bounds = bounds;
                        }
                    }
                }
                uniqueId++;
            }
            return paths;
        }
    }
}

