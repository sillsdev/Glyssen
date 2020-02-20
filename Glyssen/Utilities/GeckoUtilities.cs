using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;

namespace Glyssen.Utilities
{
	static class GeckoUtilities
	{
		public static bool ParseDomEventTargetAsGeckoElement(DomEventTarget domEventTarget, out GeckoElement geckoElement)
		{
			geckoElement = null;
			if (domEventTarget == null)
				return false;

			geckoElement = domEventTarget.CastToGeckoElement();
			return geckoElement != null;
		}

		public static void InitializeGecko()
		{
			Xpcom.Initialize(XulRunnerLocation);
		}

		private static string XulRunnerLocation
		{
			get
			{
				// Firefox files should exist in the "Firefox64" subfolder of the folder containing the executable.
				string firefoxPath = Path.Combine(Application.StartupPath, "Firefox64");
				if (File.Exists(Path.Combine(firefoxPath, "xul.dll")))
					return firefoxPath;

				// But while running in development - look for the nuget packages location.
				var version = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(GeckoWebBrowser)).Location).ProductVersion;
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);

				string fullPath = SearchForFirefoxDlls(path, version);
				if (fullPath == null)
					fullPath = SearchForFirefoxDlls(path, version + "-alpha");
				if (fullPath == null)
					fullPath = SearchForFirefoxDlls(path, version.TrimEnd('0').TrimEnd('.') + "-alpha");
				if (fullPath != null)
					return fullPath;
#if DEBUG
				// Search back from the source file. This is to allow the form designer to initalize geckofx based controls.
				fullPath = SearchForFirefoxDlls(new FileInfo(__FILE__()).DirectoryName, version);
				if (fullPath != null)
				{
					return fullPath;
				}
				throw new ApplicationException($"Unable to locate XulRunner files for Firefox FirefoxHtmlEditor. {codeBase} + {Environment.CurrentDirectory} + {(__FILE__())}");
#else
                throw new ApplicationException($"Unable to locate XulRunner files for Firefox FirefoxHtmlEditor. {codeBase} + {Environment.CurrentDirectory}");
#endif
			}
		}

		private static string SearchForFirefoxDlls(string path, string version)
		{
			string fullPath;
			var dir = new DirectoryInfo(Path.GetDirectoryName(path));
			do
			{
				string bits = Environment.Is64BitProcess ? "64." : "32.";
				fullPath = Path.Combine(dir.FullName,
					Path.Combine("packages", "Geckofx60." + bits + version, "content", "Firefox"));
				if (Directory.Exists(fullPath))
					return fullPath;
				fullPath = Path.Combine(dir.FullName,
					Path.Combine("packages", "Geckofx60." + bits + version.TrimEnd('0').TrimEnd('.'), "content",
						"Firefox"));
				if (Directory.Exists(fullPath))
					return fullPath;
				dir = dir.Parent;
			}
			while (dir != null);

			return null;
		}

#if DEBUG
		static string __FILE__([System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
		{
			return fileName;
		}
#endif
	}
}
