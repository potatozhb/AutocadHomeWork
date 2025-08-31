Requirements

Use C# for the entire implementation.
Use a relational database for the data layer.
Include instructions on how to run and test your API in a README file.
Provide generated API documentation using an OpenAPI spec, or similar.
Write Production ready code.

Architecture:
<img width="1264" height="808" alt="image" src="https://github.com/user-attachments/assets/ca678911-6b83-4aaf-b939-6342b5b17b26" />


K8S:
<img width="1230" height="718" alt="image" src="https://github.com/user-attachments/assets/f8f19bdc-dade-495d-81b8-ddd6aea192bd" />


Endpoints:
1. Login , after login success, use raw bear token for endpoints 3 and 4.

POST /api/v1/auth/login
{
  "username":"string",
  "password":"admin "  //hardcode inside, have to be "admin", because no enrollment endpoint
}

2. Get all datas in the table. use it for interview, it will show all informations include key.
GET /api/v1/Weather

3.  Get all datas by user id.
GET /api/v1/Weather/data?start={int?}&end={int?}

4. Post a record by user
POST /api/v1/Weather/data



Test it with InMemory DB:
1. set appsettings.Development.json->UserInMemoryDB to true.
2. run the service directly.
3. you will see the log "--> Using InMem Db", it means you are using InMemory DB.
4. you can test all the endpoints without any DB.
5. Default seed three records in it.


Test it with local SQL Server:
1. set appsettings.Development.json->UserInMemoryDB to false.
2. set appsettings.Development.json->SqlServerConnection to your local SQL server connection string.
3. run the service directly. with command: docker run -p 18080:8080 -d {user name}/weatherservice
4. you will see the log "--> Using SqlServer Db", it means you are using SQL server.
5. the first time start the service, it will migrate db automatically.
6. you can test all the endpoints with database.
7. Default seed three records in it.
8. If first time run the service, need apply the migration: dotnet ef database update



Release system by docker and K8S:

Go to K8S project. user terminal to follow next steps.

a. Deploy weatherservice to K8S.
  1. build service image. docker build -t {user name}/weatherservice .
  2. push image to docker hub. docker push {user name}/weatherservice
  3. change K8S -> weather-depl.yaml -> spec -> template-> spec-> containers -> image value to {user name}/weatherservice:latest
  4. run command: kubectl apply -f weather-depl.yaml
  5. deploy port service: kubectl apply -f weather-np-srv.yaml
  6. expose port number is 31333
  7. use http://localhost:31333 to test the service

b. Deploy SQL Server to K8S.
  1. deploy local volume by command: kubectl apply - f local-pvc.yaml
  2. you will see message "persistentvolumeclaim/mssql-weather created" for your first time deploy.
  3. use command: "kubectl get pvc" to check the result.

   ---Create SQL server strong password
  1. use command to create a name: mssqlcad, key:SA_PW secret.  kubectl create secret generic mssqlcad --from-literal=SA_PASSWORD="Pa55w0rd!"
  2. you will see message "secret/mssqlcad created"

  --Create loadbalancer to enable local SQL Management visit K8S db, release sql
  1. deploy three service to K8S.  kubectl apply -f mssql-depl.yaml
  2. you will see three new service running by command: kubectl get svc
  3. mssql-cad-depl, mssql-cad-clusterip-srv, mssql-cad-loadbalancer
  4. you can use local SQL Management to login the DB. username: localhost,14331 , password: pa55w0rd!
  5. local sql string: Server=localhost,14330;Initial Catalog=Weather;User ID=sa;Password=Pa55w0rd!;TrustServerCertificate=True;
  6. production sql string: Server=mssql-cad-clusterip-srv,14331;Initial Catalog=Weather;User ID=sa;Password=Pa55w0rd!;TrustServerCertificate=True;
  
c. Test the service in K8S.
http://localhost:31333/api/v1/weather/data
