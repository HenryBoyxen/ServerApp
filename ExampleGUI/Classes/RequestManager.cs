using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using ExampleGUI.SQL;
using System.Data.Entity.Migrations;

namespace ExampleGUI.Classes
{
    internal static class RequestManager
    {
        #region internal methods to simplify request methods

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
                var query = context.Sanitage.SqlQuery("SELECT * FROM Sanitage WHERE Sanitage_ID = {0}", SanitageID);
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
        
        public static void Anmelden_anmelden(NetworkStream pStream)
        {
            ClientCommunicator cc = new ClientCommunicator(pStream);
            //set benutzername from client
            cc.WriteString("next");
            string benutzername = cc.ReadString();
            if (benutzername == null)
            {
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
            DBDutyPlanManager dBDutyPlanManager = new DBDutyPlanManager();
            dBDutyPlanManager.CheckDutyPlan();
            dBDutyPlanManager.Dispose();
            cc.WriteString(sqlPassword);
        }

        //Registrieren-Methoden sind noch nicht in der Handy-App nicht implementiert

        //public static void Registrieren_initializeSchulenliste(NetworkStream pStream, StreamWriter pStreamWriter)
        //{
        //    ClientCommunicator cc = new ClientCommunicator(pStream);
        //    cc.WriteString("continue");
        //    Queue<string> schulnamenListe = new Queue<string>();
        //    using (OrgaSANItionEntities context = new OrgaSANItionEntities())
        //    {
        //        var query = context.Schulen.SqlQuery("SELECT * FROM Schulen");
        //        foreach (Schulen schulen in query)
        //        {
        //            schulnamenListe.Enqueue(schulen.Name);
        //        }
        //    }
        //    ReadStreamString(pStream);
        //    while (schulnamenListe.Count > 0)
        //    {
        //        pStreamWriter.Write(schulnamenListe.Peek());
        //        schulnamenListe.Dequeue();
        //        ReadStreamString(pStream);
        //    }
        //    pStreamWriter.Write("end");
        //}

        //public static void Registrieren_Check_benutzername(NetworkStream pStream, StreamWriter pStreamWriter)
        //{
        //    pStreamWriter.Write("continue");
        //    string benutzername = ReadStreamString(pStream);
        //    using (OrgaSANItionEntities context = new OrgaSANItionEntities())
        //    {
        //        var query = context.Benutzer.SqlQuery("SELECT * FROM Benutzer");
        //        foreach (Benutzer benutzer in query)
        //        {
        //            if (benutzer.Benutzername == benutzername)
        //            {
        //                pStreamWriter.Write("false");
        //                return;
        //            }
        //        }
        //        pStreamWriter.Write("true");
        //    }
        //}

        //public static void Registrieren_registrieren(NetworkStream pStream, StreamWriter pStreamWriter)
        //{
        //    //Get Data from new User
        //    Benutzer neuerBenutzer = new Benutzer();
        //    neuerBenutzer.Vorname = ReadStreamWithApproval(pStream, pStreamWriter);
        //    neuerBenutzer.Nachname = ReadStreamWithApproval(pStream, pStreamWriter);
        //    neuerBenutzer.E_Mail = ReadStreamWithApproval(pStream, pStreamWriter);
        //    string schulname = ReadStreamWithApproval(pStream, pStreamWriter);
        //    neuerBenutzer.Benutzername = ReadStreamWithApproval(pStream, pStreamWriter);
        //    neuerBenutzer.Passwort = ReadStreamWithApproval(pStream, pStreamWriter);

        //    //Get Schul_ID
        //    using (OrgaSANItionEntities context = new OrgaSANItionEntities())
        //    {
        //        var query = context.Schulen.SqlQuery("SELECT * FROM Schulen WHERE Name = {0}", schulname);
        //        int schul_ID = -1;
        //        foreach (Schulen s in query)
        //        {
        //            schul_ID = s.S_ID;
        //        }
        //        neuerBenutzer.Schul_ID = schul_ID;

        //        //Add new User to Databank
        //        context.Benutzer.Add(neuerBenutzer);
        //        context.SaveChanges();
        //    }
        //}

        public static void GetSanisAndSpringerFromDate(NetworkStream pStream)
        {
            ClientCommunicator cc = new ClientCommunicator(pStream);
            cc.WriteString("next");
            string date = cc.ReadString();

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

                if (B_ID_sani1 != null)
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
            cc.WriteObject(array);
        }

        public static void Homepage_initializeNextDuty(NetworkStream pStream)
        {
            ClientCommunicator cc = new ClientCommunicator(pStream);
            cc.WriteString("next");
            string username = cc.ReadString();
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
                cc.WriteString(date2.ToShortDateString());
                cc.ReadString();
                cc.WriteString(function);
            }
            else
            {
                cc.WriteString("null");
                cc.ReadString();
                cc.WriteString("null");
            }

        }

        public static void Calender_initializeAllDuties(NetworkStream pStream)
        {
            ClientCommunicator cc = new ClientCommunicator(pStream);
            cc.WriteString("next");
            string username = cc.ReadString();
            int? userID = UsernameToUID(username);
            Queue<string> queue = new Queue<string>();
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Tage.SqlQuery("SELECT * FROM Tage WHERE (Datum >= {0}) AND (Sani1 = {1} OR Sani2 = {1} OR " +
                    "Springer1 = {1} OR Springer2 = {1})", DateTime.Now.ToString("yyyy-MM-dd"), userID);
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
            cc.WriteObject(queue);
        }

        public static void Eintragung_initializeCheckboxes(NetworkStream pStream)
        {
            ClientCommunicator cc = new ClientCommunicator(pStream);
            //Get Username
            cc.WriteString("next");
            string username = cc.ReadString();
            int? sanitageID = null;
            int? springertageID = null;

            //Get sanitageID
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE Benutzername = {0}", username);
                foreach (Benutzer b in query)
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
            cc.WriteObject(array);
        }

        public static void Eintragung_eintragung(NetworkStream pStream)
        {
            ClientCommunicator cc = new ClientCommunicator(pStream);
            //Get Username
            cc.WriteString("next");
            string username = cc.ReadString();
            //Send ready
            cc.WriteString("continue");
            //Get array
            string[] array = (string[])cc.ReadObject();
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
            cc.WriteString("true");
        }
    }
}
