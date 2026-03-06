using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.ShadowCopy;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.ShadowCopy;

public class UwpShadowCopyWorkerFactory : IShadowCopyWorkerFactory
{
	private List<Type> shadowCopyTypes;

	private readonly UwpHostPlatform platform;

	public UwpShadowCopyWorkerFactory(UwpHostPlatform platform)
	{
		this.platform = platform;
		shadowCopyTypes = new List<Type>
		{
			typeof(AppxNativeShadowCopyWorker),
			typeof(AppxRecipeShadowCopyWorker),
			typeof(AppxPlatformOnlyShadowCopyWorker)
		};
	}

	public IHostShadowCopyWorker CreateWorker(SurfaceProcessInfo surfaceProcessInfo, IHostProject hostProject, IHostTelemetryService hostTelemetryService, IEnumerable<string> controlAssembliesForShadowCopy)
	{
		IHostShadowCopyWorker result = null;
		foreach (Type shadowCopyType in shadowCopyTypes)
		{
			UwpHostShadowCopyWorker uwpHostShadowCopyWorker = (UwpHostShadowCopyWorker)Activator.CreateInstance(shadowCopyType);
			if (uwpHostShadowCopyWorker.Initialize(platform, hostProject, platform.AppPackageHelper, surfaceProcessInfo, hostTelemetryService))
			{
				return uwpHostShadowCopyWorker;
			}
		}
		return result;
	}
}
