# Step 2: Create .Net core console app and Dockerize it

Let's create a .Net core console app:

    > mkdir src
    > cd src
    > mkdir CodeCamp2017.Cli
    > cd CodeCamp2017.Cli
    > dotnet new console
    > dotnet restore

Now let's run our console app:

    > dotnet run
    Hello World!

This isn't very interesting, so let's add some Nuget packages to let us connect to SQL:

    dotnet add package System.Data.SqlClient
    dotnet restore

Now let's talk to our SQL server running in Docker:

    Console.WriteLine("Hello World!");

    // Get password from environment variable:
    var password = Environment.GetEnvironmentVariable("SA_PASSWORD");
    var server = Environment.GetEnvironmentVariable("SQL_SERVER");

    using (SqlConnection conn = new SqlConnection($"Server={server};Database=master;User Id=sa;Password={password}"))
    {
        using (SqlCommand cmd = conn.CreateCommand())
        {
            Console.WriteLine($"Connecting to {server} with password {password}");
            // Wait for SQL to be available:
            while (true)
            {
                try
                {
                    conn.Open();
                    break;
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                    Console.WriteLine("Waiting for SQL...");
                    System.Threading.Thread.Sleep(10000);
                }
            }

            // Create a table if it doesn't exist:
            cmd.CommandText = "USE code_camp_demo IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'code_camp_data' AND type = 'U') CREATE TABLE code_camp_data (data_key nvarchar(10) NOT NULL PRIMARY KEY, data_value nvarchar(MAX))";
            cmd.ExecuteNonQuery();

            // Insert or update a value:
            cmd.CommandText = "IF NOT EXISTS (SELECT 1 FROM code_camp_data WHERE data_key = 'foo') INSERT INTO code_camp_data VALUES('foo', 'bar') ELSE UPDATE code_camp_data SET data_value = 'bar' WHERE data_key = 'foo'";
            cmd.ExecuteNonQuery();

            // Insert data or update if already exists:
            cmd.CommandText = "SELECT data_value FROM code_camp_data WHERE data_key = 'foo'";
            var val = cmd.ExecuteScalar();
            Console.WriteLine($"The value of foo is {val}.");
        }
    }

Let's define a docker-compose file to start up SQL:

    #docker-compose-dev.yml
    version: '2'

    services:

      mssql:
        image: douglampe-cc17/mssql
          ports:
            - "1433:1433"
          environment:
            - ACCEPT_EULA=Y
            - SA_PASSWORD=My$$trongPa$$$$word
          volumes:
            - c:/Temp/CodeCampData:/var/opt/mssql

This creates a service named "mssql" and does all of the setup we did in step 1 using docker run. Note that in a docker-compose file, you have to escape $ in environment variables as $$ otherwise $NAME will be replaced with the value of the NAME environment variable which is blank by default.

Now we can start this service using docker-compose:

    > docker-compose -f docker-compose-dev.yml
    Creating network "2consoleapp_default" with the default driver
    Creating 2consoleapp_mssql_1

Now that SQL is started in our container, let's run our console app:

    > SET SQL_SERVER=localhost
    > SET SA_PASSWORD=My$trongPa$$word
    > dotnet run
    Hello World!
    Connecting to localhost with password My$trongPa$$word
    A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
    Waiting for SQL...
    A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)                                     
    Waiting for SQL...
    A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)                                     
    Waiting for SQL...
    A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
    Waiting for SQL...
    The value of foo is bar.

Now that our app is working as intended, we can stop SQL server:

    > docker-compose -f docker-compose-dev.yml down
    Stopping 2consoleapp_mssql_1 ... done
    Removing 2consoleapp_mssql_1 ... done
    Removing network 2consoleapp_default

And publish our app:

    > dotnet publish

And create a Dockerfile to run our application in a container:

    # Dockerfile
    FROM microsoft/dotnet:latest

    # Copy startup files into container: 
    COPY /src/* /app/

    # Build applicationi:
    WORKDIR /app
    COPY src/CodeCamp2017.Cli/bin/Debug/netcoreapp1.1/publish .

    ENTRYPOINT ["dotnet", "CodeCamp2017.Cli.dll"]

And build the container:

    > docker build -t douglampe-cc17/console .

Now let's make a docker-compose file to wire the 2 services together:

    # docker-compose.yml
    version: '2'

    services:

    mssql:
      image: douglampe-cc17/mssql
        environment:
          - ACCEPT_EULA=Y
          - SA_PASSWORD=My$$trongPa$$$$word
        volumes:
          - c:/Temp/CodeCampData:/var/opt/mssql

    console:
      image: douglampe-cc17/console
        environment:
        - SA_PASSWORD=My$$trongPa$$$$word
        - SQL_SERVER=mssql

Now our console service will be able to access SQL on a host named mssql which is the same as the service name of our SQL container. We can run both services like this:

    docker-compose -f docker-compose.yml up

Then we can stop with ctrl-C