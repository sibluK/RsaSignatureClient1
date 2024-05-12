using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

namespace Client1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string serverIP = "127.0.0.1";
                int serverPort = 7070;

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                using Socket client = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await client.ConnectAsync(ipEndPoint);
                Console.WriteLine("Prisijungta prie serverio.");

                ///////////////////////////////////////////////////////////////////////////////////
                Console.Write("Iveskite teksta: ");
                string tekstas = Console.ReadLine();
                Console.Write("Iveskite p: ");
                BigInteger p = BigInteger.Parse(Console.ReadLine());

                while (!IsPrime(p))
                {
                    Console.Write("p nera pirminis skaicius, iveskite kita: ");
                    p = BigInteger.Parse(Console.ReadLine());
                }

                Console.Write("Iveskite q: ");
                BigInteger q = BigInteger.Parse(Console.ReadLine());
                while (!IsPrime(q))
                {
                    Console.Write("q nera pirminis skaicius, iveskite kita: ");
                    q = BigInteger.Parse(Console.ReadLine());
                }

                Tuple<BigInteger, BigInteger> publicKey = createPublicKey(p, q);
                BigInteger n = publicKey.Item1;
                BigInteger e = publicKey.Item2;
                WriteToFile(publicKeyFile, $"{n},{e}");
            
                BigInteger privateKey = createPrivateKey(e, (p - 1)*(q - 1));
                WriteToFile(privateKeyFile, privateKey.ToString());

                Tuple<string, string> signatureResult = signature(tekstas, privateKey, n);
                WriteToFile(signatureFile, $"{signatureResult.Item1}\n{signatureResult.Item2}");

                ///////////////////////////////////////////////////////////////////////////////////
                byte[] messageBytes = Encoding.UTF8.GetBytes($"{n},{e},{privateKey},{signatureResult.Item1},{signatureResult.Item2}");
                await client.SendAsync(messageBytes, SocketFlags.None);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Įvyko klaida: {ex.Message}");
            }
        }

        private static string publicKeyFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\Client1\\publicKey.txt";
        private static string privateKeyFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\Client1\\privateKey.txt";
        private static string signatureFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\Client1\\signature.txt";

        private static void WriteToFile(string fileName, string text)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(text);
            }
        }

        private static bool IsPrime(BigInteger number)
        {
            if (number <= 1)
                return false;
            if (number <= 3)
                return true;

            if (number % 2 == 0 || number % 3 == 0)
                return false;

            for (int i = 5; i * i <= number; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
            }

            return true;
        }

        private static BigInteger euclidAlgorithm(BigInteger a, BigInteger b)
        {
            if (a == 0)
                return b;

            return euclidAlgorithm(b % a, a);
        }

        private static BigInteger extendedEuclidAlgorithm(BigInteger a, BigInteger b, ref BigInteger s, ref BigInteger t)
        {
            if (a == 0)
            {
                s = 0;
                t = 1;
                return b;
            }

            BigInteger s1 = 1, t1 = 0;
            BigInteger DBD = extendedEuclidAlgorithm(b % a, a, ref s1, ref t1);

            s = t1 - (b / a) * s1;
            t = s1;

            return DBD;
        }

        private static Tuple<BigInteger, BigInteger> createPublicKey(BigInteger p, BigInteger q)
        {
            BigInteger n = p * q;

            BigInteger phi = (p - 1) * (q - 1);

            BigInteger e = generateE(phi);

            return Tuple.Create(n, e);
        }

        private static BigInteger createPrivateKey(BigInteger e, BigInteger phi)
        {
            BigInteger s = 0, t = 0;
            BigInteger gcd = extendedEuclidAlgorithm(e, phi, ref s, ref t);

            BigInteger d = (s % phi + phi) % phi;

            return d;
        }

        private static BigInteger generateE(BigInteger phi)
        {
            BigInteger e = 3;

            while (euclidAlgorithm(e, phi) != 1)
            {
                e += 2;
            }

            return e;
        }

        private static Tuple<string, string> signature(string x, BigInteger d, BigInteger n)
        {
            StringBuilder sText = new StringBuilder();
            StringBuilder xText = new StringBuilder();

            int current = 0;
            foreach (char letter in x)
            {
                BigInteger asciiCode = new BigInteger(letter);
                xText.Append(asciiCode);
                BigInteger s = BigInteger.ModPow(asciiCode, d, n);
                sText.Append(s);

                if (current != x.Length)
                {
                    xText.Append(' ');
                    sText.Append(' ');
                }
            }
            return Tuple.Create(xText.ToString(), sText.ToString());
        }
    }
}
