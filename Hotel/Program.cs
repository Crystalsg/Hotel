using Hotel.Storage;   
using Hotel.UI;        
using Hotel.Services;  

namespace Hotel;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DbInitializer.Initialize();   
        Application.Run(new LoginWindow());
    }
}