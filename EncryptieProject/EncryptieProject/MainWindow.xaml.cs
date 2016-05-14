using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Media;

namespace EncryptieProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string extension;
        private bool stenographyChecked;
        private Steganography steno = new Steganography();

        public MainWindow()
        {
            InitializeComponent();

            stenographyChecked = false;
            encryptieRadioButton.IsChecked = true;
        }

        //De juiste functies en teksten aanpassen voor de gekozen handeling: decryptie.
        private void decryptieRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //design
            fileEncryptionGroupBox.Visibility = Visibility.Hidden;
            fileDecryptionGroupBox.Visibility = Visibility.Visible;
            statusTextBlock.Text = "Kies de bestanden en klik op decrypteren.";
            startEncryptButton.Content = "Decrypteer";
            
            statusGrid.Background = null;
            
        }

        //De juiste functies en teksten aanpassen voor de gekozen handeling: encryptie.
        private void encryptieRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //design
            fileEncryptionGroupBox.Visibility = Visibility.Visible;
            fileDecryptionGroupBox.Visibility = Visibility.Hidden;
            statusTextBlock.Text = "Kies de bestandslocaties en klik op encrypteren.";
            startEncryptButton.Content = "Encrypteer";
            
            statusGrid.Background = null;
            
        }

        private void stenographyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            stenographyChecked = true;
            visualImage.Visibility = Visibility.Visible;
            ImageFileButton.Visibility = Visibility.Visible;
            ImageFileLocationTextBox.Visibility = Visibility.Visible;
            label_Copy2.Visibility = Visibility.Visible;
            
        }

        //het path van de te encrypteren file ophalen en in de textbox zetten
        private void encryptedFileButton_Click(object sender, RoutedEventArgs e)
        {
            encryptFileLocationTextBox.Text = getFileAndExtension();
           
        }

      

        //filepicker openen en het gekozen path terugsturen
        private string getFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
                //kan niet in een gewone bool, aangezien ShowDialog() een generische bool teruggeeft.

            if (result == true)
            {
                string fileName = dlg.FileName;
                
                return fileName;
            }
            else
                return null;
        }

        private string getFileAndExtension()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            //kan niet in een gewone bool, aangezien ShowDialog() een generische bool teruggeeft.

            if (result == true)
            {
                string fileName = dlg.FileName;
                extension = Path.GetExtension(dlg.FileName);
                return fileName;
            }
            else
                return null;
        }

        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi 
       start van de gekozen handeling: Encryptie*/

        private void startEncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string publicKeyFilePath = publicKeyDecryptionLocationTextBox.Text;
            string privateKeyFilePath = privateKeyDecryptionLocationTextBox.Text;
            string encryptedMessageFilePath = file1LocationTextBox.Text;
            string encryptedKeyFilePath = file2LocationTextBox.Text;
            string signedHashFilePath = file3LocationTextBox.Text;

            if (decryptieRadioButton.IsChecked == true)
            {
                if (publicKeyFilePath != "" && privateKeyFilePath != "" && encryptedMessageFilePath != "" && encryptedKeyFilePath != "" && signedHashFilePath != "")
                {
                    if (stenographyChecked)
                    {
                        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                        dlg.FileName = "DecryptedFile"; // Default file name
                        dlg.DefaultExt = ".txt";

                        // Show save file dialog box
                        Nullable<bool> result = dlg.ShowDialog();
                        string filename = null;

                        // Process save file dialog box results
                        if (result == true)
                        {
                            try
                            {
                                filename = dlg.FileName;


                            }
                            catch (IOException ex)
                            {
                                MessageBox.Show(ex.Message);

                            }
                        }

                        bool hashVerified = false;

                        try
                        {
                            steno.StartDecrypt(file1LocationTextBox.Text, privateKeyDecryptionLocationTextBox.Text,
                                file2LocationTextBox.Text, filename);
                            hashVerified = Encryption.VerifyFile(filename, file3LocationTextBox.Text, publicKeyDecryptionLocationTextBox.Text);


                            if (hashVerified)
                            {
                                statusTextBlock.Text = "Decryptie is gelukt. De hash komt overeen.";
                                statusGrid.Background = Brushes.Green;
                                System.Diagnostics.Process.Start(filename);
                            }
                            else
                            {
                                statusTextBlock.Text = "Decryptie is niet gelukt.";
                                statusGrid.Background = Brushes.Red;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            string decryptedMessageFilePath = Encryption.Decrypt(privateKeyFilePath, encryptedMessageFilePath, encryptedKeyFilePath);
                            bool hashVerified = Encryption.VerifyFile(decryptedMessageFilePath, signedHashFilePath, publicKeyFilePath);

                            if (hashVerified)
                            {
                                statusTextBlock.Text = "Decryptie is gelukt. De hash komt overeen.";
                                statusGrid.Background = Brushes.Green;
                                System.Diagnostics.Process.Start(decryptedMessageFilePath);
                            }
                            else
                            {
                                statusTextBlock.Text = "Decryptie is niet gelukt.";
                                statusGrid.Background = Brushes.Red;
                            }


                        }
                        catch (Exception ex)
                        {
                            statusTextBlock.Text = "Decryptie is niet gelukt.";
                            statusGrid.Background = Brushes.Red;
                        }

                    }

                } else
                {
                    MessageBox.Show("Er is niet overal een file ingegeven.");
                }
            }
            
                
            
            else
            {
                string publicEncryptionKeyFilePath = publicKeyEncryptionLocationTextBox.Text;
                string privateEncryptionKeyFilePath = privateKeyEncryptionLocationTextBox.Text;
                string locationFilePath = filesEncryptionLocationTextBox.Text;
                string toEncryptFilePath = encryptFileLocationTextBox.Text;
                if (publicEncryptionKeyFilePath != "" && privateEncryptionKeyFilePath != "" && locationFilePath != "" && toEncryptFilePath != "")
                {


                    if (stenographyChecked)
                    {
                        if (ImageFileLocationTextBox.Text != "")
                        {


                            try
                            {
                                steno.StartEncrypting(encryptFileLocationTextBox.Text
                                    , filesEncryptionLocationTextBox.Text,
                                    ImageFileLocationTextBox.Text, publicKeyEncryptionLocationTextBox.Text, privateKeyEncryptionLocationTextBox.Text);
                                statusTextBlock.Text = "Steganography en encryptie is gelukt.";
                                statusGrid.Background = Brushes.Green;
                            }
                            catch (Exception ex)
                            {
                                statusTextBlock.Text = "Steganography en encryptie is niet gelukt.";
                                statusGrid.Background = Brushes.Red;
                            }
                        }else
                        {
                            MessageBox.Show("Geef een afbeelding voor in te encrypteren");
                        }
                    }
                    else
                    {
                        try
                        {
                            Encryption.Encrypt(publicEncryptionKeyFilePath, privateEncryptionKeyFilePath, locationFilePath, toEncryptFilePath, extension);
                            statusTextBlock.Text = "Encryptie is gelukt.";
                            statusGrid.Background = Brushes.Green;
                        }
                        catch (Exception ex)
                        {
                            statusTextBlock.Text = "Encryptie is niet gelukt.";
                            statusGrid.Background = Brushes.Red;
                        }
                    }
                } else
                {
                    MessageBox.Show("Er is niet overal een file ingegeven.");
                }
            }
        }

        
        private void filesEncryptionButton_Click(object sender, RoutedEventArgs e)
        {
           string subPath = GetDirectoryPath() + "/Encryption";

            bool exists = System.IO.Directory.Exists(subPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);

            filesEncryptionLocationTextBox.Text = subPath;
        }

        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi 
       Creates a stream to read the file that has to be encrypted*/

        private Stream GetToEncryptFileStream()
        {
            Stream toEncryptStream = new FileStream(encryptFileLocationTextBox.Text, FileMode.Open, FileAccess.Read);
            return toEncryptStream;
        }

        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi 
       Creates a stream to read the keyfile*/

       

        //http://stackoverflow.com/questions/2900447/how-to-save-a-wpf-image-to-a-file
        private void SaveImage(BitmapSource image)
        {
            using (FileStream fileStream = new FileStream(encryptFileLocationTextBox.Text, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

       

        private void publicKeyEncryptionLocationButton_Click(object sender, RoutedEventArgs e)
        {
            publicKeyEncryptionLocationTextBox.Text = getFile();
        }

        private void privateKeyEncryptionLocationButton_Click(object sender, RoutedEventArgs e)
        {
            privateKeyEncryptionLocationTextBox.Text = getFile();
        }

        private string GetDirectoryPath()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            if(result == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
          
            
        }

        private void publicKeyDecryptionLocationButton_Click(object sender, RoutedEventArgs e)
        {
            publicKeyDecryptionLocationTextBox.Text = getFile();
        }

        private void privateKeyDecryptionLocationButton_Click(object sender, RoutedEventArgs e)
        {
            privateKeyDecryptionLocationTextBox.Text = getFile();
        }

        private void file1LocationButton_Click(object sender, RoutedEventArgs e)
        {
            file1LocationTextBox.Text = getFile();
        }

        private void file2LocationButton_Click(object sender, RoutedEventArgs e)
        {
            file2LocationTextBox.Text = getFile();
        }

        private void file3LocationButton_Click(object sender, RoutedEventArgs e)
        {
            file3LocationTextBox.Text = getFile();
        }

        private void stenographyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            stenographyChecked = false;
            visualImage.Visibility = Visibility.Hidden;
            label_Copy2.Visibility = Visibility.Hidden;
            ImageFileLocationTextBox.Visibility = Visibility.Hidden;
            ImageFileButton.Visibility = Visibility.Hidden;
        }

        private void ImageFileButton_Click(object sender, RoutedEventArgs e)
        {
            ImageFileLocationTextBox.Text = getFile();
            if (stenographyChecked)
            {
                try
                {
                    visualImage.Source = new BitmapImage(new Uri(ImageFileLocationTextBox.Text));
                }
                catch
                {
                    MessageBox.Show("Selecteer een afbeelding");
                }
                
            }
        }

        private string GetImageFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "*.png|*.bmp";
            Nullable<bool> result = dlg.ShowDialog();
            //kan niet in een gewone bool, aangezien ShowDialog() een generische bool teruggeeft.

            if (result == true)
            {
                string fileName = dlg.FileName;

                return fileName;
            }
            else
                return null;
        }

        private void genereateKeys_Click(object sender, RoutedEventArgs e)
        {
            string directory = GetDirectoryPath();
            if (directory != null)
            {
                string publicKey = "/PublicKey";
                string privateKey = "/PrivateKey";
                bool exists = System.IO.Directory.Exists(directory);

                if (!exists)
                    System.IO.Directory.CreateDirectory(directory);


                try
                {
                    KeyGenerator.CreateKeys(directory + publicKey, directory + privateKey);
                    statusTextBlock.Text = "Sleutels aangemaakt";
                    statusGrid.Background = Brushes.Green;
                }
                catch (Exception ex)
                {
                    statusTextBlock.Text = "Fout opgelopen tijdens het aanmaken van de sleutels";
                    statusGrid.Background = Brushes.Red;
                }
            }
            
        }
    }
}
