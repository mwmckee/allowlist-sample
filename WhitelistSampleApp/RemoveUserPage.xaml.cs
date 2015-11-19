using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Microsoft.Maker.ProjectOxford.Face.Whitelist;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WhitelistSampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RemoveUserPage : Page
    {  
        struct imageList
        {
            public string Name { get; set; }
            public StorageFolder ImageFolder { get; set; }
            public BitmapImage Image { get; set; }
        }
        private List<imageList> whitelistedUsers = new List<imageList>();
        private StorageFolder whitelistFolder;
        private bool currentlyUpdatingWhitelist = false;

        public RemoveUserPage()
        {
            this.InitializeComponent();            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateWhitelistedUsers();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private async void UpdateWhitelistedUsers()
        {
            if (!currentlyUpdatingWhitelist)
            {
                currentlyUpdatingWhitelist = true;
                await UpdateWhitelistedUsersList();
                UpdateWhitelistedUsersGrid();
                currentlyUpdatingWhitelist = false;
            }
        }

        private async Task UpdateWhitelistedUsersList()
        {
            // Clears whitelist
            whitelistedUsers.Clear();

            // If the whitelistFolder has not been opened, open it
            if (whitelistFolder == null)
            {
                whitelistFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(GeneralConstants.WhiteListFolderName, CreationCollisionOption.OpenIfExists);
            }

            // Populates subFolders list with all sub folders within the whitelist folders.
            // Each of these sub folders represents the Id photos for a single User.
            var subFolders = await whitelistFolder.GetFoldersAsync();

            if(subFolders.Count == 0)
            {
                noUsers.Visibility = Visibility.Visible;
            }

            // Iterate all subfolders in whitelist
            foreach (StorageFolder folder in subFolders)
            {
                var filesInFolder = await folder.GetFilesAsync();

                var photoStream = await filesInFolder[0].OpenAsync(FileAccessMode.Read);
                BitmapImage userImage = new BitmapImage();
                await userImage.SetSourceAsync(photoStream);

                imageList whitelistedUser = new imageList();
                whitelistedUser.Image = userImage;
                whitelistedUser.ImageFolder = folder;
                whitelistedUser.Name = folder.Name;
                whitelistedUsers.Add(whitelistedUser);
            }
        }


        private void UpdateWhitelistedUsersGrid()
        {
            WhitelistedUsersGrid.ItemsSource = new List<imageList>();
            WhitelistedUsersGrid.ItemsSource = whitelistedUsers;

            OxfordLoadingRing.Visibility = Visibility.Collapsed;
        }

        private async void WhitelistedUsersGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            imageList toDelete = (imageList)e.ClickedItem;
            FaceWhitelist.RemoveUserFromWhitelist(toDelete.Name);
            await toDelete.ImageFolder.DeleteAsync();
            UpdateWhitelistedUsers();
        }
    }
}
