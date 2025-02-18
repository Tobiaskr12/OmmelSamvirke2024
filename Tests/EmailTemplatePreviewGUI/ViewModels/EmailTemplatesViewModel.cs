using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.SupportModules.Logging;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using EmailTemplatePreviewGUI.Models;
using ServiceModules.Emails.EmailTemplateEngine;

namespace EmailTemplatePreviewGUI.ViewModels;

public partial class EmailTemplatesViewModel : ObservableObject, IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly FileWatcherService _fileWatcherService;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateEngine _templateEngine;

    private readonly string _fullTemplatesDirectory = Path.Combine(
        GetSolutionDirectory(), "Src", "ServiceModules", "Emails", "EmailTemplateEngine", "Templates"
    );

    private HubConnection? _hubConnection;
    private string _currentTemplate = string.Empty;

    public EmailTemplatesViewModel(
        ILoggingHandler logger,
        NavigationManager navigationManager,
        FileWatcherService fileWatcherService,
        IMediator mediator
    )
    {
        _navigationManager = navigationManager;
        _fileWatcherService = fileWatcherService;
        _mediator = mediator;
        _templateEngine = new TemplateEngine(
            logger, 
            Path.Combine(GetSolutionDirectory(), "Src", "ServiceModules")
        );
        
        InitializeFileWatcherHubConnection();
        PopulateTemplatesSelection();
        
        Parameters.CollectionChanged += Parameters_CollectionChanged;
    }

    [ObservableProperty] private Dictionary<string, List<EmailTemplate>> _emailTemplates = new();
    [ObservableProperty] private string _content = string.Empty;
    [ObservableProperty] private string _subject = string.Empty;
    [ObservableProperty] private ObservableCollection<Parameter> _parameters = [];
    [ObservableProperty] private Parameter? _selectedParameter;
    [ObservableProperty] private string _selectedParameterName = string.Empty;

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

        InitializeParameters();
        UpdateContent();
    }

    public async Task OnSendEmailToTestAddress()
    {
        (string Name, string Value)[] parametersArray = Parameters.Select(p => (p.Name, p.Value)).ToArray();
        Result generationResult = _templateEngine.GenerateBodiesFromTemplate(_currentTemplate, parametersArray);

        if (generationResult.IsSuccess)
        {
            Email email = new()
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                Recipients = [new Recipient { EmailAddress = "ommelsamvirketest1@gmail.com" }],
                Attachments = [],
                Subject = _templateEngine.GetSubject(),
                HtmlBody = _templateEngine.GetHtmlBody(),
                PlainTextBody = _templateEngine.GetPlainTextBody()
            };

            Result<EmailSendingStatus> sendResult = await _mediator.Send(new SendEmailCommand(email));
            if (sendResult.IsFailed)
            {
                Debug.WriteLine("Could not send the email generated from this template");
            }
        }
        else
        {
            Debug.WriteLine("Could not generate email from template");
        }
        
    }

    private void InitializeFileWatcherHubConnection()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/fileChangeHub"))
            .Build();
        
        _hubConnection.On<string>("FileChanged", _ => OnFileChangedOrSelected());
        _hubConnection.On<string>("FileSelected", _ => {
            OnFileChangedOrSelected();
            InitializeParameters();
        });
    }

    private void OnFileChangedOrSelected()
    {
        UpdateContent();
    }
    
    private void InitializeParameters()
    {
        (string key, string value)[] generatedParameters = GenerateTestParameters();
        Parameters.Clear();

        foreach ((string key, string value) in generatedParameters)
        {
            if (string.IsNullOrEmpty(key)) continue;
            if (string.IsNullOrEmpty(value)) continue;

            var parameter = new Parameter
            {
                Name = key,
                Value = value
            };
            Parameters.Add(parameter);
        }
        
        if (Parameters.Any())
        {
            SelectedParameter = Parameters.First();
            SelectedParameterName = SelectedParameter.Name; 
        }
        else
        {
            SelectedParameter = null;
            SelectedParameterName = string.Empty;
        }
    }
    
    private (string key, string value)[] GenerateTestParameters()
    {
        string templateFilePath = Path.Combine(_fullTemplatesDirectory, _currentTemplate);
        string rawHtml = ReadFileText(templateFilePath);

        // Insert partials so that parameters in partial files are included
        rawHtml = InsertPartials(rawHtml);

        var parameters = new List<(string key, string value)>();
        
        MatchCollection parameterMatches = ParametersRegex().Matches(rawHtml);
        foreach (Match match in parameterMatches)
        {
            string parameterName = match.Groups[1].Value.Trim();
            if (!parameters.Any(p => p.key.Equals(parameterName, StringComparison.OrdinalIgnoreCase)))
            {
                parameters.Add((key: parameterName, value: $"Parameter {parameters.Count + 1}"));
            }
        }

        return parameters.ToArray();
    }

    private string InsertPartials(string content)
    {
        string partialsDirectory = Path.Combine(
            GetSolutionDirectory(), "Src", "ServiceModules", "Emails", "EmailTemplateEngine", "Partials"
        );
        
        return PartialRegex().Replace(content, match =>
        {
            string partialName = match.Groups[1].Value;
            string safePartialName = partialName.Replace("/", Path.DirectorySeparatorChar.ToString());
            string partialFilePath = Path.Combine(partialsDirectory, safePartialName + ".html");
            if (!File.Exists(partialFilePath))
            {
                return string.Empty;
            }
            return ReadFileText(partialFilePath);
        });
    }

    private string ReadFileText(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private void UpdateContent(){
        (string Name, string Value)[] parametersArray = Parameters.Select(p => (p.Name, p.Value)).ToArray();
        _templateEngine.GenerateBodiesFromTemplate(_currentTemplate, parametersArray);
        Content = _templateEngine.GetHtmlBody();
        Subject = _templateEngine.GetSubject();
    }

    private void PopulateTemplatesSelection()
    {
        EmailTemplates = new Dictionary<string, List<EmailTemplate>>();
        
        string templatesBaseDirectory = Path.Combine(
            GetSolutionDirectory(), "Src", "ServiceModules", "Emails", "EmailTemplateEngine", "Templates"
        );
        string[] templatesSubDirectories = Directory.GetDirectories(templatesBaseDirectory);
        string[] templateTopLevelFiles = Directory.GetFiles(templatesBaseDirectory);
        IEnumerable<(string, string[])> templateFilesInSubDirectories = templatesSubDirectories.Select(x => 
            (Path.GetFileName(x), Directory.GetFiles(x))
        );

        EmailTemplates.Add("General", []);
        foreach (string topLevelTemplateFile in templateTopLevelFiles)
        {
            EmailTemplates["General"].Add(new EmailTemplate
            {
                Name = Path.GetFileName(topLevelTemplateFile),
                Path = topLevelTemplateFile
            });
        }

        foreach ((string dirName, string[] files) in templateFilesInSubDirectories)
        {
            EmailTemplates.Add(dirName, []);
            foreach (string templateFile in files)
            {
                EmailTemplates[dirName].Add(new EmailTemplate
                {
                    Name = dirName + "/" + Path.GetFileName(templateFile),
                    Path = templateFile
                });
            }
        }
    }
    
    private static string GetSolutionDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        while (directory is not null && directory.GetFiles("*.sln").Length == 0)
        {
            directory = directory.Parent;
        }
        
        return directory?.FullName ?? throw new Exception("Solution directory not found. Cannot initialize Template Engine");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null) await _hubConnection.DisposeAsync();
        
        Parameters.CollectionChanged -= Parameters_CollectionChanged;
        foreach (Parameter param in Parameters)
        {
            param.PropertyChanged -= Parameter_PropertyChanged;
        }
        
        GC.SuppressFinalize(this);
    }
    
    partial void OnSelectedParameterNameChanged(string value)
    {
        SelectedParameter = Parameters.FirstOrDefault(p => p.Name == value);
    }
    
    private void Parameters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (Parameter param in e.NewItems)
            {
                param.PropertyChanged += Parameter_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (Parameter param in e.OldItems)
            {
                param.PropertyChanged -= Parameter_PropertyChanged;
            }
        }
    }
    
    private void Parameter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Parameter.Value))
        {
            UpdateContent();
        }
    }
    
    /// <summary>
    /// Regex that matches {{something}}, ignoring {{>something}}.
    /// </summary>
    [GeneratedRegex(@"\{\{\s*(?!>)(.+?)(?=\s*\}\})\s*\}\}")]
    private static partial Regex ParametersRegex();
    
    [GeneratedRegex(@"\{\{>\s*(.*?)\s*\}\}")]
    private static partial Regex PartialRegex();
}
