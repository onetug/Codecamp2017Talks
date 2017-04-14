# Step 1: Create SQL server docker image from Microsoft base image

- Install Docker
- Map C: drive
- Make sure Docker has at least 3.2GB RAM
- Install SQL server:

    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=My\$trongPa\$\$word" -p 1433:1433 -d -v c:/Temp/CodeCampData:/var/opt/mssql microsoft/mssql-server-linux

What is this doing?

`-e` = set environment variable

`-p` = publish port [host]:[container]

`-d` = detatch (run in background)

`-v` = map volume [host]:[container]

How do we know it's running?

    docker ps

Now let's run some SQL commands with sqlcmd:

    docker exec -it [container_name] /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P My$trongPa$$word
    1> SELECT name FROM sys.databases
    name
    ------
    master
    tempdb
    model
    msdb
    > exit

Seems like SQL is working, so now we can stop the image:

    docker stop [cid or name]

Let's write some SQL to create a database if it doesn't already exist:

    # /app/.start.sql:
    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'code_camp_demo') CREATE DATABASE code_camp_demo

And write a script to run the SQL (make sure to use UNIX line endings):

    # /app/start.sh:
    # wait for SQL server to start:
    sleep 45s
    # run SQL:
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -i /app/start.sql
    # loop forever so container doesn't stop:
    while [ 1 ]
    do
        sleep 2
    done
 
 let's make a Dockerfile to build an image based on the base SQL server image and run this SQL:

    # Dockerfile
    FROM microsoft/mssql-server-linux:latest

    # Copy startup files into container: 
    COPY /app/* /app/

    # Grant execute permission to scripts
    RUN chmod +x /app/*.sh

    # Run startup script to execute startup SQL:
    CMD /bin/bash /opt/mssql/bin/sqlservr.sh & /bin/bash /app/start.sh


Now let's write a script to build the image:

    # docker-build.sh
    docker build -t douglampe-cc17/mssql .

If you are going to build this image using CI (after you do git add):

    git update-index --chmod=+x docker-build.sh

Finally, let's build and run the image:

    ./docker-build.sh
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=My\$trongPa\$\$word" -p 1433:1433 -it -v c:/Temp/CodeCampData:/var/opt/mssql douglampe-cc17/mssql

Now we see code_camp_demo database created:

    2017-04-09 01:30:55.23 spid51      Starting up database 'code_camp_demo'.
    2017-04-09 01:30:56.11 spid51      Parallel redo is started for database 'code_camp_demo' with worker pool size [1].
    2017-04-09 01:30:56.16 spid51      Parallel redo is shutdown for database 'code_camp_demo' with worker pool size [1].

Stop container with ctrl-C