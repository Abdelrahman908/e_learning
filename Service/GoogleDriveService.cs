using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using e_learning.Service.Interfaces;
using Google.Apis.Drive.v3.Data;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace e_learning.Service.Implementations
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly ILogger<GoogleDriveService> _logger;
        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMs = 1000;

        public GoogleDriveService(IWebHostEnvironment env, ILogger<GoogleDriveService> logger)
        {
            _logger = logger;

            try
            {
                var credentialPath = Path.Combine(env.ContentRootPath, "App_Data", "credentials.json");
                if (!System.IO.File.Exists(credentialPath))
                {
                    throw new FileNotFoundException("ملف الاعتمادات غير موجود", credentialPath);
                }

                GoogleCredential credential;
                using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.Scope.DriveFile);
                }

                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "E-Learning Platform"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل تهيئة خدمة Google Drive");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderId = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير موجود أو فارغ");

            int attempt = 0;
            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    var fileMetadata = new DriveFile
                    {
                        Name = file.FileName,
                        Parents = folderId != null ? new List<string> { folderId } : null,
                        MimeType = file.ContentType
                    };

                    using var stream = file.OpenReadStream();
                    var request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
                    request.Fields = "id,size,webViewLink";
                    request.ChunkSize = ResumableUpload.MinimumChunkSize * 4;

                    var result = await request.UploadAsync();

                    if (result.Status == UploadStatus.Completed)
                    {
                        _logger.LogInformation("تم رفع الملف بنجاح. ID: {FileId}, الحجم: {Size} بايت",
                            request.ResponseBody.Id, request.ResponseBody.Size);
                        return request.ResponseBody.Id;
                    }

                    throw new IOException($"فشل رفع الملف: {result.Exception?.Message}");
                }
                catch (Exception ex) when (attempt < MaxRetryAttempts - 1)
                {
                    attempt++;
                    _logger.LogWarning(ex, "محاولة رفع الملف رقم {Attempt}", attempt);
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }

            throw new IOException("فشل رفع الملف بعد عدة محاولات");
        }

        public async Task DeleteFileAsync(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentNullException(nameof(fileId));

            try
            {
                await _driveService.Files.Delete(fileId).ExecuteAsync();
                _logger.LogInformation("تم حذف الملف بنجاح. ID: {FileId}", fileId);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("الملف غير موجود. ID: {FileId}", fileId);
                throw new FileNotFoundException("الملف غير موجود في Google Drive", fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل حذف الملف. ID: {FileId}", fileId);
                throw;
            }
        }

        public async Task<string> GeneratePublicUrlAsync(string fileId)
        {
            try
            {
                var permission = new Permission
                {
                    Type = "anyone",
                    Role = "reader"
                };

                await _driveService.Permissions.Create(permission, fileId).ExecuteAsync();

                var file = await _driveService.Files.Get(fileId).ExecuteAsync();
                return file.WebViewLink ?? $"https://drive.google.com/file/d/{fileId}/view";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل إنشاء رابط عام للملف. ID: {FileId}", fileId);
                throw;
            }
        }

        public async Task<long> GetFileSizeAsync(string fileId)
        {
            try
            {
                var file = await _driveService.Files.Get(fileId).ExecuteAsync();
                return file.Size ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل الحصول على حجم الملف. ID: {FileId}", fileId);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string fileId)
        {
            try
            {
                await _driveService.Files.Get(fileId).ExecuteAsync();
                return true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل التحقق من وجود الملف. ID: {FileId}", fileId);
                throw;
            }
        }
    }
}
