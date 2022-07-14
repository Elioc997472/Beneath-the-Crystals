using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map_Generator : MonoBehaviour
{
    [Header("Terrain Generation")]
    public int width;
    public int height;
    [SerializeField] Vector2Int startingPlain;
    [SerializeField] float smoothness; //DO NOT SET TO 0!
    public float seed;

    [Header("von Neumman Cellular Automata")]
    [SerializeField] int fillPercent;
    [SerializeField] int smoothCount;

    [Header("Vertical Tunnel Generation")]
    [SerializeField] private Vector2Int widthRange;
    [SerializeField] private int maxWidthChange;
    [SerializeField] private int maxPathChange;
    [SerializeField] private int roughness;
    [SerializeField] private int curvyness;
    [SerializeField] private int tunnelXOffset = 0;

    [Header("GameObject References")]
    [SerializeField] private GameObject grassTile; //-2
    [SerializeField] private GameObject dirtTile; //-1
    [SerializeField] private GameObject stoneTile; //0

    public int[,] map;
    public Dictionary<int, GameObject> IDToBlock = new Dictionary<int, GameObject>();

    private Ore_Generator ore_gen;
    // Start is called before the first frame update
    void Awake()
    {
        startingPlain.y = height - 10;
        map = new int[width, height];
        IDToBlock.Add(-2, grassTile);
        IDToBlock.Add(-1, dirtTile);
        IDToBlock.Add(1, stoneTile);
        ore_gen = GetComponent<Ore_Generator>();
        ore_gen.Init();
        Generation();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Generation();
    }

    void Generation()
    {
        foreach (Transform child in transform) GameObject.Destroy(child.gameObject);
        seed = Random.Range(0f, 100f);
        map = new int[width, height];
        GenerateCellularAutomata();
        SmoothVNCellularAutomata();
        TopGeneration();
        DirectionalTunnel(widthRange, maxWidthChange, maxPathChange, roughness, curvyness);
        ConnectCaves();
        ore_gen.GenerateOres();
        RenderMap();
    }

    private void GenerateCellularAutomata()
    {
        //Seed our random number generator
        System.Random rand = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < startingPlain.y - 10; y++)
            {
                //If we have the edges set to be walls, ensure the cell is set to on (1)
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    //Randomly generate the grid
                    map[x, y] = (rand.Next(0, 100) < fillPercent) ? 1 : 0;
                }
            }
        }
    }

    private int GetVNSurroundingTiles(int x, int y)
    {
        /* von Neumann Neighbourhood looks like this ('T' is our Tile, 'N' is our Neighbour)
        *
        *   N
        * N T N
        *   N
        *
        */

        int tileCount = 0;

        //Keep the edges as walls
        if (x - 1 == 0 || x + 1 == map.GetUpperBound(0) || y - 1 == 0 || y + 1 == map.GetUpperBound(1))
        {
            tileCount++;
        }

        //Ensure we aren't touching the left side of the map
        if (x - 1 > 0)
        {
            tileCount += map[x - 1, y];
        }

        //Ensure we aren't touching the bottom of the map
        if (y - 1 > 0)
        {
            tileCount += map[x, y - 1];
        }

        //Ensure we aren't touching the right side of the map
        if (x + 1 < map.GetUpperBound(0))
        {
            tileCount += map[x + 1, y];
        }

        //Ensure we aren't touching the top of the map
        if (y + 1 < map.GetUpperBound(1))
        {
            tileCount += map[x, y + 1];
        }

        return tileCount;
    }

    private void SmoothVNCellularAutomata()
    {
        for (int i = 0; i < smoothCount; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < startingPlain.y - 10; y++)
                {
                    //Get the surrounding tiles
                    int surroundingTiles = GetVNSurroundingTiles(x, y);

                    if (x == 0 || x == map.GetUpperBound(0) - 1 || y == 0 || y == map.GetUpperBound(1))
                    {
                        //Keep our edges as walls
                        map[x, y] = 1;
                    }
                    //von Neuemann Neighbourhood requires only 3 or more surrounding tiles to be changed to a tile
                    else if (surroundingTiles > 2)
                    {
                        map[x, y] = 1;
                    }
                    else if (surroundingTiles < 2)
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }
    }

    //Tbh not sure whether to use. Is it needed? Connect horizontally or vertically?
    private void ConnectCaves()
    {
        for (int x = 3; x < width; x++)
        {
            for (int y = 0; y < startingPlain.y - 10; y++)
            {
                if (map[x - 3, y] == 1 && map[x, y] == 1)
                {
                    map[x - 2, y] = 1;
                    map[x - 1, y] = 1;
                }
            }
        }
    }

    private int[,] DirectionalTunnel(Vector2Int widthRange, int maxWidthChange, int maxPathChange, int roughness, int curvyness)
    {
        //This value goes from its minus counterpart to its positive value, in this case with a width value of 1, the width of the tunnel is 3
        int tunnelWidth = 1;
        //Set the start X position to the center of the tunnel
        int x = (tunnelXOffset == 0) ? map.GetUpperBound(0) / 2 : tunnelXOffset;

        //Set up our random with the seed
        System.Random rand = new System.Random(Time.time.GetHashCode());

        //Create the first part of the tunnel
        for (int i = -tunnelWidth; i <= tunnelWidth; i++)
        {
            map[x + i, 0] = 0;
        }
        //Cycle through the array
        for (int y = 1; y < map.GetUpperBound(1); y++)
        {
            //Check if we can change the roughness
            if (rand.Next(0, 100) > roughness)
            {
                //Get the amount we will change for the width
                int widthChange = Random.Range(-maxWidthChange, maxWidthChange);
                //Add it to our tunnel width value
                tunnelWidth += widthChange;
                //Check to see we arent making the path too small
                if (tunnelWidth < widthRange.x)
                {
                    tunnelWidth = widthRange.x;
                }
                //Check that the path width isnt over our maximum
                if (tunnelWidth > widthRange.y)
                {
                    tunnelWidth = widthRange.y;
                }
            }

            //Check if we can change the curve
            if (rand.Next(0, 100) > curvyness)
            {
                //Get the amount we will change for the x position
                int xChange = Random.Range(-maxPathChange, maxPathChange);
                //Add it to our x value
                x += xChange;
                //Check we arent too close to the left side of the map
                if (x < widthRange.y)
                {
                    x = widthRange.y;
                }
                //Check we arent too close to the right side of the map
                if (x > (map.GetUpperBound(0) - widthRange.y))
                {
                    x = map.GetUpperBound(0) - widthRange.y;
                }
            }

            //Work through the width of the tunnel
            for (int i = -tunnelWidth; i <= tunnelWidth; i++)
            {
                map[x + i, y] = 0;
            }
        }
        return map;
    }
    
    /// <summary>
    /// Generates the 20 blocks of the surface after cellular automata cave generation
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    private int[,] TopGeneration()
    { 
        if (smoothness == 0) throw new System.Exception("Cannot generate terrain with 0 smoothness!");
        int perlinChange;
        int dirtPerlin;
        bool startPerlin = false;
        //Start with smooth area on left
        for (int i = 0; i < width; i++)
        {
            perlinChange = Mathf.RoundToInt(Mathf.Lerp(-10, 10, Mathf.PerlinNoise(i / smoothness, seed)));
            dirtPerlin = Mathf.RoundToInt(Mathf.Lerp(0, 3, Mathf.PerlinNoise(i / smoothness, seed)));
            startPerlin = i >= startingPlain.x && (startPerlin || Mathf.Abs(perlinChange) <= 1);
            perlinChange = (i < startingPlain.x || !startPerlin) ? 0 : perlinChange;
            for (int j = height - 20; j < startingPlain.y + perlinChange; j++)
            {
                if (j >= startingPlain.y + perlinChange - dirtPerlin) map[i, j] = -1;
                else map[i, j] = 1;
            }
            //Top layer should be grass
            map[i, startingPlain.y + perlinChange] = -2;
        }
        return map;
    }

    private void RenderMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i, j] == 0) continue;
                Instantiate(IDToBlock[map[i, j]], new Vector2(i, j), Quaternion.identity, transform);
            }
        }
    }
}
