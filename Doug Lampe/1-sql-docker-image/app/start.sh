# wait for SQL server to start:
sleep 45s
# run SQL:
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -i /app/start.sql
# loop forever so container doesn't stop:
while [ 1 ]
do
    sleep 2
done