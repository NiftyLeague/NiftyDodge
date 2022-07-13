using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public Rigidbody2D rigidBody;
	public float moveSpeed;
	public bool shieldPowerup;
	public bool slowPowerup;
	
	private BombLauncher launcherTarget;
	private Vector2 moveDirection;

	void Update()
	{
		if (GameplayManager.I.slowPowerupOnCharacter.activeInHierarchy)
		{
			rigidBody.velocity = moveDirection * GameplayManager.I.slowPowerupProjectileSpeed;
		}
		else
		{
			rigidBody.velocity = moveDirection * moveSpeed;
		}
		

		if (transform.position.x >= GameplayManager.I.worldRightLimit.position.x || transform.position.x <= GameplayManager.I.worldLeftLimit.position.x || transform.position.y >= GameplayManager.I.worldTopLimit.position.y || transform.position.y <= GameplayManager.I.worldBottomLimit.position.y)
		{
			DestroyProjectile(false);
		}
	}

	public void InitializeProjectile(float speedAmount, Vector2 moveDirection, BombLauncher launcherTarget)
	{
		this.launcherTarget = launcherTarget;
		this.moveDirection = moveDirection;

		if (IsAPowerup())
		{
			return;
		}

		moveSpeed += speedAmount;
		if (moveSpeed > 50)
		{
			moveSpeed = 50;
		}

		if (Random.value < 0.1f)
		{
			moveSpeed = 0.1f + (Random.value * 10);
		}
	}

	public void DestroyProjectile(bool withExplosion = true)
	{
		launcherTarget.ReceiveBomb();

		if (withExplosion)
		{
			launcherTarget.gameplayManager.Explosion(transform.position);
		}

		Destroy(gameObject);
	}

	bool IsAPowerup()
	{
		if (shieldPowerup || slowPowerup)
		{
			return true;
		}

		return false;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.transform.CompareTag("Projectile"))
		{
			if (IsAPowerup())
			{
				return;
			}

			collision.gameObject.GetComponent<Projectile>().DestroyProjectile();
			DestroyProjectile();
		}
	}
}
