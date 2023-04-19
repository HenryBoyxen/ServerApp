using ExampleGUI.SQL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleGUI.Classes
{
    public class DBUserManager : IDisposable
    {
        private readonly OrgaSANItionEntities _context;

        public DBUserManager()
        {
            _context = new OrgaSANItionEntities();
        }

        /// <summary>
        /// Erhöht den Score und die Anzahl der Dienste des angegebenen Benutzers
        /// </summary>
        /// <param name="benutzer"></param>
        public void SaniEingetragen(Benutzer benutzer)
        {
            benutzer.SaniScore++;
            benutzer.AnzahlDienste++;
            _context.SaveChanges();
        }

        public void ResetAnzahlDienste()
        {
            var users = _context.Benutzer.ToList();
            foreach (var user in users)
            {
                user.AnzahlDienste = 0;
            }
            _context.SaveChanges();
        }

        public void ResetScores()
        {
            var users = _context.Benutzer.ToList();
            foreach (var user in users)
            {
                user.SaniScore = 0;
                user.SpringerScore = 0;
            }
            _context.SaveChanges();
        }

        public Benutzer GetFavPartnerOfUser(Benutzer benutzer)
        {
            Benutzer favPartner = null;
            foreach (Benutzer b in _context.Benutzer.SqlQuery("SELECT * FROM Benutzer WHERE B_ID = {0}", benutzer.FavPartner))
            {
                favPartner = b;
            }

            return favPartner;
        }

        /// <summary>
        /// Voraussetzungen: 
        /// 1. wochentag muss beim Sani True sein 
        /// 2. Muss weniger als 3 Dienste haben
        /// </summary>
        /// <param name="wochentag"></param>
        /// <returns>Gibt unter Berücksichtigung der Voraussetzungen alle verfügbaren Sanis, sortiert nach dem SaniScore, zurück</returns>
        public List<Benutzer> GetAvailableSanis(string wochentag)
        {
            //Voraussetzungen:
                //wochentag true
                //Anzahl Dienste < 3
            var query = _context.Benutzer.SqlQuery("SELECT * FROM Benutzer " +
               "WHERE AnzahlDienste < 3 AND Sanitage_ID IN " +
               "(SELECT Sanitage_ID FROM Sanitage WHERE " + wochentag + " = 'True')" +
               "ORDER BY SaniScore, NEWID()");

            return query.ToList();
        }

        public void Dispose()
        {
            _context.SaveChanges();
            _context.Dispose();
        }
    }
}
