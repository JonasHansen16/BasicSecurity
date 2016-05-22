using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
namespace EncryptieProject
{
   
    class Steganography
    {
        private byte[] encryptkey;
        private byte[] decryptkey;
        private static byte[] pixels;
        private static byte[] endImageBuffer; //array to hold the pixels of the image to become

        public Steganography()
        {
        }

        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi */
       public void StartEncrypting(string toEncryptFileLocation, string locationToSaveImage, string startImage, string publicKeyLocation, string privateKeyLocation)
       {

            using (Aes aes = Aes.Create())
            {
                

                BitmapImage bitmap = new BitmapImage(new Uri(startImage)); ;
                BitmapSource endImage = null;
                Stream toEncryptFileStream = GetFileStream(toEncryptFileLocation);
                Stream keyStream;
                byte[] key = aes.Key;
                if (toEncryptFileStream.Length == 0)
                {
                    MessageBox.Show("Geef een te encrypteren file in.");
                }
                else
                {
                  //  byte[] key = Encoding.Default.GetBytes("pizza");
                    encryptkey = key;
                    keyStream = new MemoryStream(key);
                    if (keyStream.Length == 0)
                    {
                        MessageBox.Show("Geef een key-file op.");
                    }
                    else
                    {
                        try
                        {
                            endImage = HideMessageInBitmap(toEncryptFileStream, bitmap, keyStream);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Exception:\r\n" + ex.Message);
                        }
                    }
                    keyStream.Close();
                    bitmap = null;
                    try
                    {
                        SaveImage(endImage, locationToSaveImage + "\\file_1.png");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception:\r\n" + ex.Message);
                    }

                    Encryption.EncryptFileRSA(publicKeyLocation, key, locationToSaveImage + "\\file_2");
                    byte[] calculatedHash = MD5Hash.calculateHashFromFile(toEncryptFileLocation);
                    Encryption.SignHash(calculatedHash, privateKeyLocation, locationToSaveImage + "\\File_3");

                }

            }
       }

        public string StartDecrypt(string toDecryptImageLocation, string keyLocation, string EncryptedKeyFileLocation, string endFileLocation)
        {
            BitmapSource bitmap = new BitmapImage(new Uri(toDecryptImageLocation));

             byte[] keyBytes = Encryption.DecryptFileRSA(keyLocation, EncryptedKeyFileLocation);
            Stream keyStream = new MemoryStream(keyBytes);
            Stream decryptedFileStream = new MemoryStream();

            string result = "";
            decryptkey = keyBytes;
            if (keyStream.Length == 0)
            {
                MessageBox.Show("Geef een key file op");
            }
            else
            {
                try
                {
                    ExtractMessageFromBitmap(bitmap, keyStream, ref decryptedFileStream);
                    if (endFileLocation.Length > 0)
                    {
                        decryptedFileStream.Seek(0, SeekOrigin.Begin);
                        FileStream fs = new FileStream(endFileLocation, FileMode.Create);
                        byte[] streamContent = new Byte[decryptedFileStream.Length];
                        decryptedFileStream.Read(streamContent, 0, streamContent.Length);
                        fs.Write(streamContent, 0, streamContent.Length);
                        fs.Close();
                    }
                    decryptedFileStream.Seek(0, SeekOrigin.Begin);
                    StreamReader reader = new StreamReader(decryptedFileStream, Encoding.Default);
                    String readerContent = reader.ReadToEnd();

                   result = readerContent;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:\r\n" + ex.Message);
                }
            }
            decryptedFileStream.Close();
            keyStream.Close();
            bitmap = null;

            return result;
        }

       /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi 
      Creates a stream to read the file that has to be encrypted*/

        private Stream GetFileStream(string fileLocation)
        {
            Stream toEncryptStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);
            return toEncryptStream;
        }


        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi 
        To encrypt the file with the image, extract is set to false, so the extract functions don't run in HideOrExtract()*/
        public static BitmapSource HideMessageInBitmap(Stream fileStream, BitmapSource image, Stream keyStream) { 
            BitmapSource imgSource =  HideOrExtract( ref fileStream, image, keyStream, false);
            fileStream = null;
            return imgSource;
        }

        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi 
        To extract the encrypted File from the image, extract is set to true, so the extract functions run in HideOrExtract()*/
        public static void ExtractMessageFromBitmap(BitmapSource bitmap, Stream keyStream, ref Stream fileStream)
        {
            HideOrExtract(ref fileStream, bitmap, keyStream, true);
        }

/*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi*/
        private static BitmapSource HideOrExtract(ref Stream fileStream, BitmapSource img, Stream keyStream, bool extract)
        {
            int currentStepWidth = 0; //the step to every hidden byte according to the key
            byte currentKeyByte; //Current byte in the key stream - normal direction
            byte currentReverseKeyByte; //Current byte in the key stream - reverse direction
            long keyPostion; // current position in the key stream

            //max X, Y position of the image
            int imageWidth = (Int32)(img.PixelWidth - 1);
            int imageHeigth = (Int32)(img.PixelHeight - 1);

            //get other info of image
            double dpiX = img.DpiX;
            double dpiY = img.DpiY;
            PixelFormat pixelFormat = img.Format;
            int stride = img.PixelWidth * 4;

            //Color component to hide the next byte (0=R, 1=G, 2=B)
            int currentColorComponent = 0;

            //The color of a pixel
            Color pixelColor = new Color();

            //Length of the file
            int fileLength;

            //Copy the pixels of the image to an array, so we can extract the r,g en b values
            putPixelsInArray(img, stride);

            //declaring the array of bytes for the end image
            endImageBuffer = new byte[pixels.Length];
            endImageBuffer = pixels;

            /*//getting the value for r, g and b
            pixelColor.R = GetPixelRgb('r', 0, 0);
            pixelColor.G = GetPixelRgb('g', 0, 0);
            pixelColor.B   = GetPixelRgb('b', 0, 0);*/

            if (extract) // When we want to decrypt
            {
                //Read the lenght of the hidden message from the first pixel
                pixelColor.R = GetPixelRgb(pixels,'r', 0, 0, stride);
                pixelColor.G = GetPixelRgb(pixels, 'g', 0, 0, stride);
                pixelColor.B = GetPixelRgb(pixels, 'b', 0, 0, stride);//getting the value for r, g and b
                fileLength = (pixelColor.R << 2) + (pixelColor.G << 1) + pixelColor.B; //shift the rgb-value with the number of bits specified by the int
                fileStream = new MemoryStream(fileLength);
            }
            else //When we want to encrypt
            {
                fileLength = (Int32)fileStream.Length;

                if (fileStream.Length >= 16777215) //the file is too big
                {
                    String exceptionMessage = "Het bestand is te groot, enkel 16777215 bytes zijn toegestaan";
                    throw new Exception(exceptionMessage);
                }

                //Checking if the carrier image is big enough

                //Pixels available
                long countPixels = (imageWidth * imageHeigth) - 1;
                //Pixels reguired - start with one pixel for length of the message
                long countRequiredPixels = 1;
                //add up the gaps between used pixels (sum up all the bytes of the key)
                while ((keyStream.Position < keyStream.Length) && (keyStream.Position < fileLength)) //if the key is shorter then the file, it will be repeated
                    countRequiredPixels += keyStream.ReadByte();

                //Multiplie with count of key periods
                countRequiredPixels *= (long)System.Math.Ceiling(((float)fileStream.Length / (float)keyStream.Length));

                if(countRequiredPixels > countPixels ) //when the image is too small
                {
                    String exceptionMessage = "De afbeelding is te klein voor deze te encrypteren file. " + countRequiredPixels + " pixels zijn nodig.";
                    throw new Exception(exceptionMessage);
                }

                //Write the length of the image in the first pixel
                int colorValue = fileLength;
                int red = colorValue >> 2;
                colorValue -= red << 2;
                int green = colorValue >> 1;
                int blue = colorValue - (green << 1);
                
                SetPixel(0, 0, red, green, blue, stride); 
            }

            //Reset the streams
            keyStream.Seek(0, SeekOrigin.Begin);
            fileStream.Seek(0, SeekOrigin.Begin);

            //Current position in the endImage, starts with 1 because at the first 3 values contain the file-length
            int xEndImagePosition = 1;
            int yEndImagePosition = 0;

            //Loop over the file
            for (int fileIndex = 0; fileIndex < fileLength; fileIndex++)
            {
                //To repaet the key, if it is shorter than the message
                if (keyStream.Position == keyStream.Length)
                    keyStream.Seek(0, SeekOrigin.Begin);

                //Get the next pixel-count from the key, use 1 is it's 0
                currentKeyByte = (byte)keyStream.ReadByte();
                currentStepWidth = (currentKeyByte == 0) ? (byte)1 : currentKeyByte;

                //jump to reverse-read position and read from the end of the stream
                keyPostion = keyStream.Position; 
                keyStream.Seek(-keyPostion, SeekOrigin.End); 
                currentReverseKeyByte = (byte)keyStream.ReadByte();
                //jump back to normal read position
                keyStream.Seek(keyPostion, SeekOrigin.Begin);

                //Do line breaks, if the currentStepWidth is wider than the image
                while (currentStepWidth > imageWidth)
                {
                    currentStepWidth -= imageWidth;
                    yEndImagePosition++;
                }

                //move xEndImagePosition
                if ((imageWidth - xEndImagePosition) < currentStepWidth)
                {
                    xEndImagePosition = currentStepWidth - (imageWidth - xEndImagePosition);
                    yEndImagePosition++;
                }
                else
                    xEndImagePosition += currentStepWidth;

                //Get color of the clean pixel
                pixelColor.R = GetPixelRgb(pixels,'r', xEndImagePosition, yEndImagePosition, stride);
                pixelColor.G = GetPixelRgb(pixels,'g', xEndImagePosition, yEndImagePosition, stride);
                pixelColor.B = GetPixelRgb(pixels,'b', xEndImagePosition, yEndImagePosition, stride);
                if (extract)
                {
                    //Extract the hidden message-byte from the color
                    byte foundByte =
                        (byte) (currentReverseKeyByte ^ GetColorComponent(pixelColor, currentColorComponent));
                    fileStream.WriteByte(foundByte);
                    //Rotate color components
                    currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);

                }
                else
                {
                    byte currentByte = (byte)(fileStream.ReadByte() ^ currentReverseKeyByte);

                    //changing one component of the color to the message-byte
                    SetColorComponent(ref pixelColor, currentColorComponent, currentByte);
                    //Rotate Color components
                    currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                    SetPixel(xEndImagePosition, yEndImagePosition, pixelColor.R, pixelColor.G, pixelColor.B, stride);  
                }

            }

            //closing the streams
            BitmapSource endImage = CreateImageOutOfByteArray(imageWidth + 1, imageHeigth + 1, dpiX, dpiY, stride, pixelFormat);
            keyStream = null;

            return endImage;
        }

        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi
        Returning one component of a color*/
        private static byte GetColorComponent(Color pixelColor, int colorComponent)
        {
            byte returnColor = 0;
            switch (colorComponent)
            {
                case 0:
                    returnColor = pixelColor.R;
                    break;
                case 1:
                    returnColor = pixelColor.G;
                    break;
                case 2:
                    returnColor = pixelColor.B;
                    break;
            }
            return returnColor;
        }

        /*source: http://www.codeproject.com/Articles/4877/Steganography-Hiding-messages-in-the-Noise-of-a-Pi
        Changing one component of a color*/

        private static void SetColorComponent(ref Color pixelColor, int colorComponent, byte newValue)
        {
            switch (colorComponent)
            {
                case 0:
                    pixelColor = Color.FromArgb(255, newValue, pixelColor.G, pixelColor.B);
                    break;
                case 1:
                    pixelColor = Color.FromArgb(255, pixelColor.R, newValue, pixelColor.B);
                    break;
                case 2:
                    pixelColor = Color.FromArgb(255, pixelColor.R, pixelColor.G, newValue);
                    break;
            }
        }

        /*src: https://social.msdn.microsoft.com/Forums/vstudio/en-US/82a5731e-e201-4aaf-8d4b-062b138338fe/getting-pixel-information-from-a-bitmapimage?forum=wpf 
        Calculate where the specified pixel is in the array, en return the right value for r, g and b*/
        
        private static byte GetPixelRgb(byte[] pixelData, char rgb, int xPixel, int yPixel, int stride)
        {
            int index = yPixel * stride + 4 * xPixel;

            if (rgb == 'r')
                return pixelData[index + 1];//index + 1 because in the first value of the pixel is the alpha value(opacity)
            else if (rgb == 'g')
                return pixelData[index + 2];
            else if (rgb == 'b')
                return pixelData[index + 3];
            else
                return 0;
        }

          /*src: https://social.msdn.microsoft.com/Forums/vstudio/en-US/82a5731e-e201-4aaf-8d4b-062b138338fe/getting-pixel-information-from-a-bitmapimage?forum=wpf */
        private static void putPixelsInArray(BitmapSource img, int stride)
        {
            stride = img.PixelWidth * 4; //getting 4 places for the values of every pixel (r, g, b and alpha)
            int size = img.PixelHeight * stride;
            pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);
        }



        /*@src: http://tipsandtricks.runicsoft.com/CSharp/WpfPixelDrawing.html
          @Author Andreas Hartl
        Setting pixels of the new image*/
        private static void SetPixel(int x, int y, int red, int green, int blue, int stride)
        {
            int index = y * stride + 4 * x;

            endImageBuffer[index + 1] = (byte)red;
            endImageBuffer[index + 2] = (byte)green;
            endImageBuffer[index + 3] = (byte)blue;
        }

        /*src: http://stackoverflow.com/questions/15274699/create-a-bitmapimage-from-a-byte-array*/
        private static BitmapSource CreateImageOutOfByteArray(int imageWidth, int imageHeight, double dpiXImage, double dpiYImage, int stride, PixelFormat pixelFormatImage)
        {
            if (endImageBuffer == null || endImageBuffer.Length == 0)
                return null;

            int width = imageWidth;
            var height = imageHeight;
            var dpiX = dpiXImage;
            var dpiY = dpiYImage;
            var pixelFormat = pixelFormatImage;

            var bitmap = BitmapImage.Create(width, height, dpiX, dpiY, pixelFormat, null, endImageBuffer, stride);
            return bitmap;
        }

        //http://stackoverflow.com/questions/2900447/how-to-save-a-wpf-image-to-a-file
        private static void SaveImage(BitmapSource image, string locationToSaveImage)
        {
            using (FileStream fileStream = new FileStream(locationToSaveImage, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }


    }

        }
            
        
    

