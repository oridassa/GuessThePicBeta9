using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuessThePicBeta9
{
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : Activity, View.IOnClickListener
    {
        private EditText gameidinput;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login_screen);

            this.gameidinput = FindViewById<EditText>(Resource.Id.gameid);
        }
        public async void OnClick(View v)
        {
            Intent intent;
            Button b = (Button)v;
            if (b.Text == "Quit to main menu")
            {
                intent = new Intent(this, typeof(MainActivity));
                GameEngineSingleton.DeleteInstance();
                CurrentPlayer.DeletePlayerInstance();
                base.StartActivity(intent);
            }
            else if (b.Text == "Join Game")
            {
                string gameID = gameidinput.Text.ToString();
                bool isGameIOnline = await FirebaseActions.IsGameOnline(gameID);
                if (!isGameIOnline) 
                {
                    Toast.MakeText(this, "The Game Is Not Active", ToastLength.Short).Show();
                    return;
                }
                else //this is when there is an active game
                {
                    await FirebaseActions.GameSetup(gameID);
                    GameEngine inportedGameEngine = await FirebaseActions.GetGameEngine(); //gets the gameEngine great!!!!!
                    
                    GameEngineSingleton.SetInstance(inportedGameEngine);

                    FirebaseActions.AddPlayerToLobby(CurrentPlayer.playerPointer);

                    intent = new Intent(this, typeof(GameLobbyHost));
                    base.StartActivity(intent);
                }
                //intent = new Intent(this, typeof(GameLobbyHost));
                //base.StartActivity(intent);
            }
        }
    }
}