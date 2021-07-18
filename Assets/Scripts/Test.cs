using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public static Test instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Debug.LogWarning("Multiple map managers trying to initialise!");
        }
    }

    public static Test getInstance()
    {
        return instance;
    }

    [Header("Map Properties")]
    public int width;
    public int height;
    public int cellSize;
    public int numberOfPrey;
    public int numberOfHunters;
    public Vector3 offset;
    [Range(0, 0.4f)]
    public float waterPercent;
    [Range(0, 0.05f)]
    public float probabilityDecay;
    [Range(0, 0.15f)]
    public float obstaclePercent;
    [Range(0, 0.15f)]
    public float foodPercent;
    public int seed;

    [Header("Assets")]
    public LandTile landTile;
    public WaterTile waterTile;
    public GameObject[] obstacles;
    public GameObject food;
    public GameObject prey;
    public GameObject predator;

    [Header("Object holders")]
    public GameObject tileHolder;
    public GameObject obstacleHolder;
    public GameObject foodHolder;
    public GameObject preyHolder;
    public GameObject hunterHolder;

    [Header("Uncategorized Properties")]
    public bool regenMap;

    private int[,] map;
    private GenericTile[,] mapTiles;
    private List<Vector2Int> directions;
    private List<Vector2Int> availableTiles;
    private List<GameObject> holders;
    private Gradient sampleLandGradient;
    private Gradient sampleWaterGradient;
    private float groundLevel;
    private float spawnDelayfood = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        directions = new List<Vector2Int>();
        directions.Add(new Vector2Int(-1, -1));
        directions.Add(new Vector2Int(0, -1));
        directions.Add(new Vector2Int(1, -1));
        directions.Add(new Vector2Int(-1, 0));
        directions.Add(new Vector2Int(1, 0));
        directions.Add(new Vector2Int(-1, 1));
        directions.Add(new Vector2Int(0, 1));
        directions.Add(new Vector2Int(1, 1));

        availableTiles = new List<Vector2Int>();
        holders = new List<GameObject>();

        holders.Add(tileHolder);
        holders.Add(obstacleHolder);
        holders.Add(foodHolder);
        holders.Add(preyHolder);
        holders.Add(hunterHolder);

        groundLevel = landTile.transform.localScale.y;

        CheckMapParameters(true);
        map = new int[width, height];
        mapTiles = new GenericTile[width, height];
        GenerateMap();

    }

    // Update is called once per frame
    void Update()
    {
        if (regenMap)
        {
            regenMap = false;
            RegenerateMap();
        }

        if (Input.GetKeyDown("c"))
        {
            tileHolder.GetComponent<CombineMeshes>().Combine();
        }
    }

    private void GenerateMap()
    {
        GenerateWater();
        GenerateInitialTerrain();
        ComputeDistanceFromDifferentTiles();
        SetTileColors();
        //CombineMeshTerrain();
        CombineMeshTerrain("Terrain");
        CombineMeshTerrain("Water");
        AddSurfaceObject("Obstacles", obstaclePercent);
        AddSurfaceObject("Food", foodPercent);
        SpawnWildlife("Prey");
        SpawnWildlife("Predator");
    }

    private void GenerateInitialTerrain()
    {
        bool collectSample = false;

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (map[x,y] != 2)
                {
                    map[x, y] = 1;
                    GenericTile newObject = Instantiate(landTile, offset * cellSize + new Vector3(x, 0, y) * cellSize, landTile.transform.rotation, tileHolder.transform);
                    newObject.transform.localScale = new Vector3(cellSize, newObject.transform.localScale.y, cellSize);
                    newObject.transform.position += new Vector3((float)cellSize / 2, newObject.transform.localScale.y / 2, (float)cellSize / 2);
                    newObject.GetComponent<MeshRenderer>().material.color = Color.green;
                    newObject.Start();
                    mapTiles[x, y] = newObject;
                    availableTiles.Add(new Vector2Int(x, y));

                    if (!collectSample)
                    {
                        collectSample = true;
                        LandTile sampleTile = (LandTile)newObject;
                        sampleLandGradient = sampleTile.landGradient;
                    }
                }
            }
        }
    }

    private void GenerateWater()
    {
        Random.InitState(seed);
        int index;
        int waterTiles = 0;
        int totalTiles = width * height;
        float probability;
        bool collectSample = false;

        List<Vector2Int> availableTiles = new List<Vector2Int>();
        Queue<Vector2Int> candidateTiles = new Queue<Vector2Int>();

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                availableTiles.Add(new Vector2Int(x, y));
            }
        }

        while ((float)waterTiles/ totalTiles < waterPercent)
        {
            index = Random.Range(0, availableTiles.Count);
            candidateTiles.Clear();

            candidateTiles.Enqueue(availableTiles[index]);
            availableTiles.RemoveAt(index);
            probability = 1.0f;

            while (candidateTiles.Count != 0 && (float)waterTiles / totalTiles < waterPercent)
            {
                
                Vector2Int current = candidateTiles.Dequeue();

                map[current.x, current.y] = 2;
                GenericTile newObject = Instantiate(waterTile, offset * cellSize + new Vector3(current.x, 0, current.y) * cellSize, waterTile.transform.rotation, tileHolder.transform);
                newObject.transform.localScale = new Vector3(cellSize, newObject.transform.localScale.y, cellSize);
                newObject.transform.position += new Vector3((float)cellSize / 2, newObject.transform.localScale.y / 2, (float)cellSize / 2);
                newObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                newObject.Start();
                newObject.GetComponent<WaterTile>().SetMapPosition(current);
                mapTiles[current.x, current.y] = newObject;


                waterTiles++;

                CheckNeighboursAllOrNothing(availableTiles, candidateTiles, current, probability);
                probability -= probabilityDecay;

                if (!collectSample)
                {
                    collectSample = true;
                    WaterTile sampleTile = (WaterTile)newObject;
                    sampleWaterGradient = sampleTile.waterGradient;
                }

            }
        }
    }

    private void CheckNeighboursAllOrNothing(List<Vector2Int> availableTiles, Queue<Vector2Int> candidateTiles, Vector2Int current, float probability)
    {
        double chance;
        chance = Random.Range(0.0f, 1.0f);
        if (chance <= probability)
        {
            if (current.x - 1 >= 0 && map[current.x - 1, current.y] != 2 && !candidateTiles.Contains(new Vector2Int(current.x - 1, current.y)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x - 1, current.y));
                availableTiles.Remove(new Vector2Int(current.x - 1, current.y));
            }

            if (current.x + 1 < width && map[current.x + 1, current.y] != 2 && !candidateTiles.Contains(new Vector2Int(current.x + 1, current.y)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x + 1, current.y));
                availableTiles.Remove(new Vector2Int(current.x + 1, current.y));
            }

            if (current.y - 1 >= 0 && map[current.x, current.y - 1] != 2 && !candidateTiles.Contains(new Vector2Int(current.x, current.y - 1)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x, current.y - 1));
                availableTiles.Remove(new Vector2Int(current.x, current.y - 1));
            }

            if (current.y + 1 < height && map[current.x, current.y + 1] != 2 && !candidateTiles.Contains(new Vector2Int(current.x, current.y + 1)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x, current.y + 1));
                availableTiles.Remove(new Vector2Int(current.x, current.y + 1));
            }
        }
        
    }

    private void CheckNeighbours(List<Vector2Int> availableTiles, Queue<Vector2Int> candidateTiles, Vector2Int current, float probability)
    {
        double chance;
        if (current.x - 1 >= 0 && map[current.x - 1, current.y] != 2)
        {
            chance = Random.Range(0.0f, 1.0f);
            if (chance <= probability && !candidateTiles.Contains(new Vector2Int(current.x - 1, current.y)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x - 1, current.y));
                availableTiles.Remove(new Vector2Int(current.x - 1, current.y));
            }
        }

        if (current.x + 1 < width && map[current.x + 1, current.y] != 2)
        {
            chance = Random.Range(0.0f, 1.0f);
            if (chance <= probability && !candidateTiles.Contains(new Vector2Int(current.x + 1, current.y)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x + 1, current.y));
                availableTiles.Remove(new Vector2Int(current.x + 1, current.y));
            }
        }

        if (current.y - 1 >= 0 && map[current.x, current.y - 1] != 2)
        {
            chance = Random.Range(0.0f, 1.0f);
            if (chance <= probability && !candidateTiles.Contains(new Vector2Int(current.x, current.y - 1)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x, current.y - 1));
                availableTiles.Remove(new Vector2Int(current.x, current.y - 1));
            }
        }

        if (current.y + 1 < height && map[current.x, current.y + 1] != 2)
        {
            chance = Random.Range(0.0f, 1.0f);
            if (chance <= probability && !candidateTiles.Contains(new Vector2Int(current.x, current.y + 1)))
            {
                candidateTiles.Enqueue(new Vector2Int(current.x, current.y + 1));
                availableTiles.Remove(new Vector2Int(current.x, current.y + 1));
            }
        }
    }

    private void ComputeDistanceFromDifferentTiles()
    {
        Queue<Vector2Int> edgeWaterTiles = new Queue<Vector2Int>();
        Queue<Vector2Int> edgeLandTiles = new Queue<Vector2Int>();
       
        Vector2Int curCoord;

        

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                curCoord = new Vector2Int(x, y);
                for (int i = 0; i < directions.Count; ++i)
                {
                    if (CheckEdgeNeighbourTile(curCoord + directions[i], map[x, y]))
                    {
                        mapTiles[x, y].distance = 0;
                        if (map[x,y] == 1)
                        {
                            edgeLandTiles.Enqueue(curCoord);
                        } else
                        {
                            edgeWaterTiles.Enqueue(curCoord);
                        }
                        break;
                    }
                }
            }
        }

        ComputeDistanceFromTiles(edgeLandTiles, 1);
        ComputeDistanceFromTiles(edgeWaterTiles, 2);
    }

    void ComputeDistanceFromTiles(Queue<Vector2Int> queue, int tileType)
    {
        Vector2Int current;
        Vector2Int candidate;
        int max = 0;

        while (queue.Count != 0)
        {
            current = queue.Dequeue();

            if (mapTiles[current.x, current.y].distance > max)
            {
                max = mapTiles[current.x, current.y].distance;
            }

            for (int i = 0; i < directions.Count; ++i)
            {
                candidate = current + directions[i];
                if (CheckValidNeighbour(candidate))
                {
                    if (map[candidate.x, candidate.y] == tileType)
                    {
                        if (mapTiles[candidate.x, candidate.y].distance == -1)
                        {
                            mapTiles[candidate.x, candidate.y].distance = mapTiles[current.x, current.y].distance + 1;
                            queue.Enqueue(candidate);
                        } else if (mapTiles[current.x, current.y].distance + 1 < mapTiles[candidate.x, candidate.y].distance)
                        {
                            mapTiles[candidate.x, candidate.y].distance = mapTiles[current.x, current.y].distance + 1;
                            queue.Enqueue(candidate);
                        }
                    }
                }
            }
        }

        if (tileType == 1)
        {
            LandTile.SetMaxDistance(max);
        } else
        {
            WaterTile.SetMaxDistance(max);
        }
    }

    void SetTileColors()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (mapTiles[x,y] is LandTile)
                {
                    LandTile curTile = (LandTile)mapTiles[x, y];
                    curTile.SetColor();
                } else
                {
                    WaterTile curTile = (WaterTile)mapTiles[x, y];
                    curTile.SetColor();
                }
            }
        }
    }

    void CombineMeshTerrain(string type)
    {
        // TODO: Rethink this to be able to determine when to start a new mesh
        // TODO: If the capacity of the renderer is exceeded(2^16 vertices)
        int numberOfGradientColors, startPoint, tileType;
        List<GameObject> meshHolders = new List<GameObject>();
        string holderName = "";
        Gradient targetGradient;

        if (type == "Water")
        {
            holderName = "WaterHolder";
            startPoint = 1;
            tileType = 2;
            targetGradient = sampleWaterGradient;
            numberOfGradientColors = sampleWaterGradient.colorKeys.Length;
        } else
        {
            holderName = "TerrainHolder";
            startPoint = 0;
            tileType = 1;
            targetGradient = sampleLandGradient;
            numberOfGradientColors = sampleLandGradient.colorKeys.Length;
        }

        for (int i = startPoint; i < numberOfGradientColors; ++i)
        {
            string name = holderName + i.ToString();
            GameObject newObject = new GameObject(name);

            newObject.transform.position = Vector3.zero;
            newObject.transform.rotation = Quaternion.identity;
            newObject.transform.localScale = Vector3.one;

            newObject.AddComponent<MeshFilter>();
            newObject.AddComponent<MeshRenderer>();
            newObject.AddComponent<CombineMeshes>();

            newObject.GetComponent<MeshRenderer>().material.color = targetGradient.colorKeys[i].color;

            newObject.transform.parent = tileHolder.transform;
            meshHolders.Add(newObject);
        }

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (map[x, y] == tileType)
                {
                    if (tileType == 1)
                    {
                        LandTile curTile = (LandTile)mapTiles[x, y];
                        for (int k = 0; k < meshHolders.Count; ++k)
                        {
                            if (curTile.GetComponent<MeshRenderer>().material.color == meshHolders[k].GetComponent<MeshRenderer>().material.color)
                            {
                                curTile.transform.parent = meshHolders[k].transform;
                                break;
                            }
                        }
                    } else
                    {
                        WaterTile curTile = (WaterTile)mapTiles[x, y];
                        for (int k = 0; k < meshHolders.Count; ++k)
                        {
                            if (curTile.GetComponent<MeshRenderer>().material.color == meshHolders[k].GetComponent<MeshRenderer>().material.color)
                            {
                                curTile.transform.parent = meshHolders[k].transform;
                                break;
                            }
                        }
                    }
                }
            }
        }

        for (int k = 0; k < meshHolders.Count; ++k)
        {
            meshHolders[k].GetComponent<CombineMeshes>().Combine();
        }
    }

    // TODO: Redo identify logic; how do i discern what do i need to create
    void AddSurfaceObject(string label, float surfaceLimit)
    {
        int numberOfAvailableTiles, counter, index;
        Vector2Int coord;

        numberOfAvailableTiles = availableTiles.Count;
        counter = 0;
        Debug.Log("Available tiles before populating: " + availableTiles.Count);


        while ((float)counter / numberOfAvailableTiles < surfaceLimit)
        {
            index = Random.Range(0, availableTiles.Count);
            coord = availableTiles[index];
            availableTiles.RemoveAt(index);

            // These should be the obstacles
            if (label == "Obstacles")
            {
                index = Random.Range(0, obstacles.Length);
                map[coord.x, coord.y] = 3;

                GameObject newObstacle = Instantiate(obstacles[index], offset * cellSize + new Vector3(coord.x, 0, coord.y) * cellSize, obstacles[index].transform.rotation, obstacleHolder.transform);
                newObstacle.transform.position += new Vector3((float)cellSize / 2, landTile.transform.localScale.y, (float)cellSize / 2);
            } else if (label == "Food")
            {
                GameObject newObstacle = Instantiate(food, offset * cellSize + new Vector3(coord.x, 0, coord.y) * cellSize, food.transform.rotation, foodHolder.transform);
                newObstacle.transform.position += new Vector3((float)cellSize / 2, landTile.transform.localScale.y, (float)cellSize / 2);
                newObstacle.GetComponent<Food>().SetMapPosition(coord);
            }

            counter++;
        }

        Debug.Log("Number of tiles populated: " + counter);
        Debug.Log("Available tiles after populating: " + availableTiles.Count);
        Debug.Log('\n');
    }

    void SpawnWildlife(string label)
    {
        int index;
        Vector2Int coord;

        if (label == "Prey")
        {
            for (int i = 0; i < numberOfPrey; ++i)
            {
                index = Random.Range(0, availableTiles.Count);
                coord = availableTiles[index];
                availableTiles.RemoveAt(index);

                GameObject newLife = Instantiate(prey, offset * cellSize + new Vector3(coord.x, 0, coord.y) * cellSize, prey.transform.rotation, preyHolder.transform);
                newLife.transform.position += new Vector3((float)cellSize / 2, landTile.transform.localScale.y, (float)cellSize / 2);
                newLife.GetComponent<Bunny>().SetInitialPosition(new Vector2Int(coord.x, coord.y));
                newLife.GetComponent<Bunny>().SetGender(i % 2);

                GeneticsLab.GenerateRandomBunnyCharacteristics(newLife.GetComponent<Bunny>());

                if (i % 2 == 0)
                {
                    // Paint their fur a bit different
                    newLife.GetComponent<MeshRenderer>().materials[0].color = Color.magenta;
                } else
                {
                    // Paint the fur in accord with the attractiveness
                    Color bunnyColor = newLife.GetComponent<MeshRenderer>().materials[0].color;
                    float attractivenessValue = (1 - newLife.GetComponent<Bunny>().GetAttractiveness());
                    float newValue = Mathf.Lerp(0.30f, 0.0f, attractivenessValue);
                    //Debug.Log("Color: " + bunnyColor.g);
                    bunnyColor.g += newValue;
                    //Debug.Log("Color: " + bunnyColor.g);
                    newLife.GetComponent<MeshRenderer>().materials[0].color = bunnyColor;
                }

            }
        } else
        {
            for (int i = 0; i < numberOfHunters; ++i)
            {
                index = Random.Range(0, availableTiles.Count);
                coord = availableTiles[index];
                availableTiles.RemoveAt(index);

                GameObject newLife = Instantiate(predator, offset * cellSize + new Vector3(coord.x, 0, coord.y) * cellSize, predator.transform.rotation, hunterHolder.transform);
                newLife.transform.position += new Vector3((float)cellSize / 2, landTile.transform.localScale.y, (float)cellSize / 2);
                newLife.GetComponent<Wolf>().SetInitialPosition(new Vector2Int(coord.x, coord.y));
                newLife.GetComponent<Wolf>().SetGender(i % 2);

                GeneticsLab.GenerateRandomWolfCharacteristics(newLife.GetComponent<Wolf>());

                if (i % 2 == 0)
                {
                    // Paint their fur a bit different
                    newLife.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(Color.white, new Color(0.71f, 0.396f, 0.114f), 0.75f);
                }
            }
        }
    }

    bool CheckValidNeighbour(Vector2Int coord)
    {
        int x = coord.x;
        int y = coord.y;

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return true;
        }

        return false;
    }

    public bool CheckValidMovementTile(Vector2Int coord)
    {
        int x = coord.x;
        int y = coord.y;

        if (x >= 0 && x < width && y >= 0 && y < height && map[x,y] == 1)
        {
            return true;
        }

        return false;
    }

    public void SpawnNewLife(Animals animal)
    {
        if (animal is Bunny) {
            SpawnNewBunny((Bunny)animal);
        } else if (animal is Wolf)
        {
            SpawnNewWolf((Wolf)animal);
        }
    }

    public void SpawnNewBunny(Bunny bunny)
    {
        GameObject newBunny = Instantiate(prey, bunny.transform.position, prey.transform.rotation, preyHolder.transform);
        Bunny newBunnyScript = newBunny.GetComponent<Bunny>();
        newBunnyScript.SetInitialPosition(bunny.GetMapPosition());

        int gender = Random.Range(0, 2);
        newBunnyScript.SetGender(gender);
        newBunnyScript.SetMaxScale(prey.transform.localScale.x);

        GeneticsLab.CreateNewAnimal(bunny.GetBunnyGenetics(), bunny.GetPartnerGenetics(), newBunnyScript);

        float percent = Mathf.InverseLerp(newBunnyScript.gestationTimeRange.x, newBunnyScript.gestationTimeRange.y, newBunnyScript.GetGestationTime());
        float developmentPercent = Mathf.Lerp(newBunnyScript.developmentRange.x, newBunnyScript.developmentRange.y, percent);

        newBunnyScript.SetDevelopmentPercent(developmentPercent);

        if (gender == 0)
        {
            newBunnyScript.GetComponent<MeshRenderer>().materials[0].color = Color.magenta;
        } else
        {
            // Paint the fur in accord with the attractiveness
            Color bunnyColor = newBunny.GetComponent<MeshRenderer>().materials[0].color;
            float attractivenessValue = (1 - newBunnyScript.GetAttractiveness());
            float newValue = Mathf.Lerp(0.30f, 0.0f, attractivenessValue);
            bunnyColor.g += newValue;
            newBunny.GetComponent<MeshRenderer>().materials[0].color = bunnyColor;
        }
    }

    public void SpawnNewWolf(Wolf wolf)
    {
        GameObject newWolf = Instantiate(predator, wolf.transform.position, predator.transform.rotation, hunterHolder.transform);
        Wolf newWolfScript = newWolf.GetComponent<Wolf>();
        newWolfScript.SetInitialPosition(wolf.GetMapPosition());

        int gender = Random.Range(0, 2);
        newWolfScript.SetGender(gender);
        newWolfScript.SetMaxScale(predator.transform.localScale.x);

        GeneticsLab.CreateNewAnimal(wolf.GetBunnyGenetics(), wolf.GetPartnerGenetics(), newWolfScript);

        float percent = Mathf.InverseLerp(newWolfScript.gestationTimeRange.x, newWolfScript.gestationTimeRange.y, newWolfScript.GetGestationTime());
        float developmentPercent = Mathf.Lerp(newWolfScript.developmentRange.x, newWolfScript.developmentRange.y, percent);

        newWolfScript.SetDevelopmentPercent(developmentPercent);

        if (gender == 0)
        {
            newWolfScript.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(Color.white, new Color(0.71f, 0.396f, 0.114f), 0.75f);
        }
        else
        {
            // Paint the fur in accord with the attractiveness
            Color wolfColor = newWolf.GetComponent<MeshRenderer>().materials[0].color;
            float attractivenessValue = (1 - newWolfScript.GetAttractiveness());
            float newValue = Mathf.Lerp(0.30f, 0.0f, attractivenessValue);
            wolfColor.g += newValue;
            newWolf.GetComponent<MeshRenderer>().materials[0].color = wolfColor;
        }
    }

    public List<Vector2Int> GetMapNeighbours(Vector2Int reference)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int i = 0; i < directions.Count; ++i)
        {
            int x = reference.x + directions[i].x;
            int y = reference.y + directions[i].y;

            Vector2Int current = new Vector2Int(x, y);

            if (CheckValidNeighbour(current))
            {
                result.Add(current);
            }
        }

        return result;
    }

    public int GetMapValue(Vector2Int reference)
    {
        return map[reference.x, reference.y];
    }

    public void SpawnNewFood(Vector2Int position)
    {
        availableTiles.Add(position);
        StartCoroutine(SpawnFood());

    }

    IEnumerator SpawnFood()
    {
        yield return new WaitForSeconds(spawnDelayfood);

        int index = Random.Range(0, availableTiles.Count);
        Vector2Int coord = availableTiles[index];
        availableTiles.RemoveAt(index);

        GameObject newFood = Instantiate(food, offset * cellSize + new Vector3(coord.x, 0, coord.y) * cellSize, food.transform.rotation, foodHolder.transform);
        newFood.transform.position += new Vector3((float)cellSize / 2, landTile.transform.localScale.y, (float)cellSize / 2);
        newFood.GetComponent<Food>().SetMapPosition(coord);

    }

    bool CheckEdgeNeighbourTile(Vector2Int coord, int type)
    {
        int x = coord.x;
        int y = coord.y;

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (map[x, y] != type)
                return true;
        }

        return false;
    }

    void RegenerateMap()
    {
        if (CheckMapParameters(false))
        {
            // Clear everything that we have on the map
            for (int i = 0; i < holders.Count; i++)
            {
                foreach (Transform child in holders[i].transform)
                {
                    if (child != holders[i].transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            map = new int[width, height];
            mapTiles = new GenericTile[width, height];
            GenerateMap();
        }
    }

    bool CheckMapParameters(bool firstTimeCheck)
    {
        bool correct = true;
        if (width < 1)
        {
            correct = false;
            Debug.LogWarning("Width of the map cannot be smaller than 1!");

            if (firstTimeCheck)
            {
                width = 1;
            } 
        }

        if (height < 1)
        {
            correct = false;
            Debug.LogWarning("Height of the map cannot be smaller than 1!");

            if (firstTimeCheck)
            {
                height = 1;
            }
        }

        if (cellSize < 1)
        {
            correct = false;
            Debug.LogWarning("The size of the cell cannot be smaller than 1!");
            
            if (firstTimeCheck)
            {
                cellSize = 1;
            }
        }

        if (!correct)
        {
            if (firstTimeCheck)
            {
                Debug.LogWarning("Set all the invalid values back to 1!");
            } else
            {
                Debug.LogWarning("Cannot regenerate map!");
                return false;
            }
        }

        return true;
    }

    // Returns the world position of the CENTER of a tile position given in map coordinates
    public Vector3 ConvertMapPositionToWorldPosition(Vector2Int mapPosition)
    {
        return new Vector3(
                            (mapPosition.x + offset.x) * cellSize + cellSize / 2.0f,
                            groundLevel,
                            (mapPosition.y + offset.y) * cellSize + cellSize / 2.0f
                          );
    }
}
