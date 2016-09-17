﻿using Arma3BE.Client.Infrastructure;
using Arma3BE.Client.Modules.MainModule.Models.Export;
using Arma3BE.Client.Modules.MainModule.ViewModel;
using Arma3BEClient.Libs.ModelCompact;
using Arma3BEClient.Libs.Repositories;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Arma3BEClient.Common.Extensions;
using Xceed.Wpf.AvalonDock.Controls;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace Arma3BE.Client.Modules.MainModule
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow
    {
        private readonly MainViewModel _model;
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;

        public MainWindow(MainViewModel model, IUnityContainer container, IRegionManager regionManager)
        {
            InitializeComponent();

            _model = model;
            _container = container;
            _regionManager = regionManager;
            DataContext = _model;
        }

        private void OpenServerInfo(ServerInfo serverInfo)
        {
            var control =
                _container.Resolve<ServerInfoControl>(
                    new ParameterOverride("currentServer", serverInfo).OnType<ServerMonitorModel>(),
                    new ParameterOverride("console", false).OnType<ServerMonitorModel>());

            _model.Reload();
            _regionManager.Regions[RegionNames.ServerTabRegion].Add(control, null, true);
        }

        private void ServerClick(object sender, RoutedEventArgs e)
        {
            var orig = e.OriginalSource as FrameworkElement;
            if (orig?.DataContext is ServerInfo)
            {
                var serverInfo = (ServerInfo)orig.DataContext;
                OpenServerInfo(serverInfo);
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.FindVisualAncestor<Window>().Close();
            Application.Current.Shutdown();
        }

        private void LoadedWindow(object sender, RoutedEventArgs e)
        {
            using (var r = new ServerInfoRepository())
            {
                var servers = r.GetActiveServerInfo();
                foreach (var server in servers)
                {
                    OpenServerInfo(server);
                }
            }
        }

        private async void ExportClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                DefaultExt = "*.xml",
                Filter = "*.xml|*.xml",
                Title = "Select file to save players"
            };

            var res = dlg.ShowDialog();

            if (res.HasValue && res.Value)
            {
                await Task.Run(() => Export(dlg.FileName));
                MessageBox.Show("Export finished!");
            }
        }

        private void Export(string fname)
        {
            using (var repo = PlayerRepositoryFactory.Create())
            {
                var list =
                    repo.GetAllPlayers()
                        .GroupBy(x => x.GUID)
                        .Select(x => x.OrderByDescending(y => y.Name).First())
                        .OrderBy(x => x.Name)
                        .Select(x =>
                            new PlayerXML
                            {
                                Guid = x.GUID,
                                SteamId = x.SteamId,
                                LastIP = x.LastIp,
                                LastSeen = x.LastSeen,
                                Name = x.Name,
                                Comment = x.Comment
                            }).ToList();


                using (var sw = new StreamWriter(fname))
                {
                    var ser = new XmlSerializer(typeof(List<PlayerXML>));
                    ser.Serialize(sw, list);
                }
            }
        }


        private class ImportResult
        {
            public int Added { get; set; }
            public int Updated { get; set; }
        }

        

        private ImportResult Import(string fname)
        {
            var result = new ImportResult();

            List<PlayerXML> players;
            using (var sr = new StreamReader(fname))
            {
                var ser = new XmlSerializer(typeof(List<PlayerXML>));
                players = (List<PlayerXML>)ser.Deserialize(sr);
            }

            using (var repo = PlayerRepositoryFactory.Create())
            {
                var db =
                    repo.GetAllPlayers()
                        .GroupBy(x => x.GUID)
                        .Select(x => x.OrderByDescending(y => y.Name).First())
                        .ToDictionary(x => x.GUID);

                var toadd = new List<PlayerDto>();

                foreach (var p in players)
                {
                    if (!db.ContainsKey(p.Guid))
                    {
                        result.Added++;
                        toadd.Add(new PlayerDto
                        {
                            Comment = p.Comment,
                            GUID = p.Guid,
                            LastIp = p.LastIP,
                            LastSeen = p.LastSeen,
                            Name = p.Name,
                            SteamId = p.SteamId,
                            Id = Guid.NewGuid()
                        });
                    }
                    else
                    {
                        result.Updated++;
                        var lp = db[p.Guid];
                        if (string.IsNullOrEmpty(lp.Comment) && !string.IsNullOrEmpty(p.Comment))
                        {
                            lp.Comment = p.Comment;
                        }
                        if (string.IsNullOrEmpty(lp.SteamId) && !string.IsNullOrEmpty(p.SteamId))
                        {
                            lp.SteamId = p.SteamId;
                        }

                        toadd.Add(lp);
                    }
                }


                repo.AddOrUpdatePlayers(toadd);
            }

            return result;
        }

        private async void ImportClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                DefaultExt = "*.xml",
                Filter = "*.xml|*.xml",
                Title = "Select file to import players"
            };

            var res = ofd.ShowDialog();

            if (res.HasValue && res.Value)
            {
                var result = await Task.Run(() => Import(ofd.FileName));
                MessageBox.Show($"Import finished! Added {result.Added}, updated {result.Updated}");
            }
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            var window = new About();
            window.Owner = this.FindVisualAncestor<Window>();
            window.ShowDialog();
        }
    }
}