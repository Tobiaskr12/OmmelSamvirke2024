using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using OmmelSamvirke.EmailTemplatePreviewGUI.Models;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;

namespace OmmelSamvirke.EmailTemplatePreviewGUI.ViewModels;

public partial class EmailTemplatesViewModel : ObservableObject, IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly FileWatcherService _fileWatcherService;
    private readonly IEmailTemplateEngine _templateEngine;
    
    private HubConnection? _hubConnection;
    private string _currentTemplate = string.Empty;
    
    public EmailTemplatesViewModel(ILogger logger, NavigationManager navigationManager, FileWatcherService fileWatcherService)
    {
        _navigationManager = navigationManager;
        _fileWatcherService = fileWatcherService;
        _templateEngine = new TemplateEngine(logger, Path.Combine(GetSolutionDirectory(), "Src", "OmmelSamvirke.ServiceModules"));
        
        InitializeFileWatcherHubConnection();
        PopulateTemplatesSelection();
    }

    [ObservableProperty] private Dictionary<string, List<EmailTemplate>> _emailTemplates = [];
    [ObservableProperty] private string _content = string.Empty;

    
    public async Task WatchTemplate(EmailTemplate emailTemplate)
    {
        if (_hubConnection is { State: HubConnectionState.Disconnected })
        {
            await _hubConnection.StartAsync();
        }
        
        if (string.IsNullOrWhiteSpace(emailTemplate.Path))
        {
            return;
        }
        
        _currentTemplate = emailTemplate.Name;
        _fileWatcherService.SetFileToWatch(emailTemplate.Path);
    }

    private void InitializeFileWatcherHubConnection()
    {
        _hubConnection = new HubConnectionBuilder()
                         .WithUrl(_navigationManager.ToAbsoluteUri("/fileChangeHub"))
                         .Build();
        
        _hubConnection.On<string>("FileChanged", _ =>
        {
            _templateEngine.GenerateBodiesFromTemplate(_currentTemplate);
            Content = _templateEngine.GetHtmlBody();
            
            OnPropertyChanged();
        });
        
        _hubConnection.On<string>("FileSelected", _ =>
        {
            _templateEngine.GenerateBodiesFromTemplate(_currentTemplate);
            Content = _templateEngine.GetHtmlBody();

            OnPropertyChanged();
        });
    }

    private void PopulateTemplatesSelection()
    {
        EmailTemplates = [];
        
        string templatesBaseDirectory = Path.Combine(Path.Combine(GetSolutionDirectory(), "Src", "OmmelSamvirke.ServiceModules"), "Emails", "EmailTemplateEngine", "Templates");
        string[] templatesSubDirectories = Directory.GetDirectories(templatesBaseDirectory);
        string[] templateTopLevelFiles = Directory.GetFiles(templatesBaseDirectory);
        IEnumerable<(string, string[])> templateFilesInSubDirectories = templatesSubDirectories.Select(x => (Path.GetFileName(x), Directory.GetFiles(x)));

        EmailTemplates.Add("General", []);
        foreach (string topLevelTemplateFile in templateTopLevelFiles)
        {
            EmailTemplates["General"].Add(new EmailTemplate
            {
                Name = Path.GetFileName(topLevelTemplateFile),
                Path = topLevelTemplateFile
            });
        }

        foreach ((string, string[]) templateSubDirectory in templateFilesInSubDirectories)
        {
            string groupName = templateSubDirectory.Item1;
            EmailTemplates.Add(groupName, []);
            foreach (string templateFile in templateSubDirectory.Item2)
            {
                EmailTemplates[groupName].Add(new EmailTemplate
                {
                    Name = templateSubDirectory.Item1 + "/" + Path.GetFileName(templateFile),
                    Path = templateFile
                });
            }
        }
    }
    
    private static string GetSolutionDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        while (directory is not null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        
        return directory?.FullName ?? throw new Exception("Solution directory not found. Cannot initialize Template Engine");
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null) 
            await _hubConnection.DisposeAsync();
    }
}
