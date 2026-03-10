# Requirements Document: Web-Based POS System

## Executive Summary

This document outlines the requirements for building a modern, cross-platform, web-based Point of Sale (POS) system for the HORECA industry. The new system will replace the existing WPF-based MyChair POS while maintaining backward compatibility during a transition period.

### Project Goals
- Create a responsive, cross-platform POS accessible from any device
- Support both on-premises and cloud deployment models
- Enable real-time collaboration across multiple stations
- Maintain business continuity during migration from legacy system
- Reduce hardware costs and improve operational flexibility

### Success Criteria
- Feature parity with current WPF POS system within 12 months
- Support 10+ concurrent stations without performance degradation
- Offline-capable operation with automatic sync when online
- Zero data loss during migration from legacy system
- Staff training completed within 2 weeks per location

---

## Business Context

### Current System Limitations
- **Platform Lock-in**: Windows-only, limiting device choices
- **Hardware Costs**: Expensive Windows licenses and PCs required
- **Mobility Constraints**: Limited tablet support, no mobile ordering
- **Maintenance Burden**: 10+ year old codebase with high technical debt
- **Integration Challenges**: Difficult to integrate with modern delivery platforms

### Market Opportunity
- Modern HORECA businesses expect tablet-based ordering
- Delivery integration (Uber Eats, Deliveroo) is now standard
- Cloud-based analytics and multi-location management are competitive advantages
- Lower total cost of ownership attracts small to medium businesses

---

## Stakeholders

### Primary Users
1. **Cashiers** - Process orders and payments at counter
2. **Waiters/Servers** - Take orders at tables using tablets
3. **Kitchen Staff** - View and manage order preparation
4. **Managers** - Monitor operations, generate reports
5. **Business Owners** - Access analytics, manage multiple locations

### Secondary Users
6. **System Administrators** - Configure and maintain system
7. **Support Staff** - Troubleshoot issues
8. **Delivery Drivers** - View order details (future)

---

## User Stories

### Epic 1: Order Management

#### US-1.1: Create New Order (Cashier)
**As a** cashier  
**I want to** create a new order by selecting products from the catalog  
**So that** I can process customer purchases quickly

**Acceptance Criteria:**
- Product catalog displays with categories and search
- Products can be added to cart with quantity adjustment
- Cart shows running total with tax breakdown
- Order can be saved as pending or completed
- System responds within 500ms for product selection

#### US-1.2: Table-Side Ordering (Waiter)
**As a** waiter  
**I want to** take orders at the table using a tablet  
**So that** I can provide faster service and reduce errors

**Acceptance Criteria:**
- Tablet interface optimized for touch
- Table number can be assigned to order
- Order syncs to kitchen display in real-time
- Waiter can add items to existing table orders
- Works offline with sync when connection restored

#### US-1.3: Pending Orders Management
**As a** cashier or waiter  
**I want to** save orders as pending and retrieve them later  
**So that** I can handle interrupted transactions or prepare orders in advance

**Acceptance Criteria:**
- Orders can be saved with customer name/table number
- Pending orders list shows all incomplete orders
- Pending orders can be searched by customer/table/time
- Pending order can be loaded and completed
- Pending orders sync across all stations


#### US-1.4: Order Modification
**As a** waiter or cashier  
**I want to** modify an existing order by adding or removing items  
**So that** I can accommodate customer changes

**Acceptance Criteria:**
- Existing orders can be reloaded for modification
- Order is locked when being edited to prevent conflicts
- Other users see "Order in use by [User]" indicator
- Lock is released after 5 minutes of inactivity
- Changes sync to kitchen display immediately

#### US-1.5: Split Orders
**As a** cashier  
**I want to** split a single order into multiple invoices  
**So that** customers can pay separately

**Acceptance Criteria:**
- Order items can be distributed across multiple invoices
- Each split invoice shows correct totals and tax
- Original order is marked as split
- All split invoices reference original order

### Epic 2: Payment Processing

#### US-2.1: Multiple Payment Methods
**As a** cashier  
**I want to** accept cash, card, and voucher payments  
**So that** I can accommodate customer preferences

**Acceptance Criteria:**
- Cash payment with change calculation
- Card payment integration
- Voucher/gift card redemption
- Split payment across multiple methods
- Payment receipt generated automatically

#### US-2.2: Discount Application
**As a** cashier or manager  
**I want to** apply discounts to orders  
**So that** I can honor promotions and special pricing

**Acceptance Criteria:**
- Percentage or fixed amount discounts
- Item-level or order-level discounts
- Promotional offers applied automatically
- Manager approval required for discounts over threshold
- Discount reason captured for reporting

### Epic 3: Kitchen Display System

#### US-3.1: Real-Time Order Display
**As a** kitchen staff member  
**I want to** see new orders appear immediately on the kitchen display  
**So that** I can start preparation without delay

**Acceptance Criteria:**
- New orders appear within 2 seconds of submission
- Orders color-coded by urgency (time since placed)
- Orders grouped by category (drinks, food, desserts)
- Audio/visual alert for new orders
- Display works on tablets or large monitors

#### US-3.2: Order Status Management
**As a** kitchen staff member  
**I want to** mark orders as in-progress or completed  
**So that** I can track preparation workflow

**Acceptance Criteria:**
- Orders can be marked as "Preparing", "Ready", "Delivered"
- Status updates visible to all staff in real-time
- Completed orders removed from display after 5 minutes
- Order history accessible for reference

### Epic 4: Customer Management

#### US-4.1: Customer Lookup
**As a** cashier  
**I want to** search for existing customers  
**So that** I can apply loyalty benefits and track order history

**Acceptance Criteria:**
- Search by name, phone, or customer ID
- Recent customers shown first
- Customer order history displayed
- Loyalty points/rewards shown

#### US-4.2: New Customer Registration
**As a** cashier  
**I want to** register new customers quickly  
**So that** I can capture contact information without slowing checkout

**Acceptance Criteria:**
- Minimal required fields (name, phone)
- Optional fields (email, address, birthday)
- Duplicate detection by phone number
- Customer added to database immediately

### Epic 5: Reporting & Analytics

#### US-5.1: Daily Sales Report
**As a** manager  
**I want to** view daily sales summary  
**So that** I can monitor business performance

**Acceptance Criteria:**
- Total sales, orders, and average order value
- Sales by payment method
- Sales by category
- Hourly breakdown
- Export to PDF/Excel

#### US-5.2: Inventory Tracking
**As a** manager  
**I want to** track stock levels and receive low-stock alerts  
**So that** I can prevent stockouts

**Acceptance Criteria:**
- Real-time stock levels
- Low stock threshold alerts
- Stock movement history
- Automatic stock deduction on order completion

### Epic 6: Multi-Station Coordination

#### US-6.1: Server Command System
**As a** device/tablet user  
**I want to** request print operations from the master station  
**So that** I can print receipts without direct printer access

**Acceptance Criteria:**
- Commands queued: PrintInvoice, PrintReceipt, PrintLabels, etc.
- Master station processes commands in order
- Command status visible to requester
- Failed commands can be retried
- Command history maintained for audit

#### US-6.2: Order Locking
**As a** system  
**I want to** prevent simultaneous editing of the same order  
**So that** changes don't conflict or get lost

**Acceptance Criteria:**
- Order locked when opened for editing
- Lock shows user name and timestamp
- Lock expires after 5 minutes of inactivity
- User can force-unlock their own orders
- Manager can unlock any order

### Epic 7: Offline Capability

#### US-7.1: Offline Order Creation
**As a** waiter using a tablet  
**I want to** create orders when network is unavailable  
**So that** service continues during connectivity issues

**Acceptance Criteria:**
- Orders created and stored locally
- Clear indicator of offline mode
- Orders sync automatically when online
- Conflict resolution for duplicate orders
- No data loss during offline period

#### US-7.2: Offline Product Catalog
**As a** any user  
**I want to** access product catalog offline  
**So that** I can continue taking orders

**Acceptance Criteria:**
- Product catalog cached locally
- Images and prices available offline
- Catalog updates when online
- Cache size limited to prevent storage issues

### Epic 8: Hardware Integration

#### US-8.1: Receipt Printing
**As a** cashier  
**I want to** print receipts on thermal printers  
**So that** I can provide customers with proof of purchase

**Acceptance Criteria:**
- Print via WebUSB (Chrome/Edge) or print server
- Receipt format matches current system
- Automatic retry on print failure
- Print queue for multiple receipts

#### US-8.2: Cash Drawer Integration
**As a** cashier  
**I want to** open cash drawer automatically on cash payment  
**So that** workflow is seamless

**Acceptance Criteria:**
- Drawer opens via printer kick-out or USB
- Manual open option for managers
- Drawer open events logged

### Epic 9: System Administration

#### US-9.1: User Management
**As a** system administrator  
**I want to** create and manage user accounts  
**So that** I can control system access

**Acceptance Criteria:**
- Create users with roles (Cashier, Waiter, Manager, Admin)
- Assign permissions by role
- Deactivate users without deleting history
- Password reset functionality

#### US-9.2: Configuration Management
**As a** system administrator  
**I want to** configure system settings via web interface  
**So that** I don't need database access

**Acceptance Criteria:**
- Feature flags (enable/disable features)
- Printer configuration
- Tax rates and service charges
- Business hours and holidays
- Changes take effect immediately or on next login

---

## Functional Requirements

### FR-1: Cross-Platform Compatibility
- System must run on Windows, macOS, iOS, Android, and Linux
- Responsive design adapts to screen sizes from 7" tablets to 27" monitors
- Touch-optimized interface for tablets
- Keyboard shortcuts for desktop users

### FR-2: Performance
- Page load time < 2 seconds on 4G connection
- Product search results < 500ms
- Order submission < 1 second
- Support 10+ concurrent users per location
- Handle 1000+ orders per day per location

### FR-3: Data Integrity
- All financial transactions use database transactions
- Automatic backup every 24 hours
- Point-in-time recovery capability
- Audit trail for all data changes
- No data loss during system failures

### FR-4: Security
- HTTPS required for all connections
- Role-based access control (RBAC)
- Session timeout after 30 minutes inactivity
- Password complexity requirements
- Failed login attempt lockout

### FR-5: Integration
- RESTful API for third-party integrations
- Webhook support for real-time events
- Export data to CSV/Excel/PDF
- Import products from CSV
- Integration with delivery platforms (future)

---

## Non-Functional Requirements

### NFR-1: Availability
- 99.9% uptime during business hours
- Planned maintenance during off-hours only
- Graceful degradation when backend unavailable
- Automatic reconnection after network interruption

### NFR-2: Scalability
- Support 50+ locations from single deployment
- Handle 100+ concurrent users across all locations
- Database can grow to 10M+ orders
- Horizontal scaling for API servers

### NFR-3: Usability
- New users productive within 1 hour of training
- Common tasks completable in < 5 clicks
- Error messages clear and actionable
- Consistent UI patterns across all screens

### NFR-4: Maintainability
- Modular architecture for easy updates
- Comprehensive logging for troubleshooting
- Automated testing (unit, integration, E2E)
- Documentation for all APIs and components

### NFR-5: Compatibility
- Backward compatible with existing database schema
- Can coexist with legacy WPF POS during transition
- Support gradual migration over 6-12 months
- Data export compatible with accounting software

---

## Technical Constraints

### TC-1: Database
- Must use existing SQL Server database
- Cannot break existing WPF POS during transition
- Schema changes must be backward compatible
- Support both systems accessing same database

### TC-2: Deployment
- On-premises deployment on local network
- Self-signed SSL certificate acceptable for local network
- Must work without internet connection
- Optional cloud sync for multi-location

### TC-3: Hardware
- Support existing receipt printers (ESC/POS protocol)
- Support existing cash drawers
- Support existing label printers
- Work with consumer-grade tablets and PCs

### TC-4: Browser Support
- Chrome/Edge (Chromium) - primary
- Safari (iOS) - secondary
- Firefox - secondary
- Internet Explorer - not supported

---

## Migration Requirements

### MR-1: Parallel Operation
- New web POS and legacy WPF POS must coexist
- Both systems use same database
- No data conflicts between systems
- Gradual station-by-station migration

### MR-2: Data Migration
- All historical data accessible from new system
- No data loss during migration
- Pending orders transferable between systems
- Customer data fully migrated

### MR-3: Training
- Training materials for all user roles
- Video tutorials for common tasks
- In-app help and tooltips
- Support hotline during transition

### MR-4: Rollback Plan
- Ability to revert to legacy system if needed
- Data created in new system accessible from legacy
- No permanent changes to database schema during pilot

---

## Success Metrics

### Business Metrics
- Order processing time reduced by 30%
- Hardware costs reduced by 50% over 3 years
- Staff training time reduced by 60%
- Customer satisfaction score improved by 20%
- System downtime reduced by 80%

### Technical Metrics
- 99.9% uptime achieved
- Average response time < 500ms
- Zero data loss incidents
- 90% code coverage in tests
- < 5 critical bugs per month in production

---

## Risks & Mitigation

### Risk 1: Hardware Integration Challenges
**Impact**: High  
**Probability**: Medium  
**Mitigation**: 
- Early testing with all printer models
- Fallback to print server if WebUSB fails
- Maintain legacy system as backup

### Risk 2: Performance Issues with Multiple Stations
**Impact**: High  
**Probability**: Low  
**Mitigation**:
- Load testing with 20+ concurrent users
- Database query optimization
- Caching strategy for frequently accessed data
- SignalR connection pooling

### Risk 3: Staff Resistance to Change
**Impact**: Medium  
**Probability**: Medium  
**Mitigation**:
- Involve staff in design process
- Gradual rollout with pilot stations
- Comprehensive training program
- Keep legacy system available during transition

### Risk 4: Data Synchronization Conflicts
**Impact**: High  
**Probability**: Medium  
**Mitigation**:
- Optimistic locking with conflict detection
- Last-write-wins with audit trail
- Manual conflict resolution UI for managers
- Extensive testing of concurrent scenarios

---

## Dependencies

### External Dependencies
- SQL Server 2016 or later
- .NET 8 runtime
- Modern web browser (Chrome/Edge/Safari)
- Network infrastructure (WiFi/Ethernet)

### Internal Dependencies
- Existing database schema (PosDbForAll)
- Legacy WPF POS (during transition)
- Print server (optional)
- SSL certificate for HTTPS

---

## Out of Scope (Phase 1)

The following features are explicitly out of scope for the initial release:

- Mobile apps (native iOS/Android)
- Cloud-hosted SaaS offering
- Multi-language support
- Advanced analytics and BI dashboards
- Integration with accounting software
- Customer-facing ordering kiosk
- Online ordering portal
- Delivery driver app
- Inventory management (beyond basic tracking)
- Employee scheduling
- Table reservation system

These features may be considered for future phases based on business priorities.

---

## Glossary

- **PWA**: Progressive Web App - web application that can be installed like a native app
- **Master Station**: Primary POS terminal that processes server commands and manages printers
- **Device Station**: Tablet or secondary terminal with limited functionality
- **Server Command**: Request from device station to master station for operations like printing
- **Pending Order**: Order saved but not yet completed/paid
- **Order Locking**: Mechanism to prevent simultaneous editing of same order
- **SignalR**: Real-time communication library for ASP.NET Core
- **HORECA**: Hotels, Restaurants, and Cafés industry

---

## Approval

This requirements document requires approval from:

- [ ] Business Owner
- [ ] Operations Manager
- [ ] Technical Lead
- [ ] Finance Manager

**Document Version**: 1.0  
**Last Updated**: 2026-02-25  
**Next Review**: After Phase 1 completion
