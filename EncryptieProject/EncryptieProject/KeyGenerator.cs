using System;
using System.IO;
using System.Security.Cryptography;

namespace EncryptieProject
{
    public static class KeyGenerator
    {

        //https://blogs.msdn.microsoft.com/alejacma/2008/10/23/how-to-generate-key-pairs-encrypt-and-decrypt-data-with-net-c/
        
        /// <summary>
        /// genereert een RSA keypair
        /// </summary>
        /// <param name="publicKeyFileName"></param>
        /// <param name="privateKeyFileName"></param> 
        public static void CreateKeys(string publicKeyFileName, string privateKeyFileName)

        {

            // Variables

            CspParameters cspParams = null;

            RSACryptoServiceProvider rsaProvider = null;

            StreamWriter publicKeyFile = null;

            StreamWriter privateKeyFile = null;

            string publicKey = "";

            string privateKey = "";

            try

            {

                // Create a new key pair on target CSP

                cspParams = new CspParameters();

                cspParams.ProviderType = 1; // PROV_RSA_FULL

                //cspParams.ProviderName; // CSP name

                cspParams.Flags = CspProviderFlags.UseArchivableKey;

                cspParams.KeyNumber = (int)KeyNumber.Exchange;

                rsaProvider = new RSACryptoServiceProvider(cspParams);
                // Export public key

                publicKey = rsaProvider.ToXmlString(false);

                // Write public key to file

                publicKeyFile = File.CreateText(publicKeyFileName);

                publicKeyFile.Write(publicKey);

                // Export private/public key pair

                privateKey = rsaProvider.ToXmlString(true);

                // Write private/public key pair to file

                privateKeyFile = File.CreateText(privateKeyFileName);

                privateKeyFile.Write(privateKey);

            }
            catch (Exception ex)

            {

                // Any errors? Show them

                Console.WriteLine("Exception generating a new key pair!More info:");

                Console.WriteLine(ex.Message);

            }

            finally

            {

                // Do some clean up if needed

                if (publicKeyFile != null)

                {

                    publicKeyFile.Close();

                }

                if (privateKeyFile != null)

                {

                    privateKeyFile.Close();

                }

            }
        }

    }
}
