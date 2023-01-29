using System.IO;
using UnityEditor;
using UnityEngine;

public static class SpriteAtlasSlicer
{
    [MenuItem("Assets/RenTool/Slice sprite atlas to Png")]
    static void SliceSpriteAtlasToPng()
    {
        Texture2D image = Selection.activeObject as Texture2D;//獲取旋轉的對象
        string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(image));//獲取路徑名稱
        string path = rootPath + "/" + image.name + ".PNG";//圖片路徑名稱

        TextureImporter texImp = AssetImporter.GetAtPath(path) as TextureImporter;//獲取圖片入口
        AssetDatabase.CreateFolder(rootPath, image.name);//創建文件夾

        foreach (SpriteMetaData metaData in texImp.spritesheet)//遍歷小圖集
        {
            Texture2D myimage = new Texture2D((int)metaData.rect.width, (int)metaData.rect.height);

            //abc_0:(x:2.00, y:400.00, width:103.00, height:112.00)
            for (int y = (int)metaData.rect.y; y < metaData.rect.y + metaData.rect.height; y++)//Y軸像素
            {
                for (int x = (int)metaData.rect.x; x < metaData.rect.x + metaData.rect.width; x++)
                    myimage.SetPixel(x - (int)metaData.rect.x, y - (int)metaData.rect.y, image.GetPixel(x, y));
            }

            //轉換紋理到EncodeToPNG兼容格式
            if (myimage.format != TextureFormat.ARGB32 && myimage.format != TextureFormat.RGB24)
            {
                Texture2D newTexture = new Texture2D(myimage.width, myimage.height);
                newTexture.SetPixels(myimage.GetPixels(0), 0);
                myimage = newTexture;
            }
            var pngData = myimage.EncodeToPNG();

            //AssetDatabase.CreateAsset(myimage, rootPath + "/" + image.name + "/" + metaData.name + ".PNG");
            File.WriteAllBytes(rootPath + "/" + image.name + "/" + metaData.name + ".PNG", pngData);
            // 刷新資源窗口界面
            AssetDatabase.Refresh();
        }
    }
}