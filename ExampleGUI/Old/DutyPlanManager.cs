//using ExampleGUI.SQL;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;



//namespace ExampleGUI.Classes
//{
//    internal class DutyPlanManager: IDisposable
//    {
//        private readonly OrgaSANItionEntities _context;
//        private readonly int _maxPastDays = 14;
//        private readonly int _maxFutureDays = 28;

//        public DutyPlanManager()
//        {
//            _context = new OrgaSANItionEntities();
//        }

//        /// <summary>
//        /// Wenn der letzte Tag in der DB älter als 14 Tage, ausgehend vom Montag der aktuellen Woche, ist, 
//        /// wird UpdateDutyPlan() aufgerufen
//        /// </summary>
//        public void CheckDutyPlan()
//        {
//            var tageList = _context.Tage.ToList();
//            if (!tageList.Any())
//            {
//                AddDaysToDB();
//                ServerConsole.WriteLine("Added Days to DB");
//                return;
//            }
//            DateTime minDate = _context.Tage.Min(t => t.Datum);
//            DateTime maxDate = _context.Tage.Max(t => t.Datum);

//            if (minDate < DateTimeExtension.GetMondayOfWeek(DateTime.Now).AddDays(-_maxPastDays))
//            {
//                RemoveDaysFromDB();
//                ServerConsole.WriteLine("Removed Days from DB");
//            }
//            if (maxDate < DateTimeExtension.GetMondayOfWeek(DateTime.Now).AddDays(_maxFutureDays-2))
//            {
//                AddDaysToDB();
//                ServerConsole.WriteLine("Added Days to DB");
//            }

//            return;
//        }

//        /// <summary>
//        /// Entferne Tage aus der DB, die älter als die angegebene Anzahl an '_maxPastDays',
//        /// ausgehend vom Montag der aktuellen Woche, sind.
//        /// </summary>
//        private void RemoveDaysFromDB()
//        {
//            DateTime mondayTwoWeeksAgo = DateTimeExtension.GetMondayOfWeek(DateTime.Now).AddDays(_maxPastDays);
//            var oldTage = _context.Tage.Where(t => t.Datum < mondayTwoWeeksAgo);
//            _context.Tage.RemoveRange(oldTage);
//            _context.SaveChanges();
//        }
        
//        private void AddDaysToDB()
//        {
//            //Herausfinden, wie viele Dienstpläne hinzugefügt werden müssen und das Datum des Tages, der als erstes hinzugefügt wird
//            int dutyPlansToAdd;
//            DateTime dateToAdd;
//            //Letzten Tag aus DB
//            var lastDayInDB = _context.Tage.OrderByDescending(t => t.Datum).FirstOrDefault();
//            if (lastDayInDB != null)
//            {
//                DateTime lastDayDate = lastDayInDB.Datum;
//                //Anzahl Tage zwischen Montag der aktuellen Woche und letztem Tag aus der DB
//                int difference = DateTimeExtension.GetDaysBetweenDates(DateTimeExtension.GetMondayOfWeek(DateTime.Now), lastDayDate);
//                //2 Tage zu difference hinzufügen, da der letzte Tag in der DB immer ein Freitag ist
//                difference += 2;
//                dutyPlansToAdd = _maxFutureDays - difference;
//                dateToAdd = lastDayDate.AddDays(3);
//            }
//            else
//            {
//                dutyPlansToAdd = _maxFutureDays;
//                dateToAdd = DateTimeExtension.GetMondayOfWeek(DateTime.Now);
//            }
//            //dutyPlansToAdd beinhaltet die Anzahl der Tage, die hinzugefügt werden müssen. Durch Dividieren mit 7 erhält man
//            //die Anzahl der Wochenpläne.
//            dutyPlansToAdd = dutyPlansToAdd / 7;

//            //Erstelle Tage
//            string[] wochentage =
//            {
//        "Montag",
//        "Dienstag",
//        "Mittwoch",
//        "Donnerstag",
//        "Freitag"
//    };
//            int currentMonth = dateToAdd.Month;
//            DBUserManager dbManager = new DBUserManager();

//            for (int i = 0; i < dutyPlansToAdd; i++)
//            {
//                for (int j = 0; j < 5; j++)
//                {
//                    int[] IDsSanis = GetSanisForDay(wochentage[j]);
//                    int[] IDsSpringer = GetSpringerForDay(wochentage[j], IDsSanis);

//                    Debug.WriteLine($"ID 0: {IDsSanis[0]} | ID 1: {IDsSanis[1]} | ID 2: {IDsSpringer[0]} | ID 3: {IDsSpringer[1]}");

//                    _context.Tage.Add(CreateDay(dateToAdd,
//                        IDsSanis[0], IDsSanis[1], IDsSpringer[0], IDsSpringer[1]));
//                    dateToAdd = dateToAdd.AddDays(1);
//                }
//                dateToAdd = dateToAdd.AddDays(2);
//                dbManager.ResetAnzahlDienste();
//                if (currentMonth != dateToAdd.Month)
//                {
//                    dbManager.ResetScores();
//                    currentMonth++;
//                }
//            }
//            _context.SaveChanges();
//        }

//        private Tage CreateDay(DateTime datum, int sani1ID, int? sani2ID, int springer1ID, int? springer2ID)
//        {
//            Tage tag = new Tage
//            {
//                Datum = datum,
//                Sani1 = sani1ID == 0 ? null : sani1ID,
//                Sani2 = sani2ID,
//                Springer1 = springer1ID,
//                Springer2 = springer2ID
//            };

//            return tag;
//        }


//        private static int[] GetSanisForDay(string wochentag)
//        {
//            int[] IDs = new int[2];
//            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
//            {
//                var query = context.Benutzer.SqlQuery("SELECT TOP (2) * " +
//                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
//                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 AND SaniScore = " +
//                    "(SELECT MIN(SaniScore) " +
//                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
//                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3) " +
//                    "ORDER BY NEWID()");
//                if (query.Count() == 1)
//                {
//                    Benutzer favPartner = new Benutzer();
//                    foreach (Benutzer b in query)
//                    {
//                        IDs[0] = b.B_ID;
//                        b.AnzahlDienste++;
//                        b.SaniScore += 50;
//                        favPartner = GetFavPartnerOfUser(b);
//                    }
//                    if (favPartner != null && favPartner.AnzahlDienste < 3)
//                    {
//                        IDs[1] = favPartner.B_ID;
//                        favPartner.AnzahlDienste++;
//                        favPartner.SaniScore += 50;
//                        goto done;
//                    }
//                    var query2 = context.Benutzer.SqlQuery("SELECT TOP (1) * " +
//                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
//                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 AND B_ID != " + IDs[0] + " AND SaniScore = " +
//                    "(SELECT MIN(SaniScore) " +
//                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
//                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 AND B_ID != " + IDs[0] + ")" +
//                    " ORDER BY NEWID()");
//                    foreach (Benutzer b in query2)
//                    {
//                        IDs[1] = b.B_ID;
//                        b.AnzahlDienste++;
//                        b.SaniScore += 50;
//                    }
//                }
//                else
//                {
//                    Benutzer favPartner = new Benutzer();
//                    int zähler = 0;
//                    foreach (Benutzer b in query)
//                    {
//                        IDs[zähler] = b.B_ID;
//                        b.AnzahlDienste++;
//                        b.SaniScore += 50;
//                        zähler++;
//                        if (b != null)
//                        {
//                            favPartner = GetFavPartnerOfUser(b);
//                            if (favPartner != null && favPartner.AnzahlDienste < 3)
//                            {
//                                IDs[1] = favPartner.B_ID;
//                                favPartner.AnzahlDienste++;
//                                favPartner.SaniScore += 50;
//                                break;
//                            }
//                        }
//                    }
//                }
//            done:
//                context.SaveChanges();
//            }
//            return IDs;
//        }

//        private static int[] GetSpringerForDay(string wochentag, int[] sanis)
//        {
//            int[] IDs = new int[2];
//            string B_ID1 = "";
//            string B_ID2 = "";
//            if (sanis[0] != 0)
//            {
//                B_ID1 = "AND B_ID != " + sanis[0];
//                if (sanis[1] != 0)
//                {
//                    B_ID2 = " AND B_ID != " + sanis[1];
//                }
//            }
//            string queryString = "SELECT TOP (2) * " +
//                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
//                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " + B_ID1 + B_ID2 + " AND SpringerScore = " +
//                    "(SELECT MIN(SpringerScore) " +
//                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
//                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " + B_ID1 + B_ID2 + ")";
//            queryString += " ORDER BY NEWID()";
//            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
//            {
//                var query = context.Benutzer.SqlQuery(queryString);
//                if (query.Count() == 1)
//                {
//                    Benutzer favPartner = new Benutzer();
//                    foreach (Benutzer b in query)
//                    {
//                        IDs[0] = b.B_ID;
//                        b.AnzahlDienste++;
//                        b.SpringerScore += 50;
//                    }
//                    if (favPartner != null && favPartner.AnzahlDienste < 3)
//                    {
//                        IDs[1] = favPartner.B_ID;
//                        favPartner.AnzahlDienste++;
//                        favPartner.SaniScore += 50;
//                        goto done;
//                    }
//                    var query2 = context.Benutzer.SqlQuery("SELECT TOP (1) * " +
//                        "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
//                        "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " + B_ID1 + B_ID2 + " AND B_ID != " + IDs[0] + " AND SpringerScore = " +
//                        "(SELECT MIN(SpringerScore) " +
//                        "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
//                        "WHERE SpringerScore > (SELECT MIN(SpringerScore) FROM Benutzer WHERE B_ID != " + IDs[0] + " " + B_ID1 + B_ID2 + ") AND Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " +
//                        B_ID1 + B_ID2 + " AND B_ID != " + IDs[0] + ")" +
//                        " ORDER BY NEWID()");
//                    foreach (Benutzer b in query2)
//                    {
//                        IDs[1] = b.B_ID;
//                        b.AnzahlDienste++;
//                        b.SpringerScore += 50;
//                    }
//                }
//                else
//                {
//                    Benutzer favPartner = new Benutzer();
//                    int zähler = 0;
//                    foreach (Benutzer b in query)
//                    {
//                        IDs[zähler] = b.B_ID;
//                        b.AnzahlDienste++;
//                        b.SpringerScore += 50;
//                        zähler++;
//                        if (b != null)
//                        {
//                            favPartner = GetFavPartnerOfUser(b);
//                            if (favPartner != null && favPartner.AnzahlDienste < 3)
//                            {
//                                IDs[1] = favPartner.B_ID;
//                                favPartner.AnzahlDienste++;
//                                favPartner.SpringerScore += 50;
//                                break;
//                            }
//                        }
//                    }
//                }
//            done:
//                context.SaveChanges();
//            }
//            return IDs;
//        }

        
//        private static Benutzer GetFavPartnerOfUser(Benutzer benutzer)
//        {
//            Benutzer favPartner = null;
//            OrgaSANItionEntities context = new OrgaSANItionEntities();
//            foreach (Benutzer b in context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE B_ID = {0}", benutzer.FavPartner))
//            {
//                favPartner = b;
//            }

//            return favPartner;
//        }

//        public void Dispose()
//        {
//            _context.Dispose();
//        }
//    }




//    /*
//        private int[] GetUsersForDay(string wochentag, bool isSanitaeter = true)
//        {
//            var users = GetAvailableUsersForDay(wochentag, isSanitaeter);
//            if (users.Count < 1)
//            {
//                return new int[] { 0, 0 };
//            }

//            int[] result = new int[2];
//            result[0] = users.First().B_ID;
//            if (users.Count > 1)
//            {
//                result[1] = users.Skip(1).First().B_ID;
//            }

//            var usersToUpdate = _context.Benutzer.Where(b => result.Contains(b.B_ID)).ToList();
//            foreach (var user in usersToUpdate)
//            {
//                user.AnzahlDienste++;
//                user.SaniScore += 50;
//            }

//            return result;
//        }

//        private List<Benutzer> GetAvailableUsersForDay(string wochentag, bool isSanitaeter = true)
//        {
//            DateTime currentWeekStart = DateTimeExtension.GetMondayOfWeek(DateTime.Now);
//            DateTime currentWeekEnd = currentWeekStart.AddDays(4);

//            var query = from b in _context.Benutzer
//                        join s in _context.Sanitage on b.Sanitage_ID equals s.Sanitage_ID
//                        join sp in _context.Sanitage on b.Springertage_ID equals sp.Sanitage_ID
//                        where (isSanitaeter ? wochentag : s.SpringerTage[wochentag]) == true
//                        && b.AnzahlDienste < 3
//                        && (b.LetzterDienst == null || b.LetzterDienst < currentWeekStart || b.LetzterDienst > currentWeekEnd)
//                        orderby b.SaniScore, Guid.NewGuid()
//                        select b;

//            return query.ToList();
//        }
//        */
//}
