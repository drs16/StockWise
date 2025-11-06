using CommunityToolkit.Mvvm.Input;
using StockWise.Client.Models;

namespace StockWise.Client.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}