using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyJump : MonoBehaviour
{
    public EnemyBasicAttackbox jumpHitbox;
    public KnockPlayerAway jumpKnockbox;
    private Collider enemyCollider;
    [Tooltip("Float for how high the enemy will jump")]
    public float maxJumpHeight;
    [Tooltip("Float for how long it will take the enemy to jump")]
    public float time;
    [Tooltip("Bool for if the screen will shake upon landing")]
    public bool shake;
    [Tooltip("Bool that represents the hitbox, if it is off then the hitboxes will appear halfway through the jump")]
    public bool animHit = false;
    [Tooltip("Should Y value be adjusted in code or by animation?")]
    public bool adjustRealYValue = true;
    [Tooltip("Shoud the jump delay from firing until a function is called?")]
    public bool delay = false;
    [Tooltip("Bool representing if the enemy should jump a minimum distance when they jump.")]
    public bool jumpThrough = false;
    float storedTime;

    private NavMeshAgent agent;
    private bool hitOn = false;


    public int jumpVFXNum = -1;

    EnemyVFXController vfx;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
        vfx = GetComponent<EnemyVFXController>();
        if (vfx == null)
            vfx = GetComponentInParent<EnemyVFXController>();
        if (vfx == null)
            vfx = GetComponentInChildren<EnemyVFXController>();
    }

    public void BeginJumping(float time)
    {
        if(delay)
        {
            agent.enabled = false;
            storedTime = time;
        }
        else
        {
            Vector3 target = FindObjectOfType<CharacterMover>().transform.position;
            if (Vector3.Distance(transform.position, target) < 10)
            {
                target = transform.position + Vector3.Normalize(target - transform.position) * 10;
                target.y = FindObjectOfType<CharacterMover>().transform.position.y;
            }
            LaunchJump(target, time);
        }
    }

    public void BeginJumping(float time, Vector3 target)
    {
        if (delay)
        {
            agent.enabled = false;
            storedTime = time;
        }
        else
            LaunchJump(target, time);
    }

    public void BeginJumpingPostDelay()
    {
        transform.LookAt(FindObjectOfType<CharacterMover>().transform);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        Vector3 target = FindObjectOfType<CharacterMover>().transform.position;
        LaunchJump(target, storedTime);
    }

    private void LaunchJump(Vector3 target, float time)
    {
        Physics.IgnoreCollision(enemyCollider, FindObjectOfType<CharacterController>().GetComponent<Collider>(), true);
        foreach (var c in FindObjectOfType<CharacterMover>().GetComponents<Collider>())
        {
            Physics.IgnoreCollision(enemyCollider, c, true);
        }

        agent.enabled = false;

        StartCoroutine(jumpTick(target, time));
    }
    private IEnumerator jumpTick(Vector3 target, float time)
    {
        float cTime = 0;
        float yOffset = -4 * maxJumpHeight / Mathf.Pow(time, 2);
        Vector3 startPos = transform.position;
        while (cTime < time)
        {
            if (cTime >= (3 * time) / 4 && hitOn == false && animHit == false)
            {

                jumpHitbox.HitOn(); //Hello there
                jumpKnockbox.HitOn();
                hitOn = true;
            }

            transform.position = Vector3.Lerp(startPos, target, cTime / time);
            float yAmount = 0;
            if (adjustRealYValue)
            {
                yAmount = yOffset* cTime *(cTime - time);
            }
            transform.position = new Vector3(transform.position.x, transform.position.y + yAmount, transform.position.z);

            yield return null;
            cTime += Time.deltaTime;
        }
        transform.position = target;
        DoneJumping();

    }

    public void DoneJumping()
    {
        if (shake)
            FindObjectOfType<PlayerCamControl>().ShakeCam(5, 1.5f);

        vfx.StartEffect(jumpVFXNum);

        Physics.IgnoreCollision(enemyCollider, FindObjectOfType<CharacterController>().GetComponent<Collider>(), false);
        foreach (var c in FindObjectOfType<CharacterMover>().GetComponents<Collider>())
        {
            Physics.IgnoreCollision(enemyCollider, c, false);
        }

        agent.enabled = true;
        if (animHit == false)
        {
            jumpHitbox.HitOff();
            jumpKnockbox.HitOff();
            hitOn = false;
        }
    }
}
