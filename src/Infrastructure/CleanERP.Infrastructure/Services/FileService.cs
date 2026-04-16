using CleanERP.Application.Common.Interfaces;

namespace CleanERP.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _rootPath;

    public FileService(string rootPath = "wwwroot")
    {
        _rootPath = rootPath;
        EnsureDirectoryExists(_rootPath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder = "uploads")
    {
        var uploadFolder = Path.Combine(_rootPath, folder);
        EnsureDirectoryExists(uploadFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        using var fileStreamDestination = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamDestination);

        return Path.Combine(folder, uniqueFileName);
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_rootPath, filePath);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await Task.FromResult(File.OpenRead(fullPath));
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_rootPath, filePath);
        
        if (!File.Exists(fullPath))
            return false;

        await Task.Run(() => File.Delete(fullPath));
        return true;
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_rootPath, filePath);
        return await Task.FromResult(File.Exists(fullPath));
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
