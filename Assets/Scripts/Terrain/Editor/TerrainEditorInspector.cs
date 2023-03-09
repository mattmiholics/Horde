using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System.Reflection;
using System;
using System.Collections;

[CustomEditor(typeof(TerrainEditor))]
public class TerrainEditorInspector : OdinEditor
{
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    private int index = -1;

    private bool editing = false;

    public override void OnInspectorGUI()
    {
        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        DrawDefaultInspector();

        TerrainEditor terrainEditor = (TerrainEditor)target;

        GUILayout.Space(25);

        GUIStyle style = new GUIStyle();
        style.wordWrap = true;
        style.normal.textColor = Color.cyan;
        GUILayout.Label("\"left-click\" to place block \"shift-left-click\" to remove block. \"ctrl-left-click\" to lock a column \"ctrl-shift-left-click\" to unlock a column. Gizmos must be turned on to work", style, GUILayout.ExpandWidth(true));

        using (new EditorGUILayout.HorizontalScope())
        {
            // Draw Label
            EditorGUILayout.PrefixLabel("Edit");

            // Start Change check to see if we clicked on an already selected button
            EditorGUI.BeginChangeCheck();
            int prevIndex = index;
            // Draw "Toolbar" (a group of selectable buttons). "AppCommand" is the style used for the Tools toolbar
            index = GUILayout.Toolbar(index, new[] { EditorGUIUtility.IconContent("d_editicon.sml") }, "AppCommand");
            // Enter the condition if a button is pressed (even an already selected one)
            if (EditorGUI.EndChangeCheck())
            {
                // if we clicked on the same index, deselect it
                if (index == prevIndex)
                {
                    index = -1;
                    editing = false;
                    terrainEditor.placeProxy.SetActive(false);
                    terrainEditor.removeProxy.SetActive(false);
                    terrainEditor.modifiabilityProxy.SetActive(false);
                }
                else
                {
                    editing = true;
                }
            }
        }
    }

    private void OnSceneGUI()
    {
        if (!editing)
            return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        TerrainEditor terrainEditor = (TerrainEditor)target;

        if (terrainEditor.world == null)
            terrainEditor.world = terrainEditor.GetComponent<World>();

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); ;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainEditor.groundMask))
        {
            Vector3 pos;

            if (Event.current.control)
            {
                terrainEditor.placeProxy.SetActive(false);
                terrainEditor.removeProxy.SetActive(false);
                terrainEditor.modifiabilityProxy.SetActive(true);

                pos = terrainEditor.world.GetBlockPos(hit);
                terrainEditor.modifiabilityProxy.transform.position = pos;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt)
                {
                    if (Event.current.shift)
                        terrainEditor.ModifyModifiabilityEditor(hit, true);
                    else
                        terrainEditor.ModifyModifiabilityEditor(hit, false);
                }
            }
            else if (Event.current.shift)
            {
                terrainEditor.placeProxy.SetActive(false);
                terrainEditor.removeProxy.SetActive(true);
                terrainEditor.modifiabilityProxy.SetActive(false);

                pos = terrainEditor.world.GetBlockPos(hit);
                terrainEditor.removeProxy.transform.position = pos;


                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt && terrainEditor.world.GetBlock(hit) != BlockType.Bedrock)
                        terrainEditor.ModifyTerrain(hit);
            }
            else
            {
                terrainEditor.placeProxy.SetActive(true);
                terrainEditor.removeProxy.SetActive(false);
                terrainEditor.modifiabilityProxy.SetActive(false);

                pos = terrainEditor.world.GetBlockPos(hit, true);
                terrainEditor.placeProxy.transform.position = pos;


                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt && terrainEditor.world.GetBlock(hit, true) != BlockType.Bedrock)
                    terrainEditor.ModifyTerrain(hit, terrainEditor.blockType, true);
            }
        }
        else
        {
            terrainEditor.placeProxy.SetActive(false);
            terrainEditor.removeProxy.SetActive(false);
            terrainEditor.modifiabilityProxy.SetActive(false);
        }
    }
}
