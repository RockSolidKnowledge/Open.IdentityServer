providers=(\
  "SqlServer,Server=myServerAddress;Database=myDataBase;" \
  "MySql,Server=myServerAddress;Database=myDataBase;" \
  "PostgreSql,Host=localhost;Port=5432;Database=myDataBase;")

for con in "${providers[@]}"; do
    IFS=","; set -- $con    
    ./buildschema.sh $1 $2
done