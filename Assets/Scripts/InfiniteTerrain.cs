using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public const float maxViewDist = 300f;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunksVisibles;

    Dictionary<Vector2, ChunkData> chunksDictionary = new Dictionary<Vector2, ChunkData>();

    // Start is called before the first frame update
    void Start()
    {
        chunksVisibles = Mathf.RoundToInt(maxViewDist / VoxelManager.chunkWidth);
    }

    // Update is called once per frame
    void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / VoxelManager.chunkWidth);
        int currentChunkCoordZ = Mathf.RoundToInt(viewerPosition.y / VoxelManager.chunkWidth);

        for (int xRango = -chunksVisibles; xRango <= chunksVisibles; xRango++)
        {
            for (int zRango = -chunksVisibles; zRango <= chunksVisibles; zRango++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xRango, currentChunkCoordZ + zRango);

                if (chunksDictionary.ContainsKey(viewedChunkCoord))
                {
                    //
                }
                else
                {
                    //chunksDictionary.Add(viewedChunkCoord, new ChunkData());
                }
            }
        }
    }
}
