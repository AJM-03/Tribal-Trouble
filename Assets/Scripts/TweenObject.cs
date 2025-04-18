using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenObject : MonoBehaviour
{
    public Transform startPointObject;
    public Transform endPointObject;
    private Vector3 startPoint;
    private Vector3 endPoint;
    public float travelTime;
    private Rigidbody rb;
    private Vector3 currentPos;

    CharacterController character;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPoint = startPointObject.position;
        endPoint = endPointObject.position;
    }

    void FixedUpdate()
    {
        currentPos = Vector3.Lerp(startPoint, endPoint,
            Mathf.Cos(Time.time / travelTime * Mathf.PI * 2) * -.5f + .5f);
        rb.MovePosition(currentPos);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            character = other.GetComponent<CharacterController>();
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
            character.Move(rb.velocity * Time.deltaTime);
    }







    /*public Vector3[] moveDistance;
    public float[] moveTime;
    public float[] moveDelay;

    private GameObject target;
    private Vector3 playerOffset;

    void Start()
    {
        target = null;

        if (moveDistance.Length == moveTime.Length && moveTime.Length == moveDelay.Length)
            Debug.LogError("Arrays on " + gameObject + " are not complete");

        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        for (int step = 0; step < moveDistance.Length; step++)
        {
            Vector3 thisMove = gameObject.transform.position + moveDistance[step];
            LeanTween.move(gameObject, thisMove, moveTime[step]);
            yield return new WaitForSeconds(moveTime[step] + moveDelay[step]);
        }

        StartCoroutine(Move());
    }







    void OnTriggerStay(Collider col)
    {
        //Debug.Log(col.gameObject);
        if (col.gameObject.GetComponent<PlayerMovement>())
        {
            target = col.gameObject;
            playerOffset = target.transform.position - transform.position;
        }
    }


    void OnTriggerExit(Collider col)
    {
        target.GetComponent<PlayerMovement>().movingPlatformOffset = new Vector3(0,0,0);
        target = null;
    }


    void LateUpdate()
    {
        if (target != null)
        {
            //target.GetComponent<CharacterController>().Move(-playerOffset * 5 * Time.deltaTime);
            target.GetComponent<PlayerMovement>().movingPlatformOffset = -playerOffset * 3;
            //Debug.Log(playerOffset);
        }
    }*/
}
