using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace RSA
{
    public static class Model
    {
        private static long n;
        private static long public_key, private_key;

        private static bool are_keys_generated;
        private static string log;

        public static bool AreKeysGenerated { get { return are_keys_generated; } }

        public static long Modulus { get { return n; } }
        public static long PublicKey { get { return public_key; } }
        public static long PrivateKey { get { return private_key; } }
        public static string Log { get { return log; } }

        public static int MaxMessageLength { get; private set; }

        private static long[] primes_for_secret_key = { 17, 19, 37, 41, 67, 73, 97, 131, 137, 193, 257, 521, 577, 641, 769, 1033, 1153, 2053, 2081, 2113, 4099, 4129, 8209, 12289, 16417, 18433, 32771, 32801, 32833, 40961, 65537 };
        private static Random random;

        public static bool IsPrime(long number)
        {
            if (number == 0)
            {
                return false;
            }
            if (number == 1)
            {
                return true;
            }
            else if (number == 2)
            {
                return true;
            }
            
            for (int i = 3; i <= (long)Math.Ceiling(Math.Sqrt(number)); i++)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetNumberOfBits(long number)
        {
            int n = 0;
            while (number > 0)
            {
                number >>= 1;
                n++;
            }
            return n;
        }

        public static bool ArePrimesValid(long p, long q, ref string error)
        {
            if (!IsPrime(p) || !IsPrime(q))
            {
                error = "Числа должны быть простыми";
                return false;
            }

            if (p < 0 || q < 0)
            {
                error = "Числа не должны быть отрицательными";
                return false;
            }

            //if (GetNumberOfBits(p) != GetNumberOfBits(q))
            //{
            //    error = "Числа должны содержать одинаковое количество бит";
            //    return false;
            //}

            if (p < 100 || q < 100)
            {
                error = "Числа должны быть больше, чем 100";
                return false;
            }

            if (p >= 10000 || q >= 10000)
            {
                error = "Числа должны быть меньше, чем 10000";
                return false;
            }

            return true;
        }

        private static long FastPow(long a, long b, long m)
        {
            if (b == 1)
            {
                return a;
            }
            long pow_b_over_2 = FastPow(a, b / 2, m) % m;
            long a1 = (pow_b_over_2 * pow_b_over_2) % m;
            if (b % 2 == 0)
            {
                return a1;
                
            }
            else
            {
                long a2 = (a1 * a) % m;
                return a2;
            }
        }

        private static long Mod(long a, long b)
        {
            long r = a % b;
            if (a < 0 || b < 0)
            {
                return b + r;
            }
            return r;
        }

        private static long Sign(long a)
        {
            if (a == 0)
            {
                return 0;
            }
            else if (a < 0)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        private static long ExtendedEuclidianAlgorithm(long a, long b, out long x, out long y)
        {
            if (a == 0)
            {
                x = 0;
                y = Sign(b);
                return b * Sign(b);
            }
            long prev_x, prev_y;
            long gcd = ExtendedEuclidianAlgorithm(Mod(b, a), a, out prev_x, out prev_y);
            if (a > 0 && b > 0)
            {
                x = prev_y - prev_x * (b / a);
                y = prev_x;
            }
            else
            {
                x = prev_y - prev_x * (b / a - 1);
                y = prev_x;
            }

            return gcd;
        }

        public static bool TryGenerateKeys(long p, long q, ref string error)
        { 
            if (!ArePrimesValid(p, q, ref error))
            {
                return false;
            }

            n = p * q;

            long rou = (p - 1) * (q - 1);

            List<long> prime_candidates = new List<long>();
            for (int i = 0; i < primes_for_secret_key.Length; i++)
            {
                long prime = primes_for_secret_key[i];
                if (prime < rou && rou % prime != 0)
                {
                    prime_candidates.Add(prime);
                }
            }

            if (prime_candidates.Count == 0)
            {
                for (int i = 17; i < rou; i++)
                {
                    if (rou % i == 0)
                    {
                        public_key = i;
                        break;
                    }
                }
            }
            else
            {
                public_key = prime_candidates[random.Next(0, prime_candidates.Count)];
            }
            
            long temp;
            long gcd = ExtendedEuclidianAlgorithm(public_key, -rou, out private_key, out temp);
            if (private_key < 0)
            {
                private_key = private_key - (private_key - rou + 1) / rou * rou;
            }
            long mod = Mod(public_key * private_key, rou);
            if (mod != 1)
            {
                throw new Exception("Invalid public or private key");
            }

            log = "";
            log += "Модуль n = " + p.ToString() + " * " + q.ToString() + " = " + n.ToString() + '\n';
            log += "rou = (" + p.ToString() + " - 1) * (" + q.ToString() + " - 1) = " + rou.ToString() + '\n';
            log += "Выбран публичный ключ " + public_key.ToString() + '\n';
            log += "Секретный ключ равен " + private_key.ToString() + '\n';
            log += public_key.ToString() + " * " + private_key.ToString() + " = " + (public_key * private_key).ToString() + '\n';
            log += (public_key * private_key).ToString() + " mod " + private_key.ToString() + " = " + mod.ToString() + '\n';

            int digits = (int)Math.Ceiling(Math.Log10(Modulus));
            if (digits % 2 == 1)
            {
                MaxMessageLength = digits / 2;
            }
            else
            {
                long max_message = 0;
                long pow_100 = 1;
                const long RUSSIAN_ALPHABET_LENGTH = 33;
                for (int i = 0; i < digits; i += 2)
                {
                    max_message += RUSSIAN_ALPHABET_LENGTH * pow_100;
                }
                if (Math.Log10(Modulus) < Math.Log10(max_message))
                {
                    MaxMessageLength = digits / 2 - 1;
                }
                else
                {
                    MaxMessageLength = digits / 2;
                }
            }

            return true;
        }

        public enum Key
        {
            PUBLIC,
            PRIVATE
        }

        private static int CODE_RUSSIAN_A = (int)'а';

        private static int LetterCode(char c)
        {
            switch (c)
            {
                case 'а': return 1;
                case 'б': return 2;
                case 'в': return 3;
                case 'г': return 4;
                case 'д': return 5;
                case 'е': return 6;
                case 'ё': return 7;
                case 'ж': return 8;
                case 'з': return 9;
                case 'и': return 10;
                case 'й': return 11;
                case 'к': return 12;
                case 'л': return 13;
                case 'м': return 14;
                case 'н': return 15;
                case 'о': return 16;
                case 'п': return 17;
                case 'р': return 18;
                case 'с': return 19;
                case 'т': return 20;
                case 'у': return 21;
                case 'ф': return 22;
                case 'х': return 23;
                case 'ц': return 24;
                case 'ч': return 25;
                case 'ш': return 26;
                case 'щ': return 27;
                case 'ъ': return 28;
                case 'ы': return 29;
                case 'ь': return 30;
                case 'э': return 31;
                case 'ю': return 32;
                case 'я': return 33;
                default: return 0;
            }
        }

        private static char LetterFromCode(int code)
        {
            switch (code)
            {
                case 1: return 'а';
                case 2: return 'б';
                case 3: return 'в';
                case 4: return 'г';
                case 5: return 'д';
                case 6: return 'е';
                case 7: return 'ё';
                case 8: return 'ж';
                case 9: return 'з';
                case 10: return 'и';
                case 11: return 'й';
                case 12: return 'к';
                case 13: return 'л';
                case 14: return 'м';
                case 15: return 'н';
                case 16: return 'о';
                case 17: return 'п';
                case 18: return 'р';
                case 19: return 'с';
                case 20: return 'т';
                case 21: return 'у';
                case 22: return 'ф';
                case 23: return 'х';
                case 24: return 'ц';
                case 25: return 'ч';
                case 26: return 'ш';
                case 27: return 'щ';
                case 28: return 'ъ';
                case 29: return 'ы';
                case 30: return 'ь';
                case 31: return 'э';
                case 32: return 'ю';
                case 33: return 'я';
                default: return '0';
            }
        }

        private static bool StringContainsOnlyRussianCharacters(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (LetterCode(text[i]) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsInteger(string text)
        {
            foreach (char c in text)
            {
                int code = (int)c;
                if (code < (int)'0' || code > '9')
                {
                    return false;
                }
            }
            return true;
        }

        private static long ConvertStringToMessage(string text)
        {
            text = text.ToLower();

            long m = 0;
            long power_of_100 = 1;
            for (int i = text.Length - 1; i >= 0; i--)
            {
                m += LetterCode(text[i]) * power_of_100;
                power_of_100 *= 100;
            }

            return m;
        }

        private static string ConvertMessageToString(long message)
        {
            string text = "";
            long power_of_100 = 1;
            int place = (int)Math.Ceiling(Math.Log10(message));
            for (int i = 0; i < place / 2 - 1; i++)
            {
                power_of_100 *= 100;
            }
            for (int i = place; i > 0; i -= 2)
            {
                int code = (int)((message / power_of_100) % 100);
                //log += code.ToString() + '\n';
                text += LetterFromCode(code);
                power_of_100 /= 100;
            }

            return text;
        }

        public static bool Encrypt(string message, Key key, ref string encrypted, ref string error)
        {
            if (message.Length > MaxMessageLength)
            {
                error = "Сообщение не должно содержать больше, чем " + MaxMessageLength.ToString() + " символов";
                return false;
            }
            if (!StringContainsOnlyRussianCharacters(message))
            {
                error = "Сообщение должно состоять только из русских букв";
                return false;
            }

            long key_number = public_key;
            if (key == Key.PRIVATE)
            {
                key_number = private_key;
            }

            long m = ConvertStringToMessage(message);

            long number_encrypted = FastPow(m, key_number, n);
            encrypted = Convert.ToString(number_encrypted);

            return true;
        }

        public static bool Decrypt(string encrypted, Key key, ref string decrypted, ref string error)
        {
            if (!IsInteger(encrypted))
            {
                error = "Сообщение должно быть числом";
                return false;
            }

            long m = Convert.ToInt64(encrypted);

            if (m < 0)
            {
                error = "Сообщение должно быть положительным числом";
                return false;
            }
            if (m > Modulus)
            {
                error = "Сообщение должно быть меньше модуля, равного " + Modulus.ToString();
                return false;
            }

            long key_number = private_key;
            if (key == Key.PUBLIC)
            {
                key_number = public_key;
            }

            long number_decrypted = FastPow(m, key_number, n);
            decrypted = Convert.ToString(ConvertMessageToString(number_decrypted));

            return true;
        }

        static Model()
        {
            random = new Random();
            MaxMessageLength = 0;
        }
    }
}
