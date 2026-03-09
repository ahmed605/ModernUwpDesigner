using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.HostServices;
using Microsoft.VisualStudio.DesignTools.DesignerHost.ShadowCopy;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Markup.XmlModify;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.Diagnostics;
using Microsoft.VisualStudio.DesignTools.Utility.IO;
using Microsoft.VisualStudio.DesignTools.Utility.Telemetry;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.ShadowCopy;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Xaml.LanguageService;
using Microsoft.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost;

public class UwpHostPlatform : HostPlatformBase
{
	private List<IHostSdkReference> sdkReferences;

	private bool hasLoggedCustomSdkLocationTelemetry;

	private bool hasLoggedSdkMismatchTelemetry;

	internal IAppPackageHelper AppPackageHelper { get; private set; }

	protected override IShadowCopyWorkerFactory ShadowCopyWorkerFactory => new UwpShadowCopyWorkerFactory(this);

	public override IUriResolver UriResolver => UwpUriResolver.Instance;

	internal static IServiceProvider ServiceProvider { get; private set; }

    public UwpHostPlatform(IServiceProvider serviceProvider, PlatformIdentifier platformIdentifier)
		: this(serviceProvider, platformIdentifier, null)
	{
	}

	protected UwpHostPlatform(IServiceProvider serviceProvider, PlatformIdentifier platformIdentifier, IAppPackageHelper appPackageHelper)
        //: base(serviceProvider, platformIdentifier)
        : base(serviceProvider, new PlatformIdentifier(new PlatformName(PlatformNames.UAP, platformIdentifier.TargetPlatformVersion), platformIdentifier.TargetRuntime, platformIdentifier.GetTargetFramework(), platformIdentifier.GetTargetSdk(), XamlRuntimeNames.UAP)) // HACK: FIX ME
    {
		AppPackageHelper = appPackageHelper ?? new AppPackageHelper();
		ServiceProvider = serviceProvider;
    }

	internal void SetAppPackageHelper(IAppPackageHelper appPackageHelper)
	{
		AppPackageHelper = appPackageHelper;
	}

	public override string GetEvaluatedProperty(string propertyName)
	{
		return propertyName switch
		{
			"PlatformInstalledPath" => EnvironmentHelper.GetInstalledPath(base.PlatformIdentifier.GetTargetPlatform(), base.PlatformIdentifier.GetTargetSdk()), 
			"PlatformRuntimeWindowsAssemblyLocation" => EnvironmentHelper.GetRuntimeWinmdLocation(base.PlatformIdentifier.GetTargetSdk(), base.PlatformIdentifier.GetTargetPlatform()), 
			"IsDeveloperLicenseRequired" => "true", 
			_ => base.GetEvaluatedProperty(propertyName), 
		};
	}

	public override bool IsCompatibleRuntimePlatform(IHostProject hostProject, out string errorSummary, out string errorDetails)
	{
		errorSummary = string.Empty;
		errorDetails = string.Empty;

		if (hostProject?.GetBoolProperty(Constants.Properties.SkipXamlDesignerSdkCheck) is true)
		{
			return true;
        }

		string text = hostProject?.GetPropertyCompat("TargetPlatformSdkRootOverride");
		if (!string.IsNullOrEmpty(text))
		{
			errorSummary = StringTable.CompatibleRuntimeCustomSdkSummary;
			errorDetails = string.Format(CultureInfo.InvariantCulture, StringTable.CompatibleRuntimeCustomSdkDetailsFormat, text);
			LogCustomSdkLocationTelemetry(hostProject);
			return false;
		}

		bool flag = true;
		Version version = new("10.0");
		Version windows10Sdk_10_0_ = PlatformVersionHelper.Windows10Sdk_10_0_16299;
		string text2 = StringTable.WindowsFallCreatorsUpdate;
		string arg = windows10Sdk_10_0_.ToString();
		Version runtimePlatformVersion = GetRuntimePlatformVersion();
		if (!OSHelper.IsOSVersionOrLater(version) || (runtimePlatformVersion != null && runtimePlatformVersion < windows10Sdk_10_0_))
		{
			flag = false;
		}

		Version targetPlatformVersion = base.PlatformIdentifier.TargetPlatformVersion;
		if (targetPlatformVersion != null && targetPlatformVersion > windows10Sdk_10_0_ && runtimePlatformVersion != null && targetPlatformVersion > runtimePlatformVersion)
		{
			flag = false;
			LogSdkMismatchTelemetry(hostProject, runtimePlatformVersion);
			PlatformName targetPlatform = base.PlatformIdentifier.GetTargetPlatform();
			SdkName targetSdk = base.PlatformIdentifier.GetTargetSdk();
			if (targetPlatform != null && targetSdk != null)
			{
				text2 = SdkUtil.GetPlatformFriendlyName(targetSdk, targetPlatform);
				arg = SdkUtil.GetPlatformVersion(targetSdk, targetPlatform);
			}
		}

		if (!flag)
		{
			if (text2 != null && windows10Sdk_10_0_ != null)
			{
				if (targetPlatformVersion.Build == PlatformVersionHelper.Windows10Sdk_10_0_20348.Build)
				{
					text2 = "Windows 11";
					arg = PlatformVersionHelper.Windows10Sdk_10_0_22000.ToString(4);
				}
				errorSummary = StringTable.CompatibleRuntimeDownlevelOSSummary;
				errorDetails = string.Format(CultureInfo.CurrentCulture, StringTable.CompatibleRuntimeDownlevelOSDetailsFormat, text2, arg);
			}
			return false;
		}

		return base.IsCompatibleRuntimePlatform(hostProject, out errorSummary, out errorDetails);
	}

	public override IEnumerable<IHostSdkReference> GetSdkReferences(IHostProject hostProject)
	{
		if (sdkReferences == null && hostProject is IHostDesignerProject hostDesignerProject)
		{
			List<IHostSdkReference> list = new List<IHostSdkReference>();
			string property = hostProject.GetPropertyCompat("EnableAppLocalVCLibs");
			if (!string.Equals(property, "true", StringComparison.OrdinalIgnoreCase))
			{
				IHostSdkReference hostSdkReference = hostDesignerProject.ResolveSdkReference("Microsoft.VCLibs, Version=14.0");
				if (hostSdkReference != null)
				{
					list.Add(hostSdkReference);
				}
			}
			IHostSdkReference hostSdkReference2 = hostDesignerProject.ResolveSdkReference("Microsoft.VCLibs.120, Version=14.0");
			if (hostSdkReference2 != null)
			{
				list.Add(hostSdkReference2);
			}
			sdkReferences = list;
		}
		return sdkReferences;
	}

	public Version GetRuntimePlatformVersion()
	{
		try
		{
			return UwpWindowsRuntimeUtility.GetRuntimePlatformVersion();
		}
		catch (Exception)
		{
		}
		return null;
	}

	private void LogCustomSdkLocationTelemetry(IHostProject hostProject)
	{
		if (!hasLoggedCustomSdkLocationTelemetry)
		{
			HostTelemetryService?.TelemetryService.ReportEvent(TelemetryArea.DesignSurface, "CustomSdkLocationInUse", HostTelemetryHelper.GetHostProjectTelemetryProperties(hostProject));
			hasLoggedCustomSdkLocationTelemetry = true;
		}
	}

	private void LogSdkMismatchTelemetry(IHostProject hostProject, Version runtimePlatformVersion)
	{
		if (!hasLoggedSdkMismatchTelemetry)
		{
			DataPointCollection hostProjectTelemetryProperties = HostTelemetryHelper.GetHostProjectTelemetryProperties(hostProject);
			hostProjectTelemetryProperties.Add("RuntimePlatformVersion", runtimePlatformVersion);
			HostTelemetryService?.TelemetryService.ReportEvent(TelemetryArea.DesignSurface, "RuntimeVersionMismatchDetectedForDesigner", hostProjectTelemetryProperties);
			hasLoggedSdkMismatchTelemetry = true;
		}
	}

	protected override void CleanShadowCache(string folder)
	{
		string text = Path.Combine(folder, "AppxManifest.xml");
		if (File.Exists(text))
		{
			string packageMoniker = AppPackageHelper.GetPackageMoniker(text);
			Logger.Debug("Unregistering application " + packageMoniker + " (blocking)", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\UwpHostPlatform.cs");
			AppPackageHelper.UnregisterPackage(packageMoniker, null, waitForCompletion: true);
		}
		base.CleanShadowCache(folder);
	}

	protected override async Task EnsureProjectPayloadAsync(IHostProject hostProject, SurfaceProcessInfo info, CancellationToken cancelToken)
	{
		await base.EnsureProjectPayloadAsync(hostProject, info, cancelToken);
		IHostShadowCopyWorker shadowCopyWorker = info.ShadowCopyWorker;
		string packageName = EnsurePackageName(info.ShadowCacheContent);
		await shadowCopyWorker.FixupResourcePriFileAsync(packageName, cancelToken);

        if (ServiceProvider?.GetService(typeof(DTE)) is DTE2 dte)
        {
            while (dte.Solution?.SolutionBuild?.BuildState is vsBuildState.vsBuildStateInProgress)
            {
                await Task.Delay(200);
            }
        }
    }

	protected override ISurfaceProcess ActivateSurface(IServiceProvider serviceProvider, Guid surfaceProcessId, string path, string tapPath, IPipeDataBridge dataBridge, bool inhibitStartupWatsons, CancellationToken cancelToken)
	{
		string appUserModelIdFromManifest = AppPackageHelper.GetAppUserModelIdFromManifest(path);
		Logger.Debug("Activating designer application " + appUserModelIdFromManifest, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\UwpHostPlatform.cs");
		if (ApiInformation.IsApiContractPresent("Windows.UI.Xaml.Hosting.HostingContract", 2))
		{
			IHostWatsonTracker hostWatsonTracker = serviceProvider?.GetHostService<IHostWatsonTracker>();
			Type type = Type.GetType("Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.SurfaceIsolation.UwpSurfaceProcess, Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner");
			ConstructorInfo constructor = type.GetConstructor(new Type[8]
			{
				typeof(IHostWatsonTracker),
				typeof(Guid),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(IPipeDataBridge),
				typeof(bool),
				typeof(CancellationToken)
			});
			return (ISurfaceProcess)constructor.Invoke(new object[8] { hostWatsonTracker, surfaceProcessId, path, appUserModelIdFromManifest, tapPath, dataBridge, inhibitStartupWatsons, cancelToken });
		}
		throw new PlatformNotSupportedException();
	}

	protected override Task EnsurePackageRegisteredAsync(string manifestPath, Dictionary<string, object> telemetryProperties, CancellationToken cancelToken)
	{
		Logger.Debug("Registering designer application " + manifestPath, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\UwpHostPlatform.cs");
		return Task.Run(delegate
		{
			AccessHelper.AclDirectory(PathHelperBase.GetDirectoryNameOrRoot(manifestPath));
			if (!AppPackageHelper.IsPackageRegisteredForCurrentUser(manifestPath))
			{
				cancelToken.ThrowIfCancellationRequested();
				bool removeExistingPackages = true;
				long registerTime;
				long unregisterTime;
				Exception ex = AppPackageHelper.RegisterPackageTimed(manifestPath, cancelToken, removeExistingPackages, out registerTime, out unregisterTime);
				telemetryProperties["SurfacePackageRegisterTime"] = registerTime;
				telemetryProperties["SurfacePackageUnregisterTime"] = unregisterTime;
				if (ex != null)
				{
					Logger.Debug($"Registration failed: {ex}", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\UwpHostPlatform.cs");
					throw ex;
				}
				string destFileName = manifestPath + ".registered";
				File.Copy(manifestPath, destFileName, overwrite: true);
			}
		}, cancelToken);
	}

	protected override void IdleCleanup(IHostShadowCacheContent shadowCacheContent, string triggeringProcessIdentifier)
	{
		base.IdleCleanup(shadowCacheContent, triggeringProcessIdentifier);
		AppRegistrationCleaner appRegistrationCleaner = new AppRegistrationCleaner(HostTelemetryService, triggeringProcessIdentifier, AppPackageHelper);
		appRegistrationCleaner.CleanOldPackages();
	}

	private string EnsurePackageName(IHostShadowCacheContent content)
	{
		string text = content.FindCachedItem("AppxManifest.xml");
		string text2 = AppPackageHelper.GetPackageName(text);
		if (AppPackageHelper.IsNewPackage(text2) || AppPackageHelper.IsRegisteredManifestDifferent(text))
		{
			PurgeOldPackage(content, text);
			Logger.Debug("Updating App ID for manifest " + text, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\UwpHostPlatform.cs");
			text2 = AppPackageHelper.CreateNewPackageName();
			string xml = File.ReadAllText(text);
			File.WriteAllText(text, XmlCleaner.Clean(xml, new ManifestTransformContext(text2)).Xml);
		}
		return text2;
	}

	private void PurgeOldPackage(IHostShadowCacheContent content, string manifestPath)
	{
		content.RemoveItem("resources.pri", deleteNow: true);
		if (!string.IsNullOrEmpty(content.FindCachedItem("resources.pri")))
		{
			throw new InvalidOperationException();
		}
		string packageMoniker = AppPackageHelper.GetPackageMoniker(manifestPath);
		if (AppPackageHelper.IsPackageMonikerRegisteredToCurrentUser(packageMoniker))
		{
			HostTelemetryService?.TelemetryService?.PostOperation(TelemetryArea.DesignSurface, "AppPackageRegistrationUpdated", TelemetryResult.None);
			AppPackageHelper.UnregisterPackage(packageMoniker);
		}
	}
}
