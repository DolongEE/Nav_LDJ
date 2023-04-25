using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ctrlBase : MonoBehaviour
{
    [HideInInspector]
    public bool isDie;

    public float dist1;
    public float dist2;

    private GameObject[] Enemys;
    private Transform EnemyTarget;

    private Transform myTr;

    public Transform targetTr;

    private bool shoot;
    private float enemyLookTime;
    private Quaternion enemyLookRotation;

    public GameObject bullet;
    public Transform firePos;
    private float bulletSpeed;
    private AudioSource source = null;
    public GameObject muzzleFlash;

    public AudioClip fireSfx;

    Ray ray;
    RaycastHit rayHit;
    bool check;

    public LineRenderer rayLine;
    public Transform rayDot;

    void Awake()
    {
        bullet = Resources.Load<GameObject>("Bullet/Bullet");
        fireSfx = Resources.Load<AudioClip>("Bullet/bazooka");

        myTr = GetComponent<Transform>();
        source = GetComponent<AudioSource>();
        muzzleFlash.SetActive(false);
    }
    public void StartBase()
    {
        StartCoroutine(this.TargetSetting());
        StartCoroutine(this.ShootSetting());
    }
    void Update()
    {
        ray.origin = firePos.position;
        ray.direction = firePos.TransformDirection(Vector3.forward);

        Debug.DrawRay(ray.origin, ray.direction * 30.0f, Color.green);
        if (Physics.Raycast(ray, out rayHit, 30.0f))
        {
            Vector3 posValue = firePos.InverseTransformPoint(rayHit.point);
            rayLine.SetPosition(0, posValue);
            rayDot.localPosition = posValue;

            if (shoot && rayHit.collider.tag == "Enemy")
            {
                check = true;
            }
        }
        else
        {
            rayLine.SetPosition(0, new Vector3(0, 0, 30.0f));
            rayDot.localPosition = Vector3.zero;
        }

        if (!shoot)
        {
            myTr.RotateAround(targetTr.position, Vector3.up, Time.deltaTime * 55.0f);
            check = false;
        }
        else
        {
            if (shoot)
            {
                if (Time.time > enemyLookTime)
                {
                    //enemyLookRotation = Quaternion.LookRotation(-(EnemyTarget.forward)); // - 해줘야 바라봄
                    enemyLookRotation = Quaternion.LookRotation(EnemyTarget.position - myTr.position);
                    myTr.rotation = Quaternion.Lerp(myTr.rotation, enemyLookRotation, Time.deltaTime * 2.0f);
                    enemyLookTime = Time.time + 0.01f;
                }
            }
        }

        if (shoot && check)
        {
            if (Time.time > bulletSpeed)
            {
                ShootStart();
                bulletSpeed = Time.time + 0.3f;
            }
        }
    }

    IEnumerator TargetSetting()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);
            Enemys = GameObject.FindGameObjectsWithTag("EnemyBody");
            Transform EnemyTargets = Enemys[0].transform;
            float dist = (EnemyTargets.position - myTr.position).sqrMagnitude;
            foreach (GameObject _Enemy in Enemys)
            {
                if ((EnemyTargets.position - myTr.position).sqrMagnitude < dist)
                {
                    EnemyTargets = _Enemy.transform;
                    dist = (EnemyTargets.position - myTr.position).sqrMagnitude;
                }
            }
            EnemyTarget = EnemyTargets;
        }
    }

    IEnumerator ShootSetting()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);
            dist2 = Vector3.Distance(myTr.position, EnemyTarget.position);
            if (dist2 < 35f)
            {
                shoot = true;
            }
            else
            {
                shoot = false;
            }
        }
    }

    private void ShootStart()
    {
        StartCoroutine(this.FireStart());
    }

    IEnumerator FireStart()
    {
        Instantiate(bullet, firePos.position, firePos.rotation);
        source.PlayOneShot(fireSfx, fireSfx.length + 0.2f);

        float scale = Random.Range(1.0f, 2.5f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360));
        muzzleFlash.transform.localRotation = rot;

        muzzleFlash.SetActive(true);

        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        muzzleFlash.SetActive(false);
    }
}
