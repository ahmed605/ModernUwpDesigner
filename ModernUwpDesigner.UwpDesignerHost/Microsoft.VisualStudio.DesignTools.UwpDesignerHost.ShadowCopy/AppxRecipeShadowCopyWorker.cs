using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.HostServices;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.Data;
using Microsoft.VisualStudio.DesignTools.Utility.Diagnostics;
using Microsoft.VisualStudio.DesignTools.Utility.IO;
using Microsoft.VisualStudio.DesignTools.Utility.Telemetry;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.ShadowCopy;

internal class AppxRecipeShadowCopyWorker : UwpHostShadowCopyWorker
{
    private enum AppXbfState
    {
        NotComputed,
        NoAppXbf,
        AppXbfPresent
    }

    private List<FileCopyToken> appxRecipeCopyTokens = new List<FileCopyToken>();

    private string originalManifestPath;

    private AppXbfState appXbfState;

    private IPriFileGenerator priFileGenerator;

    private WindowsMetadataHelper winmdHelper;

    private string targetAssemblyPath;

    private string projectName;

    private string defaultNamespace;

    private string sourcePriFilePath;

    private ProjectLanguage projectLanguage;

    private string LooseAppXbfPath => Path.Combine(ShadowCacheFolder, "app.xbf");

    private string EvictedAppXbfPath => Path.Combine(ShadowCacheFolder, "app.xbf.evicted");

    protected string ShadowCacheFolder => base.SurfaceInfo.ShadowCacheContent?.ShadowCacheFolder ?? string.Empty;

    public override IEnumerable<string> SurfaceProcessCriticalFiles
    {
        get
        {
            if (string.IsNullOrEmpty(sourcePriFilePath))
            {
                return Enumerable.Empty<string>();
            }
            return new string[1] { sourcePriFilePath };
        }
    }

    public override bool HasAppXbf => appXbfState == AppXbfState.AppXbfPresent;

    internal string AppxRecipePath
    {
        get
        {
            if (base.SurfaceInfo.ProjectLanguage.IsCPlusPlus())
            {
                return Path.ChangeExtension(targetAssemblyPath, ".build.appxrecipe");
            }
            return Path.Combine(Path.GetDirectoryName(targetAssemblyPath), projectName + ".build.appxrecipe");
        }
    }

    internal void SetPriFileGenerator(IPriFileGenerator priFileGenerator)
    {
        this.priFileGenerator = priFileGenerator;
    }

    public override bool Initialize(IHostPlatform platform, IHostProject hostProject, IAppPackageHelper appPackageHelper, SurfaceProcessInfo surfaceInfo, IHostTelemetryService hostTelemetry)
    {
        base.Initialize(platform, hostProject, appPackageHelper, surfaceInfo, hostTelemetry);
        base.SurfaceInfo.PackageRuntimeAssemblyCount = 0;
        targetAssemblyPath = base.HostProject.TargetAssemblyPath;
        projectName = base.HostProject.ProjectName;
        defaultNamespace = base.HostProject.DefaultNamespaceName;
        projectLanguage = base.HostProject.ProjectLanguage;
        sourcePriFilePath = Path.Combine(Path.GetDirectoryName(targetAssemblyPath), "resources.pri");
        priFileGenerator = new PriFileGenerator(hostProject.PlatformIdentifier, surfaceInfo);
        winmdHelper = new WindowsMetadataHelper(base.HostProject.PlatformIdentifier);
        if (base.SurfaceInfo.ShadowCopyType != HostShadowCopyType.All)
        {
            return false;
        }
        if (!string.Equals(base.SurfaceInfo.Architecture, base.SurfaceInfo.RuntimeArchitecture, StringComparison.OrdinalIgnoreCase))
        {
            surfaceInfo.PlatformOnlyReason = HostPlatformOnlyReason.UnsupportedArchitecture;
            return false;
        }
        if (string.IsNullOrEmpty(targetAssemblyPath) || !File.Exists(targetAssemblyPath))
        {
            surfaceInfo.PlatformOnlyReason = HostPlatformOnlyReason.UnbuiltProject;
            return false;
        }

        if (!File.Exists(AppxRecipePath))
        {
            surfaceInfo.PlatformOnlyReason = HostPlatformOnlyReason.UnbuiltProject;
            return false;
        }
        string a = base.HostProject.GetPropertyCompat("UseDotNetNativeToolchain") ?? "";
        if ((projectLanguage.IsCPlusPlus() || string.Equals(a, "true", StringComparison.InvariantCultureIgnoreCase)) && !PlatformVersionHelper.IsAtLeastRelease(base.HostProject.PlatformIdentifier.TargetPlatformMinVersion, PlatformVersionHelper.MajorRelease.RS1))
        {
            surfaceInfo.PlatformOnlyReason = HostPlatformOnlyReason.MinSKDVersionTooLow;
            return false;
        }
        if (ShouldIgnoreRecipe())
        {
            surfaceInfo.PlatformOnlyReason = HostPlatformOnlyReason.StaleAssemblies;
            return false;
        }
        if (!TryParseAppxRecipe(surfaceInfo.RuntimeArchitecture))
        {
            surfaceInfo.PlatformOnlyReason = HostPlatformOnlyReason.CouldNotParseRecipe;
            return false;
        }
        if (!CheckManifest(originalManifestPath))
        {
            return false;
        }
        return true;
    }

    private bool CheckManifest(string manifestPath)
    {
        try
        {
            if (manifestPath != null)
            {
                PackageManifestUpdater packageManifestUpdater = new PackageManifestUpdater(manifestPath);
                Version runtimePlatformVersion = GetRuntimePlatformVersion();
                if (!packageManifestUpdater.CheckTargetDeviceFamily("Windows.Universal", runtimePlatformVersion) && !packageManifestUpdater.CheckTargetDeviceFamily("Windows.Desktop", runtimePlatformVersion))
                {
                    base.SurfaceInfo.PlatformOnlyReason = HostPlatformOnlyReason.UnsupportedTargetDeviceFamily;
                    return false;
                }
            }
        }
        catch (Exception ex) when (ex is XmlException || ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Debug("Failed to check manifest file: " + ex.Message, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
        }
        return true;
    }

    private Version GetRuntimePlatformVersion()
    {
        try
        {
            return UwpWindowsRuntimeUtility.GetRuntimePlatformVersion();
        }
        catch (Exception ex)
        {
            Logger.Debug("Failed to get runtime platform version: " + ex.Message, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
        }
        return null;
    }

    private bool ShouldIgnoreRecipe()
    {
        IHostProject hostProject = base.HostProject.FindApplicationProject();
        string text = (hostProject?.FindApplicationDocumentByBuildType())?.Path;
        if (string.IsNullOrEmpty(text) || !File.Exists(text))
        {
            return false;
        }
        string text2 = hostProject.TargetAssemblyPath;
        if (string.IsNullOrEmpty(text2) || !File.Exists(text2))
        {
            return true;
        }
        DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(text);
        DateTime lastWriteTimeUtc2 = File.GetLastWriteTimeUtc(text2);
        return lastWriteTimeUtc > lastWriteTimeUtc2;
    }

    public override Task CopyProjectContentAsync(CancellationToken cancelToken)
    {
        if (projectLanguage.IsCPlusPlus())
        {
            foreach (string item in EnvironmentHelper.FindRuntimeAssemblyPaths(base.SurfaceInfo.Architecture))
            {
                base.SurfaceInfo.ShadowCacheContent.AddItem(item, Path.GetFileName(item));
            }
        }
        return Task.Factory.StartNew(/*async*/ delegate
        {
            ExecuteCopy(cancelToken);
            if (string.IsNullOrEmpty(base.SurfaceInfo.FrameworkRuntimeVersion) && base.SurfaceInfo.ShadowCacheContent != null)
            {
                base.SurfaceInfo.FrameworkRuntimeAssemblyVersion = base.SurfaceInfo.ShadowCacheContent.FindAssemblyVersionFromSourceDirectory("System.Runtime.dll");
            }
            string manifestPath = base.SurfaceInfo.ShadowCacheContent.FindCachedItem("AppxManifest.xml");
            PackageManifestUpdater manifestUpdater = new PackageManifestUpdater(manifestPath);
            UpdateManifest(manifestUpdater, cancelToken);
            StoreTargetAssemblyName();

            bool hasHostFxr = SurfaceInfo.ShadowCacheContent.HasTrackedItem("hostfxr.dll") ||
                              !string.IsNullOrWhiteSpace(SurfaceInfo.ShadowCacheContent.FindCachedItem("hostfxr.dll")) ||
                              File.Exists(Path.Combine(SurfaceInfo.ShadowCacheContent.ShadowCacheFolder, "hostfxr.dll"));

            if (hasHostFxr)
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

            /*if (HostProject.GetBoolProperty(Constants.Properties.XamlDesignerForceCleanupResourcesOnCopy, true) &&
               !HostProject.GetBoolProperty(Constants.Properties.XamlDesignerDisableEmbeddedResources))
            {
                await TryEvictAppXbfAsync(cancelToken).ConfigureAwait(continueOnCapturedContext: false);
            }*/
        }, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    protected virtual void ExecuteCopy(CancellationToken cancelToken)
    {
        foreach (FileCopyToken item in FixRecipeCopyTokens(appxRecipeCopyTokens, targetAssemblyPath))
        {
            base.SurfaceInfo.ShadowCacheContent.AddItem(item.SourcePath, item.DestinationPath);
        }
        AppxPlatformOnlyShadowCopyWorker.TrackProjectItems(base.SurfaceInfo.ShadowCacheContent, base.HostProject, cancelToken);
        base.SurfaceInfo.ShadowCacheContent.SyncFolder(cancelToken);
        if (string.IsNullOrEmpty(base.SurfaceInfo.ShadowCacheContent.FindCachedItem(winmdHelper.Destination)))
        {
            base.SurfaceInfo.ShadowCacheContent.AddItem(winmdHelper.WindowsMetaDataLocation, winmdHelper.Destination, forceCopyNow: true);
        }
    }

    protected virtual void UpdateManifest(PackageManifestUpdater manifestUpdater, CancellationToken cancelToken)
    {
        manifestUpdater.ImportDependenciesAndExtensionsFromOriginal();
        if (projectLanguage.IsCPlusPlus())
        {
            CreateInProcessServer(manifestUpdater);
            MigrateDependenciesFromUwpSurface(manifestUpdater, base.SurfaceInfo.SurfaceProcessPath);
        }
        RegisterPackageDependencies(cancelToken);
    }

    private void MigrateDependenciesFromUwpSurface(PackageManifestUpdater manifestUpdater, string surfaceProcessPath)
    {
        PackageManifestUpdater packageManifestUpdater = new PackageManifestUpdater(surfaceProcessPath);
        foreach (XmlElement item in packageManifestUpdater.EnumerateDependencies())
        {
            manifestUpdater.AddPackageDependency(item.GetAttribute("Name"), item.GetAttribute("MinVersion"), item.GetAttribute("Publisher"));
        }
        manifestUpdater.Save();
    }

    public override async Task<bool> TryEvictAppXbfAsync(CancellationToken cancellationToken)
    {
        DateTime start = DateTime.Now;
        try
        {
            return await TryEvictAppXbfAsyncImpl(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }
        catch (Exception ex) when (ex is IOException || ex is SecurityException || ex is UnauthorizedAccessException)
        {
            base.HostTelemetryService?.TelemetryService?.PostFault(TelemetryArea.DesignSurface, "DesignerHost-Exception", ex);
            return false;
        }
        finally
        {
            base.SurfaceInfo.TelemetryProperties["PriEvictAppXbfTime"] = TelemetryHelper.TimeSince(start);
        }
    }

    private async Task<bool> TryEvictAppXbfAsyncImpl(CancellationToken cancellationToken)
    {
        bool result = false;

        string looseAppXbfPath = LooseAppXbfPath;
        Logger.Debug("Evicting " + looseAppXbfPath, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
        if (File.Exists(looseAppXbfPath))
        {
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(looseAppXbfPath);
            File.Delete(looseAppXbfPath);
            string evictedAppXbfPath = EvictedAppXbfPath;
            File.WriteAllText(evictedAppXbfPath, string.Empty);
            File.SetLastWriteTimeUtc(evictedAppXbfPath, lastWriteTimeUtc);

            /*if (HostProject.GetBoolProperty(Constants.Properties.XamlDesignerDisableEmbeddedResources))
            {
                appXbfState = AppXbfState.NoAppXbf;
                return true;
            }*/

            result = true;
        }
        string destinationPriFilePath = priFileGenerator.DestinationPriFilePath;
        string dumpFilePath = priFileGenerator.PriInfoDumpFilePath;
        if (!File.Exists(destinationPriFilePath))
        {
            return result;
        }
        DateTime existingLastWriteTime = File.GetLastWriteTimeUtc(destinationPriFilePath);
        string text = await priFileGenerator.DumpPriAsync(destinationPriFilePath, dumpFilePath, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        if (!string.IsNullOrEmpty(text))
        {
            base.HostTelemetryService?.TelemetryService?.PostFault(TelemetryArea.DesignSurface, "DesignerHost-Exception", text);
            File.Delete(dumpFilePath);
            return result;
        }
        string xaml = File.ReadAllText(dumpFilePath);
        string xml = PriCleaner.Clean(xaml).Xml;
        cancellationToken.ThrowIfCancellationRequested();
        File.WriteAllText(dumpFilePath, xml);
        cancellationToken.ThrowIfCancellationRequested();
        priFileGenerator.WriteMakePriConfig(dumpFilePath, "priinfo");
        string packageName = GetPackageName(base.AppPackageHelper);
        text = await priFileGenerator.MakePriAsync(Path.GetDirectoryName(targetAssemblyPath), packageName, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        if (!string.IsNullOrEmpty(text))
        {
            base.HostTelemetryService?.TelemetryService?.PostFault(TelemetryArea.DesignSurface, "DesignerHost-Exception", text);
            File.Delete(dumpFilePath);
            File.Delete(destinationPriFilePath);
            return result;
        }

        try
        {
            File.Delete(dumpFilePath);
        }
        catch { }

        File.SetLastWriteTimeUtc(destinationPriFilePath, existingLastWriteTime);
        appXbfState = AppXbfState.NoAppXbf;
        return true;
    }

    internal void RegisterPackageDependencies(CancellationToken cancelToken)
    {
        string fileName = base.SurfaceInfo.ShadowCacheContent.FindCachedItem("AppxManifest.xml");
        string text = base.AppPackageHelper.GetPackageArchitecture(fileName);
        if (text == null || (!text.Equals("x86", StringComparison.OrdinalIgnoreCase) && !text.Equals("x64", StringComparison.OrdinalIgnoreCase)))
        {
            text = "x86";
        }
        List<string> appxDependencyPaths = GetAppxDependencyPaths(text);
        foreach (string item in appxDependencyPaths)
        {
            if (!base.AppPackageHelper.IsPackageRegisteredForCurrentUser(item))
            {
                Logger.Debug("Registering package " + item, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
                Exception ex = base.AppPackageHelper.RegisterPackage(item, cancelToken, removeExistingPackages: false);
                if (ex != null)
                {
                    Logger.Debug($"Error registering package {item}. {ex}", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
                }
            }
        }
    }

    public virtual string GetPackageName(IAppPackageHelper appPackageHelper)
    {
        string fileName = base.SurfaceInfo.ShadowCacheContent.FindCachedItem("AppxManifest.xml");
        return appPackageHelper.GetPackageName(fileName);
    }

    public virtual List<string> GetAppxDependencyPaths(string runtimeArchitecture)
    {
        if (!File.Exists(AppxRecipePath))
        {
            return new List<string>();
        }
        using XmlReader reader = XmlUtility.CreateXmlReader(AppxRecipePath);
        XmlDocument xmlDocument = XmlUtility.CreateXmlDocument();
        xmlDocument.Load(reader);
        return GetDependenciesFromXml(runtimeArchitecture, xmlDocument);
    }

    private List<string> GetDependenciesFromXml(string runtimeArchitecture, XmlDocument doc)
    {
        List<string> list = new List<string>();
        foreach (XmlElement item2 in doc.GetElementsByTagName("ResolvedSDKReference"))
        {
            XmlNodeList elementsByTagName = item2.GetElementsByTagName("Architecture");
            if (elementsByTagName.Count > 0)
            {
                string innerText = elementsByTagName[0].InnerText;
                string item = Uri.UnescapeDataString(item2.GetElementsByTagName("AppxLocation")[0].InnerText);
                if (innerText.Equals(runtimeArchitecture, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(item);
                }
            }
        }
        return list;
    }

    private bool TryParseAppxRecipe(string runtimeArchitecture)
    {
        try
        {
            XmlDocument xmlDocument = XmlUtility.CreateXmlDocument();
            using (XmlReader reader = XmlUtility.CreateXmlReader(AppxRecipePath))
            {
                xmlDocument.Load(reader);
            }
            if (xmlDocument.DocumentElement != null)
            {
                appxRecipeCopyTokens.Clear();
                PopulateAppxRecipeCopyList(xmlDocument.DocumentElement);
                return HasValidFramework(FindFrameworkVersion(xmlDocument, runtimeArchitecture));
            }
        }
        catch (Exception ex) when (ex is XmlException || ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Debug("Recipe file exists but cannot be used because of exception: " + ex.Message, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
        }
        return false;
    }

    private string FindFrameworkVersion(XmlDocument document, string runtimeArchitecture)
    {
        XmlNodeList elementsByTagName = document.GetElementsByTagName("ResolvedSDKReference");
        XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(document.NameTable);
        foreach (XmlNode item in elementsByTagName)
        {
            xmlNamespaceManager.AddNamespace("ns", item.NamespaceURI);
            XmlNode xmlNode2 = item.SelectSingleNode("ns:Name", xmlNamespaceManager);
            XmlNode xmlNode3 = item.SelectSingleNode("ns:Architecture", xmlNamespaceManager);
            if (xmlNode2 != null && xmlNode2.InnerText.StartsWith("Microsoft.NET.CoreRuntime") && xmlNode3 != null && string.Equals(xmlNode3.InnerText, runtimeArchitecture, StringComparison.CurrentCultureIgnoreCase))
            {
                XmlNode xmlNode4 = item.SelectSingleNode("ns:Version", xmlNamespaceManager);
                if (xmlNode4 != null && Version.TryParse(xmlNode4.InnerText, out var result))
                {
                    return result.ToString();
                }
            }
        }
        if (projectLanguage.IsCPlusPlus())
        {
            CoreRuntimeRegistrationHelper coreRuntimeRegistrationHelper = new CoreRuntimeRegistrationHelper(base.HostProject, base.SurfaceInfo);
            return coreRuntimeRegistrationHelper.FindCoreRuntimeVersion(base.AppPackageHelper);
        }
        return null;
    }

    protected virtual bool HasValidFramework(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            if (!projectLanguage.IsCPlusPlus() && base.SurfaceInfo.FrameworkRuntimeName == null)
            {
                return false;
            }
            base.SurfaceInfo.FrameworkRuntimeName = ".NETFramework";
            base.SurfaceInfo.FrameworkRuntimeVersion = Environment.Version.ToString();
        }
        else
        {
            base.SurfaceInfo.FrameworkRuntimeName = ".NETCore";
            base.SurfaceInfo.FrameworkRuntimeVersion = version;
        }
        return true;
    }

    private void PopulateAppxRecipeCopyList(XmlNode node)
    {
        if (string.Equals(node.Name, "AppxPackagedFile", StringComparison.OrdinalIgnoreCase))
        {
            appxRecipeCopyTokens.Add(CreateCopyToken(node, string.Empty));
        }
        else if (string.Equals(node.Name, "AppXManifest", StringComparison.OrdinalIgnoreCase))
        {
            originalManifestPath = Uri.UnescapeDataString(node.Attributes["Include"].Value);
            appxRecipeCopyTokens.Add(CreateCopyToken(node, PackageManifestUpdater.originalManifestExtension));
        }
        foreach (XmlNode childNode in node.ChildNodes)
        {
            PopulateAppxRecipeCopyList(childNode);
        }
    }

    private FileCopyToken CreateCopyToken(XmlNode node, string extension)
    {
        string sourcePath = Uri.UnescapeDataString(node.Attributes["Include"].Value);
        return new FileCopyToken(sourcePath, node.FirstChild.InnerText + extension);
    }

    protected virtual List<FileCopyToken> FixRecipeCopyTokens(List<FileCopyToken> copyList, string targetAssemblyPath)
    {
        Logger.Debug($"Fixing recipe copy tokens {copyList?.Count}", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
        List<FileCopyToken> list = new List<FileCopyToken>();
        foreach (FileCopyToken copy in copyList)
        {
            if (!ShouldIncludeToken(copy))
            {
                continue;
            }
            string text = Uri.UnescapeDataString(copy.DestinationPath);
            string sourcePath = Uri.UnescapeDataString(copy.SourcePath);
            bool flag = string.Equals(PathHelperBase.GetExtension(copy.SourcePath), ".exe", StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(targetAssemblyPath))
            {
                string fileName = Path.GetFileName(this.targetAssemblyPath);
                if (string.Equals(copy.DestinationPath, fileName, StringComparison.OrdinalIgnoreCase) && flag)
                {
                    continue;
                }
                string b = Path.Combine("entrypoint", fileName);
                if (string.Equals(text, b, StringComparison.OrdinalIgnoreCase))
                {
                    text = Path.GetFileName(text);
                }
            }
            list.Add(new FileCopyToken(sourcePath, text));
            if (!flag)
            {
                base.SurfaceInfo.PackageRuntimeAssemblyCount++;
            }
        }
        return list;
    }

    protected bool ShouldIncludeToken(FileCopyToken token)
    {
        string destinationPath = token.DestinationPath;
        if ("resources.pri".Equals(destinationPath, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }
        if ("app.xbf".Equals(destinationPath, StringComparison.InvariantCultureIgnoreCase))
        {
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(EvictedAppXbfPath);
            bool flag = lastWriteTimeUtc < File.GetLastWriteTimeUtc(token.SourcePath);
            //appXbfState = ((!flag) ? AppXbfState.NoAppXbf : AppXbfState.AppXbfPresent);
            appXbfState = ((!flag) ? AppXbfState.NoAppXbf : AppXbfState.NotComputed);
            return flag;
        }
        return true;
    }

    public override async Task FixupResourcePriFileAsync(string packageName, CancellationToken cancellationToken)
    {
        //if (HostProject.GetBoolProperty(Constants.Properties.XamlDesignerDisableEmbeddedResources))
        //    return;

        cancellationToken.ThrowIfCancellationRequested();
        DateTime start = DateTime.Now;
        string destinationPriFilePath = priFileGenerator.DestinationPriFilePath;
        string priSourcePath = sourcePriFilePath;
        Logger.Debug("Fixing resources.pri " + destinationPriFilePath, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
        if (!File.Exists(priSourcePath))
        {
            if (appXbfState == AppXbfState.NotComputed)
            {
                //appXbfState = AppXbfState.AppXbfPresent;
            }
            Logger.Debug($"Skipped fixing resources.pri (state:{appXbfState})", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
            return;
        }
        DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(priSourcePath);
        if (File.Exists(destinationPriFilePath))
        {
            DateTime lastWriteTimeUtc2 = File.GetLastWriteTimeUtc(destinationPriFilePath);
            if (lastWriteTimeUtc2 >= lastWriteTimeUtc)
            {
                DateTime lastWriteTimeUtc3 = File.GetLastWriteTimeUtc(priFileGenerator.PriInfoDumpFilePath);
                if (appXbfState == AppXbfState.NotComputed)
                {
                    //appXbfState = ((!(lastWriteTimeUtc3 >= lastWriteTimeUtc2)) ? AppXbfState.NoAppXbf : AppXbfState.AppXbfPresent);
                    appXbfState = ((!(lastWriteTimeUtc3 >= lastWriteTimeUtc2)) ? AppXbfState.NoAppXbf : AppXbfState.NotComputed);
                }
                Logger.Debug($"Found up to date resources.pri (state:{appXbfState})", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxRecipeShadowCopyWorker.cs");
                return;
            }
        }

        string priIndexerType = "pri";
        //if (HostProject.GetBoolProperty(Constants.Properties.XamlDesignerCleanupResourcesOnBuild, true))
        {
            string error = await priFileGenerator.DumpPriAsync(priSourcePath, priFileGenerator.PriInfoDumpFilePath, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            if (string.IsNullOrEmpty(error))
            {
                string xaml = File.ReadAllText(priFileGenerator.PriInfoDumpFilePath);
                string xml = PriCleaner.Clean(xaml).Xml;
                cancellationToken.ThrowIfCancellationRequested();
                File.WriteAllText(priFileGenerator.PriInfoDumpFilePath, xml);
                cancellationToken.ThrowIfCancellationRequested();

                priSourcePath = priFileGenerator.PriInfoDumpFilePath;
                priIndexerType = "priinfo";
            }
        }

        if (appXbfState == AppXbfState.NotComputed)
        {
            //appXbfState = AppXbfState.AppXbfPresent;
        }
        cancellationToken.ThrowIfCancellationRequested();
        priFileGenerator.WriteMakePriConfig(priSourcePath, priIndexerType);
        await InvokeMakePriAsync(Path.GetDirectoryName(targetAssemblyPath), packageName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        File.Delete(priFileGenerator.PriInfoDumpFilePath);
        base.SurfaceInfo.TelemetryProperties["PriFixupAppIdTime"] = TelemetryHelper.TimeSince(start);
    }

    private async Task InvokeMakePriAsync(string sourceDirectory, string packageName, CancellationToken cancellationToken)
    {
        string text = await priFileGenerator.MakePriAsync(sourceDirectory, packageName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.IsNullOrEmpty(text))
        {
            base.HostTelemetryService?.TelemetryService?.PostFault(TelemetryArea.DesignSurface, "ShadowCachePriFileGeneration", text);
            throw new InvalidOperationException(text);
        }
    }

    private void StoreTargetAssemblyName()
    {
        base.SurfaceInfo.ShadowCacheContent.CacheFileFromText(Path.Combine(ShadowCacheFolder, "UserApplicationInfo.txt"), GetApplicationActivationArguments());
    }

    private void CreateInProcessServer(PackageManifestUpdater updater)
    {
        IAccessService accessService = AccessHelper.AccessService;
        string text = Path.Combine(ShadowCacheFolder, Path.GetFileNameWithoutExtension(targetAssemblyPath) + ".exe");
        string text2 = text + ".dll";
        if (!accessService.FileExists(text))
        {
            return;
        }
        try
        {
            DateTime lastWriteTimeUtc = accessService.FileGetLastWriteTimeUtc(text);
            if (File.Exists(text2))
            {
                File.Delete(text2);
            }
            accessService.FileMove(text, text2);
            accessService.MiscConvertExeToDll(text2);
            accessService.FileSetLastWriteTimeUtc(text2, lastWriteTimeUtc);
            base.SurfaceInfo.ShadowCacheContent.AddItem(string.Empty, text2);
            string fileName = Path.GetFileName(text2);
            string className = defaultNamespace + "." + projectName + "_XamlTypeInfo.XamlMetaDataProvider";
            updater.AddActivatableClass(fileName, className);
            string className2 = defaultNamespace + ".XamlMetaDataProvider";
            updater.AddActivatableClass(fileName, className2);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private string GetApplicationActivationArguments()
    {
        if (!string.IsNullOrEmpty(targetAssemblyPath) && File.Exists(targetAssemblyPath) && !string.IsNullOrEmpty(defaultNamespace))
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}", ProjectNameHelper.GetSafeProjectName(projectName), AssemblyName.GetAssemblyName(targetAssemblyPath).FullName, defaultNamespace);
        }
        return string.Empty;
    }
}
