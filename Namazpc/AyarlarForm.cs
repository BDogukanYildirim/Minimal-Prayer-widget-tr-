using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Namazpc
{
    public class AyarlarForm : Form
    {
        private Form1 anaWidget;
        private Button btnEzan, btnHerZamanUste, btnGosterimModu;

        // Renkler
        private static readonly Color BgDark      = Color.FromArgb(18, 18, 28);
        private static readonly Color BgCard      = Color.FromArgb(38, 38, 55);
        private static readonly Color TextPrimary = Color.FromArgb(230, 230, 240);
        private static readonly Color TextMuted   = Color.FromArgb(130, 130, 160);

        // Grid sabitleri
        private const int FORM_W   = 420;
        private const int PAD      = 16;   // sol/sağ iç boşluk
        private const int GAP      = 8;    // sütunlar arası boşluk
        private const int ROW_H    = 50;   // buton yüksekliği
        private const int ROW_GAP  = 8;    // satırlar arası boşluk

        // İçerik genişliği ve tek sütun genişliği
        private int InnerW => FORM_W - PAD * 2;
        private int ColW   => (InnerW - GAP) / 2;

        public AyarlarForm(Form1 form)
        {
            anaWidget = form;
            ArayuzAyarla();
        }

        private void ArayuzAyarla()
        {
            this.Text = "Namazpc Ayarlar";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BgDark;
            this.TopMost = true;
            this.DoubleBuffered = true;

            // ── Başlık Paneli ──────────────────────────────────────────
            var pnlBaslik = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Color.Transparent
            };
            pnlBaslik.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(pnlBaslik.ClientRectangle,
                    Color.FromArgb(50, 80, 180), Color.FromArgb(25, 40, 110), LinearGradientMode.Horizontal);
                g.FillRectangle(br, pnlBaslik.ClientRectangle);
                using var pen = new Pen(Color.FromArgb(55, 255, 255, 255), 1);
                g.DrawLine(pen, 0, pnlBaslik.Height - 1, pnlBaslik.Width, pnlBaslik.Height - 1);
            };

            var lblBaslik = new Label
            {
                Text = "⚙   AYARLAR",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                BackColor = Color.Transparent
            };
            pnlBaslik.Controls.Add(lblBaslik);

            var btnKapatX = new Button
            {
                Text = "✕",
                Size = new Size(36, 36),
                Location = new Point(FORM_W - 46, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnKapatX.FlatAppearance.BorderSize = 0;
            btnKapatX.FlatAppearance.MouseOverBackColor = Color.FromArgb(180, 40, 40);
            btnKapatX.Click += (s, e) => this.Close();
            pnlBaslik.Controls.Add(btnKapatX);

            lblBaslik.MouseDown += BaslikSurkle;
            pnlBaslik.MouseDown += BaslikSurkle;

            // ── İçerik Paneli ─────────────────────────────────────────
            var pnlIcerik = new Panel
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 12, 0, 12),
                AutoScroll = true
            };

            int y = 0;

            // ═══════════════════════════════════════
            // 🎨 GÖRÜNÜM  (2 sütun × 2 satır = 4 renk butonu)
            // ═══════════════════════════════════════
            y = EkleBaslik(pnlIcerik, "🎨  Görünüm", y);

            // Satır 1: Arka Plan | Çerçeve
            EkleRenkKart(pnlIcerik, "Arka Plan",   "●", Color.FromArgb(90, 90, 100),   PAD, y,
                () => anaWidget.BackColor,   c => { anaWidget.BackColor   = c; anaWidget.AyarlariKaydet(); });
            EkleRenkKart(pnlIcerik, "Çerçeve",     "◈", Color.FromArgb(64, 130, 255),  PAD + ColW + GAP, y,
                () => anaWidget.SinirRengi,  c => { anaWidget.SinirRengi  = c; anaWidget.AyarlariKaydet(); });
            y += ROW_H + ROW_GAP;

            // Satır 2: Vakit Yazısı | Süre Yazısı
            EkleRenkKart(pnlIcerik, "Vakit Yazısı", "Aa", Color.Cyan,  PAD, y,
                () => anaWidget.VakitRengi,  c => { anaWidget.VakitRengi  = c; anaWidget.AyarlariKaydet(); });
            EkleRenkKart(pnlIcerik, "Süre Yazısı",  "Aa", Color.White, PAD + ColW + GAP, y,
                () => anaWidget.SureRengi,   c => { anaWidget.SureRengi   = c; anaWidget.AyarlariKaydet(); });
            y += ROW_H + ROW_GAP + 6;

            // ═══════════════════════════════════════
            // 🖥 DAVRANIŞ  (1 satır × 2 buton)
            // ═══════════════════════════════════════
            y = EkleBaslik(pnlIcerik, "🖥  Davranış", y);

            // Satır: Gösterim Modu | Her Zaman Üste
            btnGosterimModu = EkleButon(pnlIcerik,
                anaWidget.GosterimModu == 0 ? "Mevcut Vakit" : "Hedef Vakit",
                Color.FromArgb(55, 55, 130), PAD, y);
            btnGosterimModu.Click += (s, e) =>
            {
                anaWidget.GosterimModu = anaWidget.GosterimModu == 0 ? 1 : 0;
                btnGosterimModu.Text = anaWidget.GosterimModu == 0 ? "Mevcut Vakit" : "Hedef Vakit";
            };

            bool hzu = anaWidget.HerZamanUsteMi;
            btnHerZamanUste = EkleButon(pnlIcerik, HzuMetni(hzu),
                hzu ? Color.FromArgb(25, 95, 170) : Color.FromArgb(50, 50, 70), PAD + ColW + GAP, y);
            btnHerZamanUste.Click += (s, e) =>
            {
                anaWidget.HerZamanUsteMi = !anaWidget.HerZamanUsteMi;
                btnHerZamanUste.Text = HzuMetni(anaWidget.HerZamanUsteMi);
                btnHerZamanUste.BackColor = anaWidget.HerZamanUsteMi
                    ? Color.FromArgb(25, 95, 170) : Color.FromArgb(50, 50, 70);
            };
            y += ROW_H + ROW_GAP + 6;

            // ═══════════════════════════════════════
            // 🔔 EZAN  (1 satır × 2 buton)
            // ═══════════════════════════════════════
            y = EkleBaslik(pnlIcerik, "🔔  Ezan Sesi", y);

            // Satır: Ezan Aç/Kapat | Dosya Seç
            bool ezanAktif = anaWidget.EzanAktifMi;
            btnEzan = EkleButon(pnlIcerik, EzanMetni(ezanAktif),
                ezanAktif ? Color.FromArgb(25, 125, 70) : Color.FromArgb(50, 50, 70), PAD, y);
            btnEzan.Click += (s, e) =>
            {
                anaWidget.EzanAktifMi = !anaWidget.EzanAktifMi;
                btnEzan.Text = EzanMetni(anaWidget.EzanAktifMi);
                btnEzan.BackColor = anaWidget.EzanAktifMi
                    ? Color.FromArgb(25, 125, 70) : Color.FromArgb(50, 50, 70);
                if (!anaWidget.EzanAktifMi) anaWidget.EzanDurdur();
            };

            var btnEzanDosya = EkleButon(pnlIcerik, "📂 MP3 Seç", Color.FromArgb(45, 65, 110), PAD + ColW + GAP, y);
            btnEzanDosya.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog
                {
                    Filter = "MP3 Dosyaları|*.mp3|Tüm Dosyalar|*.*",
                    Title = "Ezan sesi dosyasını seçin"
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    anaWidget.EzanDosyasiAyarla(ofd.FileName);
                    MessageBox.Show("Ezan dosyası ayarlandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            y += ROW_H + ROW_GAP + 10;

            // ═══════════════════════════════════════
            // ALT BUTONLAR  (Kapat | Çıkış)
            // ═══════════════════════════════════════
            var btnKapatAyar = EkleButon(pnlIcerik, "✖  Kapat",
                Color.FromArgb(55, 55, 75), PAD, y);
            btnKapatAyar.Click += (s, e) => this.Close();

            var btnCikis = EkleButon(pnlIcerik, "⏻  Uygulamayı Kapat",
                Color.FromArgb(130, 28, 38), PAD + ColW + GAP, y);
            btnCikis.Click += (s, e) => Application.Exit();
            y += ROW_H + ROW_GAP;

            // Form yüksekliğini içeriğe göre ayarla
            int formH = pnlBaslik.Height + y + PAD * 2 + 8;
            this.Size = new Size(FORM_W, formH);
            this.Region = new Region(YuvarlakKose(this.ClientRectangle, 16));

            this.Controls.Add(pnlIcerik);
            this.Controls.Add(pnlBaslik);
        }

        // ── Bölüm Başlığı ─────────────────────────────────────────────
        private int EkleBaslik(Panel parent, string metin, int y)
        {
            var lbl = new Label
            {
                Text = metin,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = false,
                Size = new Size(InnerW, 20),
                Location = new Point(PAD, y),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lbl);

            var line = new Panel
            {
                Location = new Point(PAD, y + 20),
                Size = new Size(InnerW, 1),
                BackColor = Color.FromArgb(48, 48, 72)
            };
            parent.Controls.Add(line);

            return y + 30;
        }

        // ── Renk Kartı (ikon + etiket + önizleme noktası) ─────────────
        private void EkleRenkKart(Panel parent, string etiket, string ikon, Color ikonRenk,
            int x, int y, Func<Color> getColor, Action<Color> setColor)
        {
            var kart = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(ColW, ROW_H),
                BackColor = BgCard,
                Cursor = Cursors.Hand
            };
            YuvarlakKosePanel(kart, 8);

            var lblIkon = new Label
            {
                Text = ikon,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ikonRenk,
                Location = new Point(8, 0),
                Size = new Size(28, ROW_H),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblAd = new Label
            {
                Text = etiket,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = TextPrimary,
                Location = new Point(38, 0),
                Size = new Size(ColW - 74, ROW_H),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var pnlDot = new Panel
            {
                Size = new Size(20, 20),
                BackColor = getColor(),
                Location = new Point(kart.Width - 30, (ROW_H - 20) / 2),
                Cursor = Cursors.Hand
            };
            YuvarlakKosePanel(pnlDot, 5);

            Action ac = () =>
            {
                var cd = new ColorDialog { Color = getColor(), FullOpen = true };
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    setColor(cd.Color);
                    pnlDot.BackColor = cd.Color;
                }
            };
            kart.Click    += (s, e) => ac();
            lblIkon.Click += (s, e) => ac();
            lblAd.Click   += (s, e) => ac();
            pnlDot.Click  += (s, e) => ac();

            kart.Controls.Add(lblIkon);
            kart.Controls.Add(lblAd);
            kart.Controls.Add(pnlDot);

            var hoverRenk = Color.FromArgb(50, 50, 70);
            HoverEfekti(kart, BgCard, hoverRenk);
            parent.Controls.Add(kart);
        }

        // ── Standart Buton (yarım genişlik) ───────────────────────────
        private Button EkleButon(Panel parent, string metin, Color bgRenk, int x, int y)
        {
            var btn = new Button
            {
                Text = metin,
                Location = new Point(x, y),
                Size = new Size(ColW, ROW_H),
                FlatStyle = FlatStyle.Flat,
                BackColor = bgRenk,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(bgRenk.R + 22, 255),
                Math.Min(bgRenk.G + 22, 255),
                Math.Min(bgRenk.B + 22, 255));
            parent.Controls.Add(btn);
            return btn;
        }

        // ── Hover efekti ───────────────────────────────────────────────
        private void HoverEfekti(Panel pnl, Color normal, Color hover)
        {
            pnl.MouseEnter += (s, e) => pnl.BackColor = hover;
            pnl.MouseLeave += (s, e) => pnl.BackColor = normal;
            foreach (Control c in pnl.Controls)
            {
                c.MouseEnter += (s, e) => pnl.BackColor = hover;
                c.MouseLeave += (s, e) => pnl.BackColor = normal;
            }
        }

        // ── Yuvarlak köşe yolu ─────────────────────────────────────────
        private GraphicsPath YuvarlakKose(Rectangle rect, int r)
        {
            var path = new GraphicsPath();
            int d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void YuvarlakKosePanel(Panel pnl, int r)
        {
            pnl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = YuvarlakKose(pnl.ClientRectangle, r);
                pnl.Region = new Region(path);
            };
        }

        // ── Başlık sürükleme ──────────────────────────────────────────
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr h, int m, int w, int l);

        private void BaslikSurkle(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
        }

        private string HzuMetni(bool a)  => a ? "📌 Üste: AÇIK"   : "📌 Üste: KAPALI";
        private string EzanMetni(bool a) => a ? "🔔 Ezan: AÇIK"   : "🔕 Ezan: KAPALI";
    }
}