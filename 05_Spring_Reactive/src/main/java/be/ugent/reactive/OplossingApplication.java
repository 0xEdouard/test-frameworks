package be.ugent.reactive;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import reactor.core.publisher.Flux;

@SpringBootApplication
public class OplossingApplication {

    public static void main(String[] args) {
        SpringApplication.run(OplossingApplication.class, args);
    }

    @Bean
    CommandLineRunner test(BlogPostRepository repository) {
        // Initialize the database with some test data
        return args -> {
            repository.deleteAll()
                    .thenMany(
                            Flux.just("Reactive Spring Boot", "Reactive Spring Data", "Reactive MongoDB")
                                    .map(title -> new BlogPost(null, title, "Some content ..."))
                                    .flatMap(repository::save)
                    )
                    .thenMany(repository.findAll())
                    .subscribe(System.out::println);
        };
    }
}
