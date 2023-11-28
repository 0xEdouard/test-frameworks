package be.ugent.reactive;

import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

public interface BlogPostDAO {
    Flux<BlogPost> getAllPosts();
    Mono<BlogPost> addPost(final BlogPost blogPost);
    Mono<BlogPost> updatePost(final String id, final BlogPost blogPost);
    Mono<BlogPost> getPost(final String id);
    Mono<Void> deletePost(final String id);
    Flux<BlogPost> searchPostsByTitleContaining(String keyword);
    Flux<BlogPost> getChangeStreamPosts();
}
