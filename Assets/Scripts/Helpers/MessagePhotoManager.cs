namespace Assets.Scripts.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using UnityEngine;

    public static class MessagePhotoManager
    {
        public static async Task EnsurePhotoExists(string imageName)
        {
            var storagePath = $"{Application.persistentDataPath}/{imageName}";

            if (!File.Exists(storagePath))
            {
                await DriveHelper.DownloadFileByName(imageName, storagePath);
            }
        }

        public static Sprite LoadSprite(string imageName)
        {
            var storagePath = $"{Application.persistentDataPath}/{imageName}";

            // Load the image as a Texture2D
            var texture = new Texture2D(2, 2);

            texture.LoadImage(File.ReadAllBytes(storagePath));

            // Create a Sprite from the Texture2D
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
    }
}
