


# Steps to run the solution:
1. Clone the project
2. Start RabbitMQ on Docker with the command
	docker run -d --hostname my-rabbit --name dev-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management
	This will start rabbitmq on localhost with the default username/password.
3. Open OrderService.sln and run the solution in vs. This will start the GRPC Server and create RabbitMQ connection and queues.
4. Open ProductService.sln and run the solution in vs. This will present a swagger ui in the browser to call the api endpoints for viewing available products and creating/updating orders. The product service will invoke the PlaceOrder and UpdateOrder methods on the OrdersService via GRPC.
5. Open NotificationService1.sln and NotificationService2.sln and run the solutions in vs. This will start a console app which connect to the rabbitmq broker and will show the notifications recieved in console while creating or updating orders request is processed by the Order service.


**Pre-Requisite**
- Docker should be installed and running on the system.
- .net 6 SDK and VS 2022 is required.
- The Products are maintained in ProductService\db\products.json file.
