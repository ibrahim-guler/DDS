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
        private string GirdiyiGuvenliHaleGetir(string hamMetin, int maksimumKarakter)
        {
            if (string.IsNullOrWhiteSpace(hamMetin)) return "";
            if (hamMetin.Length > maksimumKarakter) return hamMetin.Substring(0, maksimumKarakter);
            return hamMetin;
        }

        // ==============================================================================
        // MODÜL 2: KAPSAMLI TEMİZLİK (REGEX) VE TÜRKÇE KARAKTER UYUMU
        // ==============================================================================
        private string MetniTamamenTemizle(string guvenliMetin)
        {
            string kucukHarfliMetin = guvenliMetin.ToLower(new System.Globalization.CultureInfo("tr-TR"));
            string tertemizMetin = Regex.Replace(kucukHarfliMetin, @"[^a-z0-9çğıöşü\s]", "");
            return tertemizMetin;
        }

        // ==============================================================================
        // MODÜL 3: VERİTABANI VE MATEMATİK MOTORU (Hibrit Algoritma)
        // ==============================================================================
        // Görevi: Sadece kelime dizisini alır, veritabanına sorar ve matematiği yapar.
        // Bağımsızlık: Arayüzdeki Butondan veya textboxtan habersizdir. Sadece sayı döndürür.
        private double HibritRiskHesapla(string[] kelimeler)
        {
            int riskliKelimeSayisi = 0;
            int toplamRiskPuani = 0;
            int toplamKelime = kelimeler.Length;

            if (toplamKelime == 0) return 0;

            string baglantiYolu = "Data Source=Analiz.db;Version=3;";

            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                baglanti.Open();

                foreach (string kelime in kelimeler)
                {
                    // Güvenli parametreli sorgumuz (SQL Injection Korumalı)
                    string sorgu = "SELECT riskPuani FROM RiskliKelimeler WHERE Kelime = @aranan";
                    using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglanti))
                    {
                        komut.Parameters.AddWithValue("@aranan", kelime);
                        object sonuc = komut.ExecuteScalar();

                        if (sonuc != null)
                        {
                            riskliKelimeSayisi++;
                            toplamRiskPuani += Convert.ToInt32(sonuc);
                        }
                    }
                }
            }

            if (riskliKelimeSayisi == 0) return 0;

            // Hibrit Formülümüz
            double yogunluk = ((double)riskliKelimeSayisi / toplamKelime) * 100;
            double riskYuzdesi = (toplamRiskPuani * 1.5) + yogunluk;

            if (riskYuzdesi > 100) riskYuzdesi = 100;

            return Math.Round(riskYuzdesi, 2);
        }

        // ==============================================================================
        // ANA VİTRİN: "ANALİZ ET" BUTONU İŞLEMLERİ (Sadece organizasyon yapar)
        // ==============================================================================
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            // ADIM 1 & 2: Veriyi al, kontrol et ve temizle (Bağımsız Modülleri kullanarak)
            string kullaniciMetni = richTextBox1.Text;
            string guvenliMetin = GirdiyiGuvenliHaleGetir(kullaniciMetni, 10000);

            if (guvenliMetin == "")
            {
                MessageBox.Show("Lütfen analiz edilecek geçerli bir metin girin.");
                button1.Enabled = true;
                return;
            }

            if (kullaniciMetni.Length > 10000)
            {
                MessageBox.Show("Uyarı: Sistem güvenliği ve performansı için metniniz ilk 10.000 karakterden sonrasını kapsamayacak şekilde kırpılmıştır.");
            }

            string tertemizMetin = MetniTamamenTemizle(guvenliMetin);
            string[] kelimeler = tertemizMetin.Split(' ');

            // ===========================================================
            // ADIM 3: İŞİ UZMANINA DEVRET (Low Coupling) **modül 3**
            // ===========================================================
            double yuvarlanmisRisk = HibritRiskHesapla(kelimeler);

            // ADIM 4: Gelen sonuca göre arayüzü (vitrini) boya
            if (yuvarlanmisRisk == 0)
            {
                label1.ForeColor = System.Drawing.Color.Green;
                label1.Text = "Risk: %0\nDurum: TEMİZ.\nBu metinde dezenformasyon izine rastlanmadı.";
            }
            else if (yuvarlanmisRisk < 20)
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

            button1.Enabled = true;
        }

        // ==============================================================================
        // ARAYÜZ (UI) KONTROLÜ: RESİM YAPIŞTIRMA ENGELİ
        // ==============================================================================
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsImage())
                {
                    e.SuppressKeyPress = true;
                    MessageBox.Show("Uyarı: Sisteme sadece metin yapıştırabilirsiniz. Resimler analiz kapsamı dışındadır.");
                }
            }
        }
    }
}
