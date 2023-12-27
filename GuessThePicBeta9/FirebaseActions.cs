using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Database;
using Firebase.Database.Query;

namespace GuessThePicBeta9
{
    public static class FirebaseActions
    {
        private static FirebaseClient firebaseClient = new
            FirebaseClient("https://guessthepic-4d369-default-rtdb.europe-west1.firebasedatabase.app/");
        public static string GetPlayersArray()
        {
            throw new NotImplementedException();
        }
        public static void UploadGamesEngine(GameEngine g)
        {
            firebaseClient.Child("Games")
                .Child("holder").PutAsync<GameEngine>(g);
            
            
        }

    }
}