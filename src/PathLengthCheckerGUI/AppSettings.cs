using Avalonia.Controls;
using PathLengthChecker;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Persists user preferences to a JSON file in the application data folder.
	/// </summary>
	public class AppSettings
	{
		private static readonly string SettingsFilePath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"PathLengthChecker",
			"settings.json");

		// Window settings
		public double WindowWidth { get; set; } = 900;
		public double WindowHeight { get; set; } = 700;
		public double WindowLeftPosition { get; set; } = 0;
		public double WindowTopPosition { get; set; } = 0;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WindowState WindowState { get; set; } = WindowState.Normal;

		// Search options
		public string RootDirectory { get; set; } = string.Empty;
		public bool ReplaceRootDirectory { get; set; } = false;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public FileSystemTypes FileSystemTypesToInclude { get; set; } = FileSystemTypes.All;
		public bool IncludeSubdirectories { get; set; } = true;
		public int MinPathLength { get; set; } = 0;
		public int MaxPathLength { get; set; } = PathLengthSearchOptions.MaximumPathLengthMaxValue;
		public string SearchPattern { get; set; } = string.Empty;
		public string RootDirectoryReplacementText { get; set; } = string.Empty;
		public bool UrlEncodePaths { get; set; } = false;

		public static AppSettings Load()
		{
			try
			{
				if (File.Exists(SettingsFilePath))
				{
					var json = File.ReadAllText(SettingsFilePath);
					return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
				}
			}
			catch
			{
				// Return defaults if the settings file can't be read.
			}
			return new AppSettings();
		}

		public void Save()
		{
			try
			{
				var directory = Path.GetDirectoryName(SettingsFilePath)!;
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(SettingsFilePath, json);
			}
			catch
			{
				// Silently ignore save failures.
			}
		}
	}
}
