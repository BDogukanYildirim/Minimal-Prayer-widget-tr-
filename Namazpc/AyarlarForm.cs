using System;
using System.Drawing;
using System.Windows.Forms;

namespace Namazpc
{
    public class AyarlarForm : Form
    {
        private Form1 anaWidget;
        private Button btnArkaPlan, btnCerceveRengi, btnVakitRengi, btnSureRengi, btnGosterimModu, btnAyarlariKapat, btnCikis;

        public AyarlarForm(Form1 form)
        {
            anaWidget = form;
            ArayuzAyarla();
        }

        private void ArayuzAyarla()
        {
            this.Text = "Ayarlar";
            // Yeni butona yer açmak için form boyunu 440'a uzattık
            this.Size = new Size(260, 440); 
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

            btnAyarlariKapat = OlusturButon("Ayarları Kapat", 295);
            btnAyarlariKapat.BackColor = Color.Gray; 
            btnAyarlariKapat.Click += (s, e) => this.Close();

            btnCikis = OlusturButon("Uygulamayı Kapat", 350);
            btnCikis.BackColor = Color.IndianRed; 
            btnCikis.Click += (s, e) => Application.Exit(); 

            this.Controls.Add(btnArkaPlan);
            this.Controls.Add(btnCerceveRengi); 
            this.Controls.Add(btnVakitRengi);
            this.Controls.Add(btnSureRengi);
            this.Controls.Add(btnGosterimModu); // Eklenen buton
            this.Controls.Add(btnAyarlariKapat);
            this.Controls.Add(btnCikis);
        }

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