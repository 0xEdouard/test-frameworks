package be.ugent.reeks1;

import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.core.env.Environment;
import org.springframework.jdbc.datasource.embedded.EmbeddedDatabaseBuilder;
import org.springframework.jdbc.datasource.embedded.EmbeddedDatabaseType;
import org.springframework.security.config.Customizer;
import org.springframework.security.config.annotation.method.configuration.EnableGlobalMethodSecurity;
import org.springframework.security.config.annotation.method.configuration.EnableMethodSecurity;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.core.userdetails.User;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.core.userdetails.jdbc.JdbcDaoImpl;
import org.springframework.security.crypto.factory.PasswordEncoderFactories;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.provisioning.JdbcUserDetailsManager;
import org.springframework.security.provisioning.UserDetailsManager;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.security.web.csrf.CookieCsrfTokenRepository;
import org.springframework.security.web.csrf.CsrfTokenRequestAttributeHandler;
import org.springframework.security.web.util.matcher.AntPathRequestMatcher;

import javax.sql.DataSource;
import java.util.logging.Logger;

@Configuration
@EnableMethodSecurity(securedEnabled = true)
public class SecurityConfig {

    @Value("${users.admin.password}")
    private String adminPassword;

    @Value("${users.admin.username}")
    private String adminUsername;

    @Value("${users.admin.roles}")
    private String adminRoles;

    @Value("${users.admin.encoded_password}")
    private String adminEncodedPassword;

    @Autowired
    Environment environment;

    @Bean
    public DataSource datasource() {
        return new EmbeddedDatabaseBuilder()
                .setType(EmbeddedDatabaseType.H2)
                .addScript(JdbcDaoImpl.DEFAULT_USER_SCHEMA_DDL_LOCATION)
                .build();
    };

    // Configuration for jdbc authentication
    @Bean
    public UserDetailsManager users(DataSource datasource) {
        PasswordEncoder encoder = PasswordEncoderFactories.createDelegatingPasswordEncoder();
        LoggerFactory.getLogger(SecurityConfig.class).info("Encoded password: " + encoder.encode(adminPassword));
        UserDetails admin = User.withUsername(adminUsername).password(encoder.encode(adminPassword)).roles(adminRoles).build();
        // Better use an externally hashed password to avoid clear text passwords in source or memory
        // https://docs.spring.io/spring-security/reference/features/authentication/password-storage.html#authentication-password-storage-boot-cli
        UserDetails admin2 = User.withUsername("admin2").password(adminEncodedPassword).roles(adminRoles).build();
        JdbcUserDetailsManager users = new JdbcUserDetailsManager(datasource);
        users.createUser(admin);
        users.createUser(admin2);
        return users;
    }

    // Configuration for in-memory authentication
//    @Bean
//    public InMemoryUserDetailsManager userDetailsService() {
//        UserDetails user = User.withUsername("test").password("test").roles("ADMIN").build();
//        return new InMemoryUserDetailsManager(user);
//    }

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
}
