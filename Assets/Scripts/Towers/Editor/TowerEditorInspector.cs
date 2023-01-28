using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TowerEditor))]
public class TowerEditorInspector : OdinEditor
{
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    private int index = -1;

    private bool editing = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TowerEditor towerEditor = (TowerEditor)target;
        
        GUILayout.Space(25);

        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        GUIStyle style = new GUIStyle();
        style.wordWrap = true;
        style.normal.textColor = Color.cyan;
        GUILayout.Label("\"left-click\" to place tower \"shift-left-click\" to remove tower. Gizmos must be turned on to work", style, GUILayout.ExpandWidth(true));

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
                    towerEditor.selectedTower.SetActive(false);
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

        TowerEditor towerEditor = (TowerEditor)target;

        if (towerEditor.world == null)
            towerEditor.world = towerEditor.GetComponent<World>();
        
        if (towerEditor.selectedTower == null)
            throw new Exception("No tower is selected in the Tower Editor");

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); ;
        RaycastHit hit;

        if (Event.current.shift)
        {
            towerEditor.selectedTower.SetActive(false);

            if (!towerEditor.proxiesActive) // acitvate proxies
            {
                towerEditor.proxiesActive = true;
                List<TowerData> tdList = towerEditor.permanentTowerParent.GetComponentsInChildren<TowerData>().Concat(towerEditor.towerParent.GetComponentsInChildren<TowerData>()).ToList();
                tdList.ForEach(m_td => { m_td.main.SetActive(false); m_td.proxy.SetActive(true); });
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, towerEditor.towerMask))
            {
                //tower removal
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt)
                {
                    TowerData n_td = hit.transform.GetComponentInParent<TowerData>(true);

                    towerEditor.RemoveTower(n_td);
                }
            }
        }
        else
        {
            if (towerEditor.proxiesActive) // deacitvate proxies
            {
                towerEditor.proxiesActive = false;
                List<TowerData> tdList = towerEditor.permanentTowerParent.GetComponentsInChildren<TowerData>().Concat(towerEditor.towerParent.GetComponentsInChildren<TowerData>()).ToList();
                tdList.ForEach(m_td => { m_td.main.SetActive(true); m_td.proxy.SetActive(false); });
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, towerEditor.groundMask))
            {
                towerEditor.selectedTower.SetActive(true);

                towerEditor.GetTowerVolumeCorners(towerEditor.td, hit, VolumeType.Main, towerEditor.td.useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2);
                towerEditor.GetTowerVolumeCorners(towerEditor.td, hit, VolumeType.Main, false, out Vector3 m_basePosition, out Vector3 m_center, out Vector3Int m_corner1, out Vector3Int m_corner2);
                towerEditor.GetTowerVolumeCorners(towerEditor.td, hit, VolumeType.Ground, towerEditor.td.useChecker, out Vector3 g_basePosition, out Vector3 g_center, out Vector3Int g_corner1, out Vector3Int g_corner2);

                towerEditor.selectedTower.transform.position = m_basePosition;
                

                if (towerEditor.world.GetBlockVolume(g_corner1, g_corner2, false, false) && towerEditor.world.GetBlockVolume(corner1, corner2, true, false)) //check ground then if empty
                {
                    if (!towerEditor.materialActive)
                    {
                        towerEditor.renderers.ForEach(r => r.sharedMaterials = r.sharedMaterials.Select(m => m = towerEditor.placeMaterial).ToArray() ); //show place proxy material
                        towerEditor.materialActive = true;
                    }

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt) //tower placed
                    {
                        if (towerEditor.td.useChecker)
                            towerEditor.world.SetBlockVolume(corner1, corner2, BlockType.Soft_Barrier); // Spawn soft barriers

                        //instantiate tower
                        GameObject newTower = Instantiate(towerEditor.selectedTower, m_basePosition, towerEditor.selectedTower.transform.rotation, towerEditor.td.editable ? towerEditor.towerParent : towerEditor.permanentTowerParent);
                        TowerData n_td = newTower.GetComponent<TowerData>();

                        if (n_td.placeBarriers)
                            towerEditor.world.SetBlockVolume(m_corner1, m_corner2, BlockType.Barrier); // Spawn barriers

                        n_td.proxy.GetComponentsInChildren<Renderer>(true).ForEach(r => r.sharedMaterials = r.sharedMaterials.Select(m => m = towerEditor.removeMaterial).ToArray()); // Set material to remove mat
                        n_td.main.SetActive(true);
                        n_td.proxy.SetActive(false);
                    }
                }
                else //if space is invalid show red proxy material
                {
                    if (towerEditor.materialActive)
                    {
                        towerEditor.renderers.ForEach(r => r.sharedMaterials = r.sharedMaterials.Select(m => m = towerEditor.removeMaterial).ToArray()); //show remove proxy material
                        towerEditor.materialActive = false;
                    }
                }
            }
            else
                towerEditor.selectedTower.SetActive(false);
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R) // If rotate is pressed
        {
            TowerData td = towerEditor.td;
            td.rotation = td.rotation > 359 ? td.rotation - 270 : td.rotation + 90;
            towerEditor.selectedTower.transform.localEulerAngles = new Vector3(0, td.rotation, 0);
        }
    }
}
