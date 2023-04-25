using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ctrlEnemyLife : MonoBehaviour
{
    public int life = 100;
    private Transform myTr;
    public GameObject enemyBloodEffect;
    public Transform enemyBloodDecal;
    public ctrlEnemy enemy;
    public MeshRenderer lifeBar;

    private void Awake()
    {
        myTr = GetComponent<Transform>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            ContactPoint contact = collision.contacts[0];
            CreateBlood(contact.point);
            life -= collision.gameObject.GetComponent<ctrlBullet>().power;
            lifeBar.material.SetFloat("_Progress", life / 100.0f);
            if (life <= 0)
            {
                enemy.EnemyDie();
            }
            enemy.HitEnemy();
        }
    }

    void OnCollision(object[] _param)
    {
        CreateBlood((Vector3)_param[0]);
        life -= (int)_param[1];
        lifeBar.material.SetFloat("_Progress", life / 100.0f);
        if (life <= 0)
        {
            enemy.EnemyDie();
        }
    }

    public void OnCollisionBarrel(Vector3 firePos)
    {
        CreateBlood(firePos);
        life = 0;
        lifeBar.material.SetFloat("_Progress", life / 100.0f);

        enemy.EnemyBarrelDie(firePos);
    }
    void CreateBlood(Vector3 pos)
    {
        StartCoroutine(this.CreateBloodEffects(pos));
    }

    IEnumerator CreateBloodEffects(Vector3 pos)
    {
        GameObject enemyblood1 = Instantiate<GameObject>(enemyBloodEffect, pos, Quaternion.identity);
        Quaternion decalRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
        float scale = Random.Range(1.0f, 2.5f);

        Transform enemyblood2 = Instantiate<Transform>(enemyBloodDecal, myTr.position, decalRot);
        enemyblood2.localScale = Vector3.one * scale;
        yield return null;
    }
}
