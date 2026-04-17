using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace DDS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // ==============================================================================
        // MODÜL 1: GÜVENLİK VE SINIR KONTROLÜ (Memory Overflow Koruması)
        // ==============================================================================
        // Görevi: Sadece metnin boyutunu kontrol eder ve sınırı aşarsa kırpar.
        // Bağımsızlık: Arayüzden bağımsızdır. Sadece metin alır ve verir.
        private string GirdiyiGuvenliHaleGetir(string hamMetin, int maksimumKarakter)
        {
            // Eğer metin boşsa, direkt boş döndür
            if (string.IsNullOrWhiteSpace(hamMetin))
            {
                return "";
            }

            // Eğer metin sınırımızı aşıyorsa (Destan yazılmışsa), sadece izin verilen kısmı kesip al
            if (hamMetin.Length > maksimumKarakter)
            {
                return hamMetin.Substring(0, maksimumKarakter);
            }

            // Sınırı aşmıyorsa metni olduğu gibi geri gönder
            return hamMetin;
        }


        // ==============================================================================
        // MODÜL 2: KAPSAMLI TEMİZLİK VE TÜRKÇE KARAKTER UYUMU
        // ==============================================================================
        // Görevi: Metin analizi öncesi harf/boşluk harici tüm sembolleri yok eder.
        private string MetniTamamenTemizle(string guvenliMetin)
        {
            // 1. Türkçe I ve İ harflerinde çuvallamaması için Türk kültürüne göre küçük harfe çevir.
            string kucukHarfliMetin = guvenliMetin.ToLower(new System.Globalization.CultureInfo("tr-TR"));

            // 2. REGEX:
            // [^a-z0-9çğıöşü\s] -> a-z, rakamlar, Türkçe harfler ve boşluk (\s) HARİÇ her şeyi bul.
            // Bulunan o yabancı sembolleri "" (hiçlik) ile değiştirerek yok et.
            string tertemizMetin = Regex.Replace(kucukHarfliMetin, @"[^a-z0-9çğıöşü\s]", "");

            return tertemizMetin;
        }


        // ==============================================================================
        // ANA MOTOR: "ANALİZ ET" BUTONU İŞLEMLERİ
        // ==============================================================================
        private void button1_Click(object sender, EventArgs e)
        {
            // --- [KİLİT MEKANİZMASI: BAŞLANGIÇ] ---
            // SPAM KORUMASI: Kullanıcı 2. kez basıp veritabanını kilitlemesin diye butonu pasif yap.
            button1.Enabled = false;

            // ADIM 1: Kullanıcıdan ham veriyi al
            string kullaniciMetni = richTextBox1.Text;

            // ADIM 2: Destan problemini çözen bağımsız fonksiyonumuzu çağırıyoruz (Sınır: 10.000)
            string guvenliMetin = GirdiyiGuvenliHaleGetir(kullaniciMetni, 10000);

            // Girdi Kontrolü: Eğer boşluk veya hiçlik girildiyse işlemi durdur.
            if (guvenliMetin == "")
            {
                MessageBox.Show("Lütfen analiz edilecek geçerli bir metin girin.");

                // Hata alındığı için kilidi açıp işlemi iptal ediyoruz
                button1.Enabled = true;
                return;
            }

            // Girdi Kontrolü: Sınır aşıldıysa kullanıcıyı bilgilendir.
            if (kullaniciMetni.Length > 10000)
            {
                MessageBox.Show("Uyarı: Sistem güvenliği ve performansı için metniniz ilk 10.000 karakterden sonrasını kapsamayacak şekilde kırpılmıştır.");
            }

            // ADIM 3: Metni temizle ve kelimelere böl
            string tertemizMetin = MetniTamamenTemizle(guvenliMetin);
            string[] kelimeler = tertemizMetin.Split(' ');
            int toplamKelime = kelimeler.Length;
            int riskliKelimeSayisi = 0;

            // ADIM 4: Çevrimdışı SQLite Veritabanı İşlemleri
            string baglantiYolu = "Data Source=Analiz.db;Version=3;";

            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                baglanti.Open();

                foreach (string kelime in kelimeler)
                {
                    string sorgu = "SELECT COUNT(*) FROM RiskliKelimeler WHERE Kelime = @aranan";
                    using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglanti))
                    {
                        komut.Parameters.AddWithValue("@aranan", kelime);
                        int sonuc = Convert.ToInt32(komut.ExecuteScalar());

                        if (sonuc > 0)
                        {
                            riskliKelimeSayisi++;
                        }
                    }
                }
            }

            // ADIM 5: Sonuç Hesaplama ve Akıllı Uyarı Sistemi
            if (riskliKelimeSayisi == 0)
            {
                label1.ForeColor = System.Drawing.Color.Green;
                label1.Text = "Risk: %0\nDurum: TEMİZ.\nBu metinde dezenformasyon izine rastlanmadı.";
            }
            else
            {
                double riskYuzdesi = ((double)riskliKelimeSayisi / toplamKelime) * 100;
                double yuvarlanmisRisk = Math.Round(riskYuzdesi, 2);

                // Eşik Değerleri (Risk Seviyeleri)
                if (yuvarlanmisRisk < 20)
                {
                    label1.ForeColor = System.Drawing.Color.Green;
                    label1.Text = "Risk: %" + yuvarlanmisRisk + "\nDurum: GÜVENLİ.\nMetin temiz görünüyor.";
                }
                else if (yuvarlanmisRisk >= 20 && yuvarlanmisRisk < 80)
                {
                    label1.ForeColor = System.Drawing.Color.DarkOrange;
                    label1.Text = "Risk: %" + yuvarlanmisRisk + "\nDurum: ŞÜPHELİ.\nMetinde abartılı ifadeler var, dikkatli okuyun.";
                }
                else
                {
                    label1.ForeColor = System.Drawing.Color.Red;
                    label1.Text = "Risk: %" + yuvarlanmisRisk + "\nDurum: YÜKSEK RİSK!\nDİKKAT: Bu metin açıkça manipülasyon içeriyor!";
                }
            }

            // --- [KİLİT MEKANİZMASI: BİTİŞ] ---
            // İşlem bitti, skor gösterildi. Artık butonu tekrar tıklanabilir yapıyoruz.
            button1.Enabled = true;
        }


        // ==============================================================================
        // ARAYÜZ KONTROLÜ: RESİM YAPIŞTIRMA ENGELİ
        // ==============================================================================
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Görevi : Sadece yapıştırma eylemini (Ctrl+V) dinler ve panoda resim varsa engeller.

            // Kullanıcı Ctrl + V tuş kombinasyonuna bastı mı?
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Panodaki veri bir resim mi?
                if (Clipboard.ContainsImage())
                {
                    // İşlemi iptal et
                    e.SuppressKeyPress = true;
                    MessageBox.Show("Uyarı: Sisteme sadece metin yapıştırabilirsiniz. Resimler analiz kapsamı dışındadır.");
                }
            }
        }
    }
}
