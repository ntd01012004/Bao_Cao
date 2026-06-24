# Microservices + RabbitMQ + gRPC + API Gateway

Mini project ASP.NET Core minh hoa kien truc microservices:

- **API Gateway**: Diem vao duy nhat, Swagger UI de test API.
- **ProductService**: Microservice gRPC tra ve danh sach san pham.
- **OrderService**: Microservice REST tao don hang, publish event `OrderCreated` qua RabbitMQ.
- **InventoryService**: Microservice lang nghe RabbitMQ va ghi nhan dat hang ton kho.

> May hien tai dung .NET SDK 10, nen project target `net10.0`. Neu cai .NET 8 SDK, co the doi `TargetFramework` ve `net8.0`.

## Kien truc

```text
Client (Swagger)
      |
      v
 ApiGateway :5161
   |      |         \
   | gRPC | HTTP     \ HTTP
   v      v          v
Product  Order    Inventory
:5139    :5191      :5170
           |
           | publish OrderCreated
           v
        RabbitMQ :5672
           |
           | consume
           v
      InventoryService
```

## Cau truc thu muc

```text
24_6_2026/
  ApiGateway/           Gateway + Swagger + gRPC client
  ProductService/       gRPC server (products.proto)
  OrderService/         REST API + RabbitMQ publisher
  InventoryService/     RabbitMQ consumer + REST API
  docker-compose.yml    RabbitMQ container
```

## Chay project

### 1. Khoi dong RabbitMQ

```powershell
cd C:\Users\Lenovo\DemoNet8Api\24_6_2026
docker compose up -d
```

RabbitMQ Management UI: http://localhost:15672 (guest/guest)

### 2. Chay cac microservice (mo 4 terminal)

```powershell
cd C:\Users\Lenovo\DemoNet8Api\24_6_2026

dotnet run --project ProductService
dotnet run --project OrderService
dotnet run --project InventoryService
dotnet run --project ApiGateway
```

Hoac build ca solution:

```powershell
dotnet build MicroservicesRabbitGrpcDemo.slnx
```

## Test bang Swagger

Mo Swagger UI tai:

```text
http://localhost:5161/swagger
```

### Luong test goi y

1. **GET /api/products** — Lay danh sach san pham tu ProductService (gRPC).
2. **GET /api/products/{id}** — Lay chi tiet 1 san pham.
3. **POST /api/orders** — Tao don hang (OrderService publish event RabbitMQ).

```json
{
  "productId": 1,
  "quantity": 2,
  "customerName": "Nguyen Van A"
}
```

4. **GET /api/orders** — Xem danh sach don hang da tao.
5. **GET /api/inventory/reservations** — Xem ban ghi ton kho sau khi InventoryService nhan event tu RabbitMQ.

## Cong nghe su dung

| Thanh phan | Cong nghe |
|------------|-----------|
| API Gateway | ASP.NET Core, Swagger, gRPC Client, HttpClient |
| Product Service | gRPC (Grpc.AspNetCore) |
| Order Service | REST API, RabbitMQ.Client |
| Inventory Service | BackgroundService, RabbitMQ consumer |
| Message Broker | RabbitMQ |

## Port mac dinh

| Service | Port |
|---------|------|
| ApiGateway | 5161 |
| OrderService | 5191 |
| ProductService | 5139 (HTTP/2 gRPC) |
| InventoryService | 5170 |
| RabbitMQ | 5672 (AMQP), 15672 (UI) |

## Ghi chu

- Du lieu san pham, don hang va ton kho luu in-memory de project gon, khong can database.
- Neu RabbitMQ chua chay, OrderService van tao don hang nhung se loi khi publish event.
- InventoryService can RabbitMQ de nhan event; neu chua co message thi API reservations tra ve mang rong.
