using Godot;
using System;

public partial class WindowsConsoleTest : Node
{
	private RichTextLabel _output;
	private LineEdit _input;

	public override void _Ready()
	{   	
		_output = GetNode<RichTextLabel>("RichTextLabel");
		_input = GetNode<LineEdit>("LineEdit");

		_output.Clear();

		// Connect Enter key on LineEdit
		_input.TextSubmitted += OnInputSubmitted;

		// Subscribe to console output
		SystemConsole.Instance.ConsoleOutputReceived += OnConsoleOutput;
	}

	public override void _ExitTree()
	{
		// Clean unsubscribe (important)
		if (SystemConsole.Instance != null)
			SystemConsole.Instance.ConsoleOutputReceived -= OnConsoleOutput;
	}

	/* ============================================================
	   INPUT → CONSOLE
	   ============================================================ */

	private void OnInputSubmitted(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		SystemConsole.Instance.Send(text+"\r");

		_input.Clear();
		_input.GrabFocus();
	}

	/* ============================================================
	   CONSOLE → OUTPUT
	   ============================================================ */

	private void OnConsoleOutput(string text)
	{
		// Append raw console output
		_output.AppendText(text);
		GD.Print(text);
		// Auto-scroll to bottom
		_output.ScrollToLine(_output.GetLineCount());
	}
}
