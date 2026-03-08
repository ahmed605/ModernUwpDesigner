using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.Utility.Diagnostics;
using Microsoft.VisualStudio.DesignTools.Utility.IO;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.ShadowCopy;

internal abstract class UwpHostShadowCopyWorker : IHostShadowCopyWorker
{
    private const string SurfaceProcessPath = "SurfaceProcess\\Uwp\\";

    private const string UwpSurfaceFile = "UwpSurface.exe";

    private const string windowsXamlDiagnosticsTapDllFileName = "Microsoft.VisualStudio.DesignTools.UwpTap.dll";

    protected IHostProject HostProject { get; private set; }

    protected SurfaceProcessInfo SurfaceInfo { get; private set; }

    protected IHostTelemetryService HostTelemetryService { get; private set; }

    internal IAppPackageHelper AppPackageHelper { get; set; }

    public IHostPlatform Platform { get; private set; }

    public abstract bool HasAppXbf { get; }

    public virtual IEnumerable<string> SurfaceProcessCriticalFiles => Enumerable.Empty<string>();

    public abstract Task CopyProjectContentAsync(CancellationToken cancelToken);

    public abstract Task<bool> TryEvictAppXbfAsync(CancellationToken cancellationToken);

    public virtual bool Initialize(IHostPlatform platform, IHostProject hostProject, IAppPackageHelper appPackageHelper, SurfaceProcessInfo surfaceInfo, IHostTelemetryService hostTelemetry)
    {
        Platform = platform;
        HostTelemetryService = hostTelemetry;
        HostProject = hostProject;
        SurfaceInfo = surfaceInfo;
        AppPackageHelper = appPackageHelper;
        SetSurfaceProcessPath();
        return true;
    }

    public string CopySurfaceProcessPayload(CancellationToken cancelToken)
    {
        string directoryName = Path.GetDirectoryName(SurfaceInfo.SurfaceProcessPath);
        Logger.Debug("Copying UwpSurface.exe components to staging folder " + SurfaceInfo.ShadowCacheContent.ShadowCacheFolder, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\UwpHostShadowCopyWorker.cs");
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary.Add("App.xbf", "App.xbf");
        dictionary.Add("UwpSurface.xr.xml", "UwpSurface.xr.xml");
        dictionary.Add("__SurfaceResources__.xbf", "__SurfaceResources__.xbf");

        bool hasHostFxr = SurfaceInfo.ShadowCacheContent.HasTrackedItem("hostfxr.dll") ||
                          !string.IsNullOrWhiteSpace(SurfaceInfo.ShadowCacheContent.FindCachedItem("hostfxr.dll")) ||
                          File.Exists(Path.Combine(SurfaceInfo.ShadowCacheContent.ShadowCacheFolder, "hostfxr.dll"));

        bool isApplication = HostProject.IsExecutable;

        //string frameworkRuntimeName = SurfaceInfo.FrameworkRuntimeName;
        //if (!(frameworkRuntimeName == ".NETCore"))
        //{
        //    if (!(frameworkRuntimeName == ".NETFramework"))
        //    {
        //        throw new NotSupportedException();
        //    }
        //    dictionary.Add("UwpSurface.exe", "UwpSurface.exe");
        //    dictionary.Add("UwpSurface.dll", "UwpSurface.dll");
        //    dictionary.Add("UwpSurface.pdb", "UwpSurface.pdb");
        //    //dictionary.Add("UwpSurface.deps.json", "UwpSurface.deps.json");
        //    //dictionary.Add("UwpSurface.runtimeconfig.json", "UwpSurface.runtimeconfig.json");
        //    dictionary.Add("AppxManifest.xml", "AppxManifest.xml");
        //}
        //else
        //{
        //    dictionary.Add("UwpSurface.exe", "UwpSurface.exe");
        //    dictionary.Add("UwpSurface.dll", "UwpSurface.dll");
        //    dictionary.Add("UwpSurface.pdb", "UwpSurface.pdb");
        //    //dictionary.Add("UwpSurface.deps.json", "UwpSurface.deps.json");
        //    //dictionary.Add("UwpSurface.runtimeconfig.json", "UwpSurface.runtimeconfig.json");
        //    dictionary.Add("AppxManifest.xml", "AppxManifest.xml");
        //}

        dictionary.Add("UwpSurface.exe", "UwpSurface.exe");
        dictionary.Add("UwpSurface.dll", "UwpSurface.dll");
        dictionary.Add("UwpSurface.pdb", "UwpSurface.pdb");
        //dictionary.Add("UwpSurface.deps.json", "UwpSurface.deps.json");
        //dictionary.Add("UwpSurface.runtimeconfig.json", "UwpSurface.runtimeconfig.json");
        dictionary.Add("AppxManifest.xml", "AppxManifest.xml");

        if (!isApplication)
        {
            dictionary.Add("Microsoft.Windows.SDK.NET.dll", "Microsoft.Windows.SDK.NET.dll");
            dictionary.Add("Microsoft.Windows.UI.Xaml.dll", "Microsoft.Windows.UI.Xaml.dll");
            dictionary.Add("WinRT.Runtime.dll", "WinRT.Runtime.dll");
        }

        if (!hasHostFxr)
        {
            dictionary.Add("UwpSurface.deps.json", "UwpSurface.deps.json");
            dictionary.Add("UwpSurface.runtimeconfig.json", "UwpSurface.runtimeconfig.json");
        }
        else
        {
            SurfaceInfo.ShadowCacheContent.RemoveItem("UwpSurface.deps.json", deleteNow: true);
            SurfaceInfo.ShadowCacheContent.RemoveItem("UwpSurface.runtimeconfig.json", deleteNow: true);

            try
            {
                File.Delete(Path.Combine(SurfaceInfo.ShadowCacheContent.ShadowCacheFolder, "UwpSurface.deps.json"));
                File.Delete(Path.Combine(SurfaceInfo.ShadowCacheContent.ShadowCacheFolder, "UwpSurface.runtimeconfig.json"));
            }
            catch { }
        }

        foreach (KeyValuePair<string, string> item in dictionary)
        {
            cancelToken.ThrowIfCancellationRequested();
            string key = item.Key;
            string value = item.Value;
            string text = Path.Combine(directoryName, key);
            if (!File.Exists(text))
            {
                continue;
            }
            if (value == "AppxManifest.xml")
            {
                string text2 = SurfaceInfo.ShadowCacheContent.FindCachedItem("AppxManifest.xml");
                if (!string.IsNullOrEmpty(text2))
                {
                    SurfaceInfo.ShadowCacheContent.AddItem(string.Empty, text2);
                    Logger.Debug("Keeping existing " + text2, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\UwpHostShadowCopyWorker.cs");
                    continue;
                }
            }
            SurfaceInfo.ShadowCacheContent.AddItem(text, value, forceCopyNow: true);
        }

        return SurfaceInfo.ShadowCacheContent.FindCachedItem("AppxManifest.xml");
    }

    public void EnsureTapAssemblyInFolder(string xamlDiagnosticFolder)
    {
        string tapFolder = SurfaceInfo.TapAssemblyFolder;
        string shadowCacheFolder = SurfaceInfo.ShadowCacheContent.ShadowCacheFolder;
        if (string.IsNullOrEmpty(tapFolder))
        {
            tapFolder = Path.Combine(xamlDiagnosticFolder, SurfaceInfo.RuntimeArchitecture);
        }

        string tapDll = Path.Combine(tapFolder, "Microsoft.VisualStudio.DesignTools.UwpTap.dll");
        bool tapExists = File.Exists(tapDll);

        if (!tapExists)
        {
            tapFolder = tapFolder.Replace("Common7\\IDE\\CommonExtensions\\Microsoft\\XamlDiagnostics", "CoreCon\\Binaries\\XamlDiagnostics");
            tapDll = Path.Combine(tapFolder, "Microsoft.VisualStudio.DesignTools.UwpTap.dll");
            tapExists = File.Exists(tapDll);
        }

        if (tapExists && !AccessHelper.IsAccessibleByAllApplicationPackages(tapDll))
        {
            SurfaceInfo.ShadowCacheContent.AddItem(tapDll, Path.Combine(shadowCacheFolder, "Microsoft.VisualStudio.DesignTools.UwpTap.dll"));
            tapFolder = shadowCacheFolder;
        }

        SurfaceInfo.TapAssemblyFolder = tapFolder;
    }

    public virtual Task FixupResourcePriFileAsync(string packageName, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: true);
    }

    private void SetSurfaceProcessPath()
    {
        //string path = Path.Combine("SurfaceProcess\\Uwp\\", SurfaceInfo.RuntimeArchitecture, "AppxManifest.xml");
        //SurfaceInfo.SurfaceProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

        string path = Path.Combine("XSurfUwp\\", SurfaceInfo.RuntimeArchitecture, "AppxManifest.xml");

        var designerExtensionAssembly = Assembly.Load("ModernUwpDesigner");
        var assemblyFolder = Path.GetDirectoryName(designerExtensionAssembly.Location);
        SurfaceInfo.SurfaceProcessPath = Path.Combine(assemblyFolder, path);
    }
}
