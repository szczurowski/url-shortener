# url-shortener

## Non-functional requirements

1. In this Take Home Assignment, a small REST service is to be developed with C#. The
.Net Core SDK can be used to create the project.
2. Data should be stored in any relational database.
3. The database should be connected via Entity Framework.
4. The application should make meaningful log outputs based on the ILogger. Either a file in the file system or the console is sufficient as a log sink.
5. All interfaces of the service should be as RESTful as possible.
6. Optional: Connect Swagger

## Functional requirements

1. A “URL shortener” service is to be implemented. With the help of a REST interface, a theoretical front end (not part of this task) should be able to generate a shortened link for any URL.
2. For this purpose, the backend should assign a unique alphanumeric ID that is as short
as possible to each transferred URL. It should also be possible for the REST client to
set its own IDs. If this ID is already in use, a corresponding REST-compliant error
message should be returned.
3. When creating a new Short-URL, the REST client should also offer the option of
specifying a time-to-live (TTL). This determines how long a short URL can be reached
before it is deleted from the database. If no TTL is parameterized in the request, the
short URL will remain forever.
4. With an HTTP-GET request on the route of the ID (e.g. http://localhost:8080/id), the
service should redirect the REST client to the long URL. It should also be possible to
delete short URLs based on their IDs.