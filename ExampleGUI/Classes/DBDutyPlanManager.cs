using ExampleGUI.SQL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace ExampleGUI.Classes
{
    public class DBDutyPlanManager: IDisposable
    {
        private OrgaSANItionEntities _context;
        private int _maxPastDays = 14;
        private int _maxFutureDays = 84;

        public DBDutyPlanManager()
        {
            _context = new OrgaSANItionEntities();
        }

        /// <summary>
        /// Überprüft den Dienstplan, ob dieser aktualisiert werden muss. 
        /// </summary>
        public void CheckDutyPlan()
        {
            var tageList = _context.Tage.ToList();
            //Wenn keine Datensätze in der DB vorliegen, werden nur Tage hinzugefügt und die Methode beendet
            if (!tageList.Any())
            {
                AddDutyPlans();
                ServerConsole.WriteLine("Added Days to DB");
                return;
            }

            //Liegen Datensätze in der DB vor, werden jene gelöscht, die älter als _maxPastDays sind und neue Dienstpläne hinzugefügt
            DateTime minDate = _context.Tage.Min(t => t.Datum);
            DateTime maxDate = _context.Tage.Max(t => t.Datum);
            if (minDate < DateTimeExtension.GetMondayOfWeek(DateTime.Now).AddDays(-_maxPastDays))
            {
                RemoveDaysFromDB();
                ServerConsole.WriteLine("Removed Days from DB");
            }
            if (maxDate < DateTimeExtension.GetMondayOfWeek(DateTime.Now).AddDays(_maxFutureDays - 2))
            {
                AddDutyPlans();
                ServerConsole.WriteLine("Added Days to DB");
            }

            return;
        }

        /// <summary>
        /// Entferne Tage aus der DB, die älter als die angegebene Anzahl an '_maxPastDays',
        /// ausgehend vom Montag der aktuellen Woche, sind.
        /// </summary>
        private void RemoveDaysFromDB()
        {
            DateTime mondayTwoWeeksAgo = DateTimeExtension.GetMondayOfWeek(DateTime.Now).AddDays(_maxPastDays);
            var oldTage = _context.Tage.Where(t => t.Datum < mondayTwoWeeksAgo);
            _context.Tage.RemoveRange(oldTage);
            _context.SaveChanges();
        }

        /// <summary>
        /// Fügt Dienstpläne in Abhängigkeit von _maxFutureDays hinzu
        /// </summary>
        private void AddDutyPlans()
        {
            //Herausfinden, wie viele Dienstpläne hinzugefügt werden müssen und das Datum des Tages, der als erstes hinzugefügt wird
            int dutyPlansToAdd = GetDutyPlansToAdd();
            DateTime mondayOfWeekToAdd = GetMondayOfFirstWeekToAdd();
            int currentMonth = mondayOfWeekToAdd.Month;
            string[] wochentage =
            {
                "Montag",
                "Dienstag",
                "Mittwoch",
                "Donnerstag",
                "Freitag"
            };

            //Für die Häufigkeit der hinzuzufügenden Dienstpläne
            for (int i = 0; i < dutyPlansToAdd; i++)
            {
                //Anzahl der hinzuzufügenden Tage pro Dienstplan (5)
                for (int j = 0; j < 5; j++)
                {
                    _context.Tage.Add(CreateTag(wochentage[j], mondayOfWeekToAdd.AddDays(j)));
                }
                mondayOfWeekToAdd = mondayOfWeekToAdd.AddDays(7);
                DBUserManager dBUserManager = new DBUserManager();
                dBUserManager.ResetAnzahlDienste();
                if (currentMonth != mondayOfWeekToAdd.Month)
                {
                    dBUserManager.ResetScores();
                    currentMonth++;
                }
                dBUserManager.Dispose();
            }
            _context.SaveChanges();
        }

        private int GetDutyPlansToAdd()
        {
            int dutyPlansToAdd;
            //Letzten Tag aus DB
            var lastDayInDB = _context.Tage.OrderByDescending(t => t.Datum).FirstOrDefault();
            if (lastDayInDB != null && lastDayInDB.Datum > DateTime.Now)
            {
                DateTime lastDayDate = lastDayInDB.Datum;
                //Anzahl Tage zwischen Montag der aktuellen Woche und letztem Tag aus der DB
                int difference = DateTimeExtension.GetDaysBetweenDates(DateTimeExtension.GetMondayOfWeek(DateTime.Now), lastDayDate);
                //2 Tage zu difference hinzufügen, da der letzte Tag in der DB immer ein Freitag ist
                difference += 2;
                dutyPlansToAdd = _maxFutureDays - difference;
            }
            else
            {
                dutyPlansToAdd = _maxFutureDays;
            }
            //dutyPlansToAdd beinhaltet die Anzahl der Tage, die hinzugefügt werden müssen. Durch Dividieren mit 7 erhält man
            //die Anzahl der Wochenpläne.
            return dutyPlansToAdd / 7;
        }

        private DateTime GetMondayOfFirstWeekToAdd()
        {
            var lastDayInDB = _context.Tage.OrderByDescending(t => t.Datum).FirstOrDefault();
            if (lastDayInDB != null && lastDayInDB.Datum > DateTime.Now)
            {
                return lastDayInDB.Datum.AddDays(3);
            }
            else
            {
                return DateTimeExtension.GetMondayOfWeek(DateTime.Now);
            }
        }

        private Tage CreateTag(string wochentag, DateTime mondayOfWeekToAdd)
        {
            int?[] IDsSanis = GetSanisForDay(wochentag);
            //int[] IDsSpringer = GetSpringerForDay(wochentag, IDsSanis);
            Tage tag = new Tage();
            {
                tag.Datum = mondayOfWeekToAdd;
                tag.Sani1 = IDsSanis[0];
                tag.Sani2 = IDsSanis[1];
                tag.Springer1 = null;
                tag.Springer2 = null;
                //tag.Springer1 = IDsSpringer[0] == 0 ? null : (int?)IDsSpringer[0];
                //tag.Springer2 = IDsSpringer[1] == 0 ? null : (int?)IDsSpringer[1];
            }
            return tag;
        }

        private int?[] GetSanisForDay(string wochentag)
        {
            int?[] IDsSanis = new int?[2];
            DBUserManager dBUserManager = new DBUserManager();
            List<Benutzer> availableSanis = dBUserManager.GetAvailableSanis(wochentag);

            //Wenn keine Sanis zur Verfügung stehen, wird das Array mit null zurückgegeben
            if(availableSanis.Count == 0)
            {
                IDsSanis[0] = null;
                IDsSanis[1] = null;
                return IDsSanis;
            }

            IDsSanis[0] = availableSanis[0].B_ID;
            dBUserManager.SaniEingetragen(availableSanis[0]);

            //Wenn es keinen zweiten Sani gibt, wird null als 2. Sani eingetragen
            if (availableSanis.Count == 1)
            {
                IDsSanis[1] = null;
                return IDsSanis;
            }

            //Wenn der erste ausgewählte Sani einen FavPartner hat und dieser in der Liste ist, wird er als 2. Sani eingetragen
            Benutzer favPartner = dBUserManager.GetFavPartnerOfUser(availableSanis[0]);
            if (favPartner != null && availableSanis.Contains(favPartner))
            {
                IDsSanis[1] = favPartner.B_ID;
                dBUserManager.SaniEingetragen(favPartner);
                dBUserManager.Dispose();
                return IDsSanis;
            }

            //Wenn kein FavPartner vorhanden oder dieser nicht in der Liste ist, wird der zweite Sani aus der Liste ausgewählt
            IDsSanis[1] = availableSanis[1].B_ID;
            dBUserManager.SaniEingetragen(availableSanis[1]);
            dBUserManager.Dispose();
            return IDsSanis;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
