using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Voltflow.Services;

public class DialogService
{
	public static readonly FilePickerSaveOptions PdfOptions = new()
	{
		Title = "Save File",
		SuggestedFileName = "output",
		DefaultExtension = ".pdf",
		FileTypeChoices =
		[
			new FilePickerFileType("PDF Files")
			{
				Patterns = ["*.pdf"],
				MimeTypes = ["text/pdf"]
			},
			new FilePickerFileType("All Files")
			{
				Patterns = ["*.*"]
			}
		]
	};

	public static readonly FilePickerSaveOptions CsvOptions = new()
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

	public async Task<bool> SaveFileDialog(Visual? parent, string data, FilePickerSaveOptions options)
	{
		var topLevel = TopLevel.GetTopLevel(parent);
		if (topLevel == null)
			return false;

		var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
		if (file == null)
			return false;

		await using var stream = await file.OpenWriteAsync();
		await using var writer = new StreamWriter(stream, Encoding.UTF8);
		await writer.WriteAsync(data);

		return true;
	}

	public async Task<bool> SaveFileDialog(Visual? parent, Stream data, FilePickerSaveOptions options)
	{
		var topLevel = TopLevel.GetTopLevel(parent);
		if (topLevel == null)
			return false;

		var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
		if (file == null)
			return false;

		await using var stream = await file.OpenWriteAsync();
		await data.CopyToAsync(stream);

		return true;
	}
}
