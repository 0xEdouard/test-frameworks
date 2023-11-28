/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package be.ugent.iii.jdbclabo.data.jdbc;


import be.ugent.iii.jdbclabo.data.IOrder;
import be.ugent.iii.jdbclabo.data.IOrderDetail;

import java.util.Date;
import java.util.List;
import java.util.Objects;

public class Order implements IOrder {

    private int number;
    private Date ordered;
    private Date required;
    private Date shipped;
    private String status;
    private String comments;
    private int customerNumber;
    private List<IOrderDetail> details;

    public Order(int number, Date ordered, Date required, Date shipped, String status, String comments, int customerNumber, List<IOrderDetail> details) {
        this.number = number;
        this.ordered = ordered;
        this.required = required;
        this.shipped = shipped;
        this.status = status;
        this.comments = comments;
        this.customerNumber = customerNumber;
        this.details = details;
    }

    @Override
    public String getComments() {
        return comments;
    }

    @Override
    public void setComments(String comments) {
        this.comments = comments;
    }

    @Override
    public int getCustomerNumber() {
        return customerNumber;
    }

    @Override
    public void setCustomerNumber(int customerNumber) {
        this.customerNumber = customerNumber;
    }

    @Override
    public List<IOrderDetail> getDetails() {
        return details;
    }

    @Override
    public void setDetails(List<IOrderDetail> details) {
        this.details = details;
    }

    @Override
    public int getNumber() {
        return number;
    }

    @Override
    public void setNumber(int number) {
        this.number = number;
    }

    @Override
    public Date getOrdered() {
        return ordered;
    }

    @Override
    public void setOrdered(Date ordered) {
        this.ordered = ordered;
    }

    @Override
    public Date getRequired() {
        return required;
    }

    @Override
    public void setRequired(Date required) {
        this.required = required;
    }

    @Override
    public Date getShipped() {
        return shipped;
    }

    @Override
    public void setShipped(Date shipped) {
        this.shipped = shipped;
    }

    @Override
    public String getStatus() {
        return status;
    }

    @Override
    public void setStatus(String status) {
        this.status = status;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Order order = (Order) o;
        return number == order.number &&
                customerNumber == order.customerNumber &&
                ordered.equals(order.ordered) &&
                required.equals(order.required) &&
                Objects.equals(shipped, order.shipped) &&
                status.equals(order.status) &&
                Objects.equals(comments, order.comments);
    }

    @Override
    public int hashCode() {
        return Objects.hash(number, ordered, required, shipped, status, comments, customerNumber, details);
    }
}
