using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class ChunkVFX : MonoBehaviour
{
    public ChunkRenderer chunkRenderer;
    public VisualEffect vfx;


    private void Awake()
    {
        chunkRenderer.ChunkUpdated += ApplyMesh;
    }
    private void OnDestroy()
    {
        chunkRenderer.ChunkUpdated -= ApplyMesh;
    }

    [Button]
    private void ApplyMesh()
    {

        vfx.SetMesh("Mesh", chunkRenderer.meshFilter.sharedMesh);
        vfx.SetVector3("BoundsSize", chunkRenderer.meshFilter.sharedMesh.bounds.size);
        vfx.SetVector3("BoundsCenter", chunkRenderer.meshFilter.sharedMesh.bounds.center);
        vfx.Play();
    }
}
