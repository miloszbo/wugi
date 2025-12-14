using Godot;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
public partial class TestConsole : Control
{
	[Export] private RichTextLabel Output;

	private SystemMonitor SM => SystemMonitor.Instance;
	private Timer _refreshTimer;

	public override void _Ready()
	{
		Output.Text = "[Monitoring panel initialized]\n";

		_refreshTimer = new Timer
		{
			OneShot = false,
			WaitTime = 0.25f
		};
		_refreshTimer.Timeout += UpdatePanel;
		AddChild(_refreshTimer);
		_refreshTimer.Start();

		Output.Text += "SystemMonitor instance OK: " + (SM != null) + "\n";
		Output.Text += "Active window (initial): " + SM.ActiveWindowTitle + "\n";
	}

	private void UpdatePanel()
	{
		var sb = new StringBuilder();

		sb.AppendLine("[ SYSTEM MONITORING PANEL ]");
		sb.AppendLine("====================================");

		/* --------------------------
		   WINDOW
		---------------------------*/
		sb.AppendLine("• Active Window");
		sb.AppendLine("   Title: " + SM.ActiveWindowTitle);
		sb.AppendLine("   PID:   " + SM.ActiveWindowPID);
		sb.AppendLine();

		/* --------------------------
		   MEMORY
		---------------------------*/
		sb.AppendLine("• Memory");
		sb.AppendLine("   Load:        " + SM.MemoryLoad + "%");
		sb.AppendLine("   Available:   " + (SM.AvailableMemory / (1024 * 1024)) + " MB");
		sb.AppendLine("   Total:       " + (SM.TotalPhysicalMemory / (1024 * 1024)) + " MB");
		sb.AppendLine();

		/* --------------------------
		   CPU
		---------------------------*/
		sb.AppendLine("• CPU");
		sb.AppendLine("   Count:        " + SM.CPUCount);
		sb.AppendLine("   Architecture: " + SM.CPUArchitecture);
		sb.AppendLine();

		/* --------------------------
		   ENVIRONMENT
		---------------------------*/
		sb.AppendLine("• Environment");
		sb.AppendLine("   OS:       " + SM.OSVersion);
		sb.AppendLine("   User:     " + SM.UserName);
		sb.AppendLine("   Machine:  " + SM.MachineName);
		sb.AppendLine("   .NET:     " + SM.DotNetVersion);
		sb.AppendLine();

		/* --------------------------
		   UPTIME
		---------------------------*/
		sb.AppendLine("• Uptime");
		sb.AppendLine("   System Uptime: " + FormatUptime(SM.SystemUptimeMS));
		sb.AppendLine();

		/* --------------------------
		   BATTERY
		---------------------------*/
		sb.AppendLine("• Power");
		sb.AppendLine("   Battery:  " + SM.BatteryPercent + "%");
		sb.AppendLine("   AC Power: " + (SM.ACConnected ? "Connected" : "Battery"));
		sb.AppendLine();

		/* --------------------------
		   GPU, DISPLAY
		---------------------------*/
		sb.AppendLine("• GPU");
		sb.AppendLine("   GPUs: " + SM.GPUList);
		sb.AppendLine();

		sb.AppendLine("• Display");
		sb.AppendLine("   Primary Resolution: " + SM.PrimaryResolution);
		sb.AppendLine();

		/* --------------------------
		   NETWORK
		---------------------------*/
		sb.AppendLine("• Network");
		sb.AppendLine("   Adapters: " + SM.NetworkAdapters);
		sb.AppendLine();

		/* --------------------------
		   DRIVES
		---------------------------*/
		sb.AppendLine("• Drives");
		sb.AppendLine("   List: " + SM.DriveList);

		foreach (char drive in ExtractDriveLetters(SM.DriveList))
		{
			long free = SM.DriveFreeSpace(drive) / (1024 * 1024);
			long total = SM.DriveTotalSpace(drive) / (1024 * 1024);
			sb.AppendLine("   " + drive + ":    " + free + " MB free / " + total + " MB total");
		}
		sb.AppendLine();

		/* --------------------------
		   SPECIAL FOLDERS
		---------------------------*/
		sb.AppendLine("• Special Folders");
		sb.AppendLine("   Desktop: " + SM.GetSpecialFolder((int)System.Environment.SpecialFolder.Desktop));
		sb.AppendLine("   Documents: " + SM.GetSpecialFolder((int)System.Environment.SpecialFolder.MyDocuments));
		sb.AppendLine("   AppData Local: " + SM.GetSpecialFolder((int)System.Environment.SpecialFolder.LocalApplicationData));
		sb.AppendLine();

		/* --------------------------
		   ACTIVE PROCESS
		---------------------------*/
		sb.AppendLine("• Active Process Info");
		sb.AppendLine("   " + SM.ActiveProcessInfo);
		sb.AppendLine();

		/* --------------------------
		   PROCESSES
		---------------------------*/
		int count = SM.CurrentProcessSnapshot.Split(';', StringSplitOptions.RemoveEmptyEntries).Length;
		sb.AppendLine("• Processes");
		sb.AppendLine("   Total: " + count);
		sb.AppendLine("   (Full list suppressed)");
		sb.AppendLine();

		/* --------------------------
		   LOCALE / TIMEZONE
		---------------------------*/
		sb.AppendLine("• Locale");
		sb.AppendLine("   UI Locale: " + SM.UILocale);
		sb.AppendLine("   Time Zone: " + SM.TimeZone);
		sb.AppendLine();

		/* --------------------------
		   FILESYSTEM WATCHER
		---------------------------*/
		sb.AppendLine("• Filesystem");
		sb.AppendLine("   Monitorowanie file explorer jest cięższe, trzeba będzie ręcznie podłączać foldery ktore chcemy obserwować bo bezpieczeństwo, i pewnnie performance let's be real >.<");
		sb.AppendLine();

		Output.Text = sb.ToString();
	}

	private string FormatUptime(ulong ms)
	{
		TimeSpan t = TimeSpan.FromMilliseconds(ms);
		return ((int)t.TotalHours).ToString("D2") + ":" +
			   t.Minutes.ToString("D2") + ":" +
			   t.Seconds.ToString("D2");
	}

	private IEnumerable<char> ExtractDriveLetters(string list)
	{
		foreach (var entry in list.Split(';', StringSplitOptions.RemoveEmptyEntries))
		{
			if (entry.Length > 0)
				yield return entry[0];
		}
	}
}
