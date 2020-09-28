namespace DatabaseUpgrader
{
    public interface IDatabaseUpgrader
    {
        /// <summary>
        /// Method for upgrading database. It will go thru all methods decorated with <see cref="DatabaseUpgradeMethodAttribute"/> and
        /// run these in order of this attribute value.
        /// <remarks>More methods with same value of this attribute have undefined order.</remarks>
        /// </summary>
        void Upgrade();

        /// <summary>
        /// Drops stored procedure. 
        /// </summary>
        /// <param name="procedureName">Name of stored procedure.</param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        void DropStoredProcedure(string procedureName);

        /// <summary>
        /// Drops table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        void DropTable(string tableName);

        /// <summary>
        /// Removes column from specified table.
        /// </summary>
        /// <param name="columnName">Name of column in table.</param>
        /// <param name="tableName">Name of table from where to drop a column.</param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        void DropColumnFromTable(string columnName, string tableName);

        /// <summary>
        /// Adds column to specified table.
        /// </summary>
        /// <param name="columnName">Name of column.</param>
        /// <param name="columnDataType">Datatype of column, with all declarations. <example>varchar(20) not null</example></param>
        /// <param name="tableName">Name of table where to add a column.</param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        void AddColumnToTable(string columnName, string columnDataType, string tableName, bool isNull);

        /// <summary>
        /// Creates new stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of procedure.</param>
        /// <param name="parameters">Names of parameters.</param>
        /// <param name="parametersDataTypes">Datatypes of parameters, with all declarations. <example>int = null</example></param>
        /// <param name="body">The body of procedure. <example>select * from foo where bar = @bar;</example></param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        void AddStoredProcedure(string procedureName, string[] parameters, string[] parametersDataTypes, string body);

        /// <summary>
        /// Creates new table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columns">Names of columns.</param>
        /// <param name="columnsDataTypes">Datatypes of columns, with all declarations. <example>varchar(20) not null</example></param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        void AddTable(string tableName, string[] columns, string[] columnsDataTypes);

        /// <summary>
        /// Runs custom DDL command(s) defined as a string(s).
        /// No validation or checks are done.
        /// </summary>
        /// <param name="command">Command definiton.</param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        void RunDDLCommands(string command);
        /// <summary>
        /// Runs custom DDL command(s) defined as a string(s).
        /// No validation or checks are done.
        /// </summary>
        /// <param name="commands">Command definiton.</param>
        /// <exception cref="DatabaseUpgradeException"></exception>
        /// <seealso cref="ParseCommands"/>
        void RunDDLCommands(string[] commands);

        /// <summary>
        /// Method parses commands in one string to separate commands.
        /// Refer concrete IDatabaseUpgrader implemetation documentation.
        /// </summary>
        /// <param name="commands">String with all commands.</param>
        /// <returns>Parsed commands.</returns>
        string[] ParseCommands(string commands);
    }
}
