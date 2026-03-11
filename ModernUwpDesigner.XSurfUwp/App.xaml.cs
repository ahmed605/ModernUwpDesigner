// TODO: Cleanup

using System;
using Microsoft.System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Microsoft.VisualStudio.XSurface.Common;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using WinRT;

namespace XSurfUwp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
    /// </summary>
    [Bindable]
    public sealed partial class Application : Windows.UI.Xaml.Application, ITapBridge
    {
        private readonly ThreadLocal<ThreadLocalApp> localApp = new(() => new ThreadLocalApp());
        private readonly XamlMetadataProvider provider = new();
        private static ContentWrapper wrapper;
        private static Application current;

        public new static Application Current => current;

        internal static ContentWrapper ContentWrapper => wrapper;

        public ThreadLocalApp Local => localApp.Value;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public Application()
        {
            //NativeMethods.MessageBox(0, "Attach", "Attach", 0);

            current = this;

            AddOtherProvider(provider);
            InitializeComponent();

            Suspending += OnSuspending;
        }

        static Application()
        {
            SuppressAggressiveDebugAssert();
            ReadProjectInfo();
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            provider.SetUserApplicationInfoFromArgs(e.Arguments);

            if (!e.PrelaunchActivated)
            {
                var window = Window.Current;
                
                wrapper = new();
                window.Content = wrapper;
                window.Activate();

                SynchronizationContext.SetSynchronizationContext(new CoreDispatcherSynchronizationContext(window.Dispatcher));
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
            return ContentWrapper?.ContentHolder.Child;
        }

        public void SetBottomAppBar(AppBar appBar)
        {
        }

        public void SetTopAppBar(AppBar appBar)
        {
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (ContentWrapper?.BackgroundElement is { } frameworkElement)
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
                FrameworkElement frameworkElement = ContentWrapper.PanZoom;
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
            Rectangle rectangle = ContentWrapper?.HairlineBorder;
            rectangle?.Stroke = new SolidColorBrush((Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), contentBorder));
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

            Local.DeviceSize = new Size(width, height);

            if (obj is UIElement uIElement)
            {
                uIElement.UpdateLayout();
            }
        }

        public bool TrySerialize(object obj, out string typeName, out string value, out bool isValueType)
        {
            Type type = obj?.GetType();
            if (type == typeof(DependencyObject))
            {
                typeName = string.Empty;
                value = string.Empty;
                isValueType = false;
                return false;
            }

            TypeInfo val = ((type != null) ? IntrospectionExtensions.GetTypeInfo(type) : null);

            if (val != null && ((Type)(object)val).IsEnum)
            {
                typeName = ((Type)(object)val).FullName;
                value = obj.ToString();
                isValueType = true;
                return true;
            }

            if (val != null && !((Type)(object)val).IsValueType && obj is not string)
            {
                typeName = ((Type)(object)val).FullName;
                value = string.Empty;
                isValueType = false;
                return true;
            }

            if (obj is DateTime time)
            {
                typeName = ((Type)(object)val).FullName;
                value = time.ToString("o");
                isValueType = true;
                return true;
            }

            typeName = string.Empty;
            value = string.Empty;
            isValueType = false;
            return false;
        }

        private static void SuppressAggressiveDebugAssert()
        {
            try
            {
                Type typeFromHandle = typeof(Debug);
                TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(typeFromHandle);

                foreach (var field in typeInfo.DeclaredFields)
                {
                    if (field.Name == "s_ShowAssertDialog")
                    {
                        Action<string, string, string> val = delegate
                        {
                        };
                        field.SetValue(null, val);
                    }
                    else if (field.Name == "s_ShowDialog")
                    {
                        Action<string, string, string, string> val2 = delegate
                        {
                        };
                        field.SetValue(null, val2);
                    }
                }
            }
            catch
            {
            }
        }

        private static void ReadProjectInfo()
        {
            StorageFile storageFile;
            try
            {
                StorageFolder installedLocation = Package.Current.InstalledLocation;
                IAsyncOperation<StorageFile> fileAsync = installedLocation.GetFileAsync("__ProjectInfo__.txt");
                fileAsync.AsTask().Wait();
                storageFile = fileAsync.GetResults();
            }
            catch (Exception)
            {
                storageFile = null;
            }

            if (storageFile != null)
            {
                IAsyncOperation<string> asyncOperation = FileIO.ReadTextAsync(storageFile);
                asyncOperation.AsTask().Wait();
                ProjectInfo.InfoStore = asyncOperation.GetResults();
            }
        }
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            Windows.UI.Xaml.Application.Start((p) =>
            {
                new Application();
            });
        }
    }
}
