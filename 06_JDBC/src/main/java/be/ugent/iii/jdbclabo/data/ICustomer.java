/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package be.ugent.iii.jdbclabo.data;

public interface ICustomer {

    String getAddressLine1();

    String getAddressLine2();

    String getCity();

    String getContactFirstName();

    String getContactLastName();

    String getCountry();

    double getCreditLimit();

    String getCustomerName();

    int getCustomerNumber();

    String getPhone();

    String getPostalCode();

    int getSalesRepEmployeeNumber();

    String getState();

    void setAddressLine1(String addressLine1);

    void setAddressLine2(String addressLine2);

    void setCity(String city);

    void setContactFirstName(String contactFirstName);

    void setContactLastName(String contactLastName);

    void setCountry(String country);

    void setCreditLimit(double creditLimit);

    void setCustomerName(String customerName);

    void setCustomerNumber(int customerNumber);

    void setPhone(String phone);

    void setPostalCode(String postalCode);

    void setSalesRepEmployeeNumber(int salesRepEmployeeNumber);

    void setState(String state);

}
