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
    public abstract class Asymmetric
    {
        protected string _key = "";         // verejny klic
        protected AsymmetricAlgorithm algorithm = null;


        public KeySafe GetKeySafe(bool includePrivateKey)
        {
            KeySafe key = new KeySafe();
            key.Key = GetKey(includePrivateKey);
            key.Algorithm = this.AlgorithmName();
            return key;
        }

        public string GetKey(bool includePrivateKey)
        {
            return algorithm.ToXmlString(includePrivateKey);
        }

        public void SetKey(string XMLString)
        {
            algorithm.FromXmlString(XMLString);
        }

        public void SetKey(KeySafe key)
        {
            if (this.AlgorithmName() != key.Algorithm)
                throw new ArgumentException("Algorithm in key is another than " + this.AlgorithmName());
            algorithm.FromXmlString(key.Key);
        }

        public abstract bool ContainsPrivateKey();

        public abstract byte[] Encrypt(string str);
        public abstract byte[] Encrypt(byte[] pole);
        public abstract void EncryptStream(Stream origData, Stream encryptStream);
        public abstract string EncryptString(string message);
        public abstract string DecryptString(string message);
        public abstract void DecryptStream(Stream encryptedData, Stream outputStream);
        public abstract byte[] Decrypt(byte[] data);


        public abstract string AlgorithmName();



    }

    public class RSA : Asymmetric
    {

        private RSACryptoServiceProvider rsa = null;

        public RSA()
            : base()
        {
            algorithm = new RSACryptoServiceProvider();
            rsa = (RSACryptoServiceProvider)algorithm;
        }

        public RSA(KeySafe key)
            : base()
        {
            algorithm = new RSACryptoServiceProvider();
            rsa = (RSACryptoServiceProvider)algorithm;
            SetKey(key.Key);
        }


        public override bool ContainsPrivateKey()
        {
            return (!rsa.PublicOnly);
        }

        public RSACryptoServiceProvider CryptoProvider
        {
            get
            {
                return rsa;
            }
        }



        /// <summary>
        /// kryptovani retezce
        /// </summary>
        /// <param name="str">retezec pro kryptovani</param>
        /// <returns>kryptovany retezec</returns>
        public override byte[] Encrypt(string str)
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
        public override byte[] Encrypt(byte[] pole)
        {

            // konverze vysledne sifry do retezcove tisknutelne podoby
            byte[] result = rsa.Encrypt(pole, true);

            return result;
        }

        /// <summary>
        /// kryptovani retezce
        /// </summary>
        /// <param name="pole">pole pro kryptovani</param>
        /// <returns></returns>
        public override void EncryptStream(Stream origData, Stream encryptStream)
        {
            throw new NotImplementedException("sorry");
        }

        /// <summary>
        /// kryptovani retezce
        /// </summary>
        /// <param name="message">zprava pro zakryptovani</param>
        /// <returns></returns>
        public override string EncryptString(string message)
        {

            // konverze vysledne sifry do retezcove tisknutelne podoby
            string result = Convert.ToBase64String(Encrypt(message));

            return result;
        }

        /// <summary>
        /// dekryptovani retezce
        /// </summary>
        /// <param name="message">zprava pro dekryptovani</param>
        /// <returns>dekryptovany retezec</returns>
        public override string DecryptString(string base64Message)
        {

            try
            {
                byte[] data = Convert.FromBase64String(base64Message);
                byte[] decData = Decrypt(data);

                UnicodeEncoding UE = new UnicodeEncoding();

                return UE.GetString(decData);
            }
            catch (Exception e)
            {
                Logging.Logger.Root.Error("", e);

                throw;
            }
        }

        public override void DecryptStream(Stream encryptedData, Stream outputStream)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        /// <summary>
        /// dekryptovani retezce
        /// </summary>
        /// <param name="message">zprava pro dekryptovani</param>
        /// <returns>dekryptovany retezec</returns>
        public override byte[] Decrypt(byte[] data)
        {

            return rsa.Decrypt(data, true);
        }

        public override string AlgorithmName()
        {
            return "RSA";
        }

    }



}
