package be.ugent.reeks1;

import io.micrometer.core.instrument.Counter;
import io.micrometer.core.instrument.MeterRegistry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.annotation.Secured;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.net.URI;
import java.util.List;

@RestController
public class BlogController {
    private final Logger logger = LoggerFactory.getLogger(BlogController.class);

    private final BlogPostDAO postDAO;

    // Custom metrics exposed through the actuator
    private final Counter postsCreateCounter;
    private final Counter postsDeleteCounter;
    private final Counter postsReadCounter;
    private final Counter postsUpdateCounter;

    public BlogController(BlogPostDAO postDAO, MeterRegistry meterRegistry) {
        this.postDAO = postDAO;
        this.postsCreateCounter = meterRegistry.counter("blogpost_total", "operation", "created");
        this.postsDeleteCounter = meterRegistry.counter("blogpost_total", "operation", "deleted");;
        this.postsReadCounter = meterRegistry.counter("blogpost_total", "operation", "read");;
        this.postsUpdateCounter = meterRegistry.counter("blogpost_total", "operation", "updated");;
    }

    /**
     * Provide a list of all blogPosts.
     */
    @GetMapping("/posts")
    public List<BlogPost> getPosts() {
        this.postsReadCounter.increment();
        return postDAO.getAllPosts();
    }

    /**
     * Provide the details of a blogPost with the given id. Throw PostNotFoundException if id doesn't exist.
     */
    @GetMapping("/posts/{id}")
    public BlogPost getPost(@PathVariable("id") long id) {
        this.postsReadCounter.increment();
        return postDAO.getPost(id).orElseThrow(() -> new PostNotFoundException(id));
    }

    /**
     * Creates a new BlogPost, setting its URL as the Location header on the
     * response.
     */
    @PostMapping("/posts")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<Void> addPost(@RequestBody BlogPost post) {
        this.postsCreateCounter.increment();
        postDAO.addPost(post);
        URI location = ServletUriComponentsBuilder
                .fromCurrentRequestUri()
                .path("/{id}")
                .buildAndExpand(post.getId())
                .toUri();
        return ResponseEntity.created(location).build();
    }

    /**
     * Removes the blogPost with the given id.
     */
    @DeleteMapping("/posts/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void deletePost(@PathVariable("id") long id) {
        this.postsDeleteCounter.increment();
        postDAO.deletePost(id);
    }

    /**
     * Update the blogPost with the given id.
     * Return http status 409 (Conflict) if id's do not match between body and path.
     */
    @PutMapping("/posts/{id}")
    @Secured("ROLE_ADMIN")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void updatePost(@RequestBody BlogPost post, @PathVariable("id") long id) {
        if (id != post.getId()) {
            throw new ResponseStatusException(HttpStatus.CONFLICT, "ID in path does not match ID in the body");
        }
        this.postsUpdateCounter.increment();
        postDAO.updatePost(id, post);
    }

    /**
     * Search for blogPost based on keywords
     */
    @GetMapping("/posts/search")
    public List<BlogPost> searchPosts(@RequestParam(name = "q") String keyword) {
        return postDAO.searchPostsByTitleContaining(keyword);
    }

    /**
     * Explicit exception handler to map PostNotFoundException to a 404 Not Found HTTP status code.
     */
    @ResponseStatus(HttpStatus.NOT_FOUND)
    @ExceptionHandler(PostNotFoundException.class)
    public void handleNotFound(Exception ex) {
        logger.warn("Exception is: " + ex.getMessage());
        // return empty 404
    }
}
