using Godot;
using System;
using System.Collections.Generic;

public partial class SystemMonitor : Node
{
	/* =====================================================================
	   SINGLETON ACCESSOR
	   ===================================================================== */

	public static SystemMonitor Instance
	{
		get
		{
			var tree = (SceneTree)Engine.GetMainLoop();
			return tree.Root.GetNode<SystemMonitor>("SystemMonitor");
		}
	}

	/* =====================================================================
	   POLL RATES
	   ===================================================================== */

	[Export] public float WindowPollRate = 0.2f;
	[Export] public float MemoryPollRate = 1.0f;
	[Export] public float ProcessPollRate = 2.0f;
	[Export] public float PowerPollRate = 5.0f;

	// Future: we can add DisplayPollRate, NetworkPollRate, etc.


	/* =====================================================================
	   TIMERS
	   ===================================================================== */

	private Timer _windowTimer;
	private Timer _memoryTimer;
	private Timer _processTimer;
	private Timer _powerTimer;


	/* =====================================================================
	   INTERNAL POLLED STATE
	   ===================================================================== */

	// Window
	private string _lastActiveWindowTitle = "";
	private uint _lastActiveWindowPID = 0;

	// Memory
	private uint _lastMemoryLoad = 0;
	private ulong _lastAvailableMemory = 0;

	// Power
	private int _lastBatteryPercent = -1;
	private bool _lastACStatus = false;

	// Processes
	private string _lastProcessSnapshot = "";


	/* =====================================================================
	   SIGNALS
	   ===================================================================== */

	[Signal] public delegate void ActiveWindowChangedEventHandler(string title, uint pid);
	[Signal] public delegate void MemoryChangedEventHandler(uint load, ulong available);
	[Signal] public delegate void BatteryChangedEventHandler(int percent, bool acConnected);
	[Signal] public delegate void ProcessListChangedEventHandler(string snapshot);

	// Filesystem monitoring
	[Signal] public delegate void FileEventReceivedEventHandler(string evt);


	/* =====================================================================
	   GODOT INITIALIZATION
	   ===================================================================== */

	public override void _Ready()
	{
		SetupTimers();
		StartTimers();
	}


	private void SetupTimers()
	{
		_windowTimer = MakeTimer(WindowPollRate, nameof(PollWindow));
		_memoryTimer = MakeTimer(MemoryPollRate, nameof(PollMemory));
		_processTimer = MakeTimer(ProcessPollRate, nameof(PollProcesses));
		_powerTimer = MakeTimer(PowerPollRate, nameof(PollPower));

		AddChild(_windowTimer);
		AddChild(_memoryTimer);
		AddChild(_processTimer);
		AddChild(_powerTimer);
	}

	private Timer MakeTimer(float seconds, string method)
	{
		var t = new Timer
		{
			WaitTime = seconds,
			OneShot = false,
			Autostart = false,
		};
		t.Timeout += () => Call(method);
		return t;
	}

	private void StartTimers()
	{
		_windowTimer.Start();
		_memoryTimer.Start();
		_processTimer.Start();
		_powerTimer.Start();
	}

	/* =====================================================================
	   POLLED SUBSYSTEMS
	   ===================================================================== */

	// ---------------- WINDOW ----------------
	private void PollWindow()
	{
		var title = WinBridge.GetActiveWindowTitle();
		var pid = WinBridge.GetActiveWindowPID();

		if (title != _lastActiveWindowTitle || pid != _lastActiveWindowPID)
		{
			_lastActiveWindowTitle = title;
			_lastActiveWindowPID = pid;
			EmitSignal(SignalName.ActiveWindowChanged, title, pid);
		}
	}

	// ---------------- MEMORY ----------------
	private void PollMemory()
	{
		var load = WinBridge.GetMemoryLoad();
		var avail = WinBridge.GetAvailablePhysicalMemory();

		if (load != _lastMemoryLoad || avail != _lastAvailableMemory)
		{
			_lastMemoryLoad = load;
			_lastAvailableMemory = avail;
			EmitSignal(SignalName.MemoryChanged, load, avail);
		}
	}

	// ---------------- POWER ----------------
	private void PollPower()
	{
		var percent = WinBridge.GetBatteryPercent();
		var ac = WinBridge.IsACConnected();

		if (percent != _lastBatteryPercent || ac != _lastACStatus)
		{
			_lastBatteryPercent = percent;
			_lastACStatus = ac;
			EmitSignal(SignalName.BatteryChanged, percent, ac);
		}
	}

	// ---------------- PROCESSES ----------------
	private void PollProcesses()
	{
		var snapshot = WinBridge.GetProcessSnapshot();

		if (snapshot != _lastProcessSnapshot)
		{
			_lastProcessSnapshot = snapshot;
			EmitSignal(SignalName.ProcessListChanged, snapshot);
		}
	}


	/* =====================================================================
	   PUBLIC POLLED ACCESSORS
	   ===================================================================== */

	public string ActiveWindowTitle => _lastActiveWindowTitle;
	public uint ActiveWindowPID => _lastActiveWindowPID;

	public uint MemoryLoad => _lastMemoryLoad;
	public ulong AvailableMemory => _lastAvailableMemory;

	public int BatteryPercent => _lastBatteryPercent;
	public bool ACConnected => _lastACStatus;

	public string CurrentProcessSnapshot => _lastProcessSnapshot;


	/* =====================================================================
	   ON-DEMAND ACCESSORS (All WinBridge functions now mapped)
	   ===================================================================== */

	// ---- Core OS & Environment ----
	public string OSVersion => WinBridge.GetOSVersion();
	public string CPUArchitecture => WinBridge.GetCPUArchitecture();
	public int CPUCount => WinBridge.GetCPUCount();
	public string MachineName => WinBridge.GetMachineName();
	public string UserName => WinBridge.GetUserName();
	public string DotNetVersion => WinBridge.GetDotnetVersion();

	// ---- Memory totals ----
	public ulong TotalPhysicalMemory => WinBridge.GetTotalPhysicalMemory();

	// ---- Time / Locale ----
	public string UILocale => WinBridge.GetUILocale();
	public string TimeZone => WinBridge.GetTimeZoneName();

	// ---- GPU / Display ----
	public string GPUList => WinBridge.GetGPUList();
	public string PrimaryResolution => WinBridge.GetPrimaryDisplayResolution();

	// ---- Network ----
	public string NetworkAdapters => WinBridge.GetNetworkAdapterNames();

	// ---- Drives ----
	public string DriveList => WinBridge.GetDriveList();
	public long DriveFreeSpace(char drive) => WinBridge.GetDriveFreeSpace(drive);
	public long DriveTotalSpace(char drive) => WinBridge.GetDriveTotalSpace(drive);

	// ---- Special folders ----
	public string GetSpecialFolder(int id) => WinBridge.GetSpecialFolder(id);

	// ---- Uptime ----
	public ulong SystemUptimeMS => WinBridge.GetSystemUptimeMS();

	// ---- Active process details ----
	public string ActiveProcessInfo => WinBridge.GetActiveProcessInfo();


	/* =====================================================================
	   FILESYSTEM WATCHING SUBSYSTEM
	   ===================================================================== */

	private bool _isWatchingFolder = false;
	private Timer _fsPollTimer;

	public bool StartWatchingFolder(string folder)
	{
		if (WinBridge.StartWatchingFolder(folder))
		{
			_isWatchingFolder = true;

			// Poll for events every 0.1s
			_fsPollTimer = new Timer
			{
				WaitTime = 0.1f,
				OneShot = false,
				Autostart = true,
			};

			_fsPollTimer.Timeout += PollFilesystemEvents;

			AddChild(_fsPollTimer);
			return true;
		}
		return false;
	}

	public void StopWatchingFolder()
	{
		if (!_isWatchingFolder)
			return;

		_isWatchingFolder = false;
		WinBridge.StopWatchingFolder();

		if (_fsPollTimer != null)
			_fsPollTimer.QueueFree();
	}

	private void PollFilesystemEvents()
	{
		string? evt;
		while ((evt = WinBridge.GetNextFileEvent()) != null)
		{
			EmitSignal(SignalName.FileEventReceived, evt);
		}
	}

	public void ClearFileEventQueue() => WinBridge.ClearFileEventQueue();
}
