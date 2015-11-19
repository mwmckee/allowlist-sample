using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Microsoft.Maker.ProjectOxford.Face.Allowlist;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AllowlistSampleApp
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
        private List<imageList> allowlistedUsers = new List<imageList>();
        private StorageFolder allowlistFolder;
        private bool currentlyUpdatingAllowlist = false;

        public RemoveUserPage()
        {
            this.InitializeComponent();            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateAllowlistedUsers();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private async void UpdateAllowlistedUsers()
        {
            if (!currentlyUpdatingAllowlist)
            {
                currentlyUpdatingAllowlist = true;
                await UpdateAllowlistedUsersList();
                UpdateAllowlistedUsersGrid();
                currentlyUpdatingAllowlist = false;
            }
        }

        private async Task UpdateAllowlistedUsersList()
        {
            // Clears allowlist
            allowlistedUsers.Clear();

            // If the allowlistFolder has not been opened, open it
            if (allowlistFolder == null)
            {
                allowlistFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(GeneralConstants.AllowListFolderName, CreationCollisionOption.OpenIfExists);
            }

            // Populates subFolders list with all sub folders within the allowlist folders.
            // Each of these sub folders represents the Id photos for a single User.
            var subFolders = await allowlistFolder.GetFoldersAsync();

            if(subFolders.Count == 0)
            {
                noUsers.Visibility = Visibility.Visible;
            }

            // Iterate all subfolders in allowlist
            foreach (StorageFolder folder in subFolders)
            {
                var filesInFolder = await folder.GetFilesAsync();

                var photoStream = await filesInFolder[0].OpenAsync(FileAccessMode.Read);
                BitmapImage userImage = new BitmapImage();
                await userImage.SetSourceAsync(photoStream);

                imageList allowlistedUser = new imageList();
                allowlistedUser.Image = userImage;
                allowlistedUser.ImageFolder = folder;
                allowlistedUser.Name = folder.Name;
                allowlistedUsers.Add(allowlistedUser);
            }
        }


        private void UpdateAllowlistedUsersGrid()
        {
            AllowlistedUsersGrid.ItemsSource = new List<imageList>();
            AllowlistedUsersGrid.ItemsSource = allowlistedUsers;

            OxfordLoadingRing.Visibility = Visibility.Collapsed;
        }

        private async void AllowlistedUsersGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            imageList toDelete = (imageList)e.ClickedItem;
            FaceAllowlist.RemovePersonFromList(toDelete.Name);
            await toDelete.ImageFolder.DeleteAsync();
            UpdateAllowlistedUsers();
        }
    }
}
