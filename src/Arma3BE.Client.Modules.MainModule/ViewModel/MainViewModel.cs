using Arma3BE.Client.Infrastructure.Events;
using Arma3BE.Client.Infrastructure.Models;
using Arma3BEClient.Libs.Repositories;
using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ExplicitCallerInfoArgument

namespace Arma3BE.Client.Modules.MainModule.ViewModel
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainViewModel : ViewModelBase
    {
        private readonly IServerInfoRepository _infoRepository;

        public MainViewModel(IEventAggregator eventAggregator, IServerInfoRepository infoRepository)
        {
            _infoRepository = infoRepository;
            ReloadAsync();

            OptionsCommand = new DelegateCommand(() =>
            {
                eventAggregator.GetEvent<ShowOptionsEvent>().Publish(new ShowOptionsEvent());
            });

            eventAggregator.GetEvent<BEServersChangedEvent>().Subscribe(async (state) =>
            {
                await ReloadAsync();
            });
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand OptionsCommand { get; set; }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public List<ServerInfoDto> Servers
        {
            // ReSharper disable once UnusedMember.Global
            get;
            set;
        }

        public async Task ReloadAsync()
        {
            Servers = (await _infoRepository.GetNotActiveServerInfoAsync()).OrderBy(x => x.Name).ToList();
            RaisePropertyChanged(nameof(Servers));
        }
    }
}