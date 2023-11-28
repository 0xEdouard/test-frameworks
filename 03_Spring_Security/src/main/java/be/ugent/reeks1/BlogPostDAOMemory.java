package be.ugent.reeks1;

import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.stream.Collectors;

@Service
@Profile("test")
public class BlogPostDAOMemory implements BlogPostDAO {
    private final Map<Long, BlogPost> blogPosts = new HashMap<>();

    public final static BlogPost helloWorldPost = new BlogPost(1L, "Hello World", "Hello World!");
    private static Long counter = 2L;

    public BlogPostDAOMemory() {
        // Add dummy blog post to the collection
        this.blogPosts.put(helloWorldPost.getId(), helloWorldPost);
    }

    /**
     * Deel 1
     */
    public List<BlogPost> getAllPosts() {
        return new ArrayList<>(blogPosts.values());
    }

    /**
     * Deel 2
     */
    public void addPost(final BlogPost blogPost) {
        blogPost.setId(counter);
        counter++;
        blogPosts.putIfAbsent(blogPost.getId(), blogPost);
    }

    public void updatePost(final long id, final BlogPost blogPost) {
        blogPosts.put(id, blogPost);
    }


    public Optional<BlogPost> getPost(final long id) {
        return Optional.ofNullable(blogPosts.get(id));
    }

    public void deletePost(final long id) {
        blogPosts.remove(id);
    }

    @Override
    public List<BlogPost> searchPostsByTitleContaining(String keyword) {
        return blogPosts.values().stream().filter(post -> post.getTitle().contains(keyword)).collect(Collectors.toList());
    }
}
