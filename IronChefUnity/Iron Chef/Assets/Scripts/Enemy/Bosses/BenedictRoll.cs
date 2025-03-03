using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenedictRoll : MonoBehaviour
{
    // Start is called before the first frame update

    public EnemyBasicAttackbox RollCollider;
    public KnockPlayerAway RollKnockbox;
    private BenedictBehavior behavior;
    public Collider collider;
    public Animator animator;
    EnemySpeedController speed;

    bool startingRoll = false;
    bool rolling = false;
    Vector3 targetFacing;

    bool recentTerrainHit = false;
    bool recentHit = false;

    public Transform RoomCenter;

    public float rollSpeed;

    PlayerCamControl cam;

    void Start()
    {
        behavior = GetComponent<BenedictBehavior>();
        speed = GetComponent<EnemySpeedController>();
        cam = FindObjectOfType<PlayerCamControl>();
        //animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(startingRoll)
        {
            if(animator.speed < 1)
            {
                animator.speed = Mathf.Clamp(animator.speed + (Time.deltaTime / 2), 0, 1);
            }
            if(animator.speed == 1)
            {
                LaunchRoll();
            }
        }
    }


    private void FixedUpdate()
    {
        if(rolling)
        {
            //Collision Check
            if(!recentHit)
            {
                var list = IronChefUtils.GetCastHits(collider, "Terrain");
                foreach (var i in list)
                {
                    if(i.gameObject.name != "Floor")
                    {
                        behavior.Laugh();

                        if (recentTerrainHit)
                        {
                            targetFacing = (RoomCenter.position - transform.position).normalized;
                            targetFacing.y = 0;
                            cam.ShakeCam(2.5f, 0.75f);
                        }
                        else
                        {
                            recentTerrainHit = true;
                            Invoke("UndoRecentTerrain", 0.5f);


                            targetFacing = FindObjectOfType<CharacterMover>().transform.position - transform.position;
                            targetFacing.y = 0;
                            cam.ShakeCam(2.5f, 0.75f);
                        }

                        recentHit = true;
                        Invoke("RecentHit", 0.25f);
                    }
                    
                }
            }
            

            //Phase Check
            var slist = IronChefUtils.GetCastHits(collider, "SpecialBossLayer1");
            if(slist.Count > 0)
            {
                behavior.GoToNextPhase();
            }


            //Move
            transform.position = Vector3.MoveTowards(transform.position, transform.position + (targetFacing * 1000), rollSpeed * speed.GetMod()  * Time.fixedDeltaTime);
            transform.LookAt(transform.position + targetFacing);
            cam.ShakeCam(0.75f, 0.75f);

            RollCollider.playersHit.Clear();
            RollKnockbox.playersHit.Clear();
        }
    }

    void UndoRecentTerrain()
    {
        recentTerrainHit = false;
    }
    void RecentHit()
    {
        recentHit = false;
    }

    private void LaunchRoll()
    {
        startingRoll = false;
        rolling = true;
        targetFacing = transform.forward;
        targetFacing.y = 0;
        recentTerrainHit = false;
        RollCollider.HitOn();
        RollKnockbox.HitOn();
        Physics.IgnoreCollision(collider, FindObjectOfType<CharacterController>().GetComponent<Collider>(), true);
        foreach(var c in FindObjectOfType<CharacterMover>().GetComponents<Collider>())
        {
            Physics.IgnoreCollision(collider, c, true);
        }
    }
    public void BeginRolling(float time)
    {
        animator.speed = 0;
        startingRoll = true;

        behavior.Laugh();

        SoundEffectSpawner.soundEffectSpawner.MakeSoundEffect(transform.position + ((FindObjectOfType<CharacterMover>().transform.position - transform.position)/2), SoundEffectSpawner.SoundEffect.EggRollStart);

        Invoke("EndRolling", time);
    }

    public void EndRolling()
    {
        RollCollider.HitOff();
        RollKnockbox.HitOff();
        rolling = false;
        behavior.DoneRolling = true;
        animator.SetBool("Roll", false);
        behavior.UndoPhaseDelay();
        Physics.IgnoreCollision(collider, FindObjectOfType<CharacterController>().GetComponent<Collider>(), false);
        foreach (var c in FindObjectOfType<CharacterMover>().GetComponents<Collider>())
        {
            Physics.IgnoreCollision(collider, c, false);
        }
    }
}
