using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EncryptieProject
{
    public static class Encryption
    {

        public static void Encrypt(string publicKeyFilePath, string privateKeyFilePath, string locationFilePath, string toEncryptFilePath, string extension)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] key = aesAlg.Key;
                byte[] IV = aesAlg.IV;
                EncryptStringToBytes_Aes(toEncryptFilePath, locationFilePath + "\\File_1", key, IV, extension);
                EncryptFileRSA(publicKeyFilePath, key, locationFilePath + "\\File_2");
                byte[] calculatedHash = MD5Hash.calculateHashFromFile(toEncryptFilePath);
                SignHash(calculatedHash, privateKeyFilePath, locationFilePath + "\\File_3");
            }

        }

        public static string Decrypt(string privateKeyFilePath, string encryptedMessageFilePath, string encryptedKeyFilePath)
        {
            byte[] decryptedKey = DecryptFileRSA(privateKeyFilePath, encryptedKeyFilePath);
            string decryptedMessageFilePath = DecryptFileFromBytes_Aes(encryptedMessageFilePath, decryptedKey);
            return decryptedMessageFilePath;

        }

        public static bool VerifyFile(string decryptedMessageFilePath, string signedHashFilePath, string publicKeyFilePath)
        {
           return VerifySignedHash(signedHashFilePath, publicKeyFilePath, decryptedMessageFilePath);

          

        
    }

       


        //https://blogs.msdn.microsoft.com/alejacma/2008/10/23/how-to-generate-key-pairs-encrypt-and-decrypt-data-with-net-c/
        //basis encryptie van een tekstfile moet nog aangepast worden naar de opgave toe
        //moet boodschap encrypteren met symmetric key ipv public key
        public static void EncryptFileRSA(string keyFileName, byte[]bytesToEncrypt, string encryptedFileName)

        {

            // Variables

            CspParameters cspParams = null;

            RSACryptoServiceProvider rsaProvider = null;

            StreamReader keyFile = null;

            

            FileStream encryptedFile = null;

            string keyText = "";

           

           

            byte[] encryptedBytes = null;

            try

            {

                // Select target CSP

                cspParams = new CspParameters();

                cspParams.ProviderType = 1; // PROV_RSA_FULL

                //cspParams.ProviderName; // CSP name

                rsaProvider = new RSACryptoServiceProvider(cspParams);

                // Read public key from file

                keyFile = File.OpenText(keyFileName);

                keyText = keyFile.ReadToEnd();

                // Import public key

                rsaProvider.FromXmlString(keyText);

                // Read plain text from file

              

                // Encrypt plain text

                

                encryptedBytes = rsaProvider.Encrypt(bytesToEncrypt, false);

                // Write encrypted text to file

                encryptedFile = File.Create(encryptedFileName);

                encryptedFile.Write(encryptedBytes, 0, encryptedBytes.Length);

            }

            catch (Exception ex)

            {

                // Any errors? Show them

                Console.WriteLine("Exception encrypting file!More info:");

                Console.WriteLine(ex.Message);

            }
            finally

            {

                // Do some clean up if needed

                if (keyFile != null)

                {

                    keyFile.Close();

                }

               

                if (encryptedFile != null)

                {

                    encryptedFile.Close();

                }

            }
        }
        //https://blogs.msdn.microsoft.com/alejacma/2008/10/23/how-to-generate-key-pairs-encrypt-and-decrypt-data-with-net-c/
        //basis decryptie moet ook nog aangepast worden naar de opgave toe


        public static byte[] DecryptFileRSA(string keyFileName, string encryptedFileName)

        {

            // Variables

            CspParameters cspParams = null;

            RSACryptoServiceProvider rsaProvider = null;

            StreamReader keyFile = null;

            FileStream encryptedFile = null;

            

            string keyText = "";

           

            byte[] encryptedBytes = null;

            byte[] plainBytes = null;

            try

            {

                // Select target CSP

                cspParams = new CspParameters();

                cspParams.ProviderType = 1; // PROV_RSA_FULL

                //cspParams.ProviderName; // CSP name

                rsaProvider = new RSACryptoServiceProvider(cspParams);

                // Read private/public key pair from file

                keyFile = File.OpenText(keyFileName);

                keyText = keyFile.ReadToEnd();

                // Import private/public key pair

                rsaProvider.FromXmlString(keyText);

                // Read encrypted text from file

                encryptedFile = File.OpenRead(encryptedFileName);

                encryptedBytes = new byte[encryptedFile.Length];

                encryptedFile.Read(encryptedBytes, 0, (int)encryptedFile.Length);

                // Decrypt text

                plainBytes = rsaProvider.Decrypt(encryptedBytes, false);

                // Write decrypted text to file

               

                return plainBytes;

            }

            catch (Exception ex)

            {

                // Any errors? Show them

                Console.WriteLine("Exception decrypting file!More info:");

                Console.WriteLine(ex.Message);

                return null;

            }

            finally

            {

                // Do some clean up if needed

                if (keyFile != null)

                {

                    keyFile.Close();

                }

                if (encryptedFile != null)

                {

                    encryptedFile.Close();

                }

               

            }

        }

        //https://msdn.microsoft.com/en-us/library/system.security.cryptography.aes%28v=vs.110%29.aspx
        //http://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice
        private static void EncryptStringToBytes_Aes(string plainFileName, string encryptedFileName, byte[] Key, byte[] IV, string extension)
        {

           
            FileStream encryptedFile = null;

            //StreamReader plainFile = null;
            FileStream plainFile = null;

            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            byte[] plainbytes = null;
            // Check arguments.
            byte[] extensionBytes = null;
            List<byte> extensionBytesList = new List<byte>(); 
            extensionBytes = System.Text.Encoding.UTF8.GetBytes(extension);
            extensionBytesList.AddRange(extensionBytes);
            for(int i = extension.Length; i < 8; i++)
            {
                extensionBytesList.Add(Convert.ToByte('$'));
            }
            extensionBytes = extensionBytesList.ToArray();


            byte[] encrypted;

            try
            {
                // Read plain text from file

                plainFile = File.OpenRead(plainFileName);

                plainbytes = new byte[plainFile.Length];

                plainFile.Read(plainbytes, 0, (int)plainFile.Length);



            }
            catch(Exception ex)
            {
                Console.WriteLine("Error reading PlainFile");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (plainFile != null)

                {

                    plainFile.Close();

                }

            }
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                List<byte> list = new List<byte>();
                list.AddRange(aesAlg.IV);
                list.AddRange(extensionBytes);
                byte[] prefix = list.ToArray();
              
               // plainbytes = Encoding.Default.GetBytes(plainText);
                
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter swEncrypt = new BinaryWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            msEncrypt.Write(prefix, 0, prefix.Length);
                           
                            swEncrypt.Write(plainbytes, 0, plainbytes.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        encrypted = msEncrypt.ToArray();
                         
                        try
                        {
                            encryptedFile = File.Create(encryptedFileName);



                            encryptedFile.Write(encrypted, 0, encrypted.Length);

                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Error Writing encrypted file");
                            Console.WriteLine(ex.StackTrace);
                        }
                        finally
                        {
                            if(encryptedFile != null)
                            {
                                encryptedFile.Close();
                            }
                        }
                    }
                }
            }


           
            

        }
        //https://msdn.microsoft.com/en-us/library/system.security.cryptography.aes%28v=vs.110%29.aspx
        //http://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice
        private static string DecryptFileFromBytes_Aes(string encryptedFileName, byte[] Key)
        {
            // Check arguments.
            
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
           
            FileStream encryptedFile = null;
            byte[] encryptedBytes = null;

            try
            {
                // Read plain text from file

                encryptedFile = File.OpenRead(encryptedFileName);

                encryptedBytes = new byte[encryptedFile.Length];

                encryptedFile.Read(encryptedBytes, 0, (int)encryptedFile.Length);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading encryptedfile");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (encryptedFile != null)

                {

                    encryptedFile.Close();

                }

            }

            // Declare the string used to hold
            // the decrypted text.
            

            var IV = new byte[16];
            var extensionbytes = new byte[8];

            Array.Copy(encryptedBytes, 16, extensionbytes, 0 , extensionbytes.Length);
            Array.Copy(encryptedBytes, 0, IV, 0, IV.Length);
            int offset = IV.Length + extensionbytes.Length;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter bwDecrypt = new BinaryWriter(csDecrypt))
                        {

                            //Decrypt Cipher Text from Message
                            bwDecrypt.Write(
                                 encryptedBytes,
                                 offset,
                                 encryptedBytes.Length - offset
                             );
                             
                           

                           
                        }
                    }
                    byte[] decryptedTextBytes = msDecrypt.ToArray();
                    string plaintext = Encoding.Default.GetString(msDecrypt.ToArray());
                    string extensionString = Encoding.UTF8.GetString(extensionbytes);
                    int l = extensionString.IndexOf('$');
                    extensionString = extensionString.Substring(0, l);

                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = "DecryptedFile"; // Default file name
                    dlg.DefaultExt = extensionString;

                    // Show save file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    // Process save file dialog box results
                    if (result == true)
                    {
                        try
                        {
                            string filename = dlg.FileName;
                            FileStream textwriter = File.Create(filename);
                            textwriter.Write(decryptedTextBytes, 0, decryptedTextBytes.Length);
                            textwriter.Close();
                            return filename;
                        }catch(IOException ex)
                        {
                            Console.WriteLine(ex.Message);
                            return null;
                        }
                        // Save document

                    }else
                    {
                        return null;
                    }




                }

            }

           

        }


        // https://msdn.microsoft.com/en-us/library/hk8wx38z%28v=vs.110%29.aspx
        // https://msdn.microsoft.com/en-us/library/9tsc5d0z%28v=vs.110%29.aspx
        public static void SignHash(byte[] fileHash, string privateKeyFileName, string encryptedHashFileName)
        {

            CspParameters cspParams = null;

            RSACryptoServiceProvider rsaProvider = null;

            StreamReader keyFile = null;

            string keyText = "";

            FileStream encryptedHashFile = null;

            byte[] signedHashValue = null;

            try

            {

                // Select target CSP

                cspParams = new CspParameters();

                cspParams.ProviderType = 1; // PROV_RSA_FULL

                //cspParams.ProviderName; // CSP name

                rsaProvider = new RSACryptoServiceProvider(cspParams);

                // Read public key from file

                keyFile = File.OpenText(privateKeyFileName);

                keyText = keyFile.ReadToEnd();

                // Import public key

                rsaProvider.FromXmlString(keyText);

                // Read plain text from file

                //Create an RSAPKCS1SignatureFormatter object and pass it the 
                //RSACryptoServiceProvider to transfer the private key.
                RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsaProvider);

                //Set the hash algorithm to SHA1.
                RSAFormatter.SetHashAlgorithm("MD5");

                //Create a signature for HashValue and assign it to 
                //SignedHashValue.
                signedHashValue = RSAFormatter.CreateSignature(fileHash);

                encryptedHashFile = File.Create(encryptedHashFileName);

                encryptedHashFile.Write(signedHashValue, 0, signedHashValue.Length);
            }
            catch (Exception ex)

            {

                // Any errors? Show them

                Console.WriteLine("Exception signing file!More info:");

                Console.WriteLine(ex.Message);

            }
            finally

            {

                // Do some clean up if needed

                if (keyFile != null)

                {

                    keyFile.Close();

                }

                if(encryptedHashFile != null)
                {
                    encryptedHashFile.Close();
                }



               

            }

           
           


         
                
            }

        // https://msdn.microsoft.com/en-us/library/hk8wx38z%28v=vs.110%29.aspx
        // https://msdn.microsoft.com/en-us/library/9tsc5d0z%28v=vs.110%29.aspx
        public static bool VerifySignedHash(string signedHashFileName, string publicKeyFileName, string decryptedMessageFilePath)
        {
            CspParameters cspParams = null;

            

            RSACryptoServiceProvider rsaProvider = null;

            StreamReader keyFile = null;

            string keyText = "";

            byte[] selfCreatedHash = null;


            FileStream encryptedFile = null;
            byte[] signedHashBytes = null;

            try

            {

                // Select target CSP

                cspParams = new CspParameters();

                cspParams.ProviderType = 1; // PROV_RSA_FULL

                //cspParams.ProviderName; // CSP name

                rsaProvider = new RSACryptoServiceProvider(cspParams);

                // Read public key from file

                keyFile = File.OpenText(publicKeyFileName);

                keyText = keyFile.ReadToEnd();

                // Import public key

                rsaProvider.FromXmlString(keyText);


              
                    // Read plain text from file

                    encryptedFile = File.OpenRead(signedHashFileName);

                    signedHashBytes = new byte[encryptedFile.Length];

                    encryptedFile.Read(signedHashBytes, 0, (int)encryptedFile.Length);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading encryptedfile");
                    Console.WriteLine(ex.StackTrace);
                }
                finally
                {
                    if (encryptedFile != null)

                    {

                        encryptedFile.Close();

                    }

                }

                selfCreatedHash = MD5Hash.calculateHashFromFile(decryptedMessageFilePath);

                RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(rsaProvider);
                RSADeformatter.SetHashAlgorithm("MD5");

                if (RSADeformatter.VerifySignature(selfCreatedHash, signedHashBytes))
                {
                    return true;
                }
                else
                {
                    return false;
                }


            }

       





    }
        }
