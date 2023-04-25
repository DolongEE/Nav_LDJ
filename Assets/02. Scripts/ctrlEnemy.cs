using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Rand = UnityEngine.Random;

[System.Serializable]
public class Anim
{
    public AnimationClip idle1;
    public AnimationClip idle2;
    public AnimationClip idle3;
    public AnimationClip idle4;
    public AnimationClip move;
    public AnimationClip surprise;
    public AnimationClip attack1;
    public AnimationClip attack2;
    public AnimationClip attack3;
    public AnimationClip attack4;
    public AnimationClip hit1;
    public AnimationClip hit2;
    public AnimationClip eat;
    public AnimationClip sleep;
    public AnimationClip die;
}

[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("ctrlEnemy/Follow EnemyCtrl")]
public class ctrlEnemy : MonoBehaviour
{
    //[Multiline(7)]          // 전달사항 있을때 사용
    //public string Ex = "안녕하세요\n 이 컴포넌트는...";

    //[TextArea(7, 11)]       // 메모장 용도정도로 사용
    //public string memo = "";

    [Header("ANIMATION")]
    //[Space(30)]
    public Anim anims;

    private Animation _anim;
    AnimationState animState;
    private float randAnimTime;
    private int randAnim;

    private Rigidbody rigid;
    private NavMeshAgent myTraceAgent;

    private Transform myTr;
    private Transform traceTarget;

    private bool traceObject;
    private bool traceAttack;

    private float hungryTime;
    private float nonHungryTime;

    float dist1;
    float dist2;

    private GameObject[] players;
    private Transform playerTarget;

    private GameObject[] baseAll;
    private Transform baseTarget;

    private Transform[] roamingCheckPoints;
    private int roamingRandcheckPos;
    private Transform roamingTarget;

    [HideInInspector]
    public bool isDie;

    public enum MODE_STATE { IDLE = 1, MOVE, SURPRISE, TRACE, ATTACK, HIT, EAT, SLEEP, DIE };
    public enum MODE_KIND { ENEMY1 = 1, ENEMY2, ENEMYBOSS };

    [Header("STATE")]
    [Space(10)]
    public MODE_STATE enemyMode = MODE_STATE.IDLE;
    [Header("SETTING")]
    public MODE_KIND enemyKind = MODE_KIND.ENEMY1;

    [Header("몬스터 인공지능")]
    [Space(10)]
    [Tooltip("몬스터의 HP")]
    [Range(0, 1000)] public int hp = 100;

    [Tooltip("몬스터의 속도")]
    [Range(1f, 10f)] public float speed = 5f;

    [Tooltip("몬스터의 발견거리")]
    [Range(10f, 40f)] [SerializeField] public float findDist = 20f;
    [Tooltip("몬스터의 추적거리")]
    [Range(10f, 40f)] [SerializeField] public float traceDist = 15f;
    [Tooltip("몬스터의 공격거리")]
    [Range(1f, 10f)] [SerializeField] public float attackDist = 2f;
    [Tooltip("몬스터의 로밍 시간")]
    [Range(1f, 30f)] [SerializeField] public float hungryTimeSet = 8f;
    [Tooltip("몬스터의 로밍 대기시간")]
    [Range(1f, 30f)] [SerializeField] public float nonHungryTimeSet = 8f;

    [Header("TEST")]
    [SerializeField] private bool isHit;
    [SerializeField] private bool hungry;
    [SerializeField] private bool sleep;
    private bool nonHungry;
    private float isHitTime;
    void Awake()
    {
        myTraceAgent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animation>();
        myTr = GetComponent<Transform>();
        baseAll = GameObject.FindGameObjectsWithTag("Base");
        roamingCheckPoints = GameObject.Find("RoamingPoint").GetComponentsInChildren<Transform>();
        rigid = GetComponent<Rigidbody>();
    }
    IEnumerator Start()
    {
#if CBT_MODE
        hp = 1000;
#elif RELEASE_MODE
        hp = 100;
#endif
        _anim.clip = anims.idle1;
        _anim.Play();

        traceTarget = baseAll[0].transform;
        myTraceAgent.SetDestination(traceTarget.position);

        StartCoroutine(ModeSet());
        StartCoroutine(ModeAction());
        StartCoroutine(this.TargetSetting());

        RoamingCheckStart();

        yield return null;
    }


    void Update()
    {
        if(Time.time > randAnimTime)
        {
            randAnim = Rand.Range(0, 4);
            randAnimTime = Time.time + 5.0f;
        }
        if(!hungry)
        {
            if(Time.time > hungryTime)
            {
                RoamingCheckStart();
                hungry = true;
                nonHungryTime = Time.time + nonHungryTimeSet + Rand.Range(3f, 7f);
                nonHungry = true;
            }
        }
        if(nonHungry)
        {
            if (Time.time > nonHungryTime)
            {
                nonHungry = false;
                hungryTime = Time.time + hungryTimeSet + Rand.Range(3f, 7f);
                hungry = false;
            }
        }
        if(isHit)
        {
            if (Time.time > isHitTime)
            {
                isHit = false;
            }
        }       
    }
    IEnumerator ModeSet()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);
            float dist = Vector3.Distance(myTr.position, traceTarget.position);

            if (isHit)
            {
                enemyMode = MODE_STATE.HIT;
            }
            else if (dist <= attackDist)
            {
                enemyMode = MODE_STATE.ATTACK;
            }
            else if (traceAttack)
            {
                enemyMode = MODE_STATE.TRACE;
            }
            else if (dist <= traceDist)
            {
                enemyMode = MODE_STATE.TRACE;
            }
            else if (hungry)
            {
                enemyMode = MODE_STATE.MOVE;
            }
            else if (sleep)
            {
                enemyMode = MODE_STATE.SLEEP;
            }
            else
            {
                enemyMode = MODE_STATE.IDLE;
            }
        }
    }

    IEnumerator ModeAction()
    {
        while(!isDie)
        {
            switch(enemyMode)
            {
                case MODE_STATE.IDLE:
                    myTraceAgent.isStopped = true;
                    if(randAnim == 0)
                    {
                        _anim.CrossFade(anims.idle1.name, 0.3f);
                    }
                    else if (randAnim == 1)
                    {
                        _anim.CrossFade(anims.idle2.name, 0.3f);
                    }
                    else if (randAnim == 2)
                    {
                        _anim.CrossFade(anims.idle3.name, 0.3f);
                    }
                    else if (randAnim == 3)
                    {
                        _anim.CrossFade(anims.idle4.name, 0.3f);
                    }
                    break;
                case MODE_STATE.TRACE:
                    myTraceAgent.isStopped = false;
                    myTraceAgent.destination = traceTarget.position;
                    if (enemyKind == MODE_KIND.ENEMYBOSS)
                    {
                        myTraceAgent.speed = speed * 1.8f;
                        _anim[anims.move.name].speed = 1.8f;
                        _anim.CrossFade(anims.move.name, 0.3f);
                    }
                    else
                    {
                        myTraceAgent.speed = speed * 1.5f;
                        _anim[anims.move.name].speed = 1.5f;
                        _anim.CrossFade(anims.move.name, 0.3f);
                    }
                    break;
                case MODE_STATE.ATTACK:
                    myTraceAgent.isStopped = true;
                    // myTr.LookAt(traceTarget.position); // 바로 바라봄
                    Quaternion enemyLookRotation = Quaternion.LookRotation(traceTarget.position - myTr.position);
                    myTr.rotation = Quaternion.Lerp(myTr.rotation, enemyLookRotation, Time.deltaTime * 10.0f);

                    if (randAnim == 0)
                    {
                        _anim.CrossFade(anims.attack1.name, 0.3f);
                    }
                    else if (randAnim == 1)
                    {
                        _anim.CrossFade(anims.attack2.name, 0.3f);
                    }
                    else if (randAnim == 2)
                    {
                        _anim.CrossFade(anims.attack3.name, 0.3f);
                    }
                    else if (randAnim == 3)
                    {
                        _anim.CrossFade(anims.attack4.name, 0.3f);
                    }
                    break;
                case MODE_STATE.MOVE:
                    myTraceAgent.isStopped = false;
                    myTraceAgent.destination = roamingTarget.position;
                    if(enemyKind == MODE_KIND.ENEMYBOSS)
                    {
                        myTraceAgent.speed = speed * 1.2f;
                        _anim[anims.move.name].speed = 1.2f;
                        _anim.CrossFade(anims.move.name, 0.3f);
                    }
                    else
                    {
                        myTraceAgent.speed = speed;
                        _anim[anims.move.name].speed = 1.0f;
                        _anim.CrossFade(anims.move.name, 0.3f);
                    }
                    break;
                case MODE_STATE.SURPRISE:
                    if(!traceObject)
                    {
                        traceObject = true;
                        myTraceAgent.isStopped = true;
                        _anim.CrossFade(anims.surprise.name, 0.3f);
                        StartCoroutine(this.TraceObject());
                    }
                    break;
                case MODE_STATE.SLEEP:
                    myTraceAgent.isStopped = true;
                    _anim.CrossFade(anims.sleep.name, 0.3f);
                    break;
                case MODE_STATE.HIT:
                    myTraceAgent.isStopped = true;
                    if (randAnim == 0 || randAnim == 1)
                    {
                        _anim.CrossFade(anims.hit1.name, 0.3f);
                    }
                    else if (randAnim == 1 || randAnim == 2) 
                    {
                        _anim.CrossFade(anims.hit2.name, 0.3f);
                    }
                    break;
            }
            yield return null;
        }
    }

    IEnumerator TraceObject()
    {
        yield return new WaitForSeconds(1.5f);
        traceAttack = true;
        yield return new WaitForSeconds(3.5f);
        traceAttack = false;
        traceObject = false;
    }

    IEnumerator TargetSetting()
    {
        while(!isDie)
        {
            yield return new WaitForSeconds(0.2f);
            players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length != 0)
            {
                playerTarget = players[0].transform;
                dist1 = (playerTarget.position - myTr.position).sqrMagnitude;
                foreach (GameObject _players in players)
                {
                    if((_players.transform.position - myTr.position).sqrMagnitude < dist1)
                    {
                        playerTarget = _players.transform;
                        dist1 = (playerTarget.position - myTr.position).sqrMagnitude;
                    }
                }
            }
            baseAll = GameObject.FindGameObjectsWithTag("Base");
            baseTarget = baseAll[0].transform;
            dist2 = (baseTarget.position - myTr.position).sqrMagnitude;
            foreach (GameObject _baseAll in baseAll)
            {
                if ((_baseAll.transform.position - myTr.position).sqrMagnitude < dist2)
                {
                    playerTarget = _baseAll.transform;
                    dist2 = (playerTarget.position - myTr.position).sqrMagnitude;
                }
            }
            if (players.Length == 0)
            {
                traceTarget = baseTarget;
            }
            else
            {
                if (dist1 <= dist2)
                {
                    traceTarget = playerTarget;
                }
                else
                {
                    traceTarget = baseTarget;
                }
            }

        }
    }

    public void RoamingCheckStart()
    {
        StartCoroutine(this.RoamingCheck(roamingRandcheckPos));
    }

    IEnumerator RoamingCheck(int pos)
    {
        roamingRandcheckPos = Rand.Range(1, roamingCheckPoints.Length);
        if(roamingRandcheckPos == pos)
        {
            RoamingCheckStart();
            yield break;
        }
        roamingTarget = roamingCheckPoints[roamingRandcheckPos];
    }

    public void EnemyDie()
    {
        StartCoroutine(this.Die());
    }

    IEnumerator Die()
    {
        isDie = true;
        _anim.CrossFade(anims.die.name, 0.3f);
        enemyMode = MODE_STATE.DIE;
        this.gameObject.tag = "Untagged";
        this.gameObject.transform.Find("EnemyBody").tag = "Untagged";
        myTraceAgent.isStopped = true;

        foreach (Collider coll in gameObject.GetComponentsInChildren<Collider>())
        {
            coll.enabled = false;
        }
        yield return new WaitForSeconds(4.5f);
        Destroy(gameObject);
    }

    public void EnemyBarrelDie(Vector3 firePos)
    {
        StartCoroutine(this.BarrelDie(firePos));
    }

    IEnumerator BarrelDie(Vector3 firePos)
    {
        isDie = true;
        _anim.CrossFade(anims.die.name, 0.3f);
        enemyMode = MODE_STATE.DIE;
        this.gameObject.tag = "Untagged";
        this.gameObject.transform.Find("EnemyBody").tag = "Untagged";
        myTraceAgent.enabled = false;

        rigid.mass = 1.0f;
        rigid.AddExplosionForce(1000.0f, firePos, 5.0f, 100.0f);
        yield return new WaitForSeconds(3.5f);
        rigid.isKinematic = true;

        foreach (Collider coll in gameObject.GetComponentsInChildren<Collider>())
        {
            coll.enabled = false;
        }

        yield return new WaitForSeconds(4.5f);
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void HitEnemy()
    {
        int rand = Rand.Range(0, 100);
        if (rand < 30)
        {
            if (randAnim == 0 || randAnim == 1)
            {
                isHitTime = Time.time + anims.hit1.length + 0.2f;
                isHit = true;
            }
            else if (randAnim == 1 || randAnim == 2)
            {
                isHitTime = Time.time + anims.hit1.length + 0.2f;
                isHit = true;
            }
        }
    }

    [ContextMenu("Func Start")]
    void FuncStart()
    {
        Debug.Log("Func Start");
        Debug.Log(speed);
    }
}
