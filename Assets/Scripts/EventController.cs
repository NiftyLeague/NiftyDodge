using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class EventController : MonoBehaviour
{
	private static EventController I;
	private static string url = "https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game/update";

	public float updateInterval;

	private static Match match;
	private static string latestMatchId;

	private void Awake()
	{
		if (I != null && I != this && I.gameObject != null)
		{
			DestroyImmediate(gameObject);
			return;
		}
		I = this;
		if (I)
		{
			DontDestroyOnLoad(gameObject);
		}

		StartCoroutine(UpdateEvents());
	}


	IEnumerator UpdateEvents()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(updateInterval);
			yield return UploadEvents();
		}
	}


	private static IEnumerator UploadEvents()
	{
		if (match == null || string.IsNullOrEmpty(match.id))
		{
			yield break;
		}
		string token = NiftyUsers.GetMyAuthorization();
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", token },
			{ "NL-Encryption", "1.1" },
		};
		string body = JsonUtility.ToJson(match);

		CleanUpMatch();
		yield return Utils.PostRequest(url, Encoding.ASCII.GetBytes(XUtils.ToXorBase64(body, token)), (w) => www = w, headers);
		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			yield break;
		}
		else if (match != null)
		{
			match.SetUpdateTicket(int.Parse(www.downloadHandler.text));
		}
	}

	public static bool IsMatchInProgress()
	{
		return match != null;
	}

	public static string AddMatchStart(string matchId, int updateTicket)
	{
		try
		{
			match = new Match(matchId, updateTicket);
			AddEvent(new Event(EventTypes.MatchStart, null));
			print($"{match.shortId} - {match.id}");
			latestMatchId = matchId;
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Failed to add ({EventTypes.MatchStart}) event {e}");
		}
		return matchId;
	}

	public static void AddMatchEnd(int degenId)
	{
		try
		{
			AddEvent(new Event(EventTypes.MatchEnd, degenId.ToString()));
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Failed to add ({EventTypes.MatchEnd}) event {e}");
		}
	}

	public static void AddScore(int score)
	{
		try
		{
			AddEvent(new Event(EventTypes.Score, score.ToString()));
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Failed to add ({EventTypes.Score}) event {e}");
		}
	}

	public static void AddDodgeBall()
	{
		try
		{
			AddEvent(new Event(EventTypes.DodgeBall, null));
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Failed to add ({EventTypes.DodgeBall}) event {e}");
		}
	}

	public static void AddDodgeBomb()
	{
		try
		{
			AddEvent(new Event(EventTypes.DodgeBomb, null));
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Failed to add ({EventTypes.DodgeBomb}) event {e}");
		}
	}

	private static void CleanUpMatch()
	{
		if (match.events.Any(e => e.type == EventTypes.MatchEnd))
		{
			latestMatchId = match.id;
			match.ClearEvents();
			match = null;
		}
		else
		{
			match.ClearEvents();
		}
	}

	private static void AddEvent(Event e)
	{
		if (match != null)
		{
			match.AddEvent(e);

			if (e.type == EventTypes.MatchEnd)
			{
				I.StartCoroutine(UploadEvents());
			}
		}
		else
		{
			print($"No match to add event '{e.type}'");
		}
	}

	public static string GetMatchShortId()
	{
		if (match != null)
		{
			return match.shortId;
		}
		return null;
	}

	public static string GetLastestMatchId()
	{
		return latestMatchId;
	}

	[Serializable]
	private class Match
	{
		public string id;
		public string shortId;
		public List<Event> events;
		public int updateTicket;

		public Match(string id, int updateTicket)
		{
			this.id = id;
			this.updateTicket = updateTicket;
			shortId = XUtils.GetInt32HashCode(id).ToString("X6");
			events = new List<Event>();
		}

		public void AddEvent(Event e)
		{
			events.Add(e);
		}

		public void ClearEvents()
		{
			events.Clear();
		}

		internal void SetUpdateTicket(int updateTicket)
		{
			this.updateTicket = updateTicket;
		}
	}


	[Serializable]
	private class Event
	{
		public string id;
		public string type;
		public int ts;
		public string value;

		public Event(string type, string value)
		{
			// print($"{type}-{id}-{Mathf.FloorToInt(PhotonNetwork.ServerTimestamp / 100000)}");
			ts = (int)XUtils.Timestamp();
			this.id = GenerateIdHash($"{type}-{ts}");
			this.type = type;
			this.value = value;
		}

		private static string GenerateIdHash(string e)
		{
			return Utils.GetMD5Hash(e).Substring(0, 6);
		}
	}

	private static class EventTypes
	{
		public static ObscuredString MatchStart = "start";
		public static ObscuredString MatchEnd = "end";
		public static ObscuredString Score = "hit";
		public static ObscuredString DodgeBall = "miss";
		public static ObscuredString DodgeBomb = "dodge";
	}
}
