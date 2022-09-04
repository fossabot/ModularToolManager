﻿using ModularToolManager.Models;
using ModularToolManager.Services.Functions;
using ModularToolManager.Services.Plugin;
using ModularToolManager.ViewModels.Extenions;
using ModularToolManagerPlugin.Plugin;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace ModularToolManager.ViewModels;

/// <summary>
/// View model to add a new function to the application
/// </summary>
internal class AddFunctionViewModel : ViewModelBase, IModalWindowEvents
{
    /// <summary>
    /// Service to use to manage plugins
    /// </summary>
    private readonly IPluginService? pluginService;

    /// <summary>
    /// Service to use for loading functions
    /// </summary>
    private readonly IFunctionService? functionService;

    /// <summary>
    /// A list with all the function plugin possiblities
    /// </summary>
    public List<FunctionPluginViewModel> FunctionPlugins => functionPlugins;

    /// <summary>
    /// Private list for all the function plugins
    /// </summary>
    private readonly List<FunctionPluginViewModel> functionPlugins;

    /// <summary>
    /// The current function model
    /// </summary>
    private FunctionModel functionModel;

    /// <summary>
    /// The display name of the new function
    /// </summary>
    [MinLength(5), MaxLength(25)]
    public string DisplayName
    {
        get => functionModel.DisplayName;
        set
        {
            functionModel.DisplayName = value;
            this.RaisePropertyChanged("DisplayName");
        }
    }

    /// <summary>
    /// The currenctly selected plugin for the function
    /// </summary>
    public FunctionPluginViewModel? SelectedFunctionPlugin
    {
        get => functionModel.Plugin is null ? null : new FunctionPluginViewModel(functionModel.Plugin!);
        set
        {
            functionModel.Plugin = value?.Plugin;
            this.RaisePropertyChanged("SelectedFunctionService");
        }
    }

    /// <summary>
    /// The parameters for the function
    /// </summary>
    public string FunctionParameters
    {
        get => functionModel.Parameters;
        set
        {
            functionModel.Parameters = value;
            this.RaisePropertyChanged("FunctionParameters");
        }
    }

    /// <summary>
    /// The currently selected path
    /// </summary>
    public string SelectedPath
    {
        get => functionModel.Path;
        set
        {
            functionModel.Path = value;
            this.RaisePropertyChanged("SelectedPath");
        }
    }

    /// <summary>
    /// Command used to add the new function
    /// </summary>
    public ICommand OkCommand { get; }

    /// <summary>
    /// Command used to abord the current changes or addition
    /// </summary>
    public ICommand AbortCommand { get; }

    /// <summary>
    /// Event if the window is getting a close requested
    /// </summary>
    public event EventHandler? Closing;

    /// <summary>
    /// Create a new instance of this class
    /// </summary>
    /// <param name="pluginService">The plugin service to use</param>
    /// <param name="functionService">The function service to use</param>
    public AddFunctionViewModel(IPluginService? pluginService, IFunctionService? functionService)
    {
        functionModel = new FunctionModel();
        this.pluginService = pluginService;
        this.functionService = functionService;
        functionPlugins = new();
        if (pluginService is not null)
        {
            functionPlugins = pluginService.GetAvailablePlugins()
                                            .Select(plugin => new FunctionPluginViewModel(plugin))
                                            .ToList();
        }

        this.WhenAnyValue(x => x.SelectedPath)
            .Throttle(TimeSpan.FromSeconds(2))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(path => SelectedPath = path.Trim());

        this.WhenAnyValue(x => x.FunctionParameters)
            .Throttle(TimeSpan.FromSeconds(2))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(parameters => FunctionParameters = parameters.Trim());

        this.WhenAnyValue(x => x.DisplayName)
            .Throttle(TimeSpan.FromSeconds(2))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(displayName => DisplayName = displayName.Trim());

        IObservable<bool> canSave = this.WhenAnyValue(x => x.DisplayName,
                                                                x => x.SelectedFunctionPlugin,
                                                                x => x.FunctionParameters,
                                                                x => x.SelectedPath,
                                                                (name, selectedFunction, parameters, path) =>
                                                                {
                                                                    IFunctionPlugin plugin = selectedFunction?.Plugin;
                                                                    bool valid = name.Length >= 5 && name.Length <= 25;
                                                                    valid &= plugin is not null;
                                                                    valid &= File.Exists(SelectedPath);
                                                                    if (valid)
                                                                    {
                                                                        FileInfo info = new FileInfo(SelectedPath);
                                                                        valid &= plugin.GetAllowedFileEndings().Select(fileExtension => fileExtension.Extension.ToLower())
                                                                                                               .Any(ending => ending == info.Extension.ToLowerInvariant());
                                                                    }

                                                                    return valid;
                                                                });

        AbortCommand = ReactiveCommand.Create(async () => Closing?.Invoke(this, EventArgs.Empty));
        OkCommand = ReactiveCommand.Create(async () =>
        {
            functionService?.AddFunction(functionModel);
            Closing?.Invoke(this, EventArgs.Empty);
        }, canSave);
    }
}
