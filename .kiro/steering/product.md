# MyChair POS - Product Overview

MyChair POS is a comprehensive Point of Sale system designed for cafes and restaurants. The solution consists of multiple interconnected applications that handle order processing, inventory management, security, and operations monitoring.

## Core Applications

- **POS**: Main point-of-sale application for taking orders, processing payments, and managing customer transactions
- **POSAdmin**: Administrative interface for system configuration and management
- **OrdersMonitor (OPS)**: Orders Processing System for kitchen/service staff to track and manage order fulfillment
- **POS-C**: Customer-facing display variant
- **ShipData**: Data synchronization utility
- **PosServerCommands**: Server command processing service

## Key Features

- Order management with support for dine-in, takeout, and delivery
- Customer management with address and order history
- Inventory and stock tracking
- Receipt and label printing
- Voucher and promotional offer management
- Multi-operator support with role-based access
- Real-time order status tracking
- Split payment and invoice management
- Integration with online ordering systems

## Database Architecture

The system uses a database-first approach with two primary databases:
- **POS Database**: Handles point-of-sale transactions, invoices, customers, and inventory
- **OMAS Database**: Manages online order monitoring and service operations

## Security & Licensing

Custom licensing system with RSA encryption for application security and device-based license validation.
