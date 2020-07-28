using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JohnsCodeBlocks
{
	public class SqlHelpers
	{
		public string ServerName { get; set; }
		public string DbName { get; set; }
		public string DbUserName { get; set; }
		public string DbUserPassword { get; set; }
		public int TimeoutSeconds { get; set; }
		public string SqlConnectionString { get; set; }
		public SqlParameterCollection SqlParameters { get; set; }
		public string StoredProcName { get; set; }
		public string SqlScriptText { get; set; }

		public SqlHelpers()	{ }
		public SqlHelpers(string _serverName, string _dbName, string _dbUserName, string _dbUserPassword, int _timeoutSeconds = 120)
		{
			ServerName = _serverName;
			DbName = _dbName;
			DbUserName = _dbUserName;
			DbUserPassword = _dbUserPassword;
			TimeoutSeconds = _timeoutSeconds;
			CreateConnectionString();
		}
		public void CreateConnectionString()
		{
			string conStrng = "Data Source = " + ServerName + "; " +
			"Connection Timeout = " + TimeoutSeconds.ToString() + "; " +
			"Initial Catalog = " + DbName + "; " +
			"Persist Security Info = True; " +
			"User ID = " + DbUserName + "; " +
			"Password = " + DbUserPassword +
			"";
			SqlConnectionString = conStrng;
		}
		public DataTable GetDataTableFromStoredProc(string procName = null, SqlParameterCollection parameters = null)
		{
			DataTable dt = new DataTable();
			if (procName != null) { StoredProcName = procName; }
			if (parameters != null) { SqlParameters = parameters; }
			if (StoredProcName == null)
			{
				dt = Helpers.CreateErrorTable("The stored procedure was not specified.  Aborting.");
				return dt;
			}
			using (SqlConnection conn = new SqlConnection(SqlConnectionString))
			{
				using (SqlCommand cmd = new SqlCommand(StoredProcName, conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					if (SqlParameters != null)
					{
						cmd.Parameters.Add(SqlParameters);
					}
					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
					{
						try
						{
							cmd.Connection.Open();
							da.Fill(dt);
							dt.TableName = "SUCCESS!";
						}
						catch (Exception ex)
						{
							dt = Helpers.CreateErrorTable(ex);
						}
						finally
						{
							cmd.Connection.Close();
							cmd.Connection.Dispose();
						}
					}
				}
			}
			return dt;
		}
		public void AddSqlParameters(string parameterName, object parameterValue)
		{
			SqlParameter p = new SqlParameter(parameterName, parameterValue);
			SqlParameters.Add(p);
		}
		public DataTable GetDataTableFromScript(string scriptText = null)
		{
			DataTable dt = new DataTable();
			if (scriptText != null) { SqlScriptText = scriptText; }
			if (SqlScriptText == null)
			{
				dt = Helpers.CreateErrorTable("No sql script text specified.  Aborting.");
				return dt;
			}
			using (SqlConnection conn = new SqlConnection(SqlConnectionString))
			{
				using (SqlCommand cmd = new SqlCommand(scriptText, conn))
				{
					cmd.CommandType = CommandType.Text;
					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
					{
						try
						{
							cmd.Connection.Open();
							da.Fill(dt);
							dt.TableName = "SUCCESS!";
						}
						catch (Exception ex)
						{
							dt = Helpers.CreateErrorTable(ex);
						}
						finally
						{
							cmd.Connection.Close();
							cmd.Connection.Dispose();
						}
					}
				}
			}
			return dt;
		}
		public string ExecuteSqlProcNoReturnData(string procName = null, SqlParameterCollection parameters = null)
		{
			string responseMessage;
			DataTable dt = new DataTable();
			if (procName != null) { StoredProcName = procName; }
			if (parameters != null) { SqlParameters = parameters; }
			if (StoredProcName == null)
			{
				responseMessage = "ERROR!|The Stored procedure was not specified. Aborting.|n/a";
				return responseMessage;
			}
			using (SqlConnection conn = new SqlConnection(SqlConnectionString))
			{
				using (SqlCommand cmd = new SqlCommand(StoredProcName, conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					if (SqlParameters != null)
					{
						cmd.Parameters.Add(SqlParameters);
					}
					try
					{
						cmd.Connection.Open();
						int rowCount = cmd.ExecuteNonQuery();
						responseMessage = "SUCCESS!|Rows affected: " + rowCount.ToString("N0") + "|n/a";
					}
					catch (Exception ex)
					{
						responseMessage = "ERROR!|" + ex.Message + "|" + ex.ToString();
					}
					finally
					{
						cmd.Connection.Close();
						cmd.Connection.Dispose();
					}
				}
			}
			return responseMessage;
		}
		public string ExecuteSqlScriptNoReturnData(string scriptText = null)
		{
			string responseMessage;
			DataTable dt = new DataTable();
			if (scriptText != null) { SqlScriptText = scriptText; }
			if (SqlScriptText == null)
			{
				responseMessage = "ERROR!|No sql script text was specified. Aborting.|n/a";
				return responseMessage;
			}
			using (SqlConnection conn = new SqlConnection(SqlConnectionString))
			{
				using (SqlCommand cmd = new SqlCommand(StoredProcName, conn))
				{
					cmd.CommandType = CommandType.Text;
					try
					{
						cmd.Connection.Open();
						int rowCount = cmd.ExecuteNonQuery();
						responseMessage = "SUCCESS!|Rows affected: " + rowCount.ToString("N0") + "|n/a";
					}
					catch (Exception ex)
					{
						responseMessage = "ERROR!|" + ex.Message + "|" + ex.ToString();
					}
					finally
					{
						cmd.Connection.Close();
						cmd.Connection.Dispose();
					}
				}
			}
			return responseMessage;
		}

	}
}
