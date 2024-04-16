using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using Firebase.Database.Query;
using Org.Apache.Http.Conn;
using Plugin.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
//using static Xamarin.Essentials.Platform;

namespace GuessThePicBeta9
{
    [Activity(Label = "GameLobbyHost", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GameLobbyHost : Activity, View.IOnClickListener
    {
        private ListView listView;
        private TextView gameidview;
        private Button startGameButton;
        private GameEngine gameEngine;
        private GameInitiator gameInitiator;
        private TextView photosLeft;

        private Player currentPlayer;
        private string gameid;

        private Thread changes;
        private bool stopThread = false;

        private int picturesLeftCount;

        private string[] playersArray;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.game_lobby_host);

            this.currentPlayer = CurrentPlayer.playerPointer;

            listView = FindViewById<ListView>(Resource.Id.list123);
            gameidview = FindViewById<TextView>(Resource.Id.gameidview);
            startGameButton = FindViewById<Button>(Resource.Id.startGameButton);
            photosLeft = FindViewById<TextView>(Resource.Id.photosLeft);

            SetPhotosLeft();

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
            CheckForChanges();
            DisableStartGameButton();

            Thread ReadyPlayersThread;
            if (currentPlayer.isAdmin)
            {
                ReadyPlayersThread = new Thread(new ThreadStart(CheckForReadyPlayers));
                ReadyPlayersThread.Start();
            }
                
                

            //changes = new Thread(CheckForChanges);
            //changes.Start();
        }
        public override void OnBackPressed() //Disables the back button
        {
        }
        public async void SetPlayersList()
        {
            
            string[] arr = await FirebaseActions.GetPlayerNamesArray();
            if (arr != null)
            {
                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arr);
                listView.Adapter = adapter;
                this.playersArray = arr;
            }
        }
        public void SetPlayersListFromArray(string[] arr) //override to send to the event listener
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
                QuitToMainMenu();
            }
            else if (b.Text == "Insert photos")
            {
                //DisableStartGameButton(); //to not allow the game to start before the upload is done
                await PickImage();
                //EnableStartGameButton();
            }
            else if (b.Text == "Start Game")
            {
                //StopListeningForChanges();
                CreateProgressDialog("Starting...");

                List<ImageDatabasePointer> imageList = await FirebaseActions.GetImageDatabasePointerList();
                GameEngine temp = await FirebaseActions.GetGameEngine();

                temp.SetImageList(imageList);
                temp.ScambleImageList();
                await temp.SetCurrentImage();

                await FirebaseActions.UploadGamesEngine(temp);
                FirebaseActions.StartGameEngineDownload();

                CheckForDownloadedGameEngine();

                //FirebaseActions.StartGame();
            }
        }
        private async void QuitToMainMenu()
        {
            Intent intent;
            stopThread = true;
            await ReturnToMainMenuActions();
            intent = new Intent(this, typeof(MainActivity));
            base.StartActivity(intent);
        }

        public void DisableStartGameButton()
        {
            startGameButton.Clickable = false;
            startGameButton.SetTextColor(Android.Graphics.Color.DarkRed);
        }
        public void EnableStartGameButton()
        {
            startGameButton.Clickable = true;
            startGameButton.SetTextColor(Android.Graphics.Color.Black);
        }

        public async Task<bool> ReturnToMainMenuActions()
        {
            stopThread = true;
            if (currentPlayer.isAdmin)
                await KillLobby();
            else
                FirebaseActions.ExitFromLobby(currentPlayer);

            GameEngineSingleton.DeleteInstance();
            CurrentPlayer.DeletePlayerInstance();
            return true;
        }


        private async Task<bool> PickImage()
        {
            Image img = await PickImageFromPhone();
            if (img == null) return false;
            if (currentPlayer.isAdmin)
            {
                await ImageManagment(img);
                return true;
            }
            await ImageManagment(img);
            return true;


        }
        public async Task<bool> ImageManagment(Image img)
        {
            if (img == null) return false;

            UpdatePhotosLeft();

            return await FirebaseActions.UploadImageUser(img);
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
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
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
        
        public void GenerateGameID()
        {
            string id = gameInitiator.GetNewGameID();
            this.gameEngine.ID = id;
            this.gameidview.Text = id;
        }
        private async Task<bool> KillLobby()
        {
            return await FirebaseActions.KillLobby();
        }

        
        private void SetPhotosLeft()
        {
            photosLeft.Text = "Add 4 more photos";
            picturesLeftCount = 4;
        }

        public void UpdatePhotosLeft()
        {
            if (picturesLeftCount == 1)
            {
                photosLeft.Text = "You are ready to play";
                FirebaseActions.SetPlayerReady();
            }
            else
            {
                picturesLeftCount--;
                photosLeft.Text = $"Add {picturesLeftCount} more photos";
            }
        }
        private async void CheckForChanges()
        {
            while (!stopThread)
            {
                if (stopThread)
                {
                    break;
                }
                bool shouldDownloadGameEngine = false;
                if (!stopThread)
                {
                    SetPlayersList();
                    shouldDownloadGameEngine = await FirebaseActions.ShouldDownloadGameEngine();
                    if (shouldDownloadGameEngine)
                    {
                        stopThread = true;
                        DownloadGameEngine();
                        CreateProgressDialog("Starting...");
                        return;
                    }
                }
                if(!currentPlayer.isAdmin)
                {
                    if (!(await FirebaseActions.IsLobbyOn()))
                    {
                        QuitToMainMenu();
                        return;
                    }
                }
                Thread.Sleep(500);
            }
        }
        public async void DownloadGameEngine()
        {
            GameEngine g = await FirebaseActions.GetGameEngine();
            GameEngineSingleton.SetInstance(g);

            FirebaseActions.SetPlayerDownloadedGameEngine();
            //need to do something to indicate that the game had been downloaded
            WaitForStartGame();
        }
        public async void WaitForStartGame()
        {
            while (true)
            {
                if (await FirebaseActions.IsGameStarted())
                {
                    StartGame();
                    return;
                }
                Thread.Sleep(100);
            }
            
        }

        public void StartGame()
        {
            Intent intent;
            intent = new Intent(this, typeof(GameplayScreen));
            base.StartActivity(intent);
        }
        public void StopListeningForChanges()
        {
            //this.changes.Abort();
            this.stopThread = true;
        }

        public async void CheckForReadyPlayers()
        {
            while (!stopThread)
            {
                Thread.Sleep(1000);
                if (stopThread)
                {
                    break;
                }
                bool isEveryoneReady = false;
                if (!FirebaseActions.IsPointerNull())
                {
                    if (this.playersArray != null)
                        isEveryoneReady = await FirebaseActions.IsAllReady(this.playersArray);
                    if (isEveryoneReady)
                    {
                        EnableStartGameButton();
                    }
                    else
                        DisableStartGameButton();
                }
            }
        }
        public async void CheckForDownloadedGameEngine()
        {
            while(true)
            {
                Thread.Sleep(500);
                if (!FirebaseActions.IsPointerNull())
                {
                    if (await FirebaseActions.IsDownloadedGameEngine(this.playersArray))
                    {
                        FirebaseActions.StartGame();
                        return;
                    }
                }
            }
        }
        private void CreateProgressDialog(string message)
        {
            ProgressDialog p = ProgressDialog.Show(this, message, "Please wait", true);
            p.SetCancelable(false);
            p.SetProgressStyle(ProgressDialogStyle.Horizontal);
            p.SetMessage("Loading...");
            p.Show();
        }
    }
}