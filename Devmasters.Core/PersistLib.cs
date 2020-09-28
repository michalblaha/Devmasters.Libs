using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;




namespace Devmasters
{
	/// <summary>
	/// persistence objektu serveru
	/// </summary>
	[Obsolete]
	public class PersistLib : IDisposable
	{
		/// <summary>
		/// delegat pro funkci cteni dat pro jeden zaznam
		/// </summary>
		/// <param name="read">ctecka</param>
		public delegate object ReadItemHandler(IDataReader read);

		// data
		private IDbConnection conn = null;				// aktualni spoj na db.

		#region Execute
		/// <summary>
		/// spusteni cteni na SQL databazi
		/// </summary>
		/// <param name="connName">jmeno spoje</param>
		/// <param name="typ">typ prikazu</param>
		/// <param name="text">prikaz</param>
		/// <param name="pars">parametry prikazu</param>
		/// <returns></returns>
		public IDataReader ExecuteReader(string connName, CommandType typ, string text, IDataParameter[] pars)
		{
			// nastaveni prikazu
			IDbCommand _comm = PrepareCommand(connName, typ, text, pars);

			// vykonec cteni
			try
			{
				_comm.Connection.Open();
				IDataReader _read = _comm.ExecuteReader();
				return _read;
			}
			catch (Exception e)
			{
                Devmasters.Logging.Logger.Root.Error(IDbCommandToString(_comm), e);
				throw;
			}
		}

		/// <summary>
		/// spusteni prikazu na SQL databazi
		/// </summary>
		/// <param name="connName">jmeno spoje</param>
		/// <param name="typ">typ prikazu</param>
		/// <param name="text">prikaz</param>
		/// <param name="pars">parametry prikazu</param>
		/// <returns></returns>
		public object ExecuteScalar(string connName, CommandType typ, string text, IDataParameter[] pars)
		{
			// nastaveni prikazu
			IDbCommand _comm = PrepareCommand(connName, typ, text, pars);

			// vykonec cteni
			object _obj = null;
			try
			{
				_comm.Connection.Open();
				_obj = _comm.ExecuteScalar();
				return _obj;
			}
			catch (Exception e)
			{
                Devmasters.Logging.Logger.Root.Error(IDbCommandToString(_comm), e);
				throw;
			}
			finally
			{
				// uzavreni spoje, neni-li
				if (_comm.Connection.State != ConnectionState.Closed)
					_comm.Connection.Close();
			}
		}

		/// <summary>
		/// spusteni prikazu bez odezvy na SQL databazi
		/// </summary>
		/// <param name="connName">jmeno spoje</param>
		/// <param name="typ">typ prikazu</param>
		/// <param name="text">prikaz</param>
		/// <param name="pars">parametry prikazu</param>
		/// <returns></returns>
		public void ExecuteNonQuery(string connName, CommandType typ, string text, IDataParameter[] pars)
		{
			// nastaveni prikazu
			IDbCommand _comm = PrepareCommand(connName, typ, text, pars);
			try
			{
				_comm.Connection.Open();
				_comm.ExecuteNonQuery();
			}
			catch (Exception e)
			{
                Devmasters.Logging.Logger.Root.Error(IDbCommandToString(_comm), e);
				throw;
			}
			finally
			{
				// uzavreni spoje, neni-li
				if (_comm.Connection.State != ConnectionState.Closed)
					_comm.Connection.Close();
			}
		}

		/// <summary>
		/// spusteni prikazu bez odezvy na SQL databazi
		/// </summary>
		/// <param name="connName">jmeno spoje</param>
		/// <param name="typ">typ prikazu</param>   
		/// <param name="text">prikaz</param>
		/// <param name="pars">parametry prikazu</param>
		/// <returns></returns>
		public DataSet ExecuteDataset(string connName, CommandType typ, string text, IDataParameter[] pars)
		{
			// nastaveni prikazu
			SqlCommand _comm = (SqlCommand)PrepareCommand(connName, typ, text, pars);
			DataSet _ds = new DataSet("result");
			try
			{
				_comm.Connection.Open();
				IDataAdapter _da = new SqlDataAdapter(_comm);
				_da.Fill(_ds);
			}
			catch (Exception e)
			{
                Devmasters.Logging.Logger.Root.Error(IDbCommandToString(_comm), e);
				throw;
			}
			finally
			{
				// uzavreni spoje, neni-li
				if (_comm.Connection.State != ConnectionState.Closed)
					_comm.Connection.Close();
			}

			return _ds;

		}

		#endregion

		#region Dispose
		/// <summary>
		/// uzavreni spoju
		/// </summary>
		public void Dispose()
		{
			// neni-li vubec pouzit, pak konec
			if (conn == null)
				return;

			// neni spoj uzavren ? zavrit ...
			if (conn.State != ConnectionState.Closed)
				conn.Close();

			conn.Dispose();
		}
		#endregion

		#region Helper
		/// <summary>
		/// priprava prikazu pro vykonani
		/// </summary>
		/// <param name="connName">jmeno spoje</param>
		/// <param name="typ">typ prikazu</param>
		/// <param name="text">prikaz</param>
		/// <param name="pars">parametry prikazu</param>
		/// <returns></returns>
		private IDbCommand PrepareCommand(string connStr, CommandType typ, string text, IDataParameter[] pars)
		{
			// vytahne spoj a inicializuje
			string _connStr = connStr;
			conn = new SqlConnection(_connStr);

			// nastaveni prikazu
			IDbCommand _comm = new SqlCommand();
			_comm.CommandTimeout = 120;
			_comm.CommandText = text;
			_comm.CommandType = typ;
			_comm.Connection = conn;

			// jestlize zadany parametry, pak pripoj
			AttachParameters(_comm, pars);
			return _comm;
		}

		/// <summary>
		/// pridej parametry do prikazu
		/// </summary>
		/// <param name="comm">prikaz</param>
		/// <param name="pars">sada parametru</param>
		private void AttachParameters(IDbCommand comm, IDataParameter[] pars)
		{
			// ohlidani vstupu
			if (comm == null)
				throw new ArgumentNullException("command");
			if (pars == null)
				return;

			// ve smycce pridej vsechny parametry
			foreach (IDataParameter p in pars)
			{
				// osetreni db. nuly
				if (p.Value == null)
					p.Value = DBNull.Value;

				comm.Parameters.Add(p);
			}
		}

		/// <summary>
		/// rozhodovani nad db. nulou
		/// </summary>
		/// <param name="obj">hodnota objektu</param>
		/// <param name="def">default hodnota</param>
		public static object IsNull(object obj, object def)
		{
			// jestlize objekt nulovy, pak vrat default hodnotu
			if (obj == null || obj == DBNull.Value)
				return def;

			return obj;
		}

		public static bool IsNull(object obj)
		{
			// jestlize objekt nulovy, pak vrat default hodnotu
			if (obj == null || obj == DBNull.Value)
				return true;

			return false;
		}

		public static string IDbCommandToString(IDbCommand comm)
		{
			if (comm == null)
				return string.Empty;

			System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);

			if (comm.Connection != null)
				sb.AppendLine("connString:" + comm.Connection.ConnectionString);
			if (comm.CommandText != null)
				sb.AppendLine("command:" + comm.CommandText);
			sb.AppendLine("command type:" + comm.CommandType.ToString());

			if (comm.Parameters != null && comm.Parameters.Count > 0)
			{
				sb.AppendLine("Parameters:");

				foreach (IDbDataParameter param in comm.Parameters)
				{
					sb.AppendFormat("   {0}<{1}>:{2}\n", param.ParameterName, param.DbType.ToString(), param.Value);
				}
			}

			return sb.ToString();
		}

#endregion
	}
}