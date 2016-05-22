using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EncryptieProject
{
    public static class MD5Hash
    {
        /// <summary>
        /// Hash maken van onze originele file
        /// </summary>
        /// <param name="originalFilePath"></param> pad naar onze file
        /// <returns>hash bytes</returns>
        public static byte[] calculateHashFromFile(string originalFilePath)
        {
            byte[] buffer; //delen van het bestand opslaan, zodat de verschillende delen gehashed kunnen worden
            int bytesRead; //om bij te houden hoeveel bytes er al ingelezen zijn
            long fileSize; //de file grootte
            long totalBytesRead = 0; //hoeveel bytes er in totaal al ingelezen zijn

            using (Stream file = File.OpenRead(originalFilePath))
            {
                fileSize = file.Length;

                using (HashAlgorithm hasher = MD5.Create())
                {
                    //ieder deel van de file moet gehashed worden dus een loop totdat alle bytes ingelezen en gehashed zijn.
                    do
                    {
                        buffer = new byte[4096]; //nieuwe buffer initialiseren met bepaalde grootte, 4069 bytes is een ideale grootte

                        bytesRead = file.Read(buffer, 0/*startPlaats*/, buffer.Length); //dit geeft terug hoeveel bytes van het bestand al ingelezen zijn
                        totalBytesRead += bytesRead;

                        hasher.TransformBlock(buffer/*dit deel wordt gehashed*/, 0, bytesRead/*aantal bytes dat gehashed moet worden*/, null, 0);

                    }
                    while (bytesRead != 0); //als het aan het einde van de file zit, wordt er niks gelezen dus bytesRead blijft 0

                    //alle hash's samenzetten om de uiteindelijke hash te bekomen
                    hasher.TransformFinalBlock(buffer, 0, 0);

                    return hasher.Hash;
                }

            }
        }

       

    }
}
