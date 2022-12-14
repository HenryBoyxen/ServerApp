using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ExampleGUI.SQL;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Data.Entity.Migrations;
using ExampleGUI.Classes;

namespace ExampleGUI
{
    public static class ServerLogik
    {
        private static Server _serverPage;
        private static readonly TcpListener _listener = new TcpListener(System.Net.IPAddress.Any, 32332);

        public static void StartListener(Server pServerPage, CancellationToken token)
        {
            _serverPage = pServerPage;
            _listener.AllowNatTraversal(true);
            _listener.Start();

            while(!token.IsCancellationRequested)
            {
                if (_listener.Pending())
                {
                    _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "Connection found\n" + _serverPage.txt_server.Text);
                    Task.Run(() => AcceptTcpClient());
                }
            }

            _listener.Stop();
            _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "Server stopped!\n" + _serverPage.txt_server.Text);
            _serverPage.Dispatcher.Invoke(() => _serverPage.btn_server_start.IsEnabled = true);
        }

        public static void AcceptTcpClient()
        {
            try
            {
                using (TcpClient client = _listener.AcceptTcpClient())
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter sw = new StreamWriter(client.GetStream());
                    sw.AutoFlush = true;

                    string request = ReadStreamString(stream);
                    _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "Request received: " + request + "\n" + _serverPage.txt_server.Text);

                    HandleRequest(request, stream, sw);

                    _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "The request was successfully processed, closing connection\n" + _serverPage.txt_server.Text);

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "Something went wrong (catch AcceptTcpClient)\n" + _serverPage.txt_server.Text);
            }
        }

        private static string ReadStreamString(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            stream.Read(buffer, 0, buffer.Length);
            int recv = 0;
            foreach (byte b in buffer)
            {
                if (b != 0)
                {
                    recv++;
                }
            }
            return Encoding.UTF8.GetString(buffer, 0, recv);
        }
        private static string ReadStreamWithApproval(NetworkStream stream, StreamWriter sw)
        {
            try
            {
                sw.Write("Continue");
                string answer = ReadStreamString(stream);
                return answer;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        private static void HandleRequest(string request, NetworkStream stream, StreamWriter sw)
        {
            switch (request)
            {
                case "Anmelden_anmelden":
                    Request_anmelden_anmelden(stream, sw);
                    break;
                case "Registrieren_initializeSchulenliste":
                    Request_registrieren_initializeSchulenliste(stream, sw);
                    break;
                case "Registrieren_registrieren":
                    Request_registrieren_registrieren(stream, sw);
                    break;
                case "Registrieren_Check_benutzername":
                    Request_registrieren_Check_benutzername(stream, sw);
                    break;
                case "GetSanisAndSpringerFromDate":
                    Request_GetSanisAndSpringerFromDate(stream, sw);
                    break;
                case "HomePage_initializeNextDuty":
                    Request_homepage_initializeNextDuty(stream, sw);
                    break;
                case "Calender_initializeAllDuties":
                    Request_calender_initializeAllDuties(stream, sw);
                    break;
                case "Eintragung_initializeCheckboxes":
                    Request_eintragung_initializeCheckboxes(stream, sw);
                    break;
                case "Eintragung_eintragung":
                    Request_eintragung_eintragung(stream, sw);
                    break;
                case "Orga_EintragungDienstplan":
                    //Request_orga_eintragungDienstplan(stream, sw);
                    break;
                default:
                    break;
            }
        }

        #region intern methods to simplify request methods

        private static int? UsernameToUID(string username)
        {
            int? userID = null;
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE Benutzername = {0}", username);
                foreach (Benutzer b in query)
                {
                    userID = b.B_ID;
                }
            }
            return userID;
        }

        private static int FindIDOfSanitage(string value1, string value2, string value3, string value4, string value5)
        {
            int ID = 0;

            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Sanitage.SqlQuery("SELECT * FROM Sanitage WHERE Montag = {0} AND " +
                    "Dienstag = {1} AND Mittwoch = {2} AND Donnerstag = {3} AND Freitag = {4}",
                    value1, value2, value3, value4, value5);
                foreach (Sanitage sa in query)
                {
                    ID = sa.Sanitage_ID;
                }
            }
            return ID;
        }

        private static string[] GetValuesOfSanitage(int SanitageID)
        {
            string[] values = new string[5];
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Sanitage.SqlQuery("SELECT * FROM Sanitage WHERE Sanitage_ID = {0}",SanitageID);
                foreach (Sanitage sa in query)
                {
                    values[0] = sa.Montag;
                    values[1] = sa.Dienstag;
                    values[2] = sa.Mittwoch;
                    values[3] = sa.Donnerstag;
                    values[4] = sa.Freitag;
                }
            }
            return values;
        }

        #endregion


        #region Reqeuest methods
        private static void Request_anmelden_anmelden(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            //set benutzername from client
            _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "Trying to read benutzername\n" + _serverPage.txt_server.Text);
            string benutzername = ReadStreamWithApproval(pStream, pStreamWriter);
            if(benutzername == null)
            {
                _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "Error reading benutzername\n" + _serverPage.txt_server.Text);
                return;
            }

            string sqlPassword = null;
            
            //set password from SQL-Database
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE benutzername = {0}", benutzername);
                foreach (Benutzer b in query)
                {
                    sqlPassword = b.Passwort;
                }
            }
            _serverPage.Dispatcher.Invoke(() => _serverPage.txt_server.Text = "Sending answer...\n" + _serverPage.txt_server.Text);
            DutyPlan.CheckDutyPlan();
            pStreamWriter.Write(sqlPassword);
        }

        private static void Request_registrieren_initializeSchulenliste(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            pStreamWriter.Write("continue");
            Queue<string> schulnamenListe = new Queue<string>();
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Schulen.SqlQuery("SELECT * FROM Schulen");
                foreach (Schulen schulen in query)
                {
                    schulnamenListe.Enqueue(schulen.Name);
                }
            }
            ReadStreamString(pStream);
            while(schulnamenListe.Count > 0)
            {
                pStreamWriter.Write(schulnamenListe.Peek());
                schulnamenListe.Dequeue();
                ReadStreamString(pStream);
            }
            pStreamWriter.Write("end");
        }

        private static void Request_registrieren_Check_benutzername(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            pStreamWriter.Write("continue");
            string benutzername = ReadStreamString(pStream);
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Benutzer.SqlQuery("SELECT * FROM Benutzer");
                foreach (Benutzer benutzer in query)
                {
                    if(benutzer.Benutzername == benutzername)
                    {
                        pStreamWriter.Write("false");
                        return;
                    }
                }
                pStreamWriter.Write("true");
            }
        }

        private static void Request_registrieren_registrieren(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            //Get Data from new User
            Benutzer neuerBenutzer = new Benutzer();
            neuerBenutzer.Vorname = ReadStreamWithApproval(pStream, pStreamWriter);
            neuerBenutzer.Nachname = ReadStreamWithApproval(pStream, pStreamWriter);
            neuerBenutzer.E_Mail = ReadStreamWithApproval(pStream, pStreamWriter);
            string schulname = ReadStreamWithApproval(pStream, pStreamWriter);
            neuerBenutzer.Benutzername = ReadStreamWithApproval(pStream, pStreamWriter);
            neuerBenutzer.Passwort = ReadStreamWithApproval(pStream, pStreamWriter);

            //Get Schul_ID
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Schulen.SqlQuery("SELECT * FROM Schulen WHERE Name = {0}", schulname);
                int schul_ID = -1;
                foreach (Schulen s in query)
                {
                    schul_ID = s.S_ID;
                }
                neuerBenutzer.Schul_ID = schul_ID;

                //Add new User to Databank
                context.Benutzer.Add(neuerBenutzer);
                context.SaveChanges();
            }
        }

        private static void Request_GetSanisAndSpringerFromDate(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            string date = ReadStreamWithApproval(pStream, pStreamWriter);

            string sani1 = "null";
            string sani2 = "null";
            string springer1 = "null";
            string springer2 = "null";

            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Tage.SqlQuery("SELECT * FROM Tage WHERE Datum = {0}", date);
                int? B_ID_sani1 = null;
                int? B_ID_sani2 = null; ;
                int? B_ID_springer1 = null; ;
                int? B_ID_springer2 = null; ;
                foreach (Tage t in query)
                {
                    B_ID_sani1 = t.Sani1;
                    B_ID_sani2 = t.Sani2;
                    B_ID_springer1 = t.Springer1;
                    B_ID_springer2 = t.Springer2;
                }

                if(B_ID_sani1 != null)
                {
                    var querySani1 = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE B_ID = {0}", B_ID_sani1);
                    foreach (Benutzer b in querySani1)
                    {
                        sani1 = b.Vorname + " " + b.Nachname;
                    }
                }

                if (B_ID_sani2 != null)
                {
                    var querySani2 = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE B_ID = {0}", B_ID_sani2);
                    foreach (Benutzer b in querySani2)
                    {
                        sani2 = b.Vorname + " " + b.Nachname;
                    }
                }

                if (B_ID_springer1 != null)
                {
                    var querySpringer1 = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE B_ID = {0}", B_ID_springer1);
                    foreach (Benutzer b in querySpringer1)
                    {
                        springer1 = b.Vorname + " " + b.Nachname;
                    }
                }

                if (B_ID_springer2 != null)
                {
                    var querySpringer2 = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE B_ID = {0}", B_ID_springer2);
                    foreach (Benutzer b in querySpringer2)
                    {
                        springer2 = b.Vorname + " " + b.Nachname;
                    }
                }
            }
            //send array
            string[] array = new string[4];
            array[0] = sani1;
            array[1] = sani2;
            array[2] = springer1;
            array[3] = springer2;
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(pStream, array);
        }

        private static void Request_homepage_initializeNextDuty(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            string username = ReadStreamWithApproval(pStream, pStreamWriter);
            int? userID = UsernameToUID(username);
            DateTime? date = null;
            string function = null;
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Tage.SqlQuery("SELECT TOP(1) * FROM Tage WHERE (Datum >= {0}) AND (Sani1 = {1} OR Sani2 = {1} OR Springer1 = {1} OR Springer2 = {1})", DateTime.Now.ToString("yyyy-MM-dd"), userID);
                foreach (Tage t in query)
                {
                    date = t.Datum;
                    if (t.Sani1 == userID || t.Sani2 == userID)
                        function = "Sanitäter";
                    else
                        function = "Springer";
                }
            }
            if (date != null)
            {
                DateTime date2 = (DateTime)date;
                pStreamWriter.Write(date2.ToShortDateString());
                ReadStreamString(pStream);
                pStreamWriter.Write(function);

            }
            else
            {
                pStreamWriter.Write("null");
                ReadStreamString(pStream);
                pStreamWriter.Write("null");
            }
                
        }

        private static void Request_calender_initializeAllDuties(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            string username = ReadStreamWithApproval(pStream, pStreamWriter);
            int? userID = UsernameToUID(username);
            Queue<string> queue = new Queue<string>();
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Tage.SqlQuery("SELECT * FROM Tage WHERE (Datum >= {0}) AND (Sani1 = {1} OR Sani2 = {1} OR " +
                    "Springer1 = {1} OR Springer2 = {1})",DateTime.Now.ToString("yyyy-MM-dd"), userID);
                foreach (Tage t in query)
                {
                    queue.Enqueue(t.Datum.ToShortDateString());
                    if (t.Sani1 == userID || t.Sani2 == userID)
                        queue.Enqueue("Sanitäter");
                    else
                        queue.Enqueue("Springer");
                }
            }
            //send queue
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(pStream, queue);
        }
    
        private static void Request_eintragung_initializeCheckboxes(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            //Get Username
            string username = ReadStreamWithApproval(pStream, pStreamWriter);
            int? sanitageID = null;
            int? springertageID = null;

            //Get sanitageID
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE Benutzername = {0}", username);
                foreach(Benutzer b in query)
                {
                    sanitageID = b.Sanitage_ID;
                    springertageID = b.Springertage_ID;
                }
            }
            string[] valuesSanitage = new string[5];
            if (sanitageID != null)
            {
                int sanitageIDNotNull = (int)sanitageID;
                valuesSanitage = GetValuesOfSanitage(sanitageIDNotNull);
            }
            else
            {
                valuesSanitage = GetValuesOfSanitage(0);
            }

            string[] valuesSpringertage = new string[5];
            if (springertageID != null)
            {
                int springertageIDNotNull = (int)springertageID;
                valuesSpringertage = GetValuesOfSanitage(springertageIDNotNull);
            }
            else
            {
                valuesSpringertage = GetValuesOfSanitage(0);
            }
            string[] array = valuesSanitage.Concat(valuesSpringertage).ToArray();

            //Send array
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(pStream, array);
        }

        private static void Request_eintragung_eintragung(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            //Get Username
            string username = ReadStreamWithApproval(pStream, pStreamWriter);
            //Send ready
            pStreamWriter.Write("continue");
            //Get array
            IFormatter formatter = new BinaryFormatter();
            string[] array = (string[])formatter.Deserialize(pStream);
            //Find ID of Sanitage
            int sanitageID = FindIDOfSanitage(array[0], array[1], array[2], array[3], array[4]);
            int springertageID = FindIDOfSanitage(array[5], array[6], array[7], array[8], array[9]);

            Benutzer benutzer = new Benutzer();
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query3 = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE Benutzername = {0}", username);
                foreach (Benutzer b in query3)
                {
                    benutzer.B_ID = b.B_ID;
                    benutzer.Vorname = b.Vorname;
                    benutzer.Nachname = b.Nachname;
                    benutzer.E_Mail = b.E_Mail;
                    benutzer.Schul_ID = b.Schul_ID;
                    benutzer.Benutzername = b.Benutzername;
                    benutzer.Passwort = b.Passwort;
                    benutzer.Sanitage_ID = sanitageID;
                    benutzer.Springertage_ID = springertageID;
                    benutzer.SaniScore = b.SaniScore;
                    benutzer.SpringerScore = b.SpringerScore;
                    benutzer.AnzahlDienste = b.AnzahlDienste;
                    benutzer.FavPartner = b.FavPartner;
                }
                context.Benutzer.AddOrUpdate(benutzer);
                context.SaveChanges();
            }
            //Send confirmation
            pStreamWriter.Write("true");
        }
        
        /*
        private static void Request_orga_eintragungDienstplan(NetworkStream pStream, StreamWriter pStreamWriter)
        {
            string benutzername = ReadStreamWithApproval(pStream, pStreamWriter);
            string[] array = new string[10];
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = ReadStreamWithApproval(pStream, pStreamWriter);
            }

            int? sanitage_ID = null;
            int? springertage_ID = null;
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                //ID Sanitage herausfinden
                var query = context.Sanitage.SqlQuery("SELECT * FROM Sanitage WHERE Montag = {0} AND " +
                    "Dienstag = {1} AND Mittwoch = {2} AND Donnerstag = {3} AND Freitag = {4}",
                    array[0], array[1], array[2], array[3], array[4]);
                foreach (Sanitage sa in query)
                {
                    sanitage_ID = sa.Sanitage_ID;
                }

                //ID Springertage herausfinden
                var query2 = context.Springertage.SqlQuery("SELECT * FROM Springertage WHERE Montag = {0} AND " +
                    "Dienstag = {1} AND Mittwoch = {2} AND Donnerstag = {3} AND Freitag = {4}",
                    array[5], array[6], array[7], array[8], array[9]);
                foreach (Springertage sp in query2)
                {
                    springertage_ID = sp.Springertage_ID;
                }

                //Benutzerinformationen herausfinden
                Benutzer benutzer = new Benutzer();
                var query3 = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE Benutzername = {0}", benutzername);
                foreach(Benutzer b in query3)
                {
                    benutzer.B_ID = b.B_ID;
                    benutzer.Vorname = b.Vorname;
                    benutzer.Nachname = b.Nachname;
                    benutzer.E_Mail = b.E_Mail;
                    benutzer.Schul_ID = b.Schul_ID;
                    benutzer.Benutzername = b.Benutzername;
                    benutzer.Passwort = b.Passwort;
                    benutzer.Sanitage_ID = sanitage_ID;
                    benutzer.Springertage_ID = springertage_ID;
                    benutzer.SaniScore = b.SaniScore;
                    benutzer.SpringerScore = b.SpringerScore;
                    benutzer.AnzahlDienste = b.AnzahlDienste;
                    benutzer.FavPartner = b.FavPartner;
                }

                context.Benutzer.AddOrUpdate(benutzer);
                context.SaveChanges();

                pStreamWriter.Write("finished");
            }
        }
    
    */
    }
    #endregion
}
