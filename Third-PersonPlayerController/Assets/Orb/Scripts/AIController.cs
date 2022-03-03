using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public GameObject explosion;
    public GameObject target;
    public GameObject bullet;

    NavMeshAgent agent;
    enum STATE { IDLE, WANDER, ATTACK, CHASE, DEAD };
    STATE state = STATE.WANDER;

    float DistanceToPlayer()
    {
        if (target.GetComponent<PlayerController>().isDead) return Mathf.Infinity;
        return Vector3.Distance(target.transform.position, this.transform.position);
    }

    bool CanSeePlayer()
    {
        Debug.Log("Distance: " + DistanceToPlayer());
        if (DistanceToPlayer() < 10)
            return true;
        return false;
    }

    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > 20)
            return true;
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.baseOffset = Random.Range(0.01f, 0.08f);
        target = GameObject.FindGameObjectWithTag("Player");
    }

    public void BlowUp()
    {
        GameObject explode = Instantiate(explosion, this.transform.position + new Vector3(0,agent.baseOffset,0), explosion.transform.rotation);
        Destroy(explode, 2);
        Destroy(this.gameObject);
    }

    bool canShoot = true;
    void Shoot()
    {
        if (!canShoot) return;
        GameObject bulletObj = Instantiate(bullet, this.transform.position + this.transform.forward, this.transform.rotation);
        bulletObj.GetComponent<Rigidbody>().AddForce(this.transform.forward * 1000);
        canShoot = false;
        Invoke("CanShoot", 1.5f);
    }

    void CanShoot()
    {
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case STATE.IDLE:
                if (CanSeePlayer()) state = STATE.CHASE;
                else if (Random.Range(0, 100) < 5)
                    state = STATE.WANDER;
                break;
            case STATE.WANDER:
                if (!agent.hasPath)
                {
                    float newX = this.transform.position.x + Random.Range(-50, 50);
                    float newZ = this.transform.position.z + Random.Range(-50, 50);
                    NavMeshHit hit;
                    Vector3 randomPoint = new Vector3(newX, 0, newZ);
                    if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        Vector3 dest = hit.position;
                        agent.SetDestination(dest);
                        agent.stoppingDistance = 0;
                    }
                    
                }
                if (CanSeePlayer()) state = STATE.CHASE;
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.IDLE;
                    agent.ResetPath();
                }
                break;
            case STATE.CHASE:
                if (target.GetComponent<PlayerController>().isDead) { state = STATE.WANDER; return; }
                agent.SetDestination(target.transform.position);
                agent.stoppingDistance = 5;

                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    state = STATE.ATTACK;
                }

                if (ForgetPlayer())
                {
                    state = STATE.WANDER;
                    agent.ResetPath();
                }

                break;
            case STATE.ATTACK:
                if (target.GetComponent<PlayerController>().isDead) { state = STATE.WANDER; return; }
                this.transform.LookAt(target.transform.position + Vector3.up);
                Shoot();
                if (DistanceToPlayer() > agent.stoppingDistance + 2)
                    state = STATE.CHASE;
                break;
        }

    }


}
