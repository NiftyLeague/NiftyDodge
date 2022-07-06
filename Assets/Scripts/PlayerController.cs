using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	[Space]
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
			audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
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
			hitProjectile.DestroyProjectile();
			gameplayManager.Lose();
		}

		if (collision.CompareTag("Explosion"))
		{
			if (gameplayManager.hasGameEnded)
			{
				return;
			}

			gameplayManager.Lose();
		}
	}
}
