/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package be.ugent.iii.jdbclabo.data;

import java.util.List;

public interface IDataStorage {

    List<IProduct> getProducts() throws DataExceptie;

    List<ICustomer> getCustomers() throws DataExceptie;

    List<IOrder> getOrders(int customerNumber) throws DataExceptie;

    int maxCustomerNumber() throws DataExceptie;

    int maxOrderNumber() throws DataExceptie;

    void addOrder(IOrder order) throws DataExceptie;

    void addCustomer(ICustomer customer) throws DataExceptie;

    void modifyCustomer(ICustomer customer) throws DataExceptie;

    void deleteCustomer(int customerNumber) throws DataExceptie;

    double getTotal(int customerNumber) throws DataExceptie;

}
