using D = UnityEngine.Debug;

// For logging to the console while Yerdy is running in the Unity editor
public static class YerdyEditorLogger
{
	public static Yerdy.LogLevel LogLevel { get; set; }

	static YerdyEditorLogger() {
		LogLevel = Yerdy.LogLevel.Warn;
	}
	
	private static void Log(Yerdy.LogLevel level, string fmt, params object[] args)
	{
		if ((int)level <= (int)LogLevel) {
			if (level == Yerdy.LogLevel.Error)
				D.LogError("[Yerdy] " + string.Format(fmt, args));
			else if (level == Yerdy.LogLevel.Warn)
				D.LogWarning("[Yerdy] " + string.Format(fmt, args));
			else
				D.Log("[Yerdy] " + string.Format(fmt, args));
		}
	}

	public static void Error(string fmt, params object[] args)
	{
		Log(Yerdy.LogLevel.Error, fmt, args);
	}

	public static void Warn(string fmt, params object[] args)
	{
		Log(Yerdy.LogLevel.Warn, fmt, args);
	}

	public static void Info(string fmt, params object[] args)
	{
		Log(Yerdy.LogLevel.Info, fmt, args);
	}

	public static void Debug(string fmt, params object[] args)
	{
		Log(Yerdy.LogLevel.Debug, fmt, args);
	}

	public static void Verbose(string fmt, params object[] args)
	{
		Log(Yerdy.LogLevel.Verbose, fmt, args);
	}

}

