using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextGameColliderScript : MonoBehaviour
{
    Vector3 game1_startPos = new Vector3(-58.56f, 0.54f, 7.49f);
    Vector3 game2_startPos = new Vector3(-68.96f, 0.54f, 7.49f);
    Vector3 game3_startPos = new Vector3(-79.87f, 0.54f, 7.49f);
    Vector3 game4_startPos = new Vector3(-92.23f, 0.54f, 5.08f);

    [SerializeField] private Collider c;
    [SerializeField] private int level = 0;
    private List<Vector3> level_vectors = new List<Vector3>();

    [SerializeField] private GameObject player;

    void Start()
    {
        // Inicializ�l�s
        level_vectors.Add(game1_startPos);
        level_vectors.Add(game2_startPos);
        level_vectors.Add(game3_startPos);
        level_vectors.Add(game4_startPos);

        // J�t�kos kezd� poz�ci� be�ll�t�sa
        if (player != null)
        {
            player.transform.position = game1_startPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Ellen�rizz�k, hogy van-e k�vetkez� szint
            if (level < level_vectors.Count - 1)
            {
                level++; // Szint l�ptet�se
                player.transform.position = level_vectors[level]; // Teleport�l�s
            }
        }
    }
}
