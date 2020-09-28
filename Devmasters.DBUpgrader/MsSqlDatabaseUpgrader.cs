using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;

namespace DatabaseUpgrader
{
    public class MsSqlDatabaseUpgrader : IDatabaseUpgrader
    {
        private const string DATABASE_VERSION_KEYWORD = "DatabaseVersion";
        private const string DEFAULT_OBJECT_NAME_FOR_VERSION_INFO = "_DatabaseUpgrader";
        private const string INITIAL_VERSION = "0.0.0.0";

        private SqlConnection _connection;
        private Type _member;
        private string _objectNameForVersionInfo;
        private MsSqlDatabaseObjectTypeForDatabaseVersion _databaseObjectTypeForVersionInfo;

        #region ctors
        public MsSqlDatabaseUpgrader(string connectionString, Type member,
            string objectNameForVersionInfo, MsSqlDatabaseObjectTypeForDatabaseVersion databaseObjectTypeForVersionInfo)
        {
            this._connection = new SqlConnection(ReworkConnectionString(connectionString));
            this._member = member;
            this._objectNameForVersionInfo = objectNameForVersionInfo;
            this._databaseObjectTypeForVersionInfo = databaseObjectTypeForVersionInfo;
        }

        public MsSqlDatabaseUpgrader(string connectionString, Type member, MsSqlDatabaseObjectTypeForDatabaseVersion databaseObjectTypeForVersionInfo)
            : this(connectionString, member, DEFAULT_OBJECT_NAME_FOR_VERSION_INFO, databaseObjectTypeForVersionInfo)
        { }

        // default is ExtendedProperty
        public MsSqlDatabaseUpgrader(string connectionString, Type member)
            : this(connectionString, member, DEFAULT_OBJECT_NAME_FOR_VERSION_INFO, MsSqlDatabaseObjectTypeForDatabaseVersion.ExtendedProperty)
        { }
        #endregion

        public void Upgrade()
        {
            IDictionary<Version, MemberInfo> upgradingMethods = HelperStuff.GetAvailableDatabaseUpgradeMethods(this._member);
            IList<MemberInfo> beforeUpgradeMethods = HelperStuff.GetBeforeUpgradeMethods(this._member);
            IList<MemberInfo> afterUpgradeMethods = HelperStuff.GetAfterUpgradeMethods(this._member);

            Trace.WriteLine(string.Format("Starting transaction."));
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                Version versionOfDatabase = GetVersionOfDatabase();
                Trace.WriteLine(string.Format("Current version of database is {0}.", versionOfDatabase));

                Version versionOfLibrary = HelperStuff.GetVersionOfLibrary(this._member);
                Trace.WriteLine(string.Format("Current version of library is {0}.", versionOfLibrary));

                var methodsForProcessing = upgradingMethods
                    .Where(m => m.Key > versionOfDatabase && m.Key <= versionOfLibrary)
                    .OrderBy(m => m.Key);

                foreach (var item in beforeUpgradeMethods)
                {
                    Trace.WriteLine(string.Format("Running Before Upgrade method with Name: {0}.", item.Name));
                    HelperStuff.ExecuteDatabaseUpgraderReflectionMethodWithParameters(item, this);
                }

                foreach (var item in methodsForProcessing)
                {
                    Trace.WriteLine(string.Format("Running Upgrade method:"));
                    Trace.Indent();
                    Trace.WriteLine(string.Format("Name: {0}.", item.Value.Name));
                    Trace.WriteLine(string.Format("Defined version: {0}.", item.Key.ToString(4)));
                    Trace.Unindent();

                    HelperStuff.ExecuteDatabaseUpgraderReflectionMethodWithParameters(item.Value, this);
                }

                foreach (var item in afterUpgradeMethods)
                {
                    Trace.WriteLine(string.Format("Running After Upgrade method with Name: {0}.", item.Name));
                    HelperStuff.ExecuteDatabaseUpgraderReflectionMethodWithParameters(item, this);
                }

                // if there was some update, set version
                if (methodsForProcessing.Count() > 0)
                {
                    // should I place here upgrade method version or library version???
                    // for now I'm using method version, 'cause it can be added new method for version and not incrementing library version (which is weird, but ...)
                    Version upgradedToVersion = methodsForProcessing.Last().Key;
                    Trace.WriteLine(string.Format("Writing new database version: {0}.", upgradedToVersion.ToString(4)));
                    SetVersionOfDatabase(upgradedToVersion);
                }

                Trace.WriteLine(string.Format("Commiting transaction."));
                ts.Complete();

                if (this._connection.State != ConnectionState.Closed)
                    this._connection.Close();
            }
        }

        #region Methods for usage to upgrade DB
        public void DropStoredProcedure(string procedureName)
        {
            ExecuteDDLCommand(string.Format("drop procedure {0}", procedureName));
        }

        public void DropTable(string tableName)
        {
            ExecuteDDLCommand(string.Format("drop table {0}", tableName));
        }

        public void DropColumnFromTable(string columnName, string tableName)
        {
            ExecuteDDLCommand(string.Format(@"
if (Exists(select * from sys.columns where Name = N'{1}'  
			and Object_ID = Object_ID(N'{0}')))
begin

alter table {0} drop column {1}
end", tableName, columnName));
        }

        public void AddColumnToTable(string columnName, string columnDataType, string tableName, bool isNull)
        {
            ExecuteDDLCommand(string.Format(@"
if (not Exists(select * from sys.columns where Name = N'{1}'  
			and Object_ID = Object_ID(N'{0}')))
begin
    alter table {0} add {1} {2} {3}
end
", tableName, columnName, columnDataType, isNull ? "NULL" : string.Empty));
        }

        public void AddTable(string tableName, string[] columns, string[] columnsDataTypes)
        {
            if (columns.Length != columnsDataTypes.Length)
                throw new ArgumentException();

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("create table {0}{1}({1}", tableName, Environment.NewLine);

            for (int i = 0; i < columns.Length; i++)
            {
                builder.AppendFormat("{0} {1}{2}{3}",
                    columns[i],
                    columnsDataTypes[i],
                    (i + 1 != columns.Length ? "," : string.Empty),
                    Environment.NewLine);
            }

            builder.Append(")");

            ExecuteDDLCommand(builder.ToString());
        }

        public void AddStoredProcedure(string procedureName, string[] parameters, string[] parametersDataTypes, string body)
        {
            if (parameters.Length != parametersDataTypes.Length)
                throw new ArgumentException();

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("create procedure {0}{1}({1}", procedureName, Environment.NewLine);

            for (int i = 0; i < parameters.Length; i++)
            {
                builder.AppendFormat("{0} {1}{2}{3}",
                    (parameters[i].StartsWith("@") ? parameters[i] : "@" + parameters[i]),
                    parametersDataTypes[i],
                    (i + 1 != parameters.Length ? "," : string.Empty),
                    Environment.NewLine);
            }

            builder.AppendFormat("){0}as{0}begin{0}", Environment.NewLine);
            builder.Append(body);
            builder.AppendFormat("{0}end", Environment.NewLine);

            ExecuteDDLCommand(builder.ToString());
        }

        public void RunDDLCommands(string command)
        {
            ExecuteDDLCommand(command);
        }

        public void RunDDLCommands(string[] commands)
        {
            foreach (string command in commands)
            {
                RunDDLCommands(command);
            }
        }

        public string[] ParseCommands(string commands)
        {
            Regex re = new Regex("^GO", RegexOptions.Multiline);
            return re.Split(commands);
        }
        #endregion

        #region Database version handling methods
        #region Get version
        private Version GetVersionOfDatabase()
        {
            CheckVersionInfoObject();

            switch (this._databaseObjectTypeForVersionInfo)
            {
                case MsSqlDatabaseObjectTypeForDatabaseVersion.Table:
                    return GetVersionOfDatabaseUsingTable();
                case MsSqlDatabaseObjectTypeForDatabaseVersion.StoredProcedure:
                    return GetVersionOfDatabaseUsingStoredProcedure();
                case MsSqlDatabaseObjectTypeForDatabaseVersion.ExtendedProperty:
                    return GetVersionOfDatabaseUsingExtendedProperty();
                default:
                    throw new Exception(); // is the compiler sick? ;)
            }
        }

        private Version GetVersionOfDatabaseUsingExtendedProperty()
        {
            this._connection.SmartOpen();

            using (SqlCommand cmd = this._connection.CreateCommand())
            {
                cmd.CommandText = string.Format("select value from ::fn_listextendedproperty('{0}', DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT)",
                    this._objectNameForVersionInfo);
                try
                {
                    object result = cmd.ExecuteScalar();
                    return new Version((string)result);
                }
                catch
                {
                    throw new DatabaseUpgradeException("Could not find version info in database.");
                }
            }
        }

        private Version GetVersionOfDatabaseUsingStoredProcedure()
        {
            this._connection.SmartOpen();

            using (SqlCommand cmd = this._connection.CreateCommand())
            {
                cmd.CommandText = this._objectNameForVersionInfo;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@" + DATABASE_VERSION_KEYWORD, SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
                try
                {
                    object result = cmd.ExecuteScalar();
                    return new Version((string)cmd.Parameters[0].Value);
                }
                catch
                {
                    throw new DatabaseUpgradeException("Could not find version info in database.");
                }
            }
        }

        private Version GetVersionOfDatabaseUsingTable()
        {
            this._connection.SmartOpen();

            using (SqlCommand cmd = this._connection.CreateCommand())
            {
                cmd.CommandText = string.Format("select value from [dbo].[{0}] where id = @id",
                    this._objectNameForVersionInfo);
                cmd.Parameters.Add("@id", SqlDbType.NVarChar, 255).Value = DATABASE_VERSION_KEYWORD;
                try
                {
                    object result = cmd.ExecuteScalar();
                    return new Version((string)result);
                }
                catch
                {
                    throw new DatabaseUpgradeException("Could not find version info in database.");
                }
            }
        }
        #endregion

        #region Set version
        private void SetVersionOfDatabase(Version version)
        {
            switch (this._databaseObjectTypeForVersionInfo)
            {
                case MsSqlDatabaseObjectTypeForDatabaseVersion.Table:
                    SetVersionOfDatabaseUsingTable(version);
                    break;
                case MsSqlDatabaseObjectTypeForDatabaseVersion.StoredProcedure:
                    SetVersionOfDatabaseUsingStoredProcedure(version);
                    break;
                case MsSqlDatabaseObjectTypeForDatabaseVersion.ExtendedProperty:
                    SetVersionOfDatabaseUsingExtendedProperty(version);
                    break;
            }
        }

        private void SetVersionOfDatabaseUsingExtendedProperty(Version version)
        {
            this._connection.SmartOpen();

            using (SqlCommand cmd = this._connection.CreateCommand())
            {
                cmd.CommandText = "sp_updateextendedproperty";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter() { Direction = ParameterDirection.ReturnValue });
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 128 /*sysname*/).Value = this._objectNameForVersionInfo;
                cmd.Parameters.Add("@value", SqlDbType.Variant).Value = version.ToString(4);
                try
                {
                    int result = cmd.ExecuteNonQuery();
                    if ((int)cmd.Parameters[0].Value != 0)
                        throw new DatabaseUpgradeException();
                }
                catch
                {
                    throw new DatabaseUpgradeException("Could not update version info.");
                }
            }
        }

        private void SetVersionOfDatabaseUsingStoredProcedure(Version version)
        {
            this._connection.SmartOpen();

            using (SqlCommand cmd = this._connection.CreateCommand())
            {
                cmd.CommandText = string.Format("ALTER PROCEDURE [dbo].[{0}](@{1} nvarchar(255) = null OUTPUT) AS BEGIN select @{1} = '{2}'; END",
                    this._objectNameForVersionInfo,
                    DATABASE_VERSION_KEYWORD,
                    version.ToString(4));
                try
                {
                    int result = cmd.ExecuteNonQuery();
                }
                catch
                {
                    throw new DatabaseUpgradeException("Could not update version info.");
                }
            }
        }

        private void SetVersionOfDatabaseUsingTable(Version version)
        {
            this._connection.SmartOpen();

            using (SqlCommand cmd = this._connection.CreateCommand())
            {
                cmd.CommandText = string.Format("update [dbo].[{0}] set value = @value where id = @id",
                    this._objectNameForVersionInfo);
                cmd.Parameters.Add("@value", SqlDbType.NVarChar).Value = version.ToString(4);
                cmd.Parameters.Add("@id", SqlDbType.NVarChar, 255).Value = DATABASE_VERSION_KEYWORD;
                try
                {
                    int result = cmd.ExecuteNonQuery();
                    if (result != 1)
                        throw new DatabaseUpgradeException();
                }
                catch
                {
                    throw new DatabaseUpgradeException("Could not update version info.");
                }
            }
        }
        #endregion

        private void CheckVersionInfoObject()
        {
            string command = string.Empty;

            switch (this._databaseObjectTypeForVersionInfo)
            {
                case MsSqlDatabaseObjectTypeForDatabaseVersion.Table:
                    command = string.Format(
                        "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type IN (N'U')) " +
                        "BEGIN " +
                        "  CREATE TABLE [dbo].[{0}] (id nvarchar(255) not null primary key, value nvarchar(max)); " +
                        "  INSERT INTO {0} (id, value) VALUES ('{1}', '{2}');" +
                        "END",
                        this._objectNameForVersionInfo,
                        DATABASE_VERSION_KEYWORD,
                        INITIAL_VERSION);
                    break;
                case MsSqlDatabaseObjectTypeForDatabaseVersion.StoredProcedure:
                    command = string.Format(
                        "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'P', N'PC')) " +
                        "BEGIN " +
                        "  EXEC sp_executesql @statement=N'CREATE PROCEDURE [dbo].[{0}] (@{1} nvarchar(255) = null OUTPUT) AS BEGIN select @{1} = ''{2}''; END' " +
                        "END",
                        this._objectNameForVersionInfo,
                        DATABASE_VERSION_KEYWORD,
                        INITIAL_VERSION);
                    break;
                case MsSqlDatabaseObjectTypeForDatabaseVersion.ExtendedProperty:
                    command = string.Format(
                        "IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE name = '{0}' AND class = 0)" +
                        "BEGIN " +
                        "  EXEC sp_addextendedproperty @name = '{0}', @value = '{1}' " +
                        "END",
                        this._objectNameForVersionInfo,
                        INITIAL_VERSION);
                    break;
            }

            this._connection.SmartOpen();
            using (SqlCommand cmd = this._connection.CreateCommand())
            {
                cmd.CommandText = command;
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        private string ReworkConnectionString(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            builder.Enlist = true;
            return builder.ToString();
        }

        private void ExecuteDDLCommand(string command)
        {


            string GO_Delimiter = "GO" + (char)13 + (char)10;
            string[] arrCmd = command.Split(new string[] { GO_Delimiter }, StringSplitOptions.RemoveEmptyEntries);

            this._connection.SmartOpen();
            foreach (string cmdTxt in arrCmd)
            {
                using (SqlCommand cmd = this._connection.CreateCommand())
                {
                    cmd.CommandTimeout = 120;
                    cmd.CommandText = cmdTxt.Trim();
                    if (cmd.CommandText.Length > 2)
                    {
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException e)
                        {
                            throw new DatabaseUpgradeException("Command execution problem.", e);
                        }
                    }
                }
            }
        }
    }
}