using MetroDiplomaDrugiDel.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MetroDiplomaDrugiDel.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MediaPlayerPage : MetroDiplomaDrugiDel.Common.LayoutAwarePage
    {
        StorageFolder videoLibrary = Windows.Storage.KnownFolders.VideosLibrary;
        private FrameworkElement SeekControl;
        private DispatcherTimer progressTimer;
        private bool _Rewinding = false;
        private bool _Forwarding = false;
        private Double _InrementalValue = 3;

        public MediaPlayerPage()
        {
            this.InitializeComponent();
            this.mediaPlayer.Loaded += mediaPlayer_Loaded;
        }

        void mediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            //this.mediaPlayer.Volume = 80;
            this.volumeControl.Value = 80;
            NaloziSeznamPredvajanja();
            this.mediaPlayer.Loaded -= mediaPlayer_Loaded;

            // Turn of any FF or RW
            _Rewinding = false;
            _Forwarding = false;
            _InrementalValue = 3;
        }

        

        

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void volumeControl_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.mediaPlayer.Volume = e.NewValue;
        }


        private void NaloziSeznamPredvajanja()
        {
            List<MediaFile> sezPredvajaj = new List<MediaFile>();
            sezPredvajaj.Add(new MediaFile("niceday.wmv", "     0:00"));
            this.lvPredvajaj.ItemsSource = sezPredvajaj;
        }

        private async void lvPredvajaj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lvPredvajaj.SelectedItem != null && this.lvPredvajaj.SelectedItem is MediaFile)
            {
                var izbranVideo = this.lvPredvajaj.SelectedItem as MediaFile;
                //var pathZaIzbranVideo = videoLibrary.DisplayName + "\\" + izbranVideo.Naziv;
                var tmpFile = await videoLibrary.GetFileAsync(izbranVideo.Naziv);
                this.mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
                try
                {
                    mediaPlayer.SetSource((await tmpFile.OpenAsync(FileAccessMode.Read)), "vmw");
                }
                catch (Exception ex)
                {
                    this.mediaPlayer.MediaOpened -= mediaPlayer_MediaOpened;
                }
            }
        }

        void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            this.mediaPlayer.MediaOpened -= mediaPlayer_MediaOpened;
            this.mediaPlayer.MediaEnded += mediaPlayer_MediaEnded;
            this.progressBar.Maximum = this.mediaPlayer.NaturalDuration.TimeSpan.Seconds;

            this.progressTimer = new DispatcherTimer();
            this.progressTimer.Interval = TimeSpan.FromSeconds(1);
            this.progressTimer.Tick += progressTimer_Tick;
        }

        void progressTimer_Tick(object sender, object e)
        {
            if (_Rewinding || _Forwarding)
            {
                int newPosition = (int)(this.mediaPlayer.Position.TotalSeconds);

                if (_Rewinding)
                {
                    newPosition = (int)(newPosition - _InrementalValue);
                }

                if (_Forwarding)
                {
                    newPosition = (int)(newPosition + _InrementalValue);
                }

                this.mediaPlayer.Position = new TimeSpan(0, 0, newPosition);
            }
            else
            {
                this.progressBar.Value = this.mediaPlayer.Position.Seconds;
            }

        }

        void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.lvPredvajaj.SelectedItem = null;
            this.mediaPlayer.MediaEnded -= mediaPlayer_MediaEnded;
            this.progressBar.Value = 0;
            this.mediaPlayer.Position = new TimeSpan(0, 0, 0);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.Play();
            progressTimer.Start();
        }

        private async void btnOpenMediaFile_Click(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".wmv");
            var file = await picker.PickSingleFileAsync();
            mediaPlayer.SetSource((await file.OpenAsync(FileAccessMode.Read)), "vmw"); 
        }

        private void btnRewind_Click(object sender, RoutedEventArgs e)
        {
            RewindVideo(null);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            PauseVideo(null);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.Stop();
            progressTimer.Stop();
            //Setcur
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            ForwardVideo(null);
        }

        public void ForwardVideo(object param)
        {
            if (_Rewinding)
            {
                _InrementalValue = 3;
            }

            _Rewinding = false;
            _Forwarding = true;
            this.mediaPlayer.Pause();

            _InrementalValue++;

            if (!progressTimer.IsEnabled)
            {
                progressTimer.Start();
            }
        }

        public void RewindVideo(object param)
        {
            if (_Forwarding)
            {
                _InrementalValue = 3;
            }

            _Rewinding = true;
            _Forwarding = false;
            this.mediaPlayer.Pause();

            _InrementalValue++;

            if (!progressTimer.IsEnabled)
            {
                progressTimer.Start();
            }
        }

        public void PauseVideo(object param)
        {
            // We only want to Pause if the media is Playing
            if (this.mediaPlayer.CurrentState == MediaElementState.Playing)
            {
                // If we can Pause the Video, Pause it
                if (this.mediaPlayer.CanPause)
                {
                    // Pause Video
                    this.mediaPlayer.Pause();
                }
                else
                {
                    // We can't pause the Video so Stop it
                    this.mediaPlayer.Stop();
                }

                if (progressTimer.IsEnabled)
                {
                    progressTimer.Stop();
                }
            }
            else
            {
                // The Media is not Playing so we are Paused
                // Play Video
                this.mediaPlayer.Play();
                progressTimer.Start();
            }

        }

        public void StopVideo(object param)
        {
            _Rewinding = false;
            _Forwarding = false;
            _InrementalValue = 3;

            // Stop Video
            this.mediaPlayer.Stop();
            progressTimer.Stop();
            //SetCurrentPosition();
        }

        public void SetSeekControl(object param)
        {
            // Hook Events into the Seek Control
            SeekControl = (FrameworkElement)param;
            SeekControl.PointerPressed += SeekControl_PointerPressed;
            //SeekControl.MouseLeftButtonDown += new MouseButtonEventHandler(SeekControl_MouseLeftButtonDown);
        }

        void SeekControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            double position = e.GetCurrentPoint(SeekControl).Position.X;
            double percent = position / SeekControl.ActualWidth;
            Seek(percent);
        }

        

        

        //private void SeekControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    double position = e.GetPosition(SeekControl).X;
        //    double percent = position / SeekControl.ActualWidth;
        //    Seek(percent);
        //}

        private void Seek(double percentComplete)
        {
            _Rewinding = false;
            _Forwarding = false;
            _InrementalValue = 3;

            TimeSpan duration = this.mediaPlayer.NaturalDuration.TimeSpan;
            int newPosition = (int)(duration.TotalSeconds * percentComplete);
            this.mediaPlayer.Position = new TimeSpan(0, 0, newPosition);
            //SetCurrentPosition();
        }

        #region Time display

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (_Rewinding || _Forwarding)
            {
                int newPosition = (int)(this.mediaPlayer.Position.TotalSeconds);

                if (_Rewinding)
                {
                    newPosition = (int)(newPosition - _InrementalValue);
                }

                if (_Forwarding)
                {
                    newPosition = (int)(newPosition + _InrementalValue);
                }

                this.mediaPlayer.Position = new TimeSpan(0, 0, newPosition);
            }
            
            //SetCurrentPosition();
        }

        private void progressBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            
        }

        private void progressBar_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.mediaPlayer.Pause();
            var a = e.GetCurrentPoint(progressBar).Position.X;
            this.mediaPlayer.Position = new TimeSpan(0,0,(int)a);
            this.mediaPlayer.Play();
        }

        //private void SetCurrentPosition()
        //{
        //    // If the Media play is complete stop the media
        //    if (CurrentPositionProperty > 0)
        //    {
        //        if (CurrentPositionProperty >= TotalDurationProperty)
        //        {
        //            // If in full screen mode - exit full screen mode
        //            var content = Application.Current.Host.Content;
        //            if (content.IsFullScreen)
        //            {
        //                content.IsFullScreen = false;
        //            }

        //            CurrentPositionProperty = 0;
        //            StopVideo(null);
        //        }
        //    }
        //    else
        //    {
        //        if (CurrentPositionProperty < 0)
        //        {
        //            // If in full screen mode - exit full screen mode
        //            var content = Application.Current.Host.Content;
        //            if (content.IsFullScreen)
        //            {
        //                content.IsFullScreen = false;
        //            }

        //            CurrentPositionProperty = 0;
        //            StopVideo(null);
        //        }
        //    }

        //    // Update the time text e.g. 01:50 / 03:30
        //    CurrentProgressProperty = string.Format(
        //        "{0}:{1} / {2}:{3}",
        //        Math.Floor(MyMediaElement.Position.TotalMinutes).ToString("00"),
        //        MyMediaElement.Position.Seconds.ToString("00"),
        //        Math.Floor(MyMediaElement.NaturalDuration.TimeSpan.TotalMinutes).ToString("00"),
        //        MyMediaElement.NaturalDuration.TimeSpan.Seconds.ToString("00"));

        //    CurrentPositionProperty = MyMediaElement.Position.TotalSeconds;
        //    MediaBufferingProperty = (MyMediaElement.CurrentState == MediaElementState.Buffering);
        //    MediaBufferingTimeProperty = String.Format("Buffering {0} %", (MyMediaElement.BufferingProgress * 100).ToString("##"));
        //}

        #endregion
    }
}
