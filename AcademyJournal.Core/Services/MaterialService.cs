using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyJournal.Core.Services
{
    public class MaterialInfo
    {
        public string Name { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string SizeFormatted { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class MaterialLoadResult
    {
        public bool Success { get; set; }
        public byte[]? Content { get; set; }
        public string? ContentType { get; set; }
        public MaterialInfo? MaterialInfo { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MaterialOperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public MaterialInfo? MaterialInfo { get; set; }
    }

    public class MaterialServiceOptions
    {
        public string RootPath { get; set; } = "Materials";
        public List<string> AllowedExtensions { get; set; } = new() { ".pdf", ".docx", ".pptx", ".mp4", ".jpg", ".png", ".txt" };
        public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;
        public bool IncludeSubdirectories { get; set; } = true;
    }

    public class MaterialService
    {
        private readonly string _rootPath;
        private readonly MaterialServiceOptions _options;

        public MaterialService() : this(new MaterialServiceOptions()) { }

        public MaterialService(MaterialServiceOptions options)
        {
            _options = options;
            _rootPath = Path.GetFullPath(_options.RootPath);

            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
        }

        public async Task<List<MaterialInfo>> ScanMaterialsAsync(string? subfolder = null)
        {
            return await Task.Run(() =>
            {
                var searchPath = string.IsNullOrEmpty(subfolder) ? _rootPath : Path.Combine(_rootPath, subfolder);

                if (!Directory.Exists(searchPath))
                    return new List<MaterialInfo>();

                var searchOption = _options.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(searchPath, "*.*", searchOption);
                var materials = new List<MaterialInfo>();

                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file).ToLower();
                    if (!_options.AllowedExtensions.Contains(extension))
                        continue;

                    materials.Add(CreateMaterialInfo(file, Path.GetRelativePath(_rootPath, file)));
                }

                return materials.OrderBy(m => m.Name).ToList();
            });
        }

        public async Task<MaterialLoadResult> LoadMaterialAsync(string relativePath)
        {
            var result = new MaterialLoadResult();

            try
            {
                var fullPath = Path.Combine(_rootPath, relativePath);

                if (!File.Exists(fullPath))
                {
                    result.Success = false;
                    result.ErrorMessage = "Файл не найден";
                    return result;
                }

                var fileInfo = new FileInfo(fullPath);

                if (fileInfo.Length > _options.MaxFileSizeBytes)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Файл превышает максимальный размер ({FormatFileSize(_options.MaxFileSizeBytes)})";
                    return result;
                }

                result.Content = await File.ReadAllBytesAsync(fullPath);
                result.ContentType = GetContentType(fileInfo.Extension);
                result.MaterialInfo = CreateMaterialInfo(fullPath, relativePath);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<MaterialOperationResult> DeleteMaterialAsync(string relativePath)
        {
            var result = new MaterialOperationResult();

            try
            {
                var fullPath = Path.Combine(_rootPath, relativePath);

                if (!File.Exists(fullPath))
                {
                    result.Success = false;
                    result.Message = "Файл не найден";
                    return result;
                }

                var materialInfo = CreateMaterialInfo(fullPath, relativePath);

                await Task.Run(() => File.Delete(fullPath));

                result.Success = true;
                result.Message = "Файл успешно удалён";
                result.MaterialInfo = materialInfo;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<MaterialOperationResult> UploadMaterialAsync(byte[] content, string fileName, string? subfolder = null)
        {
            var result = new MaterialOperationResult();

            try
            {
                var extension = Path.GetExtension(fileName).ToLower();

                if (!_options.AllowedExtensions.Contains(extension))
                {
                    result.Success = false;
                    result.Message = $"Расширение {extension} не разрешено";
                    return result;
                }

                if (content.Length > _options.MaxFileSizeBytes)
                {
                    result.Success = false;
                    result.Message = $"Файл превышает максимальный размер ({FormatFileSize(_options.MaxFileSizeBytes)})";
                    return result;
                }

                var targetFolder = string.IsNullOrEmpty(subfolder) ? _rootPath : Path.Combine(_rootPath, subfolder);

                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                var safeFileName = GetSafeFileName(fileName);
                var fullPath = Path.Combine(targetFolder, safeFileName);
                var relativePath = Path.GetRelativePath(_rootPath, fullPath);

                await File.WriteAllBytesAsync(fullPath, content);

                result.Success = true;
                result.Message = "Файл успешно загружен";
                result.MaterialInfo = CreateMaterialInfo(fullPath, relativePath);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<MaterialInfo?> GetMaterialInfoAsync(string relativePath)
        {
            return await Task.Run(() =>
            {
                var fullPath = Path.Combine(_rootPath, relativePath);
                return File.Exists(fullPath) ? CreateMaterialInfo(fullPath, relativePath) : null;
            });
        }

        private MaterialInfo CreateMaterialInfo(string fullPath, string relativePath)
        {
            var fileInfo = new FileInfo(fullPath);
            return new MaterialInfo
            {
                Name = fileInfo.Name,
                RelativePath = relativePath,
                FullPath = fullPath,
                Extension = fileInfo.Extension,
                SizeBytes = fileInfo.Length,
                SizeFormatted = FormatFileSize(fileInfo.Length),
                CreatedAt = fileInfo.CreationTime,
                ModifiedAt = fileInfo.LastWriteTime,
                Category = GetCategoryByExtension(fileInfo.Extension)
            };
        }

        private string GetCategoryByExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" or ".docx" or ".doc" or ".txt" or ".rtf" => "Документ",
                ".pptx" or ".ppt" => "Презентация",
                ".mp4" or ".avi" or ".mov" or ".mkv" => "Видео",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Изображение",
                ".mp3" or ".wav" or ".flac" => "Аудио",
                _ => "Другое"
            };
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".mp4" => "video/mp4",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        private string GetSafeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);

            var safeName = string.Join("_", nameWithoutExtension.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return $"{safeName}{extension}";
        }
    }
}
