using System;
using System.IO;

/* Unmerged change from project 'Devmasters.Crypto (net472)'
Before:
using System.Text;
After:
using System.IO;
*/
using System.Security.Cryptography;
using System.Text;


namespace Devmasters.Crypto
{
    public abstract class Symmetric
    {
        protected string key = "";          // klic
        protected string vector = "";               // iniciacni vektor

        protected SymmetricAlgorithm algorithm = null;


        /// <summary>
        /// 3DES - klic
        /// </summary>
        public string Key
        {
            get { return key; }
        }
        /// <summary>
        /// 3DES - iniciacni vektor
        /// </summary>
        public string Vector
        {
            get { return vector; }
        }

        /// <summary>
        /// kryptovani retezce
        /// </summary>
        /// <param name="str">retezec pro kryptovani</param>
        /// <returns>kryptovany retezec</returns>
        public byte[] Encrypt(string str)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] message = UE.GetBytes(str);

            return Encrypt(message);
        }

        /// <summary>
        /// kryptovani retezce
        /// </summary>
        /// <param name="pole">pole pro kryptovani</param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] pole)
        {
            MemoryStream ms = new MemoryStream();
            SymmetricAlgorithm prov = algorithm;

            // nastaveni klicu a iniciacnich vektoru
            prov.Key = Convert.FromBase64String(Key);
            prov.IV = Convert.FromBase64String(Vector);

            // vytvoreni toku pro kryptovani, dekodovani textu do UTF8, kryptovani v toku
            CryptoStream stm = new CryptoStream(ms, prov.CreateEncryptor(), CryptoStreamMode.Write);
            stm.Write(pole, 0, pole.Length);
            stm.Flush();
            stm.Close();
            ms.Flush();

            // konverze vysledne sifry do retezcove tisknutelne podoby
            byte[] result = ms.ToArray();
            ms.Close();

            return result;
        }

        /// <summary>
        /// kryptovani retezce
        /// </summary>
        /// <param name="pole">pole pro kryptovani</param>
        /// <returns></returns>
        public void EncryptStream(Stream origData, Stream encryptStream)
        {
            MemoryStream ms = new MemoryStream();
            SymmetricAlgorithm prov = algorithm;

            // nastaveni klicu a iniciacnich vektoru
            prov.Key = Convert.FromBase64String(Key);
            prov.IV = Convert.FromBase64String(Vector);

            // vytvoreni toku pro kryptovani, dekodovani textu do UTF8, kryptovani v toku
            CryptoStream stm = new CryptoStream(encryptStream, prov.CreateEncryptor(), CryptoStreamMode.Write);


            int read = 1;
            byte[] buffer = new byte[1023];
            while (read > 0)
            {
                read = origData.Read(buffer, 0, buffer.Length);
                if (read > 0)
                    stm.Write(buffer, 0, read);
            }
            stm.Flush();
            stm.FlushFinalBlock();
            //stm.Close();
        }

        /// <summary>
        /// kryptovani retezce
        /// </summary>
        /// <param name="message">zprava pro zakryptovani</param>
        /// <returns></returns>
        public string EncryptString(string message)
        {
            MemoryStream ms = new MemoryStream();
            SymmetricAlgorithm prov = algorithm;

            // nastaveni klicu a iniciacnich vektoru
            prov.Key = Convert.FromBase64String(Key);
            prov.IV = Convert.FromBase64String(Vector);

            // vytvoreni toku pro kryptovani, dekodovani textu do UTF8, kryptovani v toku
            CryptoStream stm = new CryptoStream(ms, prov.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] vstup = System.Text.UTF8Encoding.UTF8.GetBytes(message);
            stm.Write(vstup, 0, vstup.Length);
            stm.Flush();
            stm.Close();
            ms.Flush();

            // konverze vysledne sifry do retezcove tisknutelne podoby
            string result = Convert.ToBase64String(ms.ToArray());
            ms.Close();

            return result;
        }

        /// <summary>
        /// dekryptovani retezce
        /// </summary>
        /// <param name="message">zprava pro dekryptovani</param>
        /// <returns>dekryptovany retezec</returns>
        public string Decrypt(string message)
        {
            if (message == null)
                return null;

            MemoryStream ms = new MemoryStream(Convert.FromBase64String(message));
            SymmetricAlgorithm prov = algorithm;

            // nastaveni klicu a iniciacnich vektoru
            prov.Key = Convert.FromBase64String(Key);
            prov.IV = Convert.FromBase64String(Vector);

            string result = null;
            try
            {
                // vytvoreni toku pro kryptovani, dekryptovani v toku
                CryptoStream stm = new CryptoStream(ms, prov.CreateDecryptor(), CryptoStreamMode.Read);

                StreamReader sr = new StreamReader(stm, Encoding.UTF8);

                //byte[] vystup = new byte[10000];
                // int precteneByty = stm.Read(vystup, 0, 10000);
                // dekodovani textu do UTF8, cteni desifrovaneho textu
                //result = UTF8Encoding.UTF8.GetString(vystup, 0, precteneByty);
                result = sr.ReadToEnd();
                stm.Close();
                ms.Close();
            }
            catch (Exception e)
            {
                Logging.Logger.Root.Error(string.Empty, e);

                throw;
            }
            return result;
        }

        public void DecryptStream(Stream encryptedData, Stream outputStream)
        {
            if ((encryptedData == null) || (outputStream == null))
                throw new NullReferenceException("Parameters cannot be null");

            SymmetricAlgorithm prov = algorithm;
            // vytvoreni toku pro kryptovani, dekryptovani v toku
            prov.Key = Convert.FromBase64String(Key);
            prov.IV = Convert.FromBase64String(Vector);
            CryptoStream stm = new CryptoStream(encryptedData, prov.CreateDecryptor(), CryptoStreamMode.Read);

            int read = 1;
            byte[] buffer = new byte[1023];
            while (read > 0)
            {
                read = stm.Read(buffer, 0, buffer.Length);
                outputStream.Write(buffer, 0, read);
            }
            stm.Flush();
            //stm.FlushFinalBlock();
            stm.Close();
        }


        /// <summary>
        /// dekryptovani retezce
        /// </summary>
        /// <param name="message">zprava pro dekryptovani</param>
        /// <returns>dekryptovany retezec</returns>
        public byte[] Decrypt(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            MemoryStream msread = new MemoryStream(data);
            byte[] result;
            try
            {
                DecryptStream(ms, msread);
                result = msread.ToArray();
                msread.Close();
            }
            catch (Exception e)
            {
                Logging.Logger.Root.Error(string.Empty, e);

                throw;
            }
            return result;
        }


        #region Generate - 3DES
        /// <summary>
        /// generovani klice a iniciacniho vektoru
        /// </summary>
        public void GenerateKeyVector()
        {
            SymmetricAlgorithm prov = algorithm;

            // generovani noveho klice a vektoru
            prov.GenerateKey();
            prov.GenerateIV();
            // konverze bitoveho pole na retezec (pristupne jako property)
            key = Convert.ToBase64String(prov.Key);
            vector = Convert.ToBase64String(prov.IV);
        }

        #endregion

        public abstract string AlgorithmName();

        public KeySafe GetKeySafe()
        {
            KeySafe key = new KeySafe();
            key.Key = this.Key;
            key.Vector = this.Vector;
            key.Algorithm = this.AlgorithmName();

            return key;
        }

        public void SetKey(KeySafe key)
        {
            if (this.AlgorithmName() != key.Algorithm)
                throw new ArgumentException("Algorithm in key is another than " + this.AlgorithmName());
            this.key = key.Key;
            this.vector = key.Vector;
        }

    }

    public class DES : Symmetric
    {

        public DES()
            : base()
        {
            algorithm = new DESCryptoServiceProvider();
        }

        public DES(KeySafe key) : this()
        {
            base.SetKey(key);
        }

        public override string AlgorithmName()
        {
            return "DES";
        }

    }

    public class Rijndael : Symmetric
    {

        public Rijndael()
            : base()
        {
            algorithm = SymmetricAlgorithm.Create();
        }

        public Rijndael(KeySafe key)
            : this()
        {
            base.SetKey(key);
        }

        public override string AlgorithmName()
        {
            return "Rijndael";
        }
    }


    public class TripleDES : Symmetric
    {
        //private string tripleDesKey = "WOwpDG7ZKEKLfgy/eYLlDpKtSTObyU6d";			// klic
        //private string tripleDesVector = "AJ2E5WoMJA8=";			    // iniciacni vektor

        public TripleDES()
            : base()
        {
            algorithm = new TripleDESCryptoServiceProvider();
        }

        public TripleDES(KeySafe key)
            : this()
        {
            base.SetKey(key);
        }

        public override string AlgorithmName()
        {
            return "TripleDES";
        }

    }



}
