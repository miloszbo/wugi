using Godot;
using System;

public partial class SystemConsole : Node
{
	/* ============================================================
	   SINGLETON ACCESSOR
	   ============================================================ */

	public static SystemConsole Instance
	{
		get
		{
			var tree = (SceneTree)Engine.GetMainLoop();
			return tree.Root.GetNode<SystemConsole>("SystemConsole");
		}
	}

	/* ============================================================
	   CONFIG
	   ============================================================ */

	[Export] public int Columns = 120;
	[Export] public int Rows = 30;
	[Export] public float PollRate = 0.05f;

	[Export(PropertyHint.Enum, "cmd.exe,powershell.exe")]
	public string Shell = "cmd.exe";

	/* ============================================================
	   SIGNALS
	   ============================================================ */

	[Signal]
	public delegate void ConsoleOutputReceivedEventHandler(string text);

	/* ============================================================
	   INTERNAL
	   ============================================================ */

	private Timer _pollTimer;

	/* ============================================================
	   GODOT LIFECYCLE
	   ============================================================ */

	public override void _Ready()
	{
		WinConsoleBridge.Create(Columns, Rows);
		WinConsoleBridge.StartShell(Shell);

		SetupPolling();
	}

	public override void _ExitTree()
	{
		WinConsoleBridge.Close();
	}

	/* ============================================================
	   POLLING
	   ============================================================ */

	private void SetupPolling()
	{
		_pollTimer = new Timer
		{
			WaitTime = PollRate,
			OneShot = false,
			Autostart = true
		};

		_pollTimer.Timeout += PollConsole;
		AddChild(_pollTimer);
	}

	private void PollConsole()
	{
		string output = WinConsoleBridge.Read();
		if (!string.IsNullOrEmpty(output))
			EmitSignal(SignalName.ConsoleOutputReceived, output);
	}

	/* ============================================================
	   PUBLIC API (FOR UI)
	   ============================================================ */

	public void Send(string text)
	{
		if (!text.EndsWith("\r\n"))
			text += "\r\n";

		WinConsoleBridge.Write(text);
	}

	public void ResizeConsole(int cols, int rows)
	{
		Columns = cols;
		Rows = rows;
		WinConsoleBridge.Resize(cols, rows);
	}
}
