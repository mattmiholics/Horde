using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TowerHelper
{
    public static void GetTowerVolumeCorners(World world, TowerData towerData, Vector3 position, VolumeType volumeType, bool useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2)
    {
        RaycastHit hit = new RaycastHit();
        hit.point = position;
        GetTowerVolumeCorners(world, towerData, hit, volumeType, useChecker, out basePosition, out center, out corner1, out corner2);
    }

    public static void GetTowerVolumeCorners(World world, TowerData towerData, RaycastHit hit, VolumeType volumeType, bool useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2)
    {
        int rotation = towerData.rotation;

        // Get any offsets
        OffsetRotation(towerData, rotation, out Vector3 offset);

        // Get correct size
        Vector3Int size = useChecker ? towerData.checkerSize : towerData.size;
        int width = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? size.z : size.x;
        int length = (rotation > 45 && rotation < 135) || (rotation > 225 && rotation < 315) ? size.x : size.z;
        size = new Vector3Int(width, size.y, length);

        // Check for position
        hit.point += useChecker ? offset + (Vector3.up / 2) : (Vector3.up / 2); // Added a bit of up to fix rounding errors
        basePosition = world.GetBlockPos(hit, true, new int[2] { size.x, size.z }) + new Vector3(0, -0.5f, 0);

        switch (volumeType)
        {
            case VolumeType.Ground:
                // Check valid ground
                center = basePosition + new Vector3(0, -0.5f, 0);
                corner1 = Vector3Int.RoundToInt(new Vector3(center.x - ((size.x / 2f) - 0.5f), center.y, center.z - ((size.z / 2f) - 0.5f)));
                corner2 = Vector3Int.RoundToInt(new Vector3(center.x + ((size.x / 2f) - 0.5f), center.y, center.z + ((size.z / 2f) - 0.5f)));
                break;

            default:
                // Check valid empty space
                center = basePosition + new Vector3(0, size.y / 2f, 0);
                corner1 = Vector3Int.RoundToInt(center - ((Vector3)size / 2f - Vector3.one / 2f));
                corner2 = Vector3Int.RoundToInt(center + ((Vector3)size / 2f - Vector3.one / 2f));
                break;
        }
    }

    public static void OffsetRotation(TowerData towerData, float rotation, out Vector3 offset)
    {
        offset = !towerData.useChecker ? Vector3.zero :
                 (rotation >= 225 && rotation < 315) ? new Vector3(-towerData.checkerOffset.z, towerData.checkerOffset.y, -towerData.checkerOffset.x) :
                 (rotation >= 135 && rotation < 225) ? new Vector3(-towerData.checkerOffset.x, towerData.checkerOffset.y, -towerData.checkerOffset.z) :
                 (rotation >= 45 && rotation < 135) ? new Vector3(towerData.checkerOffset.z, towerData.checkerOffset.y, towerData.checkerOffset.x) :
                 towerData.checkerOffset;
    }

    public static void RemoveTower(TowerEditor towerEditor, TowerData td)
    {
        if (Application.isPlaying)
        {
            try { towerEditor.tdList.Remove(td); }
            catch { }
        }

        GetTowerVolumeCorners(towerEditor.world, td, td.transform.position, VolumeType.Main, td.useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2);

        towerEditor.world.SetBlockVolume(corner1, corner2, BlockType.Air);

        towerEditor.SmartDestroy(td.gameObject);
    }

    public static void PlaceTower(TowerEditor towerEditor, GameObject gameObject, Vector3 position, float rotation)
    {
        TowerData td = gameObject.GetComponent<TowerData>();

        GetTowerVolumeCorners(towerEditor.world, td, position, VolumeType.Main, towerEditor.td.useChecker, out Vector3 basePosition, out Vector3 center, out Vector3Int corner1, out Vector3Int corner2); // Full volume check with optional checker
        GetTowerVolumeCorners(towerEditor.world, td, position, VolumeType.Main, false, out Vector3 m_basePosition, out Vector3 m_center, out Vector3Int m_corner1, out Vector3Int m_corner2); // Full volume check without checker

        PlaceTower(towerEditor, gameObject, position, rotation, corner1, corner2, m_corner1, m_corner2);
    }

    public static void PlaceTower(TowerEditor towerEditor, GameObject gameObject, Vector3 position, float rotation, Vector3Int corner1, Vector3Int corner2, Vector3Int m_corner1, Vector3Int m_corner2)
    {
        //instantiate tower
        GameObject newTower = towerEditor.SmartInstantiate(towerEditor.selectedTowerPrefab, position, Quaternion.Euler(0, rotation, 0), gameObject.GetComponent<TowerData>().editable ? towerEditor.towerParent : towerEditor.permanentTowerParent);
        TowerData td = newTower.GetComponent<TowerData>();

        if (Application.isPlaying && td.editable)
            towerEditor.tdList.Add(td);

        if (td.useChecker)
            towerEditor.world.SetBlockVolume(corner1, corner2, BlockType.Soft_Barrier); // Spawn soft barriers

        if (td.placeBarriers)
            towerEditor.world.SetBlockVolume(m_corner1, m_corner2, BlockType.Barrier); // Spawn barriers

        td.Proxy.GetComponentsInChildren<Renderer>(true).ForEach(r => r.sharedMaterials = r.sharedMaterials.Select(m => m = towerEditor.removeMaterial).ToArray()); // Set material to remove mat
        td.Main.SetActive(true);
        td.Proxy.SetActive(false);
    }
}
