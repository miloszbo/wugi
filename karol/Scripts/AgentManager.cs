using Godot;
using System;
using System.Collections.Generic;

public partial class AgentManager : Node
{
	/* ==============================
	 * SIGNALS
	 * ============================== */

	[Signal]
	public delegate void ChatResponseEventHandler(string text);

	[Signal]
	public delegate void SystemMessageEventHandler(string text);

	/* ==============================
	 * STATE
	 * ============================== */

	private List<string> _chatHistory = new();
	private List<string> _consoleHistory = new();

	/* ==============================
	 * GODOT LIFECYCLE
	 * ============================== */

	public override void _Ready()
	{
		GD.Print("[AgentManager] Ready");

		if (HasNode("/root/SystemConsole"))
		{
			var console = GetNode<SystemConsole>("/root/SystemConsole");
			console.ConsoleOutputReceived += OnConsoleOutput;
		}
		else
		{
			GD.Print("[AgentManager] WARNING: SystemConsole not found");
		}
	}

	/* ==============================
	 * ENTRY POINT (CHAT UI)
	 * ============================== */

	public void SubmitChat(string text)
	{
		RegisterUserMessage(text);
		string context = BuildContext();
		DispatchToLLM(context);
	}

	/* ==============================
	 * CONSOLE HOOK
	 * ============================== */

	private void OnConsoleOutput(string text)
	{
		_consoleHistory.Add(text);
	}

	/* ==============================
	 * INTERNAL PIPELINE
	 * ============================== */

	private void RegisterUserMessage(string text)
	{
		_chatHistory.Add(text);
	}

	private string BuildContext()
	{
		List<string> parts = new();

		if (_chatHistory.Count > 0)
		{
			parts.Add("[chat]");
			parts.AddRange(_chatHistory);
			parts.Add("[/chat]");
		}

		if (_consoleHistory.Count > 0)
		{
			parts.Add("[console]");
			parts.AddRange(_consoleHistory);
			parts.Add("[/console]");
		}

		return string.Join("\n", parts);
	}

	private void DispatchToLLM(string context)
	{
		if (!HasNode("/root/LLM"))
		{
			EmitSignal(SignalName.SystemMessage, "LLM not available");
			return;
		}

		// ===== FULL PROMPT DUMP =====
		GD.Print("\n========== LLM PROMPT ==========\n");
		GD.Print(context);
		GD.Print("\n======== END PROMPT ==========\n");

		var llm = GetNode<LLM>("/root/LLM");
		llm.Prompt(context, OnLLMResponse);
	}

	private void OnLLMResponse(string text)
	{
		// ===== FULL RESPONSE DUMP =====
		GD.Print("\n======== LLM RESPONSE ========\n");
		GD.Print(text);
		GD.Print("\n====== END RESPONSE ======\n");

		RouteResponseToChat(text);
	}

	private void RouteResponseToChat(string text)
	{
		EmitSignal(SignalName.ChatResponse, text);
	}
}
