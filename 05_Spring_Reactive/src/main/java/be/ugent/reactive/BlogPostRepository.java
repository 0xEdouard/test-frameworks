package be.ugent.reactive;

import org.springframework.data.mongodb.repository.ReactiveMongoRepository;
import org.springframework.stereotype.Repository;
import reactor.core.publisher.Flux;

@Repository
public interface BlogPostRepository extends ReactiveMongoRepository<BlogPost, String> {
    Flux<BlogPost> findByTitleContaining(String keyword);
}
