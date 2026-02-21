using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            insertDuplicate = '6',
            deleteFromTable = '7',
            dropTable = '8',
            truncateTable = '9',
            checkTableForDuplicates = 'd',
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

        static bool ynHelper(string message, bool warning)
        {
            if (warning) DisplayWarningMessage(message + " <y/n>");
            else Console.Write(message + " <y/n>");

            ConsoleKey ck;
            try
            {
                do
                {
                    ck = ReadKeyWithEscape().Key;
                }
                while (ck != ConsoleKey.Y && ck != ConsoleKey.N);

                Console.WriteLine();

                if (ck == ConsoleKey.N) return false;
                else if (ck == ConsoleKey.Y) return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }

            return false;
        }

        //--

        static void CreateTableIfNotExists(SQLiteConnection conn, string TableName, List<string> Columns)
        {
            try
            {
                string command = string.Format("CREATE TABLE IF NOT EXISTS {0} ({1});", TableName, string.Join(", ", Columns));
                new SQLiteCommand(command, conn).ExecuteNonQuery();
                DisplaySuccessMessage("Command executed successfully");
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return;
            }
        }

        static string ReadFromTable(SQLiteConnection conn, bool Write, string TableName, List<string> Columns)
        {
            string returnString = "";

            string command = string.Format("SELECT {0} FROM {1};", string.Join(", ", Columns), TableName);

            //Debug
            //Console.WriteLine(command);

            try
            {
                using (SQLiteDataReader reader = new SQLiteCommand(command, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < Columns.Count; i++)
                        {
                            string value = "";

                            if (reader.GetFieldType(i) == typeof(string))
                                value = "'" + reader[i].ToString() + "'";
                            else
                                value = reader[i].ToString();

                            returnString += Columns[i] + ": " + value + "\n";
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

        static string ReadFromTableWhere(SQLiteConnection conn, bool Write, string TableName, List<string> ColumnsRead, List<string> ColumnsSearch, List<string> Values)
        {
            string returnString = "";

            List<string> equals = new List<string>(ColumnsSearch.Count);

            for (int i = 0; i < ColumnsSearch.Count; i++)
            {
                equals.Add(string.Format("{0}={1}", ColumnsSearch[i], Values[i]));

                //Debug
                //Console.WriteLine(equals[i]);
            }

            string command = string.Format("SELECT {0} FROM {1} WHERE {2};", string.Join(", ", ColumnsRead), TableName, string.Join(" AND ", equals));

            //Debug
            //Console.WriteLine(command);

            try
            {
                using (SQLiteDataReader reader = new SQLiteCommand(command, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < ColumnsRead.Count; i++)
                        {
                            string value = "";

                            if (reader.GetFieldType(i) == typeof(string))
                                value = "'" + reader[i].ToString() + "'";
                            else
                                value = reader[i].ToString();

                            returnString += ColumnsRead[i] + ": " + value + "\n";
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

        static void WriteNewRowsToTable(SQLiteConnection conn, string TableName, List<string> Columns, List<string> Values)
        {
                try
                {
                    string command = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", TableName, string.Join(", ", Columns), string.Join(", ", Values));
                    
                    //Debug
                    //Console.WriteLine(command);
                    
                    new SQLiteCommand(command, conn).ExecuteNonQuery();
                    DisplaySuccessMessage("Command executed successfully");
                }
                catch (Exception e)
                {
                    DisplayErrorMessage(e.Message);
                    return;
                }
        }

        static void DeleteRowFromTable(SQLiteConnection conn, string TableName, List<string> Columns, List<string> Values)
        {
            List<string> equals = new List<string>(Columns.Count);

            for (int i = 0; i < Columns.Count; i++)
            {
                equals.Add(string.Format("{0}={1}", Columns[i], Values[i]));
            }

            string command = string.Format("DELETE FROM {0} WHERE {1};", TableName, string.Join(" AND ", equals));

            //Debug
            //Console.WriteLine(command);

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
                if (!ynHelper("Are you sure you want to DROP TABLE " + TableName + "?", true)) { throw new OperationCanceledException(); }
 
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
                if (!ynHelper("Are you sure you want to TRUNCATE TABLE " + TableName + "?", true)) { throw new OperationCanceledException(); }
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

        static List<string> FindColumnNames(SQLiteConnection conn, string TableName, bool Write)
        {
            List<String> ColumnNames = new List<string>();

            try
            {
                using (SQLiteDataReader datareader = new SQLiteCommand("PRAGMA table_info(" + TableName + ");", conn).ExecuteReader())
                {
                    while (datareader.Read())
                    {
                        ColumnNames.Add(datareader["name"].ToString());

                        if (Write) Console.WriteLine(datareader["name"].ToString());
                    }

                    //Console.WriteLine("Command executed successfully");
                }
            }
            catch (Exception e)
            {
                DisplayErrorMessage(e.Message);
                return new List<string>(0);
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
            List<string> ColumnsToCheck = new List<string>(FindColumnNames(conn, TableToCheck, false));

            //Debug
            //foreach(string s in ColumnsToCheck)
            //Console.WriteLine(s);

            string PKColumn = FindPKColumnName(conn, TableToCheck, false);

            if (string.IsNullOrEmpty(PKColumn) || string.IsNullOrWhiteSpace(PKColumn))
            {
                DisplayWarningMessage("Table doesn't have a Primary Key column. Aborting...");
                throw new OperationCanceledException();
            }

            ColumnsToCheck.Remove(PKColumn);

            string read = ReadFromTable(conn, false, TableToCheck, ColumnsToCheck);
            
            string[] lines = read.Split('\n');

            int numTotalRows = lines.Length/ColumnsToCheck.Count;

            string[] rowsStrArray = new string[ numTotalRows ];

            for (int i = 0; i < numTotalRows; i++)
            {
                for (int j = 0; j < ColumnsToCheck.Count; j++)
                {
                    rowsStrArray[i] += lines[(i * ColumnsToCheck.Count) + j] + ((j == ColumnsToCheck.Count - 1) ? "" : "\n");
                }
            }

            List<string> rows = new List<string>(rowsStrArray);

            for (int i = 0; i < rows.Count - 1; i++)
            {
                for (int j = i + 1; j < rows.Count; j++)
                {
                    //Debug
                    //if (j == rows.Count - 1)
                      //  Console.Write(rows[j]);

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

                            List<string> values = new List<string>(ColumnsToCheck);
                            string[] rowLines = rows[j].Split('\n');

                            for (int k = 0; k < values.Count; k++)
                            {
                                values[k] = rowLines[k].Split(new string[] { ": " }, StringSplitOptions.None)[1];

                                //Debug
                                //Console.WriteLine(rowLines[k]);
                                //Console.WriteLine(values[k]);
                            }

                            string[] PKLines = ReadFromTableWhere(conn, false, TableToCheck, new List<string> {PKColumn}, ColumnsToCheck, values).Split('\n');
                            string PKLineSecondRow = PKLines[1];

                            //Debug
                            //Console.WriteLine(PKLineSecondRow);

                            string PKValueSecondRow = PKLineSecondRow.Split(new string[] {": "}, StringSplitOptions.None)[1];

                            //Debug
                            //Console.WriteLine(PKValueSecondRow);

                            try
                            {
                                DeleteRowFromTable(conn, TableToCheck, new List<string> { PKColumn }, new List<string> { PKValueSecondRow });

                                rows.RemoveAt(j);
                                j--;

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
                List<string> Columns = FindColumnNames(conn, TableName, false);

                //Debug
                //foreach (string s in Columns)
                //Console.WriteLine(s);

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

        static List<string> AskUserForStrArray(bool col, bool row, bool AskUserForNumItems, int? numItems)
        {
            if (AskUserForNumItems) numItems = AskUserForNumberOfItems(col, row);

            string thingToAskFor = "";

            if (col) thingToAskFor = "column";
            else if (row) thingToAskFor = "row";
            else thingToAskFor = "value";

            List<string> strArray = new List<string>();

            try
            {
                for (int i = 0; i < numItems; i++)
                {

                askforcolumns:
                    Console.Write((i + 1) + ". " + thingToAskFor + ": ");
                    string input = ReadLineWithEscape();
                    if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Please enter a " + thingToAskFor);
                        goto askforcolumns;
                    }
                    strArray.Add(input);
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
                    DisplayWarningMessage("\n\nOperation cancelled.");
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

                    Console.WriteLine("[{0}] Use existing table\n[{1}] Create table\n[{2}] Read from table\n[{3}] Read from table with WHERE conditions\n[{4}] Read entire table\n[{5}] Insert into table\n[{6}] Insert same row multiple times\n[{7}] Delete from table\n[{8}] DROP TABLE\n[{9}] TRUNCATE TABLE\n[{10}] Check table for duplicate rows\n[{11}] View column names of table\n[{12}] View Primary Key of table\n[{13}] Clear console screen\n[e] Execute non-query command\n[ESC] Cancel out of an operation\n",
                        useTable, createTable, readFromTable, readFromTableWhere, readWholeTable, insertIntoTable, insertDuplicate, deleteFromTable, dropTable, truncateTable, checkTableForDuplicates, viewColumnNames, viewPKColumnName, clrScreen);

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
                    else if (ck != useTable && ck != viewColumnNames && ck != viewPKColumnName && ck != clrScreen && ck != 'e' && ck != checkTableForDuplicates)
                    {
                        Console.WriteLine("Invalid selection.\n");
                        goto start;
                    }

                    if (ck == useTable) SetCurrentTable(conn, true);
                    else if (ck != clrScreen && ck != createTable && ck != 'e') SetCurrentTable(conn, false);

                    switch (ck)
                    {
                        case createTable:
                            DisplayWarningMessage("CREATING TABLE");
                            Console.Write("Enter table name to create: ");
                            string table_name = ReadLineWithEscape();
                            Console.WriteLine("Declare columns");
                            List<string> columns = AskUserForStrArray(true, false, true, null);

                            CreateTableIfNotExists(conn, table_name, columns);
                            break;

                        case readFromTable:
                            DisplayWarningMessage("READING FROM TABLE " + defaultTable);
                            Console.WriteLine("Specify the columns you want to read");
                            List<string> colsToRead = AskUserForStrArray(true, false, true, null);
                            ReadFromTable(conn, true, defaultTable, colsToRead);
                            break;

                        case readFromTableWhere:
                            DisplayWarningMessage("READING FROM TABLE " + defaultTable);
                            Console.WriteLine("Specify the columns you want to read");
                            colsToRead = AskUserForStrArray(true, false, true, null);

                            Console.WriteLine("\nSpecify the columns to be included in the search query");
                            List<string> colsConditions = AskUserForStrArray(true, false, true, null);

                            List<string> values = new List<string>();
                            Console.WriteLine("Specify the values to search for");
                            try
                            {
                                for (int i = 0; i < colsConditions.Count; i++)
                                {
                                    Console.Write(colsConditions[i] + "=");
                                    values.Add(ReadLineWithEscape());
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
                            DisplayWarningMessage("READING ALL ROWS FROM TABLE " + defaultTable + "\n");
                            ReadWholeTable(conn, defaultTable, true);
                            break;

                        case insertIntoTable:
                            DisplayWarningMessage("INSERTING INTO TABLE " + defaultTable);

                            int numCols = 0;
                            List<string> columnNames = new List<string>();

                            Console.WriteLine("Specify how many rows to insert");
                            int numRows = AskUserForNumberOfItems(false, true);

                            for (int i = 0; i < numRows; i++)
                            {
                                Console.WriteLine("Specify the number of columns to insert");
                                numCols = AskUserForNumberOfItems(true, false);

                                Console.WriteLine("Specify columns to insert");
                                columnNames = AskUserForStrArray(true, false, false, numCols);

                                Console.WriteLine("Specify values to insert");
                                values = AskUserForStrArray(false, false, false, numCols);

                                WriteNewRowsToTable(conn, defaultTable, columnNames, values);
                            }
                            break;

                        case insertDuplicate:
                            DisplayWarningMessage("INSERTING DUPLICATE ROWS INTO TABLE " + defaultTable);

                            Console.WriteLine("Specify how many times (rows) to insert");
                            numRows = AskUserForNumberOfItems(false, true);

                            Console.WriteLine("Specify the number of columns to insert");
                            numCols = AskUserForNumberOfItems(true, false);

                            Console.WriteLine("Specify columns to insert");
                            columnNames = AskUserForStrArray(true, false, false, numCols);

                            Console.WriteLine("Specify values to insert");
                            values = AskUserForStrArray(false, false, false, numCols);

                            for (int i = 0; i < numRows; i++)
                            {
                                WriteNewRowsToTable(conn, defaultTable, columnNames, values);
                            }
                            break;

                        case deleteFromTable:
                            DisplayWarningMessage("DELETING FROM TABLE " + defaultTable);
                            Console.WriteLine("Specify which columns will be in the WHERE statement");
                            List<string> Columns = AskUserForStrArray(true, false, true, null);
                            DeleteRowFromTable(conn, defaultTable, Columns, AskUserForStrArray(false, false, false, Columns.Count));
                            break;

                        case dropTable:
                            DisplayErrorMessage("DROPPING TABLE " + defaultTable);
                            DropTable(conn, defaultTable);
                            defaultTable = null;
                            break;

                        case truncateTable:
                            DisplayErrorMessage("TRUNCATING TABLE " + defaultTable);
                            TruncateTable(conn, defaultTable);
                            break;

                        case checkTableForDuplicates:
                            DisplayWarningMessage("CHECKING TABLE " + defaultTable + " FOR DUPLICATES");

                            bool writeLog = ynHelper("Write progress on screen?", false),
                                deleteDuplicates = ynHelper("Delete duplicate rows when they're found?", false);

                            CheckTableForDuplicateRows(conn, defaultTable, writeLog, deleteDuplicates);
                            break;

                        case viewColumnNames:
                            DisplayWarningMessage("VIEWING COLUMN NAMES OF TABLE " + defaultTable);
                            FindColumnNames(conn, defaultTable, true);
                            break;

                        case viewPKColumnName:
                            DisplayWarningMessage("VIEWING PRIMARY KEY COLUMN NAME OF TABLE " + defaultTable);
                            FindPKColumnName(conn, defaultTable, true);
                            break;

                        case clrScreen:
                            Console.Clear();
                            break;

                        case 'e':
                            DisplayWarningMessage("EXECUTING NON-QUERY COMMAND");
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
