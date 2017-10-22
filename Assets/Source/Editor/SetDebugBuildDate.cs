using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Globalization;

public class SetDebugBuildDate : Editor
{
    public class SceneViewExtenderEditorIOHooks : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            string month = DateTime.Now.Month.ToString("D2");
            string date = DateTime.Now.Day.ToString("D2");
            string year = DateTime.Now.Year.ToString().Substring(2, 2);

            Development.Debug devInstance = GameObject.FindObjectOfType<Development.Debug>();
            if (devInstance != null)
                devInstance.buildDate = string.Format("{0}_{1}_{2}", new object[] { month, date, year });

            return paths;
        }
        
    }


}

