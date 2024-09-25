// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using nanoFramework.UI;

namespace MagicMonitor.Device.Master.Graphics
{
    public enum InfoBarPosition
    {
        Top,
        bottom
    }

    public static class InformationBar
    {
        public static void DrawInformationBar(Bitmap theBitmap, Font DisplayFont, InfoBarPosition pos, string TextToDisplay)
        {
            theBitmap.DrawRectangle(Color.White, 0, 0, theBitmap.Height - 20, 320, 22, 0, 0, Color.White,
                0, theBitmap.Height - 20, Color.White, 0, theBitmap.Height, Bitmap.OpacityOpaque);
            theBitmap.DrawText(TextToDisplay, DisplayFont, Color.Black, 0, theBitmap.Height - 20);
        }
    }
}
