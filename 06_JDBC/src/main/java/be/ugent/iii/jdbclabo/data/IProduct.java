/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package be.ugent.iii.jdbclabo.data;

public interface IProduct {

    double getMsrp();

    double getPrice();

    String getProductCode();

    String getProductDescription();

    String getProductLine();

    String getProductName();

    String getProductScale();

    String getProductVendor();
    
    int getQuantityInStock();

    void setMsrp(double msrp);

    void setPrice(double price);

    void setProductCode(String productCode);

    void setProductDescription(String productDescription);

    void setProductLine(String productLine);

    void setProductName(String productName);

    void setProductScale(String productScale);

    void setProductVendor(String productVendor);
    
    void setQuantityInStock(int quantityInStock);

}
