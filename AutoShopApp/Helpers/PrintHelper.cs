using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Printing;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.System;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections; // Add this for IPropertySet
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using System.Runtime.Versioning;

namespace MMN.App.Helpers
{
    [SupportedOSPlatform("windows10.0.10240.0")]
    public class PrintHelper
    {
        private PrintManager printManager;
        private IPrintDocumentSource printDocumentSource;
        private PrintDocument printDocument;
        private IPropertySet printSettings;

        public async Task Print(Window window, FrameworkElement elementToPrint, string documentTitle)
        {
            try
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
                {
                    throw new PlatformNotSupportedException("PrintDocument is not supported on this platform.");
                }

                if (window == null)
                {
                    throw new ArgumentNullException(nameof(window), "Window cannot be null.");
                }

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                printManager = PrintManagerInterop.GetForWindow(hWnd);

                printManager.PrintTaskRequested += PrintManager_PrintTaskRequested;

                // Create the PrintDocument
                printDocument = new PrintDocument();
                printDocumentSource = printDocument.DocumentSource;
                printDocument.Paginate += PrintDocument_Paginate;
                printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;
                printDocument.AddPages += PrintDocument_AddPages;

                // Store content to print and document title
                ContentToPrint = elementToPrint;
                DocumentTitle = documentTitle;

                await PrintManagerInterop.ShowPrintUIForWindowAsync(hWnd);
            }
            finally
            {
                if (printManager != null)
                {
                    printManager.PrintTaskRequested -= PrintManager_PrintTaskRequested;
                }
            }
        }

        private FrameworkElement ContentToPrint { get; set; }
        private string DocumentTitle { get; set; }

        private void PrintManager_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var printTask = args.Request.CreatePrintTask(DocumentTitle, sourceRequested =>
            {
                sourceRequested.SetSource(printDocumentSource);
            });

            printTask.Completed += PrintTask_Completed;
        }

        private void PrintDocument_Paginate(object sender, PaginateEventArgs e)
        {
            // For simplicity, we'll just create a single page
            printDocument.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        private void PrintDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // Add the preview page
            printDocument.SetPreviewPage(e.PageNumber, ContentToPrint);
        }

        private void PrintDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            // Add the print page
            printDocument.AddPage(ContentToPrint);
            printDocument.AddPagesComplete();
        }

        private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            if (printDocument != null)
            {
                try
                {
                    printDocument.Paginate -= PrintDocument_Paginate;
                    printDocument.GetPreviewPage -= PrintDocument_GetPreviewPage;
                    printDocument.AddPages -= PrintDocument_AddPages;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error: PrintTask_Completed: {0}", ex.Message));
                
                }
            }
        }
    }
}