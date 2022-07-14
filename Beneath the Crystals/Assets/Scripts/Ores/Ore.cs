using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore : MonoBehaviour
{
    [SerializeField] private string oreName;
    [SerializeField] private int value;
    [SerializeField] private SpriteRenderer sr;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(oreName);
        if (collision.tag == "Player")
        {
            Player._instance.Score += value;
            gameObject.SetActive(false);
        }
    }
}
