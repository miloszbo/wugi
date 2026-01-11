using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class LLM : Node
{
	/* ==============================
	 * CLAUDE CONFIG (STATIC, OPTIONAL)
	 * ============================== */

	protected static readonly string MODEL = "claude-sonnet-4-5";

	protected static readonly int? MAX_TOKENS = 200;
	protected static readonly float? TEMPERATURE = 0.4f;
	protected static readonly float? TOP_P = null;
	protected static readonly int? TOP_K = null;
	protected static readonly string SYSTEM_PROMPT = @"
	Masz na imię Wugi. jesteś szarą myszką pomagającą użytkownikowi w zarzadzaniu systemem. 
	Mieszkasz w wycieku pamięci w kernelu. Masz dostęp do konsoli użytkownika, i możesz podsuwać mu komendy jednak muszą one być oznaczone w odpowiedni sposób.
	Odpowiedzi powinny być rzetelne i w miarę krótkie, 2-3 linijki łącznie z komendami.
	Przykłądowa odpowiedź:{
	Jasne, pomoge ci utworzyć ten katalog
	[cmd]mkdir ser[/cmd]
	}
	";

	protected static readonly string API_URL = "https://api.anthropic.com/v1/messages";
	protected static readonly string ANTHROPIC_VERSION = "2023-06-01";
	protected static readonly string KEY_FILE_PATH = "res://key.txt";

	/* ==============================
	 * STATE
	 * ============================== */

	private string _apiKey = "";
	private HttpRequest _http;
	private Action<string> _pendingCallback;

	/* ==============================
	 * GODOT LIFECYCLE
	 * ============================== */

	public override void _Ready()
	{
		LoadApiKey();
		SetupHttp();

		// ---- DEBUG TEST (comment out later) ----
		//Prompt("Write one ominous sentence for a sci-fi game intro.",
		//	response =>{GD.Print("\n=== LLM TEST RESPONSE ===\n",response);});
	}

	/* ==============================
	 * PUBLIC API
	 * ============================== */

	public void Prompt(string userText, Action<string> onResponse)
	{
		if (string.IsNullOrWhiteSpace(_apiKey))
		{
			GD.PushError("LLM: API key missing.");
			return;
		}

		_pendingCallback = onResponse;

		var payload = BuildPayload(userText);
		string jsonBody = JsonSerializer.Serialize(payload);

		string[] headers =
		{
			"Content-Type: application/json",
			"Accept: application/json",
			$"x-api-key: {_apiKey}",
			$"anthropic-version: {ANTHROPIC_VERSION}"
		};

		Error err = _http.Request(
			API_URL,
			headers,
			HttpClient.Method.Post,
			jsonBody
		);

		if (err != Error.Ok)
		{
			GD.PushError($"LLM: HTTPRequest failed locally: {err}");
		}
	}

	/* ==============================
	 * PAYLOAD BUILDER
	 * ============================== */

	private Dictionary<string, object> BuildPayload(string userText)
	{
		var payload = new Dictionary<string, object>();

		payload["model"] = MODEL;
		payload["messages"] = new object[]
		{
			new Dictionary<string, object>
			{
				["role"] = "user",
				["content"] = userText
			}
		};

		void Add(string key, object value)
		{
			if (value != null)
				payload[key] = value;
		}

		Add("max_tokens", MAX_TOKENS);
		Add("temperature", TEMPERATURE);
		Add("top_p", TOP_P);
		Add("top_k", TOP_K);

		if (!string.IsNullOrWhiteSpace(SYSTEM_PROMPT))
			payload["system"] = SYSTEM_PROMPT;

		return payload;
	}

	/* ==============================
	 * INTERNAL
	 * ============================== */

	private void SetupHttp()
	{
		_http = new HttpRequest();
		AddChild(_http);
		_http.RequestCompleted += OnRequestCompleted;
	}

	private void OnRequestCompleted(
		long result,
		long responseCode,
		string[] headers,
		byte[] body
	)
	{
		string bodyText = body.GetStringFromUtf8();

		if (responseCode != 200)
		{
			GD.PushError($"LLM HTTP Error {responseCode}, Response Body:\n{bodyText}");
			return;
		}

		try
		{
			using JsonDocument doc = JsonDocument.Parse(bodyText);

			string text =
				doc.RootElement
				   .GetProperty("content")[0]
				   .GetProperty("text")
				   .GetString();

			_pendingCallback?.Invoke(text);
			_pendingCallback = null;
		}
		catch (Exception e)
		{
			GD.PushError($"LLM Parse Error: {e.Message}");
			GD.PushError(bodyText);
		}
	}

	private void LoadApiKey()
	{
		if (!FileAccess.FileExists(KEY_FILE_PATH))
		{
			GD.PushError("LLM: key.txt not found.");
			return;
		}

		using var file = FileAccess.Open(KEY_FILE_PATH, FileAccess.ModeFlags.Read);
		_apiKey = file.GetLine().Trim();

		if (string.IsNullOrWhiteSpace(_apiKey))
			GD.PushError("LLM: key.txt is empty.");
	}
}
