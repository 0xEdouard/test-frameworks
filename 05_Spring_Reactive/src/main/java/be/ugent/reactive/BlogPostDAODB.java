package be.ugent.reactive;

import org.springframework.context.annotation.Profile;
import org.springframework.data.mongodb.core.ChangeStreamEvent;
import org.springframework.data.mongodb.core.ReactiveMongoTemplate;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.stereotype.Service;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

@Service
public class BlogPostDAODB implements BlogPostDAO {
    private final BlogPostRepository repository;
    private final ReactiveMongoTemplate reactiveMongoTemplate;

    public BlogPostDAODB(BlogPostRepository repository, ReactiveMongoTemplate reactiveMongoTemplate) {
        this.repository = repository;
        this.reactiveMongoTemplate = reactiveMongoTemplate;
    }

    public Flux<BlogPost> getAllPosts() {
        return repository.findAll();
    }

    public Mono<BlogPost> addPost(BlogPost blogPost) {
        return repository.save(blogPost);
    }

    public Mono<BlogPost> updatePost(String id, BlogPost blogPost) {
        return repository.save(blogPost);
    }

    public Mono<BlogPost> getPost(String id) {
        return repository.findById(id);
    }

    public Mono<Void> deletePost(String id) {
        return repository.deleteById(id);
    }

    public Flux<BlogPost> searchPostsByTitleContaining(String keyword) {
        return repository.findByTitleContaining(keyword);
    }

    /**
     * This method returns a Flux of BlogPost objects that are emitted when an insert / replace / update
     * is detected in the database.
     * @return Flux of BlogPost objects
     */
    public Flux<BlogPost> getChangeStreamPosts() {
        return reactiveMongoTemplate
                .changeStream(BlogPost.class)
                .watchCollection("posts")
                .filter(Criteria.where("operationType").in("insert", "replace", "update"))
                .listen()
                .mapNotNull(ChangeStreamEvent::getBody);
    }
}
