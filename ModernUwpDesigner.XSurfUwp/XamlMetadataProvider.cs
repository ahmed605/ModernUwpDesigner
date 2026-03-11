using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Markup;
using WinRT;

using Activator = System.Activator;

namespace XSurfUwp
{
    internal sealed partial class XamlMetadataProvider : IXamlMetadataProvider
    {
        private UserApplicationInfo userApplicationInfo;
        private object userAppTypeInfoProvider;
        private IXamlMetadataProvider xamlMetadataProvider;
        private bool metaDataRetrieveAttempted;

        private unsafe delegate*<object, Type, IXamlType> GetXamlTypeByType;
        private unsafe delegate*<object, string, IXamlType> GetXamlTypeByName;

        public unsafe IXamlType GetXamlType(Type type)
        {
            EnsureUserAppIXMP();

            if (xamlMetadataProvider is not null)
            {
                return xamlMetadataProvider.GetXamlType(type);
            }

            if (GetXamlTypeByType is not null)
            {
                return GetXamlTypeByType(userAppTypeInfoProvider, type);
            }

            return null;
        }

        public unsafe IXamlType GetXamlType(string fullName)
        {
            EnsureUserAppIXMP();

            if (xamlMetadataProvider is not null)
            {
                return xamlMetadataProvider.GetXamlType(fullName);
            }

            if (GetXamlTypeByName is not null)
            {
                return GetXamlTypeByName(userAppTypeInfoProvider, fullName);
            }

            return null;
        }

        public XmlnsDefinition[] GetXmlnsDefinitions()
        {
            if (xamlMetadataProvider is not null)
            {
                return xamlMetadataProvider.GetXmlnsDefinitions();
            }

            return Array.Empty<XmlnsDefinition>();
        }

        private UserApplicationInfo GetUserApplicationInfo()
        {
            if (userApplicationInfo == null)
            {
                StorageFile storageFile = null;

                try
                {
                    StorageFolder installedLocation = Package.Current.InstalledLocation;
                    IAsyncOperation<StorageFile> fileAsync = installedLocation.GetFileAsync("UserApplicationInfo.txt");
                    fileAsync.AsTask().Wait();
                    storageFile = fileAsync.GetResults();
                }
                catch (Exception ex) when (ex is AggregateException || ex is FileNotFoundException)
                {

                }

                if (storageFile != null)
                {
                    IAsyncOperation<string> asyncOperation = FileIO.ReadTextAsync(storageFile);
                    asyncOperation.AsTask().Wait();
                    string results = asyncOperation.GetResults();
                    if (results != null)
                    {
                        SetUserApplicationInfoFromArgs(results);
                    }
                }
            }

            return userApplicationInfo;
        }

        internal void SetUserApplicationInfoFromArgs(string args)
        {
            if (!string.IsNullOrEmpty(args))
            {
                string[] array = args.Split(';', (StringSplitOptions)0);
                if (array.Length == 3)
                {
                    userApplicationInfo = new UserApplicationInfo
                    {
                        UserApplicationProjectName = array[0],
                        UserApplicationFullAssemblyName = array[1],
                        UserApplicationRootNamespace = array[2]
                    };
                }
            }
        }

        private unsafe void EnsureUserAppIXMP()
        {
            if (metaDataRetrieveAttempted)
                return;

            metaDataRetrieveAttempted = true;

            UserApplicationInfo userApplicationInfo = GetUserApplicationInfo();
            if (userApplicationInfo is null)
                return;

            string className = userApplicationInfo.UserApplicationRootNamespace + "." + userApplicationInfo.UserApplicationProjectName + "_XamlTypeInfo.XamlMetaDataProvider";
            xamlMetadataProvider = LoadUserAppIXMP(userApplicationInfo.UserApplicationFullAssemblyName, className);
            if (xamlMetadataProvider is null)
            {
                string className2 = userApplicationInfo.UserApplicationRootNamespace + ".XamlMetaDataProvider";
                xamlMetadataProvider = LoadUserAppIXMP(userApplicationInfo.UserApplicationFullAssemblyName, className2);
                if (xamlMetadataProvider is null)
                {
                    string typeName = userApplicationInfo.UserApplicationRootNamespace + "." + userApplicationInfo.UserApplicationProjectName + "_XamlTypeInfo.XamlTypeInfoProvider";
                    userAppTypeInfoProvider = LoadUserAppIXMPReflection(userApplicationInfo.UserApplicationFullAssemblyName, typeName);
                    if (userAppTypeInfoProvider is not null)
                    {
                        GetXamlTypeByType = (delegate*<object, Type, IXamlType>)userAppTypeInfoProvider.GetType().GetMethod(nameof(GetXamlTypeByType)).MethodHandle.GetFunctionPointer();
                        GetXamlTypeByName = (delegate*<object, string, IXamlType>)userAppTypeInfoProvider.GetType().GetMethod(nameof(GetXamlTypeByName)).MethodHandle.GetFunctionPointer();
                    }
                }
            }
        }

        private static IXamlMetadataProvider LoadUserAppIXMP(string assemblyName, string className)
        {
            try
            {
                var factory = (WinRT.Interop.IActivationFactory)ActivationFactory.Get(className);
                object instance = factory.ActivateInstance();
                return instance as IXamlMetadataProvider;
            }
            catch
            {
                return (IXamlMetadataProvider)LoadUserAppIXMPReflection(assemblyName, className);
            }
        }

        // TODO: Cleanup
        private static object LoadUserAppIXMPReflection(string assemblyName, string typeName)
        {
            try
            {
                Assembly val = Assembly.Load(new AssemblyName(assemblyName));
                return Activator.CreateInstance(val.GetType(typeName));
            }
            catch
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom($"{assemblyName.Split(',').FirstOrDefault()}.dll");
                    return Activator.CreateInstance(assembly.GetType(typeName));
                }
                catch
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom($"{assemblyName.Split(',').FirstOrDefault()}.Projection.dll");
                        return Activator.CreateInstance(assembly.GetType(typeName));
                    }
                    catch { }
                }

                return null;
            }
        }
    }
}
