package be.ugent.reeks1;

import java.util.List;
import java.util.Optional;

public interface BlogPostDAO {
    List<BlogPost> getAllPosts();
    void addPost(final BlogPost blogPost);
    void updatePost(final long id, final BlogPost blogPost);
    Optional<BlogPost> getPost(final long id);
    void deletePost(final long id);
    List<BlogPost> searchPostsByTitleContaining(String keyword);
}
