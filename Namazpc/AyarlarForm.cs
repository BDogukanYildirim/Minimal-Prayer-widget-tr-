using System;
using System.Drawing;
using System.Windows.Forms;

namespace Namazpc
{
    public class AyarlarForm : Form
    {
        private Form1 anaWidget;
        private Button btnArkaPlan, btnCerceveRengi, btnVakitRengi, btnSureRengi, btnGosterimModu, btnHerZamanUste, btnEzan, btnEzanDosya, btnAyarlariKapat, btnCikis;

        public AyarlarForm(Form1 form)
        {
            anaWidget = form;
            ArayuzAyarla();
        }

        private void ArayuzAyarla()
        {
            this.Text = "Ayarlar";
            this.Size = new Size(260, 610);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.BackColor = Color.FromArgb(45, 45, 45);
            this.TopMost = true;

            btnArkaPlan = OlusturButon("Arka Plan Rengini Değiştir", 20);
            btnArkaPlan.Click += (s, e) => {
                ColorDialog cd = new ColorDialog { Color = anaWidget.BackColor, FullOpen = true };
                if (cd.ShowDialog() == DialogResult.OK) { anaWidget.BackColor = cd.Color; anaWidget.AyarlariKaydet(); }
            };

            btnCerceveRengi = OlusturButon("Çerçeve Rengini Değiştir", 75);
            btnCerceveRengi.Click += (s, e) => {
                ColorDialog cd = new ColorDialog { Color = anaWidget.SinirRengi, FullOpen = true };
                if (cd.ShowDialog() == DialogResult.OK) { anaWidget.SinirRengi = cd.Color; anaWidget.AyarlariKaydet(); }
            };

            btnVakitRengi = OlusturButon("Vakit Yazı Rengini Değiştir", 130);
            btnVakitRengi.Click += (s, e) => {
                ColorDialog cd = new ColorDialog { Color = anaWidget.VakitRengi, FullOpen = true };
                if (cd.ShowDialog() == DialogResult.OK) { anaWidget.VakitRengi = cd.Color; anaWidget.AyarlariKaydet(); }
            };

            btnSureRengi = OlusturButon("Süre Yazı Rengini Değiştir", 185);
            btnSureRengi.Click += (s, e) => {
                ColorDialog cd = new ColorDialog { Color = anaWidget.SureRengi, FullOpen = true };
                if (cd.ShowDialog() == DialogResult.OK) { anaWidget.SureRengi = cd.Color; anaWidget.AyarlariKaydet(); }
            };

            // YENİ: Gösterim Modu Değiştirme Butonu
            btnGosterimModu = OlusturButon(anaWidget.GosterimModu == 0 ? "Mod: Mevcut Vakit" : "Mod: Hedef Vakit", 240);
            btnGosterimModu.BackColor = Color.DarkSlateBlue;
            btnGosterimModu.Click += (s, e) => {
                // Modu 0 ise 1 yap, 1 ise 0 yap
                anaWidget.GosterimModu = anaWidget.GosterimModu == 0 ? 1 : 0;
                // Butonun üzerindeki yazıyı anında güncelle
                btnGosterimModu.Text = anaWidget.GosterimModu == 0 ? "Mod: Mevcut Vakit" : "Mod: Hedef Vakit";
            };

            btnAyarlariKapat = OlusturButon("Ayarları Kapat", 460);
            btnAyarlariKapat.BackColor = Color.Gray; 
            btnAyarlariKapat.Click += (s, e) => this.Close();

            btnCikis = OlusturButon("Uygulamayı Kapat", 515);
            btnCikis.BackColor = Color.IndianRed; 
            btnCikis.Click += (s, e) => Application.Exit(); 

            // Her Zaman Üste toggle butonu
            bool hzu = anaWidget.HerZamanUsteMi;
            btnHerZamanUste = OlusturButon(HzuButonMetni(hzu), 295);
            btnHerZamanUste.BackColor = hzu ? Color.FromArgb(30, 100, 150) : Color.FromArgb(70, 70, 70);
            btnHerZamanUste.Click += (s, e) =>
            {
                anaWidget.HerZamanUsteMi = !anaWidget.HerZamanUsteMi;
                btnHerZamanUste.Text = HzuButonMetni(anaWidget.HerZamanUsteMi);
                btnHerZamanUste.BackColor = anaWidget.HerZamanUsteMi ? Color.FromArgb(30, 100, 150) : Color.FromArgb(70, 70, 70);
            };

            // Ezan Sesi toggle butonu
            bool ezanAktif = anaWidget.EzanAktifMi;
            btnEzan = OlusturButon(EzanButonMetni(ezanAktif), 350);
            btnEzan.BackColor = ezanAktif ? Color.FromArgb(30, 130, 90) : Color.FromArgb(100, 40, 40);
            btnEzan.Click += (s, e) =>
            {
                anaWidget.EzanAktifMi = !anaWidget.EzanAktifMi;
                btnEzan.Text = EzanButonMetni(anaWidget.EzanAktifMi);
                btnEzan.BackColor = anaWidget.EzanAktifMi ? Color.FromArgb(30, 130, 90) : Color.FromArgb(100, 40, 40);
                if (!anaWidget.EzanAktifMi) anaWidget.EzanDurdur();
            };

            // Ezan dosyası seç butonu
            btnEzanDosya = OlusturButon("Ezan Dosyası Seç (MP3)", 405);
            btnEzanDosya.BackColor = Color.FromArgb(60, 80, 120);
            btnEzanDosya.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "MP3 Dosyaları|*.mp3|Tüm Dosyalar|*.*";
                    ofd.Title = "Ezan sesi dosyasını seçin";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        anaWidget.EzanDosyasiAyarla(ofd.FileName);
                        MessageBox.Show("Ezan dosyası ayarlandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            this.Controls.Add(btnArkaPlan);
            this.Controls.Add(btnCerceveRengi); 
            this.Controls.Add(btnVakitRengi);
            this.Controls.Add(btnSureRengi);
            this.Controls.Add(btnGosterimModu);
            this.Controls.Add(btnHerZamanUste);
            this.Controls.Add(btnEzan);
            this.Controls.Add(btnEzanDosya);
            this.Controls.Add(btnAyarlariKapat);
            this.Controls.Add(btnCikis);
        }

        private string HzuButonMetni(bool aktif) => aktif ? "📌 Her Zaman Üste: AÇIK" : "📌 Her Zaman Üste: KAPALI";
        private string EzanButonMetni(bool aktif) => aktif ? "🔔 Ezan Sesi: AÇIK" : "🔕 Ezan Sesi: KAPALI";

        private Button OlusturButon(string text, int yPozisyonu)
        {
            return new Button {
                Text = text, 
                Location = new Point(20, yPozisyonu), 
                Size = new Size(200, 45),
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.FromArgb(65, 65, 65), 
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                Cursor = Cursors.Hand
            };
        }
    }
}