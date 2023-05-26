namespace Assets.Scripts.Helpers
{
    using Cysharp.Threading.Tasks;

    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Drive.v3;
    using Google.Apis.Services;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using UnityEngine.Networking;

    public static class DriveHelper
    {
        public static async Task DownloadFileByName(string fileName, string savePath, Action<string> SetStatusText = null)
        {
            if (SetStatusText != null)
                SetStatusText($"Searching for file: {fileName}...");

            // Create a service account credential using the service account email and required scopes
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer("insta-666@instagram-387816.iam.gserviceaccount.com")
            {
                Scopes = new string[] {
                DriveService.Scope.Drive,
                DriveService.Scope.DriveAppdata,
                DriveService.Scope.DriveFile,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveMetadataReadonly,
                DriveService.Scope.DriveReadonly,
            }
            }.FromPrivateKey("-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDOhT7QRzFaNOvb\nRqWMJyiJE2KvGjPs8aga6apCYO0K61bULau1omkvi6Cuyx9/3+whS8lwhvNPZl+D\n4UAarCcsNcdhi9zU3NLuSL+uS6lkht3FSlQc1oIetMF49mU50s+mkB4SkLoBc2yp\nGN+QSylJhk1XNh/qigmfc/vJcpW/7BGD2Bpugf2onoQaZcgEcpG8bHConLiyRWpu\nMpjEDfe6X18yVoth20bRlmDKkjRBiey5rrRPMof6gaefX+ghZAmrB7F2DaWF0ucG\nBJbtbg54cqKGa7n7W79ia3faAIOoyqqE52cLDMpCPAYLF1Goo7floCMj9yfn9v2g\n1+V3X7D3AgMBAAECggEACpJquWyLj1SaNIcxE6Wh+Ae/INgJ4ORkP744qwhWS+SU\nPDRewaquER2vJ2cTa7nv5TjlFBa0cP6Km9rK8tm0xsO1kIUBfv+hkti0TmQQtrh4\ntPOtyxCSvYrGYSIr4xBWEWCF9D6tvEPx7n/ezJsP8OWaBp3+d2Etm5iyXzObGt5k\nyEvFNQ00Wk2x41FpnVVt1/Usj4s2eBagcYKjKiNnGCPB1RdYKhYmlXdJ+Fp4LG7+\nDwRBAnaWLtxzcIbN6yArXBouqWE7aUEroNwdO0H+P95Cyq3WwKDoY48p8iZ3xGqP\nk/mwCiWZhARCT4DV6ajzlJgJzwHXWdtaesZ2lvEogQKBgQD1Frslw4WOzEGF1cEX\nOxVV4vkPwT0HSe6kVgMNyXUiO4MaUWckOPBeBHOGcjslcrDmNkfiWWfzQKrC2xMp\n8QSGzv4FJedvaULg6+TxxP+35RtzIVtr86ZW4UfzHCCc7tuDKX+ZtO5wKqHaa/Y6\nOOe/PlTNDyT1jRTak9ZMsS+kYQKBgQDXtvPfdbWqgJJB7yGxvz76OMGYd9BfERin\nUvGu3lqtYFLeV7shZCiH538+azJW8Sogjv5LjhRCvzv3540/UmqDXU1XUvSNQBxp\nULGnrnwGs58mtXoB39oGisVCItJxlm9iEnqBKULIzMVrcu/GNV6/rcAFSUMNXGO/\nYncoRp9UVwKBgB6c+j9bTsFpbf3Dl1zJen7B3Q5EutTAPVi1jagZM9JU0Klm5ZU2\nr39u2uc+OXkR/FqlCRGSzVMrDqlMJ/ajLkeQb1ZBR8k4wqvJi01PqAlWSrl0vzCa\nvHo5pX0OVFyF0VFuy4tteCL2kETyG01mcjwHExyR+bHaL3Kl6KynrSXBAoGBAJf4\nuYkVqX3MhL3U1d0eyiSItcNPrco8Bi1jnwc+eY2pzdf84MYehPtyGVAFP3rG/pHf\ni8H4/8ciaQI27GjPDGEt6135AHc+0oVTp0VmBNTH2PjxY1pMtZJkU6JLXA1QGXpz\nYED8q6NBuFgBqnDuiqjppN1Uhtuz2kYr/ZyvXHKTAoGBAKhPVPyCwWj7nY+0SvQC\n36A+dq4IUnsyNIDIzf08FUn8zYxGylBpdcCb1c9iqx5DY4jhABm2Z4wccs1RgzMT\nyJhkCVnnLnXMF4BtlCVo/WcBcYMYZmXt5P/YCBzvd+hm7yq+BoZVTan4na1VyZDD\nzV8lZ0lr8M0XXk/gYRbtZ5tC\n-----END PRIVATE KEY-----"));

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

            using (UnityWebRequest unityWebRequest = UnityWebRequest.Get($"https://drive.google.com/uc?export=download&id={file.Id}"))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerFile(savePath);

                await unityWebRequest.SendWebRequest();

                if (unityWebRequest.result != UnityWebRequest.Result.Success)
                {
                    if (SetStatusText != null)
                        SetStatusText($"Failed to download {file.Name}: " + unityWebRequest.error);

                    return;
                }

                if (SetStatusText != null)
                    SetStatusText($"File '{file.Name}' downloaded successfully!");
            }
        }

    }
}
