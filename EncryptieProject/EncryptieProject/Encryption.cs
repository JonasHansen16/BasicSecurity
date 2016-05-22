using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EncryptieProject
{
    public static class Encryption
    {
        /// <summary>
        /// Zorgt voor de volledige encryptie genereert 3 files die terecht komen in locationFilePath
        /// </summary>
        /// <param name="publicKeyFilePath"></param> de publickeylocatie
        /// <param name="privateKeyFilePath"></param> privatekeylocatie
        /// <param name="locationFilePath"></param> directory waar alles opgeslagen wordt
        /// <param name="toEncryptFilePath"></param> bestand dat geëncrypteerd wordt
        /// <param name="extension"></param> de extensie van het bestand dat wordt geëncrypteerd
        public static void Encrypt(string publicKeyFilePath, string privateKeyFilePath, string locationFilePath, string toEncryptFilePath, string extension)
        {
            //Aes is een ingebouwde functie in .net 
            //genereert hier de symmetrische key voor ons
            using (Aes aesAlg = Aes.Create())
            {
                byte[] key = aesAlg.Key;
                byte[] IV = aesAlg.IV;
                EncryptFileToBytes_Aes(toEncryptFilePath, locationFilePath + "\\File_1", key, IV, extension);
                EncryptFileRSA(publicKeyFilePath, key, locationFilePath + "\\File_2");
                byte[] calculatedHash = MD5Hash.calculateHashFromFile(toEncryptFilePath);
                SignHash(calculatedHash, privateKeyFilePath, locationFilePath + "\\File_3");
            }

        }
        /// <summary>
        /// volledige decryptie van de file met 2 bestanden File_1, File_2
        /// </summary>
        /// <param name="privateKeyFilePath"></param> pad naar de private key van de ontvanger
        /// <param name="encryptedMessageFilePath"></param> pad naar de geëncrypteerde boodschap (File_1)
        /// <param name="encryptedKeyFilePath"></param> pad naar de geëncrypteerde sleutel (File_2)
        /// <returns>geeft pad terug naar de plaats waar de gedecrypteerde boodschap is opgeslagen</returns>
        public static string Decrypt(string privateKeyFilePath, string encryptedMessageFilePath, string encryptedKeyFilePath)
        {
            byte[] decryptedKey = DecryptFileRSA(privateKeyFilePath, encryptedKeyFilePath);
            string decryptedMessageFilePath = DecryptFileFromBytes_Aes(encryptedMessageFilePath, decryptedKey);
            return decryptedMessageFilePath;

        }
        /// <summary>
        /// hash verifiëren nadat de decryptie is gelukt
        /// </summary>
        /// <param name="decryptedMessageFilePath"></param> pad naar de gedecrypteerde file
        /// <param name="signedHashFilePath"></param> pad naar File_3
        /// <param name="publicKeyFilePath"></param> pad naar public key van de verzender
        /// <returns>geeft een bool terug</returns>
        public static bool VerifyFile(string decryptedMessageFilePath, string signedHashFilePath, string publicKeyFilePath)
        {
           return VerifySignedHash(signedHashFilePath, publicKeyFilePath, decryptedMessageFilePath);

          

        
    }

       


        //https://blogs.msdn.microsoft.com/alejacma/2008/10/23/how-to-generate-key-pairs-encrypt-and-decrypt-data-with-net-c/
        /// <summary>
        /// Hier wordt onze symmetrische key geëncrypteerd met RSA
        /// </summary>
        /// <param name="keyFileName"></param> public key van ontvanger
        /// <param name="bytesToEncrypt"></param> bytes van onze symmetrische key
        /// <param name="encryptedFileName"></param> waar we deze file willen wegschrijven pad met hardcoded naam File_2
        public static void EncryptFileRSA(string keyFileName, byte[]bytesToEncrypt, string encryptedFileName)

        {

            //aanmaken van nodige variabelen
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
                rsaProvider = new RSACryptoServiceProvider(cspParams);

                // Read public key from file

                keyFile = File.OpenText(keyFileName);

                keyText = keyFile.ReadToEnd();

                // Import public key

                rsaProvider.FromXmlString(keyText);

                //de encryptie 
                encryptedBytes = rsaProvider.Encrypt(bytesToEncrypt, false);

                // Write encrypted bytes to file

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
        /// <summary>
        /// de decryptie van onze symmetrische key adhv RSA
        /// </summary>
        /// <param name="keyFileName"></param> pad naar de private key van de ontvanger
        /// <param name="encryptedFileName"></param> pad naar de geëncrypteerde symmetrische key
        /// <returns>geeft de symmetrische key terug als een byte array</returns>
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
                //ophalen private key
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

                // Read encrypted bytes from file

                encryptedFile = File.OpenRead(encryptedFileName);

                encryptedBytes = new byte[encryptedFile.Length];

                encryptedFile.Read(encryptedBytes, 0, (int)encryptedFile.Length);

                // Decrypt bytes

                plainBytes = rsaProvider.Decrypt(encryptedBytes, false);

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
        /// <summary>
        /// hier wordt onze file geëncrypteerd met de symmetrische sleutel en wordt opgeslagen als bytes er is dus geen extensie
        /// </summary>
        /// <param name="plainFileName"></param> pad naar de file die geëncrypteerd moet worden
        /// <param name="encryptedFileName"></param> opslaan van de encrypted file heeft het pad met hardcoded File_1 als naam
        /// <param name="Key"></param> de symmetrische sleutel die gegeneerd werd door de Aes klasse
        /// <param name="IV"></param> de initialisatie vector die nodig is voor de symmetrische encryptie gegeneerd door Aes klasse
        /// <param name="extension"></param> de extensie van de file die geëncrypteerd word
        private static void EncryptFileToBytes_Aes(string plainFileName, string encryptedFileName, byte[] Key, byte[] IV, string extension)
        {

            // initialiseren van objecten die gebruikt gaan worden
            FileStream encryptedFile = null;
            FileStream plainFile = null;
            byte[] plainbytes = null;
            byte[] extensionBytes = null;
            List<byte> extensionBytesList = new List<byte>();

            //check om te kijken of mijn keys niet leeg zijn
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            //omzetten van de extensiestring naar bytes
            extensionBytes = System.Text.Encoding.UTF8.GetBytes(extension);
            //deze bytes zetten in onze list
            extensionBytesList.AddRange(extensionBytes);

            //ik voeg hier $ toe aan het eind van mijn lijst zodat deze altijd 8 groot is omdat extensies van grootte kunnen verschillen
            for(int i = extension.Length; i < 8; i++)
            {
                extensionBytesList.Add(Convert.ToByte('$'));
            }

            //zet mijn lijst terug naar een array
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

                // hier plak ik mijn IV en extensie in een lijst achter elkaar
                list.AddRange(aesAlg.IV);
                list.AddRange(extensionBytes);

                // deze lijst zet ik dan in een byte array die ik vooraan plak in de geëncrypteerde file omdat deze nodig zijn tijdens decryptie
                byte[] prefix = list.ToArray();
                             
                
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter swEncrypt = new BinaryWriter(csEncrypt))
                        {

                            //hier steek ik dus de prefix vooraan in File_1 dit gedeelte is niet geëncrypteerd
                            msEncrypt.Write(prefix, 0, prefix.Length);
                           
                            //hier wordt de encryptie gedaan van de bytes van onze originele file
                            swEncrypt.Write(plainbytes, 0, plainbytes.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        //we schrijven onze memorystream naar een array
                        encrypted = msEncrypt.ToArray();
                         
                        try
                        {
                            //wegschrijven van de file
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
        /// <summary>
        /// Decryptie van File_1
        /// </summary>
        /// <param name="encryptedFileName"></param> pad naar File_1
        /// <param name="Key"></param>byte array van onze symmetrische key
        /// <returns>geeft het pad terug van decrypted file</returns>
        private static string DecryptFileFromBytes_Aes(string encryptedFileName, byte[] Key)
        {
            // Check arguments.
           
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
           
            //aanmaken variabelen
            FileStream encryptedFile = null;
            byte[] encryptedBytes = null;

            try
            {
                // Read bytes from file

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

            //aanmaken array voor onze IV
            var IV = new byte[16];
            //aanmaken array voor onze extensie
            var extensionbytes = new byte[8];

            //kopiëren van de extensiebytes beginnend op plaats 16 van de File_1 bytes
            Array.Copy(encryptedBytes, 16, extensionbytes, 0 , extensionbytes.Length);
            //kopiëren van de eerste 16 bytes van File_1 bytes naar IV array nodig voor de decryptie
            Array.Copy(encryptedBytes, 0, IV, 0, IV.Length);
            //offset zijn de bytes die we moeten overslaan van File_1 dit zijn dus de eerste 24 bytes
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

                            //Decrypteren bytes en wegschrijven naar onze stream
                            bwDecrypt.Write(
                                 encryptedBytes,
                                 offset,
                                 encryptedBytes.Length - offset
                             );
                             
                           

                           
                        }
                    }
                    //memorystream omzetten naar array
                    byte[] decryptedTextBytes = msDecrypt.ToArray();

                    //omzetten van onze extensiebytes naar string
                    string extensionString = Encoding.UTF8.GetString(extensionbytes);

                    //hier pakken we de substring van onze extensiestring dus kappen we alle extra '$' karakters weg
                    int l = extensionString.IndexOf('$');
                    extensionString = extensionString.Substring(0, l);

                    //een savefiledialog wordt opgeroepen om te vragen aan de user waar hij de file wilt opslaan
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = "DecryptedFile"; // Default file name
                    //extensie wordt toegevoegd in onze dialog
                    dlg.DefaultExt = extensionString;

                    // Show save file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    // Process save file dialog box results
                    if (result == true)
                    {
                        try
                        {
                            //wegschrijven van de file
                            string filename = dlg.FileName;
                            FileStream textwriter = File.Create(filename);
                            textwriter.Write(decryptedTextBytes, 0, decryptedTextBytes.Length);
                            textwriter.Close();
                            //pad van de file teruggeven
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
        /// <summary>
        /// signing van de hash met private key van verzender
        /// </summary>
        /// <param name="fileHash"></param> bytes gegeneerd door onze originele file te hashen
        /// <param name="privateKeyFileName"></param> pad naar private key van de verzender
        /// <param name="encryptedHashFileName"></param> pad waar deze file wordt opgeslagen met hardcoded File_3 als naam
        public static void SignHash(byte[] fileHash, string privateKeyFileName, string encryptedHashFileName)
        {
            //aanmaken nodige variabelen
            CspParameters cspParams = null;

            RSACryptoServiceProvider rsaProvider = null;

            StreamReader keyFile = null;

            string keyText = "";

            FileStream encryptedHashFile = null;

            byte[] signedHashValue = null;

            try

            {
                //inlezen van de private key

                // Select target CSP

                cspParams = new CspParameters();

                cspParams.ProviderType = 1; // PROV_RSA_FULL

                //cspParams.ProviderName; // CSP name

                rsaProvider = new RSACryptoServiceProvider(cspParams);

                // Read private key from file

                keyFile = File.OpenText(privateKeyFileName);

                keyText = keyFile.ReadToEnd();

                // Import private key

                rsaProvider.FromXmlString(keyText);

                //Create an RSAPKCS1SignatureFormatter object and pass it the 
                //RSACryptoServiceProvider to transfer the private key.
                RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsaProvider);

                //Set the hash algorithm to MD5.
                RSAFormatter.SetHashAlgorithm("MD5");

                //Create a signature for HashValue and assign it to 
                //SignedHashValue.
                signedHashValue = RSAFormatter.CreateSignature(fileHash);

                //wegschrijven van de file
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
        /// <summary>
        /// verifiëren van onze hash in File_3
        /// </summary>
        /// <param name="signedHashFileName"></param> pad naar file_3
        /// <param name="publicKeyFileName"></param> pad naar public key verzender
        /// <param name="decryptedMessageFilePath"></param> pad naar de file die gedecrypteerd is om te hashen
        /// <returns></returns>
        public static bool VerifySignedHash(string signedHashFileName, string publicKeyFileName, string decryptedMessageFilePath)
        {

            //aanmaken variabelen
            CspParameters cspParams = null;           
            RSACryptoServiceProvider rsaProvider = null;
            StreamReader keyFile = null;
            string keyText = "";
            byte[] selfCreatedHash = null;
            FileStream encryptedFile = null;
            byte[] signedHashBytes = null;

            try

            {
                //ophalen public key

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


              
                    // Read bytes from file

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

                //hash maken van gedecrypteerde file
                selfCreatedHash = MD5Hash.calculateHashFromFile(decryptedMessageFilePath);

                //public key steken in onze deformatter
                RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(rsaProvider);
                //hash algoritme zetten op MD5
                RSADeformatter.SetHashAlgorithm("MD5");

                //verifiëren van de hash geeft false terug als ze niet overeenkomen true als ze wel overeenkomen
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
