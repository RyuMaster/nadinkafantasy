/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

public class OnlineMapsImporter : AssetPostprocessor
{  
	void OnPreprocessTexture()
	{
		if (assetPath.Contains("Resources/OnlineMapsTiles")) 
        {
			TextureImporter textureImporter = assetImporter as TextureImporter;
			textureImporter.mipmapEnabled = false;
			textureImporter.isReadable = true;
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
            textureImporter.textureFormat = TextureImporterFormat.RGB24;
#endif
            textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.maxTextureSize = 256;
		}
	}
}