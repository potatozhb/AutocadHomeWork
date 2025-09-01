<h3 style="font-size:24px">Requirements </h3>

1. Use C# for the entire implementation. 

2. Use a relational database for the data layer. 

3. Include instructions on how to run and test your API in a README file. 

4. Provide generated API documentation using an OpenAPI spec, or similar. 

5. Write Production ready code. 

--------------------------------------------------------------------------------

<h3 style="font-size:24px">Architecture:</h3>
<img width="1252" height="759" alt="image" src="https://github.com/user-attachments/assets/fd5ec626-5e47-4287-994d-661679d5bf06" />

K8S:
<img width="1230" height="718" alt="image" src="https://github.com/user-attachments/assets/f8f19bdc-dade-495d-81b8-ddd6aea192bd" />


<h3 style="font-size:24px">Endpoints:</h3>
<h4 style="font-size:24px">Base Url is: http://localhost:{5016 is for local test, depends on your test environment}</h4>
<h4 style="font-size:24px">Authentication: Bearer {your token} </h4>

1. POST /api/v1/auth/login 

Description: Login the system , after login success, use raw bear token for endpoints 3 and 4.  

**Request body** is Json, username will be used in other endpoints header **"x-userId"** value.

{ 

  "username":"string", 
  
  "password":"admin "  //hardcoded inside, have to be "admin", because no enrollment endpoint 

} 

**Response**

Success: (200) 

{

    "token": "jwt-token"
  
}

Failed: (401 Unauthorized)

{

    error information
  
}

2. GET /api/v1/Weather 

Description: Get all datas in the table. **use it for interview, it will show all informations include key.**

** Response(200) ** all data in an Json array

[

    {
    
        "id": "4bbbad7e-f1d5-4273-b6de-0f8c50a93cf2",
        
        "userId": "Joe",
        
        "rain": true,
        
        "createTime": "2025-08-30T23:52:09.6288805",
        
        "updateTime": "2025-08-30T23:52:09.6288806"
        
    }
    
]


3.  GET /api/v1/Weather/data?start={int?}&end={int?}

Description: Get all datas by **loged in user id**.

**Authorization**

Bearer jwt-token

**Header**

"x-userId":{loged in user name}

**Parameters**

optional start:{start index}

optional end:{end index}

**Response**

{

    "data": [
    
        {
        
            "timestamp": "2025-08-31T18:32:50.547942",
            
            
            "rain": true
            
        }
        
    ]
    
}


4. POST /api/v1/Weather/data

Description: Post a record by **loged in user id**.

**Authorization**

Bearer jwt-token

**Header**

"x-userId":{loged in user name}

**Request body**

{

     "rain":bool
     
}

**Response**

Success: (201) 

{
    "timestamp": "2025-08-31T18:32:50.547942Z",
    
    "rain": true
}

Failed: (401 Unauthorized)

{

    error information
  
}

Failed: (400 BadRequest)

{

    error information
  
}



<h3 style="font-size:24px">Rate limiting:</h3>

Clients are limited to 10 requests per 10 seconds per IP.

If exceeded, the API returns:


Response (429)

{

  "error": "Rate limit exceeded."
  
}


<h3 style="font-size:24px">Test it with InMemory DB:</h3>

1. set appsettings.Development.json->UseInMemoryDB to true. Base Url is: http://localhost:5016

2. run the service directly.

3. you will see the log "--> Using InMem Db", it means you are using InMemory DB.

4. you can test all the endpoints without any DB.

5. Default seed three records in it.


<h3 style="font-size:24px">Test it with local SQL Server:</h3>

1. set appsettings.Development.json->UserInMemoryDB to false. Base Url is: http://localhost:{your sql exposed port number, in my test, i use 18080}

2. set appsettings.Development.json->SqlServerConnection to your local SQL server connection string.

3. run the service directly. with command: docker run -p 18080:8080 -d {user name}/weatherservice

4. you will see the log "--> Using SqlServer Db", it means you are using SQL server.

5. the first time start the service, it will migrate db automatically.

6. you can test all the endpoints with database.

7. Default seed three records in it.

8. If first time run the service, need apply the migration: dotnet ef database update




<h3 style="font-size:24px">Release system by docker and K8S:</h3>

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
  1. use command to create a name: mssqlcad, key:SA_PASSWORD secret.  kubectl create secret generic mssqlcad --from-literal=SA_PASSWORD="Pa55w0rd!"
  2. you will see message "secret/mssqlcad created"

  --Create loadbalancer to enable local SQL Management visit released sql in K8S
  1. deploy three service to K8S.  kubectl apply -f mssql-depl.yaml
  2. you will see three new service running by command: kubectl get svc
  3. mssql-cad-depl, mssql-cad-clusterip-srv, mssql-cad-loadbalancer
  4. you can use local SQL Management to login the DB. username: localhost,14331 , password: Pa55w0rd!
  5. local sql string: Server=localhost,14330;Initial Catalog=Weather;User ID=sa;Password=Pa55w0rd!;TrustServerCertificate=True;
  6. production sql string: Server=mssql-cad-clusterip-srv,14331;Initial Catalog=Weather;User ID=sa;Password=Pa55w0rd!;TrustServerCertificate=True;
  
c. Test the service in K8S.
http://localhost:31333/api/v1/weather/data
