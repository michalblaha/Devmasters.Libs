using System;
using System.Collections.Generic;
using System.Linq;

namespace Devmasters
{
    public class Numeral
    {
        /**
 * Prevede cislo mezi dvemi soustavami
 * @param numbaseFrom soustava, ve ktere je cislo uvedene
 * @param numbaseTo soustava, do ktere cislo prevadime
 * @param number cislo k prevodu 0-9, A-Z
 * @return cislo prevedene do zadane soustavy
 * @throws java.lang.ArgumentException pokud ma kterakoliv ze soustav
 * bazi nizsi nez 2 nebo vyssi nez maxNumbase
 */
        private static char[] baseSet = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        protected int maxNumbase = 0;
        private int numbase;

        public Numeral(int numBase)
            : this(numBase, baseSet.ToList().Take(numBase).ToList())
        {

        }

        protected Numeral(char[] allowedChars)
        : this(allowedChars.ToList())
        {
        }
        protected Numeral(List<char> allowedChars)
            : this(allowedChars.Count, allowedChars)
        {

/* Unmerged change from project 'Devmasters.Core (net472)'
Before:
        }
        
        protected Numeral(int numBase, List<char> allowedChars)
After:
        }

        protected Numeral(int numBase, List<char> allowedChars)
*/
        }

        protected Numeral(int numBase, List<char> allowedChars)
        {
            this.allowedChars = allowedChars;
            maxNumbase = allowedChars.Count;
            numbase = numBase;

        }



        //char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        List<char> allowedChars;


        public int ToDecimal(string value)
        {
            return convertToDecimal(this.numbase, value);
        }
        public string FromDecimal(int value)
        {
            return convertFromDecimal(this.numbase, value);
        }

        private string convert(int numbaseFrom, int numbaseTo, String number)
        {
            return convertFromDecimal(numbaseTo, convertToDecimal(numbaseFrom, number));
        }
        /**
         * Provede prevod ze zadane soustavy do desitkove, zadana soustava je
         * reprezentovana pomoci cisel 0-9, a pismen A-Z, soustavy se zakladem
         * vyssim nez maxNumbase nejsou podporovany
         * @param numbase zaklad soustavy
         * @param number cislo k prevedeni
         * @return cislo v desitkove soustave
         * @throws java.lang.ArgumentException v pripade soustavy nizsi 2 a vyssi maxNumbase
         */
        private int convertToDecimal(int numbase, string number)
        {
            if (numbase < 2 || numbase > maxNumbase)
                throw new ArgumentException("Neplatna soustava: " + numbase);
            int result = 0; //vysledek
            int position = 0; //kolikate misto odzadu pocitame
            for (int i = number.Length - 1; i >= 0; i--)
            {
                int numeral = convertToInt(number[i]);
                if (numeral >= numbase) throw new ArgumentException("Neplatna soustava");
                result += numeral * (int)Math.Pow(numbase, position); //prevod cislice na odpovidajici pozici
                position++;// posun pozice
            }
            return result;
        }
        /**
         * Prevede cislo z desitkove soustavy do nejake jine
         * @param numbase zaklad soustavy do ktere prevadime
         * @param number desitkove cislo k prevodu
         * @return prevede cislo jako retezec 0-9 A-Z
         * @throws java.lang.ArgumentException pokud je zaklad soustavy 
         * mensi 2 nebo vyssi nez maxNumbase
         */
        private string convertFromDecimal(int numbase, int number)
        {
            if (numbase < 2 || numbase > maxNumbase)
                throw new ArgumentException("Neplatna soustava: " + numbase);
            String result = "";
            while (number != 0)
            {
                int remainder = number % numbase; //zjisteni zbytku (hodnoty na odpovidajici pozici)
                number = number / numbase; //posun o pozici dal
                result = convertToNumeral(remainder) + result;
            }
            return result;
        }
        /**
         * Prevede ASCII znak reprezentujici cislici soustavy na integer
         * @param charAt 0-9 A-Z
         * @return cislo v desitkove soustave odpovidajici hodnote cislice
         */
        private int convertToInt(char charAt)
        {

            if (allowedChars.Contains(charAt))
            {
                return allowedChars.FindIndex(m => m == charAt);
            }
            else
                throw new ArgumentException("Cislice neni v rozsahu");

            /*
            if (charAt >= 48 && charAt <= 57) return charAt - 48;
            else if (charAt >= 65 && charAt <= 90) return charAt - 65 + 10;//0-9 jsou cisla
            throw new ArgumentException("Cislice neni 0-9 A-Z");
             */

        }
        /**
         * Prevede cislici na odpovidajici znak
         * @param remainder cislice 0-35
         * @return cislice prevedena na 0-9 A-Z
         */
        private char convertToNumeral(int numeral)
        {
            if (numeral < 0 || numeral > maxNumbase)
                throw new ArgumentException("Cislice mimo rozsah");
            else
            {
                return allowedChars[numeral];
            }

            /*            
                        if (numeral >= 0 && numeral < 10) return (char)(48 + numeral);
                        else if (numeral >= 10 && numeral <= 35) return (char)(65 + numeral - 10);
                        throw new ArgumentException("Neni cislice 0-35");
             */
        }
    }
}
