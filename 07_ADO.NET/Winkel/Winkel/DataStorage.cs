using System.Configuration;
using System.Data.Common;
using System.Text;

namespace Winkel
{
    public abstract class DataStorage
    {

        // Gebruik deze constanten; 
        // zo hoef je niet op juiste spelling te letten
        // en kan je de juiste waarde uit de aangeboden lijst halen 
        // (enkele letters intikken volstaat).
        public const string CUSTOMERNUMBER = "customerNumber";
        public const string CUSTOMERNAME = "customerName";
        public const string CONTACTLASTNAME = "contactLastName";
        public const string CONTACTFIRSTNAME = "contactFirstName";
        public const string PHONE = "phone";
        public const string ADDRESSLINE1 = "addressLine1";
        public const string ADDRESSLINE2 = "addressLine2";
        public const string CITY = "city";
        public const string STATE = "state";
        public const string POSTALCODE = "postalCode";
        public const string COUNTRY = "country";
        public const string SALESREPEMPLOYEENUMBER = "salesRepEmployeeNumber";
        public const string CREDITLIMIT = "creditLimit";

        public const string ORDERNUMBER = "orderNumber";
        public const string ORDERDATE = "orderDate";
        public const string REQUIREDDATE = "requiredDate";
        public const string SHIPPEDDATE = "shippedDate";
        public const string STATUS = "status";
        public const string COMMENTS = "comments";

        public const string PRODUCTCODE = "productCode";
        public const string QUANTITYORDERED = "quantityOrdered";
        public const string PRICEEACH = "priceEach";
        public const string ORDERLINENUMBER = "orderLineNumber";

        protected StringBuilder errorMessages = new StringBuilder(); // kan handig zijn; niet verplicht.

        protected ConnectionStringSettings connectionStringSettings;
        protected DbProviderFactory dbProviderFactory;

        public DataStorage()
        {
            connectionStringSettings = ConfigurationManager.ConnectionStrings["classicmodelsDataBase"];
            dbProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);

        }

        protected DbConnection GetConnection()
        {
            DbConnection connection = dbProviderFactory.CreateConnection();
            connection.ConnectionString = connectionStringSettings.ConnectionString;
            return connection;
        }

        // Het type van de parameter instellen is een goed idee,
        // het gebeurt hier echter niet. 
        // Controleer nog: gaat het inderdaad automatisch goed?
        protected DbParameter MaakParameter(string parameternaam, object waarde)
        {
            DbParameter parameter = dbProviderFactory.CreateParameter();
            parameter.ParameterName = parameternaam;
            parameter.Value = waarde;

            // als je het type toch expliciet instelt:
            /*
            if (waarde is double)
            {
                parameter.DbType = DbType.Double;
            }
            else if(waarde is int)
            {
                parameter.DbType = DbType.Int32;
            }
            else if(waarde is String)
            {
                parameter.DbType = DbType.String;
            }
            */
            return parameter;
        }

    }
}
