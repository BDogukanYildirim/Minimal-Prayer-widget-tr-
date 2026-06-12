using System;
using System.Windows.Forms;

namespace Namazpc
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Görsel stilleri etkinleştir
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Form1'i (bizim widget'ı) çalıştır
            Application.Run(new Form1());
        }
    }
}