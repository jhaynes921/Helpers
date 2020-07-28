using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace JohnsCodeBlocks
{
    public static class Helpers
    {
		#region Conversions
		public static int C_INT(string inputVal)
		{
			int outputVal;
			if (!int.TryParse(inputVal, out outputVal)) { outputVal = -99; }
			return outputVal;
		}
		public static long C_LONG(string inputVal)
		{
			long outputVal;
			if (!long.TryParse(inputVal, out outputVal)) { outputVal = -99; }
			return outputVal;
		}
		public static bool C_BOOL_T(string inputVal) //default val is true
		{
			bool outputVal;
			if (!bool.TryParse(inputVal, out outputVal)) { outputVal = false; }
			return outputVal;
		}
		public static bool C_BOOL_F(string inputVal) //default val is false
		{
			bool outputVal;
			if (!bool.TryParse(inputVal, out outputVal)) { outputVal = true; }
			return outputVal;
		}
		public static decimal C_DEC(string inputVal)
		{
			decimal outputVal;
			if (!decimal.TryParse(inputVal, out outputVal)) { outputVal = -1; }
			return outputVal;
		}
		public static DateTime C_DATE(string inputVal)
		{
			DateTime outputVal;
			if (!DateTime.TryParse(inputVal, out outputVal)) { outputVal = new DateTime(1900, 01, 01, 0, 0, 0); }
			return outputVal;
		}
		#endregion



		#region Other
		public static string GetConString(string serverName, string dbName, string dbUsername, string dbPassword, int timeoutVal = 120)
		{
			string conStrng = "Data Source = " + serverName + "; " +
			"Connection Timeout = " + timeoutVal.ToString() + "; " +
			"Initial Catalog = " + dbName + "; " +
			"Persist Security Info = True; " +
			"User ID = " + dbUsername + "; " +
			"Password = " + dbPassword +
			"";
			return conStrng;
		}
		public static DataTable CreateErrorTable(Exception ex)
		{
			DataTable dt = new DataTable();
			dt.TableName = "ERROR!";
			dt.Columns.Add("ErrorMessage", typeof(string));
			dt.Rows.Add(ex.ToString());
			return dt;
		}
		public static DataTable CreateErrorTable(string ErrorMessage)
		{
			DataTable dt = new DataTable();
			dt.TableName = "ERROR!";
			dt.Columns.Add("ErrorMessage", typeof(string));
			dt.Rows.Add(ErrorMessage);
			return dt;
		}
	
		#endregion

		#region Write to File
		public static string WriteDataTableToDelimitedFile(string filePath, DataTable dt, char delimChar = ',', bool fieldsQuoted = true, bool overWriteExisting = true)
		{
			int linesWritten = 0;
			try
			{
				if (File.Exists(filePath))
				{
					if (overWriteExisting == true) { File.Delete(filePath); }
					else
					{
						string pathOnly = Path.GetDirectoryName(filePath);
						string nameOnly = Path.GetFileNameWithoutExtension(filePath);
						string extension = Path.GetExtension(filePath);
						string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
						string newName = nameOnly + "_OLD_" + timeStamp + "." + extension; //do I need the period here?
						string newPath = Path.Combine(pathOnly, newName);
						File.Move(filePath, newPath);
					}
				}
			}
			catch (Exception ex)
			{
				string errMsg = "ERROR: There was an error trying to remove or rename an existing file: \r\n" + ex.ToString();
				return errMsg;
			}

			try
			{
				StringBuilder sbH = new StringBuilder();
				int colcount = dt.Columns.Count;
				if (colcount > 0) { colcount = colcount - 1; }
				for (int i = 0; i <= colcount; i++)
				{
					if (i > 0) { sbH.Append(delimChar); }
					string colName = dt.Columns[i].ColumnName;
					if (fieldsQuoted == true)
					{
						colName = "\"" + colName + "\"";
					}
					sbH.Append(colName);
				}
				using (StreamWriter sw = new StreamWriter(filePath))
				{
					sw.WriteLine(sbH.ToString());
					linesWritten++;
					foreach (DataRow dr in dt.Rows)
					{
						StringBuilder sbR = new StringBuilder();
						for (int i = 0; i <= colcount; i++)
						{
							if (i > 0) { sbR.Append(delimChar); }
							string colData = dr[i].ToString();
							colData = CleanString(colData);
							if (fieldsQuoted == true)
							{
								colData = "\"" + colData + "\"";
							}
							sbR.Append(colData);
						}
						sw.WriteLine(sbR.ToString());
						linesWritten++;
					}
				}
			}
			catch (Exception ex)
			{
				string errMsg = "ERROR: There was an error writing the output file (\"" + filePath + "\"): \r\n" + ex.ToString();
				return errMsg;
			}
			string outputMsg;
			outputMsg = "File \"" + filePath + "\" written successfully (" + linesWritten.ToString("N0") + " lines total).";
			return outputMsg;
		}
		#endregion

		#region Text Manipulations
		public static string CleanString(string inputText, bool EscapeQuotes = true, bool replaceCommas = true, bool replaceVerticalBars = true, bool inactivateHTML = true)
		{
			string outputText = inputText;
			if (EscapeQuotes == true)
			{
				outputText = outputText.Replace("\"", "&quot;");
				outputText = outputText.Replace("\'", "&#39;");
			}
			if (replaceVerticalBars == true)
			{
				outputText = outputText.Replace("|", "&#124;");
			}
			if (replaceCommas == true)
			{
				outputText = outputText.Replace(",", "&#44;");
			}
			outputText = outputText.Replace("\r\n", "<BR>");
			outputText = outputText.Replace("\r", "<CR>");
			outputText = outputText.Replace("\n", "<LF>");
			outputText = outputText.Replace("\t", "<TAB>");
			if (inactivateHTML == true)
			{
				outputText = outputText.Replace("<", "&LT;");
				outputText = outputText.Replace(">", "&GT;");
			}
			outputText = Regex.Replace(outputText, @"\s+", " ");
			return outputText;
		}
		#endregion

		#region Importing Data From Files
		public static DataTable ImportDataFromDelimitedFile(string filepath, char delimChar = ',', bool fieldsQuoted = false, bool columnNamesInFirstLine = true)
		{
			DataTable dt = new DataTable();

			using (StreamReader sw = new StreamReader(filepath))
			{
				int lineNbr = 0;
				string line;
				while ((line = sw.ReadLine()) != null)
				{
					lineNbr++;

					line = PreCleanLine(line, fieldsQuoted, delimChar); //should always return vertical bar separated data.

					string[] fields = line.Split('|');

					if (lineNbr == 1)
					{
						if (columnNamesInFirstLine == true)
						{
							foreach (string fieldName in fields)
							{
								dt.Columns.Add(fieldName, typeof(string));
							}
						}
						else
						{
							int colCount = fields.Length;
							if (colCount > 0)
							{
								for (int i = 0; i >= colCount - 1; i++)
								{
									int colNbr = i + 1;
									dt.Columns.Add("Column_" + colNbr.ToString(), typeof(string));
								}
							}
							else
							{
								dt = CreateErrorTable("No columns were found, aborting import.");
								return dt;
							}

							DataRow dr = dt.NewRow();
							int fieldNbr = 0;
							try
							{
								foreach (string fieldData in fields)
								{
									string thisField = CleanString(fieldData);
									dr[fieldNbr] = thisField;
									fieldNbr++;
								}
								dt.Rows.Add(dr);
							}
							catch (Exception ex)
							{
								//do nothing here, maybe log later?
							}
						}
					}
					else
					{
						DataRow dr = dt.NewRow();
						int fieldNbr = 0;
						try
						{
							foreach (string fieldData in fields)
							{
								string thisField = CleanString(fieldData);
								dr[fieldNbr] = thisField;
								fieldNbr++;
							}
							dt.Rows.Add(dr);
						}
						catch (Exception ex)
						{
							//do nothing here, maybe log later?
						}
					}
				}
			}
			return dt;
		}
		public static string PreCleanLine(string line, bool fieldsQuoted = false, char delimChar = ',')
		{
			if (delimChar != '|')
			{
				line = line.Replace("|", "&#124;");
			}
			if (fieldsQuoted == true)
			{
				if (line.Substring(0, 1) == "\"")
				{
					if (line.Length > 1)
					{
						line = line.Substring(1, line.Length - 1);
					}
				}
				if (line.Substring(line.Length - 1, 1) == "\"")
				{
					line = line.Substring(0, line.Length - 1);
				}
				line = line.Replace("\"" + delimChar + "\"", "|");
				line = line.Replace("\"" + delimChar, "|");
				line = line.Replace(delimChar + "\"", "|");
			}


			return line;
		}
		public static DataTable ImportCsvFile(string filePath, bool fieldsQuoted = true)
		{
			string fileDir = Path.GetDirectoryName(filePath);
			string errorPath = Path.Combine(fileDir, "Errors");
			if (!Directory.Exists(errorPath)) { Directory.CreateDirectory(errorPath); }
			string errorFileNameOnly = Path.GetFileNameWithoutExtension(filePath) + "_ERRORS.txt";
			string errorFileFullPath = Path.Combine(errorPath, errorFileNameOnly);
			DataTable dt = new DataTable();
			try
			{
				using (StreamReader sr = new StreamReader(filePath))
				{
					int lineNbr = 0;
					string line;
					while ((line = sr.ReadLine()) != null)
					{
						if (fieldsQuoted == true)
						{
							line = line.Replace("\"", "");
						}
						if (lineNbr == 0)
						{
							string[] fieldNames = line.Split(',');
							foreach (string fieldName in fieldNames)
							{
								dt.Columns.Add(fieldName, typeof(string));
							}
						}
						else
						{
							try
							{
								DataRow dr = dt.NewRow();
								string[] dataFields = line.Split(',');
								int fieldNbr = 0;
								foreach (string dataField in dataFields)
								{
									dr[fieldNbr] = dataField;
									fieldNbr++;
								}
								dt.Rows.Add(dr);

							}
							catch (Exception ex)
							{
								string errorMsg = "ERROR ON LINE " + lineNbr.ToString() + ":\t" + line + "\r\n" + ex.ToString();
								WriteToErrorFile(errorFileFullPath, errorMsg);
							}
						}
						lineNbr++;
					}
				}
				dt.TableName = Path.GetFileNameWithoutExtension(filePath);
			}
			catch (Exception ex)
			{
				string errorMsg = "THERE WAS AN ERROR IMPORTING THE DATATABLE:\r\n" + ex.ToString();
				WriteToErrorFile(errorFileFullPath, errorMsg);
				dt = CreateErrorTable(ex);
			}

			return dt;
		}
		public static void WriteToErrorFile(string filePath, string errorText)
		{
			if (!File.Exists(filePath))
			{
				using (StreamWriter sw = File.CreateText(filePath))
				{
					sw.Write(errorText);
				}
			}
			else
			{
				using (StreamWriter sw = File.AppendText(filePath))
				{
					sw.Write("\r\n\r\n" + errorText);
				}
			}
		}
		#endregion

	}
}
