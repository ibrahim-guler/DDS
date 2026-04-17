# DDS
DDS - Çevrimdışı Doğruluk Değerlendirme Sistemi
Geliştirici: Mehmet İbrahim GÜLER

Proje Özeti: > DDS, haber metinlerini %100 çevrimdışı ve yerel bir SQLite veritabanı kullanarak analiz eden, kullanıcı gizliliğini merkeze alan bir masaüstü uygulamasıdır.

Sistemdeki Güvenlik Zırhları:

Memory Overflow Koruması: 10.000 karakter sınırı ile RAM şişmesi engellendi.

Regex Temizliği: Gizli noktalama ve manipülatif semboller temizlendi.

Input Validation: Arayüze resim/formatlı metin kopyalanması engellendi.

UI Freezing: İşlem sırasında buton kilitlenerek veritabanı çökmeleri önlendi.

Kurulum: Sistemi çalıştırmak için Analiz.db dosyasının bin/Debug klasöründe olması zorunludur.
