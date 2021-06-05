using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;

namespace GoogleAPIService.Models
{
    public class GoogleDriveFilesRepository
    {
        public static string[] Scopes = { DriveService.Scope.Drive };
        public static IWebHostEnvironment _wenv;

        public static DriveService GetService()
        {
            UserCredential credential;
            using (var stream = new FileStream(@"D:\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                String FolderPath = @"D:\";
                String FilePath = Path.Combine(FolderPath, "DriveServiceCredentials9.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //Create Drive API service.
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleAPIService",
            });

            return service;
        }

        public static List<GoogleDriveFiles> GetDriveFiles()
        {
            DriveService service = GetService();

            // Define parameters of request.
            FilesResource.ListRequest FileListRequest = service.Files.List();

            //listRequest.PageSize = 10;
            //listRequest.PageToken = 10;
            FileListRequest.Fields = "nextPageToken, files(id, name, size, version, trashed, createdTime)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFiles> FileList = new List<GoogleDriveFiles>();

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFiles File = new GoogleDriveFiles
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime
                    };
                    FileList.Add(File);
                }
            }
            return FileList;
        }

        public static void FileUpload(string Base64String, string FileName, string MimeType)
        {
            
            //var contents = new StreamContent(new MemoryStream(bytes));
            

            DriveService service = GetService();
            var bytes = Convert.FromBase64String(Base64String);
            //var rootPath = _wenv.WebRootPath;
            //var filePath = rootPath + "/GoogleDriveFiles/";
            //string path = Path.Combine(filePath,
            //Path.GetFileName(file.FileName));
            //file.SaveAs(path);

            var FileMetaData = new Google.Apis.Drive.v3.Data.File();
            FileMetaData.Name = FileName;
            FileMetaData.MimeType = MimeType;

            FilesResource.CreateMediaUpload request;

            //var stream = contents;
            //request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
            //request.Fields = "id";
            //request.Upload();

            using (var contents = new MemoryStream(bytes))
            {
                request = service.Files.Create(FileMetaData, contents, FileMetaData.MimeType);
                request.Fields = "id";
                request.Upload();
            }

        }

        public static string DownloadGoogleFile(string fileId)
        {
            DriveService service = GetService();

            var rootPath = _wenv.WebRootPath;
         

            string FolderPath = rootPath + "/GoogleDriveFiles/";
            FilesResource.GetRequest request = service.Files.Get(fileId);

            string FileName = request.Execute().Name;
            string FilePath = System.IO.Path.Combine(FolderPath, FileName);

            MemoryStream stream1 = new MemoryStream();

            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            //Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            //Console.WriteLine("Download complete.");
                            SaveStream(stream1, FilePath);
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            //Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream1);
            return FilePath;
        }

        private static void SaveStream(MemoryStream stream, string FilePath)
        {
            using (System.IO.FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }

        public static void DeleteFile(GoogleDriveFiles files)
        {
            DriveService service = GetService();
            try
            {
                // Initial validation.
                if (service == null)
                    throw new ArgumentNullException("service");

                if (files == null)
                    throw new ArgumentNullException(files.Id);

                // Make the request.
                service.Files.Delete(files.Id).Execute();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files.Delete failed.", ex);
            }
        }
    }
}
