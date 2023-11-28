/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package be.ugent.iii.jdbclabo.data;

public class DataExceptie extends Exception {

    /**
     * Creates a new instance of <code>JDBCExceptie</code> without detail message.
     */
    public DataExceptie() {
    }


    /**
     * Constructs an instance of <code>JDBCExceptie</code> with the specified detail message.
     * @param msg the detail message.
     */
    public DataExceptie(String msg) {
        super(msg);
    }
}
