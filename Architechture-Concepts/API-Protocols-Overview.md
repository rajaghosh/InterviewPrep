# API Protocols

## Overview

A visual reference to the major API technologies used in modern software architecture.

---

## REST

REST is an architectural style for designing networked applications, using stateless communication and standard HTTP methods.

**Flow:**
```
Client → HTTP URL → Server
              ↓
            JSON
```

---

## Webhooks

A webhook is a mechanism for one system to notify another system in real-time via HTTP callbacks when a specific event occurs.

**Flow:**
```
Your App → Register Webhooks to subscribe to Notifications → Webhook API
                                                                    ↓
                                                             Cloud Data
                                                                    ↓
                                                       Notify App as Configured
```

---

## GraphQL

GraphQL is a query language for APIs that allows clients to request only the data they need.

**Flow:**
```
Clients (mobile, desktop, browser etc)
        ↓
     GraphQL
        ↓
  ┌─────────────┐
  │  [api]      │
  │  Rest API   │
  │  [api]      │
  │  Rest API   │
  │  Server     │
  │  Backend    │
  │  Service    │
  └─────────────┘
```

---

## SOAP

SOAP is a protocol for exchanging structured information using XML.

**Structure:**
```
┌──────────────────────────┐
│       Message Body       │
│    XML Format            │
│  WSDL                    │
├──────────────────────────┤
│       SOAP Envelope      │
│       SOAP Header        │
├──────────────────────────┤
│       HTTP Protocol      │
└──────────────────────────┘
```

---

## WebSocket

WebSockets provide a full-duplex communication channel over a single, long-lived connection, allowing for real-time data exchange.

**Handshake Flow:**
```
Client → Request   →
       ← Handshake ←  Server
       ↔ WebSocket ↔
```

---

## gRPC

gRPC is a high-performance, open-source framework for RPCs using Protocol Buffers.

**Architecture:**
```
                      ┌──────────────────┐
                      │   gRPC Server    │
                      │   C++ Service    │
                      └──────────────────┘
                             ↑
              ┌──────────────┴──────────────┐
              │                             │
   ┌──────────────────┐         ┌──────────────────────┐
   │   gRPC Stub      │         │   gRPC Stub           │
   │   Ruby Client    │         │   Android-JavaClient  │
   └──────────────────┘         └──────────────────────┘
```

---

## MQTT

MQTT is a lightweight publish-subscribe messaging protocol designed for low-bandwidth, high-latency, or unreliable networks.

**Use Case:** IoT data transmission

**Flow:**
```
IoT Device → MQTT Broker → Computer
IoT Device → MQTT Broker → Mobile Device
```

---

## AMQP

AMQP is an open-standard protocol for message-oriented middleware, facilitating message routing, queuing, and delivery.

**Architecture:**
```
Producer → Publish → Exchange
                        ↓ Route
Consumer ← Consume ← Message Queue
```

---

## SSE (Server-Sent Events)

SSE is a simple and efficient standard for server-push notifications over an HTTP connection.

**Flow:**
```
        Connection
Client ←── Event ───── Server
       ←── Event ─────
       ←── Event ─────
        Close Conversation
```

---

## EDI (Electronic Data Interchange)

EDI is a set of standards for exchanging structured business data between organizations electronically without human intervention.

**Flow:**
```
BUYER'S          Purchase Order →         SUPPLIER'S
INTERNAL    ──────────────────────────→   INTERNAL
SYSTEM           ← Invoice               SYSTEM
```

---

## EDA (Event-Driven Architecture)

Event-Driven Architecture (EDA) is a trending software architecture pattern nowadays.

**Flow:**
```
Producer → ┌──────────────┐
           │  Consumer A  │
           └──────────────┘
           ┌──────────────┐
           │  Consumer B  │
           └──────────────┘
```

---

## API Technologies Wheel (Summary)

| Protocol | Type | Best For |
|---|---|---|
| **REST** | Request-Response | Standard web APIs, CRUD operations |
| **Webhooks** | Event-driven push | Real-time notifications, async events |
| **GraphQL** | Query Language | Flexible data fetching, reducing over/under-fetching |
| **SOAP** | XML-based Protocol | Enterprise systems, strict contracts |
| **WebSocket** | Full-duplex | Real-time bidirectional communication (chat, gaming) |
| **gRPC** | RPC / Protocol Buffers | High-performance microservices, low latency |
| **MQTT** | Pub/Sub | IoT devices, low-bandwidth networks |
| **AMQP** | Message Queue | Message routing, reliable delivery |
| **SSE** | Server Push | Live feeds, unidirectional real-time updates |
| **EDI** | Data Interchange | B2B structured document exchange |
| **EDA** | Event-Driven | Decoupled systems, async processing |
