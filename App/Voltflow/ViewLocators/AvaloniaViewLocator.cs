using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using Voltflow.ViewModels;

namespace Voltflow.ViewLocators;

/// <summary>
/// This viewlocator tells Avalonia which view to use for a given viewmodel.
/// 
/// if i set the datacontext of a control to a viewmodel,
/// then this viewlocator will find the view for that viewmodel.
/// </summary>
public class AvaloniaViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object data)
    {
        return data is ViewModelBase;
    }
}