using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore_Generator : MonoBehaviour
{
    [Header("Ore Generation Settings")]
    [SerializeField] private OreRules[] rules;

    private Map_Generator map_gen;
    private LinkedList<Vector2Int> veinList = new LinkedList<Vector2Int>();

    public void Init()
    {
        map_gen = GetComponent<Map_Generator>();
        SetRenderIDs();
    }

    [System.Serializable]
    public struct OreRules
    {
        public GameObject block;
        [Range(100, 1000)]
        public int renderID; //Set
        public int minHeight;
        public int maxHeight;
        [Range(0, 100)]
        public float veinSpawnChance; //Percent chance to start a vein
        public int maxVeins; //If null, there will be no limit to spawn number
        public Vector2Int veinSizeRange; //Randomly selects number of attempts to propagate the vein
    }

    private void SetRenderIDs()
    {
        foreach (OreRules rule in rules) map_gen.IDToBlock.Add(rule.renderID, rule.block);
    }

    public void GenerateOres()
    {
        System.Random rand = new System.Random(map_gen.seed.GetHashCode());
        foreach (OreRules rule in rules)
        {
            int veinCt = 0;
            for (int y = rule.minHeight; y < rule.maxHeight || y < map_gen.height; y++)
            {
                for (int x = 0; x < map_gen.width && veinCt < rule.maxVeins; x++)
                {
                    if (rand.Next(0, 100) < rule.veinSpawnChance && map_gen.map[x, y] == 1)
                    {
                        map_gen.map[x, y] = rule.renderID;
                        veinList.AddLast(new Vector2Int(x, y));
                        veinCt++;
                    }
                }
            }
            foreach (Vector2Int cord in veinList)
            {
                int spawnChances = Random.Range(rule.veinSizeRange.x, rule.veinSizeRange.y);
                Vector2Int currPos = cord;
                while (spawnChances > 0)
                {
                    spawnChances--;
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            currPos.x += 1;
                            break;
                        case 1:
                            currPos.y -= 1;
                            break;
                        case 2:
                            currPos.x -= 1;
                            break;
                        case 3:
                            currPos.y += 1;
                            break;
                    }//Choose direction
                    if (currPos.x < 0 || currPos.x >= map_gen.width || currPos.y < 0 || currPos.y >= map_gen.height) currPos = cord;
                    if (map_gen.map[currPos.x, currPos.y] == 1) map_gen.map[currPos.x, currPos.y] = rule.renderID;
                }
            }
        }
    }
}
