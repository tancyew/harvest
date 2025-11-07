# Harvest - Supplier to Customer Direct Ordering Platform

## Project Overview

Harvest is a web-based platform that bridges the gap between **suppliers (farmers)** and **customers (hawkers/retailers)** by enabling direct product ordering and transactions. The platform eliminates intermediaries, allowing hawkers to purchase fresh produce and products directly from farmers through an intuitive web interface.

## Technology Stack

- **Framework**: Blazor Server / Blazor WebAssembly (.NET 9)
- **Language**: C#
- **UI Framework**: [MudBlazor](https://mudblazor.com/) - Material Design components for Blazor
- **Database**: SQLite (local persistence)
- **Authentication**: OTP-based authentication (initial dummy implementation)
- **PDF Generation**: For invoice generation
- **Email**: For invoice and notification delivery

## Architecture

### Application Type
- Blazor web application (Server or WebAssembly based on requirements)
- Component-based architecture following Blazor patterns
- SQLite database for data persistence

### User Roles

1. **Customer (Hawker/Retailer)**
   - Browse products
   - Place orders
   - View invoices
   - Make payments
   - Receive custom pricing

2. **Supplier (Farmer)** _(Phase 2)_
   - Manage product catalog
   - Update inventory
   - View and fulfill orders
   - Issue invoices

3. **Admin**
   - Manage users and roles
   - Configure pricing rules
   - Oversee operations

## Core Modules

### 1. Authentication & Authorization
**Status**: Priority Module

- **Role-based authentication** with user types:
  - Customer (Hawker)
  - Supplier (Farmer)
  - Admin
- **OTP-based login** for customers/users
  - Initial implementation: Dummy OTP (e.g., always accept "123456")
  - Future: SMS/Email OTP integration
- Session management
- Password recovery (future)

**Key Entities**:
- `User` (UserId, Email, Phone, Role, UserType, IsActive, CreatedDate)
- `OtpCode` (OtpId, UserId, Code, ExpiryTime, IsUsed)

---

### 2. Supplier Management
**Status**: Phase 2

- Supplier registration and profile management
- Business information (farm name, location, contact)
- Supplier verification status
- Product catalog management per supplier

**Key Entities**:
- `Supplier` (SupplierId, UserId, BusinessName, Location, ContactInfo, VerificationStatus)

---

### 3. Product Management
**Status**: Priority Module

- Product catalog with categories
- **Multiple product images** support
- Product details:
  - Name, description, category
  - Unit of measure (kg, pcs, box, etc.)
  - Base price
  - SKU/Product code
- Product status (Active/Inactive)
- Search and filter functionality

**Key Entities**:
- `Product` (ProductId, Name, Description, Category, UnitOfMeasure, BasePrice, SKU, IsActive)
- `ProductImage` (ImageId, ProductId, ImageUrl, IsPrimary, DisplayOrder)

---

### 4. Inventory Management
**Status**: Priority Module

- Real-time stock tracking
- Stock levels per product
- Low stock alerts
- Inventory adjustments
- Stock movement history
- Multi-location inventory (future enhancement)

**Key Entities**:
- `Inventory` (InventoryId, ProductId, QuantityAvailable, ReorderLevel, LastUpdated)
- `InventoryTransaction` (TransactionId, ProductId, Quantity, TransactionType, Date, Reference)

---

### 5. Sales Orders
**Status**: Priority Module

- Customer order placement
- Order status workflow:
  - Pending → Confirmed → Packed → Shipped → Delivered → Completed
  - Cancellation handling
- Order line items with quantities
- Order summary and history
- Order search and filtering
- Delivery scheduling

**Key Entities**:
- `SalesOrder` (OrderId, CustomerId, OrderDate, Status, TotalAmount, DeliveryDate)
- `OrderLineItem` (LineItemId, OrderId, ProductId, Quantity, UnitPrice, DiscountAmount, LineTotal)

---

### 6. Invoicing
**Status**: Priority Module

- Automatic invoice generation from orders
- **PDF invoice generation**
- **Email invoice delivery**
- Invoice numbering system (auto-increment with prefix)
- Invoice details:
  - Customer info
  - Line items with pricing
  - Tax calculations (if applicable)
  - Payment terms
- Invoice status (Draft, Sent, Paid, Overdue)
- Invoice history and reprinting

**Key Entities**:
- `Invoice` (InvoiceId, OrderId, InvoiceNumber, InvoiceDate, DueDate, Status, TotalAmount)
- `InvoiceLineItem` (LineItemId, InvoiceId, ProductId, Description, Quantity, UnitPrice, Amount)

---

### 7. Payment Management
**Status**: Priority Module

- Payment recording and tracking
- Payment methods:
  - Cash
  - Bank Transfer
  - Credit/Debit Card (future)
  - Digital wallet (future)
- Partial payment support
- Payment status tracking
- Payment history
- Outstanding balance calculation
- Payment receipt generation

**Key Entities**:
- `Payment` (PaymentId, InvoiceId, PaymentDate, Amount, PaymentMethod, Status, Reference)
- `PaymentTransaction` (TransactionId, PaymentId, Amount, TransactionDate, Status)

---

### 8. Credit Notes
**Status**: Priority Module

- Issue credit notes for:
  - Order returns
  - Price adjustments
  - Damaged goods
  - Customer complaints
- Credit note approval workflow
- Credit application to customer account
- Credit note PDF generation
- Credit balance tracking

**Key Entities**:
- `CreditNote` (CreditNoteId, InvoiceId, CustomerId, CreditNoteNumber, IssueDate, Reason, Amount, Status)
- `CreditNoteLineItem` (LineItemId, CreditNoteId, ProductId, Quantity, UnitPrice, Amount)

---

### 9. Customer-Specific Pricing
**Status**: Priority Module

**Hybrid Pricing Model** - Supports flexible pricing strategies per customer:

#### Pricing Types:
1. **Direct Pricing**: Fixed price per product per customer
2. **Percentage Discount**: Discount % off base product price
3. **Tiered Pricing**: Volume-based pricing (future)
4. **Promotional Pricing**: Time-limited special prices (future)

#### Features:
- Default to base product price if no custom pricing exists
- Priority system when multiple pricing rules apply
- Effective date ranges for pricing
- Bulk pricing rule setup
- Pricing history and audit trail

**Key Entities**:
- `CustomerPricing` (PricingId, CustomerId, ProductId, PricingType, DirectPrice, DiscountPercentage, EffectiveFrom, EffectiveTo, Priority)
- `PricingHistory` (HistoryId, CustomerId, ProductId, OldPrice, NewPrice, ChangedDate, ChangedBy)

#### Pricing Calculation Logic:
```csharp
// Pseudocode for price calculation
decimal GetCustomerPrice(int customerId, int productId, decimal basePrice)
{
    var customerPricing = GetActiveCustomerPricing(customerId, productId);

    if (customerPricing == null)
        return basePrice; // Use base price

    if (customerPricing.PricingType == "DirectPricing")
        return customerPricing.DirectPrice;

    if (customerPricing.PricingType == "PercentageDiscount")
        return basePrice * (1 - customerPricing.DiscountPercentage / 100);

    return basePrice;
}
```

---

## Database Schema

### SQLite Database
- **Database File**: `harvest.db` (in application directory)
- **Migrations**: Entity Framework Core migrations
- **Schema Management**: Code-first approach

### Key Relationships
```
User (1) ──→ (Many) Customer
User (1) ──→ (1) Supplier
Customer (1) ──→ (Many) SalesOrder
Customer (1) ──→ (Many) CustomerPricing
SalesOrder (1) ──→ (Many) OrderLineItem
SalesOrder (1) ──→ (1) Invoice
Invoice (1) ──→ (Many) Payment
Invoice (1) ──→ (Many) CreditNote
Product (1) ──→ (Many) ProductImage
Product (1) ──→ (1) Inventory
Product (1) ──→ (Many) CustomerPricing
```

---

## Development Phases

### Phase 1: MVP (Core Customer Functionality)
- ✅ Authentication (OTP dummy implementation)
- ✅ Product catalog with images
- ✅ Inventory management
- ✅ Sales order creation
- ✅ Invoice generation (PDF)
- ✅ Payment recording
- ✅ Customer-specific pricing
- ✅ Credit notes
- ✅ SQLite persistence

### Phase 2: Supplier Features
- Supplier portal
- Supplier product management
- Order fulfillment workflow
- Supplier dashboard

### Phase 3: Advanced Features
- Real OTP via SMS/Email
- Payment gateway integration
- Advanced reporting and analytics
- Mobile app (Blazor Hybrid / MAUI)
- Multi-tenant support
- Notifications system

---

## Getting Started

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 / VS Code / Rider
- SQLite (included with .NET)

### Installation
```bash
# Clone the repository
git clone <repository-url>
cd harvest

# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Run the application
dotnet run
```

### Default Admin Credentials
```
Email: admin@harvest.com
OTP: 123456 (dummy implementation)
```

---

## Project Structure

```
Harvest/
├── Components/          # Blazor components
│   ├── Pages/          # Page components
│   ├── Shared/         # Shared components
│   └── Layout/         # Layout components
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── Models/             # Entity models
│   ├── User.cs
│   ├── Product.cs
│   ├── SalesOrder.cs
│   └── ...
├── Services/           # Business logic services
│   ├── AuthService.cs
│   ├── ProductService.cs
│   ├── OrderService.cs
│   ├── InvoiceService.cs
│   ├── PricingService.cs
│   └── ...
├── wwwroot/            # Static files
│   ├── images/
│   └── css/
├── Program.cs
└── appsettings.json
```

---

## Key Features Implementation Notes

### Image Storage
- Product images stored in `wwwroot/images/products/`
- Multiple images per product supported
- First image marked as primary for listings

### PDF Generation
- Use library like QuestPDF or DinkToPdf
- Invoice template with company branding
- Support for printing and email attachment

### Email Service
- SMTP configuration in appsettings.json
- Support for invoice delivery
- Order confirmation emails
- Payment receipts

### Data Persistence
- SQLite database with EF Core
- Automatic migrations
- Seed data for initial setup

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=harvest.db"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@harvest.com",
    "SenderName": "Harvest Platform"
  },
  "OtpSettings": {
    "DummyMode": true,
    "DummyOtpCode": "123456",
    "OtpExpiryMinutes": 5
  }
}
```

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## License

[MIT License](LICENSE)

---

## Contact

Project Link: [https://github.com/yourusername/harvest](https://github.com/yourusername/harvest)

---

## Roadmap

- [ ] Phase 1: Core customer ordering functionality
- [ ] Phase 2: Supplier management portal
- [ ] Phase 3: Real OTP integration
- [ ] Phase 4: Payment gateway integration
- [ ] Phase 5: Advanced analytics and reporting
- [ ] Phase 6: Mobile application
