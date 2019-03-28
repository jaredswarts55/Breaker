using Autofac;

namespace Breaker
{
    using System.Windows;
    using Caliburn.Micro;
    using Windows;

    /// <summary>
    /// Provides a window manager for the application
    /// </summary>
    public class AppWindowManager : WindowManager
    {
        /// <summary>
        /// Selects a base window depending on the view, model and dialog options
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="view">The view</param>
        /// <param name="isDialog">Whether it's a dialog</param>
        /// <returns>The proper window</returns>
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = view as BaseWindow;

            if (window == null)
            {
                if (isDialog)
                {
                    window = new BaseDialogWindow
                    {
                        Content = view,
                        SizeToContent = SizeToContent.WidthAndHeight
                    };
                }
                else
                {
                    var baseWindow = AppBootstrapper.Container.Resolve<BaseWindow>();
                    baseWindow.Content = view;
                    baseWindow.SizeToContent = SizeToContent.Manual;
                    baseWindow.WindowState = WindowState.Normal;
                    window = baseWindow;
                }

                window.SetValue(View.IsGeneratedProperty, true);
            }
            else
            {
                Window owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                {
                    window.Owner = owner;
                }
            }

            return window;
        }
    }
}
