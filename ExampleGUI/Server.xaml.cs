using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ExampleGUI.Classes;

namespace ExampleGUI
{
    /// <summary>
    /// Interaktionslogik für Server.xaml
    /// </summary>
    public partial class Server : Page
    {
        private TcpListener tcpListener;
        private static int port = 32332;
        private static readonly IReadOnlyDictionary<string, Action<NetworkStream>> RequestHandlers = new Dictionary<string, Action<NetworkStream>>
        {
            { "Anmelden_anmelden", RequestManager.Anmelden_anmelden },
            //{ "Registrieren_initializeSchulenliste", RequestManager.Registrieren_initializeSchulenliste },
            //{ "Registrieren_registrieren", RequestManager.Registrieren_registrieren },
            //{ "Registrieren_Check_benutzername", RequestManager.Registrieren_Check_benutzername },
            { "GetSanisAndSpringerFromDate", RequestManager.GetSanisAndSpringerFromDate },
            { "HomePage_initializeNextDuty", RequestManager.Homepage_initializeNextDuty },
            { "Calender_initializeAllDuties", RequestManager.Calender_initializeAllDuties },
            { "Eintragung_initializeCheckboxes", RequestManager.Eintragung_initializeCheckboxes },
            { "Eintragung_eintragung", RequestManager.Eintragung_eintragung },
        };
        private CancellationTokenSource _tokenSource;
        public Server()
        {
            InitializeComponent();
            ServerConsole._Console = txt_server;
        }

        private async void btn_server_start_Click(object sender, RoutedEventArgs e)
        {
            await ServerConsole.WriteLine("Der Server wird gesartet...");
            btn_server_start.IsEnabled = false;
            btn_server_stop.IsEnabled = true;
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            try
            {
                tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
                tcpListener.AllowNatTraversal(true);
                tcpListener.Start();
                await ServerConsole.WriteLine("Der Server wurde erfolgreich gestartet und wartet auf eingehende Anfragen.");

                await WaitAndHandleClient(token);
            }
            catch
            {

            }
            finally
            {
                _tokenSource.Dispose();
            }
            btn_server_start.IsEnabled = true;
        }

        private async void btn_server_stop_Click(object sender, RoutedEventArgs e)
        {
            btn_server_stop.IsEnabled = false;
            await ServerConsole.WriteLine("Der Server wird gestoppt!");
            _tokenSource?.Cancel();
            while(tcpListener != null && tcpListener.Pending())
            {
                await Task.Delay(500);
            }
            tcpListener?.Stop();
            btn_server_start.IsEnabled = true;
        }

        private async Task WaitAndHandleClient(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (tcpListener.Pending())
                    {
                        await ServerConsole.WriteLine("Es wurde ausstehende Verbindung gefunden...");
                        try
                        {
                            using (TcpClient client = await tcpListener.AcceptTcpClientAsync())
                            using (NetworkStream stream = client.GetStream())
                            {
                                if (!token.IsCancellationRequested)
                                {
                                    ClientCommunicator cc = new ClientCommunicator(stream);
                                    string request = cc.ReadString();
                                    await ServerConsole.WriteLine($"Es wurde folgende Anfrage gestellt: {request}");

                                    if (RequestHandlers.TryGetValue(request, out Action<NetworkStream> handler))
                                    {
                                        handler?.Invoke(stream);
                                    }

                                    await ServerConsole.WriteLine("Die Anfrage wurde erfolgreich bearbeitet!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await ServerConsole.WriteLine($"Bei der Anfrage ist etwas schief gelaufen: {ex.Message}");
                        }
                    }
                    await Task.Delay(500, token); // wait for half a second before checking again
                }
            }
            finally
            {
                tcpListener.Stop();
                await ServerConsole.WriteLine("Der Server wurde erfolgreich gestoppt!");
            }
        }
    }
}

