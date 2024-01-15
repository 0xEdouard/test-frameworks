using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Winkel
{
    class DataStorageMetDataTable : DataStorage
    {
        // De DataTable wordt bij constructie van een object van de klasse DataStorageMetDataTable
        // onmiddellijk ingevuld met alle customers uit de databank.
        // Zo trek je meteen de hele structuur van de datatabel binnen, en moet je deze structuur
        // niet zelf expliciet aanmaken. 
        // (We vermijden dus code van volgende strekking:
        //         table.Columns.Add(CUSTOMERNAME, typeof(String)); )
        // Uiteraard zullen de customers hier enkel opgevraagd worden, 
        // en zal er nog geen enkele update op de databank uitgevoerd worden.
        private DataTable table;


        // Om de DataTable van customers aan te kunnen passen volgens de noden, heb je een adapter nodig.
        // Dat is een object van een zelf te schrijven CustomerAdapter-klasse.
        // De CustomerAdapter zal aangemaakt worden, en in zijn eigen constructor
        // worden ook meteen zijn SelectCommand, InsertCommand, DeleteCommand en UpdateCommand
        // zo goed mogelijk ingesteld. 
        // Indien er ook parameters bij dat commando horen, dan worden
        // die ook al ingesteld. Voor een parameter heb je nodig: de parameternaam, de naam van de kolom in de databank/-table,
        // het type en eventueel de versie (DataRowVersion.Current of DataRowVersion.Original).
        private CustomersTableDao adapter; // nodig om dataTable aan te passen

        public DataStorageMetDataTable()
        {
            adapter = new CustomersTableDao(dbProviderFactory, GetConnection());

            table = adapter.GetCustomersWithoutUpdate();
            // nu is dataTable opgevuld, en heeft dus ook meteen de juiste structuur
            // anders zou je de hele structuur zelf moeten aanleggen:
            /*               
            dataTableCustomers = new DataTable("tabelVoorCustomers");
            DataColumn keyKolom = dataTableCustomers.Columns.Add(CUSTOMERNUMBER, typeof(int));
            dataTableCustomers.Columns.Add(CUSTOMERNAME, typeof(String));
            dataTableCustomers.Columns.Add(CONTACTLASTNAME, typeof(String));
            dataTableCustomers.Columns.Add(CONTACTFIRSTNAME, typeof(String));
            dataTableCustomers.Columns.Add(PHONE, typeof(String));
            dataTableCustomers.Columns.Add(ADDRESSLINE1, typeof(String));
            dataTableCustomers.Columns.Add(ADDRESSLINE2, typeof(String));
            dataTableCustomers.Columns.Add(CITY, typeof(String));
            dataTableCustomers.Columns.Add(STATE, typeof(String));
            dataTableCustomers.Columns.Add(POSTALCODE, typeof(String));
            dataTableCustomers.Columns.Add(COUNTRY, typeof(String));
            dataTableCustomers.Columns.Add(SALESREPEMPLOYEENUMBER, typeof(int));
            dataTableCustomers.Columns.Add(CREDITLIMIT, typeof(double));
            dataTableCustomers.PrimaryKey = new DataColumn[] { keyKolom };
            */
        }

        public void DeleteCustomer(string customerNumber)
        {
            // Duid enkel de customer aan; effectief weghalen uit de database gebeurt later 'en vrac'.
            DataRow? rij = table.Rows.Find(customerNumber);
            if (rij != null)
            {
                rij.Delete();
            }

        }

        public void AddCustomer(Customer customer)
        {
            DataRow row = table.NewRow();

            row[ADDRESSLINE1] = customer.AddressLine1;
            row[ADDRESSLINE2] = customer.AddressLine2;
            row[CITY] = customer.City;
            row[CONTACTFIRSTNAME] = customer.ContactFirstName;
            row[CONTACTLASTNAME] = customer.ContactLastName;
            row[COUNTRY] = customer.Country;
            row[CREDITLIMIT] = customer.CreditLimit;
            row[CUSTOMERNAME] = customer.CustomerName;
            row[CUSTOMERNUMBER] = customer.CustomerNumber;
            row[PHONE] = customer.Phone;
            row[POSTALCODE] = customer.PostalCode;
            row[SALESREPEMPLOYEENUMBER] = customer.SalesRepEmployeeNumber;
            row[STATE] = customer.State;

            table.Rows.Add(row);
        }

        public void UpdateCustomer(string alleVelden, string alleWaarden, string customerNumber)
        {
            // Pas de juiste rij in de datatable aan; overzetten naar de database zelf kan later 'en vrac'.
            // De eerste parameter bevat de primary key van de customer.
            // Vervang de vier vraagtekens: de tweede (en derde en ...) parameter(s) bevatten 
            // één of meer kolomnamen en de bijhorende nieuwe waarde(n) die in die kolom(men) ingevuld moeten worden.
            DataRow? row = table.Rows.Find(customerNumber);
            if (row != null)
            {
                string[] velden = alleVelden.Split(';');
                string[] waarden = alleWaarden.Split(';');
                for (int i = 0; i < velden.Length; i++)
                {
                    row[velden[i]] = waarden[i];
                }
            }
        }

        // Wat hieronder staat is enkel een hulpmethode om makkelijk te kunnen controleren 
        // wat er in een gegeven DataTable zit.
        // (Gezien het een HULPmethode is, geven we hier bewust een parameter mee.
        //  Zo kun je deze hulpmethode voor elke datatable gebruiken; niet enkel voor de instantievariabele.)
        private List<Customer> GetCustomersFromTable(DataTable table)
        {
            List<Customer> list = new List<Customer>();
            foreach (DataRow row in table.Rows)
            {
                Customer customer = new Customer();
                customer.AddressLine1 = row[ADDRESSLINE1].ToString(); customer.AddressLine2 = row[ADDRESSLINE2].ToString();
                customer.City = row[CITY].ToString();
                customer.ContactFirstName = row[CONTACTFIRSTNAME].ToString();
                customer.ContactLastName = row[CONTACTLASTNAME].ToString();
                customer.Country = row[COUNTRY].ToString();
                customer.CreditLimit = (double)row[CREDITLIMIT];
                customer.CustomerName = row[CUSTOMERNAME].ToString();
                customer.CustomerNumber = (int)row[CUSTOMERNUMBER];
                customer.Phone = row[PHONE].ToString();
                customer.PostalCode = row[POSTALCODE].ToString();
                if (!(row[SALESREPEMPLOYEENUMBER] is System.DBNull))
                {
                    customer.SalesRepEmployeeNumber = (int)row[SALESREPEMPLOYEENUMBER];
                }
                // foutmelding: conversie niet geldig -> als DBNull niet getest werd!
                customer.State = row[STATE].ToString();

                list.Add(customer);

            }

            return list;

        }

        public List<Customer> GetCustomersFromDataBase_WithoutDataTableUpdate()
        {
            return GetCustomersFromTable(adapter.GetCustomersWithoutUpdate());
        }

        public List<Customer> GetCustomersFromDataTable_NotCertainTheyAreInDataBase()
        {
            // Je vraagt de instantievariabele "DataTable table" op; 
            // maar als die nog niet overgezet is naar
            // de databank, heb je niet de echte info uit de databank.
            // Als je dat wil, zal je eerst moeten updaten (zie andere methode in deze klasse).
            return GetCustomersFromTable(table);
        }


        // Het updaten van de databank door de adapter stellen we uit
        // tot alle wijzigingen doorgevoerd zijn. Moest de gebruiker 'Update' vergeten
        // oproepen: als hij een List van Customers opvraagt, wordt hij eraan herinnerd
        // (zie naamgevingen van drie verschillende public methodes met returntype 
        // List<Customer>).
        public void Update()
        {
            adapter.Update(table);
        }


        // Moest de gebruiker de Update vergeten vragen: 
        // gebeurt automatisch in finalizer / destructor.
        ~DataStorageMetDataTable()
        {
            Update();
        }
    }
}
