using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Winkel
{
    class CustomersTableDao
    {
        private DbDataAdapter adapter;

        public CustomersTableDao(DbProviderFactory factory, DbConnection connection)
        {
            adapter = factory.CreateDataAdapter();
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            StelSelectCommandIn(connection, factory);
            StelInsertCommandIn(connection, factory);
            StelUpdateCommandIn(connection, factory);
            StelDeleteCommandIn(connection, factory);
            // We willen echt niet alles deleten... 
            // maar van zodra we één rij weghalen, moet het deleteCommand ingesteld zijn!
        }


        private void StelSelectCommandIn(DbConnection connection, DbProviderFactory factory)
        {
            adapter.SelectCommand = connection.CreateCommand();
            adapter.SelectCommand.CommandText = ConfigurationManager.AppSettings["SELECT_ALL_CUSTOMERS"];
        }

        private void StelInsertCommandIn(DbConnection connection, DbProviderFactory factory)
        {
            adapter.InsertCommand = connection.CreateCommand();
            adapter.InsertCommand.CommandText = ConfigurationManager.AppSettings["INSERT_ONE_CUSTOMER"];
            StelGrosVanParametersInVoorCommand(adapter.InsertCommand, factory);
            adapter.InsertCommand.Parameters.Add(CrParam("@" + DataStorage.CUSTOMERNUMBER, DbType.String, DataStorage.CUSTOMERNUMBER, DataRowVersion.Current, factory));
        }

        private void StelDeleteCommandIn(DbConnection connection, DbProviderFactory factory)
        {
            adapter.DeleteCommand = connection.CreateCommand();
            adapter.DeleteCommand.CommandText = ConfigurationManager.AppSettings["DELETE_CUSTOMER_WITH_NUMBER"];
            adapter.DeleteCommand.Parameters.Add(CrParam("@" + DataStorage.CUSTOMERNUMBER, DbType.String, DataStorage.CUSTOMERNUMBER, factory));
        }

        // De tekst van updateCommand includeert ALLE kolommen,
        // ook diegene die in dat specifieke geval niet gewijzigd moeten worden.
        // Maar dan cover je tenminste alle soorten aanvragen. 
        // (Zie DataStorageMetDataTable.ZetWijzigingenKlaarVoorCustomer)
        private void StelUpdateCommandIn(DbConnection connection, DbProviderFactory factory)
        {
            adapter.UpdateCommand = connection.CreateCommand();
            adapter.UpdateCommand.CommandText = ConfigurationManager.AppSettings["UPDATE_CUSTOMER_WITH_NUMBER"];
            StelGrosVanParametersInVoorCommand(adapter.UpdateCommand, factory);
            adapter.UpdateCommand.Parameters.Add(CrParam("@" + DataStorage.CUSTOMERNUMBER, DbType.String, DataStorage.CUSTOMERNUMBER, DataRowVersion.Original, factory));
        }

        private void StelGrosVanParametersInVoorCommand(DbCommand command, DbProviderFactory factory)
        {
            command.Parameters.Add(CrParam("@" + DataStorage.ADDRESSLINE1, DbType.String, DataStorage.ADDRESSLINE1, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.ADDRESSLINE2, DbType.String, DataStorage.ADDRESSLINE2, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.CITY, DbType.String, DataStorage.CITY, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.CONTACTFIRSTNAME, DbType.String, DataStorage.CONTACTFIRSTNAME, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.CONTACTLASTNAME, DbType.String, DataStorage.CONTACTLASTNAME, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.COUNTRY, DbType.String, DataStorage.COUNTRY, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.CREDITLIMIT, DbType.Double, DataStorage.CREDITLIMIT, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.CUSTOMERNAME, DbType.String, DataStorage.CUSTOMERNAME, DataRowVersion.Current, factory));
            //command.Parameters.Add(CrParam("@" + DataStorage.CUSTOMERNUMBER, DbType.String, DataStorage.CUSTOMERNUMBER));
            command.Parameters.Add(CrParam("@" + DataStorage.PHONE, DbType.String, DataStorage.PHONE, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.POSTALCODE, DbType.String, DataStorage.POSTALCODE, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.SALESREPEMPLOYEENUMBER, DbType.Int32, DataStorage.SALESREPEMPLOYEENUMBER, DataRowVersion.Current, factory));
            command.Parameters.Add(CrParam("@" + DataStorage.STATE, DbType.String, DataStorage.STATE, DataRowVersion.Current, factory));

        }

        private DbParameter CrParam(string parameterName, DbType type, string column, DbProviderFactory factory)
        {
            DbParameter parameter = factory.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = type;
            parameter.SourceColumn = column;
            return parameter;
        }
        private DbParameter CrParam(string parameterName, DbType type, string column, DataRowVersion version, DbProviderFactory factory)
        {
            DbParameter parameter = CrParam(parameterName, type, column, factory);
            parameter.SourceVersion = version;
            return parameter;
        }

        // haalt enkel uit de databank, zonder te updaten
        public DataTable GetCustomersWithoutUpdate()
        {
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public void Update(DataTable table)
        {
            adapter.Update(table);
        }


    }
}
