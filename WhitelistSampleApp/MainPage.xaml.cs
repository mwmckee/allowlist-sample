using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.Maker.Devices.Media.UsbCamera;
using Microsoft.Maker.ProjectOxford.Face.Whitelist;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WhitelistSampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private UsbCamera webcam;

        // Oxford Related Variables:
        private static bool initializedOxford = false;

        public MainPage()
        {
            this.InitializeComponent();

            if (initializedOxford == false)
            {
                InitializeApp();
            }
        }

        /// <summary>
        /// Triggered when webcam feed loads both for the first time and every time page is navigated to.
        /// If no WebcamHelper has been created, it creates one. Otherwise, simply restarts webcam preview feed on page.
        /// </summary>
        private async void WebcamFeed_Loaded(object sender, RoutedEventArgs e)
        {
            if (webcam == null || !webcam.IsInitialized())
            {
                webcam = new UsbCamera();
                await webcam.InitializeAsync();
                WebcamFeed.Source = webcam.MediaCaptureInstance;

                // Check to make sure MediaCapture isn't null before attempting to start preview. Will be null if no camera is attached.
                if (WebcamFeed.Source != null)
                {
                    await webcam.StartCameraPreview();
                }
            }
            else if (webcam.IsInitialized())
            {
                WebcamFeed.Source = webcam.MediaCaptureInstance;

                // Check to make sure MediaCapture isn't null before attempting to start preview. Will be null if no camera is attached.
                if (WebcamFeed.Source != null)
                {
                    await webcam.StartCameraPreview();
                }
            }
        }

        public async void InitializeApp()
        {
            initializedOxford = await FaceWhitelist.InitializeOxford();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // List to store users recognized by Oxford Face API
            // Count will be greater than 0 if there is an authorized user at the door
            List<string> recognizedUsers = new List<string>();

            // Confirms that webcam has been properly initialized and oxford is ready to go
            if (webcam.IsInitialized() && initializedOxford)
            {
                // Stores current frame from webcam feed in a temporary folder
                StorageFile image = await webcam.CapturePhoto();
                try
                {
                    // Oxford determines whether or not the user is on the Whitelist and returns true if so
                    recognizedUsers = await FaceWhitelist.IsFaceInWhitelist(image);
                    if (recognizedUsers.Count > 0)
                    {
                        Text.Text = recognizedUsers[0];
                    }
                    else
                    {
                        // Otherwise, inform user that they were not recognized by the sytem
                        Text.Text = "Unidentified";
                    }
                }
                catch (FaceRecognitionException fe)
                {
                    switch (fe.ExceptionType)
                    {
                        // Fails and catches as a FaceRecognitionException if no face is detected in the image
                        case FaceRecognitionExceptionType.NoFaceDetected:
                            Text.Text = "WARNING: No face detected in this image.";
                            break;
                    }
                }

            }
            else
            {
                if (!webcam.IsInitialized())
                {
                    // The webcam has not been fully initialized for whatever reason:
                    Text.Text = "Unable to analyze user as the camera failed to initlialize properly.";
                }

                if (!initializedOxford)
                {
                    // Oxford is still initializing:
                    Text.Text = "Unable to analyze user as Oxford Facial Recogntion is still initializing.";
                }
            }
        }

        private async void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Stops camera preview on this page, so that it can be started on NewUserPage
            await webcam.StopCameraPreview();

            //Navigates to NewUserPage, passing through initialized WebcamHelper object
            Frame.Navigate(typeof(NewUserPage), webcam);
        }

        private async void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Stops camera preview on this page, so that it can be started on NewUserPage
            await webcam.StopCameraPreview();

            //Navigates to NewUserPage, passing through initialized WebcamHelper object
            Frame.Navigate(typeof(RemoveUserPage), webcam);
        }
    }
}
