using UnityEngine;

namespace Zaubar.VideoPackage
{
    public class VideoAspectRatio
    {
        public static Vector3 GetAspectRatio(Texture sourceTexture, Vector3 localScaleAtStart)
        {
            var width = sourceTexture.width;
            var height = sourceTexture.height;

            var diff = (float)width / (float)height;
            var newScale = new Vector3((localScaleAtStart.x * diff), localScaleAtStart.y, localScaleAtStart.z);

            return newScale;
        }
    }
}