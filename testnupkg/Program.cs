using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace testnupkg
{
    class Program
    {
        static string defaultTable = "";
   
        const char useTable = 'u',
            createTable = '1',
            readFromTable = '2',
            readFromTableWhere = '3',
            readWholeTable = '4',
            insertIntoTable = '5',
            deleteFromTable = '6',
            dropTable = '7',
            truncateTable = '8',
            checkTableForDuplicates = '9',
            viewColumnNames = 'c',
            viewPKColumnName = 'p',
            clrScreen = 's';

        static void DisplaySuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void DisplayWarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void DisplayErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message + "\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        //--

        static void CreateTableIfNotExists(SQLiteConnection conn, string TableName, string VarsWithCommas)
        {
            try
            {
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + TableName + "(" + VarsWithCommas + ");", conn).ExecuteNonQuery();
                DisplaySuccessMessage("Command executed successfully");
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return;
            }
        }

        static string ReadFromTable(SQLiteConnection conn, bool Write, string TableName, string[] ColumnsNoCommas)
        {
            string returnString = "";
            string queryString = "SELECT ";

            for (int i = 0; i < ColumnsNoCommas.Length; i++)
            {
                 queryString += ColumnsNoCommas[i] + ", ";
            }

            char[] queryStringCharArray = queryString.ToCharArray();
            queryString = "";

            for (int i = 0; i < queryStringCharArray.Length - 2; i++)
            {
                queryString += queryStringCharArray[i].ToString();
            }

            queryString +=  " FROM " + TableName + ";";

            //Console.WriteLine("Executing Query:\n" + queryString + "\n");

            try
            {
                using (SQLiteDataReader reader = new SQLiteCommand(queryString, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < ColumnsNoCommas.Length; i++)
                        {
                            string value = "";

                            if (reader.GetFieldType(i) == typeof(string))
                                value = "'" + reader[i].ToString() + "'";
                            else
                                value = reader[i].ToString();

                            returnString += ColumnsNoCommas[i] + ": " + value + "\n";
                        }
                    }

                   // Console.WriteLine("Command executed successfully");
                }
            }

            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return "";
            }

            if (Write)
            {
                Console.WriteLine(returnString);
            }

            return returnString;
        }

        static string ReadFromTableWhere(SQLiteConnection conn, bool Write, string TableName, string[] ColumnsToReadNoCommas, string[] ColumnsConditionsNoCommas, string[] ValuesNoCommas)
        {
            string returnString = "";
            string queryString = "SELECT ";

            for (int i = 0; i < ColumnsToReadNoCommas.Length; i++)
            {
                 queryString += ColumnsToReadNoCommas[i] + ", ";
            }

            char[] queryStringCharArray = queryString.ToCharArray();
            queryString = "";

            for (int i = 0; i < queryStringCharArray.Length - 2; i++)
            {
                queryString += queryStringCharArray[i].ToString();
            }

            queryString +=  " FROM " + TableName + " WHERE ";

            for (int i = 0; i < ColumnsConditionsNoCommas.Length; i++)
            {
                 queryString += ColumnsConditionsNoCommas[i] + "=" + ValuesNoCommas[i] + " AND ";
            }

            queryStringCharArray = queryString.ToCharArray();
            queryString = "";

            for (int i = 0; i < queryStringCharArray.Length - 5; i++)
            {
                queryString += queryStringCharArray[i].ToString();
            }

            //Console.WriteLine("Executing Query:\n" + queryString + "\n");

            try
            {
                using (SQLiteDataReader reader = new SQLiteCommand(queryString, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < ColumnsToReadNoCommas.Length; i++)
                        {
                            string value = "";

                            if (reader.GetFieldType(i) == typeof(string))
                                value = "'" + reader[i].ToString() + "'";
                            else
                                value = reader[i].ToString();

                            returnString += ColumnsToReadNoCommas[i] + ": " + value + "\n";
                        }
                    }

                   // Console.WriteLine("Command executed successfully");
                }
            }

            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return "";
            }

            if (Write)
            {
                Console.WriteLine(returnString);
            }

            return returnString;
        }

        static void WriteNewRowsToTable(SQLiteConnection conn, int numRows, string TableName, string ColumnsWithCommas, string[] ValuesWithCommas)
        {
            for (int i = 0; i < numRows; i++)
            {
                try
                {
                    new SQLiteCommand("INSERT INTO " + TableName + " (" + ColumnsWithCommas + ") VALUES (" + ValuesWithCommas[i] + ");", conn).ExecuteNonQuery();
                    DisplaySuccessMessage("Command executed successfully");
                }
                catch (Exception e)
                {
                    DisplayErrorMessage(e.Message);
                    return;
                }
            }
        }

        static void DeleteRowFromTable(SQLiteConnection conn, string TableName, string[] ColumnsNoCommas, string[] ValuesNoCommas)
        {
            string command = "DELETE FROM " + TableName + " WHERE ";

            for (int i = 0; i < ColumnsNoCommas.Length; i++)
            {
                command += ColumnsNoCommas[i] + "=" + ValuesNoCommas[i] + (i == ColumnsNoCommas.Length-1 ? ";" : " AND ");
            }

            //Console.WriteLine("Executing Command:\n" + command + "\n");

            try
            {
                new SQLiteCommand(command, conn).ExecuteNonQuery();
                DisplaySuccessMessage("Command executed successfully");
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return;
            }
        }

        static void DropTable(SQLiteConnection conn, string TableName)
        {
            try
            {
                DisplayWarningMessage("Are you sure you want to DROP TABLE " + TableName + "? <y/n>");

                ConsoleKey ck;

                do
                {
                    ck = ReadKeyWithEscape().Key;
                }
                while (ck != ConsoleKey.Y && ck != ConsoleKey.N);

                if (ck == ConsoleKey.N) return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            try
            {
                new SQLiteCommand("DROP TABLE " + TableName + ";", conn).ExecuteNonQuery();
                DisplaySuccessMessage("\nsuccessfully dropped table");
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return;
            }
        }

        static void TruncateTable(SQLiteConnection conn, string TableName)
        {
            try
            {
                DisplayWarningMessage("Are you sure you want to TRUNCATE TABLE " + TableName + "? <y/n>");

                ConsoleKey ck;

                do
                {
                    ck = ReadKeyWithEscape().Key;
                }
                while (ck != ConsoleKey.Y && ck != ConsoleKey.N);

                if (ck == ConsoleKey.N) return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }

            try
            {
                new SQLiteCommand("DELETE FROM " + TableName + ";", conn).ExecuteNonQuery();
                DisplaySuccessMessage("\nsuccessfully truncated table.");
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return;
            }
        }

        static string[] FindColumnNames(SQLiteConnection conn, string TableName, bool Write)
        {
            List<String> ColumnNamesList = new List<string>();

            try
            {
                using (SQLiteDataReader datareader = new SQLiteCommand("PRAGMA table_info(" + TableName + ");", conn).ExecuteReader())
                {
                    while (datareader.Read())
                    {
                        ColumnNamesList.Add(datareader["name"].ToString());
                    }

                    //Console.WriteLine("Command executed successfully");
                }
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return new string[0];
            }

            string[] ColumnNames = new string[ColumnNamesList.Count];

            for (int i = 0; i < ColumnNames.Length; i++)
            {
                ColumnNames[i] = ColumnNamesList[i];

                if (Write) Console.WriteLine(ColumnNames[i]);
            }

            return ColumnNames;
        }

        static string FindPKColumnName(SQLiteConnection conn, string TableName, bool Write)
        {
            string PKColumnName = "";

            try
            {
                using (SQLiteDataReader datareader = new SQLiteCommand("PRAGMA table_info(" + TableName + ");", conn).ExecuteReader())
                {
                    while (datareader.Read())
                    {
                        if (Convert.ToInt32(datareader["pk"]) == 1) PKColumnName = datareader["name"].ToString();
                    }

                    //Console.WriteLine("Command executed successfully");
                }
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return "";
            }

            if (Write) Console.WriteLine(PKColumnName);

            return PKColumnName;
        }

        static void CheckTableForDuplicateRows(SQLiteConnection conn, string TableToCheck, bool WriteLog, bool DeleteDuplicateRows)
        {
            List<string> ColumnsToCheckList = new List<string>(FindColumnNames(conn, TableToCheck, false));
            string PKColumn = FindPKColumnName(conn, TableToCheck, false);
            ColumnsToCheckList.Remove(PKColumn);
            string[] ColumnsToCheck = new string[ColumnsToCheckList.Count];

            for (int i = 0; i < ColumnsToCheckList.Count; i++)
            {
                ColumnsToCheck[i] = ColumnsToCheckList[i];
                //Debug
                //Console.WriteLine(ColumnsToCheck[i]);
            } 

            string read = ReadFromTable(conn, false, TableToCheck, ColumnsToCheck);
            
            string[] lines = read.Split('\n');

            int numTotalRows = lines.Length/ColumnsToCheck.Length;

            string[] rowsStrArray = new string[ numTotalRows ];

            for (int i = 0; i < numTotalRows; i++)
            {
                for (int j = 0; j < ColumnsToCheck.Length; j++)
                {
                    rowsStrArray[i] += lines[(i*ColumnsToCheck.Length) + j] + ((j == ColumnsToCheck.Length - 1) ? "" : "\n");
                }
            }

            List<string> rows = new List<string>(rowsStrArray);

            for (int i = 0; i < rows.Count - 1; i++)
            {
                for (int j = i + 1; j < rows.Count; j++)
                {
                    if (rows[i] == rows[j])
                    {
                        if (WriteLog) 
                        {
                            Console.WriteLine("rows " + (i+1) + " and " + (j+1) + " are duplicates\n");
                        }

                        if (DeleteDuplicateRows)
                        {
                            if (WriteLog)
                            Console.WriteLine("Attempting to delete row " + (j+1) + "...\n");

                            string[] values = new string[ColumnsToCheck.Length];
                            string[] rowLines = rows[j].Split('\n');

                            for (int k = 0; k < values.Length; k++)
                            {
                                values[k] = rowLines[k].Split(new string[] { ": " }, StringSplitOptions.None)[1];

                                //Debug
                                //Console.WriteLine(values[k]);
                            }

                            string PKColumnName = FindPKColumnName(conn, TableToCheck, false);

                            string PKValue = ReadFromTableWhere(conn, false, TableToCheck, new string[] { PKColumnName }, ColumnsToCheck, values).Split('\n')[1].Split(new string[] {": "}, StringSplitOptions.None)[1];

                            //Debug
                            //Console.WriteLine(PKValue);

                            try
                            {
                                DeleteRowFromTable(conn, TableToCheck, new string[] { PKColumnName }, new string[] { PKValue });

                                if (WriteLog)
                                {
                                    DisplaySuccessMessage("Successfully deleted row\n");
                                }
                            }
                            catch (Exception e)
                            {
                                DisplayErrorMessage(e.Message);
                                return;
                            }
                        }
                    }
                }
            }
        }

        static string ReadWholeTable(SQLiteConnection conn, string TableName, bool Write)
        {
            try
            {
                string[] Columns = FindColumnNames(conn, TableName, false);
                return ReadFromTable(conn, Write, TableName, Columns);
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return "";
            }
        }

        //-

        static void SetCurrentTable(SQLiteConnection conn, bool user)
        {
            List<string> tables = new List<string>();

            try
            {
                if (!user && !string.IsNullOrEmpty(defaultTable) && !string.IsNullOrWhiteSpace(defaultTable)) return;

                using (SQLiteDataReader reader = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", conn).ExecuteReader())
                {
                    int i = 1;

                    while (reader.Read())
                    {
                        tables.Add(reader[0].ToString());
                        Console.WriteLine("[{0}] {1}", i, reader[0]);
                        i++;
                    }
                }

                if (tables.Count == 0)
                {
                    DisplayWarningMessage("No tables found");
                    throw new OperationCanceledException();
                }

                bool firsttry = true;
            asktablenumber:
                if (firsttry)
                Console.WriteLine("\nThe table you choose will be marked as default for this session\nyou can change it anytime by the menu option");
                Console.Write("Enter the number of the table you want to choose: ");

                string input = ReadLineWithEscape();
                firsttry = false;

                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                {
                    DisplayWarningMessage("\nPlease enter a number or press ESC to cancel");
                    goto asktablenumber;
                }

                for (int i = 0; i < input.Length; i++)
                {
                    if (!char.IsDigit(input[i]))
                    {
                        DisplayWarningMessage("\nPlease enter a number or press ESC to cancel");
                        goto asktablenumber;
                    }
                }

                int selected_table = int.Parse(input) - 1;

                if (selected_table < 0 || selected_table >= tables.Count)
                {
                    DisplayWarningMessage("\ninvalid selection.");
                    goto asktablenumber;
                }

                defaultTable = tables[selected_table];
                Console.WriteLine();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        static int AskUserForNumberOfItems(bool col, bool row)
        {
            string thingToAskFor = "";

            if (col) thingToAskFor = "columns";
            else if (row) thingToAskFor = "rows";
            else thingToAskFor = "values";

            int numItems = 0;

            while (numItems <= 0)
            {
                Console.Write("Number of " + thingToAskFor + ": ");
                try
                {
                    numItems = int.Parse(ReadLineWithEscape());
                    Console.WriteLine();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    DisplayErrorMessage(e.Message);
                    return 0;
                }
            }

            return numItems;
        }

        static string[] AskUserForStrArray(bool col, bool row, bool AskUserForNumItems, int? numItems)
        {
            if (AskUserForNumItems) numItems = AskUserForNumberOfItems(col, row);

            string thingToAskFor = "";

            if (col) thingToAskFor = "column";
            else if (row) thingToAskFor = "row";
            else thingToAskFor = "value";

            string[] strArray = new string[0];

            try
            {
                for (int i = 0; i < numItems; i++)
                {
                    Array.Resize(ref strArray, strArray.Length + 1);

                askforcolumns:
                    Console.Write((i + 1) + ". " + thingToAskFor + ": ");
                    string input = ReadLineWithEscape();
                    if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Please enter a " + thingToAskFor);
                        goto askforcolumns;
                    }
                    strArray[i] = input;
                }

                Console.WriteLine();
            }
            catch (OperationCanceledException)
            {
                throw;
            }

            return strArray;
        }

        static string ReadLineWithEscape()
        {
            StringBuilder input = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nOperation cancelled.");
                    throw new OperationCanceledException();
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input.Remove(input.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    input.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }

        static ConsoleKeyInfo ReadKeyWithEscape(bool intercept = true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept);

            if (key.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("\nOperation cancelled.");
                throw new OperationCanceledException();
            }

            return key;
        }

        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.White;

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
            };

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
            {
                conn.Open();

            start:
                try
                {
                    Console.WriteLine();

                    Console.WriteLine("[{0}] Use existing table\n[{1}] Create table\n[{2}] Read from table\n[{3}] Read from table with WHERE conditions\n[{4}] Read entire table\n[{5}] Insert into table\n[{6}] Delete from table\n[{7}] DROP TABLE\n[{8}] TRUNCATE TABLE\n[{9}] Check table for duplicate rows\n[{10}] View column names of table\n[{11}] View Primary Key of table\n[{12}] Clear console screen\n[e] Execute non-query command\n[ESC] Cancel out of an operation\n",
                        useTable, createTable, readFromTable, readFromTableWhere, readWholeTable, insertIntoTable, deleteFromTable, dropTable, truncateTable, checkTableForDuplicates, viewColumnNames, viewPKColumnName, clrScreen);

                    char ck = ReadKeyWithEscape().KeyChar;
                    ck = char.ToLower(ck);

                    if (char.IsDigit(ck))
                    {
                        if (ck < '0' || ck > '9')
                        {
                            Console.WriteLine("Invalid selection.\n");
                            goto start;
                        }

                    }
                    else if (ck != useTable && ck != viewColumnNames && ck != viewPKColumnName && ck != clrScreen && ck != 'e')
                    {
                        Console.WriteLine("Invalid selection.\n");
                        goto start;
                    }

                    if (ck == useTable) SetCurrentTable(conn, true);
                    else if (ck != clrScreen && ck != createTable && ck != 'e') SetCurrentTable(conn, false);

                    switch (ck)
                    {
                        case createTable:
                            Console.Write("Enter table name to create: ");
                            string table_name = ReadLineWithEscape();
                            Console.Write("Declare columns (Separate with commas and spaces): ");
                            string vars = ReadLineWithEscape();

                            CreateTableIfNotExists(conn, table_name, vars);
                            break;

                        case readFromTable:
                            ReadFromTable(conn, true, defaultTable, AskUserForStrArray(true, false, true, null));
                            break;

                        case readFromTableWhere:
                            Console.WriteLine("Specify the columns you want to read");
                            string[] colsToRead = AskUserForStrArray(true, false, true, null);
                            Console.WriteLine("\nSpecify the columns to be included in the search query");
                            string[] colsConditions = AskUserForStrArray(true, false, true, null);
                            string[] values = new string[0];
                            Console.WriteLine("Specify the values to search for");
                            try
                            {
                                for (int i = 0; i < colsConditions.Length; i++)
                                {
                                    Array.Resize(ref values, values.Length + 1);

                                    Console.Write(colsConditions[i] + "=");
                                    values[i] = ReadLineWithEscape();
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                DisplayErrorMessage(e.Message);
                                return;
                            }

                            Console.WriteLine();

                            ReadFromTableWhere(conn, true, defaultTable, colsToRead, colsConditions, values);
                            break;

                        case readWholeTable:
                            ReadWholeTable(conn, defaultTable, true);
                            break;

                        case insertIntoTable:
                            int numRows = AskUserForNumberOfItems(false, true);

                            Console.Write("Columns to insert (Separate with commas and spaces): ");
                            string columnNames = ReadLineWithEscape();

                            for (int i = 0; i < numRows; i++)
                            {
                                Console.Write("Values for " + (i + 1) + ". row " + ((i == 0) ? "(Separate with commas and spaces)" : "") + ": ");
                                WriteNewRowsToTable(conn, 1, defaultTable, columnNames, new string[] { ReadLineWithEscape() });
                            }
                            break;

                        case deleteFromTable:
                            string[] Columns = AskUserForStrArray(true, false, true, null);
                            DeleteRowFromTable(conn, defaultTable, Columns, AskUserForStrArray(false, false, false, Columns.Length));
                            break;

                        case dropTable:
                            DropTable(conn, defaultTable);
                            defaultTable = null;
                            break;

                        case truncateTable:
                            TruncateTable(conn, defaultTable);
                            break;

                        case checkTableForDuplicates:
                            bool writeLog = false, deleteDuplicates = false;

                            Console.WriteLine("Write progress on screen? <y/n>");

                            ConsoleKey ckCurrent;

                            do
                            {
                                ckCurrent = ReadKeyWithEscape().Key;
                            }
                            while (ckCurrent != ConsoleKey.Y && ckCurrent != ConsoleKey.N);

                            writeLog = (ckCurrent == ConsoleKey.Y);

                            Console.WriteLine("\nDelete duplicate rows when they're found? <y/n>");

                            do
                            {
                                ckCurrent = ReadKeyWithEscape().Key;
                            }
                            while (ckCurrent != ConsoleKey.Y && ckCurrent != ConsoleKey.N);

                            deleteDuplicates = (ckCurrent == ConsoleKey.Y);

                            CheckTableForDuplicateRows(conn, defaultTable, writeLog, deleteDuplicates);
                            break;

                        case viewColumnNames:
                            FindColumnNames(conn, defaultTable, true);
                            break;

                        case viewPKColumnName:
                            FindPKColumnName(conn, defaultTable, true);
                            break;

                        case clrScreen:
                            Console.Clear();
                            break;

                        case 'e':
                            try
                            {
                                string command = "";
                                Console.Write("Command: ");
                                command = ReadLineWithEscape();

                                new SQLiteCommand(command, conn).ExecuteNonQuery();
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                DisplayErrorMessage(e.Message);
                                return;
                            }
                            break;
                            
                        default:
                            break;
                    }

                    goto start;
                }
                catch (OperationCanceledException)
                {
                    goto start;
                }
            }
            
            Console.ReadLine();
        }
    }
}
