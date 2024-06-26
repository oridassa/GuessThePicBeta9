﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GuessThePicBeta9
{
    public class Image
    {
        public byte[] ImageBytesData { get; set; } //נתונים של התמונה
        public string SourcePlayer { get; set; } //a player which the image came from
        public Image(byte[] ImageBytesData, string SourcePlayer)//gets name and byte data
        {
            this.ImageBytesData = ImageBytesData;
            this.SourcePlayer = SourcePlayer;
        }
        public Image()
        {

        }

        public Image(byte[] imageBytesDATA)  //gets only byte data
        {
            this.ImageBytesData = imageBytesDATA;
            this.SourcePlayer = CurrentPlayer.name;
        }

        public Image(string base64Image) 
        {
            this.SourcePlayer = CurrentPlayer.name;
            this.ImageBytesData = this.ConvertBase64ToBytes(base64Image);
        }
        public Image(MemoryStream mStream)
        {
            this.SourcePlayer = CurrentPlayer.name; 
            this.ImageBytesData = mStream.ToArray();
        }
        public Image(string base64Image, string sourcePlayer)
        {
            this.SourcePlayer = sourcePlayer;
            this.ImageBytesData = this.ConvertBase64ToBytes(base64Image);
        }
        public Image(Image other)
        {
            this.ImageBytesData = other.ImageBytesData;
            this.SourcePlayer = other.SourcePlayer;
        }

        public string ConvertBytesToBase64(byte[] imageBytes)
        {
            string imageBase64 = Convert.ToBase64String(imageBytes, Base64FormattingOptions.InsertLineBreaks);// convert bytes to base64
            return imageBase64;
        }

        public byte[] ConvertBase64ToBytes(string base64Image)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            return imageBytes;
        }

        public Bitmap GetBipmapImage() //returns the image as a bitmap
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(this.ImageBytesData, 0, this.ImageBytesData.Length);
            return bitmap;
        }
        public bool SetImageToImageview(ImageView imageView) //sets the image to an imageView
        {
            if (this.ImageBytesData != null)
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(this.ImageBytesData, 0, this.ImageBytesData.Length);
                imageView.SetImageBitmap(bitmap);
                return true;
            }
            return false;

        }
    }
}