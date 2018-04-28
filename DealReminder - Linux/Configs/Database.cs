using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using DealReminder_Linux.Logging;
using Mono.Data.Sqlite;

namespace DealReminder_Linux.Configs
{
    internal class Database
    {
        public static SqliteConnection Connection = new SqliteConnection(@"Data Source=|DataDirectory|Database.db;MultipleActiveResultSets=True;Version=3;");
        public static string DatabaseFile = Path.Combine(FoldersFilesAndPaths.Data, "Database.db");

        public static void OpenConnection()
        {
            if (Connection.State == ConnectionState.Open) return;
            try
            {
                Connection.Close();
                Connection.Open();
            }
            catch (Exception ex)
            {
                Logger.Write("Datenbank Verbindung öffnen Fehlgeschlagen - Grund: " + ex.Message);
                MessageBox.Show(@"Es gab einen Fehler beim Öffnen der Datenbank Verbindung." + Environment.NewLine + @"Bitte Kontaktieren den Entwickler." + Environment.NewLine + @"Fehlercode: " + ex.Message, @"DealReminder für Amazon", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void CloseConnection()
        {
            if (Connection.State == ConnectionState.Closed) return;
            try
            {
                Connection.Close();
            }
            catch (Exception ex)
            {
                Logger.Write("Datenbank Verbindung schließen Fehlgeschlagen - Grund: " + ex.Message);
                MessageBox.Show(@"Es gab einen Fehler bei Schließen der Datenbank Verbindung." + Environment.NewLine + @"Bitte Kontaktieren den Entwickler." + Environment.NewLine + @"Fehlercode: " + ex.Message, @"DealReminder für Amazon", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void StartUpCheck()
        {
            Logger.Write("Überprüfe Datenbank...");

            RemoveOldDatabase();
            CreateNewDatabase();
            CheckTables();
            UpdateColumns();
           
            Logger.Write("Datenbank Überprüfung beendet...");
        }

        public static bool TableExists(String table)
        {
            try
            {
                OpenConnection();
                SqliteCommand checkEntry = new SqliteCommand(
                    "SELECT COUNT(*) FROM " + table, Database.Connection);
                checkEntry.ExecuteScalar();
                return true;;
            }
            catch
            {
                return false;
            }
        }

        public static void RemoveOldDatabase()
        {
            if (File.Exists(Path.Combine(FoldersFilesAndPaths.Data, "Datenbank.mdf")))
            {
                Logger.Write("Alte Datenbank (Data\\Datenbank.mdf) wird gelöscht.");
                MessageBox.Show("Servus lieber DealReminder Nutzer,\n" +
                                "bei dir wurde eine veraltete Datenbank gefunden welche leider nicht länger unterstützt wird.\n\n" +
                                "Aus diesem Grund wird DealReminder diese nun Löschen und anschließend eine neue Erstellen.\n\n" +
                                "Bei diesem vorgang werden leider alle Produkte und deren Erinnerungen Gelöscht und müssen anschließend neu Eingetragen werden.\n\n" +
                                "Dies wird das letzte mal sein das eine neue Datenbank angelegt werden muss, in Zukunft erfolgen alle Datenbank Updates/änderungen\n" +
                                "Vollautomatisch direkt über die DealReminder App.\n\n" +
                                "Die neue änderung bringt nun auch Support für Windows 7 und älter mit sich.\n\n" +
                                "Mfg euer Spegeli");
                File.Delete(Path.Combine(FoldersFilesAndPaths.Data, "Datenbank.mdf"));
                if (File.Exists(Path.Combine(FoldersFilesAndPaths.Data, "Datenbank_log.ldf")))
                    File.Delete(Path.Combine(FoldersFilesAndPaths.Data, "Datenbank_log.ldf"));
            }
        }

        public static void CreateNewDatabase()
        {
            if (File.Exists(DatabaseFile)) return;
            Logger.Write(@"Datenbank nicht vorhanden. Neue Datenbank (Data\Database.db) wird erstellt.");
            try
            {
                SqliteConnection.CreateFile(DatabaseFile);
            }
            catch (Exception ex)
            {
                Logger.Write("Erstellen der Datenbank Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                Application.Exit();
            }
        }

        public static void CheckTables()
        {
            if (!TableExists("Products"))
            {
                Logger.Write("Table \"Products\" in Datenbank nicht vorhanden...neu erstellen.");
                try
                {
                    OpenConnection();
                    const string entry = "create table if not exists Products (ID INTEGER PRIMARY KEY," +
                                         "Status int(1) default '0'," +
                                         "Store vchar(2)," +
                                         "[ASIN / ISBN] vchar(2)," +
                                         "Name nchar(255)," +
                                         "[Preis: Neu] vchar(10)," +
                                         "[Preis: Wie Neu] vchar(10), " +
                                         "[Preis: Sehr Gut] vchar(10)," +
                                         "[Preis: Gut] vchar(10)," +
                                         "[Preis: Akzeptabel] vchar(10)," +
                                         "[URL] nchar(255)," +
                                         "[Letzter Check] datetime)";
                    SqliteCommand command = new SqliteCommand(entry, Connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Write("Erstellen der Tabelle \"Products\" Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                    Application.Exit();
                }
            }

            if (!TableExists("Reminders"))
            {
                Logger.Write("Table \"Reminders\" in Datenbank nicht vorhanden...neu erstellen.");
                try
                {
                    OpenConnection();
                    const string entry = "create table if not exists Reminders (ID INTEGER PRIMARY KEY," +
                                         "ProductID int," +
                                         "Zustand nchar(255)," +
                                         "Preis vchar(10)," +
                                         "Email nchar(255), " +
                                         "Telegram int," +
                                         "[Erinnerung Gesendet] datetime)";
                    SqliteCommand command = new SqliteCommand(entry, Connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Write("Erstellen der Tabelle \"Reminders\" Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                    Application.Exit();
                }
            }
        }

        public static void UpdateColumns()
        {
            Dictionary<string, string> updateColumns = new Dictionary<string, string>
            {
                {
                    //V0.0.1.4
                    "Status", "ALTER TABLE Products ADD COLUMN Status INT(1) DEFAULT '0'"
                }
            };
            foreach (var pair in updateColumns)
            {
                string columnName = pair.Key;
                string sql = pair.Value;
                try
                {
                    OpenConnection();
                    SqliteCommand command = new SqliteCommand(sql, Connection);
                    command.ExecuteNonQuery();
                }
                catch (SqliteException)
                {
                    Logger.Write($"Fehler beim Updaten der Spalte [{columnName}]. Most likely it already exists, which is fine.");
                }
            }
        }
    }
}
