using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WordCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            WordCounter counter = new WordCounter();
            counter.Run();
            Console.Read();
        }
    }
    class WordCounter
    {
        #region Variables
        private char[] delimiters = { '.', '?', '!' };// Cümleleri birbirinden ayırmak için cümleyi bitiren noktalama işaretleri tanımlandı.
        private int sentenceIndex = 0;
        private int threadCount = 5;// Default thread sayısı tanımlandı
        private int completedThread = 0;// Tüm threadlerin tamamlandıklarını kontrol etmek için bu parametre kullanıldı
        private List<string> wordCounts = new List<string>();// Cümle içindeki tüm kelimeler bu listeye eklenir.
        private string fileName = "C:\\test.txt";
        private string[] sentences;
        private static readonly object lockObject = new object();//Aynı anda farklı threadlerin aynı cümleye atanmaması için o fonksiyon içinde lock objesi kullanıldı
        #endregion Variables 

        /// <summary>
        /// 1. Dosya yolu ve yardımcı thread sayısı bulunur.
        /// 2. Dosya içeriği okunur
        /// 3. Cümle ayıraçlarına göre paragraf cümlelere ayrılır.
        /// 4. Thread sayısı kadar thread başlatılır.
        /// 5. Tüm thread'lerin tamamlanması beklenir.
        /// 6. Ortalama kelime sayısı ve cümle sayıları bulunur.
        /// 7. Bulunan sonuçlar ekrana yazdırılır.
        /// </summary>
        public void Run()
        {
            Initialize();

            string text = ClearText(File.ReadAllText(fileName));

            sentences = text.Split(delimiters);

            for (int i = 0; i < threadCount; i++)
            {
                Thread thread = new Thread(() =>
                {
                    ThreadProcess();
                    completedThread++;
                });
                thread.Start();
            }

            while (threadCount != completedThread) { }

            int avgScore = wordCounts.Count / (sentences.Length - 1);

            int sentenceCount = sentences.Length - 1;

            #region PrintingResult
            Console.WriteLine("Sentence Count  : " + sentenceCount);

            Console.WriteLine("Avg. Word Count : " + avgScore);

            wordCounts
                .GroupBy(info => info)
                .Select(group => new
                {
                    Word = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(a => a.Count)
                .ToList()
                .ForEach(a =>
                {
                    Console.WriteLine(a.Word + " : " + a.Count);
                });

            #endregion PrintingResult
        }

        /// <summary>
        /// Başlangıç olarak dosya yolu ve yardımcı thread sayısı alınır
        /// </summary>
        public void Initialize()
        {
            Console.WriteLine("Dosya yolu giriniz (Örnek: C:\\test.txt)");
            fileName = Console.ReadLine();
            Console.WriteLine("Yardımcı thread sayısı giriniz? (Y/N) (Default Thread : 5)");
            if (Console.ReadLine() == "Y")
            {
                Console.WriteLine("Yardımcı thread sayısını giriniz : ");
                threadCount = Convert.ToInt32(Console.ReadLine());
            }
            Console.WriteLine("İşlemler yapılıyor.");
            Console.WriteLine("....................");
        }

        /// <summary>
        /// Task Process süreci çalıştırılır
        /// </summary>
        private void ThreadProcess()
        {
            lock (lockObject)
            {
                while (sentenceIndex < sentences.Length - 1)
                {
                    WordCount(sentenceIndex);
                    sentenceIndex++;
                }
            }
        }

        /// <summary>
        /// Cümle boşluklara göre parse edilir ve wordCount listesine eklenir
        /// </summary>
        /// <param name="sentenceIndex"></param>
        public void WordCount(int sentenceIndex)
        {
            string[] words = sentences[sentenceIndex].Trim().Split(' ');
            foreach (var item in words)
            {
                wordCounts.Add(item);
            }
        }

        /// <summary>
        /// Text içerisindeki özel karakterleri temizlemek için kullanılır.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ClearText(string text)
        {
            string clearText = "";
            if (!string.IsNullOrEmpty(text))
            {
                clearText = text;
                clearText = clearText.Replace("\r", "");
                clearText = clearText.Replace("\n", " ");
                clearText = clearText.Replace("\t", " ");
                clearText = clearText.Replace(",", "");
                clearText = clearText.Replace(";", "");
                clearText = clearText.Replace("“", "");
                clearText = clearText.Replace("”", "");
                clearText = clearText.Replace("...", ".");
                clearText = clearText.Replace("-", ""); ;
            }
            return clearText;
        }
    }
}
