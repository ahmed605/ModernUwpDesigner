using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.Diagnostics;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class PriFileGenerator : IPriFileGenerator
{
	private readonly PlatformName targetPlatform;

	private readonly SdkName targetSdk;

	private readonly SurfaceProcessInfo surfaceProcessInfo;

	private const string MakePriConfigTemplate = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n<resources targetOsVersion=\"10.0.0\" majorVersion=\"1\">\r\n        <packaging>\r\n        </packaging>\r\n        <index startIndexAt=\"{startIndexAtToken}\" root=\"{rootToken}\">\r\n            <indexer-config type=\"{indexerType}\"/>\r\n        </index>\r\n</resources>\r\n";

	private const string ConfigFileName = "config.xml";

	private string ShadowCacheFolder => surfaceProcessInfo.ShadowCacheContent.ShadowCacheFolder;

	private string MakePriExePath => EnvironmentHelper.GetPathToSdkFile(targetPlatform, targetSdk, "makepri.exe");

	public string DestinationPriFilePath => Path.Combine(ShadowCacheFolder, "resources.pri");

	public string PriInfoDumpFilePath => Path.Combine(ShadowCacheFolder, DestinationPriFilePath + ".xml");

	public string ConfigFilePath => Path.Combine(ShadowCacheFolder, ConfigFileName);

	public PriFileGenerator(PlatformIdentifier platformIdentifier, SurfaceProcessInfo surfaceProcessInfo)
	{
		//targetPlatform = platformIdentifier.GetTargetPlatform();
		//targetSdk = platformIdentifier.GetTargetSdk();
		this.surfaceProcessInfo = surfaceProcessInfo;

		// HACK: FIX ME

        var identifier = platformIdentifier;
        if (identifier.TargetPlatformIdentifier != PlatformNames.UAP)
            identifier = new PlatformIdentifier(new PlatformName(PlatformNames.UAP, platformIdentifier.TargetPlatformVersion), platformIdentifier.TargetRuntime, platformIdentifier.GetTargetFramework(), platformIdentifier.GetTargetSdk(), XamlRuntimeNames.UAP);

        this.targetPlatform = identifier.GetTargetPlatform();
        this.targetSdk = identifier.GetTargetSdk();
    }

	public void WriteMakePriConfig(string priPath, string indexerType)
	{
		StringBuilder stringBuilder = new StringBuilder(MakePriConfigTemplate);
		stringBuilder.Replace("{rootToken}", SecurityElement.Escape(ShadowCacheFolder));
		stringBuilder.Replace("{startIndexAtToken}", SecurityElement.Escape(priPath));
		stringBuilder.Replace("{indexerType}", indexerType);
		surfaceProcessInfo.ShadowCacheContent.CacheFileFromText(ConfigFileName, stringBuilder.ToString());
	}

	public async Task<string> MakePriAsync(string sourceDirectory, string packageName, CancellationToken cancellationToken)
	{
		string arguments = "new /ProjectRoot \"" + sourceDirectory + "\" /ConfigXml \"" + ConfigFilePath + "\" /IndexName \"" + packageName + "\" /OutputFile \"" + DestinationPriFilePath + "\" /Overwrite";
		int num = await ExecuteMakePri(arguments, cancellationToken);
		if (num == 0)
		{
			Logger.Debug("Successfully generated pri file with arguments: " + arguments, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PriFileGenerator.cs");
			return string.Empty;
		}
		return string.Format(CultureInfo.CurrentCulture, StringTable.ResourcePriGenerationFailed, num, MakePriExePath, arguments);
	}

	public async Task<string> DumpPriAsync(string indexFile, string outputFile, CancellationToken cancellationToken)
	{
		string arguments = "dump /IndexFile \"" + indexFile + "\" /OutputFile \"" + outputFile + "\" /DumpType Detailed /Overwrite";
		int num = await ExecuteMakePri(arguments, cancellationToken);
		if (num == 0)
		{
			Logger.Debug("Successfully dumped pri file with arguments: " + arguments, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PriFileGenerator.cs");
			return string.Empty;
		}
		return $"Failed to dump resource.pri, exit code {num} when running command: \"{MakePriExePath}\" {arguments}";
	}

	private Task<int> ExecuteMakePri(string arguments, CancellationToken cancellationToken)
	{
		string makePriExePath = MakePriExePath;
		Logger.Debug(makePriExePath + " " + arguments, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PriFileGenerator.cs");
		cancellationToken.ThrowIfCancellationRequested();
		ProcessStartInfo processStartInfo = new ProcessStartInfo(makePriExePath);
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = true;
		processStartInfo.Arguments = arguments;
		Process process = new Process();
		process.StartInfo = processStartInfo;
		TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
		CancellationTokenRegistration registration = cancellationToken.Register(delegate
		{
			try
			{
				process.Kill();
			}
			catch (InvalidOperationException ex)
			{
				Logger.Debug("Exception while trying to cancel resource.pri generation: " + ex.Message, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PriFileGenerator.cs");
			}
		}, useSynchronizationContext: false);
		process.Exited += delegate
		{
			registration.Dispose();
			completionSource.TrySetResult(process.ExitCode);
		};
		process.EnableRaisingEvents = true;
		process.Start();
		return completionSource.Task;
	}
}
