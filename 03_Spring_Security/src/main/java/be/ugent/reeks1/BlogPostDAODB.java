package be.ugent.reeks1;

import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service
@Profile("!test")
public class BlogPostDAODB implements BlogPostDAO {
    private final BlogPostRepository repository;

    public BlogPostDAODB(BlogPostRepository repository) {
        this.repository = repository;
    }

    @Override
    public List<BlogPost> getAllPosts() {
        return repository.findAll();
    }

    @Override
    public void addPost(BlogPost blogPost) {
        repository.save(blogPost);
    }

    @Override
    public void updatePost(long id, BlogPost blogPost) {
        repository.save(blogPost);
    }

    @Override
    public Optional<BlogPost> getPost(long id) {
        return repository.findById(id);
    }

    @Override
    public void deletePost(long id) {
        repository.deleteById(id);
    }

    @Override
    public List<BlogPost> searchPostsByTitleContaining(String keyword) {
        return repository.findByTitleContaining(keyword);
    }
}
