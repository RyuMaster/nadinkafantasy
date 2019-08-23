/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsDemos
{
    [AddComponentMenu("Infinity Code/Online Maps/Demos/GUITextureSize")]
    public class GUITextureSize : MonoBehaviour
    {
        private int screenWidth;
        private int screenHeight;

        private void Start()
        {
            UpdateTexture();
        }

        private void Update()
        {
            if (screenWidth != Screen.width || screenHeight != Screen.height) UpdateTexture();
        }

        private void UpdateTexture()
        {
#if UNITY_4_6
            GUITexture gt = guiTexture;
            Rect pi = guiTexture.pixelInset;
#else
            GUITexture gt = GetComponent<GUITexture>();
            Rect pi = gt.pixelInset;
#endif
            screenWidth = Screen.width;
            screenHeight = Screen.height;

            float sw = screenWidth / (float) gt.texture.width;
            float sh = screenHeight / (float) gt.texture.height;

            if (sw > sh)
            {
                pi.width = screenWidth;
                pi.height = sw * gt.texture.height;
            }
            else
            {
                pi.height = screenHeight;
                pi.width = sh * gt.texture.width;
            }

            pi.x = pi.width / -2;
            pi.y = pi.height / -2;

            gt.pixelInset = pi;
        }
    }
}