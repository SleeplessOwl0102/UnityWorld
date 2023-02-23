using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    /// <summary>
    /// Provides utility extensions to copy data from substance to unity textures.
    /// </summary>
    internal static class TextureExtensions
    {
        internal static byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int length = Marshal.SizeOf(typeof(Color32)) * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        internal static Color32[] FlipY(Color32[] input, int width, int height)
        {
            Color32[] output = new Color32[input.Length];

            for (int y = 0, i = 0, o = output.Length - width; y < height; y++, i += width, o -= width)
                Array.Copy(input, i, output, o, width);

            return output;
        }
    }
}