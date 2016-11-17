﻿using Arma3BE.Client.Infrastructure.Commands;
using Arma3BE.Client.Infrastructure.Events.BE;
using Arma3BE.Client.Infrastructure.Models;
using Arma3BE.Server;
using Arma3BE.Server.Models;
using Arma3BEClient.Libs.ModelCompact;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Arma3BE.Client.Modules.ManageServerModule.Models
{
    public class ServerMonitorManageServerViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Guid _serverId;
        private IEnumerable<Mission> _missions;
        private Mission _selectedMission;

        public ServerMonitorManageServerViewModel(ServerInfo serverInfo, IEventAggregator eventAggregator)
        {
            _serverId = serverInfo.Id;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<BEMessageEvent<BEItemsMessage<Mission>>>().Subscribe(e =>
            {
                if (e.ServerId == this._serverId)
                    this.Missions = e.Items;
            });

            SetMissionCommand = new ActionCommand(() =>
            {
                var m = SelectedMission;
                if (m != null)
                {
                    var mn = m.Name;
                    SendCommand(CommandType.Mission, mn);
                }
            },
                () => SelectedMission != null);

            RefreshCommand = new ActionCommand(() => SendCommand(CommandType.Missions));

            InitCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.Init);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
            ShutdownCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.Shutdown);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
            ReassignCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.Reassign);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
            RestartCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.Restart);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
            LockCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.Lock);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
            UnlockCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.Unlock);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });


            LoadBansCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.LoadBans);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
            LoadScriptsCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.LoadScripts);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
            LoadEventsCommand = new ActionCommand(() =>
           {
               SendCommand(CommandType.LoadEvents);
               MessageBox.Show("Executed", "Server command", MessageBoxButton.OK);
           });
        }

        public string Title { get { return "Manage Server"; } }

        public Mission SelectedMission
        {
            get { return _selectedMission; }
            set
            {
                _selectedMission = value;
                OnPropertyChanged("SelectedMission");
                SetMissionCommand.RaiseCanExecuteChanged();
            }
        }

        public IEnumerable<Mission> Missions
        {
            get { return _missions; }
            set
            {
                _missions = value;
                OnPropertyChanged("Missions");
            }
        }

        public ActionCommand RefreshCommand { get; set; }
        public ActionCommand SetMissionCommand { get; set; }
        public ActionCommand InitCommand { get; set; }
        public ActionCommand ShutdownCommand { get; set; }
        public ActionCommand ReassignCommand { get; set; }
        public ActionCommand RestartCommand { get; set; }
        public ActionCommand LockCommand { get; set; }
        public ActionCommand UnlockCommand { get; set; }
        public ActionCommand LoadBansCommand { get; set; }
        public ActionCommand LoadScriptsCommand { get; set; }
        public ActionCommand LoadEventsCommand { get; set; }

        private void SendCommand(CommandType commandType, string parameters = null)
        {
            _eventAggregator.GetEvent<BEMessageEvent<BECommand>>()
                .Publish(new BECommand(_serverId, commandType, parameters));
        }
    }
}