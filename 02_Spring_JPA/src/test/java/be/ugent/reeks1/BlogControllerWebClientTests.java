package be.ugent.reeks1;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.reactive.AutoConfigureWebTestClient;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.web.reactive.server.WebTestClient;

import static org.springframework.http.HttpHeaders.ACCEPT;
import static org.springframework.http.MediaType.APPLICATION_JSON;
import static org.springframework.http.MediaType.APPLICATION_JSON_VALUE;
import static org.springframework.http.MediaType.APPLICATION_XML_VALUE;

// https://docs.spring.io/spring-boot/docs/2.1.5.RELEASE/reference/html/boot-features-testing.html
// Beveiliging: https://www.baeldung.com/spring-security-integration-tests
//              https://www.viralpatel.net/basic-authentication-spring-webclient/
//              https://docs.spring.io/spring-security/site/docs/5.2.0.RELEASE/reference/html/test-webflux.html
@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@AutoConfigureWebTestClient
@ActiveProfiles("test")
public class BlogControllerWebClientTests {
	/**
	 * By using SpringBootTest.WebEnvironment.RANDOM_PORT the test will start the server
	 * on a random port and Spring Boot automatically provides us with a WebTestClient.
	 */
	@LocalServerPort
	private int port;

	@Autowired
	private WebTestClient webClient;

	@Test
	public void testGetPosts() {
		this.webClient.get()
				.uri("/posts")
				.header(ACCEPT, APPLICATION_JSON_VALUE)
				.exchange()
				.expectStatus().isOk()
				.expectHeader().contentType(APPLICATION_JSON)
				.expectBodyList(BlogPost.class).hasSize(1)
				.contains(BlogPostDAOMemory.helloWorldPost);
	}

	@Test
	public void testGetPostJSON() {
		this.webClient.get()
				.uri("/posts/{id}", 1L)
				.header(ACCEPT, APPLICATION_JSON_VALUE)
				.exchange()
				.expectStatus().isOk()
				.expectHeader().contentType(APPLICATION_JSON_VALUE)
				.expectBody(BlogPost.class).isEqualTo(BlogPostDAOMemory.helloWorldPost);
	}

	@Test
	public void testGetPostXML() {
		this.webClient.get()
				.uri("/posts/{id}", 1L)
				.header(ACCEPT, APPLICATION_XML_VALUE)
				.exchange()
				.expectStatus().isOk()
				.expectHeader().contentTypeCompatibleWith(APPLICATION_XML_VALUE);
//                jackson-dataformat-xml does not provide automatic xml unmarshalling (yet) for web(flux)client
//                .expectBody(BlogPost.class).isEqualTo(BlogPostDAOMemory.helloWorldPost);
	}


	@Test
	public void testCreatePost() {
		this.webClient.post()
				.uri("/posts")
				.contentType(APPLICATION_JSON)
				.bodyValue(new BlogPost(2L, "Title", "Content"))
				//.body(BodyInserters.fromValue(new BlogPost(2L, "Title", "Content")))
				.exchange()
				.expectStatus().isCreated()
				.expectHeader().exists("Location")
				.expectBody().isEmpty();
	}

	@Test
	public void testGetUnExistingPost() {
		this.webClient.get()
				.uri("/posts/{id}", 99L)
				.exchange()
				.expectStatus().isNotFound();
	}

	@Test
	public void testUpdatePost() {
		BlogPost updatedPost = new BlogPost(2L, "new title", "new content");
		this.webClient.put()
				.uri("/posts/{id}", updatedPost.getId())
				.contentType(APPLICATION_JSON)
				.bodyValue(updatedPost)
//                .body(BodyInserters.fromValue(updatedPost))
				.exchange()
				.expectStatus().isNoContent();
	}

	@Test
	public void testDeletePost() {
		this.webClient.delete()
				.uri("/posts/{id}", 2L)
				.exchange()
				.expectStatus().isNoContent();

		this.webClient.get()
				.uri("/posts/{id}", 2L)
				.exchange()
				.expectStatus().isNotFound();
	}
}
