using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Web.Hosting;

namespace EmployeeService.Data
{
    /// <summary>
    /// Initializes SQLite database with schema and test data
    /// </summary>
    public static class DatabaseInitializer
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static void Initialize()
        {
            if (_initialized)
                return;

            lock (_lock)
            {
                if (_initialized)
                    return;

                string connectionString = ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString;

                string dataDir = HostingEnvironment.IsHosted
                    ? HostingEnvironment.MapPath("~/App_Data")
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }

                string dbPath = Path.Combine(dataDir, "EmployeeDatabase.db");
                connectionString = $"Data Source={dbPath};Version=3;";

                bool needsInitialization = !File.Exists(dbPath) || (File.Exists(dbPath) && new FileInfo(dbPath).Length == 0);

                if (File.Exists(dbPath) && new FileInfo(dbPath).Length > 0)
                {
                    try
                    {
                        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();
                            using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM Employee", connection))
                            {
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                if (count == 0)
                                {
                                    needsInitialization = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        needsInitialization = true;
                    }
                }

                if (needsInitialization)
                {
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }
                    
                    SQLiteConnection.CreateFile(dbPath);

                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();

                        try
                        {
                            CreateSchema(connection);
                            InsertTestData(connection);
                        }
                        catch (Exception ex)
                        {
                            if (File.Exists(dbPath))
                            {
                                File.Delete(dbPath);
                            }
                            throw new Exception($"Failed to initialize database: {ex.Message}", ex);
                        }
                    }
                }

                _initialized = true;
            }
        }

        private static void CreateSchema(SQLiteConnection connection)
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Employee (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ManagerID INTEGER NULL,
                    Enable INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY (ManagerID) REFERENCES Employee(ID)
                );";

            using (SQLiteCommand command = new SQLiteCommand(createTableSql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void InsertTestData(SQLiteConnection connection)
        {
            string insertSql = @"
                INSERT INTO Employee (Name, ManagerID, Enable) VALUES
                ('Andrey', NULL, 1),
                ('Nir', 1, 1),
                ('Roman', 1, 1),
                ('Smadar', 2, 1),
                ('Barak', 2, 1),
                ('Alexey', 3, 1),
                ('Igor', 3, 0);";

            using (SQLiteCommand cmd = new SQLiteCommand(insertSql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}

