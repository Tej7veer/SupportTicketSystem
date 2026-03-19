using SupportTicketDesktop.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SupportTicketDesktop;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LoginForm());
    }
}
