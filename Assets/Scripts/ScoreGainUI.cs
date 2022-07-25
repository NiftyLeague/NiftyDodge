using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreGainUI : MonoBehaviour
{
    public TextMeshProUGUI scoreGainedText;
    public List<Color32> randomColors;

    private float colorTimer;
    private int lastColor = -1;

    private void Update()
    {
        colorTimer += Time.deltaTime;
        if (colorTimer >= 0.1f)
        {
            int nextColor = Random.Range(0, randomColors.Count);
            if (lastColor == nextColor) 
            {
                nextColor++;
                nextColor = Mathf.Clamp(nextColor, 0, randomColors.Count-1);
            }

            lastColor = nextColor;

            scoreGainedText.color = randomColors[nextColor];

            colorTimer = 0;
        }
    }

    public void Initialize(int scoreGainedAmount)
    {
        transform.localPosition = new Vector2(Random.Range(-1.2f, 1.2f), -2 + Random.Range(-1.5f, 1.5f));
        scoreGainedText.text = "+" + scoreGainedAmount.ToString("0");
		StartCoroutine(AnimateScoreGained());
    }

    IEnumerator AnimateScoreGained()
    {
		float a = 0;
		float b = 0.1f;

		Tween<float> scalePointMessageTween = new Tween<float>(a, b, 0.5f, TweenEaseType.CubicIn);

		while (!scalePointMessageTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			scoreGainedText.transform.localScale = new Vector3(0.1f, scalePointMessageTween.Update(Time.deltaTime), 0.1f);
		}

		yield return new WaitForSeconds(1.0f);

        Destroy(gameObject);
	}
}
