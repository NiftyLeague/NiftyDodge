using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public Transform playerTransform;

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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Projectile"))
		{
			if (gameplayManager.hasGameEnded)
			{
				return;
			}

			Projectile hitProjectile = collision.GetComponent<Projectile>();

			if (hitProjectile.shieldPowerup)
			{
				gameplayManager.audioManager.PlaySound("PowerupGetShield");
				gameplayManager.SetShieldPowerup(true);
				hitProjectile.DestroyProjectile(false);
				return;
			}
			else if (hitProjectile.slowPowerup)
			{
				gameplayManager.audioManager.PlaySound("PowerupGetSlow");
				gameplayManager.SetSlowPowerup(true);
				hitProjectile.DestroyProjectile(false);
				return;
			}
			else
			{
				if (gameplayManager.shieldPowerupOnCharacter.activeInHierarchy)
				{
					gameplayManager.SetShieldPowerup(false);
					return;
				}

				hitProjectile.DestroyProjectile();
				gameplayManager.Lose();
				return;
			}

		}

		if (collision.CompareTag("Explosion"))
		{
			if (gameplayManager.hasGameEnded)
			{
				return;
			}

			if (gameplayManager.shieldPowerupOnCharacter.activeInHierarchy)
			{
				gameplayManager.SetShieldPowerup(false);
				return;
			}

			gameplayManager.Lose();
		}
	}
}
