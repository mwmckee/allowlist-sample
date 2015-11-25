﻿using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Microsoft.ProjectOxford.Face;
using Microsoft.Maker.ProjectOxford.Face.Allowlist;
using Microsoft.Maker.Devices.Media.UsbCamera;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AllowlistSampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewUserPage : Page
    {
        private UsbCamera webcam;

        private StorageFile currentIdPhotoFile;

        public NewUserPage()
        {
            this.InitializeComponent();            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                //Sets passed through WecamHelper from MainPage as local webcam object
                webcam = e.Parameter as UsbCamera;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error when navigating to NewUserPage: " + exception.Message);
            }
        }

        private async void WebcamFeed_Loaded(object sender, RoutedEventArgs e)
        {
            WebcamFeed.Source = webcam.MediaCaptureInstance;

            // Check to make sure MediaCapture isn't null before attempting to start preview. Will be null if no camera is attached.
            if (WebcamFeed.Source != null)
            {
                await webcam.StartCameraPreview();
            }
        }

        private async void Capture_Click(object sender, RoutedEventArgs e)
        {
            // Capture current frame from webcam, store it in temporary storage and set the source of a BitmapImage to said photo
            currentIdPhotoFile = await webcam.CapturePhoto();
            var photoStream = await currentIdPhotoFile.OpenAsync(FileAccessMode.ReadWrite);
            BitmapImage idPhotoImage = new BitmapImage();
            await idPhotoImage.SetSourceAsync(photoStream);

            // Set the soruce of the photo control the new BitmapImage and make the photo control visible
            IdPhotoControl.Source = idPhotoImage;
            IdPhotoControl.Visibility = Visibility.Visible;

            // Collapse the webcam feed and the capture photo button. Make the enter user name grid visible.
            WebcamFeed.Visibility = Visibility.Collapsed;

            UserNameGrid.Visibility = Visibility.Visible;
            CaptureButton.Visibility = Visibility.Collapsed;
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(UserNameBox.Text))
            {
                StorageFolder allowlistFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(GeneralConstants.AllowListFolderName, CreationCollisionOption.OpenIfExists);
                StorageFolder currentFolder = await allowlistFolder.CreateFolderAsync(UserNameBox.Text, CreationCollisionOption.ReplaceExisting);

                await currentIdPhotoFile.MoveAsync(currentFolder);
                try
                {
                    await FaceAllowlist.AddPersonToList(UserNameBox.Text, currentFolder);
                } 
                catch(ClientException exception)
                {
                    if(exception.HttpStatus == (System.Net.HttpStatusCode)429)
                    {
                        var dialog = new Windows.UI.Popups.MessageDialog("Command rate exceeded. Please wait a minute and resubmit your request.");
                        dialog.Commands.Add(new UICommand("Ok"));
                        await dialog.ShowAsync();
                    }
                }

                await webcam.StopCameraPreview();
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Collapse the confirm photo buttons and open the capture photo button.
            CaptureButton.Visibility = Visibility.Visible;
            UserNameGrid.Visibility = Visibility.Collapsed;
            UserNameBox.Text = "";

            // Collapse the Photo Preview control and open the webcam feed
            WebcamFeed.Visibility = Visibility.Visible;
            IdPhotoControl.Visibility = Visibility.Collapsed;
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            await webcam.StopCameraPreview();
            Frame.Navigate(typeof(MainPage));
        }
    }
}
