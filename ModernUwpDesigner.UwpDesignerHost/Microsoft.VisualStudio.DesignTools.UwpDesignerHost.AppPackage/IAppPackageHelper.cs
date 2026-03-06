using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

public interface IAppPackageHelper
{
	int ActivateApplication(string appUserModelId, bool designerMode, string activationContext, object site);

	void DisablePackageDebugging(string packageMoniker);

	string GetAppUserModelIdFromManifest(string manifest);

	bool IsNewPackage(string packageName);

	string CreateNewPackageName();

	string GetPackageArchitecture(string fileName);

	string GetPackageFamilyName(string fileName);

	string GetPackageMoniker(string fileName);

	string GetPackageName(string fileName);

	bool IsPackageMonikerRegisteredToCurrentUser(string packageMoniker, string directory = null);

	IEnumerable<AppPackageInfo> GetInstalledPackageLocationsForUser();

	bool IsPackageRegisteredForCurrentUser(string manifestPath);

	bool IsRegisteredManifestDifferent(string manifestPath);

	Exception RegisterPackage(string manifestFilePath, bool removeExistingPackages = false);

	Exception RegisterPackage(string manifestFilePath, CancellationToken cancelToken, bool removeExistingPackages);

	Exception RegisterPackageTimed(string manifestFilePath, CancellationToken cancelToken, bool removeExistingPackages, out long registerTime, out long unregisterTime);

	void SetPackageDebugging(string packageMoniker, string debuggerCommandLine, string environment);

	void UnregisterPackage(string appPackageMoniker, Action<Exception> onCompleted = null, bool waitForCompletion = false);

	WaitHandle UnregisterPackageAsync(string appPackageMoniker, Action<Exception> onCompleted = null, bool needWaitHandle = true);
}
