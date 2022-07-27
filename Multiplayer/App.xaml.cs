using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Multiplayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IContainer Container { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            BuildApplication();
            StartMainWindow();
        }
        private void BuildApplication()
        {
            var assembly = Assembly.GetExecutingAssembly();

            /// Initialize dependency injections.
            var builder = new ContainerBuilder();

            /// Add the MainWindow.
            builder.RegisterType<MainWindow>()
                .SingleInstance();

            /// Add data classes.
            builder.RegisterAssemblyTypes(assembly)
                .AssignableTo<IData>()
                .InstancePerLifetimeScope();

            Container = builder.Build();
        }

        private void StartMainWindow() =>
            Container.BeginLifetimeScope()
            .Resolve<MainWindow>()
            .Show();
    }
}
