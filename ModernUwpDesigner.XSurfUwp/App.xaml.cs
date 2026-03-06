// TODO: Cleanup

using Microsoft.System;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using WinRT;
using XSurfUwp.XSurfUwp_XamlTypeInfo;
using Activator = System.Activator;

namespace XSurfUwp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
    /// </summary>
    public sealed partial class Application : Windows.UI.Xaml.Application, ITapBridge, IXamlMetadataProvider
    {
        private ThreadLocal<ThreadLocalApp> localApp = new(() => new ThreadLocalApp());

        private UserApplicationInfo userApplicationInfo;

        private XamlTypeInfoProvider _provider;

        private object userAppTypeInfoProvider;

        private IXamlMetadataProvider xamlMetadataProvider;

        private bool metaDataRetrieveAttempted;

        private bool dispatcherInitialized;

        public new static Application Current => (Application)Windows.UI.Xaml.Application.Current;

        internal ContentWrapper ContentWrapper => (ContentWrapper)Window.Current.Content;

#nullable disable
        public ThreadLocalApp Local => localApp.Value;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public Application()
        {
            //MessageBox(0, "Attach", "Attach", 0);

            InitializeComponent();

            Suspending += OnSuspending;
        }
#nullable restore

        static Application()
        {
            SuppressAggressiveDebugAssert();
            ReadProjectInfo();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            SetUserApplicationInfoFromArgs(e.Arguments);
            if (!e.PrelaunchActivated)
            {
                var window = Window.Current;
                window.Content = new ContentWrapper();
                window.Activate();

                SynchronizationContext.SetSynchronizationContext(new CoreDispatcherSynchronizationContext(window.Dispatcher));
            }
        }

        private void SetUserApplicationInfoFromArgs(string args)
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

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        [DynamicWindowsRuntimeCast(typeof(Page))]
        [DynamicWindowsRuntimeCast(typeof(UIElement))]
        public unsafe void RegisterContent(object content)
        {
            ContentWrapper.SetContent((UIElement)content);
            if (content is Page dependencyObject)
            {
                SetBottomAppBar(DT.GetRuntimeBottomAppBar(dependencyObject));
                SetTopAppBar(DT.GetRuntimeTopAppBar(dependencyObject));
            }

            Window current = Window.Current;
            current.SizeChanged += Window_SizeChanged;
        }

        public unsafe void ClearContent()
        {
            ContentWrapper?.ClearContent();
            Window current = Window.Current;
            current.SizeChanged -= Window_SizeChanged;
        }

        public object GetHitTestRoot()
        {
#nullable disable
            return ContentWrapper?.ContentHolder.Child;
#nullable restore
        }

        public void SetBottomAppBar(AppBar appBar)
        {
        }

        public void SetTopAppBar(AppBar appBar)
        {
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            FrameworkElement? frameworkElement = ContentWrapper?.BackgroundElement;
            if (frameworkElement != null)
            {
                frameworkElement.Width = e.Size.Width;
                frameworkElement.Height = e.Size.Height;
            }
        }

        [DynamicWindowsRuntimeCast(typeof(CompositeTransform))]
        public void SetPanZoomTransform(double scale, double offsetX, double offsetY)
        {
            if (ContentWrapper is not null)
            {
                FrameworkElement? frameworkElement = ContentWrapper.PanZoom;
                if (frameworkElement?.RenderTransform is CompositeTransform compositeTransform)
                {
                    frameworkElement.Opacity = 1.0;
                    if (compositeTransform.ScaleX != scale)
                    {
                        ContentWrapper.HairlineBorder.StrokeThickness = 1.0 / scale;
                    }
                    compositeTransform.ScaleX = scale;
                    compositeTransform.ScaleY = scale;
                    compositeTransform.TranslateX = offsetX;
                    compositeTransform.TranslateY = offsetY;
                }
            }
        }

        [DynamicWindowsRuntimeCast(typeof(Rectangle))]
        public void SetFlatBackground(string checkerboardColor)
        {
            if (ContentWrapper?.BackgroundElement is Rectangle rectangle)
            {
                rectangle.Fill = new SolidColorBrush((Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), checkerboardColor));
            }
        }

        public void SetContentBorder(string contentBorder)
        {
            Rectangle? rectangle = ContentWrapper?.HairlineBorder;
            if (rectangle != null)
            {
                rectangle.Stroke = new SolidColorBrush((Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), contentBorder));
            }
        }

        [DynamicWindowsRuntimeCast(typeof(UIElement))]
        [DynamicWindowsRuntimeCast(typeof(DependencyObject))]
        public void SetRootSize(object obj, double width, double height)
        {
            if (obj is DependencyObject dependencyObject)
            {
                DT.SetRootWidth(dependencyObject, width);
                DT.SetRootHeight(dependencyObject, height);
            }
            Local.DeviceSize = new Windows.Foundation.Size(width, height);
            if (obj is UIElement uIElement)
            {
                uIElement.UpdateLayout();
            }
        }

        public bool TrySerialize(object obj, out string typeName, out string value, out bool isValueType)
        {
#nullable disable
            System.Type type = obj?.GetType();
            if (type == typeof(DependencyObject))
            {
                typeName = string.Empty;
                value = string.Empty;
                isValueType = false;
                return false;
            }
            TypeInfo val = ((type != null) ? IntrospectionExtensions.GetTypeInfo(type) : null);
            if (val != null && ((System.Type)(object)val).IsEnum)
            {
                typeName = ((System.Type)(object)val).FullName;
                value = obj.ToString();
                isValueType = true;
                return true;
            }
            if (val != null && !((System.Type)(object)val).IsValueType && !(obj is string))
            {
                typeName = ((System.Type)(object)val).FullName;
                value = string.Empty;
                isValueType = false;
                return true;
            }
            if (obj is System.DateTime)
            {
                typeName = ((System.Type)(object)val).FullName;
                value = ((System.DateTime)obj).ToString("o");
                isValueType = true;
                return true;
            }
            typeName = string.Empty;
            value = string.Empty;
            isValueType = false;
            return false;
#nullable restore
        }

        private static void SuppressAggressiveDebugAssert()
        {
            try
            {
                System.Type typeFromHandle = typeof(Debug);
                TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(typeFromHandle);
                System.Collections.Generic.IEnumerator<FieldInfo> enumerator = typeInfo.DeclaredFields.GetEnumerator();
                try
                {
                    while (((System.Collections.IEnumerator)enumerator).MoveNext())
                    {
                        FieldInfo current = enumerator.Current;
                        if (((MemberInfo)current).Name == "s_ShowAssertDialog")
                        {
                            Action<string, string, string> val = delegate
                            {
                            };
                            current.SetValue((object?)null, (object)val);
                        }
                        else if (((MemberInfo)current).Name == "s_ShowDialog")
                        {
                            Action<string, string, string, string> val2 = delegate
                            {
                            };
                            current.SetValue((object?)null, (object)val2);
                        }
                    }
                }
                finally
                {
                    ((System.IDisposable)enumerator)?.Dispose();
                }
            }
            catch
            {
            }
        }

        private static void ReadProjectInfo()
        {
            StorageFile? storageFile = null;
            try
            {
                StorageFolder installedLocation = Package.Current.InstalledLocation;
                IAsyncOperation<StorageFile> fileAsync = installedLocation.GetFileAsync("__ProjectInfo__.txt");
                ((System.Threading.Tasks.Task)WindowsRuntimeSystemExtensions.AsTask<StorageFile>(fileAsync)).Wait();
                storageFile = fileAsync.GetResults();
            }
            catch (System.Exception)
            {
                storageFile = null;
            }
            if (storageFile != null)
            {
                IAsyncOperation<string> asyncOperation = FileIO.ReadTextAsync(storageFile);
                ((System.Threading.Tasks.Task)WindowsRuntimeSystemExtensions.AsTask<string>(asyncOperation)).Wait();
                ProjectInfo.InfoStore = asyncOperation.GetResults();
            }
        }

        public IXamlType GetXamlType(System.Type type)
        {
            if (_provider == null)
            {
                _provider = new XamlTypeInfoProvider();
            }
            return _provider.GetXamlTypeByType(type) ?? GetUserAppXamlType(type);
        }

        public IXamlType GetXamlType(string fullName)
        {
            if (_provider == null)
            {
                _provider = new XamlTypeInfoProvider();
            }
            return _provider.GetXamlTypeByName(fullName) ?? GetUserAppXamlType(fullName);
        }

        public XmlnsDefinition[] GetXmlnsDefinitions()
        {
            return new XmlnsDefinition[0];
        }

        private void EnsureUserAppIXMP()
        {
            if (metaDataRetrieveAttempted)
            {
                return;
            }
            metaDataRetrieveAttempted = true;
            UserApplicationInfo? userApplicationInfo = GetUserApplicationInfo();
            if (userApplicationInfo == null)
            {
                return;
            }
            string className = userApplicationInfo.UserApplicationRootNamespace + "." + userApplicationInfo.UserApplicationProjectName + "_XamlTypeInfo.XamlMetaDataProvider";
            xamlMetadataProvider = LoadUserAppIXMP(className);
            if (xamlMetadataProvider == null)
            {
                string className2 = userApplicationInfo.UserApplicationRootNamespace + ".XamlMetaDataProvider";
                xamlMetadataProvider = LoadUserAppIXMP(className2);
                if (xamlMetadataProvider == null)
                {
                    string typeName = userApplicationInfo.UserApplicationRootNamespace + "." + userApplicationInfo.UserApplicationProjectName + "_XamlTypeInfo.XamlTypeInfoProvider";
                    userAppTypeInfoProvider = LoadUserAppIXMPReflection(userApplicationInfo.UserApplicationFullAssemblyName, typeName);
                }
            }
        }

        private IXamlMetadataProvider LoadUserAppIXMP(string className)
        {
#nullable disable
            object instance = null;

            try
            {
                var factory = (WinRT.Interop.IActivationFactory)WinRT.ActivationFactory.Get(className);
                instance = factory.ActivateInstance();
                return instance as IXamlMetadataProvider;
            }
            catch
            {
                return null;
            }
#nullable restore
        }

        // TODO: Cleanup
        private object LoadUserAppIXMPReflection(string assemblyName, string typeName)
        {
#nullable disable
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_000b: Expected O, but got Unknown
            try
            {
                Assembly val = Assembly.Load(new AssemblyName(assemblyName));
                return Activator.CreateInstance(val.GetType(typeName));
            }
            catch (System.Exception)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom($"{assemblyName.Split(',').FirstOrDefault()}.dll");
                    return System.Activator.CreateInstance(assembly.GetType(typeName));
                }
                catch
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom($"{assemblyName.Split(',').FirstOrDefault()}.Projection.dll");
                        return System.Activator.CreateInstance(assembly.GetType(typeName));
                    } catch { }
                }

                return null;
            }
#nullable restore
        }

        public IXamlType GetUserAppXamlType(System.Type type)
        {
#nullable disable
            EnsureUserAppIXMP();
            if (userAppTypeInfoProvider != null)
            {
                return ((MethodBase)RuntimeReflectionExtensions.GetRuntimeMethod(userAppTypeInfoProvider.GetType(), "GetXamlTypeByType", new System.Type[1] { typeof(System.Type) })).Invoke(userAppTypeInfoProvider, new object[1] { type }) as IXamlType;
            }
            if (xamlMetadataProvider != null)
            {
                return xamlMetadataProvider.GetXamlType(type);
            }
            return null;
#nullable restore
        }

        public IXamlType GetUserAppXamlType(string fullName)
        {
#nullable disable
            EnsureUserAppIXMP();
            if (userAppTypeInfoProvider != null)
            {
                return ((MethodBase)RuntimeReflectionExtensions.GetRuntimeMethod(userAppTypeInfoProvider.GetType(), "GetXamlTypeByName", new System.Type[1] { typeof(string) })).Invoke(userAppTypeInfoProvider, new object[1] { fullName }) as IXamlType;
            }
            if (xamlMetadataProvider != null)
            {
                return xamlMetadataProvider.GetXamlType(fullName);
            }
            return null;
#nullable restore
        }

        private UserApplicationInfo? GetUserApplicationInfo()
        {
            if (userApplicationInfo == null)
            {
                StorageFile? storageFile = null;
                try
                {
                    StorageFolder installedLocation = Package.Current.InstalledLocation;
                    IAsyncOperation<StorageFile> fileAsync = installedLocation.GetFileAsync("UserApplicationInfo.txt");
                    ((System.Threading.Tasks.Task)WindowsRuntimeSystemExtensions.AsTask<StorageFile>(fileAsync)).Wait();
                    storageFile = fileAsync.GetResults();
                }
                catch (System.Exception ex) when (ex is AggregateException || ex is FileNotFoundException)
                {
                }
                if (storageFile != null)
                {
                    IAsyncOperation<string> asyncOperation = FileIO.ReadTextAsync(storageFile);
                    ((System.Threading.Tasks.Task)WindowsRuntimeSystemExtensions.AsTask<string>(asyncOperation)).Wait();
                    string results = asyncOperation.GetResults();
                    if (results != null)
                    {
                        SetUserApplicationInfoFromArgs(results);
                    }
                }
            }
            return userApplicationInfo;
        }
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            global::Windows.UI.Xaml.Application.Start((p) =>
            {
                new Application();
            });
        }
    }
}
