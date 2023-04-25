using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ctrlPlayer : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent myTraceAgent;
    Vector3 movePoint = Vector3.zero;

    Ray ray;
    RaycastHit rayHit1;
    RaycastHit rayHit2;

    [HideInInspector]
    public bool isDie;

    public float dist1;
    public float dist2;

    private GameObject[] Enemys;
    private Transform EnemyTarget;

    private Transform myTr;
    public Transform targetTr;

    private bool shot;
    private float enemyLookTime;
    private Quaternion enemyLookRotation;

    public Transform firePos;
    private float bulletSpeed;
    private AudioSource source = null;
    public GameObject muzzleFlash;
    public AudioClip fireSfx;

    bool check;

    public LineRenderer rayLine;
    public Transform rayDot;
    private bool turnRight;
    private float turnValue;

    public int power;
    private bool FireAction;

    Animator myAnim;

    public int life;
    public Image lifeBar;

    public GameObject damageEffect;
    public Projector damageProjector;

    [HideInInspector]
    public bool barrelFire;
    private Transform barrelPos;

    void Awake()
    {
        myTr = GetComponent<Transform>();
        myTraceAgent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        source = GetComponent<AudioSource>();
        muzzleFlash.SetActive(false);
        myAnim = GetComponentInChildren<Animator>();
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(5.0f);
        StartCoroutine(this.TargetSetting());
        StartCoroutine(this.ShotSetting());
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.blue);

        if (Input.GetMouseButtonDown(0) && !isDie)
        {
            if (Physics.Raycast(ray, out rayHit1, Mathf.Infinity, 1 << LayerMask.NameToLayer("Barrel")))
            {
                movePoint = rayHit1.point;

                myTraceAgent.destination = movePoint;
                myTraceAgent.stoppingDistance = 10.0f;
            }
            else if (Physics.Raycast(ray, out rayHit1, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                movePoint = rayHit1.point;

                myTraceAgent.destination = movePoint;
                myTraceAgent.stoppingDistance = 0.0f;
            }
        }

        ray.origin = firePos.position;
        ray.direction = firePos.TransformDirection(Vector3.forward);

        if (Physics.Raycast(ray, out rayHit1, 15.0f))
        {
            Vector3 posValue = firePos.InverseTransformPoint(rayHit1.point);
            rayLine.SetPosition(0, posValue);
            rayDot.localPosition = posValue;
            if (shot && (rayHit1.collider.tag == "Enemy" || rayHit1.collider.tag == "Barrel"))
            {
                check = true;
            }
        }
        else
        {
            if (Mathf.Abs(myTraceAgent.velocity.z) > 0)
            {
                rayLine.SetPosition(0, new Vector3(0, 0, 0));
                rayDot.localPosition = Vector3.zero;
            }
            else
            {
                rayLine.SetPosition(0, new Vector3(0, 0, 15.0f));
                rayDot.localPosition = Vector3.zero;
            }
        }

        if (!shot)
        {
            if (FireAction)
            {
                if (Time.time > turnValue)
                {
                    turnRight = !turnRight;
                    turnValue = Time.time + 1.5f;
                }
                if (turnRight)
                {
                    myTr.Rotate(Vector3.up * Time.deltaTime * 55.0f);
                }
                if (!turnRight)
                {
                    myTr.Rotate(Vector3.up * -Time.deltaTime * 55.0f);
                }
            }
            check = false;
        }
        else
        {
            if (shot)
            {
                if (Time.time > enemyLookTime)
                {
                    Vector3 targetDir = EnemyTarget.position - myTr.position;
                    float dotValue = Vector3.Dot(myTr.forward, targetDir.normalized);
                    if (dotValue > 1.0f)
                    {
                        dotValue = 1.0f;
                    }
                    else if (dotValue < -1.0f)
                    {
                        dotValue = -1.0f;
                    }

                    float value = Mathf.Acos(dotValue);
                    if (value * Mathf.Rad2Deg > 17.0f)
                    {
                        enemyLookRotation = Quaternion.LookRotation(EnemyTarget.position - myTr.position);
                        myTr.rotation = Quaternion.Lerp(myTr.rotation, enemyLookRotation, Time.deltaTime * 7.0f);
                        enemyLookTime = Time.time * 0.01f;
                    }
                    else
                    {
                        myTr.LookAt(EnemyTarget);
                    }
                }
            }
        }
        if (shot && check)
        {
            if (Time.time > bulletSpeed)
            {
                ShotStart();
                bulletSpeed = Time.time + 0.3f;
            }
        }
        myAnim.SetFloat("Speed", Mathf.Abs(myTraceAgent.velocity.z));
        if (shot)
        {
            myAnim.SetBool("shot", true);
        }
        else
        {
            myAnim.SetBool("shot", false);
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
                if ((_Enemy.transform.position - myTr.position).sqrMagnitude < dist)
                {
                    EnemyTargets = _Enemy.transform;
                    dist = (EnemyTargets.position - myTr.position).sqrMagnitude;
                }
            }
            if (barrelFire)
            {
                EnemyTarget = barrelPos;
            }
            else
            {
                EnemyTarget = EnemyTargets;
            }
        }
    }
    IEnumerator ShotSetting()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);
            dist2 = Vector3.Distance(myTr.position, EnemyTarget.position);

            if (myTraceAgent.velocity.z != 0.0f)
            {
                FireAction = false;
            }
            else
            {
                FireAction = true;
            }

            if (FireAction)
            {
                if (dist2 < 20.0f)
                {
                    shot = true;
                }
                else
                {
                    shot = false;
                }
            }
            else
            {
                shot = false;
            }
        }
    }
    private void ShotStart()
    {
        if (!isDie)
        {
            StartCoroutine(this.FireStart());
        }
    }

    IEnumerator FireStart()
    {
        source.PlayOneShot(fireSfx, fireSfx.length - 0.2f);

        float scale = Random.Range(1.0f, 1.3f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360));
        muzzleFlash.transform.localRotation = rot;
        muzzleFlash.SetActive(true);

        if (Physics.Raycast(firePos.position, firePos.forward, out rayHit2, 17.0f))
        {
            if (rayHit2.collider.tag == "Enemy")
            {
                object[] _params = new object[2];
                _params[0] = rayHit2.point;
                _params[1] = power;
                rayHit2.collider.gameObject.SendMessage("OnCollision", _params, SendMessageOptions.DontRequireReceiver);
            }
            else if (rayHit2.collider.tag == "Barrel")
            {
                Debug.Log(2);
                object[] _params = new object[2];
                _params[0] = rayHit2.point;
                _params[1] = firePos.position;
                rayHit2.collider.gameObject.SendMessage("OnCollision", _params, SendMessageOptions.DontRequireReceiver);
            }
        }

        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        muzzleFlash.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "EnemyWeapon")
        {
            ContactPoint contact = collision.contacts[0];

            CreateDamage(contact.point);

            int damage = collision.gameObject.GetComponent<EnemyWeapon>().power;
            life -= damage;
            lifeBar.fillAmount += damage / 100.0f;
            damageProjector.farClipPlane -= damage / 100.0f;

            if (life <= 0)
            {
                StartCoroutine(PlayerDie());
            }
        }
    }
    void CreateDamage(Vector3 pos)
    {
        StartCoroutine(this.CreateDamageEffect(pos));
    }

    IEnumerator CreateDamageEffect(Vector3 pos)
    {
        Instantiate(damageEffect, pos, Quaternion.identity);
        yield return null;
    }

    IEnumerator PlayerDie()
    {
        isDie = true;
        firePos.gameObject.SetActive(false);
        gameObject.tag = "Untagged";
        myTraceAgent.enabled = false;

        Rigidbody[] rig = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody _rig in rig)
        {
            _rig.constraints = RigidbodyConstraints.FreezePositionZ;
        }
        myAnim.enabled = false;
        yield return null;
    }
    public void BarrelFire(Transform barrelTr)
    {
        barrelPos = barrelTr;
        barrelFire = true;
    }
}

// https://docs.unity3d.com/kr/current/Manual/class-NavMeshAgent.html
// https://docs.unity3d.com/kr/current/Manual/class-NavMeshObstacle.html
// https://docs.unity3d.com/kr/current/Manual/class-OffMeshLink.html

// https://docs.unity3d.com/kr/current/Manual/nav-AdvancedSettings.html
// https://docs.unity3d.com/kr/current/Manual/nav-HeightMesh.html