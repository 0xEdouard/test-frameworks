using System.Data.Common;
using Winkel;

// Hier worden de klassen DataStorageMetReader
// en DataStorageMetDataTable getest.


// Register provider
DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);


DataStorageMetReader storage = new DataStorageMetReader();
WriteCustomers(storage);

//Klant toevoegen
AddCustomer(storage);

WriteCustomers(storage);

//Bestelling toevoegen;
AddOrder(storage);

DataStorageMetDataTable storageDataTable = new DataStorageMetDataTable();

//Zes customers toevoegen,
// nl.  drie met nummer 888xxxx
//   en drie met nummer 999xxxx
int nrLaatsteCustomer = LastCustomerNumber(storageDataTable);
WriteCustomersInDataTable(storageDataTable);

//Customers met nummers 888xxxx weghalen
DeleteCustomersAbove(storageDataTable);
WriteCustomersInDataTable(storageDataTable);

//Customer aanpassen 
UpdateCustomers(storageDataTable, nrLaatsteCustomer);
WriteCustomersInDataTable(storageDataTable);

static void WriteCustomers(DataStorageMetReader storage)
{
    List<Customer> customers = storage.GetCustomers();

    Console.WriteLine("lijst heeft lengte " + customers.Count);

    foreach (Customer cust in customers)
    {
        Console.WriteLine(cust.ToString());
    }
    Console.WriteLine("EINDE DATABASELIJST");

}

static void AddCustomer(DataStorageMetReader storage)
{

    Customer c = new Customer();
    c.AddressLine1 = "EEN !";
    c.AddressLine2 = "TWEE !";
    c.City = "city";
    c.ContactFirstName = "voornaampje";
    c.ContactLastName = "achternaam contactpersoon";
    c.Country = "BE";
    c.CreditLimit = 50.00;
    c.CustomerName = "naam van klant";
    c.CustomerNumber = 789789789;
    c.Phone = "003212456";
    c.PostalCode = "XM4545";
    c.SalesRepEmployeeNumber = 10101010;
    c.State = "West-Vlaanderen";


    storage.AddCustomer(c);

}

static void AddOrder(DataStorageMetReader storage)
{
    Random random = new Random();
    Order order = new Order();
    order.Comments = "dit order heeft geen comments";
    order.CustomerNumber = 99999;
    order.Number = 110000 + random.Next(1, 10000);
    order.Ordered = DateTime.ParseExact("01/12/2018", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    order.Required = DateTime.ParseExact("25/12/2018", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    order.Shipped = DateTime.ParseExact("13/12/2018", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    order.Status = "ok";

    for (int i = 0; i < 5; i++)
    {
        OrderDetail detail = new OrderDetail();
        detail.OrderNumber = order.Number;
        detail.OrderLineNumber = 1 + i;
        detail.Price = 10.0 * (1 + i);
        detail.ProductCode = "" + ((1 + i) * 111);
        detail.Quantity = 100 * (1 + i);
        order.Details.Add(detail);
    }

    storage.AddOrder(order);
}

static void WriteCustomersInDataTable(DataStorageMetDataTable storageDataTable)
{
    List<Customer> customers = storageDataTable.GetCustomersFromDataTable_NotCertainTheyAreInDataBase();

    Console.WriteLine("\n***\n***\n***lijst in DataTABLE heeft lengte " + customers.Count + "\n***\n");
    for (int i = 120; i < customers.Count; i++)
    {
        Console.WriteLine(customers[i].ToString());
    }
    Console.WriteLine("EINDE DATATABLELIJST");

    customers = storageDataTable.GetCustomersFromDataBase_WithoutDataTableUpdate();

    Console.WriteLine("\n***\n***\n***lijst in DataBASE (oude versie) heeft lengte " + customers.Count + "\n***\n");
    for (int i = 120; i < customers.Count; i++)
    {
        Console.WriteLine(customers[i].ToString());
    }
    Console.WriteLine("EINDE DATABASELIJST");
}

static Customer MaakCustomer(int nummer)
{
    Customer c = new Customer();
    c.AddressLine1 = "EEN";
    c.AddressLine2 = "TWEE";
    c.City = "city";
    c.ContactFirstName = "Jan";
    c.ContactLastName = "Jans";
    c.Country = "BE";
    c.CreditLimit = 50.00;
    c.CustomerName = "Firma Peeters";
    c.CustomerNumber = nummer;
    c.Phone = "003212456";
    c.PostalCode = "XM4545";
    c.SalesRepEmployeeNumber = 10101010;
    c.State = "West-Vlaanderen";

    return c;
}

static int LastCustomerNumber(DataStorageMetDataTable storageDataTable)
{

    Random random = new Random();
    int nummerVanLaatstToegevoegdeCustomer = 9990000;
    Console.WriteLine("\nEr worden 6 customers toegevoegd:");
    Console.WriteLine("3 met nr 888xxxx, 3 met nr 999xxxx, allen met naam 'Jan Jans'.");
    for (int i = 0; i < 3; i++)
    {
        storageDataTable.AddCustomer(MaakCustomer(8880000 + random.Next(1, 10000)));
        nummerVanLaatstToegevoegdeCustomer = 9990000 + random.Next(1, 10000);
        storageDataTable.AddCustomer(MaakCustomer(nummerVanLaatstToegevoegdeCustomer));
    }
    return nummerVanLaatstToegevoegdeCustomer;
}

static void DeleteCustomersAbove(DataStorageMetDataTable storageDataTable)
{
    Console.WriteLine("\nAlle customers met nummer 888xxxx worden verwijderd.");
    for (int i = 0; i < 10000; i++)
    {
        storageDataTable.DeleteCustomer("" + (8880000 + i));
    }
}

 static void UpdateCustomers(DataStorageMetDataTable storageDataTable, int nummerVanLaatstToegevoegdeCustomer)
{
    Console.WriteLine("\nDe laatst toegevoegde customer (999xxxx) krijgt contactName 'ABC' \nen city 'DEF' een nieuw " +
        "telefoonnr met veel nullen.");
    string nieuweContactFirstName = "ABC";
    string nieuweCity = "DEF";

    string velden = "contactFirstName;city;phone";
    string waarden = nieuweContactFirstName + ";" + nieuweCity + ";xx32922000000";

    storageDataTable.UpdateCustomer(velden, waarden, "" + nummerVanLaatstToegevoegdeCustomer);
}