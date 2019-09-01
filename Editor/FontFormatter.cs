using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FontFormatter : EditorWindow
{
    private const string editorPrefsKey = "Utilities.FontFormatter";
    private const string menuItemName = "Utilities/Font Formatter";
    private Font selectedFont;

    private int fontSize;

    private Color fontColor;
    
    private string[] fontStyleOptions = new string[]
    {
        "Normal", "Bold", "Italic", "BoldItalic", 
    };
    private int fontStyle;
    
    private bool includePrefabs;

    [MenuItem(menuItemName)]
    public static void DisplayWindow()
    {
        var window = GetWindow<FontFormatter>(true, "Font Formatter");

        var position = window.position;
        // position.size = new Vector2(300, 140);
        window.maxSize = new Vector2(300, 160);
        window.minSize = window.maxSize;
        position.center = new Rect(0,0,Screen.currentResolution.width, Screen.currentResolution.height).center;
        window.position = position;
        window.Show();
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
     var path = EditorPrefs.GetString(editorPrefsKey+".selectedFont");
     if(path != string.Empty)
        selectedFont = AssetDatabase.LoadAssetAtPath<Font>(path)??Resources.GetBuiltinResource<Font>(path);
        fontSize = EditorPrefs.GetInt(editorPrefsKey+".fontSize", 18);
        includePrefabs = EditorPrefs.GetBool(editorPrefsKey+".includePrefabs", false);
        fontStyle = EditorPrefs.GetInt(editorPrefsKey+".fontStyle", 0);
        fontColor = Color.black;
    }

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// This function can be called multiple times per frame (one call per event).
    /// </summary>
    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        selectedFont = (Font)EditorGUILayout.ObjectField("Select Font", selectedFont, typeof(Font), false);
        fontSize = (int)EditorGUILayout.IntField("Font Size", fontSize);
        fontColor = (Color)EditorGUILayout.ColorField("Font Color", fontColor);
        fontStyle = EditorGUILayout.Popup("Font Style", fontStyle, fontStyleOptions);
        includePrefabs = EditorGUILayout.ToggleLeft("Include Prefabs", includePrefabs);
        if(EditorGUI.EndChangeCheck())
        {
             EditorPrefs.SetString(editorPrefsKey + ".destination", GetAssetPath(selectedFont, "ttf"));
             EditorPrefs.SetBool(editorPrefsKey + ".includePrefabs", includePrefabs);
        }
        if (GUILayout.Button("Replace Selected (Hierarchy only)"))
        {
            ReplaceSelected();
        }
        if (GUILayout.Button("Replace Selected Children (Hierarchy only)"))
        {
            ReplaceSelectedChildren();
        }
        if (GUILayout.Button("Replace All (All Scenes + Hierarchy)"))
        {
            ReplaceAll(selectedFont, includePrefabs);
        }
    }

    private void ReplaceSelected()
    {
        List<Text> textComponents = new List<Text>();
        foreach (GameObject item in Selection.objects)
        {
            if(item.GetComponent<Text>())
            {
                textComponents.Add(item.GetComponent<Text>());
            }
        }
        int replacedFonts = SetFont(selectedFont, textComponents);
        Debug.LogFormat("Replaced {0} font(s), from hierarchy", replacedFonts);
    }

    private void ReplaceSelectedChildren()
    {
        List<Text> textComponents = new List<Text>();
        foreach (GameObject item in Selection.objects)
        {
            Text[] textComp = item.GetComponentsInChildren<Text>(true);
            foreach (Text textItem in textComp)
            {
                textComponents.Add(textItem);
            }
        }
        int replacedFonts = SetFont(selectedFont, textComponents);
        Debug.LogFormat("Replaced {0} font(s), from hierarchy", replacedFonts);
    }
    private void ReplaceAll(Font targetFont, bool includePrefabs)
     {
         var sceneMatches = 0;
         for (var i = 0; i < SceneManager.sceneCount; i++)
         {
             var scene = SceneManager.GetSceneAt(i);
             var gos = new List<GameObject>(scene.GetRootGameObjects());
             foreach (var go in gos)
             {
                 sceneMatches += SetFont(targetFont, go.GetComponentsInChildren<Text>(true));
             }
         }
 
         if (includePrefabs)
         {
             var prefabMatches = 0;
             var prefabs =
                 AssetDatabase.FindAssets("t:Prefab")
                     .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));
             foreach (var prefab in prefabs)
             {
                 prefabMatches += SetFont(targetFont, prefab.GetComponentsInChildren<Text>(true));
             }
 
             Debug.LogFormat("Replaced {0} font(s), {1} in scenes, {2} in prefabs", sceneMatches + prefabMatches, sceneMatches, prefabMatches);
         }
         else
         {
             Debug.LogFormat("Replaced {0} font(s) in scenes", sceneMatches);
         }
     }
 
     private int SetFont(Font targetFont, IEnumerable<Text> texts)
     {
         var fontReplaced = 0;
         foreach (var text in texts)
         {
             text.font = targetFont;
             text.fontSize = fontSize;
             text.color = fontColor;
             text.fontStyle = GetFontStyle(fontStyle);
             fontReplaced++;
             if (GUI.changed) EditorUtility.SetDirty (text);
         }
        //  SceneView.RepaintAll();
         
         return fontReplaced;
     }
    
    private FontStyle GetFontStyle(int fontStyleIndex)
    {
        switch(fontStyleIndex)
        {
            case 0:
                return FontStyle.Normal;
            case 1:
                return FontStyle.Bold;
            case 2:
                return FontStyle.Italic;
            case 3:
                return FontStyle.BoldAndItalic;
            default:
                return FontStyle.Normal;
        }
    }
     private static string GetAssetPath(Object assetObject, string defaultExtension)
     {
         var path = AssetDatabase.GetAssetPath(assetObject);
         if (path.StartsWith("Library/", System.StringComparison.InvariantCultureIgnoreCase))
             path = assetObject.name + "." + defaultExtension;
         return path;
     }
}
