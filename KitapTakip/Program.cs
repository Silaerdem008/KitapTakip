using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace KitapTakipYeni
{
    // Ctrl + Shift + B ile önce derleme
    // Ctrl + F5 ile çalıştır.
    class Program
    {
        private static readonly string DbPath = "kitaplar.db";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("🚀 Kitap Takip Sistemi Başlatılıyor...");
                VeritabaniOlustur();
                MenuGoster();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                Console.WriteLine("Devam etmek için bir tuşa basın...");
                Console.ReadKey();
            }
        }

        static void VeritabaniOlustur()
        {
            if (!File.Exists(DbPath))
            {
                using var connection = new SqliteConnection($"Data Source={DbPath}");
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE Kitaplar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Ad TEXT NOT NULL,
                        Yazar TEXT NOT NULL,
                        YayinYili INTEGER,
                        Tur TEXT
                    )";
                command.ExecuteNonQuery();
                Console.WriteLine("✅ Veritabanı oluşturuldu.");
            }
        }

        static void MenuGoster()
        {
            while (true)
            {
                Console.WriteLine("\n📚 KİTAP TAKİP SİSTEMİ");
                Console.WriteLine("======================");
                Console.WriteLine("1) Yeni Kitap Ekle");
                Console.WriteLine("2) Tüm Kitapları Listele");
                Console.WriteLine("3) Kitap Sil");
                Console.WriteLine("4) Kitap Güncelle");
                Console.WriteLine("5) Çıkış");
                Console.Write("Seçiminiz: ");

                string secim = Console.ReadLine();

                switch (secim)
                {
                    case "1":
                        KitapEkle();
                        break;
                    case "2":
                        KitaplariListele();
                        break;
                    case "3":
                        KitapSil();
                        break;
                    case "4":
                        KitaplarıGüncelle();
                        break;
                    case "5":
                        Console.WriteLine("Görüşmek üzere! 📖");
                        return;
                    default:
                        Console.WriteLine("❌ Geçersiz seçim!");
                        break;
                }
            }
        }

        static void KitapEkle()
        {
            Console.WriteLine("\n📘 Yeni Kitap Ekle");
            Console.Write("Kitap Adı: ");
            string ad = Console.ReadLine() ?? "";
            Console.Write("Yazar: ");
            string yazar = Console.ReadLine() ?? "";
            Console.Write("Yayın Yılı: ");
            string yilStr = Console.ReadLine();
            int yil = int.TryParse(yilStr, out int y) ? y : 0;
            Console.Write("Tür: ");
            string tur = Console.ReadLine() ?? "";

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Kitaplar (Ad, Yazar, YayinYili, Tur)
                VALUES ($ad, $yazar, $yil, $tur)";
            command.Parameters.AddWithValue("$ad", ad);
            command.Parameters.AddWithValue("$yazar", yazar);
            command.Parameters.AddWithValue("$yil", yil);
            command.Parameters.AddWithValue("$tur", tur);
            command.ExecuteNonQuery();

            Console.WriteLine("✅ Kitap eklendi!");
        }

        static void KitaplariListele()
        {
            Console.WriteLine("\n📖 Tüm Kitaplar");
            Console.WriteLine("---------------");

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Kitaplar";
            using var reader = command.ExecuteReader();

            bool varMi = false;
            while (reader.Read())
            {
                varMi = true;
                Console.WriteLine($"ID: {reader["Id"]}");
                Console.WriteLine($"Ad: {reader["Ad"]}");
                Console.WriteLine($"Yazar: {reader["Yazar"]}");
                Console.WriteLine($"Yıl: {reader["YayinYili"]}");
                Console.WriteLine($"Tür: {reader["Tur"]}");
                Console.WriteLine("-------------------");
            }

            if (!varMi)
            {
                Console.WriteLine("Henüz kitap eklenmemiş.");
            }
        }
        static void KitapSil()
        {
            Console.WriteLine("\n🗑️ Kitap Sil");
            Console.Write("Silmek istediğiniz kitabın ID'sini girin: ");
            string idStr = Console.ReadLine();

            if (!int.TryParse(idStr, out int id))
            {
                Console.WriteLine("❌ Geçersiz ID!");
                return;
            }

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Kitaplar WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            int affected = command.ExecuteNonQuery();

            if (affected > 0)
            {
                Console.WriteLine("✅ Kitap silindi!");
            }
            else
            {
                Console.WriteLine("❌ Bu ID'ye sahip kitap bulunamadı.");
            }
        }
        static void KitaplarıGüncelle()
        {
            Console.WriteLine("\n✏️ Kitap Güncelle");
            Console.Write("Güncellemek istediğiniz kitabın ID'sini girin: ");
            string idStr = Console.ReadLine();

            if (!int.TryParse(idStr, out int id))
            {
                Console.WriteLine("❌ Geçersiz ID!");
                return;
            }

            // Önce kitabı kontrol et
            using (var connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT Ad, Yazar, YayinYili, Tur FROM Kitaplar WHERE Id = $id";
                    checkCmd.Parameters.AddWithValue("$id", id);
                    using (var reader = checkCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Console.WriteLine("❌ Bu ID'ye sahip kitap bulunamadı.");
                            return;
                        }

                        string mevcutAd = reader["Ad"].ToString();
                        string mevcutYazar = reader["Yazar"].ToString();
                        int mevcutYil = Convert.ToInt32(reader["YayinYili"]);
                        string mevcutTur = reader["Tur"].ToString();

                        reader.Close();

                        Console.WriteLine($"\nMevcut Bilgiler:");
                        Console.WriteLine($"Ad: {mevcutAd}");
                        Console.WriteLine($"Yazar: {mevcutYazar}");
                        Console.WriteLine($"Yıl: {mevcutYil}");
                        Console.WriteLine($"Tür: {mevcutTur}");
                        Console.WriteLine("-------------------");
                    }
                }
            }

            // Yeni bilgileri al
            Console.Write("Yeni Kitap Adı (boş bırakırsanız değişmez): ");
            string yeniAd = Console.ReadLine();
            Console.Write("Yeni Yazar (boş bırakırsanız değişmez): ");
            string yeniYazar = Console.ReadLine();
            Console.Write("Yeni Yayın Yılı (boş bırakırsanız değişmez): ");
            string yeniYilStr = Console.ReadLine();
            Console.Write("Yeni Tür (boş bırakırsanız değişmez): ");
            string yeniTur = Console.ReadLine();

            // Güncelleme sorgusu
            string guncelleSql = "UPDATE Kitaplar SET ";
            var parametreler = new List<string>();
            var degerler = new List<object>();

            if (!string.IsNullOrWhiteSpace(yeniAd))
            {
                parametreler.Add("Ad = $ad");
                degerler.Add(yeniAd);
            }
            if (!string.IsNullOrWhiteSpace(yeniYazar))
            {
                parametreler.Add("Yazar = $yazar");
                degerler.Add(yeniYazar);
            }
            if (!string.IsNullOrWhiteSpace(yeniYilStr) && int.TryParse(yeniYilStr, out int yeniYil))
            {
                parametreler.Add("YayinYili = $yil");
                degerler.Add(yeniYil);
            }
            if (!string.IsNullOrWhiteSpace(yeniTur))
            {
                parametreler.Add("Tur = $tur");
                degerler.Add(yeniTur);
            }

            if (parametreler.Count == 0)
            {
                Console.WriteLine("❌ Hiçbir değişiklik yapılmadı.");
                return;
            }

            guncelleSql += string.Join(", ", parametreler);
            guncelleSql += " WHERE Id = $id";

            using (var connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = guncelleSql;
                    foreach (var deger in degerler)
                    {
                        command.Parameters.Add(new SqliteParameter());
                    }
                    // Parametreleri tekrar ekle (daha güvenli yol)
                    command.Parameters.Clear();
                    if (!string.IsNullOrWhiteSpace(yeniAd)) command.Parameters.AddWithValue("$ad", yeniAd);
                    if (!string.IsNullOrWhiteSpace(yeniYazar)) command.Parameters.AddWithValue("$yazar", yeniYazar);
                    if (!string.IsNullOrWhiteSpace(yeniYilStr) && int.TryParse(yeniYilStr, out int yil)) command.Parameters.AddWithValue("$yil", yil);
                    if (!string.IsNullOrWhiteSpace(yeniTur)) command.Parameters.AddWithValue("$tur", yeniTur);
                    command.Parameters.AddWithValue("$id", id);

                    int affected = command.ExecuteNonQuery();
                    if (affected > 0)
                    {
                        Console.WriteLine("✅ Kitap başarıyla güncellendi!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Güncelleme sırasında bir hata oluştu.");
                    }
                }
            }
        }
    }
}