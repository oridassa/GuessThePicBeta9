using Android.App;
using Android.App.AppSearch;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessThePicBeta9
{
    public class ImageDatabasePointer
    {
        public string[] Keys { get; set; } //the directions to the image from pointer
        public ImageDatabasePointer(string player, string imgKey) 
        { 
            Keys = new string[] { "UsersPictures", player, imgKey };
        }
        public ImageDatabasePointer(string[] keys)
        {
            Keys = keys;
        }
        public ImageDatabasePointer(string imgKey) 
        {
            Keys = new string[] { "UsersPictures", CurrentPlayer.name, imgKey };
        }
        public ImageDatabasePointer() { }   
        public string First()
        {
            return Keys[0];
        }
        public string Second()
        {
            return Keys[1];
        }
        public string Third()
        {
            return Keys[2];
        }
        public async Task<Image> GetImageObject() //returns the coresponding image for the imagePointer
        {
            return await FirebaseActions.GetImageByPointer(this);
        }
        
    }
}