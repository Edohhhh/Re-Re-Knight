using UnityEngine;
using System.Collections;

public class MiniSlimeController : MonoBehaviour, IEnemyDataProvider/*, IStunnable*/
{
    public Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public float detectionRadius = 2f;
    public float attackDistance = 0.8f;
    public float maxHealth = 50f;
    public float damage = 10f;
    public float maxSpeed = 2f;
    public float acceleration = 4f;
    public GameObject miniSlimePrefab;

    //private bool isStunned = false;
    //public void EndStun() => isStunned = false;

    private bool alreadyUnregistered = false;

    private FSM<EnemyInputs> fsm;
    private HealthSystem health;

    private void Start()
    {
        Debug.Log("MiniSlimeController Start - registrando enemigo");
        EnemyManager.Instance.RegisterEnemy();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("MiniSlime no encontró al jugador con el tag 'Player'");
            }
        }

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<HealthSystem>();
        health.OnDamaged += HandleStun;

        EnemyIdleState idle = new EnemyIdleState(transform);
        EnemyAttackState attack = new EnemyAttackState(transform);
        MiniSlimeDeathState death = new MiniSlimeDeathState(this);
        //EnemyStunState stun = new EnemyStunState(transform);

        idle.AddTransition(EnemyInputs.SeePlayer, attack);
        attack.AddTransition(EnemyInputs.LostPlayer, idle);

        idle.AddTransition(EnemyInputs.Die, death);
        attack.AddTransition(EnemyInputs.Die, death);

        //idle.AddTransition(EnemyInputs.Stun, stun);
        //attack.AddTransition(EnemyInputs.Stun, stun);

        //stun.AddTransition(EnemyInputs.SeePlayer, attack);
        //stun.AddTransition(EnemyInputs.LostPlayer, idle);

        fsm = new FSM<EnemyInputs>(idle);

    }

    private void Update()
    {
        fsm.Update();

        //if (!isStunned)
        //{
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= detectionRadius)
                Transition(EnemyInputs.SeePlayer);
            else
                Transition(EnemyInputs.LostPlayer);
        //}

        animator.SetBool("isWalking", fsm.GetCurrentState() is EnemyAttackState);

        if (fsm.GetCurrentState() is EnemyAttackState && player != null)
        {
            Vector2 direction = player.position - transform.position;
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    public void Transition(EnemyInputs input)
    {
        //if (input == EnemyInputs.Stun) ;
        ////isStunned = true;
        //else if (input == EnemyInputs.SeePlayer || input == EnemyInputs.LostPlayer) ;
        //    //isStunned = false;

        fsm.Transition(input);
    }

    public void Die()
    {
        if (alreadyUnregistered) return; // Evita doble ejecución

        alreadyUnregistered = true;

        Vector3 pos = transform.position;

        Instantiate(miniSlimePrefab, pos + new Vector3(1.5f, 1.5f, 0), Quaternion.identity);
        //Instantiate(miniSlimePrefab, pos + new Vector3(-1.5f, 1.5f, 0), Quaternion.identity);
        //Instantiate(miniSlimePrefab, pos + new Vector3(1.5f, -1.5f, 0), Quaternion.identity);
        Instantiate(miniSlimePrefab, pos + new Vector3(-1.5f, -1.5f, 0), Quaternion.identity);

        StartCoroutine(DelayedDeath());
        //StartCoroutine(UnregisterAfterFrame());
        //Destroy(gameObject);
    }

    //private IEnumerator UnregisterAfterFrame()
    //{
    //    yield return new WaitForEndOfFrame();  // Espera a que los hijos se registren
    //    EnemyManager.Instance.UnregisterEnemy();
    //}

    private IEnumerator DelayedDeath()
    {
        yield return new WaitForEndOfFrame();
        EnemyManager.Instance.UnregisterEnemy();
        Destroy(gameObject);
    }

    private void HandleStun()
    {
        Transition(EnemyInputs.Stun);
    }

    public float GetCurrentHealth() => health.GetCurrentHealth();
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => attackDistance;
    public float GetDamage() => damage;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;

    //public bool IsStunned() => isStunned;
}