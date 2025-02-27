using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Voltflow.Services;

public class DialogService
{
	private readonly FilePickerSaveOptions _options = new()
	{
		Title = "Save File",
		SuggestedFileName = "output",
		DefaultExtension = ".csv",
		FileTypeChoices =
		[
			new FilePickerFileType("CSV Files")
			{
				Patterns = ["*.csv"],
				MimeTypes = ["text/csv"]
			},
			new FilePickerFileType("All Files")
			{
				Patterns = ["*.*"]
			}
		]
	};

	public async Task<bool> SaveFileDialog(Visual? parent, string csv)
	{
		var topLevel = TopLevel.GetTopLevel(parent);
		if (topLevel == null)
			return false;

		var file = await topLevel.StorageProvider.SaveFilePickerAsync(_options);
		if (file == null)
			return false;

		await using var stream = await file.OpenWriteAsync();
		await using var writer = new StreamWriter(stream, Encoding.UTF8);
		await writer.WriteAsync(csv);

		return true;
	}
}
