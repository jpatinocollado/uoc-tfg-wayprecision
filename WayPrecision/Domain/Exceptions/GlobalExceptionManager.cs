using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Exceptions
{
    public static class GlobalExceptionManager
    {
        private static void HandleException(ControlledException ex, ContentPage page)
        {
            page.DisplayAlert("Error", ex.Message, "OK");
        }

        private static void HandleException(Exception ex, ContentPage page)
        {
            page.DisplayAlert("Error", "Se ha producido un error inesperado. Por favor, inténtelo de nuevo más tarde.", "OK");
            Console.WriteLine($"Se ha producido una excepción no controlada: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Console.WriteLine($"Complete Exception: {ex}");
        }

        public static void HandleException(object exception, ContentPage page)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (exception is ControlledException controlledEx)
                {
                    HandleException(controlledEx, page);
                }
                else if (exception is Exception ex)
                {
                    HandleException(ex, page);
                }
                else
                {
                    Console.WriteLine("Se ha producido una excepción desconocida.");
                }
            });
        }
    }
}