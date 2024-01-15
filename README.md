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


# 7 ADO.NET

! gebruik steeds
`using (DbConnection connection = GetConnection()){...}`

vergeet ook niet in DataStorage de **connectie methodes** te implementeren & de hardcoded SQL uit een bestand te lezen.
## DataReaders
### lijst van alle <\klasse> tonen
in een ...Reader.cs klasse
alle customers tonen
```c#
public class DataStorageMetReader : DataStorage
{
    public List<Customer> GetCustomers()
    {
        List<Customer> list = new List<Customer>();
        using (DbConnection connection = GetConnection())
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = ConfigurationManager.AppSettings["SELECT_ALL_CUSTOMERS"];
            connection.Open();
            try
            {
                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    // Strings mogen null zijn in de database; dat veroorzaakt
                    // geen problemen.
                    // Maar int's en double's veroorzaken die wel!
                    Customer c = new Customer();
                    c.AddressLine1 = reader[ADDRESSLINE1].ToString();
                    c.AddressLine2 = reader[ADDRESSLINE2].ToString();
                    c.City = reader[CITY].ToString();
                    c.ContactFirstName = reader[CONTACTFIRSTNAME].ToString();
                    c.ContactLastName = reader[CONTACTLASTNAME].ToString();
                    c.Country = reader[COUNTRY].ToString();
                    if (!(reader[CREDITLIMIT] is DBNull))
                    {
                        c.CreditLimit = (double)reader[CREDITLIMIT]; // double
                    }
                    c.CustomerName = reader[CUSTOMERNAME].ToString();
                    /*if (!(reader[CUSTOMERNUMBER] is DBNull))
                    {
                        c.CustomerNumber = (int)reader[CUSTOMERNUMBER]; // int
                    }*/
                    c.CustomerNumber = (int)reader[CUSTOMERNUMBER]; // int
                    c.Phone = reader[PHONE].ToString();
                    c.PostalCode = reader[POSTALCODE].ToString();
                    if (!(reader[SALESREPEMPLOYEENUMBER] is DBNull))
                    {
                        c.SalesRepEmployeeNumber = (int)reader[SALESREPEMPLOYEENUMBER];
                    }
                    c.State = reader[STATE].ToString();
                    list.Add(c);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        return list;
    }
    ...
}
```

in `App.config` moet men ook de zaken in de ConfigurationManager instellen
```config
...
<appSettings>
	<add key="SELECT_ALL_CUSTOMERS" // <--
		 value="select * from customers"/>
...
```

#### controleren als de waarde null is

`if (!(reader[CREDITLIMIT] is DBNull))`


### x toevoegen

...Reader.cs klasse
addCustomer implementatie
```c#
public class DataStorageMetReader : DataStorage
{
public void AddCustomer(Customer customer)
{
    using (DbConnection connection = GetConnection())
    {
        DbCommand command = connection.CreateCommand();

        

        command.CommandText = ConfigurationManager.AppSettings["INSERT_ONE_CUSTOMER"];

        // naam, value
        command.Parameters.Add(MaakParameter("@" + CUSTOMERNAME, customer.CustomerName));
        command.Parameters.Add(MaakParameter("@" + ADDRESSLINE1, customer.AddressLine1));
        command.Parameters.Add(MaakParameter("@" + ADDRESSLINE2, customer.AddressLine2));
        command.Parameters.Add(MaakParameter("@" + CUSTOMERNUMBER, customer.CustomerNumber));
        command.Parameters.Add(MaakParameter("@" + CONTACTFIRSTNAME, customer.ContactFirstName));
        command.Parameters.Add(MaakParameter("@" + CONTACTLASTNAME, customer.ContactLastName));
        command.Parameters.Add(MaakParameter("@" + PHONE, customer.Phone));
        command.Parameters.Add(MaakParameter("@" + CITY, customer.City));
        command.Parameters.Add(MaakParameter("@" + STATE, customer.State));
        command.Parameters.Add(MaakParameter("@" + POSTALCODE, customer.PostalCode));
        command.Parameters.Add(MaakParameter("@" + COUNTRY, customer.Country));
        command.Parameters.Add(MaakParameter("@" + SALESREPEMPLOYEENUMBER, customer.SalesRepEmployeeNumber));
        command.Parameters.Add(MaakParameter("@" + CREDITLIMIT, customer.CreditLimit));

        connection.Open(); // niet vergeten!!!
        try
        {
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                errorMessages.Append("Index #" + i + "\n" +
                    "Message: " + ex.Errors[i].Message + "\n" +
                    "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                    "Source: " + ex.Errors[i].Source + "\n" +
                    "Procedure: " + ex.Errors[i].Procedure + "\n");
            }
            Console.WriteLine(errorMessages.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
}
```

in `App.config`
```config
<appSettings>
	...
	<add key="INSERT_ONE_CUSTOMER" // <--
		 value="insert into customers (customerNumber,customerName,contactLastName,contactFirstName,
     phone,addressLine1,addressLine2,city,state,postalCode,country,
     salesRepEmployeeNumber,creditLimit) 
     values(@customerNumber,@customerName,@contactLastName,@contactFirstName,
     @phone,@addressLine1,@addressLine2,@city,@state,@postalCode,@country,
     @salesRepEmployeeNumber,@creditLimit)"/>
	<add key="SELECT_ORDERS_OF_CUSTOMER"
		 value="select * from orders where customerNumber = @customerNumber"/>
```

### x toevoegen met rollback


in ...Reader.cs
merk vooral de Transaction & rollback op
```c#
...
public void AddOrder(Order order)
{
    using (DbConnection connection = GetConnection())
    {
        connection.Open();

        DbTransaction transaction = connection.BeginTransaction();

        DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = ConfigurationManager.AppSettings["INSERT_ONE_ORDER"];
        command.Parameters.Add(MaakParameter("@" + ORDERNUMBER, order.Number));
        command.Parameters.Add(MaakParameter("@" + ORDERDATE, order.Ordered));
        command.Parameters.Add(MaakParameter("@" + REQUIREDDATE, order.Required));
        command.Parameters.Add(MaakParameter("@" + SHIPPEDDATE, order.Shipped));
        command.Parameters.Add(MaakParameter("@" + STATUS, order.Status));
        command.Parameters.Add(MaakParameter("@" + COMMENTS, order.Comments));
        command.Parameters.Add(MaakParameter("@" + CUSTOMERNUMBER, "" + order.CustomerNumber));


        try
        {
            command.ExecuteNonQuery();
            int i = 0;
            foreach (OrderDetail detail in order.Details)
            {
                i++;
                DbCommand commandExtra = connection.CreateCommand();
                commandExtra.Transaction = transaction;

                commandExtra.CommandText = ConfigurationManager.AppSettings["INSERT_ORDERDETAILS"];
                commandExtra.Parameters.Add(MaakParameter("@" + ORDERNUMBER, detail.OrderNumber));
                commandExtra.Parameters.Add(MaakParameter("@" + ORDERLINENUMBER, detail.OrderLineNumber));
                commandExtra.Parameters.Add(MaakParameter("@" + PRICEEACH, detail.Price));
                // Hoe controleer je of de foutopvang goed is?
                // ANTWOORD:
                // Ik haal DE REGEL CODE HIERONDER weg, zodat er een fout ontstaat 
                // - controleer dat Order zelf NIET werd toegevoegd!!
                // Of, als het via unit test moet: je voegt een detail toe waarvan productcode in
                // main niet ingevuld is.
                commandExtra.Parameters.Add(MaakParameter("@" + PRODUCTCODE, detail.ProductCode));
                commandExtra.Parameters.Add(MaakParameter("@" + QUANTITYORDERED, detail.Quantity));
                commandExtra.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        catch (SqlException ex) // <--
        {
            transaction.Rollback(); // <--
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                Console.WriteLine(ex.Errors[i].Message);
            }
        }
        catch (Exception ex)
        {
            transaction.Rollback(); // <--
            Console.WriteLine(ex.Message);
        }
    }
}
```

## DataAdapter
geen DataSet nodig indien men maar met 1 tabel werkt


### beginstappen
allereerst een DAO object aanmaken! (zie CustomersTableDAO.cs )


ook een DataTable
de attributen & constructor
```c#
    class DataStorageMetDataTable : DataStorage
    {
        private DataTable table;

        private CustomersTableDao adapter; // nodig om dataTable aan te passen
        //...
        // constructor
		public DataStorageMetDataTable()
        {
            adapter = new CustomersTableDao(dbProviderFactory, GetConnection());
			// table = adapter.GetCustomersWithoutUpdate();

    }
```

### verwijderen uit db
(verwijderen gebeurt niet meteen)

```c#
public void DeleteCustomer(string customerNumber)
{
	// Duid enkel de customer aan; effectief weghalen uit de database gebeurt later 'en vrac'.
	DataRow? rij = table.Rows.Find(customerNumber);
	if (rij != null)
	{
		rij.Delete();
	}

}
```

### toevoegen in db
in afgeleide DataStorage klasse
```c#
public void AddCustomer(Customer customer)
{
	DataRow row = table.NewRow();

	row[ADDRESSLINE1] = customer.AddressLine1;
	row[ADDRESSLINE2] = customer.AddressLine2;
	row[CITY] = customer.City;
	row[CONTACTFIRSTNAME] = customer.ContactFirstName;
	row[CONTACTLASTNAME] = customer.ContactLastName;
	row[COUNTRY] = customer.Country;
	row[CREDITLIMIT] = customer.CreditLimit;
	row[CUSTOMERNAME] = customer.CustomerName;
	row[CUSTOMERNUMBER] = customer.CustomerNumber;
	row[PHONE] = customer.Phone;
	row[POSTALCODE] = customer.PostalCode;
	row[SALESREPEMPLOYEENUMBER] = customer.SalesRepEmployeeNumber;
	row[STATE] = customer.State;

	table.Rows.Add(row);
}
```
### definitief wijzigen
in DataStorageMetDataTable
```c#
        public void Update()
        {
            adapter.Update(table);
        }
                // Moest de gebruiker de Update vergeten vragen: 
        // gebeurt automatisch in finalizer / destructor.
        ~DataStorageMetDataTable()
        {
            Update();
        }
```

# 8 REST webservices met webAPI ASP.NET Core

## Model

verplichte velden?  (Id hoeft hierbij niet)
`[Required] annotatie`

nullable?
`public <type>? <naamAttribuut> { get; set; }`



vb:
`Nieuwsbericht.cs`
```c#
public class Nieuwsbericht
{
    public int? Id { get; set; }
    [Required] 
    public string Titel { get; set; }
    [Required]
    public string Bericht { get; set; }
    [Required]
    public DateTime Datum { get; set; }
}
```


## Controller
### in-memory

bevat meestal een Dictionary
in Data directory
```c#
public class NieuwsberichtRepository
{
    private Dictionary<int, Nieuwsbericht> berichten; // <--
    private int idTeller;

    public NieuwsberichtRepository()
    {
        berichten = new Dictionary<int, Nieuwsbericht>(); // <--
        idTeller = 0;
    }
    // ...
}
```

Zorg dat klasse slechts 1x wordt aangemaakt:
in `Program.cs`
```c#
builder.Services.AddSingleton<NieuwsberichtRepository>();
```

### API controller
in `Controllers` map

- erft van `: ControllerBase`
- heeft als annotaties boven klasse
```c#
[Route("api/[controller]")]
[ApiController]
```
- repo als attribuut  `NieuwsberichtRepository repository;` en in constructor!
#### GET
vb
```c#
// GET: api/<NieuwsberichtController>
[HttpGet]
public IEnumerable<Nieuwsbericht> Get()
{
	return repository.Messages;
}
```
#### met ID
```c#
// GET api/<NieuwsberichtController>/5
[HttpGet("{id}")]
public ActionResult<Nieuwsbericht> Get(int id)
{
	if (repository.IdExists(id))
	{
		return repository[id];
	} else
	{
		return NotFound();
	}
}
```

### POST
vb
```c#
// POST api/<NieuwsberichtController>
[HttpPost]
public ActionResult<Nieuwsbericht> Post(Nieuwsbericht bericht)
{
	Nieuwsbericht nieuwbericht = repository.Add(bericht);
	return CreatedAtAction("Get", new { id = nieuwbericht.Id }, nieuwbericht);
}
```

### PUT
```c#
// PUT api/<NieuwsberichtController>/5
[HttpPut("{id}")]
public IActionResult Put(int id, Nieuwsbericht nieuwsbericht)
{
	if (nieuwsbericht.Id == id)
	{
		repository.Update(nieuwsbericht);
		return NoContent();
	}

	else
	{
		// Use methods of ControllerBase to return status to client
		return BadRequest();
	}
}
```

### delete
```c#
// DELETE api/<NieuwsberichtController>/5
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
	if (repository.IdExists(id))
	{
		repository.Delete(id);
		// Use methods of ControllerBase to return status to client
		return NoContent();
	}
	else
	{
		// Use methods of ControllerBase to return status to client
		return NotFound();
	}
}
```

### DB Controller
Scaffolding item

## Swagger / OpenApi
zie Theorie
### kleine intro
in `Program.cs`
```c#
// ...
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NieuwsAPI", Version = "v1" });

    // Set the comments path for the Swagger JSON and UI.
    // Vergeet niet de xml file generation te enablen in <Project>.csproj // <------
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
}
    );
// ...
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

vergeet de summaries en dergelijke niet:
```c#
/// <summary>
/// Toont alle nieuwsberichten
/// </summary>
/// <returns>
/// Een collectie van nieuwsberichten
/// </returns>
```

### niet meer deel uitmaken van body
om *overposting* tegen te gaan

gewoon in het model hetgene optional maken:
```c#
public class Nieuwsbericht
{
    public int? Id { get; set; }
    //...
}
```



en in POST ziet dit er dan als volgt uit
```c#
/// <summary>
/// Voeg een nieuw nieuwsbericht toe
/// </summary>
/// <remarks>
/// Voorbeeld request:
/// 
///     POST api/NewsMessages
///     {
///         "titel": "tweede bericht",
///         "bericht": "inhoud bericht",
///         "datum": "2020-11-13T11:37:59.833Z"
///     }
///     
/// </remarks>
/// <param name="nieuwsbericht">nieuw nieuwsbericht</param>   


// POST: api/NieuwsberichtData
// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
[HttpPost]
public async Task<ActionResult<Nieuwsbericht>> PostNieuwsbericht(Nieuwsbericht nieuwsbericht)
{
    _context.Nieuwsbericht.Add(nieuwsbericht);
    await _context.SaveChangesAsync();

    return CreatedAtAction("GetNieuwsbericht", new { id = nieuwsbericht.Id }, nieuwsbericht);
}
```


# 9 ASP.NET MVC
## CRUD scaffolded controller
scaffolding = auto generaten

## Views aanpassen


## form validatie
in een View
```c#
<div class="row">
    <div class="col-md-4">
        <form asp-action="Create">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="Title" class="control-label"></label>
                <input asp-for="Title" class="form-control" />
                <span asp-validation-for="Title" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Message" class="control-label"></label>
                <input asp-for="Message" class="form-control" />
                <span asp-validation-for="Message" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Date" class="control-label"></label>
                <input asp-for="Date" class="form-control" />
                <span asp-validation-for="Date" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Create" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>
```
de `asp-validation` zal kijken nr de annotaties van dat attribuut & indien niet voldoet de errorMessage bij dat attribuut weergeven
## Localizer
in Controller
```c#
using Microsoft.Extensions.Localization;
// ...
	// attribuut
	private readonly IStringLocalizer<NewsMessagesController> _localizer;
	// in constructor
	public NewsMessagesController(TheaterContext context, IStringLocalizer<NewsMessagesController> localizer)
	{
    _context = context;
    _localizer = localizer;
	}
```

gebruik ervan
```c#
	// ...
	// GET: NewsMessages
	public async Task<IActionResult> Index()
	{
		ViewData["emptyMessage"] = _localizer["Geen nieuwsberichten op dit moment."]; 
		return View(await _context.NewsMessage.ToListAsync());
	}
```

bovenaan View (...cshtml)
```html
@model IEnumerable<Labo09_ASPNETMVC.Models.NewsMessage>
@using Microsoft.AspNetCore.Mvc.Localization // <!-- -->
@inject IViewLocalizer Localizer // <!-- -->
@{
    ViewData["Title"] = "Index";
//...
```
gebruik ervan
```html
<p><a asp-action="Create" class="btn btn-success">@Localizer["Nieuw bericht"]</a></p>
```

### Resources
men moet uiteraard ook nog de bestanden zelf hebben die de vertaling bevatten
dit zit vr een controller bv in `Resources/Controllers/NewsMessagesController.en.resx`

in `Program.cs` moet men er ook nog nr verwijzen
```c#
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
// ...
builder.Services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
// ...
var supportedCultures = new[] { "nl", "en", "en-US", "fr", "fr-FR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0]) // standaard nl
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
// ...
```

### Error messages vertalen
in Model zelf
vb
```c#
	[Display(Name = "Nieuwsbericht titel")]
	[Required(ErrorMessage = "Titel is vereist.")]
	public string Title { get; set; }
```

dan zullen er dus bestanden zijn met die keys die het vertalen

## authenticate & autorisatie
### paswoord requirements
`https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0`
in `Program.cs`
```c#
// ...
builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});
// ...
```


# 10 ASP.NET MVC 2.0
## OAuth
...



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
