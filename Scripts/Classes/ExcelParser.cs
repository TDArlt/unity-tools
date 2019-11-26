using System;
using System.Collections.Generic;
using System.Xml;

namespace unexpected
{
    /// <summary>
    /// This is a parser class for Excel-xml-files.
    /// It will also allow you to load a full excel-document (consisting of multiple sheets) and have its content accessible without long searching everytime.
    /// Your excel-table must have text at the first row and in every first column, otherwise the cells for this area will not be recognized.
    /// You can then take a table and get each element by using Rowname + Columname.
    /// On top of that, you can get an XML out of a table again which can be saved. As this here only looks at the content of cells, you might also use this class
    /// to clean up excel documents?
    /// 
    /// Usage:
    /// - Parse from a string using ExcelParser.ParseExcelXML(string) and get a list of sheets.
    /// - The single sheets can be accessed using sheets["mySheetName"], where the names are equal to the ones given in Excel.
    /// - Each sheet contains an ExcelParser.Table<string>-Instance.
    /// - Get a value you like to have from this instance using table.GetValue("myRow", "myColumn"), where these two names
    ///   match the ones locating them in your excel table.
    /// - Get the list of all row-titles via table.RowTitles and the list of all column-titles using table.ColumnTitles.
    /// - Add or set a new value using table.SetValue("myRow", "myColumn", "newValue"). If there is no such row or column,
    ///   it will be created automatically, so don't bother about them.
    /// - You can also remove rows and columns, but why?
    /// 
    /// Notes:
    /// - The parser will not get any xml-tags used inside a cell. So any kind of formatting (font, color, bold, italics,...) will be lost when loading.
    /// - If you try to get values that do not exist, this class will fire a ValueNotFoundException having a "Type"-parameter that tells you what
    ///   went wrong exactly. If you don't know exactly about your content, don't check everytime but use try & catch.
    /// 
    /// 
    /// v1.0, 2015/09
    /// Written by Chris Arlt, c.arlt@unexpected.de
    /// </summary>
    class ExcelParser
    {
        /// <summary>
        /// This is an excel table which is not sorted by anything. It's used to access values fast and directly only.
        /// </summary>
        /// <typeparam name="T">This is the data type of the content of a Table</typeparam>
        public class Table<T>
        {
            /// <summary>The internal data structure for our table</summary>
            private SortedList<string, SortedList<string, T>> table;
            private SortedList<string, SortedList<string, T>> ReturnTable { get { return table; } }

            /// <summary>All the row titles that exist here</summary>
            private string[] rowTitles;
            /// <summary>All the column titles that exist here</summary>
            private string[] columnTitles;

            /// <summary>All the row titles that exist in this table</summary>
            public string[] RowTitles { get { return rowTitles; } }
            /// <summary>All the column titles that exist in this table</summary>
            public string[] ColumnTitles { get { return columnTitles; } }


            /// <summary>Create an instance of a table</summary>
            public Table()
            {
                table = new SortedList<string, SortedList<string, T>>();
                rowTitles = new string[0];
                columnTitles = new string[0];
            }


            /// <summary>
            /// Get a value on a distinct position in the table
            /// </summary>
            /// <param name="row">The desired row</param>
            /// <param name="column">The desired column</param>
            /// <returns>The value</returns>
            /// <exception cref="ValueNotFoundException">If the value cannot be found (no such row or no such column), this exception will be thrown and its Type will tell you, what's wrong.</exception>
            public T GetValue(string row, string column)
            {
                if (ReturnTable.ContainsKey(row))
                {
                    SortedList<string, T> tableRow = ReturnTable[row];
                    if (tableRow.ContainsKey(column))
                        return tableRow[column];
                    else
                        throw new ValueNotFoundException(ValueNotFoundException.ExceptionType.NoColumn);
                }
                else
                    throw new ValueNotFoundException(ValueNotFoundException.ExceptionType.NoRow);
            }


            /// <summary>
            /// This sets a value at a distinct position in the table. Will override existing values or add a new one.
            /// </summary>
            /// <param name="row">This is the row where to place the new value</param>
            /// <param name="column">This is the column where to place the new value</param>
            /// <param name="value">Is the value to write in</param>
            public void SetValue(string row, string column, T value)
            {
                // Add row & column, if necessary (will not be added, if they already exist)
                AddRow(row);
                AddColumn(column);

                // Set value at the correct position
                table[row][column] = value;
            }




            /// <summary>This adds a new empty column to our table or does nothing, if this column already exists</summary>
            /// <param name="columnTitle">This is the title of the new column</param>
            public void AddColumn(string columnTitle)
            {
                // Column does NOT exist
                if (!ExistsInArray(ColumnTitles, columnTitle))
                {
                    // add this column to our array
                    Array.Resize<string>(ref columnTitles, ColumnTitles.Length + 1);
                    columnTitles[ColumnTitles.Length - 1] = columnTitle;

                    // Go through each row and add this column
                    foreach (string aRow in RowTitles)
                        table[aRow].Add(columnTitle, default(T));
                }
            }

            /// <summary>This adds a new empty row to our table or does nothing, if this row already exists</summary>
            /// <param name="rowTitle">This is the title of the new row</param>
            public void AddRow(string rowTitle)
            {
                // Row does NOT exist
                if (!ExistsInArray(RowTitles, rowTitle))
                {
                    // add this row to our array
                    Array.Resize<string>(ref rowTitles, RowTitles.Length + 1);
                    rowTitles[RowTitles.Length - 1] = rowTitle;

                    // Generate the new row
                    SortedList<string, T> newRow = new SortedList<string, T>();
                    foreach (string aColumn in ColumnTitles)
                        newRow.Add(aColumn, default(T));

                    // Add the new row to our list
                    table.Add(rowTitle, newRow);
                }
            }


            /// <summary>This removes a column completely from the table</summary>
            /// <param name="columnTitle">This is the title of the column</param>
            public void RemoveColumn(string columnTitle)
            {
                // Only work, if there is a column with that name
                if (ExistsInArray(ColumnTitles, columnTitle))
                {
                    // Build new array for the columns
                    string[] newColumns = new string[ColumnTitles.Length - 1];

                    int newI = 0;
                    for (int i = 0; i < ColumnTitles.Length; i++)
                    {
                        if (!ColumnTitles[i].Equals(columnTitle))
                        {
                            newColumns[newI] = ColumnTitles[i];
                            newI++;
                        }
                    }
                    columnTitles = newColumns;

                    // Go through each row and remove this column
                    foreach (string aRow in RowTitles)
                        table[aRow].Remove(columnTitle);
                }
            }

            /// <summary>This removes a row completely from the table</summary>
            /// <param name="rowTitle">This is the title of the row</param>
            public void RemoveRow(string rowTitle)
            {
                // Only work, if there is a column with that name
                if (ExistsInArray(RowTitles, rowTitle))
                {
                    // Build new array for the rows
                    string[] newRows = new string[RowTitles.Length - 1];

                    int newI = 0;
                    for (int i = 0; i < RowTitles.Length; i++)
                    {
                        if (!RowTitles[i].Equals(rowTitle))
                        {
                            newRows[newI] = ColumnTitles[i];
                            newI++;
                        }
                    }
                    rowTitles = newRows;

                    // Remove row
                    table.Remove(rowTitle);
                }
            }



            /// <summary>Get the content of this table in a nice formatted string</summary>
            public override string ToString()
            {
                string output = "";

                // Print all columns in first line
                foreach (string aColumn in ColumnTitles)
                {
                    output += "\t" + aColumn;
                }

                // Go through all rows
                foreach (string aRow in RowTitles)
                {
                    output += "\n" + aRow;

                    foreach (string aColumn in ColumnTitles)
                    {
                        T aVal = GetValue(aRow, aColumn);
                        if (aVal != null)
                            output += "\t" + aVal.ToString();
                        else
                            output += "\t ";
                    }
                }

                return output;
            }

            /// <summary>Converts this table back into an excel-readable xml</summary>
            /// <param name="doc">This is the document that should be used for creating nodes</param>
            /// <returns>An xml node with the desired content</returns>
            public XmlNode ToExcelXML(XmlDocument doc)
            {
                // This is our root node to be saved
                XmlNode output = doc.CreateElement("Table");

                // This is our current row we like to save
                XmlNode currentRow = doc.CreateElement("Row");

                // This is the current cell we like to save
                XmlNode currentCell = doc.CreateElement("Cell");
                // This is the real data
                XmlNode currentData = doc.CreateElement("Data");
                XmlAttribute attribute = doc.CreateAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet");
                attribute.Value = "String";
                currentData.Attributes.Append(attribute);

                // Write one empty cell

                // Fill in content
                currentData.InnerText = "-";

                // Connect nodes
                currentCell.AppendChild(currentData);
                currentRow.AppendChild(currentCell);

                // Write first line: all column titles
                foreach (string aColumn in ColumnTitles)
                {
                    // Create new nodes
                    currentCell = doc.CreateElement("Cell");
                    currentData = doc.CreateElement("Data");
                    attribute = doc.CreateAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet");
                    attribute.Value = "String";
                    currentData.Attributes.Append(attribute);

                    // Fill in content
                    currentData.AppendChild(doc.CreateCDataSection(aColumn));

                    // Connect nodes
                    currentCell.AppendChild(currentData);
                    currentRow.AppendChild(currentCell);
                }

                // Connect row
                output.AppendChild(currentRow);


                // Now, write real content. Go through all rows
                foreach (string aRow in RowTitles)
                {
                    // Create new node
                    currentRow = doc.CreateElement("Row");

                    // Create row title
                    currentCell = doc.CreateElement("Cell");
                    currentData = doc.CreateElement("Data");
                    attribute = doc.CreateAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet");
                    attribute.Value = "String";
                    currentData.Attributes.Append(attribute);

                    // Fill in content
                    currentData.AppendChild(doc.CreateCDataSection(aRow));

                    // Connect nodes
                    currentCell.AppendChild(currentData);
                    currentRow.AppendChild(currentCell);


                    // Go through all columns
                    foreach (string aColumn in ColumnTitles)
                    {
                        // Create new nodes
                        currentCell = doc.CreateElement("Cell");
                        currentData = doc.CreateElement("Data");
                        attribute = doc.CreateAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet");
                        attribute.Value = "String";
                        currentData.Attributes.Append(attribute);

                        // Fill in content
                        if (GetValue(aRow, aColumn) != null)
                            currentData.AppendChild(doc.CreateCDataSection(GetValue(aRow, aColumn).ToString()));
                        else
                            currentData.InnerText = "-";

                        // Connect nodes
                        currentCell.AppendChild(currentData);
                        currentRow.AppendChild(currentCell);
                    }

                    // Connect row
                    output.AppendChild(currentRow);
                }

                return output;
            }

        }

        /// <summary>
        /// This is a class for Excel-sheets (pages) which contain a table.
        /// </summary>
        public class Sheets : SortedList<string, Table<string>>
        {
            public Sheets() : base() {; }

            /// <summary>Get a table by sheet name</summary>
            /// <param name="name">The name of the sheet</param>
            public Table<string> Get(string name)
            {
                if (this.ContainsKey(name))
                    return this[name];
                else
                    throw new ValueNotFoundException(ValueNotFoundException.ExceptionType.NoTable);
            }


            /// <summary>Get a table by sheet name</summary>
            /// <param name="index">The index of the sheet</param>
            public Table<string> Get(int index)
            {
                // Find out, if this is in range
                if (index >= this.Count || index < 0)
                    throw new IndexOutOfRangeException();

                // Now, get the correct one
                return this[this.Keys[index]];
            }


            /// <summary>Print into a readable format</summary>
            public override string ToString()
            {
                string output = "";

                // Go through all sheets & print them
                foreach (string aKey in this.Keys)
                {
                    output += "===========  " + aKey + "  ===========\n";
                    output += this[aKey].ToString();
                    output += "\n\n";
                }

                return output;
            }

            /// <summary>Converts this table back into an excel-readable xml</summary>
            /// <returns>A long string of all the content, correctly formatted.</returns>
            public XmlDocument ToExcelXML()
            {
                XmlDocument output = new XmlDocument();
                XmlNode rootNode = output.CreateElement("Workbook");

                // Set general properties for output



                // Add declaration
                XmlNode docNode = output.CreateXmlDeclaration("1.0", null, null);
                output.AppendChild(docNode);
                XmlProcessingInstruction pi = output.CreateProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
                output.AppendChild(pi);

                // Add default attributes
                XmlAttribute attribute = output.CreateAttribute("xmlns");
                attribute.Value = "urn:schemas-microsoft-com:office:spreadsheet";
                rootNode.Attributes.Append(attribute);
                attribute = output.CreateAttribute("xmlns:o");
                attribute.Value = "urn:schemas-microsoft-com:office:office";
                rootNode.Attributes.Append(attribute);
                attribute = output.CreateAttribute("xmlns:x");
                attribute.Value = "urn:schemas-microsoft-com:office:excel";
                rootNode.Attributes.Append(attribute);
                attribute = output.CreateAttribute("xmlns:ss");
                attribute.Value = "urn:schemas-microsoft-com:office:spreadsheet";
                rootNode.Attributes.Append(attribute);
                attribute = output.CreateAttribute("xmlns:html");
                attribute.Value = "http://www.w3.org/TR/REC-html40";
                rootNode.Attributes.Append(attribute);


                foreach (string key in this.Keys)
                {
                    // Create node
                    XmlNode worksheet = output.CreateElement("Worksheet");

                    // Set attributes
                    attribute = output.CreateAttribute("Name", "urn:schemas-microsoft-com:office:spreadsheet");
                    attribute.Value = key;
                    worksheet.Attributes.Append(attribute);

                    // add content
                    worksheet.AppendChild(this[key].ToExcelXML(output));

                    // Link to root node
                    rootNode.AppendChild(worksheet);
                }

                // Add root to our document
                output.AppendChild(rootNode);


                return output;
            }

        }




        /// <summary>This parses a string into an excel-sheets-instance</summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Sheets ParseExcelXML(string input)
        {
            Sheets output = new Sheets();
            int sheetCounter = 0;

            // Load string into XML object
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);

            // Get sheets that might exist
            XmlNodeList possibleSheets = doc["Workbook"].ChildNodes;
            foreach (XmlNode aPossibleSheet in possibleSheets)
            {
                // Only look at so called "work sheets" = sheets
                if (aPossibleSheet.Name.ToLower().Equals("worksheet"))
                {
                    // This sheet has a name
                    string sheetName = sheetCounter.ToString();
                    if (aPossibleSheet.Attributes["ss:Name"] != null)
                        sheetName = aPossibleSheet.Attributes["ss:Name"].Value;

                    sheetCounter++;

                    // Create a table
                    Table<string> aTable = new Table<string>();

                    // Go through the table
                    XmlNodeList rows = aPossibleSheet["Table"].ChildNodes;
                    List<string> columnTitles = new List<string>();
                    bool firstRow = true;

                    foreach (XmlNode aRow in rows)
                    {
                        // Only look at real rows
                        if (aRow.Name.ToLower().Equals("row"))
                        {
                            //Debugger.addMessage(Debugger.LEVEL_LOWEST, "New row");
                            int columnCounter = 0;

                            bool firstColumn = true;
                            string rowName = "";

                            // and go through all the columns
                            foreach (XmlNode aCell in aRow.ChildNodes)
                            {
                                // If this is the first element, check, if everything is correct or we might need to skip this row
                                if (firstColumn && !firstRow && aCell.Name.ToLower().Equals("cell"))
                                {
                                    // Find out, if we need to skip this row and skip it...
                                    if (aCell["Data"] == null || aCell["Data"].InnerText.Trim().Equals("") || ExistsInArray(aTable.RowTitles, aCell["Data"].InnerText))
                                        break;

                                    // Usually, set title
                                    rowName = aCell["Data"].InnerText;
                                }

                                // Only look at real cells:
                                if (aCell.Name.ToLower().Equals("cell"))
                                {
                                    //Debugger.addMessage(Debugger.LEVEL_LOWEST, "Content: " + rowName + ": '" + aCell.InnerText + "'\nCurrent column: " + columnCounter.ToString() + "\t" + "firstColumn: " + firstColumn.ToString() + "\t" + "firstRow: " + firstRow.ToString() + "\t" + "columnTitles.Count: " + columnTitles.Count.ToString());

                                    // Get current content, if possible
                                    XmlNode currentContent = aCell["Data"];

                                    // If there is no valid content in the "Data"-Tag, perhaps there is an "ss:Data"-tag?
                                    if (currentContent == null)
                                        currentContent = aCell["ss:Data"];




                                    // in first column...
                                    if (!firstColumn)
                                    {
                                        // Only save content that have a heading
                                        if (firstRow || columnCounter < columnTitles.Count)
                                        {
                                            // For all other columns
                                            // In first row...
                                            if (firstRow)
                                            {
                                                // Add this one to our column list, but only if this is a valid content
                                                if (currentContent != null)
                                                    columnTitles.Add(currentContent.InnerText);
                                            }
                                            else
                                            {
                                                if (aCell.Attributes["ss:Index"] != null)
                                                {
                                                    int desiredPos = int.Parse(aCell.Attributes["ss:Index"].Value.Trim()) - 2;
                                                    if (desiredPos < columnTitles.Count)
                                                        columnCounter = desiredPos;
                                                }


                                                // Don't save a cell that has no content
                                                if (currentContent != null)
                                                {
                                                    //Debugger.addMessage(Debugger.LEVEL_LOWEST, "Saving: " + rowName + "/" + columnTitles.ToArray()[columnCounter] + ": '" + currentContent.InnerText + "'");

                                                    // In all usual rows...
                                                    // We know the row name (rowName) and the column name is from the columnTitles-array the position at which we are now
                                                    aTable.SetValue(rowName, columnTitles.ToArray()[columnCounter], currentContent.InnerText);
                                                }
                                            }

                                            columnCounter++;
                                        }
                                    }

                                    firstColumn = false;
                                }

                                // Set first column end, if we were in it at the top
                                if (firstColumn && !firstRow && aCell.Name.ToLower().Equals("cell"))
                                {
                                    firstColumn = false;
                                }
                            }

                            // Now, the first row must be finished
                            firstRow = false;
                        }
                    }

                    // Now, we have the table for this sheet. Add it.
                    if (!output.ContainsKey(sheetName))
                        output.Add(sheetName, aTable);

                }
            }




            return output;
        }


        /// <summary>The exception when anything went wrong with the Table</summary>
        public class ValueNotFoundException : Exception
        {
            /// <summary>The possible exceptions that may happen</summary>
            public enum ExceptionType
            {
                /// <summary>This is the exception type, if this table does not exist</summary>
                NoTable,
                /// <summary>This is the exception type, if the row does not exist</summary>
                NoRow,
                /// <summary>This is the exception type, if the column does not exist</summary>
                NoColumn
            }
            private ExceptionType type;
            /// <summary>This is the type of the current exception</summary>
            public ExceptionType Type { get { return type; } }

            public override String Message { get { return Type.ToString(); } }

            /// <summary>Create this exception</summary>
            /// <param name="type">This is the type of this exception</param>
            public ValueNotFoundException(ExceptionType type)
            {
                this.type = type;
            }
        }


        /// <summary> Checks, if an element already exists in an array </summary>
        /// <param name="array">The array to look in</param>
        /// <param name="search">The thing to search for</param>
        /// <returns></returns>
        private static bool ExistsInArray(object[] array, object search)
        {
            bool returnVal = false;
            foreach (object elem in array)
                if (elem.Equals(search))
                    returnVal = true;

            return returnVal;
        }
    }
}