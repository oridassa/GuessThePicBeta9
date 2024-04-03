using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Android.Service.Autofill;
using Android.Graphics;
using AndroidX.Browser.Trusted;
using Android.Media;
using System.IO;
//
using Java.IO;
using Java.Nio;
using Android.App.AppSearch;
//


namespace GuessThePicBeta9
{
    [Activity(Label = "GameplayScreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GameplayScreen : Activity, View.IOnClickListener
    {
        private TextView roundcounter;
        private ImageView imageView;
        private GridLayout grid;

        private ProgressBar progressBar;
        private Handler handler = new Handler();

        private Button pick;

        private IList<Button> buttons;

        List<string> playersArray;

        private Player currentPlayer = CurrentPlayer.playerPointer;

        private int timePoints;

        private Image image;

        GameEngine gameEngine;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.gameplay_screen);

            this.roundcounter = FindViewById<TextView>(Resource.Id.roundcount);
            this.imageView = FindViewById<ImageView>(Resource.Id.img);
            this.grid = FindViewById<GridLayout>(Resource.Id.grid);
            this.progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            this.buttons = new List<Button>();

            InitializeButtons();

            this.timePoints = 0;

            gameEngine = GameEngineSingleton.GetInstance();

            SetTitle();

            image = gameEngine.GetCurrentImage();
            SetImage();

            // Start the progress bar update thread
            var thread = new Thread(UpdateProgressBar);
            thread.Start();
        }

        private void SetImage()
        {
            Bitmap bitmap = image.GetBipmapImage();

            imageView.SetImageBitmap(bitmap);
        }

        private void UpdateProgressBar()
        {
            for (int i = 0; i <= 1000; i++)
            {
                // Update progress on UI thread
                handler.Post(() => progressBar.Progress = i);

                // Sleep for a short duration to simulate progress
                Thread.Sleep(5);
                timePoints = i;
            }

            // After 5 seconds, post the function to be executed on the UI thread
            handler.Post(() => SetNextImage());
            handler.Post(() => ColorTheButton());
            handler.Post(() => RemoveButtonClickabillity());
            Thread.Sleep(2000);

            handler.Post(() => ShowScoreBoard());
        }
        private void InitializeButtons()
        {
            playersArray = GameEngineSingleton.GetInstance().Players.Keys.ToList();

            foreach (string player in playersArray)
            {
                Button tempButton = new Button(this);
                tempButton.Text = player;
                tempButton.SetOnClickListener(this);
                grid.AddView(tempButton);
                buttons.Add(tempButton);
            }
        }
        private void ShowScoreBoard()
        {
            Intent intent = new Intent(this, typeof(ScoreBoardActivity));
            base.StartActivity(intent);
        }

        public void ColorTheButton()
        {
            if (pick != null)
                if (pick.Text == image.SourcePlayer)
                    pick.SetBackgroundColor(Color.Green);
                else
                    pick.SetBackgroundColor(Color.Red);
            Toast.MakeText(this, $"{currentPlayer.points}", ToastLength.Short).Show();
            pick = null;
        }

        public void OnClick(View v)
        {
            Button pressedButton = (Button)v;
            if (pick == null)
            {
                pick = pressedButton;
                if(pick.Text == image.SourcePlayer)
                {
                    currentPlayer.AddPoints(timePoints);
                }
                pick.SetBackgroundColor(Color.DarkGray);
            }
            FirebaseActions.UploadNamePointsString();
        }
        public void RemoveButtonClickabillity()
        {
            foreach(Button button in buttons)
            {
                button.Clickable = false;
            }
        }
        public void SetNextImage()
        {
            if (!gameEngine.IsLastImage())
            {
                gameEngine.SetNextImage();
            }
        }
        public void SetTitle() 
        {
            roundcounter.Text = $"Round {gameEngine.GetRoundNum()}";
        }
    }
}