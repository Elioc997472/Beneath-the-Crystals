using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField] private string type;
    [SerializeField] private int hardness;
    [SerializeField] private float health;
    [SerializeField] private float currDamage;
    [SerializeField] public List<OreDrop> drops;

    [System.Serializable]
    public struct OreDrop
    {
        public GameObject ore;
        public float dropChance; //% chance of dropping
        public int dropQuantity;
    }

    public void Mining()
    {
        if (Player._instance.MiningStrength >= hardness)
        {
            if (currDamage > health) BreakBlock();
            //start incrementing
            currDamage += Player._instance.MiningSpeed * Time.deltaTime;
        }
        else
        {
            //Play bonk bonk bad sound and don't mine
        }
    }

    public void ResetDamage()
    {
        currDamage = 0;
    }

    private void BreakBlock()
    {
        foreach (OreDrop drop in drops)
        {
            if (Random.Range(0f, 100f) <= drop.dropChance)
            {
                for (int i = 0; i < drop.dropQuantity; i++) Instantiate(drop.ore, transform.position, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }
}
