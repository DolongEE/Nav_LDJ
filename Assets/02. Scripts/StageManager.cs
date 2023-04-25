using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private Transform[] EnemySpawnPoints;
    public GameObject Enemy;
    private bool gameEnd;
    private GameObject[] Enemys;

    public ctrlBase baseStart;

    private void Awake()
    {
        EnemySpawnPoints = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();
        StartCoroutine(this.CreateEnemy());
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(5.0f);
        baseStart.StartBase();
    }

    IEnumerator CreateEnemy()
    {
        while (!gameEnd)
        {
            yield return new WaitForSeconds(5.0f);
            Enemys = GameObject.FindGameObjectsWithTag("Enemy");
            if (Enemys.Length < 20)
            {
                for (int i = 1; i < EnemySpawnPoints.Length; i++)
                {
                    Instantiate(Enemy, EnemySpawnPoints[i].localPosition, EnemySpawnPoints[i].localRotation);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
