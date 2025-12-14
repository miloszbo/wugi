using Godot;
using WinMonitor; // This is the namespace of your DLL
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
public static class WinBridge
{
	private static bool _initLogged = false;

	private static void LogInit()
	{
		if (_initLogged)
			return;

		_initLogged = true;
		string path = ProjectSettings.GlobalizePath("res://Native/WinMonitor.dll");
		GD.Print("[WinBridge] Loaded managed WinMonitor.dll at: ", path);
	}

	/* =====================================================================
	   WINDOW / USER32
	   ===================================================================== */

	public static string GetActiveWindowTitle()
	{
		LogInit();
		return WinAPI.GetActiveWindowTitle();
	}

	public static uint GetActiveWindowPID()
	{
		LogInit();
		return WinAPI.GetActiveWindowPID();
	}

	/* =====================================================================
	   SYSTEM UPTIME
	   ===================================================================== */

	public static ulong GetSystemUptimeMS()
	{
		LogInit();
		return WinAPI.GetSystemUptimeMS();
	}

	/* =====================================================================
	   MEMORY
	   ===================================================================== */

	public static uint GetMemoryLoad()
	{
		LogInit();
		return WinAPI.GetMemoryLoad();
	}

	public static ulong GetTotalPhysicalMemory()
	{
		LogInit();
		return WinAPI.GetTotalPhysicalMemory();
	}

	public static ulong GetAvailablePhysicalMemory()
	{
		LogInit();
		return WinAPI.GetAvailablePhysicalMemory();
	}

	/* =====================================================================
	   CPU
	   ===================================================================== */

	public static int GetCPUCount()
	{
		LogInit();
		return WinAPI.GetCPUCount();
	}

	public static string GetCPUArchitecture()
	{
		LogInit();
		return WinAPI.GetCPUArchitecture();
	}

	/* =====================================================================
	   OS / ENVIRONMENT
	   ===================================================================== */

	public static string GetOSVersion()
	{
		LogInit();
		return WinAPI.GetOSVersion();
	}

	public static string GetMachineName()
	{
		LogInit();
		return WinAPI.GetMachineName();
	}

	public static string GetUserName()
	{
		LogInit();
		return WinAPI.GetUserName();
	}

	public static string GetDotnetVersion()
	{
		LogInit();
		return WinAPI.GetDotnetVersion();
	}

	/* =====================================================================
	   DRIVE / FILESYSTEM
	   ===================================================================== */

	public static string GetDriveList()
	{
		LogInit();
		return WinAPI.GetDriveList();
	}

	public static long GetDriveFreeSpace(char driveLetter)
	{
		LogInit();
		return WinAPI.GetDriveFreeSpace(driveLetter);
	}

	public static long GetDriveTotalSpace(char driveLetter)
	{
		LogInit();
		return WinAPI.GetDriveTotalSpace(driveLetter);
	}

	/* =====================================================================
	   SPECIAL FOLDERS
	   ===================================================================== */

	public static string GetSpecialFolder(int folder)
	{
		LogInit();
		return WinAPI.GetSpecialFolder(folder);
	}

	/* =====================================================================
	   NETWORK
	   ===================================================================== */

	public static string GetNetworkAdapterNames()
	{
		LogInit();
		return WinAPI.GetNetworkAdapterNames();
	}

	/* =====================================================================
	   GPU
	   ===================================================================== */

	public static string GetGPUList()
	{
		LogInit();
		return WinAPI.GetGPUList();
	}

	/* =====================================================================
	   DISPLAY
	   ===================================================================== */

	public static string GetPrimaryDisplayResolution()
	{
		LogInit();
		return WinAPI.GetPrimaryDisplayResolution();
	}

	/* =====================================================================
	   BATTERY / POWER
	   ===================================================================== */

	public static int GetBatteryPercent()
	{
		LogInit();
		return WinAPI.GetBatteryPercent();
	}

	public static bool IsACConnected()
	{
		LogInit();
		return WinAPI.IsACConnected();
	}

	/* =====================================================================
	   LOCALE / TIMEZONE
	   ===================================================================== */

	public static string GetTimeZoneName()
	{
		LogInit();
		return WinAPI.GetTimeZoneName();
	}

	public static string GetUILocale()
	{
		LogInit();
		return WinAPI.GetUILocale();
	}

	/* =====================================================================
	   PROCESS SUMMARY
	   ===================================================================== */

	public static int GetProcessCount()
	{
		LogInit();
		return WinAPI.GetProcessCount();
	}

	public static string GetProcessList()
	{
		LogInit();
		return WinAPI.GetProcessList();
	}

	/* =====================================================================
	   FILESYSTEM EVENTS
	   ===================================================================== */

	public static bool StartWatchingFolder(string folder)
	{
		LogInit();
		return WinAPI.StartWatchingFolder(folder);
	}

	public static void StopWatchingFolder()
	{
		LogInit();
		WinAPI.StopWatchingFolder();
	}

	public static string? GetNextFileEvent()
	{
		LogInit();
		return WinAPI.GetNextFileEvent();
	}

	public static void ClearFileEventQueue()
	{
		LogInit();
		WinAPI.ClearFileEventQueue();
	}

	/* =====================================================================
	   PROCESS SNAPSHOTS
	   ===================================================================== */

	public static string GetProcessSnapshot()
	{
		LogInit();
		return WinAPI.GetProcessSnapshot();
	}

	public static string GetActiveProcessInfo()
	{
		LogInit();
		return WinAPI.GetActiveProcessInfo();
	}
}
