using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public Transform playerTransform;
	public SpriteRenderer playerSpriteRenderer;

	private bool canGetHit = true;
	private InputState input = new InputState();

	private void Start()
	{
		PlayerSpriteManager.I.SetCharacterSprites();
	}

	void FixedUpdate()
	{
		if (gameplayManager.hasGameEnded)
		{
			return;
		}

		InputReader.GetInput(input);

		if (input.PressedY)
		{
			if (!PlayerSpriteManager.I.CanChangeCharacters())
			{
				return;
			}
			gameplayManager.audioManager.PlaySound("MenuOptionSelect");
			PlayerSpriteManager.I.ChangeCharacter();
		}
	}

	IEnumerator HurtFlash()
	{
		canGetHit = false;

		for (int i = 0; i < 30; i++)
		{
			playerSpriteRenderer.gameObject.SetActive(!playerSpriteRenderer.gameObject.activeInHierarchy);
			yield return new WaitForSeconds(0.05f);
		}

		playerSpriteRenderer.gameObject.SetActive(true);

		canGetHit = true;
	}

	public void AnimateHurtFlash()
	{
		StartCoroutine(HurtFlash());
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Projectile"))
		{
			if (gameplayManager.hasGameEnded)
			{
				return;
			}

			Projectile hitProjectile = collision.GetComponent<Projectile>();

			if (hitProjectile.powerupType != PowerupType.None)
			{
				gameplayManager.EnablePowerup(hitProjectile.powerupType);
				hitProjectile.DestroyProjectile(false);
				return;
			}
			else
			{
				if (!canGetHit)
				{
					return;
				}

				if (gameplayManager.invincibilityPowerupOnCharacter.gameObject.activeInHierarchy)
				{
					return;
				}

				gameplayManager.LoseLife();
				return;
			}

		}

		//if (collision.CompareTag("Explosion"))
		//{
		//	if (gameplayManager.hasGameEnded)
		//	{
		//		return;
		//	}

		//	if (gameplayManager.shieldPowerupOnCharacter.activeInHierarchy)
		//	{
		//		gameplayManager.SetShieldPowerup(false);
		//		return;
		//	}

		//	gameplayManager.Lose();
		//}
	}
}
