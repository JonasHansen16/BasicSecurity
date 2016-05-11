using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            if (decryptieRadioButton.IsChecked == true)
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
                            Console.WriteLine(ex.Message);

                        }
                      

                    }
                    else
                    {

                    }

                   // Steganography steganography = new Steganography();
                    steno.StartDecrypt(file1LocationTextBox.Text, privateKeyDecryptionLocationTextBox.Text,
                        file2LocationTextBox.Text, filename);
                    bool hashVerified = Encryption.VerifyFile(filename, file3LocationTextBox.Text, publicKeyDecryptionLocationTextBox.Text);
                    if (hashVerified)
                    {
                        MessageBox.Show("Hash ok");
                        System.Diagnostics.Process.Start(filename);
                    }
                    else
                    {
                        MessageBox.Show("Hash does not match, file from unknown origin");
                    }



                }
                else
                {
                    try
                    {
                        string publicKeyFilePath = publicKeyDecryptionLocationTextBox.Text;
                        string privateKeyFilePath = privateKeyDecryptionLocationTextBox.Text;
                        string encryptedMessageFilePath = file1LocationTextBox.Text;
                        string encryptedKeyFilePath = file2LocationTextBox.Text;
                        string signedHashFilePath = file3LocationTextBox.Text;

                        string decryptedMessageFilePath = Encryption.Decrypt(privateKeyFilePath, encryptedMessageFilePath, encryptedKeyFilePath);
                        bool hashVerified = Encryption.VerifyFile(decryptedMessageFilePath, signedHashFilePath, publicKeyFilePath);

                        if (hashVerified)
                        {
                            MessageBox.Show("Hash ok");
                            System.Diagnostics.Process.Start(decryptedMessageFilePath);
                        }
                        else
                        {
                            MessageBox.Show("Hash does not match, file from unknown origin");
                        }


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }




                //try
                //{
                //    Steganography.ExtractMessageFromBitmap(bitmap, keyStream, ref decryptedFileStream);
                //    if (file2LocationTextBox.Text.Length > 0)
                //    {
                //        decryptedFileStream.Seek(0, SeekOrigin.Begin);
                //        FileStream fs = new FileStream(file2LocationTextBox.Text, FileMode.Create);
                //        byte[] streamContent = new Byte[decryptedFileStream.Length];
                //        decryptedFileStream.Read(streamContent, 0, streamContent.Length);
                //        fs.Write(streamContent, 0, streamContent.Length);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("Exception:\r\n" + ex.Message);
                //}


                //bitmap = null;
            }
            else
            {


                if (stenographyChecked)
                {
                   // Steganography steganography = new Steganography();
                    //BitmapImage startImage = new BitmapImage(new Uri(ImageFileLocationTextBox.Text));
                    steno.StartEncrypting(encryptFileLocationTextBox.Text
                        , filesEncryptionLocationTextBox.Text,
                        ImageFileLocationTextBox.Text, publicKeyEncryptionLocationTextBox.Text, privateKeyEncryptionLocationTextBox.Text);
                }
                else
                {
                    try
                    {
                        string publicKeyFilePath = publicKeyEncryptionLocationTextBox.Text;
                        string privateKeyFilePath = privateKeyEncryptionLocationTextBox.Text;
                        string locationFilePath = filesEncryptionLocationTextBox.Text;
                        string toEncryptFilePath = encryptFileLocationTextBox.Text;
                        Encryption.Encrypt(publicKeyFilePath, privateKeyFilePath, locationFilePath, toEncryptFilePath, extension);
                        MessageBox.Show("encryptie gedaan");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }





                //try
                //{
                //    Steganography.HideMessageInBitmap(toEncryptFileStream, bitmap, keyStream);
                //    SaveImage(bitmap);
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("Exception:\r\n" + ex.Message);
                //}


                //bitmap = null;

            }
        }

        
        private void filesEncryptionButton_Click(object sender, RoutedEventArgs e)
        {
            filesEncryptionLocationTextBox.Text = GetDirectoryPath();
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
            return dialog.FileName;
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
            label_Copy1.Content = "selecteer te encrypteren bestand";
        }

        private void ImageFileButton_Click(object sender, RoutedEventArgs e)
        {
            ImageFileLocationTextBox.Text = getFile();
            if (stenographyChecked)
            {
                visualImage.Source = new BitmapImage(new Uri(ImageFileLocationTextBox.Text));
            }
        }
    }
}
