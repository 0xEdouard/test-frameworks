# 1 Rest SPRING
## Dependency Injection
```java
@RestController  
public class BlogController {  
    private BlogPostDao blogPostDao;  
  
    public BlogPostController(BlogPostDao blogPostDao) {  
        this.blogPostDao = blogPostDao;  
    }  
  
    @GetMapping("/posts")  
    public List<BlogPost> getAllBlogPosts(){  
        return blogPostDao.getAllPosts();  
    }  
}
```

## Accept types
### application/XML
https://codeboje.de/spring-mvc-rest-api-xml-response/

```xml
<dependency>
    <groupId>com.fasterxml.jackson.dataformat</groupId>
    <artifactId>jackson-dataformat-xml</artifactId>
</dependency>
```
MAVEN RELOAD NIET VERGETEN
# 2 JPA
## actuator
```xml
<dependencies>
    <dependency>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-actuator</artifactId>
    </dependency>
</dependencies>
```
### metrics
in `application.properties` de metrics endpoint activeren
```js
management.endpoints.web.exposure.include=health,info,metrics
```

**aantal HTTP request vinden:**
`http://localhost:8080/actuator/metrics/http.server.requests`

**server *up time*:**
`http://localhost:8080/actuator/metrics/process.uptime`

#### counter
```java
import io.micrometer.core.instrument.Counter;
import io.micrometer.core.instrument.MeterRegistry;

// als attributen
private final Counter blogPostCreateCounter;

// in Controller constructor initialiseren
public ...Controller(..., MeterRegistry meterRegistry){
	blogPostCreateCounter = meterRegistry.counter("<name>", "<tag_key>", "tag_value>");
	/* BV:
	// http://localhost:8080/actuator/metrics/blogpost_total?tag=operation:created
	blogPostCreateCounter = meterRegistry.counter("blogpost_total", "operation", "created");  
	*/
}

// in methode
// @PostMapping(...)
public void create...(...){
	blogPostCreateCounter.increment();
}
```
tag naming convention: `http://localhost:8080/actuator/metrics/{metric_name}?tag={tag_key}:{tag_value}`

## tests
dependency toevoegen & maven reloaden
```xml
<dependency>
    <groupId>org.springframework.boot</groupId>
    <artifactId>spring-boot-starter-webflux</artifactId>
    <scope>test</scope>
</dependency>
```

klasse annotaties
```java
@SpringBootTest ( webEnvironment = SpringBootTest.WebEnvironment. RANDOM_PORT )  
@AutoConfigureWebTestClient
class ...Tests {
	@Autowired  
	private WebTestClient wtc;
}
```

### GET
#### JSON expect at least 1
```java
@Test  
public void testGetPosts() {  
   this.wtc.get()  
         .uri("/posts")  
         .header(ACCEPT, APPLICATION_JSON_VALUE)  
         .exchange()  
         .expectStatus().isOk()  
         .expectHeader().contentType(APPLICATION_JSON)  
         .expectBodyList(BlogPost.class).hasSize(1)  
         .contains(BlogPostDAOMemory.helloWorldPost);  
}
```
#### XML
```java
@Test  
void testGetPostXML(){  
   this.wtc.get()  
         .uri("/posts")  
         .header(ACCEPT, APPLICATION_XML_VALUE)  
         .exchange()  
         .expectStatus().isOk()  
         .expectHeader().contentTypeCompatibleWith(APPLICATION_XML_VALUE);  
}
```

#### 404, not found
```java
@Test  
public void testGetUnExistingPost() {  
   this.wtc.get()  
         .uri("/posts/{id}", 99L)  
         .exchange()  
         .expectStatus().isNotFound();  
}
```

### POST
```java
@Test  
void testCreatePost(){  
   this.wtc.post()  
         .uri("/posts")  
         .contentType(APPLICATION_JSON)  
         .bodyValue(new BlogPost(2L, "some Title", "some content"))  
         .exchange()  
         .expectStatus().isCreated()  
         .expectHeader().exists("Location")  
         .expectBody().isEmpty();  
}
```
### PUT
```java
@Test  
void testUpdatePost(){  
   BlogPost updatedPost = new BlogPost(2L, "new title", "new content")  
   this.wtc.put()  
         .uri("/posts/{id}", 99L)  
         .contentType(APPLICATION_JSON)  
         .bodyValue(updatedPost)  
         .exchange()  
         .expectStatus().isNoContent();  
}
```
### DELETE
```java
@Test  
void testDeleteBlogPost(){  
   this.wtc.delete()  
         .uri("/posts/{id}", 2L)  
         .exchange()  
         .expectStatus().isNoContent();

	this.wtc.get()  
         .uri("/posts/{id}", 2L)  
         .exchange()  
         .expectStatus().isNotFound();  
}
```
## Database
1. Repository verbinden 
```java
import org.springframework.data.jpa.repository.JpaRepository;  
  
public interface BlogPostRepository extends JpaRepository<BlogPost, Long> {  
}
```
2. ...DAODB injecten ermee & methodes overriden
```java
@Service  
// @Profile("!test")  
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
}
```
3. ! Vergeet in BlogPost de `@Entity` annotatie niet!
##### Profiles
bij MemoryDAO
```java
@Profile("test")
```

bij DBDAO
```java
@Profile("!test")
```

in testApp
```java
@ActiveProfiles("test")
```

# 3 (jdbc) Beveiliging
## Config
1. in de application properties de admin username & password definiëren
```xml
users.admin.encoded_pass=abcdef
```
3. Een SecurityConfig.java bestand maken
```java
@Configuration  
@EnableMethodSecurity(securedEnabled = true)  
public class SecurityConfig {...}
```
hierin zitten de volgende zaken:
- attributen die naar de waarden in .properties wijzen
```java
@Value("${users.admin.encoded_pass}")  
private String adminEncodedPassword;

@Value("${users.admin.roles}$")
private String adminRoles;
```


- Datasource waar credentials worden opgeslagen
```java
// waar credentials worden opgeslagen
@Bean  
public DataSource datasource() {  
    return new EmbeddedDatabaseBuilder()  
            .setType(EmbeddedDatabaseType.H2)  
            .addScript(JdbcDaoImpl.DEFAULT_USER_SCHEMA_DDL_LOCATION)  
            .build();
}
```
- UserDetailsManager die de gebruikers initialiseert
```java
// jdbc auth  
@Bean  
public UserDetailsManager users(DataSource datasource) {  
    PasswordEncoder encoder = PasswordEncoderFactories.createDelegatingPasswordEncoder();
    // encode not-encoded password too  
    UserDetails admin = User.withUsername(adminUsername).password(encoder.encode(adminPassword)).roles(adminRoles).build();  
    UserDetails admin2 = User.withUsername("admin2").password(adminEncodedPassword).roles(adminRoles).build();  
  
    JdbcUserDetailsManager users = new JdbcUserDetailsManager(datasource);  
    users.createUser(admin);  
    users.createUser(admin2);  
    return users;  
}
```
- URL auth
```java
// filter chain config  
@Bean  
public SecurityFilterChain filterChain(HttpSecurity http) throws Exception {  
    http.authorizeHttpRequests(requests -> requests  
                    .requestMatchers(new AntPathRequestMatcher("/admin.html")).hasRole("ADMIN")  
                    .anyRequest().permitAll())  
            .httpBasic(Customizer.withDefaults())  
            .csrf(csrf -> csrf  
  
                    .csrfTokenRepository(CookieCsrfTokenRepository.withHttpOnlyFalse())  
                    .ignoringRequestMatchers(new AntPathRequestMatcher("/h2-console/**"))  
                    .csrfTokenRequestHandler(new CsrfTokenRequestAttributeHandler())) // Disable BREACH  
            .headers(headers -> headers  
                    .frameOptions(frameOptionsConfig -> frameOptionsConfig.sameOrigin()));  
    if (environment.getActiveProfiles().length > 0 && environment.getActiveProfiles()[0].trim().equalsIgnoreCase("test")) {  
        http.csrf(csrf -> csrf.disable());  
    }  
    return http.build();  
}
```

## access restriction
in de Controller:
bv bij een POST:
```java
@PostMapping("/posts")  
@PreAuthorize("hasRole('ADMIN')")
public ResponseEntity<Void> addBlogPost(...){...}
```
bij PUT:
kan ook via `@Secured`, maar dat support geen *Spring Expression Language (SpEL)*
```java
@PutMapping("/posts/{id}")  
@Secured("ROLE_ADMIN")  
@ResponseStatus(HttpStatus.NO_CONTENT)  
public void updateBlogPost(...){...}
}
```

## tests met auth
in de WebTestClient login attributen toevoegen:
```java
@Value("${users.admin.password}")  
private String adminPassword;  
  
@Value("${users.admin.username}")  
private String adminUsername;
```
vervolgens kan men in de tests waar nodig het volgende toevoegen:
```java
...
.headers(header -> header.setBasicAuth(adminUsername, adminPassword))
...
```
volledig voorbeeld:
```java
@Test  
public void testDeletePost() {  
   this.webClient.delete()  
         .uri("/posts/{id}", 2L)  
         .headers(header -> header.setBasicAuth(adminUsername, adminPassword))  
         .exchange()  
         .expectStatus().isNoContent();
```

#### Postman
in Postman kan men met **xsrf tokens** werken, zie hiervoor
`Inhoud > Labo REST webservices met beveiliging en webapp - 18 oktober > slides`
dia 6 tem 8
=> kan door dit uit de cookies te halen & dit vervolgens als omgevingsvariabele in te stellen

## Search feature
allereerst in de ...Repository een methode toevoegen
```java
public interface BlogPostRepository extends JpaRepository<BlogPost, Long> {  
    List<BlogPost> findByTitleContaining(String keyword);  
  
}
```

vervolgens in de ...DAO interface een methode toevoegen
```java
public interface ...DAO {
	// ...
	List<BlogPost> searchPostsByTitleContaining(String keyword);
}
```

en dit dan implementeren in de DBDAO
```java
public class ...DBDAO implements ...DAO {
	//...
	@Override  
	public List<BlogPost> searchPostsByTitleContaining(String keyword) {  
	    return bpr.findByTitleContaining(keyword);  
	}
}
```
kan ook in de In-Memory db met H2:
```java
public class ...InMemoryDAO implements ...DAO {
	// ...
	@Override  
	public List<BlogPost> searchPostsByTitleContaining(String keyword) {  
	    return blogPosts.values().stream().filter(post -> post.getTitle().contains(keyword)).collect(Collectors.toList());  
	}
}
```

als laatste dit in de Controller toevoegen
```java
public ...Controller {
	// ...
	@GetMapping("/posts/search")  
	public List<BlogPost> searchPosts(@RequestParam(name = "q") String keyword){
	    return this.blogPostDao.searchPostsByTitleContaining(keyword);  
	}
}
```
dit zal er dan als `.../posts/search?q=...` als request gebruikt kunnen worden

## uitbreidingen (skipped)

# 4 Groepsopdracht (skipped)
# 5 Spring Reactive
## MongoDB
### Item klasse
heeft `@Document` annotatie & `@Id` bij Id attribuut (String !)

### Repository
```java
@Repository  
public interface BlogPostRepository extends ReactiveMongoRepository<BlogPost, String> {  
    Flux<BlogPost> findByTitleContaining(String keyword);  
}
```

## Controller
returnt nu `Flux<...>` objecten wegens async
bv
```java
@GetMapping("/posts")  
public Flux<BlogPost> getPosts() {  
    this.postsReadCounter.increment();  
    return postDAO.getAllPosts();  
}
```
! kan ook `Mono<...>` zijn als het slechts 1 object zou returnen
bv
```java
@GetMapping("/posts/{id}")  
public Mono<BlogPost> getPost(@PathVariable("id") String id) {  
    this.postsReadCounter.increment();  
    return postDAO.getPost(id).switchIfEmpty(Mono.error(new PostNotFoundException(id)));  
}
```

#### toevoegen / add
vb "De HTTP-response bevat de locatie header en HTTP-status  
201: created":
```java
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
```

#### verwijderen / delete
```java
@DeleteMapping("/posts/{id}")  
@ResponseStatus(HttpStatus.NO_CONTENT)  
public Mono<Void> deletePost(@PathVariable("id") String id) {  
    this.postsDeleteCounter.increment();  
    return postDAO.deletePost(id);  
}
```

### Error handling
eerst en vooral opnieuw een nieuwe exception class maken
```java
public class PostNotFoundException extends RuntimeException {  
    public PostNotFoundException(String id) {  
        super("Could not find post with id=" + id);  
    }  
}
```
vervolgens hier gebruik van maken in de Controller
```java
@ResponseStatus(HttpStatus.NOT_FOUND)  
@ExceptionHandler(PostNotFoundException.class)  
public void handleNotFound(Exception ex) {  
    logger.warn("Exception is: " + ex.getMessage());  
    // return empty 404  
}
```
#### ID niet gevonden
```java
@GetMapping("/posts/{id}")  
public Mono<BlogPost> getPost(@PathVariable("id") String id) {  
    this.postsReadCounter.increment();  
    return postDAO.getPost(id).switchIfEmpty(Mono.error(new PostNotFoundException(id)));  
}
```

#### UPDATE conflict
```java
@PutMapping("/posts/{id}")  
@ResponseStatus(HttpStatus.NO_CONTENT)  
public Mono<BlogPost> updatePost(@RequestBody BlogPost post, @PathVariable("id") String id) {  
    if (!id.equals(post.getId())) {  
        throw new ResponseStatusException(HttpStatus.CONFLICT, "ID in path does not match ID in the body");  
    }  
    this.postsUpdateCounter.increment();  
    return postDAO.updatePost(id, post);  
}
```
## stream
blocking I/O "omzeilen"
### 3 soorten GET delay
#### normale GET met delay 1 sec
```java
@GetMapping("/stream/posts-delay")  
public Flux<BlogPost> getPostsStreamV1() {  
    this.postsReadCounter.increment();  
    return postDAO.getAllPosts().delayElements(Duration.ofSeconds(1)).log();  
}
```
#### GET produces text/event-stream
```java
@GetMapping(value = "/stream/posts-text", produces = MediaType.TEXT_EVENT_STREAM_VALUE)  
public Flux<BlogPost> getPostsStreamV2() {  
    this.postsReadCounter.increment();  
    return postDAO.getAllPosts().delayElements(Duration.ofSeconds(1)).log();  
}
```
een 2e manier zou izjn om de client een Request te laten sturen met een specifieke header... (kan getest worden via Postman)
#### event stream subscription
stuurt een event voor elke aanpassing aan de tabel met \<items> in de db
```java
@GetMapping(value = "/stream/posts-json", produces = MediaType.APPLICATION_NDJSON_VALUE)  
public Flux<BlogPost> getPostsStreamV3() {  
    this.postsReadCounter.increment();  
    return postDAO.getAllPosts().delayElements(Duration.ofSeconds(1)).log();  
}
```
### in DAO & controller implementeren
in DAO interface
```java
Flux<BlogPost> getChangeStreamPosts();
```

in ...DAODB
```java
public Flux<BlogPost> getChangeStreamPosts() {  
    return reactiveMongoTemplate  
            .changeStream(BlogPost.class)  
            .watchCollection("posts")  
            .filter(Criteria.where("operationType").in("insert", "replace", "update"))  
            .listen()  
            .mapNotNull(ChangeStreamEvent::getBody);  
}
```

in controller
```java
@GetMapping(value = "/stream/posts", produces = MediaType.TEXT_EVENT_STREAM_VALUE)  
public Flux<BlogPost> getChangeStreamPosts() {  
    this.postsReadCounter.increment();  
    return postDAO.getChangeStreamPosts().log();  
}
```
# 6 JDBC
## SQL injection
gebruik 
`PrepareStatement`, `executeQuery` & `setInt/setString/...`
voorbeeld
```java
@Override  
public void addCustomer(ICustomer customer) throws DataExceptie {  
  
  
    try (Connection conn = openConnectie(); PreparedStatement stmt = conn.prepareStatement(insertCustomer)) {  
        stmt.setInt(1, customer.getCustomerNumber());  
        stmt.setString(2, customer.getCustomerName());  
        stmt.setString(3, customer.getContactLastName());  
        stmt.setString(4, customer.getContactFirstName());  
        stmt.setString(5, customer.getPhone());  
        stmt.setString(6, customer.getAddressLine1());  
        if (customer.getAddressLine2() != null && !customer.getAddressLine2().equals("")) {  
            stmt.setString(7, customer.getAddressLine2());  
        } else {  
            stmt.setNull(7, java.sql.Types.VARCHAR);  
        }  
        stmt.setString(8, customer.getCity());  
        if (customer.getState() != null && !customer.getState().equals("")) {  
            stmt.setString(9, customer.getState());  
        } else {  
            stmt.setNull(9, java.sql.Types.VARCHAR);  
        }  
        if (customer.getPostalCode() != null && !customer.getPostalCode().equals("")) {  
            stmt.setString(10, customer.getPostalCode());  
        } else {  
            stmt.setNull(10, java.sql.Types.VARCHAR);  
        }  
        stmt.setString(11, customer.getCountry());  
        if (customer.getSalesRepEmployeeNumber() != 0) {  
            stmt.setInt(12, customer.getSalesRepEmployeeNumber());  
        } else {  
            stmt.setNull(12, java.sql.Types.INTEGER);  
        }  
        if (customer.getCreditLimit() != 0) {  
            stmt.setDouble(13, customer.getCreditLimit());  
        } else {  
            stmt.setNull(13, java.sql.Types.DOUBLE);  
        }  
        stmt.executeUpdate();  
  
    } catch (SQLException ex) {  
        throw new DataExceptie(foutInsertCustomer);  
    }  
  
}
```

## TOT HIER GERAAKT

# Summary
## ID not found / Error handling
aparte RuntimeException klasse:
```java
package com.example.reeks1;  
  
public class BlogPostNotFoundException extends RuntimeException {  
    public BlogPostNotFoundException(Long id) {  
        super("Could not find post with id=" + id);  
    }  
}
```

implementatie in Controller:
```java
@ResponseStatus(HttpStatus.NOT_FOUND)  
@ExceptionHandler(BlogPostNotFoundException.class)  
public void handleNotFound(Exception ex){  
    System.out.println("Exception is: " + ex.getMessage());  
}
```

gebruik voorbeeld
```java
@GetMapping("/posts/{id}")  
public BlogPost getBlogPostById(@PathVariable("id") long id) {  
    return blogPostDao.getBlogPostById(id).orElseThrow(() -> new BlogPostNotFoundException(id));  
}
```
