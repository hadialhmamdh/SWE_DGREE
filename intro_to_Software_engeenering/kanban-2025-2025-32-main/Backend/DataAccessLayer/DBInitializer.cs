using System;
using System.Data.SQLite;
using System.IO;

public class DBInitializer
{
    private const string DbFileName = "kanban.db";

    public static void Initialize()
    {
        string dbPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), DbFileName));
        bool isNewDatabase = !File.Exists(dbPath);

        if (isNewDatabase)
        {
            SQLiteConnection.CreateFile(dbPath);
            Console.WriteLine("Database file 'kanban.db' created.");
        }

        using (var connection = new SQLiteConnection($"Data Source={dbPath}; Version=3;"))
        {
            connection.Open();

            // Enable foreign key constraints
            using (var pragmaCmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                pragmaCmd.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(connection))
            {
                // --- Users ---
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Email TEXT PRIMARY KEY,
                        Password TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();

                // --- Boards ---
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Boards (
                        Id INTEGER PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Owner TEXT NOT NULL,
                        BoardCounter INTEGER NOT NULL,
                        FOREIGN KEY(Owner) REFERENCES Users(Email) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // --- BoardMembers ---
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS BoardMembers (
                        Board_id INTEGER NOT NULL,
                        User TEXT NOT NULL,
                        PRIMARY KEY(Board_id, User),
                        FOREIGN KEY(Board_id) REFERENCES Boards(Id) ON DELETE CASCADE,
                        FOREIGN KEY(User) REFERENCES Users(Email) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // --- Columns ---
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Columns (
                        Column TEXT NOT NULL,
                        [Limit] INTEGER NOT NULL,
                        BoardID INTEGER NOT NULL,
                        PRIMARY KEY (Column, BoardID),
                        FOREIGN KEY (BoardID) REFERENCES Boards(Id) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // --- Tasks ---
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER NOT NULL,
                        BoardID INTEGER NOT NULL,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        DueDate TEXT,
                        CreationTime TEXT,
                        Assignee TEXT,
                        Column TEXT NOT NULL,
                        PRIMARY KEY (Id, BoardID),
                        FOREIGN KEY (Assignee) REFERENCES Users(Email) ON DELETE SET NULL,
                        FOREIGN KEY (BoardID) REFERENCES Boards(Id) ON DELETE CASCADE,
                        FOREIGN KEY (Column, BoardID) REFERENCES Columns(Column, BoardID) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // --- SystemConfig ---
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS SystemConfig (
                        Key TEXT PRIMARY KEY,
                        Value TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();

                // Ensure BoardCounter key exists, insert only if it doesn't exist
                command.CommandText = @"
                    INSERT OR IGNORE INTO SystemConfig (Key, Value)
                    VALUES ('BoardCounter', '0');";
                command.ExecuteNonQuery();

                Console.WriteLine("Database initialized successfully.");
            }
        }
    }
}
