﻿using Arma3BE.Client.Infrastructure.Extensions;
using Arma3BE.Client.Modules.PlayersModule.Models;
using Arma3BEClient.Libs.ModelCompact;
using System.Windows;

namespace Arma3BE.Client.Modules.PlayersModule.Boxes
{
    /// <summary>
    ///     Interaction logic for PlayerViewWindow.xaml
    /// </summary>
    public partial class PlayerViewWindow : Window
    {
        private readonly PlayerViewModel _model;

        public PlayerViewWindow(PlayerViewModel model)
        {
            _model = model;
            InitializeComponent();


            dgBans.ContextMenu = dgBans.Generate<Ban>();
            dgHist.ContextMenu = dgHist.Generate<PlayerHistory>();
            dgNotes.ContextMenu = dgNotes.Generate<Note>();


            DataContext = _model;
        }
    }
}