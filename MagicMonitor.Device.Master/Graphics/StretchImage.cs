﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.UI;

namespace MagicMonitor.Device.Master.Graphics
{
    public class StretchImage
    {
        public StretchImage(Bitmap fullScreenBitmap, Font DisplayFont)
        {
            Bitmap bmWaterfall = new Bitmap(WaterFallJpg.WaterFall, Bitmap.BitmapImageType.Jpeg);
            fullScreenBitmap.Clear();
            fullScreenBitmap.Flush();
            fullScreenBitmap.DrawImage(0, 0, bmWaterfall, 0, 0, bmWaterfall.Width, bmWaterfall.Height);
            fullScreenBitmap.Flush();
            for (int i = 0; i < 35; i++)
            {
                fullScreenBitmap.StretchImage(0, 0, bmWaterfall, bmWaterfall.Width + (i + 1) * 7, bmWaterfall.Height + (i + 1) * 7, 255);
                InformationBar.DrawInformationBar(fullScreenBitmap, DisplayFont, InfoBarPosition.bottom, "Stretch Image");
                fullScreenBitmap.Flush();
            }

            bmWaterfall.Dispose();
        }
    }
}
