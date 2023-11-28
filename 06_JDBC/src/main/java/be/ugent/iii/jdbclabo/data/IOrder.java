/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package be.ugent.iii.jdbclabo.data;

import java.util.Date;
import java.util.List;

public interface IOrder {

    String getComments();

    int getCustomerNumber();

    List<IOrderDetail> getDetails();

    int getNumber();

    Date getOrdered();

    Date getRequired();

    Date getShipped();

    String getStatus();

    void setComments(String comments);

    void setCustomerNumber(int customerNumber);

    void setDetails(List<IOrderDetail> details);

    void setNumber(int number);

    void setOrdered(Date ordered);

    void setRequired(Date required);

    void setShipped(Date shipped);

    void setStatus(String status);

}
