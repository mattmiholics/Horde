using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using SFB;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

[ExecuteAlways]
public class World : MonoBehaviour
{
    public static event Action<HashSet<ChunkRenderer>> ChunkUpdated;
    public static event Action WorldCreated;
    public static event Action<string, bool> WorldSaved;
    public static event Action<string, bool> WorldLoaded;

    public int mapSizeInChunks = 6;
    public int chunkSize = 16, chunkHeight = 100;
    public int border = 12;
    [Space]
    public BlockDataManager blockDataManager;
    [Space]
    public GameObject chunkPrefab;
    public WorldRenderer worldRenderer;
    [Space]
    public TerrainGenerator terrainGenerator;
    public Vector2Int mapSeedOffset;

    public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary;

    CancellationTokenSource taskTokenSource = new CancellationTokenSource();
    [HideInInspector]
    public int progress;

    public UnityEvent OnWorldCreated, OnNewChunksGenerated;

    [PropertySpace(15, 15)]
    [Button(Name = "Generate World", Expanded = true)]
    private void GenerateWorldButton()
    {
        GenerateWorld();
    }

    [ButtonGroup("SaveButtons")]
    private void Save()
    {
        SaveWorld();
    }

    [ButtonGroup("SaveButtons")]
    private void SaveAs()
    {
        SaveWorld(true);
    }

    [Button(Name = "Load World")]
    private void LoadWorldButton()
    {
        LoadWorld(true);
    }

    [NonSerialized, OdinSerialize]
    public WorldData worldData;

    public bool IsWorldCreated { get; private set; }

    private Coroutine editorUpdate;

    private static World _instance;
    public static World Instance { get { return _instance; } }

    private void OnEnable()
    {
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged += SaveTemp;
        if (!EditorApplication.isPlayingOrWillChangePlaymode) //this will load the editor for non-play mode
            LoadWorld();
        #endif
    }
    private void OnDisable()
    {
        taskTokenSource.Cancel();
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= SaveTemp;
        #endif
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            LoadWorld(); //called during build play
        }

        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }
    }

    #if UNITY_EDITOR
    private void SaveTemp(PlayModeStateChange change)
    {
        switch (change)
        {
            case PlayModeStateChange.ExitingEditMode:
                SaveWorld(false, true);
                break;
            case PlayModeStateChange.EnteredPlayMode:
                //LoadWorld(false, true); //this loads the editor version of play mode (only called if this scene is first)
                break;
        }
    }
    #endif

    public async void GenerateWorld(bool loadOnly = false)
    {
        #if UNITY_EDITOR
        blockDataManager.InitializeBlockDictionary();
        editorUpdate = StartCoroutine(EditorUpdate());
        #endif


        if (!loadOnly)
        {
            worldRenderer.chunkPool.Clear();
            worldRenderer.DeleteRenderers();
            chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>();

            worldData = new WorldData
            {
                chunkHeight = this.chunkHeight,
                chunkSize = this.chunkSize,
                mapSizeInChunks = this.mapSizeInChunks,
                border = this.border,
                mapSeedOffset = this.mapSeedOffset,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
            };
        }

        await GenerateWorld(Vector3Int.zero, loadOnly);
    }

    private async Task GenerateWorld(Vector3Int position, bool loadOnly = false)
    {
        progress = 0;

        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsFromCenter(), taskTokenSource.Token);

        if (!loadOnly)
        {
            terrainGenerator.GenerateBiomePoints(position, mapSizeInChunks, chunkSize, mapSeedOffset);

            foreach (Vector3Int pos in worldGenerationData.chunkPositionsToRemove)
            {
                WorldDataHelper.RemoveChunk(this, pos);
            }

            foreach (Vector3Int pos in worldGenerationData.chunkDataToRemove)
            {
                WorldDataHelper.RemoveChunkData(this, pos);
            }

            ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;

            try
            {
                dataDictionary = await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
            }
            catch (Exception)
            {
                Debug.Log("Task canceled");
                return;
            }

            foreach (var calculatedData in dataDictionary)
            {
                worldData.chunkDataDictionary.Add(calculatedData.Key, calculatedData.Value);
            }
        }

        ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

        List<ChunkData> dataToRender = worldData.chunkDataDictionary
            .Where(keyvaluepair => (loadOnly ? worldGenerationData.chunkPositionsToUpdate : worldGenerationData.chunkPositionsToCreate).Contains(keyvaluepair.Key))
            .Select(keyvalpair => keyvalpair.Value)
            .ToList();

        try
        {
            meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        }
        catch (Exception)
        {
            Debug.LogError("Task canceled");
            Time.timeScale = 1;
            #if UNITY_EDITOR
            if (editorUpdate != null)
                StopCoroutine(editorUpdate);
            #endif
            return;
        }

        StartCoroutine(ChunkCreationCoroutine(meshDataDictionary, loadOnly));
    }

    private Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
    {
        ConcurrentDictionary<Vector3Int, MeshData> dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();
        return Task.Run(() =>
        {
            for (int i = 0; i < dataToRender.Count; i++)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                MeshData meshData = Chunk.GetChunkMeshData(this, dataToRender[i]);
                dictionary.TryAdd(dataToRender[i].worldPosition, meshData);

                progress = Mathf.RoundToInt(((float)i / (dataToRender.Count - 1)) * 45);
            }

            return dictionary;
        }, taskTokenSource.Token
        );
    }

    private Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
    {
        ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

        return Task.Run(() =>
        {
            for (int i = 0; i < chunkDataPositionsToCreate.Count; i++)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                ChunkData data = new ChunkData(chunkSize, chunkHeight, chunkDataPositionsToCreate[i]);
                ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset, worldData);
                dictionary.TryAdd(chunkDataPositionsToCreate[i], newData);

                progress = 45 + Mathf.RoundToInt(((float)i / (chunkDataPositionsToCreate.Count - 1)) * 45);
            }
            return dictionary;
        },
        taskTokenSource.Token
        );
    }

    #if UNITY_EDITOR
    IEnumerator EditorUpdate()
    {
        Time.timeScale = 0;
        while (!Application.isPlaying)
        {
            EditorApplication.update += EditorApplication.QueuePlayerLoopUpdate;
            yield return null;
        }
        yield return null;
    }
    #endif

    IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary, bool loadOnly)
    {
        for (int i = 0; i < meshDataDictionary.Count; i++)
        {
            CreateChunk(worldData, meshDataDictionary.Keys.ElementAtOrDefault(i), meshDataDictionary.Values.ElementAtOrDefault(i), loadOnly);
            yield return 0;
            progress = 90 + Mathf.RoundToInt(((float)i / (meshDataDictionary.Count - 1)) * 10);
        }
        if (IsWorldCreated == false)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
            WorldCreated?.Invoke();
        }
        Time.timeScale = 1;
        #if UNITY_EDITOR
        if (editorUpdate != null)
            StopCoroutine(editorUpdate);
        #endif
    }

    private void CreateChunk(WorldData worldData, Vector3Int position, MeshData meshData, bool loadOnly)
    {
        ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, position, meshData);
        if (!loadOnly)
            chunkDictionary.Add(position, chunkRenderer);

    }

    public bool IsBlockModifiable(Vector3Int blockWorldPos)
    {
        Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, blockWorldPos.x, blockWorldPos.y, blockWorldPos.z);
        ChunkData containerChunk = null;

        worldData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

        if (containerChunk != null)
        {
            Vector3Int blockPos = Chunk.GetBlockInChunkCoordinates(containerChunk, new Vector3Int(blockWorldPos.x, blockWorldPos.y, blockWorldPos.z));

            if (containerChunk.unmodifiableColumns.Contains(new Vector2Int(blockPos.x, blockPos.z))) //check if column is unmodifiable
                return false;
            else
                return true;
        }
        else
            return false;
    }

    public BlockType GetBlock(RaycastHit hit, bool place = false)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return BlockType.Nothing;

        Vector3Int pos = Vector3Int.RoundToInt(GetBlockPos(hit, place));

        return WorldDataHelper.GetBlock(this, pos);
    }

    public bool GetBlockVolume(Vector3Int corner1, Vector3Int corner2, bool checkEmpty, bool checkModifiability = true) //if it is checking empty: true if empty, false if any blocks || if checking filled: true if filled, false if any empty
    {
        Vector3Int size = corner2 - corner1;

        for (int x = 0; (size.x > 0 ? x <= size.x : x >= size.x); x += (size.x > 0 ? 1 : -1))
        {
            for (int y = 0; (size.y > 0 ? y <= size.y : y >= size.y); y += (size.y > 0 ? 1 : -1))
            {
                for (int z = 0; (size.z > 0 ? z <= size.z : z >= size.z); z += (size.z > 0 ? 1 : -1))
                {
                    if (checkModifiability && !IsBlockModifiable(new Vector3Int(corner1.x + x, corner1.y + y, corner1.z + z))) //check if column is unmodifiable
                        return false;

                    BlockType block = GetBlockFromChunkCoordinates(null, corner1.x + x, corner1.y + y, corner1.z + z);
                    if (checkEmpty ? //whether to be checking for emptiness or filled
                        block != BlockType.Nothing && block != BlockType.Air : //if the block is anything but empty
                        block == BlockType.Nothing || block == BlockType.Air || block == BlockType.Barrier)  //if the block is empty
                        return false;
                }
            }
        }
        return true;
    }

    public bool SetBlock(RaycastHit hit, BlockType blockType, bool place = false)
    {
        HashSet<ChunkRenderer> updateChunks = new HashSet<ChunkRenderer>();

        Vector3Int pos = Vector3Int.RoundToInt(GetBlockPos(hit, place));

        Vector3Int chunkPos = Chunk.ChunkPositionFromBlockCoords(this, pos.x, pos.y, pos.z);

        ChunkRenderer chunk = WorldDataHelper.GetChunk(this, chunkPos);
        if (chunk == null)
            return false;

        WorldDataHelper.SetBlock(this, pos, blockType);
        chunk.ModifiedByThePlayer = true;
        updateChunks.Add(chunk);

        if (Chunk.IsOnEdge(chunk.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(this, chunk.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {
                //neighbourData.modifiedByThePlayer = true;
                if (neighbourData != null)
                {
                    ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(this, neighbourData.worldPosition);
                    if (chunkToUpdate != null)
                        updateChunks.Add(chunkToUpdate);
                }
            }

        }

        foreach (ChunkRenderer cr in updateChunks)
            cr.UpdateChunk();
        ChunkUpdated?.Invoke(updateChunks);
        return true;
    }

    public bool SetBlockVolume(Vector3Int corner1, Vector3Int corner2, BlockType blockType)
    {
        Vector3Int size = corner2 - corner1;

        HashSet<ChunkRenderer> updateChunks = new HashSet<ChunkRenderer>();

        for (int x = 0; (size.x > 0 ? x <= size.x : x >= size.x); x += (size.x > 0 ? 1 : -1))
        {
            for (int y = 0; (size.y > 0 ? y <= size.y : y >= size.y); y += (size.y > 0 ? 1 : -1))
            {
                for (int z = 0; (size.z > 0 ? z <= size.z : z >= size.z); z += (size.z > 0 ? 1 : -1))
                {
                    Vector3Int pos = Vector3Int.RoundToInt(new Vector3(corner1.x + x, corner1.y + y, corner1.z + z));
                    Vector3Int chunkPos = Chunk.ChunkPositionFromBlockCoords(this, corner1.x + x, corner1.y + y, corner1.z + z);

                    ChunkRenderer chunk = WorldDataHelper.GetChunk(this, chunkPos);
                    if (chunk == null)
                        return false;

                    updateChunks.Add(chunk);

                    WorldDataHelper.SetBlock(this, pos, blockType);
                    chunk.ModifiedByThePlayer = true;

                    if (Chunk.IsOnEdge(chunk.ChunkData, pos))
                    {
                        List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(this, chunk.ChunkData, pos);
                        foreach (ChunkData neighbourData in neighbourDataList)
                        {
                            //neighbourData.modifiedByThePlayer = true;
                            if (neighbourData != null)
                            {
                                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(this, neighbourData.worldPosition);
                                if (chunkToUpdate != null)
                                    updateChunks.Add(chunkToUpdate);
                            }
                        }

                    }
                }
            }
        }
        foreach (ChunkRenderer cr in updateChunks)
            cr.UpdateChunk();
        ChunkUpdated?.Invoke(updateChunks);
        return true;
    }

    public void SetModifiability(RaycastHit hit, bool unlock)
    {
        Vector3Int blockWorldPos = Vector3Int.RoundToInt(GetBlockPos(hit));

        Vector3Int chunkPos = Chunk.ChunkPositionFromBlockCoords(this, blockWorldPos.x, blockWorldPos.y, blockWorldPos.z);

        ChunkRenderer chunk = WorldDataHelper.GetChunk(this, chunkPos);

        Vector3Int blockPos = Chunk.GetBlockInChunkCoordinates(chunk.ChunkData, new Vector3Int(blockWorldPos.x, blockWorldPos.y, blockWorldPos.z));

        if (chunk != null)
        {
            Vector2Int columnPos = new Vector2Int(blockPos.x, blockPos.z);
            if (unlock && chunk.ChunkData.unmodifiableColumns.Contains(columnPos))
            {
                chunk.ChunkData.unmodifiableColumns.Remove(columnPos);
                chunk.UpdateChunk();
                ChunkUpdated?.Invoke(new HashSet<ChunkRenderer> { chunk });
            }
            else if (!unlock)
            {
                chunk.ChunkData.unmodifiableColumns.Add(columnPos);
                chunk.UpdateChunk();
                ChunkUpdated?.Invoke(new HashSet<ChunkRenderer> { chunk });
            }
        }
    }

    public Vector3 GetBlockPos(RaycastHit hit, bool place = false, int[] baseWidthLength = null )
    {
        Vector3 pos = new Vector3(
             GetBlockPositionIn(hit.point.x, hit.normal.x, place),
             GetBlockPositionIn(hit.point.y, hit.normal.y, place),
             GetBlockPositionIn(hit.point.z, hit.normal.z, place)
             );

        if (baseWidthLength != null)
        {
            float x, y, z;
            if (baseWidthLength[0] % 2 != 1)
            {
                x = pos.x + 0.5f;
                x = Mathf.Round(x) - 0.5f;
            }
            else
                x = Mathf.Round(pos.x);

            if (baseWidthLength[1] % 2 != 1)
            {
                z = pos.z + 0.5f;
                z = Mathf.Round(z) - 0.5f;
            }
            else
                z = Mathf.Round(pos.z);

            y = pos.y;
            y = Mathf.Round(y);

            pos = new Vector3(x, y, z);

            return pos;
        }
        return Vector3Int.RoundToInt(pos);
    }

    private float GetBlockPositionIn(float pos, float normal, bool place)
    {
        float halfway = Mathf.Abs(pos % 1);
        if (0.49f < halfway && halfway < 0.51f)
        {
            pos += place ? (normal / 2) : -(normal / 2);
        }

        return (float)pos;
    }


    private WorldGenerationData GetPositionsFromCenter()
    {
        List<Vector3Int> allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundOrigin(this);

        List<Vector3Int> allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundOrigin(this);

        Vector3Int center = new Vector3Int(chunkSize * mapSizeInChunks, 0, chunkSize * mapSizeInChunks);
        List<Vector3Int> chunkPositionsToCreate = WorldDataHelper.SelectPositonsToCreate(this, worldData, allChunkPositionsNeeded, center);
        List<Vector3Int> chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositonsToCreate(worldData, allChunkDataPositionsNeeded, center);

        List<Vector3Int> chunkPositionsToRemove = WorldDataHelper.GetUnnededChunks(this, worldData, allChunkPositionsNeeded);
        List<Vector3Int> chunkDataToRemove = WorldDataHelper.GetUnnededData(worldData, allChunkDataPositionsNeeded);

        WorldGenerationData data = new WorldGenerationData
        {
            chunkPositionsToCreate = chunkPositionsToCreate,
            chunkDataPositionsToCreate = chunkDataPositionsToCreate,
            chunkPositionsToRemove = chunkPositionsToRemove,
            chunkDataToRemove = chunkDataToRemove,
            chunkPositionsToUpdate = allChunkPositionsNeeded
        };
        return data;

    }

    internal BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
    {
        Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
        ChunkData containerChunk = null;

        worldData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

        if (containerChunk == null)
            return BlockType.Nothing;
        Vector3Int blockInCHunkCoordinates = Chunk.GetBlockInChunkCoordinates(containerChunk, new Vector3Int(x, y, z));
        return Chunk.GetBlockFromChunkCoordinates(this, containerChunk, blockInCHunkCoordinates);
    }

    public struct WorldGenerationData
    {
        public List<Vector3Int> chunkPositionsToCreate;
        public List<Vector3Int> chunkDataPositionsToCreate;
        public List<Vector3Int> chunkPositionsToRemove;
        public List<Vector3Int> chunkDataToRemove;
        public List<Vector3Int> chunkPositionsToUpdate;
    }

    public Vector3Int GetSurfaceHeightPosition(Vector3 nearestVector, bool searchBelow = false, bool searchAbove = false)
    {
        Vector3Int origionalBlockPos = Vector3Int.RoundToInt(nearestVector);
        Vector3Int blockPos = Vector3Int.RoundToInt(nearestVector);

        for (int i = (searchBelow ? blockPos.y : chunkHeight - 1); i > 0; i--)
        {
            BlockType block = GetBlockFromChunkCoordinates(null, blockPos.x, i, blockPos.z);
            if (block != BlockType.Nothing && block != BlockType.Air && block != BlockType.Soft_Barrier)
            {
                blockPos.y = i + 1;
                break;
            }
        }

        if (searchAbove)
        {
            Vector3Int tempPos = origionalBlockPos;

            for (int i = tempPos.y; i < chunkHeight; i++)
            {
                BlockType block = GetBlockFromChunkCoordinates(null, tempPos.x, i, tempPos.z);
                if (block != BlockType.Nothing && block != BlockType.Air && block != BlockType.Soft_Barrier)
                {
                    tempPos.y = i + 1;
                    break;
                }
            }

            blockPos = Vector3.Distance(blockPos, origionalBlockPos) > Vector3.Distance(tempPos, origionalBlockPos) ? tempPos : blockPos;
        }

        return blockPos;
    }

    private static string worldAssetPath = "/Resources/Worlds/";
    private static string fileType = "txt";

    private static string defaultAssetFolder = "temp";

    [InfoBox("This is the current file path of the world data file")]
    [OnInspectorInit("@inspectorPreviewAssetPath = UnityEngine.Application.dataPath + customAssetPath")]
    [ShowInInspector]
    [HideLabel]
    [DisplayAsString(false)]
    [PropertySpace(10)]
    private string inspectorPreviewAssetPath;

    [SerializeField, HideInInspector]
    private string customAssetPath; // This will get overwritten after saving

    [SerializeField, HideInInspector]
    private string runtimeAssetPath;

    // SAVE and LOAD methods
    public void SaveWorld(bool saveAs = false, bool saveTemp = false, bool saveToResources = true)
    {
        string assetName = default;
        string assetDir = default;

        if (saveToResources)
        {
            if (saveAs)
            {
                customAssetPath = StandaloneFileBrowser.SaveFilePanel("Create World Data", Application.dataPath + worldAssetPath, "", fileType).Replace("\\", "/");

                if (customAssetPath.StartsWith(Application.dataPath))
                    customAssetPath = customAssetPath.Substring(Application.dataPath.Length);
                else
                    throw new Exception("Please keep world data contained within the Unity project data path");

                inspectorPreviewAssetPath = Application.dataPath + customAssetPath;
            }

            if (!customAssetPath.Contains("Resources"))
                throw new Exception("Please keep world data contained within the Unity project resources file structure");

            assetName = saveTemp ? defaultAssetFolder : Path.GetFileNameWithoutExtension(customAssetPath);
            assetDir = saveTemp ? Application.dataPath + worldAssetPath + defaultAssetFolder : Application.dataPath + Path.GetDirectoryName(customAssetPath) + (File.Exists(Application.dataPath + customAssetPath) ? "" : "/" + assetName);
        }
        else
        {
            // Logic for saving during runtime
        }

        assetDir = assetDir.Replace("\\", "/");
        if (!Directory.Exists(assetDir))
            Directory.CreateDirectory(assetDir);

        assetDir += "/";

        Debug.Log("Saved to: " + assetDir + assetName + "." + fileType);

        byte[] bytes = Sirenix.Serialization.SerializationUtility.SerializeValue(worldData, DataFormat.Binary);
        File.WriteAllBytes(assetDir + assetName + "." + fileType, bytes);

        WorldSaved?.Invoke(assetDir, saveToResources);

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }

    public void LoadWorld(bool loadAs = false, bool loadTemp = false, bool loadFromResources = true)
    {
        string assetName = default;
        string assetDir = default;

        if (loadFromResources)
        {
            Resources.UnloadUnusedAssets();

            if (loadAs)
            {
                customAssetPath = StandaloneFileBrowser.OpenFilePanel("Get World Data", Application.dataPath + worldAssetPath, fileType, false).FirstOrDefault().Replace("\\", "/");

                if (customAssetPath.StartsWith(Application.dataPath))
                    customAssetPath = customAssetPath.Substring(Application.dataPath.Length);
                else
                    throw new Exception("Please keep world data contained within the Unity project data path");

                inspectorPreviewAssetPath = Application.dataPath + customAssetPath;
            }

            if (!customAssetPath.Contains("Resources"))
                throw new Exception("Please keep world data contained within the Unity project resources file structure");

            assetName = loadTemp ? defaultAssetFolder : Path.GetFileNameWithoutExtension(customAssetPath);
            assetDir = loadTemp ? Application.dataPath + worldAssetPath + defaultAssetFolder : Application.dataPath + Path.GetDirectoryName(customAssetPath);
        }
        else
        {
            // Logic for loading during runtime

            /*if (!File.Exists(Application.dataPath + customAssetPath))
                throw new Exception("Missing world data files! No world was loaded!");*/
        }

        assetDir = assetDir.Replace("\\", "/");

        assetDir = assetDir + "/";

        Debug.Log("Load from: " + assetDir + assetName + "." + fileType);

        byte[] bytes = Resources.Load<TextAsset>(assetDir.Replace(Application.dataPath + "/Resources/", "") + assetName).bytes;
        worldData = Sirenix.Serialization.SerializationUtility.DeserializeValue<WorldData>(bytes, DataFormat.Binary);

        worldRenderer.chunkPool.Clear();
        worldRenderer.DeleteRenderers();
        worldRenderer.LoadRenderersFromChunkData(this, ref chunkDictionary, ref worldData.chunkDataDictionary);


        // Set barrier blocks to air blocks so that tower/decoration placement can do that manually, just in case an id is switched intentionally
        /*worldData.chunkDataDictionary.Values.ToList()
                                            .ForEach(cd => Chunk
                                            .LoopThroughTheBlocks(cd, (x, y, z) =>
                                            {
                                                BlockType block = Chunk.GetBlockFromChunkCoordinates(this, cd, x, y, z);
                                                if (block == BlockType.Barrier || block == BlockType.Soft_Barrier)
                                                    Chunk.SetBlock(this, cd, new Vector3Int(x, y, z), BlockType.Air);
                                            }));*/

        chunkSize = worldData.chunkSize;
        chunkHeight = worldData.chunkHeight;
        mapSizeInChunks = worldData.mapSizeInChunks;
        mapSeedOffset = worldData.mapSeedOffset;

        WorldLoaded?.Invoke(assetDir, loadFromResources);

        GenerateWorld(true);
    }
}

[Serializable]
public struct WorldData
{
    public Dictionary<Vector3Int, ChunkData> chunkDataDictionary;
    public int chunkSize;
    public int chunkHeight;
    public int mapSizeInChunks;
    public int border;
    public Vector2Int mapSeedOffset;
}
