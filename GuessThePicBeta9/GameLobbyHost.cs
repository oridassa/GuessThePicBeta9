using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Apache.Http.Conn;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GuessThePicBeta9
{
    [Activity(Label = "GameLobbyHost")]
    public class GameLobbyHost : Activity, View.IOnClickListener
    {
        private ListView listView;
        private TextView gameidview;
        private Button startGameButton;
        private GameEngine gameEngine;
        private GameInitiator gameInitiator;

        private Player currentPlayer;
        private string gameid;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.game_lobby_host);

            this.currentPlayer = CurrentPlayer.playerPointer;

            listView = FindViewById<ListView>(Resource.Id.list123);
            gameidview = FindViewById<TextView>(Resource.Id.gameidview);
            startGameButton = FindViewById<Button>(Resource.Id.startGameButton);

            startGameButton.Visibility = DetermineIfHost();

            if (currentPlayer.isAdmin)
            {
                gameEngine = GameEngineSingleton.GetInstance();
                gameInitiator = new GameInitiator();
                GenerateGameID();
                await FirebaseActions.GameSetup(gameEngine.ID, gameEngine);
            }
            else
            {
                gameEngine = await FirebaseActions.GetGameEngine();
                this.gameidview.Text = gameEngine.ID.ToString();
            }

            SetPlayersList();
        }
        public async void SetPlayersList()
        {
            string[] arr = await FirebaseActions.GetPlayerNamesArray();
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arr);
            listView.Adapter = adapter;
        }
        public void SetPlayersList(string[] arr) //override to send to the event listener
        {
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arr);
            listView.Adapter = adapter;
        }

        public override void OnRequestPermissionsResult(int requestCode,
            string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public async void OnClick(View v)
        {
            Intent intent;
            Button b = (Button)v;
            if (b.Text == "Quit to main menu")
            {
                if (currentPlayer.isAdmin)
                    KillLobby();
                else
                    FirebaseActions.ExitFromLobby(currentPlayer);
                    
                GameEngineSingleton.DeleteInstance();
                CurrentPlayer.DeletePlayerInstance();
                intent = new Intent(this, typeof(MainActivity));
                base.StartActivity(intent);
                
            }
            else if (b.Text == "Insert photos")
            {
                //player pressed "Insert photos"
                //UploadPictures();
                await PickImage();

            }
            else if (b.Text == "Start Game")
            {
                //host wants to start the game
                FirebaseActions.UploadGamesEngine(gameEngine);
                StartGame();
            }
        }



        private async Task<bool> PickImage()
        {
            Image img = await PickImageFromPhone();
            if (img == null) return false;
            if (currentPlayer.isAdmin)
            {
                ImageManagmentHost(img);
                return true;
            }
            ImageManagmentPlayer(img);
            return true;


        }

        public async Task<bool> PicImageHost()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, $"pick another photo", ToastLength.Short).Show();
                return false;
            }
            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full
            });
            if (file != null)
            {
                byte[] bytes = File.ReadAllBytes(file.Path);
                if (bytes.Length > 0)
                {
                    gameEngine.AddImage(new Image(bytes, "noam"));
                    return true;
                }
            }
            return false;
        }

        public void ImageManagmentHost(Image img)
        {
            if (img == null) return;
            FirebaseActions.UploadImageUser(img);

        }
        public void ImageManagmentPlayer(Image img)
        {
            if (img == null) return;
            FirebaseActions.UploadImageUser(img);
                
        }

        public async Task<Image> PickImageFromPhone()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, $"pick another photo", ToastLength.Short).Show();
                return null;
            }
            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full
            });
            if (file != null)
            {
                byte[] bytes = File.ReadAllBytes(file.Path);
                if (bytes.Length > 0)
                {
                    return new Image(bytes, currentPlayer.name);
                }
            }
            return null;
        }

        private ViewStates DetermineIfHost()//if player is host there will be a button to start the game, else, there would not be
        {
            if (currentPlayer.isAdmin)
                return ViewStates.Visible;
            return ViewStates.Gone;
        }

        private void SetPlayerListView()
        {

        }
        public async void StartGame()
        {
            Intent intent;

            if (gameEngine.HasImages())
            {
                intent = new Intent(this, typeof(GameplayScreen));
                base.StartActivity(intent);
            }
            else
            {
                Toast.MakeText(this, "No pictures added", ToastLength.Short).Show();
            }


            
        }
        public void GenerateGameID()
        {
            string id = gameInitiator.GetNewGameID();
            this.gameEngine.ID = id;
            this.gameidview.Text = id;
        }
        private void KillLobby()
        {
            FirebaseActions.KillLobby();
        }
    }
}