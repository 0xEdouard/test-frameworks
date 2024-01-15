using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Winkel;

// test voor ADO.Net labo deel "DataReaders"

// BELANGRIJK
// Gebruik deze test niet als enige tool om de geschreven code te controleren.
// Om zinvol automatisch te testen of je kan toevoegen aan een databank, 
// moet je immers ook uit de databank kunnen ophalen.
// Dus zal een UnitTest beide methodes (ophalen en toevoegen) tegelijk testen.
// Gebruik in eerste instantie een hoofdprogramma om (visueel) na te gaan
// of het ophalen apart al lukt; idem voor het toevoegen.
// Vergeet uiteraard de databank niet te refreshen als je wil nagaan of er iets veranderd is.
// (Indien refreshen niet voldoende is: connectie 'manueel' sluiten en opnieuw openen.)
//
// Als de volledige opdracht afgewerkt en via hoofdprogramma getest is,
// kan je deze UnitTesten runnen - maar dat impliceert het schrijven van nog extra methodes
// (bonus-opdracht).

namespace TestWinkel
{
    public class Tests
    {


        Random random;
        DataStorageMetReader dataStorage;
        [SetUp]
        public void Setup()
        {

            // Register provider
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
            random = new Random();
            dataStorage = new DataStorageMetReader();
        }

        private string CreateChar()
        {
            return "" + (char)('A' + random.Next(0, 26));
        }

        private Customer CreateCustomer()
        {
            Customer customer = new Customer();
            customer.AddressLine1 = "Voskenslaan";
            customer.AddressLine2 = "of Schoonmeersen";
            customer.City = "Gent";
            customer.ContactFirstName = "Gilles";
            customer.ContactLastName = "Donckers";
            customer.Country = "Belgie";
            customer.CreditLimit = 10000;
            customer.CustomerName = "Firma X" + CreateChar() + CreateChar();
            customer.CustomerNumber = 99000 + random.Next(1, 1000);
            customer.Phone = "0032999999999";
            customer.PostalCode = "9000";
            customer.SalesRepEmployeeNumber = 90909090;
            customer.State = "Oost-Vlaanderen";
            return customer;
        }

        private Order CreateOrderWithDetails(int customerNumber)
        {
            Order order = new Order();
            order.Comments = "geen commentaar voorzien bij dit order";
            order.CustomerNumber = customerNumber;
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

            return order;
        }

        [Test]
        public void testGetCustomersAddCustomer()
        {
            List<Customer> customersVoor = dataStorage.GetCustomers();
            int aantalVoor = customersVoor.Count;

            Customer customer = CreateCustomer();
            dataStorage.AddCustomer(customer);

            List<Customer> customersNa = dataStorage.GetCustomers();
            Assert.AreEqual(aantalVoor + 1, customersNa.Count);

            Assert.IsTrue(customersNa.Contains(customer));
            // Merk op: 'contains' gebruikt. Moet belletje doen rinkelen.
        }

        // Je kan onmogelijk automatisch testen of een order goed is toegevoegd
        // als je de orders niet kan opvragen.
        // Voor deze test zul je dus nog wat extra methodes moeten schrijven,
        // bovenop de gevraagde methodes in Ufora.

        [Test]
        public void testGetOrdersAndAddOrderWithDetails()
        {
            
            // Deze test zal faliekant aflopen als er geen customers zijn. 
            int customerNumber = dataStorage.GetCustomers()[0].CustomerNumber;

            List<Order> ordersVoor = dataStorage.GetOrdersWithoutDetailsFromCustomer(customerNumber);

            Order orderToeTeVoegen = CreateOrderWithDetails(customerNumber);
            dataStorage.AddOrder(orderToeTeVoegen);

            List<Order> ordersNa = dataStorage.GetOrdersWithoutDetailsFromCustomer(customerNumber);

            Assert.AreEqual(ordersVoor.Count + 1, ordersNa.Count);
            // order is blijkbaar toegevoegd

            // Eerst terug het juiste order uit de databank halen, nl. op ordernumber.
            // Daarna de details uit de databank halen, om te kunnen nagaan of alle 
            // details er zijn.
            Order orderUitDatabankGehaald = null;
            // Omdat er geen methode 'getOrderWithNumber' in de klasse DataStorage
            // aanwezig is, overlopen we de list van orders die uit de databank gehaald is.
            // Dat levert ons ook het order op dat opgeslagen werd.
            foreach (Order elt in ordersNa)
            {
                if (elt.Number == orderToeTeVoegen.Number)
                {
                    orderUitDatabankGehaald = elt;
                    return;
                }
            }

            dataStorage.FillDetailsOfOrder(orderUitDatabankGehaald);

            Assert.AreEqual(orderToeTeVoegen, orderUitDatabankGehaald);
            

        }
    }
}