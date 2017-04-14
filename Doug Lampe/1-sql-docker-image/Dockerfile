FROM microsoft/mssql-server-linux:latest

# Copy startup files into container: 
COPY /app/* /app/

# Grant execute permission to scripts
RUN chmod +x /app/*.sh

# Run startup script to execute startup SQL:
CMD /bin/bash /opt/mssql/bin/sqlservr.sh & /bin/bash /app/start.sh
