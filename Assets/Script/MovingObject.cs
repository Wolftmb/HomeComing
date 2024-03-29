﻿using UnityEngine;
using System.Collections;

namespace Completed
{
	public abstract class MovingObject : MonoBehaviour
	{
		public float moveTime = 0.1f;	
		public LayerMask blockingLayer;
		
		
		private BoxCollider2D boxCollider; 	
		private Rigidbody2D rb2D;				
		private float inverseMoveTime;

        protected virtual void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            rb2D = GetComponent<Rigidbody2D>();
            inverseMoveTime = 1f / moveTime;
        }

        protected bool Move(int xDir, int yDir, out RaycastHit2D hit, bool checkAndMove)
        {
            Vector2 start = transform.position;
            Vector2 end = start + new Vector2(xDir, yDir);

            float startX = start.x;
            float endX = end.x;

            SpriteRenderer mySpriteRenderer = GetComponent<SpriteRenderer>();
            Player playerScript = GetComponent<Player>();
            Enemy enemyScript = GetComponent<Enemy>();

            if (playerScript != null && enemyScript == null)
            {
                if (endX < startX && mySpriteRenderer != null)
                {
                    mySpriteRenderer.flipX = true;
                }
                else if (endX > startX && mySpriteRenderer != null)
                {
                    mySpriteRenderer.flipX = false;
                }
            }
            else if (enemyScript != null && playerScript == null)
            {
                if (endX < startX && mySpriteRenderer != null)
                {
                    mySpriteRenderer.flipX = false;
                }
                else if (endX > startX && mySpriteRenderer != null)
                {
                    mySpriteRenderer.flipX = true;
                }
            }

            boxCollider.enabled = false;
            hit = Physics2D.Linecast(start, end, blockingLayer);
            boxCollider.enabled = true;

            if (hit.transform == null && checkAndMove)
            {
				StartCoroutine(SmoothMovement(end));

                return true;
            }
            return false;
        }

        protected IEnumerator SmoothMovement(Vector3 end)
        {
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            while (sqrRemainingDistance > float.Epsilon)
            {
                Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
                rb2D.MovePosition(newPosition);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }
        }


        protected virtual void AttemptMove<T>(int xDir, int yDir)
          where T : Component
        {
            RaycastHit2D hit;
            bool canMove = Move(xDir, yDir, out hit, true);

            if (hit.transform == null)
            {
                return;
            }

            T hitComponent = hit.transform.GetComponent <T> ();
            Enemy enemyComponent = hit.transform.GetComponent<Enemy>();
            Enemy myEnemyComponent = GetComponent<Enemy>();
            Player myComponent = GetComponent<Player>();

            if (!canMove && myComponent != null && enemyComponent != null)
            {
                EnemyAttack(enemyComponent);
            }
            if (!canMove && hitComponent != null) OnCantMove (hitComponent);
        }
		

		protected abstract void OnCantMove <T> (T component)
			where T : Component;
        protected abstract void EnemyAttack<T>(T component)
        where T : Component;
    }
}
