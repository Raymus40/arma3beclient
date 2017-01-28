﻿using Arma3BE.Client.Infrastructure.Contracts;
using Arma3BE.Client.Infrastructure.Events;
using Arma3BE.Client.Infrastructure.Events.BE;
using Arma3BEClient.Libs.ModelCompact;
using Arma3BEClient.Libs.Repositories;
using Prism.Events;
using System;
// ReSharper disable MemberCanBePrivate.Global

namespace Arma3BE.Client.Modules.MainModule.ViewModel
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerMonitorModel : DisposableViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private bool _connected;

        public ServerMonitorModel(ServerInfo currentServer,
            IEventAggregator eventAggregator)
        {
            CurrentServer = currentServer;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<ConnectServerEvent>().Subscribe(BeServerConnectHandler, ThreadOption.UIThread);
            _eventAggregator.GetEvent<DisConnectServerEvent>().Subscribe(BeServerDisconnectHandler, ThreadOption.UIThread);
            _eventAggregator.GetEvent<RunServerEvent>().Publish(CurrentServer);
        }

        public ServerInfo CurrentServer { get; }

        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged();
            }
        }

        public string Title => CurrentServer.Name;

        private void BeServerDisconnectHandler(ServerInfo info)
        {
            if (info.Id != CurrentServer.Id) return;
            Connected = false;
        }

        private void BeServerConnectHandler(ServerInfo info)
        {
            Connected = true;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            _eventAggregator.GetEvent<CloseServerEvent>()
                .Publish(CurrentServer);
        }

        public void SetActive(Guid serverId, bool active = false)
        {
            using (var repo = new ServerInfoRepository())
            {
                repo.SetServerInfoActive(serverId, active);
            }

            _eventAggregator.GetEvent<BEServersChangedEvent>().Publish(null);
        }

        public void CloseServer()
        {
            SetActive(CurrentServer.Id, false);
        }

        public void OpenServer()
        {
            SetActive(CurrentServer.Id, true);
        }
    }
}