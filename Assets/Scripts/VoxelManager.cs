using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelManager : MonoBehaviour
{

    //private List<ChunkData> chunksData;
    public GameObject chunk;
    private List<GameObject> chunks = new List<GameObject>();
    private List<Vector3> vertices;
    private List<int> triangles;
    public Camera player;
    public const int chunkHeight = 128;
    public const int chunkWidth = 32;
    public const float voxelSize = 1f;

    //Chunk management
    public const float maxViewDist = 500f;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunksVisibles;

    Dictionary<Vector2, GameObject> chunksDictionary = new Dictionary<Vector2, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //chunks = new List<GameObject>();
        chunksVisibles = Mathf.RoundToInt(maxViewDist / chunkWidth);
        viewer = player.transform;
        //chunksData = new List<ChunkData>();
        //chunks = new List<GameObject>();
        /*for (int i = 0; i < 5; i++)
        {
            for (int u = 0; u < 5; u++)
            {
                for (int v = 0; v < 1; v++)
                {
                    CreateChunk(i * chunkWidth, v * chunkHeight, u * chunkWidth);
                }
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CreateChunk(transform.position.x - 32, transform.position.y, transform.position.z - 32);
        }*/
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        //Debug.Log(viewerPosition);
        UpdateVisibleChunks();
        //SetVisibilityOfChunksOnList(true);
            
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            Ray ray = new Ray(player.transform.position, player.transform.forward);

            //Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
            //Debug.DrawRay(transform.position, forward, Color.green);

            if (Physics.Raycast(ray, out hit, 5) && hit.transform.CompareTag("suelo"))
            {
                //hit.transform.GetComponent<MeshFilter>().mesh.triangles = hit.transform.GetComponent<MeshFilter>().mesh.triangles.Except(new int[] { hit.triangleIndex }).ToArray();
                updateChunk(0, hit);
            }
        }
    }

    void CreateChunk(float xPosition, float yPosition, float zPosition)
    {
        GameObject chunk = Instantiate(this.chunk, new Vector3(xPosition, -yPosition, zPosition), Quaternion.identity);
        
        chunk.tag = "suelo";
        bool[][][] voxels; 
        
        voxels = new bool[Mathf.RoundToInt(chunkWidth / voxelSize + 1)][][];

        for (int i = 0; i < voxels.Length; i++)
        {
            voxels[i] = new bool[Mathf.RoundToInt(chunkWidth / voxelSize + 1)][];

            for (int u = 0; u < voxels[i].Length; u++)
            {
                voxels[i][u] = new bool[Mathf.RoundToInt(chunkHeight / voxelSize)];
            }
        }

        //ChunkData chunkData = new ChunkData(voxels);

        int xVoxels = 0;
        int zVoxels;
        int yVoxels;
        for (float x = 0; x < chunkWidth + voxelSize; x += voxelSize)
        {
            zVoxels = 0;
            for (float z = 0; z < chunkWidth + voxelSize; z += voxelSize)
            {
                yVoxels = 0;
                for (float y = 0; y < chunkHeight; y += voxelSize)
                {
                    //El número que multiplica a cada una de las coordenadas de PerlinNoise hace que el cambio de altura sea más o menos brusco 
                    //(cuanto más pequeño, el cambio será más suave y se necesitarán mas chunks para respresntar la misma variación de altura que con un número más grande).
                    //El número que multiplica al resultado de la función PerlinNoise permite determinar el rango de altura (cuanto más grande más altura)

                    float noise = Mathf.PerlinNoise((xPosition + xVoxels * voxelSize) * 0.02f, (zPosition + zVoxels * voxelSize) * 0.02f) * 100;
                    if (noise <= yVoxels)
                    {
                        voxels[xVoxels][zVoxels][yVoxels] = true;
                    }
                    else
                    {
                        voxels[xVoxels][zVoxels][yVoxels] = false;
                    }

                    noise = perlinNoise.get3DPerlinNoise(new Vector3(xPosition + xVoxels * voxelSize, zPosition + zVoxels * voxelSize, yVoxels * voxelSize), 0.05f);
                    if (noise <= 0.6f && noise >= 0.4f)
                    {
                        voxels[xVoxels][zVoxels][yVoxels] = false;
                    }

                    y = (float)Math.Round(y, 2);
                    yVoxels++;
                }
                zVoxels++;
            }
            xVoxels++;
        }

        chunk.GetComponent<ChunkData>().voxels = voxels;
        chunk.SetActive(false);
        //chunksData.Add(chunkData);
        chunksDictionary.Add(new Vector2(xPosition / chunkWidth, zPosition / chunkWidth), chunk);
        //chunks.Add(chunk);
        createMesh(chunk);
        //return chunkData;
    }

    void createMesh(GameObject chunk)
    {
        ChunkData chunkData = chunk.GetComponent<ChunkData>();
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < chunkData.voxels.Length - 1; x++)
        {
            for (int z = 0; z < chunkData.voxels[x].Length - 1; z++)
            {
                for (int y = 0; y < chunkData.voxels[x][z].Length; y++)
                {
                    if (y - 1 >= 0 && y + 1 <= (chunkHeight - voxelSize) / voxelSize)
                    {
                        if (chunkData.voxels[x][z][y - 1] == false && chunkData.voxels[x][z][y] == true)
                        {
                            vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
                            vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));
                            vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
                            vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));

                            triangles.Add(vertices.Count - 4);
                            triangles.Add(vertices.Count - 3);
                            triangles.Add(vertices.Count - 2);
                            triangles.Add(vertices.Count - 1);
                            triangles.Add(vertices.Count - 3);
                            triangles.Add(vertices.Count - 4);
                        }

                        if (chunkData.voxels[x][z][y + 1] == false && chunkData.voxels[x][z][y] == true)
                        {
                            vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + z * voxelSize));
                            vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - (y + 1)* voxelSize, transform.position.z + (z + 1) * voxelSize));
                            vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + z * voxelSize));
                            vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));

                            triangles.Add(vertices.Count - 4);
                            triangles.Add(vertices.Count - 3);
                            triangles.Add(vertices.Count - 2);
                            triangles.Add(vertices.Count - 1);
                            triangles.Add(vertices.Count - 3);
                            triangles.Add(vertices.Count - 4);
                        }
                    }
                    else
                    {
                        if (y == 0)
                        {
                            if (chunkData.voxels[x][z][y] == true)
                            {
                                vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
                                vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));
                                vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
                                vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));

                                triangles.Add(vertices.Count - 4);
                                triangles.Add(vertices.Count - 3);
                                triangles.Add(vertices.Count - 2);
                                triangles.Add(vertices.Count - 1);
                                triangles.Add(vertices.Count - 3);
                                triangles.Add(vertices.Count - 4);
                            }
                        }

                        if (y == (chunkHeight - voxelSize) / voxelSize)
                        {
                            if (chunkData.voxels[x][z][y - 1] == false && chunkData.voxels[x][z][y] == true)
                            {
                                vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
                                vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));
                                vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
                                vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));

                                triangles.Add(vertices.Count - 4);
                                triangles.Add(vertices.Count - 3);
                                triangles.Add(vertices.Count - 2);
                                triangles.Add(vertices.Count - 1);
                                triangles.Add(vertices.Count - 3);
                                triangles.Add(vertices.Count - 4);
                            }
                        }
                    }

                    if (z - 1 >= 0 && z + 1 <= (chunkWidth - voxelSize) / voxelSize)
                    {
                        if (chunkData.voxels[x][z + 1][y] == false && chunkData.voxels[x][z][y] == true)
                        {
                            zFrontMesh(x, y, z);
                        }

                        if (chunkData.voxels[x][z - 1][y] == false && chunkData.voxels[x][z][y] == true)
                        {
                            zBackMesh(x, y, z);
                        }
                    }
                    else
                    {
                        if (z == 0)
                        {
                            if (chunkData.voxels[x][z + 1][y] == false && chunkData.voxels[x][z][y] == true)
                            {
                                zFrontMesh(x, y, z);
                            }
                        }

                        if (z == (chunkWidth - voxelSize) / voxelSize)
                        {
                            if (chunkData.voxels[x][z - 1][y] == false && chunkData.voxels[x][z][y] == true)
                            {
                                zBackMesh(x, y, z);
                            }

                            if (chunkData.voxels[x][z + 1][y] == false && chunkData.voxels[x][z][y] == true)
                            {
                                zFrontMesh(x, y, z);
                            }

                            if (chunkData.voxels[x][z + 1][y] == true && chunkData.voxels[x][z][y] == false)
                            {
                                zFrontMeshReversed(x, y, z);
                            }
                        }
                    }

                    if (x - 1 >= 0 && x + 1 <= (chunkWidth - voxelSize) / voxelSize)
                    {
                        if (chunkData.voxels[x + 1][z][y] == false && chunkData.voxels[x][z][y] == true)
                        {
                            xFrontMesh(x, y, z);
                        }

                        if (chunkData.voxels[x - 1][z][y] == false && chunkData.voxels[x][z][y] == true)
                        {
                            xBackMesh(x, y, z);
                        }
                    }
                    else
                    {
                        if (x == 0)
                        {
                            if (chunkData.voxels[x + 1][z][y] == false && chunkData.voxels[x][z][y] == true)
                            {
                                xFrontMesh(x, y, z);
                            }
                        }

                        if (x == (chunkWidth - voxelSize) / voxelSize)
                        {
                            if (chunkData.voxels[x - 1][z][y] == false && chunkData.voxels[x][z][y] == true)
                            {
                                xBackMesh(x, y, z);
                            }

                            if (chunkData.voxels[x + 1][z][y] == false && chunkData.voxels[x][z][y] == true)
                            {
                                xFrontMesh(x, y, z);
                            }

                            if (chunkData.voxels[x + 1][z][y] == true && chunkData.voxels[x][z][y] == false)
                            {
                                xFrontMeshReversed(x, y, z);
                            }
                        }
                    }
                }
            }
        }
        chunk.GetComponent<MeshFilter>().mesh.Clear();
        chunk.GetComponent<MeshFilter>().mesh.vertices = vertices.ToArray();
        chunk.GetComponent<MeshFilter>().mesh.triangles = triangles.ToArray();
        chunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
    }

    void xFrontMesh(int x, int y, int z)
    {
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);
    }

    void xBackMesh(int x, int y, int z)
    {
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);
    }

    void xFrontMeshReversed(int x, int y, int z)
    {
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));

        //En este caso el orden de los vertices de los triángulos es justo al revés, ya que estamos pintando la pared de en frente 
        //(no la del voxel en el que estamos)
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
    }

    void zFrontMesh(int x, int y, int z)
    {
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);
    }

    void zBackMesh(int x, int y, int z)
    {
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + z * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + z * voxelSize));

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);
    }

    void zFrontMeshReversed(int x, int y, int z)
    {
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + (x + 1) * voxelSize, transform.position.y - (y + 1) * voxelSize, transform.position.z + (z + 1) * voxelSize));
        vertices.Add(new Vector3(transform.position.x + x * voxelSize, transform.position.y - y * voxelSize, transform.position.z + (z + 1) * voxelSize));

        //En este caso el orden de los vertices de los triángulos es justo al revés, ya que estamos pintando la pared de en frente 
        //(no la del voxel en el que estamos)
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
    }

    void updateChunk(int action, RaycastHit hit)
    {
        Vector3 pointInTargetBlock;

        pointInTargetBlock = hit.point + player.transform.forward * 0.01f;

        //Destroy(hit.transform.gameObject);

        int chunkPosX = Mathf.FloorToInt(pointInTargetBlock.x / chunkWidth) * chunkWidth;
        int chunkPosZ = Mathf.FloorToInt(pointInTargetBlock.z / chunkWidth) * chunkWidth;

        //Aquí se cogería el chunk correspondiente del diccionario con Vector2(chunkPosX, chunkPosZ). De momento voy a hacerlo solo con el chunk 0
        Vector2 chunkDictionaryKey = new Vector2(chunkPosX / chunkWidth, chunkPosZ / chunkWidth);

        int blockX = Mathf.FloorToInt(pointInTargetBlock.x) - chunkPosX;
        int blockY = Mathf.FloorToInt(pointInTargetBlock.y) + 1;
        int blockZ = Mathf.FloorToInt(pointInTargetBlock.z) - chunkPosZ;

        Debug.Log(blockX);
        Debug.Log(blockY);
        Debug.Log(blockZ);

        chunksDictionary[chunkDictionaryKey].GetComponent<ChunkData>().voxels[blockX][blockZ][-blockY] = false;
        createMesh(hit.transform.gameObject);
    }

    void UpdateVisibleChunks()
    {
        SetVisibilityOfChunksOnList(false);
        chunks.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkWidth);
        int currentChunkCoordZ = Mathf.RoundToInt(viewerPosition.y / chunkWidth);

        for (int xRango = -chunksVisibles; xRango <= chunksVisibles; xRango++)
        {
            for (int zRango = -chunksVisibles; zRango <= chunksVisibles; zRango++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xRango, currentChunkCoordZ + zRango);

                if (chunksDictionary.ContainsKey(viewedChunkCoord))
                {
                    /*float viewerDistFromNearestEdge = Mathf.Sqrt(chunksDictionary[viewedChunkCoord].GetComponent<Renderer>().bounds.SqrDistance(viewerPosition));
                    bool visible = viewerDistFromNearestEdge >= maxViewDist;*/
                    chunksDictionary[viewedChunkCoord].SetActive(true);
                    if (chunksDictionary[viewedChunkCoord].activeSelf)
                    {
                        chunks.Add(chunksDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    CreateChunk(viewedChunkCoord.x * chunkWidth, 0, viewedChunkCoord.y * chunkWidth);
                    //chunksDictionary.Add(viewedChunkCoord, new ChunkData());
                }
            }
        }
    }

    void SetVisibilityOfChunksOnList(bool visible)
    {
        foreach (GameObject chunk in chunks)
        {
            chunk.SetActive(visible);
        }
    }
}
