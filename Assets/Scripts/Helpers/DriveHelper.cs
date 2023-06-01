namespace Assets.Scripts.Helpers
{
    using Cysharp.Threading.Tasks;

    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Drive.v3;
    using Google.Apis.Drive.v3.Data;
    using Google.Apis.Services;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using UnityEngine.Networking;

    using File = Google.Apis.Drive.v3.Data.File;

    public static class DriveHelper
    {
        public static async Task DownloadFileByName(string fileName, string savePath, Action<string> SetStatusText = null)
        {
            if (SetStatusText != null)
                SetStatusText($"Searching for file: {fileName}...");

            // Create a service account credential using the service account email and required scopes
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer("instagram@instagram-388012.iam.gserviceaccount.com")
            {
                Scopes = new string[] {
                DriveService.Scope.Drive,
                DriveService.Scope.DriveAppdata,
                DriveService.Scope.DriveFile,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveMetadataReadonly,
                DriveService.Scope.DriveReadonly,
            }
            }.FromPrivateKey("-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCj/X3XtW6SpaSS\nr10/ZTMeuofh5mB/+IK1jMtJpIU1n6AiMnuTYDGFIFoy4hMJ5l7ZHrhV1YN6Or+q\nrWVLIYIpeoBlQ8mI+SOLEsMyBoetzyvEqqXrjhssC5jaEWCzYF2rQVfDPko6rIDs\n9fuI3ZlBG+epdu8Gk+u/oYd/Pm+MqfWDOTxYfS0tBeqzeHyOhMRmWQeAHnCXV6/H\nGb9NesOmqATd++AvjpoPo/S3VDZx9gIVbVI3q6vEhT4tcuOXk1P+TG2sRhjWFusY\noSkiC5g7xdonlGBQ7l35MYVo/rDkLGmav62MUGjulu5O0jJtcmWQuE9rCfkcye/O\n+me3Uu77AgMBAAECggEAFu2bSayRUCmer++a3wE8O4Ci/P+j4GCTjeSoi5xYD55I\n2l8qjfl2EwHp+pFHTiwKeNrltQKMRmuuXqOttOpzc28wQnhO1jrXMFuoNPdawv9j\nRDUDy8JrpCXe8iZZ2dqQXiBdr+umnzt9LyRZTdKF2eeX5Ua+trG51Wo1MqI9IHJt\nbwj6fjISxMaWfVTYnGeflVXAFXWkYNfJK7e0+evo5mxZ5Y4LKjKO9Y9OUZa86Ao/\nEgOKoXu20PXTALJ5ADSFeIdbr9bwpy7s19QbSpIbclRfShnf0/z+iIHTNW2CePpl\nWch93hkxUyh/ay5essd2Teg9743gnKqOHqn9MhrgAQKBgQDc7RyEZqBXZcxSlo+E\n4IEiZGqOifCJFmyEqJfgx60YkNJHvNXdSwOSbbnogjYiFQtuV8jGv+YhjwW2gcsk\nr+EjRXpwH4tkMiWn63iv6bZ9q84rWm5WVG17hffqqCcCD2zxFhhJ7dx3YBGWoM1W\ny7ijPw/5Y3A2tC3E2wTnwVc++wKBgQC+BmKiO1Mu7xxybRynvac2eT9QjokLR3j7\nzijk0bcggFRKgCs3NvggBPoBAdIoLXHS9U6WjwjkH76BFyYs9q81v92UMDUlZYME\nPOqywRuiRnBGmwzleYFKAQCsg9xZ9+XuzfgAVlBsB3GEnR9DXO/KKMvZp0Yptbcp\njMd9OGQQAQKBgQCVrgguJCHqVMwUAHIIQtr65DHVlNtk5c5sKpWL83zxMd1mQShc\nn+AxqynTv7TRbpSqE8ux7H4MqoviVRm/J3JPpVkI8jZMkjU1CbJg7OV5S5eJg+FR\nmC39DI0lbPvQx4a16JRYMlG2h14jQZsdfTUBVU6LVrXGOljwELvIERP9VQKBgGZf\ndEXoZhET+qReyiBIWUxMl+KjV4t/DtvBnmBf5yOYX1DfqeiOMbC2XaWrQHgqu3am\nd5c/KdAUlgJf4U45+/yeCBasvgUOoj3nP53b0TJkdlpjb9g01IV08tL+GvlRR0uX\nJTOxTJRWkj6ak1wsNncX8XKp3m/cGPhGgragabABAoGAaRWXmwMZfqS4fz9ATYlg\nbmLmt5qSu1iy1OfMFrDhFo2K4wMyvUbwv1+ZobuSX9d8z2zLXdIFoeDxK5ui7rIm\ny7/kqoQQjddFPHzmDvfKyyidCVgYNwjcqDV8FHpHghGfD7DTcHgFm1REdQdySFh4\nhrkdytIUS+q6btPA8973cuk=\n-----END PRIVATE KEY-----"));

            // Create the Drive service using the service account credential
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            // Define the search query to retrieve files with the specified name
            var listRequest = service.Files.List();
            listRequest.Q = $"name='{fileName}'";

            // Execute the search query
            var searchResponse = await listRequest.ExecuteAsync();

            // Check if any files match the search query
            if (searchResponse.Files == null || searchResponse.Files.Count == 0)
            {
                if (SetStatusText != null)
                    SetStatusText($"File '{fileName}' not found.");

                return;
            }

            // Get the first matching file
            var file = searchResponse.Files[0];

            if (SetStatusText != null)
                SetStatusText($"Downloading file: {file.Name}...");

            // Create the request to download the file
            var request = service.Files.Get(file.Id);

            // Get the file content as a stream
            using (var stream = new MemoryStream())
            {
                var result = await request.DownloadAsync(stream);

                if (result.Status != Google.Apis.Download.DownloadStatus.Completed)
                {
                    if (SetStatusText != null)
                        SetStatusText($"Failed to download {file.Name}: " + result.Exception.Message);

                    return;
                }

                try
                {
                    // Save the file content to the destination path
                    using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                    {
                        stream.Position = 0;
                        await stream.CopyToAsync(fileStream);
                    }
                }
                catch(Exception ex)
                {
                    if (SetStatusText != null)
                    {
                        SetStatusText($"File '{fileName}' downloaded failed!");
                        SetStatusText(ex.Message);
                    }
                }
            }

            if (SetStatusText != null)
                SetStatusText($"File '{fileName}' downloaded successfully!");
        }

        public static async Task UploadFileByName(string fileName, string fileContent, Action<string> SetStatusText = null)
        {
            // Create a service account credential using the service account email and required scopes
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer("instagram@instagram-388012.iam.gserviceaccount.com")
            {
                Scopes = new string[] {
                DriveService.Scope.Drive,
                DriveService.Scope.DriveAppdata,
                DriveService.Scope.DriveFile,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveMetadataReadonly,
                DriveService.Scope.DriveReadonly,
            }
            }.FromPrivateKey("-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCj/X3XtW6SpaSS\nr10/ZTMeuofh5mB/+IK1jMtJpIU1n6AiMnuTYDGFIFoy4hMJ5l7ZHrhV1YN6Or+q\nrWVLIYIpeoBlQ8mI+SOLEsMyBoetzyvEqqXrjhssC5jaEWCzYF2rQVfDPko6rIDs\n9fuI3ZlBG+epdu8Gk+u/oYd/Pm+MqfWDOTxYfS0tBeqzeHyOhMRmWQeAHnCXV6/H\nGb9NesOmqATd++AvjpoPo/S3VDZx9gIVbVI3q6vEhT4tcuOXk1P+TG2sRhjWFusY\noSkiC5g7xdonlGBQ7l35MYVo/rDkLGmav62MUGjulu5O0jJtcmWQuE9rCfkcye/O\n+me3Uu77AgMBAAECggEAFu2bSayRUCmer++a3wE8O4Ci/P+j4GCTjeSoi5xYD55I\n2l8qjfl2EwHp+pFHTiwKeNrltQKMRmuuXqOttOpzc28wQnhO1jrXMFuoNPdawv9j\nRDUDy8JrpCXe8iZZ2dqQXiBdr+umnzt9LyRZTdKF2eeX5Ua+trG51Wo1MqI9IHJt\nbwj6fjISxMaWfVTYnGeflVXAFXWkYNfJK7e0+evo5mxZ5Y4LKjKO9Y9OUZa86Ao/\nEgOKoXu20PXTALJ5ADSFeIdbr9bwpy7s19QbSpIbclRfShnf0/z+iIHTNW2CePpl\nWch93hkxUyh/ay5essd2Teg9743gnKqOHqn9MhrgAQKBgQDc7RyEZqBXZcxSlo+E\n4IEiZGqOifCJFmyEqJfgx60YkNJHvNXdSwOSbbnogjYiFQtuV8jGv+YhjwW2gcsk\nr+EjRXpwH4tkMiWn63iv6bZ9q84rWm5WVG17hffqqCcCD2zxFhhJ7dx3YBGWoM1W\ny7ijPw/5Y3A2tC3E2wTnwVc++wKBgQC+BmKiO1Mu7xxybRynvac2eT9QjokLR3j7\nzijk0bcggFRKgCs3NvggBPoBAdIoLXHS9U6WjwjkH76BFyYs9q81v92UMDUlZYME\nPOqywRuiRnBGmwzleYFKAQCsg9xZ9+XuzfgAVlBsB3GEnR9DXO/KKMvZp0Yptbcp\njMd9OGQQAQKBgQCVrgguJCHqVMwUAHIIQtr65DHVlNtk5c5sKpWL83zxMd1mQShc\nn+AxqynTv7TRbpSqE8ux7H4MqoviVRm/J3JPpVkI8jZMkjU1CbJg7OV5S5eJg+FR\nmC39DI0lbPvQx4a16JRYMlG2h14jQZsdfTUBVU6LVrXGOljwELvIERP9VQKBgGZf\ndEXoZhET+qReyiBIWUxMl+KjV4t/DtvBnmBf5yOYX1DfqeiOMbC2XaWrQHgqu3am\nd5c/KdAUlgJf4U45+/yeCBasvgUOoj3nP53b0TJkdlpjb9g01IV08tL+GvlRR0uX\nJTOxTJRWkj6ak1wsNncX8XKp3m/cGPhGgragabABAoGAaRWXmwMZfqS4fz9ATYlg\nbmLmt5qSu1iy1OfMFrDhFo2K4wMyvUbwv1+ZobuSX9d8z2zLXdIFoeDxK5ui7rIm\ny7/kqoQQjddFPHzmDvfKyyidCVgYNwjcqDV8FHpHghGfD7DTcHgFm1REdQdySFh4\nhrkdytIUS+q6btPA8973cuk=\n-----END PRIVATE KEY-----"));

            // Create the Drive service using the service account credential
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            // Define the search query to retrieve files with the specified name
            var listRequest = service.Files.List();
            listRequest.Q = $"name='{fileName}'";

            // Execute the search query
            var searchResponse = await listRequest.ExecuteAsync();

            var existingFile = searchResponse.Files.FirstOrDefault(f => f.Name == fileName);
            if (existingFile != null)
            {
                // Update the existing file
                var fileUpload = service.Files.Update(new File { Name = fileName }, existingFile.Id, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)), "application/json");
                var uploadStatus = await fileUpload.UploadAsync();

                if (uploadStatus.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    if (SetStatusText != null)
                        SetStatusText("File updated successfully");
                }
                else
                {
                    if (SetStatusText != null)
                    {
                        SetStatusText("File updated failed");
                        SetStatusText(uploadStatus.Exception.Message);
                    }
                }
            }
            else
            {
                // Create a new file
                var newFile = new File { Name = fileName };
                var fileUpload = service.Files.Create(newFile, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)), "application/json");
                var uploadStatus = await fileUpload.UploadAsync();

                if (uploadStatus.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    if (SetStatusText != null)
                        SetStatusText("File updated successfully");
                }
                else
                {
                    if (SetStatusText != null)
                    {
                        SetStatusText("File updated failed");
                        SetStatusText(uploadStatus.Exception.Message);
                    }
                }

                existingFile = newFile;
            }
        }
    }
}
