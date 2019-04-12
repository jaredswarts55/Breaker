using System;
using System.IO;
using System.Linq;
using System.Windows;
using Autofac;
using Breaker.Core.Models;
using Breaker.Core.Services.Base;
using Breaker.Startup;
using Breaker.ViewModels;
using Breaker.ViewModels.SubModels;
using Breaker.Windows;
using Caliburn.Micro;
using MediatR;
using Shell32;

namespace Breaker
{
    /// <summary>
    ///     Provides a bootstrapper for the application
    /// </summary>
    public class AppBootstrapper : AutofacBootstrapper<MasterViewModel>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AppBootstrapper" /> class
        /// </summary>
        public AppBootstrapper()
        {
            Initialize();
        }


        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<AppWindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<MasterViewModel>().As<IShell>();
            builder.RegisterType<BaseWindow>().AsSelf();
            SetupMediatr(builder);
        }

        private void SetupMediatr(ContainerBuilder builder)
        {
            // mediator itself
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(
                context =>
                {
                    var c = context.Resolve<IComponentContext>();
                    return t => c.Resolve(t);
                }
            );

            builder.RegisterAssemblyTypes(typeof(AppBootstrapper).Assembly, typeof(SearchEntry).Assembly)
                   .Where(
                       t => t.Name.EndsWith("Repository") ||
                            t.Name.EndsWith("Factory") ||
                            t.Name.EndsWith("Handler") ||
                            t.Name.EndsWith("Client") ||
                            t.Name.EndsWith("Service")
                   )
                   .AsImplementedInterfaces();
        }

        protected override void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
            EnforceNamespaceConvention = false;
        }

        /// <summary>
        ///     Runs on application startup
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The startup events</param>
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
            var userSettingsService = Container.Resolve<IUserSettingsService>();
            SearchEntries.InitializeSearch(GetShortcutTarget, userSettingsService);
            
        }
        public static (string path, string arguments) GetShortcutTarget(string lnkPath)
        {
            var shell = new Shell();
            var fullPath = Path.GetFullPath(lnkPath);
            var directory = shell.NameSpace(Path.GetDirectoryName(fullPath));
            var itm = directory.Items().Item(Path.GetFileName(fullPath));
            ShellLinkObject linkObject = null;
            try
            {
                linkObject = (ShellLinkObject)itm.GetLink;
            }
            // A very small number of these fail with access rights issues. I think they are created by system? Admins don't have rights
            catch (Exception) { }
            var targetPath = linkObject?.Target?.Path;
            var arguments = linkObject?.Arguments;
            return (targetPath, arguments);
        }
       
    }
}