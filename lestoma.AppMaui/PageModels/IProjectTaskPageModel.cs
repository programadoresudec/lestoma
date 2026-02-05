using CommunityToolkit.Mvvm.Input;
using lestoma.AppMaui.Models;

namespace lestoma.AppMaui.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}