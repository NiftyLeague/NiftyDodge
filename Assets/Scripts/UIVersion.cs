using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVersion : MonoBehaviour
{
	public Text version;
	public Text session;


	void Start()
	{
		version.text = $"v{Application.version}";
		var sesstionId = EventController.GetMatchShortId();
		session.text = string.IsNullOrEmpty(sesstionId) ? Launcher.shortSessionId : sesstionId;
	}

	public static void SetSession(string session)
	{
		if (string.IsNullOrEmpty(session))
		{
			return;
		}
		var ui = FindObjectOfType<UIVersion>();
		if (ui)
		{
			ui.session.text = session;
		}
	}
}
