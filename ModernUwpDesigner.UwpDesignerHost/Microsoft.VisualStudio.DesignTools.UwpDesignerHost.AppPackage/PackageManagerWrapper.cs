using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

public class PackageManagerWrapper
{
	private PackageManager packageManager;

	public PackageManagerWrapper()
	{
		packageManager = new PackageManager();
	}

	public IEnumerable<global::Windows.ApplicationModel.Package> FindPackagesForUser(string sid, string packageName, string packagePublisher)
	{
		return packageManager.FindPackagesForUser(sid, packageName, packagePublisher);
	}

	public IEnumerable<global::Windows.ApplicationModel.Package> FindPackagesForUser(string sid)
	{
		return packageManager.FindPackagesForUser(sid);
	}

	public IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> AddPackageAsync(Uri uri)
	{
		return packageManager.AddPackageAsync(uri, Enumerable.Empty<Uri>(), DeploymentOptions.None);
	}

	public virtual IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> RegisterPackageAsync(Uri uri)
	{
		return packageManager.RegisterPackageAsync(uri, Enumerable.Empty<Uri>(), DeploymentOptions.DevelopmentMode);
	}

	public IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> RemovePackageAsync(string fullName)
	{
		return packageManager.RemovePackageAsync(fullName);
	}
}
