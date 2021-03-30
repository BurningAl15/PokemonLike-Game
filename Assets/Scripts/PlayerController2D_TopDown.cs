using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController2D_TopDown : MonoBehaviour
{
    public float moveSpeed;
    public bool isMoving;
    private Vector2 input;

    private Coroutine currentCoroutine = null;

    [SerializeField] private Animator anim;

    [SerializeField] private float collisionRadius;
    [SerializeField] private LayerMask collisionMask;

    [SerializeField] private LayerMask battleMask;

    private Transform playerPos;
    
    void Start()
    {
        playerPos = transform;
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //No Diagonal Movement
            if (input.x != 0) input.y = 0;
            
            if (input != Vector2.zero)
            {
                anim.SetFloat("moveX",input.x);
                anim.SetFloat("moveY",input.y);
                
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                // Debug.DrawLine(playerPos.position,targetPos,Color.green);
                
                if (isWalkable(targetPos))
                {
                    if (currentCoroutine == null)
                    {
                        currentCoroutine = StartCoroutine(Move(targetPos));
                    }
                }
            }
        }
        anim.SetBool("isMoving",isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        
        while ((targetPos-playerPos.position).sqrMagnitude>Mathf.Epsilon)
        {
            playerPos.position = Vector3.MoveTowards(playerPos.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        playerPos.position = targetPos;

        isMoving = false;
        CheckForEncounters();
        currentCoroutine = null;
    }

    bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, collisionRadius, collisionMask) != null)
        {
            return false;
        }

        return true;
    }

    void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(playerPos.position, collisionRadius, battleMask) != null)
        {
            if (Random.Range(1, 101) <= 10)
            {
                ScenaryType._instance.GameState_Battle();
                Debug.Log("Encountered a wild pokemon");
            }
        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = new Color(1, 0, 0);
    //     Gizmos.DrawSphere(transform.position, collisionRadius);
    // }
}
