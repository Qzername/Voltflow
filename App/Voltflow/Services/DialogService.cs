using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Voltflow.Services;

public class DialogService
{
    public async Task<string> OpenFileDialog(FilePickerOpenOptions options)
    {
        string result = string.Empty;

        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var files = await desktop.MainWindow!.StorageProvider.OpenFilePickerAsync(options);

            if (files.Count == 0)
                return "";

            string? tempResult = files[0].Path.LocalPath;

            if (result is null)
                return "";

            result = tempResult;
        }

        return result;
    }

    public async Task<string> OpenDirectoryDialog(FolderPickerOpenOptions options)
    {
        string result = string.Empty;

        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var files = await desktop.MainWindow!.StorageProvider.OpenFolderPickerAsync(options);

            if (files.Count == 0)
                return "";

            string? tempResult = files[0].Path.LocalPath;

            if (result is null)
                return "";

            result = tempResult;
        }

        return result;
    }
}
