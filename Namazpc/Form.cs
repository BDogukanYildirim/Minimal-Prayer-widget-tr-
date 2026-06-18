using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.Media;
using System.IO;
using System.Diagnostics;

namespace Namazpc
{
    public partial class Form1 : Form
    {
        private Label lblVakit;
        private Label lblSure;
        private System.Windows.Forms.Timer mainTimer;

        private double lat = 0, lng = 0;
        private bool konumBulundu = false;
        private int aranıyorAnimasyonSayaci = 0;

        private bool soruEkraniAktif = false;
        private bool kullaniciCevrimdisiReddetti = false;
        private Button btnOnayla;
        private Button btnReddet;

        private bool ilkKonumAyarlandi = false;
        private float fontVakitSize = 28f;
        private float fontSureSize = 24f;

        private DateTime[] vakitler = new DateTime[6];
        private string[] vakitIsimleri = { "İmsak", "Güneş", "Öğle", "İkindi", "Akşam", "Yatsı" };

        // Ezan: hangi vakitlerde ezan çalındı (gün başına sıfırlanır)
        private bool[] ezanCalindi = new bool[6];
        private DateTime sonEzanGunu = DateTime.MinValue;

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(string command, System.Text.StringBuilder returnString, int returnLength, IntPtr hwndCallback);

        public Color VakitRengi { get { return lblVakit.ForeColor; } set { lblVakit.ForeColor = value; } }
        public Color SureRengi { get { return lblSure.ForeColor; } set { lblSure.ForeColor = value; } }
        private Color _sinirRengi = Color.DodgerBlue;
        public Color SinirRengi { get { return _sinirRengi; } set { _sinirRengi = value; this.Invalidate(); } }

        public int GosterimModu
        {
            get { return guncelAyarlar.GosterimModu; }
            set { guncelAyarlar.GosterimModu = value; AyarlariKaydet(); }
        }

        public bool EzanAktifMi
        {
            get { return guncelAyarlar.EzanAktif; }
            set { guncelAyarlar.EzanAktif = value; AyarlariKaydet(); }
        }

        public bool HerZamanUsteMi
        {
            get { return guncelAyarlar.HerZamanUste; }
            set { guncelAyarlar.HerZamanUste = value; this.TopMost = value; AyarlariKaydet(); }
        }

        private string ayarlarYolu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Namazpc", "ayarlar.json");
        private string vakitlerYolu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Namazpc", "AdhanTimes.json");

        private AyarlarData guncelAyarlar = new AyarlarData();
        private Dictionary<string, string[]> guncelVakitler = new Dictionary<string, string[]>();

        public Form1()
        {
            ArayuzAyarla();
            DosyalariYukle();
            _ = BaslatAsync();
        }

        private void ArayuzAyarla()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(35, 35, 35);
            this.MinimumSize = new Size(50, 20);
            this.StartPosition = FormStartPosition.Manual;

            // YENİ: Titremeyi (Flickering) %100 önleyen donanımsal özellik
            this.DoubleBuffered = true;

            lblVakit = new Label { AutoSize = true, ForeColor = Color.Cyan, Font = new Font("Segoe UI", fontVakitSize, FontStyle.Bold), Text = "Başlatılıyor...", TextAlign = ContentAlignment.MiddleCenter };
            lblSure = new Label { AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", fontSureSize, FontStyle.Bold), Text = "...", TextAlign = ContentAlignment.MiddleCenter };

            btnOnayla = new Button { Text = "Onayla", Size = new Size(95, 35), Visible = false, FlatStyle = FlatStyle.Flat, BackColor = Color.SeaGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnOnayla.FlatAppearance.BorderSize = 0;
            btnOnayla.Click += Onayla_Click;

            btnReddet = new Button { Text = "Reddet", Size = new Size(95, 35), Visible = false, FlatStyle = FlatStyle.Flat, BackColor = Color.IndianRed, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnReddet.FlatAppearance.BorderSize = 0;
            btnReddet.Click += (s, e) => { soruEkraniAktif = false; kullaniciCevrimdisiReddetti = true; };

            this.Controls.Add(lblVakit); this.Controls.Add(lblSure);
            this.Controls.Add(btnOnayla); this.Controls.Add(btnReddet);

            this.MouseDown += FormMouseOlaylari;
            lblVakit.MouseDown += FormMouseOlaylari;
            lblSure.MouseDown += FormMouseOlaylari;
            this.MouseWheel += FareTekerlegiIleOlcekle;
            this.FormClosing += (s, e) => AyarlariKaydet();
        }

        private void Onayla_Click(object sender, EventArgs e)
        {
            soruEkraniAktif = false;
            string bugunStr = DateTime.Now.ToString("dd_MM_yyyy");
            if (guncelVakitler.ContainsKey(bugunStr))
            {
                VakitleriAta(bugunStr);
                konumBulundu = true;
            }
        }

        private void VakitleriAta(string tarihKey)
        {
            string[] v = guncelVakitler[tarihKey];
            string[] parts = tarihKey.Split('_');
            string formatliTarih = $"{parts[2]}-{parts[1]}-{parts[0]}";

            for (int i = 0; i < 6; i++)
                vakitler[i] = DateTime.Parse($"{formatliTarih} {v[i]}");
        }

        private async Task BaslatAsync()
        {
            mainTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            mainTimer.Tick += MainTimer_Tick;
            mainTimer.Start();

            string bugunStr = DateTime.Now.ToString("dd_MM_yyyy");

            if (guncelVakitler.ContainsKey(bugunStr))
            {
                VakitleriAta(bugunStr);
                konumBulundu = true;
                _ = KonumVeVakitCek();
            }
            else
            {
                while (!konumBulundu)
                {
                    await KonumVeVakitCek();
                    if (!konumBulundu) await Task.Delay(5000);
                }
            }
        }

        private async Task KonumVeVakitCek()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    string ipJson = await client.GetStringAsync("https://ipinfo.io/json");
                    using (JsonDocument doc = JsonDocument.Parse(ipJson))
                    {
                        string loc = doc.RootElement.GetProperty("loc").GetString() ?? "";
                        string[] parts = loc.Split(',');
                        lat = double.Parse(parts[0], CultureInfo.InvariantCulture);
                        lng = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        guncelAyarlar.SonSehir = doc.RootElement.GetProperty("city").GetString() + ", " + doc.RootElement.GetProperty("region").GetString();
                    }

                    string latStr = lat.ToString(CultureInfo.InvariantCulture);
                    string lngStr = lng.ToString(CultureInfo.InvariantCulture);

                    string urlBugun = $"https://api.aladhan.com/v1/timings?latitude={latStr}&longitude={lngStr}&method=13";
                    string bugunJson = await client.GetStringAsync(urlBugun);
                    using (JsonDocument doc = JsonDocument.Parse(bugunJson))
                    {
                        var t = doc.RootElement.GetProperty("data").GetProperty("timings");
                        string bugunStr = DateTime.Now.ToString("dd_MM_yyyy");

                        guncelVakitler[bugunStr] = new string[] {
                            t.GetProperty("Fajr").GetString()?.Substring(0,5) ?? "",
                            t.GetProperty("Sunrise").GetString()?.Substring(0,5) ?? "",
                            t.GetProperty("Dhuhr").GetString()?.Substring(0,5) ?? "",
                            t.GetProperty("Asr").GetString()?.Substring(0,5) ?? "",
                            t.GetProperty("Maghrib").GetString()?.Substring(0,5) ?? "",
                            t.GetProperty("Isha").GetString()?.Substring(0,5) ?? ""
                        };
                        VakitleriAta(bugunStr);
                    }

                    konumBulundu = true;
                    soruEkraniAktif = false;
                    kullaniciCevrimdisiReddetti = false;
                    AyarlariKaydet();
                    VakitleriKaydet();

                    int icindeBulundugumuzYil = DateTime.Now.Year;
                    Dictionary<string, string[]> tumGelenVeriler = new Dictionary<string, string[]>();

                    for (int yil = icindeBulundugumuzYil; yil <= icindeBulundugumuzYil + 1; yil++)
                    {
                        string urlYil = $"https://api.aladhan.com/v1/calendar/{yil}?latitude={latStr}&longitude={lngStr}&method=13";
                        string takvimJson = await client.GetStringAsync(urlYil);

                        using (JsonDocument doc = JsonDocument.Parse(takvimJson))
                        {
                            var aylar = doc.RootElement.GetProperty("data").EnumerateObject();
                            foreach (var ay in aylar)
                            {
                                var gunler = ay.Value.EnumerateArray();
                                foreach (var gun in gunler)
                                {
                                    string tarih = gun.GetProperty("date").GetProperty("gregorian").GetProperty("date").GetString() ?? "";
                                    DateTime dt = DateTime.ParseExact(tarih, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                    string key = dt.ToString("dd_MM_yyyy");

                                    var t = gun.GetProperty("timings");
                                    tumGelenVeriler[key] = new string[] {
                                        t.GetProperty("Fajr").GetString()?.Substring(0,5) ?? "",
                                        t.GetProperty("Sunrise").GetString()?.Substring(0,5) ?? "",
                                        t.GetProperty("Dhuhr").GetString()?.Substring(0,5) ?? "",
                                        t.GetProperty("Asr").GetString()?.Substring(0,5) ?? "",
                                        t.GetProperty("Maghrib").GetString()?.Substring(0,5) ?? "",
                                        t.GetProperty("Isha").GetString()?.Substring(0,5) ?? ""
                                    };
                                }
                            }
                        }
                    }

                    guncelVakitler.Clear();
                    for (int i = 0; i <= 365; i++)
                    {
                        string arananTarih = DateTime.Now.AddDays(i).ToString("dd_MM_yyyy");
                        if (tumGelenVeriler.ContainsKey(arananTarih))
                        {
                            guncelVakitler[arananTarih] = tumGelenVeriler[arananTarih];
                        }
                    }

                    guncelAyarlar.SonGuncelleme = DateTime.Now;
                    AyarlariKaydet();
                    VakitleriKaydet();
                }
                catch
                {
                    if (!konumBulundu && !soruEkraniAktif && !kullaniciCevrimdisiReddetti)
                    {
                        string bugunStr = DateTime.Now.ToString("dd_MM_yyyy");
                        if (guncelVakitler.ContainsKey(bugunStr))
                        {
                            soruEkraniAktif = true;
                        }
                        else
                        {
                            lblVakit.Text = "Hata!";
                            lblSure.Text = "Çevrimdışı veriler tükendi.\nLütfen internete bağlanın.";
                        }
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.BackColor);
            int kavis = 30; int kalinlik = 7;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            using (GraphicsPath path = OvalYolOlustur(rect, kavis)) { this.Region = new Region(path); }
            int offset = kalinlik / 2;
            Rectangle cerceveRect = new Rectangle(offset - 2, offset - 1, this.Width - kalinlik + 3, this.Height - kalinlik + 3);
            using (GraphicsPath cercevePath = OvalYolOlustur(cerceveRect, kavis - offset))
            using (Pen pen = new Pen(SinirRengi, kalinlik)) { e.Graphics.DrawPath(pen, cercevePath); }
        }

        private GraphicsPath OvalYolOlustur(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) radius = 1; int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure(); return path;
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (soruEkraniAktif)
            {
                lblVakit.Font = new Font("Segoe UI", 14, FontStyle.Bold); lblVakit.Text = "Bağlantı Koptu";
                lblSure.Font = new Font("Segoe UI", 10); lblSure.Text = $"Son Konum: {guncelAyarlar.SonSehir}\nDevam edilsin mi?";
                btnOnayla.Visible = true; btnReddet.Visible = true; DinamikHizalaSoru(); return;
            }
            if (!konumBulundu)
            {
                aranıyorAnimasyonSayaci++; string noktalar = new string('.', aranıyorAnimasyonSayaci % 4);
                lblVakit.Text = "Bağlantı Aranıyor"; lblSure.Text = $"Lütfen Bekleyin{noktalar}";
                btnOnayla.Visible = false; btnReddet.Visible = false; DinamikHizala(); return;
            }
            btnOnayla.Visible = false; btnReddet.Visible = false;

            DateTime simdi = DateTime.Now;
            int siradakiIdx = -1;

            // Gün değiştiyse ezan takip dizisini sıfırla
            if (simdi.Date != sonEzanGunu.Date)
            {
                ezanCalindi = new bool[6];
                sonEzanGunu = simdi;
            }

            for (int i = 0; i < vakitler.Length; i++)
            {
                if (simdi < vakitler[i]) { siradakiIdx = i; break; }
            }

            if (siradakiIdx == -1)
            {
                // Gece yarısı geçişi: Ertesi günün vakitlerini yükle
                string yarınStr = DateTime.Now.ToString("dd_MM_yyyy");
                if (guncelVakitler.ContainsKey(yarınStr))
                {
                    VakitleriAta(yarınStr);
                }
                else
                {
                    // Sözlükte yoksa tüm vakitlere 1 gün ekle (geçici)
                    for (int j = 0; j < vakitler.Length; j++)
                        vakitler[j] = vakitler[j].AddDays(1);
                }
                siradakiIdx = 0;
                // Arka planda yeni günün verilerini çek
                _ = KonumVeVakitCek();
            }

            int suankiIdx = (siradakiIdx - 1 + 6) % 6;

            // Ezan: geçen vakit (suankiIdx) için ezan çal (ilk 0. saniyesinde)
            if (guncelAyarlar.EzanAktif && suankiIdx >= 0 && !ezanCalindi[suankiIdx])
            {
                TimeSpan gecen = simdi - vakitler[suankiIdx];
                if (gecen.TotalSeconds >= 0 && gecen.TotalSeconds < 3)
                {
                    ezanCalindi[suankiIdx] = true;
                    EzanCal();
                }
            }

            TimeSpan diff = vakitler[siradakiIdx] - simdi;

            if (guncelAyarlar.GosterimModu == 0)
            {
                lblVakit.Text = vakitIsimleri[suankiIdx];
            }
            else
            {
                lblVakit.Text = vakitIsimleri[siradakiIdx];
            }

            lblSure.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", diff.Hours, diff.Minutes, diff.Seconds);
            DinamikHizala();
        }

        private void DosyalariYukle()
        {
            try
            {
                if (File.Exists(ayarlarYolu))
                {
                    guncelAyarlar = JsonSerializer.Deserialize<AyarlarData>(File.ReadAllText(ayarlarYolu)) ?? new AyarlarData();
                    this.BackColor = Color.FromArgb(guncelAyarlar.ArkaPlan); _sinirRengi = Color.FromArgb(guncelAyarlar.Cerceve);
                    lblVakit.ForeColor = Color.FromArgb(guncelAyarlar.Vakit); lblSure.ForeColor = Color.FromArgb(guncelAyarlar.Sure);
                    fontVakitSize = guncelAyarlar.VakitBoyutu; fontSureSize = guncelAyarlar.SureBoyutu;
                    this.TopMost = guncelAyarlar.HerZamanUste;
                    if (guncelAyarlar.KonumX != -1) { this.Location = new Point(guncelAyarlar.KonumX, guncelAyarlar.KonumY); ilkKonumAyarlandi = true; }
                }

                if (File.Exists(vakitlerYolu))
                {
                    guncelVakitler = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(vakitlerYolu)) ?? new Dictionary<string, string[]>();
                }
            }
            catch { }
        }

        public void VakitleriKaydet()
        {
            try
            {
                string dir = Path.GetDirectoryName(vakitlerYolu);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(vakitlerYolu, JsonSerializer.Serialize(guncelVakitler, options));
            }
            catch { }
        }

        public void AyarlariKaydet()
        {
            try
            {
                guncelAyarlar.ArkaPlan = this.BackColor.ToArgb();
                guncelAyarlar.Cerceve = this.SinirRengi.ToArgb();
                guncelAyarlar.Vakit = this.VakitRengi.ToArgb();
                guncelAyarlar.Sure = this.SureRengi.ToArgb();
                guncelAyarlar.VakitBoyutu = fontVakitSize;
                guncelAyarlar.SureBoyutu = fontSureSize;
                guncelAyarlar.KonumX = this.Location.X;
                guncelAyarlar.KonumY = this.Location.Y;

                string dir = Path.GetDirectoryName(ayarlarYolu);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
                File.WriteAllText(ayarlarYolu, JsonSerializer.Serialize(guncelAyarlar));
            }
            catch { }
        }

        private void DinamikHizala()
        {
            int targetWidth = Math.Max(lblVakit.Width, lblSure.Width) + 60;
            int targetHeight = lblSure.Bottom + 18;

            lblVakit.Location = new Point((targetWidth - lblVakit.Width) / 2, 12);
            lblSure.Location = new Point((targetWidth - lblSure.Width) / 2, lblVakit.Bottom - 5);

            // YENİ: Sadece formun dış çerçevesi büyüyüp küçülürse Invalidate() çağırılır. 
            // Saniye akarken boyut değişmiyorsa arka plan baştan çizilmez, bozulmalar yok olur.
            if (this.Width != targetWidth || this.Height != targetHeight)
            {
                this.Size = new Size(targetWidth, targetHeight);
                this.Invalidate();
            }

            if (!ilkKonumAyarlandi)
            {
                this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width - 20, 20);
                ilkKonumAyarlandi = true;
            }
        }

        private void DinamikHizalaSoru()
        {
            int w = Math.Max(260, Math.Max(lblVakit.Width, lblSure.Width) + 60);
            lblVakit.Location = new Point((w - lblVakit.Width) / 2, 15);
            lblSure.Location = new Point((w - lblSure.Width) / 2, lblVakit.Bottom + 5);
            btnOnayla.Location = new Point((w - 205) / 2, lblSure.Bottom + 15);
            btnReddet.Location = new Point(btnOnayla.Right + 15, btnOnayla.Top);

            int targetHeight = btnOnayla.Bottom + 20;

            if (this.Width != w || this.Height != targetHeight)
            {
                this.Size = new Size(w, targetHeight);
                this.Invalidate();
            }
        }

        private void FareTekerlegiIleOlcekle(object sender, MouseEventArgs e)
        {
            if (soruEkraniAktif) return;
            if (e.Delta > 0 && fontVakitSize < 70f) { fontVakitSize += 2f; fontSureSize += 2f; }
            else if (e.Delta < 0 && fontVakitSize > 12f) { fontVakitSize -= 2f; fontSureSize -= 2f; }
            lblVakit.Font = new Font("Segoe UI", fontVakitSize, FontStyle.Bold);
            lblSure.Font = new Font("Segoe UI", fontSureSize, FontStyle.Bold);

            // Fareyle büyütüp küçültürken formun boyutları zorla değiştiği için burada manuel tetikliyoruz.
            int w = Math.Max(lblVakit.Width, lblSure.Width) + 60;
            this.Size = new Size(w, lblSure.Bottom + 18);
            this.Invalidate();
            DinamikHizala();
        }

        private void EzanCal()
        {
            try
            {
                string ezanYolu = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                    "ezan.mp3");

                if (!File.Exists(ezanYolu)) return;

                // Önceki çalmayı durdur ve kapat
                mciSendString("close ezan", null, 0, IntPtr.Zero);
                // Yeni dosyayı aç
                mciSendString($"open \"{ezanYolu}\" type mpegvideo alias ezan", null, 0, IntPtr.Zero);
                // Çal
                mciSendString("play ezan", null, 0, IntPtr.Zero);
            }
            catch { }
        }

        public void EzanDurdur()
        {
            try { mciSendString("stop ezan", null, 0, IntPtr.Zero); mciSendString("close ezan", null, 0, IntPtr.Zero); } catch { }
        }

        public void EzanDosyasiAyarla(string kaynakYol)
        {
            try
            {
                string hedefYol = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                    "ezan.mp3");
                File.Copy(kaynakYol, hedefYol, overwrite: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya kopyalanamıyor: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr h, int m, int w, int l);
        private void FormMouseOlaylari(object sender, MouseEventArgs e)
        {
            this.Focus(); if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
            else if (e.Button == MouseButtons.Right) { new AyarlarForm(this).Show(); }
        }
    }

    public class AyarlarData
    {
        public int ArkaPlan { get; set; } = Color.FromArgb(35, 35, 35).ToArgb();
        public int Cerceve { get; set; } = Color.DodgerBlue.ToArgb();
        public int Vakit { get; set; } = Color.Cyan.ToArgb();
        public int Sure { get; set; } = Color.White.ToArgb();
        public float VakitBoyutu { get; set; } = 28f;
        public float SureBoyutu { get; set; } = 24f;
        public int KonumX { get; set; } = -1;
        public int KonumY { get; set; } = -1;
        public string SonSehir { get; set; } = "";
        public DateTime SonGuncelleme { get; set; } = DateTime.MinValue;
        public int GosterimModu { get; set; } = 0;
        public bool EzanAktif { get; set; } = true;
        public bool HerZamanUste { get; set; } = true;
    }
}