package be.ugent.reactive;

import io.micrometer.core.instrument.Counter;
import io.micrometer.core.instrument.MeterRegistry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.util.UriComponentsBuilder;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.time.Duration;

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
    public Flux<BlogPost> getPosts() {
        this.postsReadCounter.increment();
        return postDAO.getAllPosts();
    }

    /**
     * Provide a stream of all blogPosts with 1 sec delay between each post.
     * 3 implementations: basic, with media-type text/event-stream, and with media-type application/x-ndjson
     */
    @GetMapping("/stream/posts-delay")
    public Flux<BlogPost> getPostsStreamV1() {
        this.postsReadCounter.increment();
        return postDAO.getAllPosts().delayElements(Duration.ofSeconds(1)).log();
    }

    @GetMapping(value = "/stream/posts-text", produces = MediaType.TEXT_EVENT_STREAM_VALUE)
    public Flux<BlogPost> getPostsStreamV2() {
        this.postsReadCounter.increment();
        return postDAO.getAllPosts().delayElements(Duration.ofSeconds(1)).log();
    }

    @GetMapping(value = "/stream/posts-json", produces = MediaType.APPLICATION_NDJSON_VALUE)
    public Flux<BlogPost> getPostsStreamV3() {
        this.postsReadCounter.increment();
        return postDAO.getAllPosts().delayElements(Duration.ofSeconds(1)).log();
    }

    /**
     * ChangeStream of all blogPosts.
     */
    @GetMapping(value = "/stream/posts", produces = MediaType.TEXT_EVENT_STREAM_VALUE)
    public Flux<BlogPost> getChangeStreamPosts() {
        this.postsReadCounter.increment();
        return postDAO.getChangeStreamPosts().log();
    }

    /**
     * Provide the details of a blogPost with the given id. Throw PostNotFoundException if id doesn't exist.
     */
    @GetMapping("/posts/{id}")
    public Mono<BlogPost> getPost(@PathVariable("id") String id) {
        this.postsReadCounter.increment();
        return postDAO.getPost(id).switchIfEmpty(Mono.error(new PostNotFoundException(id)));
    }

    /**
     * Creates a new BlogPost, setting its URL as the Location header on the
     * response.
     */
    @PostMapping("/posts")
    public Mono<ResponseEntity<Void>> addPost(@RequestBody BlogPost post, UriComponentsBuilder uriBuilder) {
        this.postsCreateCounter.increment();
        return postDAO.addPost(post)
                .map(savedPost -> ResponseEntity.created(
                        uriBuilder
                            .path("/posts/{id}")
                            .buildAndExpand(savedPost.getId())
                            .toUri())
                        .build());
    }

    /**
     * Removes the blogPost with the given id.
     */
    @DeleteMapping("/posts/{id}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public Mono<Void> deletePost(@PathVariable("id") String id) {
        this.postsDeleteCounter.increment();
        return postDAO.deletePost(id);
    }

    /**
     * Update the blogPost with the given id.
     * Return http status 409 (Conflict) if id's do not match between body and path.
     */
    @PutMapping("/posts/{id}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public Mono<BlogPost> updatePost(@RequestBody BlogPost post, @PathVariable("id") String id) {
        if (!id.equals(post.getId())) {
            throw new ResponseStatusException(HttpStatus.CONFLICT, "ID in path does not match ID in the body");
        }
        this.postsUpdateCounter.increment();
        return postDAO.updatePost(id, post);
    }

    /**
     * Search for blogPost based on keywords
     */
    @GetMapping("/posts/search")
    public Flux<BlogPost> searchPosts(@RequestParam(name = "q") String keyword) {
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
