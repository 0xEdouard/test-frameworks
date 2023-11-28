package be.ugent.reactive;

/**
 * One could annotate this class with @ResponseStatus(HttpStatus.NOT_FOUND) instead of adding the explicit exception
 * handler in the RestController.
 */
public class PostNotFoundException extends RuntimeException {
    public PostNotFoundException(String id) {
        super("Could not find post with id=" + id);
    }
}
