using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.IO;
using Microsoft.Win32;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.System;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

public class AppPackageHelper : IAppPackageHelper
{
	private class DeployedOperationHandle
	{
		private int refCount;

		public ManualResetEvent WaitHandle { get; }

		public IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> DeploymentOperation { get; set; }

		public Exception Exception { get; set; }

		public DeployedOperationHandle(ManualResetEvent waitHandle, IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation)
		{
			WaitHandle = waitHandle;
			DeploymentOperation = deploymentOperation;
		}

		public DeployedOperationHandle(ManualResetEvent waitHandle)
		{
			WaitHandle = waitHandle;
		}

		public void AddRef()
		{
			Interlocked.Increment(ref refCount);
		}

		public void Release()
		{
			int num = Interlocked.Decrement(ref refCount);
			if (num <= 0)
			{
				WaitHandle.Dispose();
			}
		}
	}

	public const string AppIdentityPrefix = "app.a";

	public const string AppPublisherName = "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US";

	private static Dictionary<string, DeployedOperationHandle> pendingDeploymentOperations = new Dictionary<string, DeployedOperationHandle>();

	private static object syncLock = new object();

	private static int RegistrationTimeoutInSecond
	{
		get
		{
			string environmentVariable = Environment.GetEnvironmentVariable("AppPackageRegistrationTimeoutInSecond");
			if (string.IsNullOrEmpty(environmentVariable) || !int.TryParse(environmentVariable, out var result) || result < 0)
			{
				return 60;
			}
			return result;
		}
	}

	protected PackageManagerWrapper PackageManager { get; set; }

	public event EventHandler WaitingOnPackageManager;

	public AppPackageHelper()
		: this(new PackageManagerWrapper())
	{
	}

	internal AppPackageHelper(PackageManagerWrapper packageManager)
	{
		PackageManager = packageManager;
	}

	public int ActivateApplication(string appUserModelId, bool designerMode, string activationContext, object site)
	{
		bool isRunningElevated = OSHelper.IsRunningElevated;
		AppPackageNativeMethods.SetProcessRestrictionExemption(isExempt: true);
		int processId = 0;
		object obj = null;
		AppPackageNativeMethods.IApplicationActivationManager applicationActivationManager = null;
		AppPackageNativeMethods.IObjectWithSite objectWithSite = null;
		try
		{
			AppPackageNativeMethods.CLSCTX dwClsContext = AppPackageNativeMethods.CLSCTX.CLSCTX_INPROC_SERVER;
			if (AppPackageNativeMethods.DwmIsCompositionEnabled(out var enabled) != 0)
			{
				enabled = false;
			}
			if (!enabled)
			{
				dwClsContext = AppPackageNativeMethods.CLSCTX.CLSCTX_LOCAL_SERVER;
			}
			if (isRunningElevated)
			{
				dwClsContext = AppPackageNativeMethods.CLSCTX.CLSCTX_LOCAL_SERVER;
			}
			obj = AppPackageNativeMethods.CoCreateInstance(Guid.Parse("45BA127D-10A8-46EA-8AB7-56EA9078943C"), null, dwClsContext, Guid.Parse("2e941141-7f97-4756-ba1d-9decde894a3d"));
			applicationActivationManager = (AppPackageNativeMethods.IApplicationActivationManager)obj;
			if (site != null)
			{
				objectWithSite = applicationActivationManager as AppPackageNativeMethods.IObjectWithSite;
				objectWithSite.SetSite(site);
			}
			applicationActivationManager.ActivateApplication(appUserModelId, activationContext, designerMode ? AppPackageNativeMethods.ActivateOptions.DesignMode : AppPackageNativeMethods.ActivateOptions.None, out processId);
			return processId;
		}
		finally
		{
			if (obj != null)
			{
				Marshal.ReleaseComObject(obj);
			}
			if (applicationActivationManager != null)
			{
				Marshal.ReleaseComObject(applicationActivationManager);
			}
			if (objectWithSite != null)
			{
				Marshal.ReleaseComObject(objectWithSite);
			}
		}
	}

	public bool IsPackageRegisteredForCurrentUser(string manifestPath)
	{
		if (string.IsNullOrEmpty(manifestPath) || !PathHelper.FileExists(manifestPath))
		{
			return false;
		}
		AppPackageNativeMethods.IAppxManifestPackageId appxManifestPackageId = null;
		try
		{
			appxManifestPackageId = GetPackageId(manifestPath);
			if (appxManifestPackageId == null)
			{
				return false;
			}
			appxManifestPackageId.GetName(out var name);
			appxManifestPackageId.GetPublisher(out var publisher);
			appxManifestPackageId.GetArchitecture(out var packageArchitecture);
			appxManifestPackageId.GetVersion(out var rawVersion);
			string stringSidForCurrentUser = GetStringSidForCurrentUser();
			IEnumerable<global::Windows.ApplicationModel.Package> source = PackageManager.FindPackagesForUser(stringSidForCurrentUser, name, publisher);
			return source.Any((global::Windows.ApplicationModel.Package p) => p.Id.Architecture == packageArchitecture && IsGreaterOrEqualVersion(p.Id.Version, rawVersion));
		}
		finally
		{
			if (appxManifestPackageId != null)
			{
				Marshal.ReleaseComObject(appxManifestPackageId);
			}
		}
	}

	public bool IsPackageMonikerRegisteredToCurrentUser(string packageMoniker, string directory = null)
	{
		string stringSidForCurrentUser = GetStringSidForCurrentUser();
		IEnumerable<global::Windows.ApplicationModel.Package> source = PackageManager.FindPackagesForUser(stringSidForCurrentUser);
		return source.Any((global::Windows.ApplicationModel.Package package) => package.Id != null && string.Equals(package.Id.FullName, packageMoniker, StringComparison.OrdinalIgnoreCase) && (directory == null || PathHelperBase.ArePathsEquivalent(package.InstalledLocation.Path, directory)));
	}

	public IEnumerable<AppPackageInfo> GetInstalledPackageLocationsForUser()
	{
		string stringSidForCurrentUser = GetStringSidForCurrentUser();
		IEnumerable<global::Windows.ApplicationModel.Package> enumerable = PackageManager.FindPackagesForUser(stringSidForCurrentUser);
		List<AppPackageInfo> list = new List<AppPackageInfo>();
		foreach (global::Windows.ApplicationModel.Package item in enumerable)
		{
			string installLocation = string.Empty;
			try
			{
				installLocation = item.InstalledLocation.Path;
			}
			catch (FileNotFoundException)
			{
			}
			if (PackageWasCreatedByDesigner(item))
			{
				list.Add(new AppPackageInfo(item.Id.FullName, installLocation));
			}
		}
		return list;
	}

	private static bool PackageWasCreatedByDesigner(global::Windows.ApplicationModel.Package package)
	{
		string name = package.Id.Name;
		if (name.Length <= "app.a".Length)
		{
			return false;
		}
		string input = name.Substring("app.a".Length);
		if (name.StartsWith("app.a") && Guid.TryParse(input, out var _) && package.Id.Publisher == "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US")
		{
			return package.IsDevelopmentMode;
		}
		return false;
	}

	public Exception RegisterPackage(string manifestFilePath, bool removeExistingPackages = false)
	{
		return RegisterPackage(manifestFilePath, CancellationToken.None, removeExistingPackages);
	}

	public Exception RegisterPackage(string manifestFilePath, CancellationToken cancelToken, bool removeExistingPackages)
	{
		long registerTime;
		long unregisterTime;
		return RegisterPackageTimed(manifestFilePath, cancelToken, removeExistingPackages, out registerTime, out unregisterTime);
	}

	public Exception RegisterPackageTimed(string manifestFilePath, CancellationToken cancelToken, bool removeExistingPackages, out long registerTime, out long unregisterTime)
	{
		int num = (int)TimeSpan.FromSeconds(RegistrationTimeoutInSecond).TotalMilliseconds;
		unregisterTime = 0L;
		if (removeExistingPackages)
		{
			RemoveExistingPackages(manifestFilePath, num, out unregisterTime, cancelToken);
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Exception e = null;
		DeployedOperationHandle deployedOperationHandle = RegisterPackageAsync(manifestFilePath, delegate
		{
		}, delegate(Exception exception)
		{
			e = exception;
		});
		try
		{
			if (deployedOperationHandle.Exception == null)
			{
				int millisecondsTimeout = ((cancelToken == CancellationToken.None) ? (-1) : num);
				this.WaitingOnPackageManager?.Invoke(this, null);
				int num2 = WaitHandle.WaitAny(new WaitHandle[2] { deployedOperationHandle.WaitHandle, cancelToken.WaitHandle }, millisecondsTimeout);
				if (num2 == 1 || num2 == 258)
				{
					if (num2 == 258)
					{
						deployedOperationHandle.Exception = new TimeoutException(string.Format(CultureInfo.CurrentCulture, StringTable.RegisterPackageTimeoutMessage, manifestFilePath));
					}
					else
					{
						if (cancelToken.IsCancellationRequested)
						{
							if (deployedOperationHandle.DeploymentOperation != null)
							{
								deployedOperationHandle.DeploymentOperation.Cancel();
								lock (syncLock)
								{
									pendingDeploymentOperations.Remove(manifestFilePath);
								}
							}
							throw new OperationCanceledException(cancelToken);
						}
						deployedOperationHandle.Exception = new InvalidOperationException();
					}
				}
			}
		}
		finally
		{
			deployedOperationHandle.Release();
		}
		registerTime = stopwatch.ElapsedMilliseconds;
		return deployedOperationHandle.Exception;
	}

	private void RemoveExistingPackages(string manifestFilePath, int registrationTimeLimit, out long unregisterTime, CancellationToken cancelToken)
	{
		unregisterTime = 0L;
		lock (syncLock)
		{
			AppPackageNativeMethods.IAppxManifestPackageId appxManifestPackageId = null;
			try
			{
				appxManifestPackageId = GetPackageId(manifestFilePath);
				appxManifestPackageId.GetName(out var name);
				appxManifestPackageId.GetPublisher(out var publisher);
				string stringSidForCurrentUser = GetStringSidForCurrentUser();
				Queue<IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress>> queue = new Queue<IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress>>();
				IEnumerable<global::Windows.ApplicationModel.Package> enumerable = PackageManager.FindPackagesForUser(stringSidForCurrentUser, name, publisher);
				foreach (global::Windows.ApplicationModel.Package item in enumerable)
				{
					queue.Enqueue(PackageManager.RemovePackageAsync(item.Id.FullName));
				}
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				while (queue.Count > 0)
				{
					cancelToken.ThrowIfCancellationRequested();
					IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> asyncOperationWithProgress = queue.Dequeue();
					if (asyncOperationWithProgress.Status == AsyncStatus.Started)
					{
						if (stopwatch.ElapsedMilliseconds > registrationTimeLimit)
						{
							throw new TimeoutException(string.Format(CultureInfo.CurrentCulture, StringTable.RemoveExistingPackageTimeoutMessage, name));
						}
						queue.Enqueue(asyncOperationWithProgress);
					}
					Thread.Sleep(10);
				}
				stopwatch.Stop();
				unregisterTime = stopwatch.ElapsedMilliseconds;
			}
			finally
			{
				if (appxManifestPackageId != null)
				{
					Marshal.ReleaseComObject(appxManifestPackageId);
				}
			}
		}
	}

	private DeployedOperationHandle RegisterPackageAsync(string manifestFilePath, Action<string, bool> onCompleted, Action<Exception> onException = null)
	{
		bool isAppXPackage = PathHelperBase.HasExtension(manifestFilePath, ".appx", ".msix");
		if (isAppXPackage)
		{
			try
			{
				X509Certificate2 certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(manifestFilePath));
				X509Store x509Store;
				using (x509Store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine))
				{
					x509Store.Open(OpenFlags.ReadWrite);
					if (!x509Store.Certificates.Contains(certificate))
					{
						x509Store.Add(certificate);
					}
					x509Store.Close();
				}
			}
			catch (CryptographicException)
			{
			}
		}
		DeployedOperationHandle operationHandle;
		lock (syncLock)
		{
			if (pendingDeploymentOperations.TryGetValue(manifestFilePath, out operationHandle))
			{
				operationHandle.AddRef();
				return operationHandle;
			}
			operationHandle = new DeployedOperationHandle(new ManualResetEvent(initialState: false));
			try
			{
				if (isAppXPackage)
				{
					operationHandle.DeploymentOperation = PackageManager.AddPackageAsync(new Uri(manifestFilePath));
				}
				else
				{
					operationHandle.DeploymentOperation = PackageManager.RegisterPackageAsync(new Uri(manifestFilePath));
				}
			}
			catch (Exception ex2)
			{
				operationHandle.WaitHandle.Set();
				onException?.Invoke(ex2);
				operationHandle.Exception = ex2;
				return operationHandle;
			}
			pendingDeploymentOperations.Add(manifestFilePath, operationHandle);
			operationHandle.AddRef();
		}
		operationHandle.DeploymentOperation.Completed = delegate
		{
			bool flag = false;
			string text = null;
			try
			{
				try
				{
					lock (syncLock)
					{
						pendingDeploymentOperations.Remove(manifestFilePath);
					}
					if (!isAppXPackage)
					{
						text = GetPackageMoniker(manifestFilePath);
					}
					operationHandle.Exception = GetExceptionFromAsyncOperationResult(operationHandle.DeploymentOperation);
					flag = operationHandle.Exception == null;
					onCompleted?.Invoke(text, flag);
					if (operationHandle.Exception != null)
					{
						throw operationHandle.Exception;
					}
				}
				catch (Exception exception)
				{
					operationHandle.Exception = exception;
				}
				if (operationHandle.Exception != null)
				{
					onException?.Invoke(operationHandle.Exception);
				}
				if (flag && !string.IsNullOrEmpty(text))
				{
					EnsureNoPlmShutdown(text);
				}
			}
			finally
			{
				try
				{
					operationHandle.WaitHandle.Set();
				}
				catch (ObjectDisposedException)
				{
				}
			}
		};
		return operationHandle;
	}

	public void UnregisterPackage(string appPackageMoniker, Action<Exception> onCompleted = null, bool waitForCompletion = false)
	{
		WaitHandle waitHandle = UnregisterPackageAsync(appPackageMoniker, onCompleted, waitForCompletion || onCompleted != null);
		if (waitHandle != null)
		{
			if (waitForCompletion)
			{
				WaitForCompletion(waitHandle);
				return;
			}
			Thread thread = new Thread(WaitForCompletion);
			thread.Name = "UnregisterPackage Completion Waiter";
			thread.Start(waitHandle);
		}
	}

	public WaitHandle UnregisterPackageAsync(string appPackageMoniker, Action<Exception> onCompleted = null, bool needWaitHandle = true)
	{
		try
		{
			if (!IsPackageMonikerRegisteredToCurrentUser(appPackageMoniker))
			{
				if (onCompleted != null)
				{
					onCompleted(null);
				}
				return null;
			}
		}
		catch (COMException)
		{
		}
		ManualResetEvent waitHandle;
		IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> asyncOperationWithProgress;
		lock (syncLock)
		{
			if (pendingDeploymentOperations.TryGetValue(appPackageMoniker, out var value))
			{
				return value.WaitHandle;
			}
			waitHandle = (needWaitHandle ? new ManualResetEvent(initialState: false) : null);
			asyncOperationWithProgress = PackageManager.RemovePackageAsync(appPackageMoniker);
			pendingDeploymentOperations.Add(appPackageMoniker, new DeployedOperationHandle(waitHandle, asyncOperationWithProgress));
		}
		asyncOperationWithProgress.Completed = delegate(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> result, AsyncStatus status)
		{
			lock (syncLock)
			{
				pendingDeploymentOperations.Remove(appPackageMoniker);
			}
			Exception exceptionFromAsyncOperationResult = GetExceptionFromAsyncOperationResult(result);
			if (onCompleted != null)
			{
				onCompleted(exceptionFromAsyncOperationResult);
			}
			if (waitHandle != null)
			{
				waitHandle.Set();
			}
		};
		return waitHandle;
	}

	[Conditional("DEBUG")]
	public static void EnableElevatedActivationIfNeeded()
	{
		if (!OSHelper.IsRunningElevated)
		{
			return;
		}
		using Process process = Process.GetCurrentProcess();
		string fileName = process.MainModule.FileName;
		string fileName2 = Path.GetFileName(fileName);
		string text = "{AB7DC0D3-451E-42C2-8F12-81E85D050E9D}";
		string subkeyName = "SOFTWARE\\Classes\\AppID\\" + fileName2;
		string subkeyName2 = "SOFTWARE\\Classes\\AppID\\" + text;
		byte[] array = RegistryHelper.RetrieveLocalMachineRegistryValue<byte[]>(subkeyName2, "AccessPermission");
		string a = RegistryHelper.RetrieveLocalMachineRegistryValue<string>(subkeyName, "AppID");
		if (!string.Equals(a, text, StringComparison.OrdinalIgnoreCase) || array == null || array.Length < 20)
		{
			array = new byte[168]
			{
				1, 0, 4, 128, 136, 0, 0, 0, 152, 0,
				0, 0, 0, 0, 0, 0, 20, 0, 0, 0,
				2, 0, 116, 0, 5, 0, 0, 0, 0, 0,
				20, 0, 7, 0, 0, 0, 1, 1, 0, 0,
				0, 0, 0, 5, 10, 0, 0, 0, 0, 0,
				20, 0, 3, 0, 0, 0, 1, 1, 0, 0,
				0, 0, 0, 5, 18, 0, 0, 0, 0, 0,
				24, 0, 7, 0, 0, 0, 1, 2, 0, 0,
				0, 0, 0, 5, 32, 0, 0, 0, 32, 2,
				0, 0, 0, 0, 20, 0, 3, 0, 0, 0,
				1, 1, 0, 0, 0, 0, 0, 5, 11, 0,
				0, 0, 0, 0, 24, 0, 3, 0, 0, 0,
				1, 2, 0, 0, 0, 0, 0, 15, 2, 0,
				0, 0, 1, 0, 0, 0, 1, 2, 0, 0,
				0, 0, 0, 5, 32, 0, 0, 0, 32, 2,
				0, 0, 1, 2, 0, 0, 0, 0, 0, 5,
				32, 0, 0, 0, 32, 2, 0, 0
			};
			RegistryHelper.SetRegistryValue(Registry.LocalMachine, subkeyName, "AppID", text, RegistryValueKind.String);
			RegistryHelper.SetRegistryValue(Registry.LocalMachine, subkeyName2, "AuthenticationLevel", 0u, RegistryValueKind.DWord);
			RegistryHelper.SetRegistryValue(Registry.LocalMachine, subkeyName2, "AccessPermission", array, RegistryValueKind.Binary);
			throw new Exception("Elevated activation is enabled for " + fileName2 + ". You need to restart process.");
		}
	}

	public string GetAppUserModelIdFromManifest(string manifest)
	{
		IStream stream = null;
		AppPackageNativeMethods.IAppxFactory appxFactory = null;
		AppPackageNativeMethods.IAppxManifestReader manifestReader = null;
		AppPackageNativeMethods.IAppxManifestApplicationsEnumerator applications = null;
		AppPackageNativeMethods.IAppxManifestApplication application = null;
		try
		{
			string appUserModelId = null;
			AppPackageNativeMethods.SHCreateStreamOnFile(manifest, AppPackageNativeMethods.StgmConstants.STGM_READ, ref stream);
			appxFactory = (AppPackageNativeMethods.IAppxFactory)new AppPackageNativeMethods.AppxFactory();
			appxFactory.CreateManifestReader(stream, out manifestReader);
			manifestReader.GetApplications(out applications);
			bool hasCurrent = false;
			applications.GetHasCurrent(out hasCurrent);
			while (hasCurrent)
			{
				applications.GetCurrent(out application);
				if (!string.IsNullOrEmpty(appUserModelId))
				{
					throw new InvalidOperationException("More than one app user model id");
				}
				application.GetAppUserModelId(out appUserModelId);
				applications.MoveNext(out hasCurrent);
			}
			return appUserModelId;
		}
		finally
		{
			if (stream != null)
			{
				Marshal.ReleaseComObject(stream);
			}
			if (appxFactory != null)
			{
				Marshal.ReleaseComObject(appxFactory);
			}
			if (manifestReader != null)
			{
				Marshal.ReleaseComObject(manifestReader);
			}
			if (applications != null)
			{
				Marshal.ReleaseComObject(applications);
			}
			if (application != null)
			{
				Marshal.ReleaseComObject(application);
			}
		}
	}

	public bool IsNewPackage(string packageName)
	{
		if (!string.IsNullOrEmpty(packageName))
		{
			//return packageName.Equals("20d757be-9681-45b1-bb28-39ed843dae99");
			return packageName.Equals("798368a0-000f-4e51-899b-112a31c2dbb6");
		}
		return true;
	}

	public string CreateNewPackageName()
	{
		return "app.a" + Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
	}

	public virtual string GetPackageMoniker(string fileName)
	{
		return GetPackageProperty(fileName, delegate(AppPackageNativeMethods.IAppxManifestPackageId packageId)
		{
			packageId.GetPackageFullName(out var packageFullName);
			return packageFullName;
		});
	}

	public string GetPackageName(string fileName)
	{
		return GetPackageProperty(fileName, delegate(AppPackageNativeMethods.IAppxManifestPackageId packageId)
		{
			packageId.GetName(out var name);
			return name;
		});
	}

	public string GetPackageFamilyName(string fileName)
	{
		return GetPackageProperty(fileName, delegate(AppPackageNativeMethods.IAppxManifestPackageId packageId)
		{
			packageId.GetPackageFamilyName(out var packageFamilyName);
			return packageFamilyName;
		});
	}

	public string GetPackageArchitecture(string fileName)
	{
		return GetPackageProperty(fileName, delegate(AppPackageNativeMethods.IAppxManifestPackageId packageId)
		{
			packageId.GetArchitecture(out var architecture);
			return Enum.GetName(typeof(ProcessorArchitecture), architecture);
		});
	}

	private AppPackageNativeMethods.IAppxManifestPackageId GetPackageId(string fileName)
	{
		IStream stream = null;
		AppPackageNativeMethods.IAppxFactory appxFactory = null;
		AppPackageNativeMethods.IAppxPackageReader packageReader = null;
		AppPackageNativeMethods.IAppxManifestReader manifestReader = null;
		AppPackageNativeMethods.IAppxManifestPackageId packageId = null;
		try
		{
			AppPackageNativeMethods.SHCreateStreamOnFile(fileName, AppPackageNativeMethods.StgmConstants.STGM_READ, ref stream);
			appxFactory = (AppPackageNativeMethods.IAppxFactory)new AppPackageNativeMethods.AppxFactory();
			if (PathHelperBase.HasExtension(fileName, ".appx", ".msix"))
			{
				appxFactory.CreatePackageReader(stream, out packageReader);
				packageReader.GetManifest(out manifestReader);
			}
			else
			{
				appxFactory.CreateManifestReader(stream, out manifestReader);
			}
			manifestReader.GetPackageId(out packageId);
			return packageId;
		}
		finally
		{
			if (stream != null)
			{
				Marshal.ReleaseComObject(stream);
			}
			if (appxFactory != null)
			{
				Marshal.ReleaseComObject(appxFactory);
			}
			if (packageReader != null)
			{
				Marshal.ReleaseComObject(packageReader);
			}
			if (manifestReader != null)
			{
				Marshal.ReleaseComObject(manifestReader);
			}
		}
	}

	public void DisablePackageDebugging(string packageMoniker)
	{
		AppPackageNativeMethods.PackageDebugSettings packageDebugSettings = new AppPackageNativeMethods.PackageDebugSettings();
		AppPackageNativeMethods.IPackageDebugSettings packageDebugSettings2 = (AppPackageNativeMethods.IPackageDebugSettings)packageDebugSettings;
		packageDebugSettings2.DisableDebugging(packageMoniker);
	}

	public void SetPackageDebugging(string packageMoniker, string debuggerCommandLine, string environment)
	{
		AppPackageNativeMethods.PackageDebugSettings packageDebugSettings = new AppPackageNativeMethods.PackageDebugSettings();
		AppPackageNativeMethods.IPackageDebugSettings packageDebugSettings2 = (AppPackageNativeMethods.IPackageDebugSettings)packageDebugSettings;
		packageDebugSettings2.EnableDebugging(packageMoniker, debuggerCommandLine, environment);
	}

	public virtual void EnsureNoPlmShutdown(string packageMoniker)
	{
		SetPackageDebugging(packageMoniker, null, null);
	}

	private static Exception GetExceptionFromAsyncOperationResult(IAsyncInfo asyncInfo)
	{
		Exception ex = null;
		try
		{
			ex = asyncInfo.ErrorCode;
		}
		catch (COMException ex2)
		{
			if (ex2.ErrorCode != -2147417846)
			{
				throw;
			}
		}
		if (Marshal.GetHRForException(ex) == 0)
		{
			return null;
		}
		return ex;
	}

	private static bool IsGreaterOrEqualVersion(PackageVersion version, ulong rawVersion)
	{
		ulong num = ((ulong)version.Major << 48) | ((ulong)version.Minor << 32) | ((ulong)version.Build << 16) | version.Revision;
		return num >= rawVersion;
	}

	private static void WaitForCompletion(object waitHandleArg)
	{
		WaitHandle waitHandle = (WaitHandle)waitHandleArg;
		using (waitHandle)
		{
			waitHandle.WaitOne();
		}
	}

	private string GetPackageProperty(string fileName, Func<AppPackageNativeMethods.IAppxManifestPackageId, string> propertyAccess)
	{
		AppPackageNativeMethods.IAppxManifestPackageId appxManifestPackageId = null;
		try
		{
			appxManifestPackageId = GetPackageId(fileName);
			return propertyAccess(appxManifestPackageId);
		}
		finally
		{
			if (appxManifestPackageId != null)
			{
				Marshal.ReleaseComObject(appxManifestPackageId);
			}
		}
	}

	private string GetStringSidForCurrentUser()
	{
		WindowsIdentity current = WindowsIdentity.GetCurrent(ifImpersonating: false);
		SecurityIdentifier user = current.User;
		return user.ToString();
	}

	public bool IsRegisteredManifestDifferent(string manifestPath)
	{
		string path = manifestPath + ".registered";
		if (!File.Exists(manifestPath) || !File.Exists(path))
		{
			return true;
		}
		string text = File.ReadAllText(manifestPath);
		string value = File.ReadAllText(path);
		return !text.Equals(value);
	}
}
