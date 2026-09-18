echo "Criando fila film-events no LocalStack SQS..."
awslocal sqs create-queue --queue-name film-events
echo "Fila film-events criada com sucesso!"
