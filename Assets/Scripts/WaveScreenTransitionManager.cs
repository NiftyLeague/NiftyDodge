using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScreenTransitionManager : MonoBehaviour
{
    public GameplayManager gameplayManager;
    [Space]
    public GameObject transitionerCanvas;
    public SpriteRenderer background;
    public Transform surviveTheText;
    public Transform cryptoText;
    public Transform winterText;
    public Transform goText;
    public Transform youSuccumbedToText;
    public Transform theCryptoWinterText;
    public Transform skullIcon;
    public Transform bossText;
    public Transform fightText;
    [Space]
    public float transitionOffScreenX = 50;
    public float transitionOnScreenX = 0;
    private bool hasPlayedWaveStartBefore;

    private void Start()
    {
        ResetEverything();
    }

    private void ResetEverything()
    {
        transitionerCanvas.SetActive(false);
        background.color = new Color(1, 1, 1, 0);
    }

    public IEnumerator WaveStartTransition()
    {
        if (hasPlayedWaveStartBefore)
        {
            yield break;
        }

        gameplayManager.audioManager.PlayMusic(1);

        hasPlayedWaveStartBefore = true;

        ResetEverything();
        transitionerCanvas.SetActive(true);

        Tween<float> backgroundScreenAlpha = new Tween<float>(0, 0.75f, 0.5f, TweenEaseType.CubicOut);

        while (!backgroundScreenAlpha.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(1, 1, 1, backgroundScreenAlpha.Update(Time.deltaTime));
        }

        gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, 0);

        Tween<float> wordMove1 = new Tween<float>(transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove1.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            surviveTheText.position = new Vector2(wordMove1.Update(Time.deltaTime), 0);
        }

        gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, 0.2f);

        Tween<float> wordMove2 = new Tween<float>(-transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            cryptoText.position = new Vector2(wordMove2.Update(Time.deltaTime), 0);
        }

        gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, 0.4f);

        Tween<float> wordMove3 = new Tween<float>(transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove3.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            winterText.position = new Vector2(wordMove3.Update(Time.deltaTime), 0);
        }

        yield return new WaitForSeconds(1);

        Tween<float> wordMove4 = new Tween<float>(transitionOnScreenX, transitionOffScreenX, 0.2f, TweenEaseType.CubicOut);
        Tween<float> wordMove5 = new Tween<float>(transitionOnScreenX, -transitionOffScreenX, 0.2f, TweenEaseType.CubicOut);

        while (!wordMove4.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            surviveTheText.position = new Vector2(wordMove4.Update(Time.deltaTime), 0);
            cryptoText.position = new Vector2(wordMove5.Update(Time.deltaTime), 0);
            winterText.position = new Vector2(wordMove4.Update(Time.deltaTime), 0);
        }

        gameplayManager.audioManager.PlaySound("ScreenTransitionGo");

        Tween<float> wordMove6 = new Tween<float>(30, -30, 1.5f, TweenEaseType.Linear);

        while (!wordMove6.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            goText.position = new Vector2(wordMove6.Update(Time.deltaTime), 0);
        }

        Tween<float> backgroundScreenAlpha2 = new Tween<float>(0.75f, 0, 0.5f, TweenEaseType.CubicOut);

        while (!backgroundScreenAlpha2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(1, 1, 1, backgroundScreenAlpha2.Update(Time.deltaTime));
        }

        transitionerCanvas.SetActive(false);
    }

    public IEnumerator BossFightTransition()
    {
        ResetEverything();
        transitionerCanvas.SetActive(true);

        gameplayManager.audioManager.PlayMusic(2);

        Tween<float> backgroundScreenAlpha = new Tween<float>(0, 0.75f, 0.5f, TweenEaseType.CubicOut);

        while (!backgroundScreenAlpha.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(1, 1, 1, backgroundScreenAlpha.Update(Time.deltaTime));
        }

        gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, -0.2f);

        Tween<float> wordMove1 = new Tween<float>(transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove1.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            bossText.position = new Vector2(wordMove1.Update(Time.deltaTime), 0);
        }

        gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, -0.4f);

        Tween<float> wordMove2 = new Tween<float>(-transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            fightText.position = new Vector2(wordMove2.Update(Time.deltaTime), 0);
        }

        yield return new WaitForSeconds(1);

        gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, -0.6f);

        Tween<float> wordMove4 = new Tween<float>(transitionOnScreenX, transitionOffScreenX, 0.4f, TweenEaseType.CubicOut);
        Tween<float> wordMove5 = new Tween<float>(transitionOnScreenX, -transitionOffScreenX, 0.4f, TweenEaseType.CubicOut);

        while (!wordMove4.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            bossText.position = new Vector2(wordMove4.Update(Time.deltaTime), 0);
            fightText.position = new Vector2(wordMove5.Update(Time.deltaTime), 0);
        }

        Tween<float> backgroundScreenAlpha2 = new Tween<float>(0.75f, 0, 0.5f, TweenEaseType.CubicOut);

        while (!backgroundScreenAlpha2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(1, 1, 1, backgroundScreenAlpha2.Update(Time.deltaTime));
        }

        transitionerCanvas.SetActive(false);
    }

    public IEnumerator GameOverTransition()
    {
        ResetEverything();

        transitionerCanvas.SetActive(true);

        Tween<float> backgroundScreenAlpha = new Tween<float>(0, 0.75f, 0.5f, TweenEaseType.CubicOut);

        while (!backgroundScreenAlpha.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(1, 0, 0, backgroundScreenAlpha.Update(Time.deltaTime));
        }

        //gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, -0.4f);
        gameplayManager.audioManager.PlaySound("GameoverWhooshes");

        Tween<float> wordMove1 = new Tween<float>(transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove1.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            youSuccumbedToText.position = new Vector2(wordMove1.Update(Time.deltaTime), 0);
        }

        //gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, -0.6f);

        Tween<float> wordMove2 = new Tween<float>(-transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            theCryptoWinterText.position = new Vector2(wordMove2.Update(Time.deltaTime), 0);
        }

        //gameplayManager.audioManager.PlaySound("ScreenTransitionStart", 1, -0.8f);

        Tween<float> wordMove3 = new Tween<float>(transitionOffScreenX, transitionOnScreenX, 0.5f, TweenEaseType.CubicOut);

        while (!wordMove3.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            skullIcon.position = new Vector2(wordMove3.Update(Time.deltaTime), 0);
        }

        //gameplayManager.audioManager.PlaySound("ScreenTransitionGameOver", 1, -0.6f);

        yield return new WaitForSeconds(1);

        Tween<float> wordMove4 = new Tween<float>(0, -20, 2.0f, TweenEaseType.CubicIn);
        Tween<float> wordMove5 = new Tween<float>(0, -20, 1.6f, TweenEaseType.CubicIn);
        Tween<float> wordMove6 = new Tween<float>(0, -20, 0.8f, TweenEaseType.CubicIn);

        while (!wordMove4.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            youSuccumbedToText.position = new Vector2(0, wordMove4.Update(Time.deltaTime));
            theCryptoWinterText.position = new Vector2(0, wordMove5.Update(Time.deltaTime));
            skullIcon.position = new Vector2(0, wordMove6.Update(Time.deltaTime));
        }

        Tween<float> backgroundScreenAlpha2 = new Tween<float>(0.75f, 0, 0.5f, TweenEaseType.CubicOut);

        while (!backgroundScreenAlpha2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(1, 1, 1, backgroundScreenAlpha2.Update(Time.deltaTime));
        }

        transitionerCanvas.SetActive(false);

        gameplayManager.EndGame();
    }
}
