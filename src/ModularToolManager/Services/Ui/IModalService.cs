﻿using Avalonia.Controls;
using ModularToolManager.Models;
using System.Threading.Tasks;

namespace ModularToolManager.Services.Ui;

/// <summary>
/// Interface to use for open up modals
/// </summary>
public interface IModalService
{
    /// <summary>
    /// Show a new modal async
    /// </summary>
    /// <param name="modalData">Thhe modal data to be placed inside of the modal</param>
    /// <param name="parent">The parent of the modal</param>
    /// <returns>A awaitable task for the modal</returns>
    Task ShowModalWindowAsync(ShowWindowModel modalData, Window parent);
}
