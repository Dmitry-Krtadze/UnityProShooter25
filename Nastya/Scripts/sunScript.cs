using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunScript : MonoBehaviour
{
    public GameObject missle;
    private Transform target;
    public float speed = 5f;

    public bool isChasingPlayer = false; // <-- флаг, управляется извне

    public void SpawnPrikol()
    {
        Instantiate(missle, transform.position, Quaternion.identity);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        isChasingPlayer = true;
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }

    void Update()
    {
        if (target != null && isChasingPlayer)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("allo check");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("allo check");
            transform.position = new Vector3(54.7999992f, 73.6800003f, -40.5999985f);
        }
    }
}
