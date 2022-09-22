using CodeStage.AntiCheat.ObscuredTypes;
using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AntiCheatController : MonoBehaviour
{
	private ObscuredInt speedHackCount;
	private ObscuredInt timeHackCount;
	private ObscuredInt obscuredHackCount;
	private ObscuredBool customSpeedHackDetected = false;
	private ObscuredBool cheatEngineDetected = false;


	private void Start()
	{
		InvokeRepeating(nameof(SlowUpdate), 5f, 5f);
	}

	private void SlowUpdate()
	{
		try
		{
			if (!cheatEngineDetected && IsCheatEngineRunning())
			{
				OnCheatEngineDetected();
			}

			if (!customSpeedHackDetected && Mathf.Abs(Time.timeScale - 1f) > 0.001f && Time.timeScale != PengweevilController.PENG_DEATH_TIMESCALE)
			{
				OnCustomSpeedHackDetected();
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	public void OnObscuredHackDetected()
	{
		obscuredHackCount++;
		ReportCheat("obscured-hack");
	}

	public void OnSpeedHackDetected()
	{
		speedHackCount++;
		ReportCheat("speed-hack");
	}

	public void OnCustomSpeedHackDetected()
	{
		customSpeedHackDetected = true;
		JObject metadata = new JObject();
		try
		{
			metadata["custom_detection"] = true;
		}
		catch { }
		ReportCheat("speed-hack", metadata);
	}

	public void OnCheatEngineDetected()
	{
		cheatEngineDetected = true;
		JObject metadata = new JObject();
		try
		{
			System.Diagnostics.Process[] processCollection = System.Diagnostics.Process.GetProcesses();
			HashSet<string> prs = new HashSet<string>();
			foreach (System.Diagnostics.Process p in processCollection)
			{
				prs.Add(p.ProcessName);
			}
			var sorted = prs.ToArray();
			Array.Sort(sorted);
			metadata["processes"] = new JArray(sorted);
		}
		catch { }
		ReportCheat("cheat-engine", metadata);
	}

	private void ReportCheat(string cheatType, JObject metadata = null)
	{
		StartCoroutine(PostCheatReport(cheatType, metadata));
	}

	private IEnumerator PostCheatReport(string cheatType, JObject metadata)
	{
		JObject jsonObject = JObject.Parse(@"{
			'application': '$app',
			'version': '$ver',
			'platform': '$plat',
			'session': '$session',
			'device_id': '$device',
			'events': [{
				'timestamp': $ts,
				'category': 'cheat',
				'label': '$label',
				'value': '',
				'metadata': {}
			}]
		}".Replace("$ts", XUtils.Timestamp().ToString())
		.Replace("$label", cheatType)
		.Replace("$session", Launcher.sessionId)
		.Replace("$device", XUtils.GetDeviceId())
		.Replace("$plat", Application.platform.ToString().ToLower())
		.Replace("$app", Application.productName.ToLower())
		.Replace("$ver", Application.version.ToLower()));
		if (metadata != null)
		{
			jsonObject["events"][0]["metadata"] = metadata;
		}

		print(jsonObject.ToString());
		yield return WebRequestHelper.PostJsonRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/user-events/update", jsonObject.ToString(), true, true);
		yield return new WaitForSecondsRealtime(0.25f);

		if (speedHackCount > 0 || timeHackCount > 2 || obscuredHackCount > 1 || cheatEngineDetected || customSpeedHackDetected)
		{
			Debug.LogError("Failed to synchronize!");
			SceneManager.LoadScene("MainMenu");
		}
	}


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowTextLength(IntPtr hWnd);
	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
	public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	private static string GetWindowText(IntPtr hWnd)
	{
		int size = GetWindowTextLength(hWnd);
		if (size > 0)
		{
			var builder = new StringBuilder(size + 1);
			GetWindowText(hWnd, builder, builder.Capacity);
			return builder.ToString();
		}
		return String.Empty;
	}

	private static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
	{
		IntPtr found = IntPtr.Zero;
		List<IntPtr> windows = new List<IntPtr>();
		EnumWindows(delegate (IntPtr wnd, IntPtr param)
		{
			if (filter(wnd, param))
			{
				windows.Add(wnd);
			}
			return true;
		}, IntPtr.Zero);
		return windows;
	}

	private static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
	{
		return FindWindows(delegate (IntPtr wnd, IntPtr param)
		{
			return GetWindowText(wnd).Contains(titleText);
		});
	}

	public static bool IsCheatEngineRunning()
	{
		var ce = FindWindowsWithText("Cheat Engine");
		foreach (var win in ce)
		{
			return true;
		}
		return false;
	}
#else
    public static bool IsCheatEngineRunning()
    {
        //Not supported on this platform
        return false;
    }
#endif
}
