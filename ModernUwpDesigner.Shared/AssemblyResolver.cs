using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ModernUwpDesigner.Shared
{
	internal static class AssemblyRedirectModuleInitializer
	{
		private static bool _eventRegistered;
		private static string _vsDir;

		[ModuleInitializer]
		public static void Init()
		{
			if (_eventRegistered) return;
			_eventRegistered = true;

			var domain = AppDomain.CurrentDomain;

			_vsDir = domain.BaseDirectory;
			domain.AssemblyResolve += OnAssemblyResolve;
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string path;
			var requestedName = new AssemblyName(args.Name);

			if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.DesignerContractBase", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.DesignerContractBase.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.DesignerContract", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.DesignerContract.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.DesignerHost", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.DesignerHost.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.Markup", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.Markup.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.Utility", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.Utility.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.UtilityBase", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.UtilityBase.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.XamlLanguageServiceBase", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.XamlLanguageServiceBase.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.Telemetry", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.Telemetry.dll");
			}
			else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner", StringComparison.OrdinalIgnoreCase))
			{
				path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.dll");
			}
            else if (requestedName.Name.Equals("Microsoft.VisualStudio.DesignTools.SurfaceDesigner", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.Combine(_vsDir, "PrivateAssemblies\\Microsoft.VisualStudio.DesignTools.SurfaceDesigner.dll");
            }
            else if (requestedName.Name.Equals("Microsoft.VisualStudio.Shell.Interop", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.Combine(_vsDir, "PublicAssemblies\\Microsoft.VisualStudio.Shell.Interop.dll");
            }
            else if (requestedName.Name.Equals("Microsoft.VisualStudio.Shell.Framework", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.Combine(_vsDir, "PublicAssemblies\\Microsoft.VisualStudio.Shell.Framework.dll");
            }
            else if (requestedName.Name.Equals("Microsoft.VisualStudio.Threading", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.Combine(_vsDir, "PublicAssemblies\\Microsoft.VisualStudio.Threading.17.x\\Microsoft.VisualStudio.Threading.dll");
            }
            else if (requestedName.Name.Equals("Microsoft.VisualStudio.Shell", StringComparison.OrdinalIgnoreCase) ||
                     requestedName.Name.Equals("Microsoft.VisualStudio.Shell.15.0", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.Combine(_vsDir, "PublicAssemblies\\Microsoft.VisualStudio.Shell.15.0.dll");
            }
            else
			{
				return null;
			}

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.GetName().Name.Equals(requestedName.Name, StringComparison.OrdinalIgnoreCase))
				{
					return assembly;
				}
			}

			try
			{
				return Assembly.LoadFrom(path);
			}
			catch { }

			return null;
		}
	}
}
