using ExampleGUI.SQL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleGUI.Classes
{
    public static class DutyPlan
    {
        #region methods to simplify
        private static DateTime GetMondayofWeek(DateTime date)
        {
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(-1);
            }
            return date;
        }

        private static int GetDifferenceBetweenDays(DateTime lowerDate, DateTime higherDate)
        {
            int difference = 0;

            while(lowerDate.Date < higherDate)
            {
                lowerDate = lowerDate.AddDays(1);
                difference++;
            }

            return difference;
        }

        private static DateTime? GetLastDayInDatabase()
        {
            DateTime? lastDay = null;
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Tage.SqlQuery("SELECT * FROM Tage WHERE Datum = (SELECT MAX(Datum) FROM Tage)");
                foreach (Tage t in query)
                {
                    lastDay = t.Datum;
                }
            }
            return lastDay;
        }

        private static int GetNumberOfDutyPlansToAdd()
        {
            //Get last day
            DateTime? lastDay = GetLastDayInDatabase();
            //Add days up to 3 weeks into the future
            int difference = 0;
            if (lastDay != null)
            {
                DateTime lastDayNotNull = (DateTime)lastDay;
                DateTime dateTimeNow = GetMondayofWeek(DateTime.Now);
                lastDayNotNull = GetMondayofWeek(lastDayNotNull);
                if (dateTimeNow > lastDayNotNull)
                {
                    difference = GetDifferenceBetweenDays(lastDayNotNull, dateTimeNow);
                    difference += 4;
                }
                else
                    difference = GetDifferenceBetweenDays(dateTimeNow, lastDayNotNull);
            }
            else
                difference = 6;
            return difference;
        }

        private static Benutzer GetFavPartnerOfUser(Benutzer benutzer)
        {
            Benutzer favPartner = null;
            OrgaSANItionEntities context = new OrgaSANItionEntities();
            foreach (Benutzer b in context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE B_ID = {0}", benutzer.FavPartner))
            {
                favPartner = b;
            }

            return favPartner;
        }

        private static void ResetAnzahlDienste()
        {
            OrgaSANItionEntities context = new OrgaSANItionEntities();
            foreach (Benutzer b in context.Benutzer.SqlQuery("SELECT * FROM Benutzer"))
            {
                b.AnzahlDienste = 0;
            }
            context.SaveChanges();
        }

        private static void ResetScores()
        {
            OrgaSANItionEntities context = new OrgaSANItionEntities();
            foreach (Benutzer b in context.Benutzer.SqlQuery("SELECT * FROM Benutzer"))
            {
                b.SaniScore = 0;
                b.SpringerScore = 0;
            }
            context.SaveChanges();
        }

        #endregion

        public static void CheckDutyPlan()
        {
            DateTime minDate = DateTime.Now;
            //Get MIN date from database
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Tage.SqlQuery("SELECT * FROM Tage WHERE Datum = (SELECT MIN(Datum) FROM Tage)");
                foreach (Tage t in query)
                {
                    minDate = t.Datum;
                }
            }
            if (minDate.ToString("yyyy-MM-dd") != GetMondayofWeek(DateTime.Now).AddDays(-14).ToString("yyyy-MM-dd"))
                UpdateDutyPlan();
        }

        private static void UpdateDutyPlan()
        {
            //Get date of monday 2 weeks ago
            DateTime date = GetMondayofWeek(DateTime.Now).AddDays(-14);
            //Delete Tage which are older than 2 weeks
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Tage.SqlQuery("SELECT * FROM Tage WHERE Datum < {0}",date.ToString("yyyy-MM-dd"));
                foreach (Tage t in query)
                {
                    context.Tage.Remove(t);
                }
                context.SaveChanges();
            }
            AddDaysToDutyPlan();
        }

        private static void AddDaysToDutyPlan()
        {
            DateTime mondayOfFirstWeekToAdd = DateTime.Now.AddDays(-14);
            string[] wochentage =
            {
                "Montag",
                "Dienstag",
                "Mittwoch",
                "Donnerstag",
                "Freitag"
            };
            int NumberOfDutyPlansToAdd = GetNumberOfDutyPlansToAdd();
            if(NumberOfDutyPlansToAdd < 6)
            {
                mondayOfFirstWeekToAdd = (DateTime)GetLastDayInDatabase();
                mondayOfFirstWeekToAdd.AddDays(3);
            }
            int currentMonth = mondayOfFirstWeekToAdd.Month;
            OrgaSANItionEntities context = new OrgaSANItionEntities();
            for (int i = 0; i < NumberOfDutyPlansToAdd; i++)
            {
                for(int j = 0; j < 5; j++)
                {
                    context.Tage.Add(CreateDay(wochentage[j], mondayOfFirstWeekToAdd));
                    mondayOfFirstWeekToAdd = mondayOfFirstWeekToAdd.AddDays(1);
                }
                mondayOfFirstWeekToAdd = mondayOfFirstWeekToAdd.AddDays(2);
                ResetAnzahlDienste();
                if (currentMonth != mondayOfFirstWeekToAdd.Month)
                {
                    ResetScores();
                    currentMonth++;
                }
            }
            context.SaveChanges();
        }

        private static Tage CreateDay(string wochentag, DateTime datum)
        {
            int[] IDsSanis = GetSanisForDay(wochentag);
            int[] IDsSpringer = GetSpringerForDay(wochentag, IDsSanis);
            Tage tag = new Tage();
            {
                tag.Datum = datum;
                tag.Sani1 = IDsSanis[0] == 0 ? null : (int?)IDsSanis[0];
                tag.Sani2 = IDsSanis[1] == 0 ? null : (int?)IDsSanis[1];
                tag.Springer1 = IDsSpringer[0] == 0 ? null : (int?)IDsSpringer[0];
                tag.Springer2 = IDsSpringer[1] == 0 ? null : (int?)IDsSpringer[1];
            }
            return tag;
        }

        private static int[] GetSanisForDay(string wochentag)
        {
            int[] IDs = new int[2];
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Benutzer.SqlQuery("SELECT TOP (2) * " +
                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 AND SaniScore = " +
                    "(SELECT MIN(SaniScore) " +
                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3) " +
                    "ORDER BY NEWID()");
                if(query.Count() == 1)
                {
                    Benutzer favPartner = new Benutzer();
                    foreach (Benutzer b in query)
                    {
                        IDs[0] = b.B_ID;
                        b.AnzahlDienste++;
                        b.SaniScore += 50;
                        favPartner = GetFavPartnerOfUser(b);
                    }
                    if(favPartner != null && favPartner.AnzahlDienste < 3)
                    {
                        IDs[1] = favPartner.B_ID;
                        favPartner.AnzahlDienste++;
                        favPartner.SaniScore += 50;
                        goto done;
                    }
                    var query2 = context.Benutzer.SqlQuery("SELECT TOP (1) * " +
                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 AND B_ID != " + IDs[0] + " AND SaniScore = " +
                    "(SELECT MIN(SaniScore) " +
                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Sanitage_ID = Sanitage.Sanitage_ID) " +
                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 AND B_ID != " + IDs[0] + ")" +
                    " ORDER BY NEWID()");
                    foreach (Benutzer b in query2)
                    {
                        IDs[1] = b.B_ID;
                        b.AnzahlDienste++;
                        b.SaniScore += 50;
                    }
                }
                else
                {
                    Benutzer favPartner = new Benutzer();
                    int zähler = 0;
                    foreach (Benutzer b in query)
                    {
                        IDs[zähler] = b.B_ID;
                        b.AnzahlDienste++;
                        b.SaniScore += 50;
                        zähler++;
                        if (b != null)
                        {
                            favPartner = GetFavPartnerOfUser(b);
                            if(favPartner != null && favPartner.AnzahlDienste < 3)
                            {
                                IDs[1] = favPartner.B_ID;
                                favPartner.AnzahlDienste++;
                                favPartner.SaniScore += 50;
                                break;
                            }
                        }
                    }
                }
                done:
                context.SaveChanges();
            }
            return IDs;
        }

        private static int[] GetSpringerForDay(string wochentag, int[] sanis)
        {
            int[] IDs = new int[2];
            string B_ID1 = "";
            string B_ID2 = "";
            if (sanis[0] != 0)
            {
                B_ID1 = "AND B_ID != " + sanis[0];
                if (sanis[1] != 0)
                {
                    B_ID2 = " AND B_ID != " + sanis[1];
                }
            }
            string queryString = "SELECT TOP (2) * " +
                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " + B_ID1 + B_ID2 + " AND SpringerScore = " +
                    "(SELECT MIN(SpringerScore) " +
                    "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
                    "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " + B_ID1 + B_ID2 + ")";
            queryString += " ORDER BY NEWID()";
            using (OrgaSANItionEntities context = new OrgaSANItionEntities())
            {
                var query = context.Benutzer.SqlQuery(queryString);
                if (query.Count() == 1)
                {
                    Benutzer favPartner = new Benutzer();
                    foreach (Benutzer b in query)
                    {
                        IDs[0] = b.B_ID;
                        b.AnzahlDienste++;
                        b.SpringerScore += 50;
                    }
                    if (favPartner != null && favPartner.AnzahlDienste < 3)
                    {
                        IDs[1] = favPartner.B_ID;
                        favPartner.AnzahlDienste++;
                        favPartner.SaniScore += 50;
                        goto done;
                    }
                    var query2 = context.Benutzer.SqlQuery("SELECT TOP (1) * " +
                        "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
                        "WHERE Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " + B_ID1 + B_ID2 + " AND B_ID != " + IDs[0] + " AND SpringerScore = " +
                        "(SELECT MIN(SpringerScore) " +
                        "FROM Benutzer INNER JOIN Sanitage ON (Benutzer.Springertage_ID = Sanitage.Sanitage_ID) " +
                        "WHERE SpringerScore > (SELECT MIN(SpringerScore) FROM Benutzer WHERE B_ID != " + IDs[0] + " " + B_ID1 + B_ID2 +") AND Sanitage." + wochentag + " = 'True' AND AnzahlDienste < 3 " +
                        B_ID1 + B_ID2 + " AND B_ID != " + IDs[0] + ")" +
                        " ORDER BY NEWID()");
                    foreach (Benutzer b in query2)
                    {
                        IDs[1] = b.B_ID;
                        b.AnzahlDienste++;
                        b.SpringerScore += 50;
                    }
                }
                else
                {
                    Benutzer favPartner = new Benutzer();
                    int zähler = 0;
                    foreach (Benutzer b in query)
                    {
                        IDs[zähler] = b.B_ID;
                        b.AnzahlDienste++;
                        b.SpringerScore += 50;
                        zähler++;
                        if (b != null)
                        {
                            favPartner = GetFavPartnerOfUser(b);
                            if (favPartner != null && favPartner.AnzahlDienste < 3)
                            {
                                IDs[1] = favPartner.B_ID;
                                favPartner.AnzahlDienste++;
                                favPartner.SpringerScore += 50;
                                break;
                            }
                        }
                    }
                }
                done:
                context.SaveChanges();
            }
            return IDs;
        }
    }
}