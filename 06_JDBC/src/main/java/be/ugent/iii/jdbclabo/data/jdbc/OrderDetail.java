/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package be.ugent.iii.jdbclabo.data.jdbc;



import be.ugent.iii.jdbclabo.data.IOrderDetail;

import java.util.Objects;

public class OrderDetail implements IOrderDetail {

    private int orderNumber;
    private String productCode;
    private int quantity;
    private double price;
    private int orderLineNumber;

    public OrderDetail(int orderNumber, String productCode, int quantity, double price, int orderLineNumber) {
        this.orderNumber = orderNumber;
        this.productCode = productCode;
        this.quantity = quantity;
        this.price = price;
        this.orderLineNumber = orderLineNumber;
    }

    @Override
    public int getOrderLineNumber() {
        return orderLineNumber;
    }

    @Override
    public void setOrderLineNumber(int orderLineNumber) {
        this.orderLineNumber = orderLineNumber;
    }

    @Override
    public double getPrice() {
        return price;
    }

    @Override
    public void setPrice(double price) {
        this.price = price;
    }

    @Override
    public String getProductCode() {
        return productCode;
    }

    @Override
    public void setProductCode(String productCode) {
        this.productCode = productCode;
    }

    @Override
    public int getQuantity() {
        return quantity;
    }

    @Override
    public void setQuantity(int quantity) {
        this.quantity = quantity;
    }

    @Override
    public int getOrderNumber() {
        return orderNumber;
    }

    @Override
    public void setOrderNumber(int orderNumber) {
        this.orderNumber = orderNumber;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        OrderDetail that = (OrderDetail) o;
        return orderNumber == that.orderNumber &&
                quantity == that.quantity &&
                Double.compare(that.price, price) == 0 &&
                orderLineNumber == that.orderLineNumber &&
                productCode.equals(that.productCode);
    }

    @Override
    public int hashCode() {
        return Objects.hash(orderNumber, productCode, quantity, price, orderLineNumber);
    }
}
